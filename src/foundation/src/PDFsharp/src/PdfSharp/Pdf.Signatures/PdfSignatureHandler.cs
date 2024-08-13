// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if !NET6_0_OR_GREATER
using System.Text;
#endif
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// PdfDocument signature handler.
    /// Attaches a PKCS#7 signature digest to PdfDocument.
    /// </summary>
    // DigitalSignatureHandler rename file
    public class DigitalSignatureHandler
    {
        /// <summary>
        /// Space ... big enough reserved space to replace ByteRange placeholder [0 0 0 0] with the actual computed value of the byte range to sign
        /// Worst case: signature dictionary is near the end of an 10 GB PDF file.
        /// </summary>
        const int ByteRangePaddingLength = 36; // = "[0 9999999999 9999999999 9999999999]".Length

        DigitalSignatureHandler(PdfDocument document, IDigitalSigner signer, DigitalSignatureOptions options)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Signer = signer ?? throw new ArgumentNullException(nameof(signer));
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (options.PageIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.PageIndex),
                    "Signature page index cannot be negative.");
            }

            // TODO in document: Set document version depending on digest type from options.
        }

        /// <summary>
        /// Gets or creates the digital signature handler for the specified document.
        /// </summary>
        public static DigitalSignatureHandler ForDocument(PdfDocument document, IDigitalSigner signer, DigitalSignatureOptions options)
        {
            return document._digitalSignatureHandler ??= new(document, signer, options);
        }


        /// <summary>
        /// Gets the PDF document the signature will be attached to.
        /// </summary>
        public PdfDocument Document { get; init; }

        /// <summary>
        /// Gets the options for the digital signature.
        /// </summary>
        public DigitalSignatureOptions Options { get; init; }

        IDigitalSigner Signer { get; init; }

        internal async Task ComputeSignatureAndRange(PdfWriter writer)
        {
            var (rangedStreamToSign, byteRangeArray) = GetRangeToSignAndByteRangeArray(writer.Stream);

            Debug.Assert(_signatureFieldByteRangePdfArray != null);
            writer.Stream.Position = _signatureFieldByteRangePdfArray.StartPosition;
            byteRangeArray.WriteObject(writer);

            // Computing signature from document’s digest.
            var signature = await Signer.GetSignatureAsync(rangedStreamToSign).ConfigureAwait(false);

            Debug.Assert(_placeholderItem != null);
            int expectedLength = _placeholderItem.Size;
            if (signature.Length > expectedLength)
                throw new Exception($"The actual digest length {signature.Length} is larger than the approximation made {expectedLength}. Not enough room in the placeholder to fit the signature.");

            // Write the signature at the space reserved by placeholder item.
            writer.Stream.Position = _placeholderItem.StartPosition;

            // When the signature includes a timestamp, the exact length is unknown until the signature is definitely calculated.
            // Therefore, we write the angle brackets here and override the placeholder white spaces.
            writer.WriteRaw('<');
            writer.Write(PdfEncoders.RawEncoding.GetBytes(FormatHex(signature)));

            // Fill up the allocated placeholder. Signature is sometimes considered invalid if there are spaces after '>'.
            for (int x = signature.Length; x < expectedLength; ++x)
                writer.WriteRaw("00");

            writer.WriteRaw('>');
        }

        string FormatHex(byte[] bytes)  // ...use RawEncoder
        {
#if NET6_0_OR_GREATER
            return Convert.ToHexString(bytes);
#else
            var result = new StringBuilder();

            for (int idx = 0; idx < bytes.Length; idx++)
                result.AppendFormat("{0:X2}", bytes[idx]);

            return result.ToString();
#endif
        }

        /// <summary>
        /// Get the bytes ranges to sign.
        /// As recommended in PDF specs, whole document will be signed, except for the hexadecimal signature token value in the /Contents entry.
        /// Example: '/Contents &lt;aaaaa111111&gt;' => '&lt;aaaaa111111&gt;' will be excluded from the bytes to sign.
        /// </summary>
        /// <param name="stream"></param>
        (RangedStream rangedStream, PdfArray byteRangeArray) GetRangeToSignAndByteRangeArray(Stream stream)
        {
            Debug.Assert( _placeholderItem !=null, nameof(_placeholderItem) + " must not be null here.");

            SizeType firstRangeOffset = 0;
            SizeType firstRangeLength = _placeholderItem.StartPosition;
            SizeType secondRangeOffset = _placeholderItem.EndPosition;
            SizeType secondRangeLength = stream.Length - _placeholderItem.EndPosition;

            var byteRangeArray = new PdfArray();
            byteRangeArray.Elements.Add(new PdfLongInteger(firstRangeOffset));
            byteRangeArray.Elements.Add(new PdfLongInteger(firstRangeLength));
            byteRangeArray.Elements.Add(new PdfLongInteger(secondRangeOffset));
            byteRangeArray.Elements.Add(new PdfLongInteger(secondRangeLength));

            var rangedStream = new RangedStream(stream,
            [
                new(firstRangeOffset, firstRangeLength),
                new(secondRangeOffset, secondRangeLength)
            ]);

            return (rangedStream, byteRangeArray);
        }

        /// <summary>
        /// Adds the PDF objects required for a digital signature.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal async Task AddSignatureComponentsAsync()
        {
            if (Options.PageIndex >= Document.PageCount)
                throw new ArgumentOutOfRangeException($"Signature page doesn't exist, specified page was {Options.PageIndex + 1} but document has only {Document.PageCount} page(s).");

            var signatureSize = await Signer.GetSignatureSizeAsync().ConfigureAwait(false);
            _placeholderItem = new(signatureSize);
            _signatureFieldByteRangePdfArray = new PdfArrayWithPadding(Document, ByteRangePaddingLength, new PdfLongInteger(0), new PdfLongInteger(0), new PdfLongInteger(0), new PdfLongInteger(0));

            var signatureDictionary = GetSignatureDictionary(_placeholderItem, _signatureFieldByteRangePdfArray);
            var signatureField = GetSignatureField(signatureDictionary);

            var annotations = Document.Pages[Options.PageIndex].Elements.GetArray(PdfPage.Keys.Annots);
            if (annotations == null)
                Document.Pages[Options.PageIndex].Elements.Add(PdfPage.Keys.Annots, new PdfArray(Document, signatureField));
            else
                annotations.Elements.Add(signatureField);

            // acroform

            var catalog = Document.Catalog;

            if (catalog.Elements.GetObject(PdfCatalog.Keys.AcroForm) == null)
                catalog.Elements.Add(PdfCatalog.Keys.AcroForm, new PdfAcroForm(Document));

            if (!catalog.AcroForm.Elements.ContainsKey(PdfAcroForm.Keys.SigFlags))
                catalog.AcroForm.Elements.Add(PdfAcroForm.Keys.SigFlags, new PdfInteger(3));
            else
            {
                var sigFlagVersion = catalog.AcroForm.Elements.GetInteger(PdfAcroForm.Keys.SigFlags);
                if (sigFlagVersion < 3)
                    catalog.AcroForm.Elements.SetInteger(PdfAcroForm.Keys.SigFlags, 3);
            }

            if (catalog.AcroForm.Elements.GetValue(PdfAcroForm.Keys.Fields) == null)
                catalog.AcroForm.Elements.SetValue(PdfAcroForm.Keys.Fields, new PdfAcroField.PdfAcroFieldCollection(new PdfArray()));
            catalog.AcroForm.Fields.Elements.Add(signatureField);
        }

        PdfSignatureField GetSignatureField(PdfDictionary signatureDic)  // PdfSignatureDictionary
        {
            var signatureField = new PdfSignatureField(Document);

            signatureField.Elements.Add(PdfAcroField.Keys.V, signatureDic);

            // Annotation keys.
            signatureField.Elements.Add(PdfAcroField.Keys.FT, new PdfName("/Sig"));
            signatureField.Elements.Add(PdfAcroField.Keys.T, new PdfString("Signature1")); // TODO If already exists, will it cause error? implement a name chooser if yes.
            signatureField.Elements.Add(PdfAcroField.Keys.Ff, new PdfInteger(132));
            signatureField.Elements.Add(PdfAcroField.Keys.DR, new PdfDictionary());
            signatureField.Elements.Add(PdfSignatureField.Keys.Type, new PdfName("/Annot"));
            signatureField.Elements.Add("/Subtype", new PdfName("/Widget"));
            signatureField.Elements.Add("/P", Document.Pages[Options.PageIndex]);

            signatureField.Elements.Add("/Rect", new PdfRectangle(Options.Rectangle));

            signatureField.CustomAppearanceHandler = Options.AppearanceHandler ?? new DefaultSignatureAppearanceHandler()
            {
                Location = Options.Location,
                Reason = Options.Reason,
                Signer = Signer.CertificateName
            };
            // TODO Call RenderCustomAppearance(); here.
            signatureField.PrepareForSave(); // TODO PdfSignatureField.PrepareForSave() is not triggered automatically so let's call it manually from here, but it would be better to be called automatically.

            Document.Internals.AddObject(signatureField);

            return signatureField;
        }

        PdfDictionary GetSignatureDictionary(PdfSignaturePlaceholderItem contents, PdfArray byteRange)
        {
            PdfSignature signatureDic = new(Document);

            signatureDic.Elements.Add(PdfSignatureField.Keys.Type, new PdfName("/Sig"));
            signatureDic.Elements.Add(PdfSignatureField.Keys.Filter, new PdfName("/Adobe.PPKLite"));
            signatureDic.Elements.Add(PdfSignatureField.Keys.SubFilter, new PdfName("/adbe.pkcs7.detached"));
            signatureDic.Elements.Add(PdfSignatureField.Keys.M, new PdfDate(DateTime.Now));

            signatureDic.Elements.Add(PdfSignatureField.Keys.Contents, contents);
            signatureDic.Elements.Add(PdfSignatureField.Keys.ByteRange, byteRange);
            signatureDic.Elements.Add(PdfSignatureField.Keys.Reason, new PdfString(Options.Reason));
            signatureDic.Elements.Add(PdfSignatureField.Keys.Location, new PdfString(Options.Location));

            var properties = new PdfDictionary(Document);
            signatureDic.Elements.Add("/Prop_Build", properties);
            var propertyItems = new PdfDictionary(Document);
            properties.Elements.Add("/App", propertyItems);
            propertyItems.Elements.Add("/Name",
                String.IsNullOrWhiteSpace(Options.AppName) ?
                new PdfName("/PDFsharp http://www.pdfsharp.net") :
                new PdfName($"/{Options.AppName}"));

            Document.Internals.AddObject(signatureDic);

            return signatureDic;
        }

        PdfSignaturePlaceholderItem? _placeholderItem;
        PdfArrayWithPadding? _signatureFieldByteRangePdfArray;
    }
}
