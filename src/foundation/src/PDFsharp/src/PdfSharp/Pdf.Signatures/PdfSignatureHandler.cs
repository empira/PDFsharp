// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if !NET6_0_OR_GREATER
using System.Text;
#endif
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using System.Text;
using System.Text.RegularExpressions;

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
        /// Big enough space reserved by PdfPlaceholderObject to be replaced by the actual computed value of the byte range to sign
        /// Worst case: signature dictionary is near the end of an 10 GB PDF file.
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

            // TODO_OLD in document: Set document version depending on digest type from options.
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
            if (Options.AppendSignature && writer.FullPath != null)
            {
                await ComputeIncrementalSignatureAsync(writer.Stream).ConfigureAwait(continueOnCapturedContext: false);
                return;
            }
            var (stream, obj) = GetRangeToSignAndByteRangeArray(writer.Stream);
            _signatureFieldByteRangePlaceholder.WriteActualObject(obj, writer);
            byte[] array = await Signer.GetSignatureAsync(stream).ConfigureAwait(continueOnCapturedContext: false);
            int size = _placeholderItem.Size;
            if (array.Length > size)
            {
                throw new Exception($"The actual digest length {array.Length} is larger than the approximation made {size}. Not enough room in the placeholder to fit the signature.");
            }
            writer.Stream.Position = _placeholderItem.StartPosition;
            writer.WriteRaw('<');
            writer.Write(PdfEncoders.RawEncoding.GetBytes(FormatHex(array)));
            for (int i = array.Length; i < size; i++)
            {
                writer.WriteRaw("00");
            }
            writer.WriteRaw('>');
        }

        /// <summary>
        /// Minimal placeholder for incremental-sign attempt.
        /// This function is intentionally conservative: it tries to update the file
        /// in-place only if the placeholder offsets appear valid. A robust incremental
        /// implementation requires writing incremental xref & trailer; we'll extend
        /// this later. For now this helper tries to write the signature bytes at the
        /// reserved placeholder offset in the existing file (works when offsets align).
        /// </summary>
        // Antes (exemplo):
        // internal async Task ComputeIncrementalSignatureAsync(string path, Stream ignored) { ... }

        // Depois: novo método que usa o stream já aberto.
        internal async Task ComputeIncrementalSignatureAsync(Stream targetStream)
        {
            if (targetStream == null)
            {
                throw new ArgumentNullException("targetStream");
            }
            if (!targetStream.CanRead || !targetStream.CanSeek || !targetStream.CanWrite)
            {
                throw new InvalidOperationException("Target stream must be readable, seekable and writable for incremental signature.");
            }
            (RangedStream rangedStream, PdfArray byteRangeArray) rangeToSignAndByteRangeArray = GetRangeToSignAndByteRangeArray(targetStream);
            RangedStream item = rangeToSignAndByteRangeArray.rangedStream;
            PdfArray item2 = rangeToSignAndByteRangeArray.byteRangeArray;
            PdfWriter writer = new PdfWriter(targetStream, Document, null)
            {
                Layout = PdfWriterLayout.Compact
            };
            _signatureFieldByteRangePlaceholder.WriteActualObject(item2, writer);
            byte[] array = await Signer.GetSignatureAsync(item).ConfigureAwait(continueOnCapturedContext: false);
            int size = _placeholderItem.Size;
            if (array.Length > size)
            {
                throw new Exception($"Actual signature length {array.Length} exceeds placeholder {size}.");
            }
            targetStream.Position = _placeholderItem.StartPosition;
            targetStream.WriteByte(60);
            string text = PdfEncoders.ToHexStringLiteral(array, unicode: false, prefix: false, null);
            int num = text.Length - 2;
            byte[] array2 = new byte[num];
            PdfEncoders.RawEncoding.GetBytes(text, 1, num, array2, 0);
            targetStream.Write(array2, 0, array2.Length);
            for (int i = array.Length; i < size; i++)
            {
                byte[] bytes = PdfEncoders.RawEncoding.GetBytes("00");
                targetStream.Write(bytes, 0, bytes.Length);
            }
            targetStream.WriteByte(62);
            targetStream.Flush();
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
            Debug.Assert(_placeholderItem != null, nameof(_placeholderItem) + " must not be null here.");

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
            {
                throw new ArgumentOutOfRangeException($"Signature page doesn't exist, specified page was {Options.PageIndex + 1} but document has only {Document.PageCount} page(s).");
            }
            _placeholderItem = new PdfSignaturePlaceholderItem(await Signer.GetSignatureSizeAsync().ConfigureAwait(continueOnCapturedContext: false));
            _signatureFieldByteRangePlaceholder = new PdfPlaceholderObject(36);
            PdfSignature2 signatureDictionary = GetSignatureDictionary(_placeholderItem, _signatureFieldByteRangePlaceholder);
            if (Options.AppendSignature)
            {
                PdfCatalog catalog = Document.Catalog;
                if (catalog.Elements.GetObject("/AcroForm") == null)
                {
                    catalog.Elements.Add("/AcroForm", new PdfAcroForm(Document));
                }
                PdfAcroForm acroForm = catalog.AcroForm;
                if (!acroForm.Elements.ContainsKey("/SigFlags"))
                {
                    acroForm.Elements.Add("/SigFlags", new PdfInteger(3));
                }
                else if (acroForm.Elements.GetInteger("/SigFlags") < 3)
                {
                    acroForm.Elements.SetInteger("/SigFlags", 3);
                }
                int valueOrDefault = (acroForm.Fields?.Elements?.Count).GetValueOrDefault();
                PdfDictionary pdfDictionary = new PdfDictionary(Document);
                pdfDictionary.Elements["/FT"] = new PdfName("/Sig");
                pdfDictionary.Elements["/T"] = new PdfString($"Signature{valueOrDefault + 1}");
                pdfDictionary.Elements["/V"] = signatureDictionary;
                pdfDictionary.Elements["/Ff"] = new PdfInteger(4);
                pdfDictionary.Elements["/Type"] = new PdfName("/Annot");
                pdfDictionary.Elements["/Subtype"] = new PdfName("/Widget");
                pdfDictionary.Elements["/Rect"] = new PdfRectangle(Options.Rectangle);
                pdfDictionary.Elements["/P"] = Document.Pages[Options.PageIndex].Reference;
                Document.Internals.AddObject(pdfDictionary);
                if (acroForm.Elements["/Fields"] is PdfArray pdfArray)
                {
                    pdfArray.Elements.Add(pdfDictionary.Reference);
                }
                else
                {
                    PdfArray pdfArray2 = new PdfArray(Document);
                    pdfArray2.Elements.Add(pdfDictionary.Reference);
                    acroForm.Elements["/Fields"] = pdfArray2;
                }
                if (!acroForm.Elements.ContainsKey("/DR"))
                {
                    acroForm.Elements.Add("/DR", new PdfDictionary(Document));
                }
                if (!acroForm.Elements.ContainsKey("/DA"))
                {
                    acroForm.Elements.Add("/DA", new PdfString("/Helv 0 Tf 0 g"));
                }
            }
            else
            {
                PdfSignatureField signatureField = GetSignatureField(signatureDictionary);
                PdfArray array = Document.Pages[Options.PageIndex].Elements.GetArray("/Annots");
                if (array == null)
                {
                    Document.Pages[Options.PageIndex].Elements.Add("/Annots", new PdfArray(Document, signatureField));
                }
                else
                {
                    array.Elements.Add(signatureField);
                }
                PdfCatalog catalog2 = Document.Catalog;
                if (catalog2.Elements.GetObject("/AcroForm") == null)
                {
                    catalog2.Elements.Add("/AcroForm", new PdfAcroForm(Document));
                }
                if (!catalog2.AcroForm.Elements.ContainsKey("/SigFlags"))
                {
                    catalog2.AcroForm.Elements.Add("/SigFlags", new PdfInteger(3));
                }
                else if (catalog2.AcroForm.Elements.GetInteger("/SigFlags") < 3)
                {
                    catalog2.AcroForm.Elements.SetInteger("/SigFlags", 3);
                }
                if (catalog2.AcroForm.Elements.GetValue("/Fields") == null)
                {
                    catalog2.AcroForm.Elements.SetValue("/Fields", new PdfAcroField.PdfAcroFieldCollection(new PdfArray()));
                }
                catalog2.AcroForm.Fields.Elements.Add(signatureField);
            }
        }

        PdfSignatureField GetSignatureField(PdfSignature2 signatureDic)
        {
            var signatureField = new PdfSignatureField(Document);

            signatureField.Elements.Add(PdfAcroField.Keys.V, signatureDic);

            // Annotation keys.
            signatureField.Elements.Add(PdfAcroField.Keys.FT, new PdfName("/Sig"));
            signatureField.Elements.Add(PdfAcroField.Keys.T, new PdfString("Signature1")); // TODO_OLD If already exists, will it cause error? implement a name chooser if yes.
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

            // TODO_OLD Call RenderCustomAppearance(); here.
            signatureField.PrepareForSave(); // TODO_OLD PdfSignatureField.PrepareForSave() is not triggered automatically so let's call it manually from here, but it would be better to be called automatically
           
            // Se vazio, define para imprimir (requisito PDF/A para SigField)
            signatureField.Elements.SetInteger("/F", 4);            

            Document.Internals.AddObject(signatureField);

            return signatureField;
        }

        PdfSignature2 GetSignatureDictionary(PdfSignaturePlaceholderItem contents, PdfPlaceholderObject byteRange)
        {
            PdfSignature2 signatureDic = new(Document);

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
                PdfName.FromString(Options.AppName));

            Document.Internals.AddObject(signatureDic);

            return signatureDic;
        }

        PdfSignaturePlaceholderItem? _placeholderItem;
        PdfPlaceholderObject? _signatureFieldByteRangePlaceholder;
    }
}
