﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using System.Text;
#if WPF
using System.IO;
#endif

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// PdfDocument signature handler.
    /// Attaches a PKCS#7 signature digest to PdfDocument.
    /// Digest algorithm will be either SHA256/SHA512 depending on PdfDocument.Version.
    /// </summary>
    public class PdfSignatureHandler
    {
        private PdfString signatureFieldContentsPdfString;
        private PdfArray signatureFieldByteRangePdfArray;

        /// <summary>
        /// Cached signature length (in bytes) for each PDF version since digest length depends on digest algorithm that depends on PDF version.
        /// </summary>
        private static Dictionary<int, int> knownSignatureLengthInBytesByPdfVersion = new();

        /// <summary>
        /// (arbitrary) big enough reserved space to replace ByteRange placeholder [0 0 0 0] with the actual computed value of the byte range to sign
        /// </summary>
        private const int byteRangePaddingLength = 36;

        /// <summary>
        /// Pdf Document signature will be attached to
        /// </summary>
        public PdfDocument Document { get; private set; }

        /// <summary>
        /// Signature options
        /// </summary>
        public PdfSignatureOptions Options { get; private set; }
        private ISigner signer { get; set; }

        /// <summary>
        /// Attach this signature handler to the given Pdf document
        /// </summary>
        /// <param name="documentToSign">Pdf document to sign</param>
        public void AttachToDocument(PdfDocument documentToSign)
        {
            this.Document = documentToSign;
            this.Document.BeforeSave += AddSignatureComponents;
            this.Document.AfterSave += ComputeSignatureAndRange;

            // estimate signature length by computing signature for a fake byte[]
            if (!knownSignatureLengthInBytesByPdfVersion.ContainsKey(documentToSign.Version))
                knownSignatureLengthInBytesByPdfVersion[documentToSign.Version] = 
                    signer.GetSignedCms(new MemoryStream(new byte[] { 0 }), documentToSign.Version).Length
                    + 10 /* arbitrary margin added because TSA timestamp response's length seems to vary from a call to another (I saw a variation of 1 byte) */;
        }

        public PdfSignatureHandler(ISigner signer, PdfSignatureOptions options)
        {
            if (signer is null)
                throw new ArgumentNullException(nameof(signer));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (options.PageIndex < 0)
                throw new ArgumentOutOfRangeException($"Signature page index cannot be negative.");

            this.signer = signer;
            this.Options = options;
        }

        private void ComputeSignatureAndRange(object sender, PdfDocumentEventArgs e)
        {
            var writer = e.Writer;

            var isVerbose = writer.Layout == IO.PdfWriterLayout.Verbose; // DEBUG mode makes the writer Verbose and will introduce 1 extra space between entries key and value
            // if Verbose, a space is added between entry key and entry value
            var verboseExtraSpaceSeparatorLength = isVerbose ? 1 : 0;

            var (rangedStreamToSign, byteRangeArray) = GetRangeToSignAndByteRangeArray(writer.Stream, verboseExtraSpaceSeparatorLength);

            // writing actual ByteRange in place of the placeholder

            writer.Stream.Position = (signatureFieldByteRangePdfArray as PdfArrayWithPadding).PositionStart;
            byteRangeArray.WriteObject(writer);

            // computing signature from document's digest
            var signature = signer.GetSignedCms(rangedStreamToSign, Document.Version);

            if (signature.Length > knownSignatureLengthInBytesByPdfVersion[Document.Version])
                throw new Exception("The actual digest length is bigger that the approximation made. Not enough room in the placeholder to fit the signature.");

            // directly writes document's signature in the /Contents<> entry
            writer.Stream.Position = signatureFieldContentsPdfString.PositionStart
                + verboseExtraSpaceSeparatorLength /* tempContentsPdfString is orphan, so it will not write the space delimiter: need to begin write 1 byte further if Verbose */
                + 1 /* skip the begin-delimiter '<' */;
            writer.Write(PdfEncoders.RawEncoding.GetBytes(FormatHex(signature)));
        }

        private string FormatHex(byte[] bytes) // starting from .net5, could be replaced by Convert.ToHexString(Byte[]). keeping current method to be ease .net48/netstandard compatibility
        {
            var retval = new StringBuilder();

            for (int idx = 0; idx < bytes.Length; idx++)
                retval.AppendFormat("{0:X2}", bytes[idx]);

            return retval.ToString();
        }

        /// <summary>
        /// Get the bytes ranges to sign.
        /// As recommended in PDF specs, whole document will be signed, except for the hexadecimal signature token value in the /Contents entry.
        /// Example: '/Contents &lt;aaaaa111111&gt;' => '&lt;aaaaa111111&gt;' will be excluded from the bytes to sign.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="verboseExtraSpaceSeparatorLength"></param>
        /// <returns></returns>
        private (RangedStream rangedStream, PdfArray byteRangeArray) GetRangeToSignAndByteRangeArray(Stream stream, int verboseExtraSpaceSeparatorLength)
        {
            long firstRangeOffset = 0,
                firstRangeLength = signatureFieldContentsPdfString.PositionStart + verboseExtraSpaceSeparatorLength,
                secondRangeOffset = signatureFieldContentsPdfString.PositionEnd,
                secondRangeLength = (int)stream.Length - signatureFieldContentsPdfString.PositionEnd;

            var byteRangeArray = new PdfArray();
            byteRangeArray.Elements.Add(new PdfLongInteger(firstRangeOffset));
            byteRangeArray.Elements.Add(new PdfLongInteger(firstRangeLength));
            byteRangeArray.Elements.Add(new PdfLongInteger(secondRangeOffset));
            byteRangeArray.Elements.Add(new PdfLongInteger(secondRangeLength));

            var rangedStream = new RangedStream(stream, new List<RangedStream.Range>()
            {
                new RangedStream.Range(firstRangeOffset, firstRangeLength),
                new RangedStream.Range(secondRangeOffset, secondRangeLength)
            });

            return (rangedStream, byteRangeArray);
        }

        private void AddSignatureComponents(object sender, EventArgs e)
        {
            if (Options.PageIndex >= Document.PageCount)
                throw new ArgumentOutOfRangeException($"Signature page doesn't exist, specified page was {Options.PageIndex + 1} but document has only {Document.PageCount} page(s).");

            var fakeSignature = Enumerable.Repeat((byte)0x00/*padded with zeros, as recommended (trailing zeros have no incidence on signature decoding)*/, knownSignatureLengthInBytesByPdfVersion[Document.Version]).ToArray();
            var fakeSignatureAsRawString = PdfEncoders.RawEncoding.GetString(fakeSignature, 0, fakeSignature.Length);
            signatureFieldContentsPdfString = new PdfString(fakeSignatureAsRawString, PdfStringFlags.HexLiteral); // has to be a hex string
            signatureFieldByteRangePdfArray = new PdfArrayWithPadding(Document, byteRangePaddingLength, new PdfLongInteger(0), new PdfLongInteger(0), new PdfLongInteger(0), new PdfLongInteger(0));
            //Document.Internals.AddObject(signatureFieldByteRange);

            var signatureDictionary = GetSignatureDictionary(signatureFieldContentsPdfString, signatureFieldByteRangePdfArray);
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

        private PdfSignatureField GetSignatureField(PdfDictionary signatureDic)
        {
            var signatureField = new PdfSignatureField(Document);

            signatureField.Elements.Add(PdfSignatureField.Keys.V, signatureDic);

            // annotation keys
            signatureField.Elements.Add(PdfSignatureField.Keys.FT, new PdfName("/Sig"));
            signatureField.Elements.Add(PdfSignatureField.Keys.T, new PdfString("Signature1")); // TODO? if already exists, will it cause error? implement a name choser if yes
            signatureField.Elements.Add(PdfSignatureField.Keys.Ff, new PdfInteger(132));
            signatureField.Elements.Add(PdfSignatureField.Keys.DR, new PdfDictionary());
            signatureField.Elements.Add(PdfSignatureField.Keys.Type, new PdfName("/Annot"));
            signatureField.Elements.Add("/Subtype", new PdfName("/Widget"));
            signatureField.Elements.Add("/P", Document.Pages[Options.PageIndex]);

            signatureField.Elements.Add("/Rect", new PdfRectangle(Options.Rectangle));

            signatureField.CustomAppearanceHandler = Options.AppearanceHandler ?? new DefaultSignatureAppearanceHandler()
            {
                Location = Options.Location,
                Reason = Options.Reason,
                Signer = signer.GetName()
            };
            signatureField.PrepareForSave(); // TODO: for some reason, PdfSignatureField.PrepareForSave() is not triggered automatically so let's call it manually from here, but it would be better to be called automatically

            Document.Internals.AddObject(signatureField);

            return signatureField;
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
