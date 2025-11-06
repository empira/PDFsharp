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
            // If caller requested an append/incremental-sign option, branch to incremental flow.
            // NOTE: still experimental - full incremental algorithm requires careful creation
            // of incremental objects and trailer. For now we prepare the point of interception
            // and leave the default behavior untouched unless AppendSignature == true.
            if (Options.AppendSignature && writer.FullPath != null)
            {
                // For now call a helper that will attempt an incremental signature.
                // The helper below is a minimal placeholder and currently will
                // attempt to write the computed signature into the existing file
                // using the placeholder offsets determined during save.
                await ComputeIncrementalSignatureAsync(writer.Stream).ConfigureAwait(false);
                return;
            }

            var (rangedStreamToSign, byteRangeArray) = GetRangeToSignAndByteRangeArray(writer.Stream);

            Debug.Assert(_signatureFieldByteRangePlaceholder != null);
            _signatureFieldByteRangePlaceholder.WriteActualObject(byteRangeArray, writer);

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
            if (targetStream is null)
                throw new ArgumentNullException(nameof(targetStream));

            if (!targetStream.CanRead || !targetStream.CanSeek || !targetStream.CanWrite)
                throw new InvalidOperationException("Target stream must be readable, seekable and writable for incremental signature.");

            // IMPORTANT: aqui reutilizamos exatamente a lógica que precisava do arquivo aberto:
            //  - obter ranged stream e byte range via GetRangeToSignAndByteRangeArray
            //  - escrever o byteRange placeholder (já feito normalmente antes)
            //  - calcular a assinatura sobre o ranged stream
            //  - escrever a assinatura no placeholder

            // Observe: suponho que _signatureFieldByteRangePlaceholder e _placeholderItem já foram inicializados
            // por AddSignatureComponentsAsync, como na implementação padrão.
            var (rangedStream, byteRangeArray) = GetRangeToSignAndByteRangeArray(targetStream);

            // Escreve o ByteRange atual (substitui a placeholder na posição certa)
            Debug.Assert(_signatureFieldByteRangePlaceholder != null);
            // Note: WriteActualObject precisa de um PdfWriter em sua implementação atual. Se WriteActualObject
            // aceita um writer, preferir reusar o writer. Se não aceitar, adaptar para escrever diretamente no stream.
            // Aqui assumimos que você tem acesso a um writer (ou que WriteActualObject tem overload).
            // Se for necessário, você pode criar um PdfWriter temporário que usa targetStream e o Document.
            // Exemplo (se WriteActualObject usa PdfWriter):
            var tempWriter = new PdfWriter(targetStream, Document, /*effectiveSecurityHandler*/ null)
            {
                Layout = PdfWriterLayout.Compact
            };
            _signatureFieldByteRangePlaceholder.WriteActualObject(byteRangeArray, tempWriter);

            // Calcula a assinatura (rangedStream é um stream que representa os ranges a assinar)
            byte[] signature = await Signer.GetSignatureAsync(rangedStream).ConfigureAwait(false);

            // Verifica tamanho
            Debug.Assert(_placeholderItem != null);
            int expectedLength = _placeholderItem.Size;
            if (signature.Length > expectedLength)
                throw new Exception($"Actual signature length {signature.Length} exceeds placeholder {expectedLength}.");

            // Escreve a assinatura hex no local reservado
            targetStream.Position = _placeholderItem.StartPosition;
            // write '<'
            targetStream.WriteByte((byte)'<');

            // convert signature to hex literal like "<ABC...>"
            var hexLiteral = PdfEncoders.ToHexStringLiteral(signature, false, false, null); // returns string with angle brackets

            // Option A (recommended): copy inner chars directly into byte[] without creating an extra substring
            var contentLength = hexLiteral.Length - 2; // exclude '<' and '>'
            var writeBytes = new byte[contentLength];
            PdfEncoders.RawEncoding.GetBytes(hexLiteral, 1, contentLength, writeBytes, 0); // copy from string to bytes
            targetStream.Write(writeBytes, 0, writeBytes.Length);

            // pad remainder with '00' pairs if placeholder larger than signature
            for (int i = signature.Length; i < expectedLength; i++)
            {
                // each pad is two ascii characters '0''0' representing a byte in hex, but in original code 1 '00' per byte is written as literal '0''0'
                // however original used writer.WriteRaw("00"); — here we write the ascii bytes for "00".
                var pad = PdfEncoders.RawEncoding.GetBytes("00");
                targetStream.Write(pad, 0, pad.Length);
            }

            // write '>'
            targetStream.WriteByte((byte)'>');

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
                throw new ArgumentOutOfRangeException(
                    $"Signature page doesn't exist, specified page was {Options.PageIndex + 1} but document has only {Document.PageCount} page(s).");

            int signatureSize = await Signer.GetSignatureSizeAsync().ConfigureAwait(false);
            _placeholderItem = new(signatureSize);
            _signatureFieldByteRangePlaceholder = new PdfPlaceholderObject(ByteRangePlaceholderLength);

            var signatureDictionary =
                GetSignatureDictionary(_placeholderItem, _signatureFieldByteRangePlaceholder);

            // ================================================================
            // 🔒 PATCH — aplica apenas se for assinatura incremental
            // ================================================================
            if (Options.AppendSignature)
            {
                PdfCatalog catalog = Document.Catalog;
                if (catalog.Elements.GetObject(PdfCatalog.Keys.AcroForm) == null)
                    catalog.Elements.Add(PdfCatalog.Keys.AcroForm, new PdfAcroForm(Document));

                PdfAcroForm acroForm = catalog.AcroForm;

                if (!acroForm.Elements.ContainsKey(PdfAcroForm.Keys.SigFlags))
                    acroForm.Elements.Add(PdfAcroForm.Keys.SigFlags, new PdfInteger(3));
                else
                {
                    int sigFlagVersion = acroForm.Elements.GetInteger(PdfAcroForm.Keys.SigFlags);
                    if (sigFlagVersion < 3)
                        acroForm.Elements.SetInteger(PdfAcroForm.Keys.SigFlags, 3);
                }

                int signatureCount = acroForm.Fields?.Elements?.Count ?? 0;

                PdfDictionary sigField = new PdfDictionary(Document);
                sigField.Elements["/FT"] = new PdfName("/Sig");
                sigField.Elements["/T"] = new PdfString($"Signature{signatureCount + 1}");
                sigField.Elements["/V"] = signatureDictionary;
                sigField.Elements["/Ff"] = new PdfInteger(1 << 2);
                sigField.Elements["/Type"] = new PdfName("/Annot");
                sigField.Elements["/Subtype"] = new PdfName("/Widget");
                sigField.Elements["/Rect"] = new PdfRectangle(Options.Rectangle);
                sigField.Elements["/P"] = Document.Pages[Options.PageIndex].Reference;

                Document.Internals.AddObject(sigField);

                if (acroForm.Elements["/Fields"] is PdfArray fieldsArray)
                {
                    fieldsArray.Elements.Add(sigField.Reference);
                }
                else
                {
                    PdfArray newFields = new PdfArray(Document);
                    newFields.Elements.Add(sigField.Reference);
                    acroForm.Elements["/Fields"] = newFields;
                }

                if (!acroForm.Elements.ContainsKey("/DR"))
                    acroForm.Elements.Add("/DR", new PdfDictionary(Document));

                if (!acroForm.Elements.ContainsKey("/DA"))
                    acroForm.Elements.Add("/DA", new PdfString("/Helv 0 Tf 0 g"));
            }
            else
            {
                // ================================================================
                // ⚙️ Fluxo original — primeira assinatura
                // ================================================================
                PdfDictionary signatureField = GetSignatureField(signatureDictionary);

                PdfArray annotations = Document.Pages[Options.PageIndex].Elements.GetArray(PdfPage.Keys.Annots);
                if (annotations == null)
                    Document.Pages[Options.PageIndex].Elements.Add(PdfPage.Keys.Annots, new PdfArray(Document, signatureField));
                else
                    annotations.Elements.Add(signatureField);

                PdfCatalog catalog = Document.Catalog;

                if (catalog.Elements.GetObject(PdfCatalog.Keys.AcroForm) == null)
                    catalog.Elements.Add(PdfCatalog.Keys.AcroForm, new PdfAcroForm(Document));

                if (!catalog.AcroForm.Elements.ContainsKey(PdfAcroForm.Keys.SigFlags))
                    catalog.AcroForm.Elements.Add(PdfAcroForm.Keys.SigFlags, new PdfInteger(3));
                else
                {
                    int sigFlagVersion = catalog.AcroForm.Elements.GetInteger(PdfAcroForm.Keys.SigFlags);
                    if (sigFlagVersion < 3)
                        catalog.AcroForm.Elements.SetInteger(PdfAcroForm.Keys.SigFlags, 3);
                }

                if (catalog.AcroForm.Elements.GetValue(PdfAcroForm.Keys.Fields) == null)
                    catalog.AcroForm.Elements.SetValue(PdfAcroForm.Keys.Fields,
                        new PdfAcroField.PdfAcroFieldCollection(new PdfArray()));

                catalog.AcroForm.Fields.Elements.Add(signatureField);
            }

            PdfDictionary markInfo =
                Document.Catalog.Elements.GetDictionary("/MarkInfo")
                ?? new PdfDictionary(Document);

            markInfo.Elements.SetBoolean("/Marked", true);
            Document.Catalog.Elements["/MarkInfo"] = markInfo;
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
            signatureField.PrepareForSave(); // TODO_OLD PdfSignatureField.PrepareForSave() is not triggered automatically so let's call it manually from here, but it would be better to be called automatically.

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
