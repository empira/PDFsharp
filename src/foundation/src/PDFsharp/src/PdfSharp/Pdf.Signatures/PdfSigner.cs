using PdfSharp.Drawing;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using System.Security.Cryptography.X509Certificates;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// Utility class for signing PDF-documents
    /// </summary>
    public class PdfSigner
    {
        private readonly Stream inputStream;

        private readonly PdfDocument document;

        private readonly ISigner signer;

        private readonly PdfSignatureOptions options;

        /// <summary>
        /// Create new new instance for the specified document and with the specified options
        /// </summary>
        /// <param name="documentStream">Stream specifying the document to sign. Must be readable and seekable</param>
        /// <param name="signatureOptions">The options that spefify, how the signing is performed</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public PdfSigner(Stream documentStream, PdfSignatureOptions signatureOptions)
        {
            if (documentStream is null)
                throw new ArgumentNullException(nameof(documentStream));
            if (!documentStream.CanRead || !documentStream.CanSeek)
                throw new ArgumentException("Invalid stream. Must be readable and seekable", nameof(documentStream));
            options = signatureOptions ?? throw new ArgumentNullException(nameof(signatureOptions));

            if (options.Certificate is null)
                throw new ArgumentException("A certificate is required to sign");
            if (options.PageIndex < 0)
                throw new ArgumentException("Page index cannot be less than zero");

            inputStream = documentStream;
            document = PdfReader.Open(documentStream, PdfDocumentOpenMode.Append);
            signer = new DefaultSigner(signatureOptions);
        }

        /// <summary>
        /// Signs the document
        /// </summary>
        /// <returns>A stream containing the signed document. Stream-position is 0</returns>
        public Stream Sign()
        {
            var signatureValue = CreateSignatureValue();
            var signatureField = GetOrCreateSignatureField(signatureValue);
            RenderSignatureAppearance(signatureField);

            var finalDocumentLength = 0L;
            var contentStart = 0L;
            var contentEnd = 0L;
            var rangeStart = 0L;
            var rangeEnd = 0L;
            var extraSpace = 0;
            signatureValue.SignatureContentsWritten = (sigValue, start, end) =>
            {
                contentStart = start;
                contentEnd = end;
            };
            signatureValue.SignatureRangeWritten = (sigValue, start, end) =>
            {
                rangeStart = start;
                rangeEnd = end;
            };
            document.AfterSave = (writer) =>
            {
                extraSpace = writer.Layout == PdfWriterLayout.Verbose ? 1 : 0;
            };
            var ms = new MemoryStream();
            // copy original document to output-stream
            inputStream.Seek(0, SeekOrigin.Begin);
            inputStream.CopyTo(ms);
            // append incremental update
            document.Save(ms);

            finalDocumentLength = ms.Length;

            contentStart += extraSpace;
            rangeStart += extraSpace;

            // write new ByteRange array
            var rangeArrayValue = string.Format(CultureInfo.InvariantCulture, "[0 {0} {1} {2}]",
                contentStart, contentEnd, finalDocumentLength - contentEnd);
            Debug.Assert(rangeArrayValue.Length <= rangeEnd - rangeStart);
            rangeArrayValue = rangeArrayValue.PadRight((int)(rangeEnd - rangeStart), ' ');
            ms.Seek(rangeStart, SeekOrigin.Begin);
            var writeBytes = PdfEncoders.RawEncoding.GetBytes(rangeArrayValue);
            ms.Write(writeBytes, 0, writeBytes.Length);

            // concat the ranges before and after the content-string
            var lengthToSign = contentStart + finalDocumentLength - contentEnd;
            var toSign = new byte[lengthToSign];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(toSign, 0, (int)contentStart);
            ms.Seek(contentEnd, SeekOrigin.Begin);
            ms.Read(toSign, (int)contentStart, (int)(finalDocumentLength - contentEnd));

            // do the signing
            var signatureData = signer.GetSignedCms(toSign, document);

            // move past the '<'
            ms.Seek(contentStart + 1, SeekOrigin.Begin);
            // convert signature to string
            var signHexString = PdfEncoders.ToHexStringLiteral(signatureData, false, false, null);
            writeBytes = new byte[signHexString.Length - 2];
            // exclude '<' and '>' from hex-string and overwrite fake value
            PdfEncoders.RawEncoding.GetBytes(signHexString, 1, signHexString.Length - 2, writeBytes, 0);
            ms.Write(writeBytes, 0, writeBytes.Length);

            ms.Position = 0;

            document.Dispose();

            return ms;
        }

        private int GetContentLength()
        {
            return signer.GetSignedCms(new MemoryStream(new byte[] { 0 }), document).Length + 10;
        }

        private PdfSignatureField GetOrCreateSignatureField(PdfSignatureValue value)
        {
            var acroForm = document.GetOrCreateAcroForm();
            var fieldList = GetExistingFields();
            // if a field with the specified name exist, use that
            // Note: only root-level fields are currently supported
            var fieldWithName = fieldList.FirstOrDefault(f => f.Name == options.FieldName);
            if (fieldWithName != null && !(fieldWithName is PdfSignatureField))
                throw new ArgumentException(
                    $"Field '{options.FieldName}' exist in document, but it is not a Signature-Field ({fieldWithName.GetType().Name})");

            var isNewField = false;
            var signatureField = fieldList.FirstOrDefault(f =>
                f is PdfSignatureField && f.Name == options.FieldName) as PdfSignatureField;
            if (signatureField == null)
            {
                // field does not exist, create new one
                signatureField = new PdfSignatureField(document)
                {
                    Name = options.FieldName
                };
                document.IrefTable.Add(signatureField);
                acroForm.Fields.Elements.Add(signatureField);
                isNewField = true;
            }
            // Flags: SignaturesExit + AppendOnly
            acroForm.Elements.SetInteger(PdfAcroForm.Keys.SigFlags, 3);

            signatureField.Value = value;
            signatureField.Elements.SetInteger(PdfAcroField.Keys.Ff, (int)PdfAcroFieldFlags.NoExport);
            signatureField.Elements.SetName(PdfAnnotation.Keys.Type, "/Annot");
            signatureField.Elements.SetName(PdfAnnotation.Keys.Subtype, "/Widget");
            if (isNewField)
            {
                signatureField.Elements.SetReference("/P", document.Pages[options.PageIndex]);
                signatureField.Elements.Add(PdfAnnotation.Keys.Rect, new PdfRectangle(options.Rectangle));
            }
            var annotations = document.Pages[options.PageIndex].Elements.GetArray(PdfPage.Keys.Annots);
            if (annotations == null)
                document.Pages[options.PageIndex].Elements.Add(PdfPage.Keys.Annots, new PdfArray(document, signatureField));
            else if (!annotations.Elements.Contains(signatureField))
                annotations.Elements.Add(signatureField);

            return signatureField;
        }

        private PdfSignatureValue CreateSignatureValue()
        {
            var signatureDict = new PdfSignatureValue(document);
            document.IrefTable.Add(signatureDict);

            var contentLength = GetContentLength();
            var content = Enumerable.Repeat<byte>(0, contentLength).ToArray();
            signatureDict.Contents = content;
            signatureDict.Filter = "/Adobe.PPKLite";
            signatureDict.SubFilter = "/adbe.pkcs7.detached";
            signatureDict.SigningDate = DateTime.Now;

            var documentLength = inputStream.Length;
            // fill with large enough fake values. we will overwrite these later
            var byteRange = new PdfArray(document, new PdfLongInteger(0), new PdfLongInteger(documentLength),
                new PdfLongInteger(documentLength), new PdfLongInteger(documentLength));
            signatureDict.ByteRange = byteRange;
            if (options.Reason is not null)
                signatureDict.Reason = options.Reason;
            if (options.Location is not null)
                signatureDict.Location = options.Location;
            if (options.ContactInfo is not null)
                signatureDict.ContactInfo = options.ContactInfo;

            return signatureDict;
        }

        private void RenderSignatureAppearance(PdfSignatureField signatureField)
        {
            if (string.IsNullOrEmpty(options.Signer))
                options.Signer = signer.GetName() ?? "unknown";

            XRect annotRect;
            var rect = signatureField.Elements.GetRectangle(PdfAnnotation.Keys.Rect);
            if (rect.IsEmpty)
            {
                // XRect.IsEmpty returns false even when width and height are zero ??
                if (options.Rectangle.Width <= 0 || options.Rectangle.Height <= 0)
                    return;

                annotRect = options.Rectangle;
                signatureField.Elements.SetRectangle(PdfAnnotation.Keys.Rect, new PdfRectangle(annotRect));
            }
            else
                annotRect = rect.ToXRect();

            var form = new XForm(document, annotRect.Size);
            var gfx = XGraphics.FromForm(form);
            var renderer = options.Renderer ?? new DefaultSignatureRenderer();
            renderer.Render(gfx, annotRect, options);
            form.DrawingFinished();
            // form.PdfRenderer might be null here (in GDI build)
            form.PdfRenderer?.Close();

            if (signatureField.Elements[PdfAnnotation.Keys.AP] is not PdfDictionary ap)
            {
                ap = new PdfDictionary(document);
                signatureField.Elements.Add(PdfAnnotation.Keys.AP, ap);
            }
            ap.Elements.SetReference("/N", form.PdfForm);
        }

        /// <summary>
        /// Gets the list of existing root-fields of this document
        /// </summary>
        /// <returns></returns>
        private IEnumerable<PdfAcroField> GetExistingFields()
        {
            var fields = new List<PdfAcroField>();
            if (document.AcroForm?.Fields != null)
            {
                for (var i = 0; i < document.AcroForm.Fields.Count; i++)
                {
                    var field = document.AcroForm.Fields[i];
                    fields.Add(field);
                }
            }
            return fields;
        }
    }
}
