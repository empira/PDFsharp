// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;
using PdfSharp.Pdf.Signatures;

namespace PdfSharp.Snippets.Pdf
{
    /// <summary>
    /// Implementation of IDigitalSigner using BouncyCastle.
    /// </summary>
    public class BouncyCastleSigner((X509Certificate2, X509Certificate2Collection) certificateData, PdfMessageDigestType digestType) : IDigitalSigner
    {
        /// <summary>
        /// Gets the name of the certificate.
        /// </summary>
        public string CertificateName => Certificate.GetNameInfo(X509NameType.SimpleName, false);

        public async Task<int> GetSignatureSizeAsync()
        {
            if (_signatureSize == 0)
            {
                // The signature size varies depending on the length of the digest and the size of the certificate.
                // We simply calculate it once by signing an empty stream.
                _signatureSize = (await GetSignatureAsync(new MemoryStream([0])).ConfigureAwait(false)).Length;

                // With DSA, signature size varies without timestamp. A variation of 2 bytes was observed. Make it 4 to allow more variation.
                _signatureSize += 4;
            }
            return _signatureSize;
        }
        int _signatureSize;

        public async Task<byte[]> GetSignatureAsync(Stream stream)
        {
            await Task.CompletedTask.ConfigureAwait(false);

            stream.Position = 0;

            var signedDataGenerator = new CmsSignedDataGenerator();

            var cert = DotNetUtilities.FromX509Certificate(Certificate);
            var key = DotNetUtilities.GetKeyPair(GetAsymmetricAlgorithm(Certificate));
#if NET6_0_OR_GREATER
            var allCerts = CertificateChain.Select(DotNetUtilities.FromX509Certificate);
#else
            var allCerts = CertificateChain.OfType<X509Certificate2>().Select(DotNetUtilities.FromX509Certificate);
#endif

            var store = CollectionUtilities.CreateStore(allCerts);

            signedDataGenerator.AddSigner(key.Private, cert, GetProperDigestAlgorithm(DigestType));
            signedDataGenerator.AddCertificates(store);

            var msg = new CmsProcessableInputStream(stream);

            var signedData = signedDataGenerator.Generate(msg, false);

            return signedData.GetEncoded();
        }


        X509Certificate2 Certificate { get; set; } = certificateData.Item1;

        X509Certificate2Collection CertificateChain { get; } = certificateData.Item2;

        PdfMessageDigestType DigestType { get; } = digestType;

        //byte[] GetSignedCms(Stream rangedStream, int pdfVersion)
        //{
        //}

        /// <summary>
        /// adbe.pkcs7.detached supported algorithms:
        /// SHA1 (PDF 1.3), SHA256 (PDF 1.6), SHA384/SHA512/RIPEMD160 (PDF 1.7)
        /// </summary>
        /// <param name="pdfVersion">PDF version as int</param>
        string GetProperDigestAlgorithm(int pdfVersion)
        {
            return pdfVersion switch
            {
                >= 17 => CmsSignedGenerator.DigestSha512,
                16 => CmsSignedGenerator.DigestSha256,
                >= 13 => CmsSignedGenerator.DigestSha256, // SHA1 is obsolete, use at least SHA256

                _ => CmsSignedGenerator.DigestSha256
            };
        }

        string GetProperDigestAlgorithm(PdfMessageDigestType digestType)
        {
            return digestType switch
            {
                PdfMessageDigestType.SHA1 => CmsSignedGenerator.DigestSha1,  // SHA1 is obsolete, but you can use it.
                PdfMessageDigestType.SHA256 => CmsSignedGenerator.DigestSha256,
                PdfMessageDigestType.SHA384 => CmsSignedGenerator.DigestSha384,
                PdfMessageDigestType.SHA512 => CmsSignedGenerator.DigestSha512,
                PdfMessageDigestType.RIPEMD160 => CmsSignedGenerator.DigestRipeMD160,
                _ => throw new ArgumentOutOfRangeException(nameof(digestType), digestType, null)
            };
        }

        AsymmetricAlgorithm? GetAsymmetricAlgorithm(X509Certificate2 cert)
        {
            const string RSA = "1.2.840.113549.1.1.1";
#if NET6_0_OR_GREATER
            const string DSA = "1.2.840.10040.4.1";
#endif
            const string ECC = "1.2.840.10045.2.1";

            return cert.PublicKey.Oid.Value switch
            {
                RSA => cert.GetRSAPrivateKey(),
#if NET6_0_OR_GREATER
                DSA => cert.GetDSAPrivateKey(),
#endif
                ECC => cert.GetECDsaPrivateKey(),

                _ => throw new NotImplementedException($"Unexpected OID value '{cert.PublicKey.Oid.Value}' for certificate."),
            };
        }
    }
}
