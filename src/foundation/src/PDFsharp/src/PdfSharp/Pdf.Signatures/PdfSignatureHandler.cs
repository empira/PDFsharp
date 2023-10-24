// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;
#if WPF
using System.IO;
#endif

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// PdfDocument signature handler.
    /// Attaches a PKCS#7 signature digest to PdfDocument.
    /// Digest algorithm will be either SHA1/SHA256/SHA512 depending on PdfDocument.Version.
    /// </summary>
    public class PdfSignatureHandler
    {
        private PdfString signatureFieldContentsPdfString;
        private PdfArray signatureFieldByteRangePdfArray;

        /// <summary>
        /// Cache signature length (bytes) for each PDF version since digest length depends on digest algorithm that depends on PDF version.
        /// </summary>
        private static Dictionary<int, int> knownSignatureLengthInBytesByPdfVersion = new Dictionary<int, int>();

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
            if (!knownSignatureLengthInBytesByPdfVersion.ContainsKey(documentToSign.Version))
                knownSignatureLengthInBytesByPdfVersion[documentToSign.Version] = signer.GetSignedCms(new MemoryStream(new byte[] { 0 }), documentToSign.Version).Length;
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

            var byteRangeArray = new PdfArray();
            byteRangeArray.Elements.Add(new PdfInteger(0));
            byteRangeArray.Elements.Add(new PdfInteger(signatureFieldContentsPdfString.PositionStart - PdfSignatureField.Keys.Contents.Length)); // PositionStart actually indicates position of the HexLiteral string right after the /Contents so we need to exclude full /Contents entry
            byteRangeArray.Elements.Add(new PdfInteger(signatureFieldContentsPdfString.PositionEnd));
            byteRangeArray.Elements.Add(new PdfInteger((int)writer.Stream.Length - signatureFieldContentsPdfString.PositionEnd));

            writer.Stream.Position = (signatureFieldByteRangePdfArray as PdfArrayWithPadding).PositionStart;
            byteRangeArray.WriteObject(writer);

            // computing and writing document's digest

            var rangedStreamToSign = GetRangeToSign(writer.Stream); // will exclude SignatureField's /Contents entry from hash computation

            var signature = signer.GetSignedCms(rangedStreamToSign, Document.Version);
            if (signature.Length != knownSignatureLengthInBytesByPdfVersion[Document.Version])
                throw new Exception("The digest length is different that the approximation made.");

            var signatureAsRawString = PdfEncoders.RawEncoding.GetString(signature, 0, signature.Length);
            var tempContentsPdfString = new PdfString(signatureAsRawString, PdfStringFlags.HexLiteral); // has to be a hex string
            writer.Stream.Position = signatureFieldContentsPdfString.PositionStart + 1/*' '*/; // tempContentsPdfString is orphan, so it will not write the space. need to begin write 1 byte further
            tempContentsPdfString.WriteObject(writer);
        }

        private RangedStream GetRangeToSign(Stream stream)
        {
            return new RangedStream(stream, new List<RangedStream.Range>()
            {
                new RangedStream.Range(0, signatureFieldContentsPdfString.PositionStart - PdfSignatureField.Keys.Contents.Length), // PositionStart actually indicates position of the HexLiteral string right after the /Contents so we need to exclude full /Contents entry
                new RangedStream.Range(signatureFieldContentsPdfString.PositionEnd, stream.Length - signatureFieldContentsPdfString.PositionEnd)
            });
        }

        private void AddSignatureComponents(object sender, EventArgs e)
        {
            var fakeSignature = Enumerable.Repeat((byte)0x20/*actual value does not matter*/, knownSignatureLengthInBytesByPdfVersion[Document.Version]).ToArray();
            var fakeSignatureAsRawString = PdfEncoders.RawEncoding.GetString(fakeSignature, 0, fakeSignature.Length);
            signatureFieldContentsPdfString = new PdfString(fakeSignatureAsRawString, PdfStringFlags.HexLiteral);
            signatureFieldByteRangePdfArray = new PdfArrayWithPadding(Document, byteRangePaddingLength, new PdfInteger(0), new PdfInteger(0), new PdfInteger(0), new PdfInteger(0));
            //Document.Internals.AddObject(signatureFieldByteRange);

            var signatureDictionary = GetSignatureDictionary(signatureFieldContentsPdfString, signatureFieldByteRangePdfArray);
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
                catalog.AcroForm.Elements.SetValue(PdfAcroForm.Keys.Fields, new PdfAcroField.PdfAcroFieldCollection(new PdfArray()));
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
