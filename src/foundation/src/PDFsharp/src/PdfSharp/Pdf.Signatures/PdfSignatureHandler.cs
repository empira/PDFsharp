// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
#if WPF
using System.IO;
#endif
using System.Text;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// PdfDocument signature handler.
    /// Attaches a PKCS#7 signature digest to PdfDocument.
    /// Digest algorithm will be either SHA1/SHA256/SHA512 depending on PdfDocument.Version.
    /// </summary>
    public class PdfSignatureHandler
    {
        private PdfString signatureFieldContents;
        private PdfArray signatureFieldByteRange;

        /// <summary>
        /// Cache signature length (bytes) for each PDF version since digest length depends on digest algorithm that depends on PDF version.
        /// </summary>
        private static Dictionary<int, int> maximumSignatureLengthByPdfVersion = new Dictionary<int, int>();

        private const int byteRangePaddingLength = 36; // place big enough required to replace [0 0 0 0] with the correct value

        public PdfDocument Document { get; private set; }
        public PdfSignatureOptions Options { get; private set; }
        private ISigner signer { get; set; }

        public void AttachToDocument(PdfDocument documentToSign)
        {
            this.Document = documentToSign;
            this.Document.BeforeSave += AddSignatureComponents;
            this.Document.AfterSave += ComputeSignatureAndRange;

            // estimate signature length by computing signature for a fake byte[]
            if (!maximumSignatureLengthByPdfVersion.ContainsKey(documentToSign.Version))
                maximumSignatureLengthByPdfVersion[documentToSign.Version] = signer.GetSignedCms(new MemoryStream(new byte[] { 0 }), documentToSign.Version).Length;
        }

        public PdfSignatureHandler(ISigner signer, PdfSignatureOptions options)
        {
            this.signer = signer;
            this.Options = options;
        }

        private void ComputeSignatureAndRange(object sender, PdfDocumentEventArgs e)
        {
            var writer = e.Writer;            

            // writing actual ByteRange in place of the placeholder

            var rangeArray = new PdfArray();
            rangeArray.Elements.Add(new PdfInteger(0));
            rangeArray.Elements.Add(new PdfInteger(signatureFieldContents.PositionStart));
            rangeArray.Elements.Add(new PdfInteger(signatureFieldContents.PositionEnd));
            rangeArray.Elements.Add(new PdfInteger((int)writer.Stream.Length - signatureFieldContents.PositionEnd));

            writer.Stream.Position = (signatureFieldByteRange as PdfArrayWithPadding).PositionStart;
            rangeArray.WriteObject(writer);


            // computing and writing document's digest

            var rangeToSign = GetRangeToSign(writer.Stream); // will exclude SignatureField's /Contents from hash computation

            var digest = signer.GetSignedCms(rangeToSign, Document.Version);
            if (digest.Length > maximumSignatureLengthByPdfVersion[Document.Version])
                throw new Exception("The digest length is bigger that the approximation made.");

            var hexFormatedDigest = Encoding.Default.GetBytes(FormatHex(digest));

            writer.Stream.Position = signatureFieldContents.PositionStart + 1/*' '*/ + 1/*'<'*/; // PositionStart starts right after /Contents, so we take into account the space separator and the starting '<' before writing the hash
            writer.Write(hexFormatedDigest);
        }

        string FormatHex(byte[] bytes) // starting from .net5, could be replaced by Convert.ToHexString(Byte[]). keeping current method to be ease .net48 compatibility
        {
            var retval = new StringBuilder();

            for (int idx = 0; idx < bytes.Length; idx++)
                retval.AppendFormat("{0:x2}", bytes[idx]);

            return retval.ToString();
        }

        private RangedStream GetRangeToSign(Stream stream)
        {
            return new RangedStream(stream, new List<RangedStream.Range>()
            {
                new RangedStream.Range(0, signatureFieldContents.PositionStart),
                new RangedStream.Range(signatureFieldContents.PositionEnd, stream.Length - signatureFieldContents.PositionEnd)
            });
        }

        private void AddSignatureComponents(object sender, EventArgs e)
        {            
            var hashPlaceholderValue = new String('0', maximumSignatureLengthByPdfVersion[Document.Version]);
            signatureFieldContents = new PdfString(hashPlaceholderValue, PdfStringFlags.HexLiteral);
            signatureFieldByteRange = new PdfArrayWithPadding(Document, byteRangePaddingLength, new PdfInteger(0), new PdfInteger(0), new PdfInteger(0), new PdfInteger(0));
            //Document.Internals.AddObject(signatureFieldByteRange);

            var signatureDictionary = GetSignatureDictionary(signatureFieldContents, signatureFieldByteRange);
            var signatureField = GetSignatureField(signatureDictionary);
            RenderAppearance(signatureField, Options.AppearanceHandler ?? new DefaultSignatureAppearanceHandler()
            {
                Location = Options.Location,
                Reason = Options.Reason,
                Signer = signer.GetName()
            });

            var annotations = Document.Pages[0].Elements.GetArray(PdfPage.Keys.Annots);
            if (annotations == null)
                Document.Pages[0].Elements.Add(PdfPage.Keys.Annots, new PdfArray(Document, signatureField));
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
                catalog.AcroForm.Elements.SetValue(PdfAcroForm.Keys.Fields, new PdfArray());
            catalog.AcroForm.Fields.Elements.Add(signatureField);
        }

        private PdfSignatureField GetSignatureField(PdfDictionary signatureDic)
        {
            var signatureField = new PdfSignatureField(Document);

            signatureField.Elements.Add(PdfSignatureField.Keys.V, signatureDic);

            // annotation keys
            signatureField.Elements.Add(PdfSignatureField.Keys.FT, new PdfName("/Sig"));
            signatureField.Elements.Add(PdfSignatureField.Keys.T, new PdfString("Signature1")); // TODO if already exists, will it cause error? implement a name choser if yes
            signatureField.Elements.Add(PdfSignatureField.Keys.Ff, new PdfInteger(132));
            signatureField.Elements.Add(PdfSignatureField.Keys.DR, new PdfDictionary());
            signatureField.Elements.Add(PdfSignatureField.Keys.Type, new PdfName("/Annot"));
            signatureField.Elements.Add("/Subtype", new PdfName("/Widget"));
            signatureField.Elements.Add("/P", Document.Pages[0]);
            
            signatureField.Elements.Add("/Rect", new PdfRectangle(Options.Rectangle));

            Document.Internals.AddObject(signatureField);

            return signatureField;
        }

        private void RenderAppearance(PdfSignatureField signatureField, IAnnotationAppearanceHandler appearanceHandler)
        {
            PdfRectangle rect = signatureField.Elements.GetRectangle(PdfAnnotation.Keys.Rect);

            var visible = !(rect.X1 + rect.X2 + rect.Y1 + rect.Y2 == 0);

            if (!visible)
                return;

            if (appearanceHandler == null)
                throw new Exception("AppearanceHandler is null");
            
            XForm form = new XForm(Document, rect.Size);
            XGraphics gfx = XGraphics.FromForm(form);

            appearanceHandler.DrawAppearance(gfx, rect.ToXRect());

            form.DrawingFinished();

            // Get existing or create new appearance dictionary
            PdfDictionary ap = signatureField.Elements[PdfAnnotation.Keys.AP] as PdfDictionary;
            if (ap == null)
            {
                ap = new PdfDictionary(Document);
                signatureField.Elements[PdfAnnotation.Keys.AP] = ap;
            }

            // Set XRef to normal state
            ap.Elements["/N"] = form.PdfForm.Reference;
        }

        private PdfDictionary GetSignatureDictionary(PdfString contents, PdfArray byteRange)
        {
            PdfDictionary signatureDic = new PdfDictionary(Document);

            signatureDic.Elements.Add(PdfSignatureField.Keys.Type, new PdfName("/Sig"));
            signatureDic.Elements.Add(PdfSignatureField.Keys.Filter, new PdfName("/Adobe.PPKLite"));
            signatureDic.Elements.Add(PdfSignatureField.Keys.SubFilter, new PdfName("/adbe.pkcs7.detached"));
            signatureDic.Elements.Add(PdfSignatureField.Keys.M, new PdfDate(DateTime.Now));

            signatureDic.Elements.Add(PdfSignatureField.Keys.Contents, contents);
            signatureDic.Elements.Add(PdfSignatureField.Keys.ByteRange, byteRange);
            signatureDic.Elements.Add(PdfSignatureField.Keys.Reason, new PdfString(Options.Reason));
            signatureDic.Elements.Add(PdfSignatureField.Keys.Location, new PdfString(Options.Location));

            Document.Internals.AddObject(signatureDic);

            return signatureDic;
        }        
    }
}
