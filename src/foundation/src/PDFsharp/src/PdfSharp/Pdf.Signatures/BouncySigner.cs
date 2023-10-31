// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;
#if WPF
using System.IO;
#endif
using System.Security.Cryptography.X509Certificates;

namespace PdfSharp.Pdf.Signatures
{
    public class BouncySigner : ISigner
    {
        private X509Certificate2 Certificate { get; set; }
        private X509Certificate2Collection CertificateChain { get; }

        public string GetName()
        {
            return Certificate.GetNameInfo(X509NameType.SimpleName, false);
        }

        public BouncySigner(Tuple<X509Certificate2, X509Certificate2Collection> certificateData)
        {
            this.Certificate = certificateData.Item1;
            this.CertificateChain = certificateData.Item2;
        }

        public byte[] GetSignedCms(Stream rangedStream, int pdfVersion)
        {
            rangedStream.Position = 0;

            CmsSignedDataGenerator signedDataGenerator = new CmsSignedDataGenerator();

            var cert = DotNetUtilities.FromX509Certificate(Certificate);
            var key = DotNetUtilities.GetKeyPair(Certificate.PrivateKey);
            var allCerts = CertificateChain.OfType<X509Certificate2>().Select(item => DotNetUtilities.FromX509Certificate(item));

            var store = CollectionUtilities.CreateStore(allCerts);

            signedDataGenerator.AddSigner(key.Private, cert, GetProperDigestAlgorithm(pdfVersion));
            signedDataGenerator.AddCertificates(store);

            CmsProcessableInputStream msg = new CmsProcessableInputStream(rangedStream);

            CmsSignedData signedData = signedDataGenerator.Generate(msg, false);

            return signedData.GetEncoded();
        }

        /// <summary>
        /// adbe.pkcs7.detached supported algorithms: SHA1 (PDF 1.3), SHA256 (PDF 1.6), SHA384/SHA512/RIPEMD160 (PDF 1.7)
        /// </summary>
        /// <param name="pdfVersion">PDF version as int</param>
        /// <returns></returns>
        private string GetProperDigestAlgorithm(int pdfVersion)
        {
            switch (pdfVersion)
            {
                case int when pdfVersion >= 17:
                    return CmsSignedDataGenerator.DigestSha512;
                case int when pdfVersion == 16:
                    return CmsSignedDataGenerator.DigestSha256;
                case int when pdfVersion >= 13:
                default:
                    return CmsSignedDataGenerator.DigestSha256; // SHA1 is obsolete, use at least SHA256
            }
        }
    }
}