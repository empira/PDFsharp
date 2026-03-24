// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if !NET8_0_OR_GREATER
using System.Text;
#endif
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.IO;
using System.Text;

// v7.0.0 TODO review

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// PdfDocument signature handler.
    /// Attaches a PKCS#7 signature digest to PdfDocument.
    /// </summary>
    public class DigitalSignatureHandler
    {
        /// <summary>
        /// Large enough space reserved by PdfPlaceholder to be replaced by the actual computed value of the byte range to sign.
        /// Worst case: signature dictionary is near the end of an 10 GB PDF file. So we reserve 10 digits.
        /// However, the current implementation can only support 2 GB files.
        /// </summary>
        const int ByteRangePlaceholderLength = 36; // = "[0 9999999999 9999999999 9999999999]".Length

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

            // TODO_OLD in document: Set document version depending on digest type from options.  // TODO: what?
        }

        /// <summary>
        /// Gets or creates the digital signature handler for the specified document.
        /// </summary>
        public static DigitalSignatureHandler ForDocument(PdfDocument document, IDigitalSigner signer, DigitalSignatureOptions options)
        {
            return document.DigitalSignatureHandler ??= new(document, signer, options);
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

        /// <summary>
        /// Computes the signature and byte range after the document stream was written.
        /// </summary>
        internal async Task ComputeSignatureAndRange(PdfWriter writer)
        {
            var (rangedStreamToSign, byteRangeArray) = GetRangeToSignAndByteRangeArray(writer.Stream);

            // Write the /ByteRange entry '[...2 times offset, length...]'.
            Debug.Assert(_byteRangePlaceholder != null);
            var sb = new StringBuilder(4096);
            sb.Append(byteRangeArray);
            // Length must match placeholder.
            sb.Append(new String(' ', _byteRangePlaceholder.Length - sb.Length));
            _byteRangePlaceholder.SetValue(sb.ToString());
            _byteRangePlaceholder.WriteEffectiveValue(writer);

            // Computing signature from document’s digest.
            var signature = await Signer.GetSignatureAsync(rangedStreamToSign).ConfigureAwait(false);

            Debug.Assert(_contentsPlaceholder != null);
            //int expectedLength = (_contentsPlaceholder.Length - 2) / 2;
            int contentsHexLength = 2 * signature.Length + 2;
            if (contentsHexLength > _contentsPlaceholder.Length)
            {
                // This should not happen.
                throw new InvalidOperationException(
                    $"The actual digest length '{contentsHexLength}' is larger than the approximation made '{_contentsPlaceholder.Length}'. " +
                    "Not enough space in the placeholder to fit the hex-encoded signature.");
            }

            // Write the /Contents entry '<...signature hex string...>'.
            // When the signature includes a timestamp, the exact length is unknown until the signature is definitely calculated.
            // Therefore, we write the angle brackets here and override the placeholder white-spaces.
            // According to the PDF reference, the Contents key of a signature dictionary shall not be encrypted.
            sb.Clear();
            sb.Append('<');
            sb.Append(FormatHex(signature));
#if true
            // Filler bytes must be part of the hex string. Trailing blanks are invalid PDF.
            //sb.Append(new String('0', 2 * (expectedLength - signature.Length)));
            sb.Append(new String('0', _contentsPlaceholder.Length - sb.Length - 1));
            sb.Append('>');
#else
            sb.Append('>');
            sb.Append(new String(' ', 2 * (expectedLength - signature.Length)));
#endif
            _contentsPlaceholder.SetValue(sb.ToString());
            _contentsPlaceholder.WriteEffectiveValue(writer);
        }

        string FormatHex(byte[] bytes)  // ...use RawEncoder
        {
#if NET8_0_OR_GREATER
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
            Debug.Assert(_contentsPlaceholder != null, nameof(_contentsPlaceholder) + " must not be null here.");

            var firstRangeOffset = 0L;
            var firstRangeLength = _contentsPlaceholder.StartPosition;
            var secondRangeOffset = _contentsPlaceholder.EndPosition;
            var secondRangeLength = stream.Length - _contentsPlaceholder.EndPosition;

            var byteRangeArray = new PdfArray();
#if USE_LONG_SIZE
            byteRangeArray.Elements.Add(new PdfLongInteger(firstRangeOffset));
            byteRangeArray.Elements.Add(new PdfLongInteger(firstRangeLength));
            byteRangeArray.Elements.Add(new PdfLongInteger(secondRangeOffset));
            byteRangeArray.Elements.Add(new PdfLongInteger(secondRangeLength));
#else
            byteRangeArray.Elements.Add(new PdfInteger(firstRangeOffset));
            byteRangeArray.Elements.Add(new PdfInteger(firstRangeLength));
            byteRangeArray.Elements.Add(new PdfInteger(secondRangeOffset));
            byteRangeArray.Elements.Add(new PdfInteger(secondRangeLength));
#endif
            var rangedStream = new RangedStream(stream,
            [
                new(firstRangeOffset, firstRangeLength),
                new(secondRangeOffset, secondRangeLength)
            ]);

            return (rangedStream, byteRangeArray);
        }

        /// <summary>
        /// Adds the PDF objects required for a digital signature as placeholders.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal async Task AddSignatureComponentsAsync() // #US321 TODO Use appropriate classes.
        {
            if (Options.PageIndex >= Document.PageCount)
                throw new ArgumentOutOfRangeException($"Signature page doesn't exist, specified page was {Options.PageIndex + 1} but document has only {Document.PageCount} page(s).");

            var signatureSize = await Signer.GetSignatureSizeAsync().ConfigureAwait(false);
            _contentsPlaceholder = new(2 * signatureSize + 2);
            _byteRangePlaceholder = new(ByteRangePlaceholderLength);

            var signatureDictionary = GetSignatureDictionary(_contentsPlaceholder, _byteRangePlaceholder);
            var signatureField = GetSignatureField(signatureDictionary);

            var page = Document.Pages[Options.PageIndex];
            var annotations = page.Elements.GetArray(PdfPage.Keys.Annots);
            if (annotations == null)
                page.Elements.Add(PdfPage.Keys.Annots, new PdfArray(Document, signatureField));
            else
                annotations.Elements.Add(signatureField);
            
            var catalog = Document.Catalog;
            var acroForm = catalog.GetOrCreateAcroForm();

            if (!acroForm.Elements.ContainsKey(PdfForm.Keys.SigFlags))
                acroForm.Elements.Add(PdfForm.Keys.SigFlags, new PdfInteger(3, true));
            else
            {
                var sigFlagVersion = acroForm.Elements.GetInteger(PdfForm.Keys.SigFlags);
                if (sigFlagVersion < 3)
                    acroForm.Elements.SetIntegerFlag(PdfForm.Keys.SigFlags, 3);
            }
            
            acroForm.Fields.Elements.Add(signatureField);
        }

        PdfFormSignatureField GetSignatureField(PdfSignature signatureDic) // #US321 TODO Use appropriate classes.
        {
            var signatureField = new PdfFormSignatureField(Document);

            signatureField.Elements.Add(PdfFormField.Keys.V, signatureDic);

            // #AcroForms
            // Annotation keys.
            signatureField.Elements.Add(PdfFormField.Keys.FT, new PdfName(PdfFormFieldType.Signature));
            signatureField.Elements.Add(PdfFormField.Keys.T, new PdfString("Signature1")); // TODO If already exists, will it cause error? implement a name chooser if yes.
            signatureField.Elements.Add(PdfFormField.Keys.Ff, new PdfInteger(132));
            // signatureField.Elements.Add(PdfFormField.Keys.DR, new PdfDictionary());  TODO COMPILE
            signatureField.Elements.Add(PdfAnnotation.Keys.Type, new PdfName("/Annot"));
            signatureField.Elements.Add(PdfAnnotation.Keys.Subtype, new PdfName("/Widget"));
            signatureField.Elements.Add(PdfAnnotation.Keys.P, Document.Pages[Options.PageIndex]);

            signatureField.Elements.Add(PdfAnnotation.Keys.Rect, new PdfRectangle(Options.Rectangle));

            // TODO COMPILE
            signatureField.CustomAppearanceHandler = Options.AppearanceHandler ?? new DefaultSignatureAppearanceHandler()
            {
                Location = Options.Location,
                Reason = Options.Reason,
                Signer = Signer.CertificateName
            };

            // Call RenderCustomAppearance(); here.
            signatureField.RenderAppearance();
            // Rendering the signature in PrepareForSave is too late and leads to inconsistent embedded fonts.

            //Document.Internals.AddObject(signatureField); AcroFields are already indirect.

            return signatureField;
        }

        PdfSignature GetSignatureDictionary(PdfPlaceholder contents, PdfPlaceholder byteRange) // #US321 TODO Use appropriate classes.
        {
            PdfSignature signatureDic = new(Document);

            signatureDic.Elements.Add(PdfSignature.Keys.Type, new PdfName(PdfFormFieldType.Signature));
            signatureDic.Elements.Add(PdfSignature.Keys.Filter, new PdfName("/Adobe.PPKLite"));
            signatureDic.Elements.Add(PdfSignature.Keys.SubFilter, new PdfName("/adbe.pkcs7.detached"));
            signatureDic.Elements.Add(PdfSignature.Keys.M, new PdfDate(DateTimeOffset.Now));

            signatureDic.Elements.Add(PdfSignature.Keys.Contents, contents);
            signatureDic.Elements.Add(PdfSignature.Keys.ByteRange, byteRange);
            signatureDic.Elements.Add(PdfSignature.Keys.Reason, new PdfString(Options.Reason));
            signatureDic.Elements.Add(PdfSignature.Keys.Location, new PdfString(Options.Location));

            var properties = new PdfDictionary(Document);
            signatureDic.Elements.Add("/Prop_Build", properties);
            var propertyItems = new PdfDictionary(Document);
            properties.Elements.Add("/App", propertyItems);
            propertyItems.Elements.Add("/Name",
                String.IsNullOrWhiteSpace(Options.AppName) ?
                new PdfName("/PDFsharp http://www.pdfsharp.net") :
                PdfName.FromString(Options.AppName));

            Document.Internals.AddObject(signatureDic);

            return signatureDic;
        }

        PdfPlaceholder? _contentsPlaceholder;
        PdfPlaceholder? _byteRangePlaceholder;
    }
}
