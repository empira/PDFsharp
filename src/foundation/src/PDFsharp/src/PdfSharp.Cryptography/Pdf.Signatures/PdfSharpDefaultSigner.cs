// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if NET6_0_OR_GREATER
using System.Net.Http.Headers;
#endif
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// Signer implementation that uses .NET classes only.
    /// </summary>
    public class PdfSharpDefaultSigner : IDigitalSigner
    {
        static readonly Oid SignatureTimeStampOid = new("1.2.840.113549.1.9.16.2.14");
#if NET6_0_OR_GREATER
        const string TimestampQueryContentType = "application/timestamp-query";
        const string TimestampReplyContentType = "application/timestamp-reply";
#endif

        /// <summary>
        /// Creates a new instance of the PdfSharpDefaultSigner class.
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="digestType"></param>
        /// <param name="timeStampAuthorityUri"></param>
        public PdfSharpDefaultSigner(X509Certificate2 certificate, PdfMessageDigestType digestType, Uri? timeStampAuthorityUri = null)
        {
            Certificate = certificate;
            DigestType = digestType;
#if NET6_0_OR_GREATER
            TimeStampAuthorityUri = timeStampAuthorityUri;
#else
            // We don’t know how to get a time stamp with .NET Standard.
            // If you need it you must implement your own signer.
            if (timeStampAuthorityUri != null)
                throw new ArgumentException(nameof(timeStampAuthorityUri) + " must be null when using .NET Framework or .NET Standard.");
#endif
            MustAddTimeStamp = timeStampAuthorityUri is not null;
        }

        /// <summary>
        /// Gets the name of the certificate.
        /// </summary>
        public string CertificateName => Certificate.GetNameInfo(X509NameType.SimpleName, false) ?? "(undefined)";

        /// <summary>
        /// Determines the size of the contents to be reserved in the PDF file for the signature.
        /// </summary>
        public async Task<int> GetSignatureSizeAsync()
        {
            if (_signatureSize == 0)
            {
                // The signature size varies depending on the length of the digest
                // and the size of the certificate.
                // We simply calculate it once by signing an empty stream.
                _signatureSize = (await GetSignatureAsync(new MemoryStream([0])).ConfigureAwait(false)).Length;
                if (MustAddTimeStamp)
                {
                    // Add arbitrary padding because TSA timestamp response’s length seems to vary from one call to another by 1 byte.
                    _signatureSize += 10; // 2 was found to be too small. Make it 10 to allow for some DSA variation.
                }
                else
                {
                    // With DSA, signature size varies without timestamp. A variation of 2 bytes was observed. Make it 4 to allow more variation.
                    _signatureSize += 4;
                }
            }
            return _signatureSize;
        }
        int _signatureSize;

        /// <summary>
        /// Creates the signature for a stream containing the PDF file.
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<byte[]> GetSignatureAsync(Stream stream)
        {
            var content = new byte[stream.Length];
            stream.Position = 0;
            // ReSharper disable once MethodHasAsyncOverload
            int read = stream.Read(content, 0, content.Length);
            if (read != content.Length)
                throw new InvalidOperationException($"Tried to read {content.Length} bytes but got {read} bytes.");

            // Sign the byte range.
            var contentInfo = new ContentInfo(content);
            var signedCms = new SignedCms(contentInfo, true);
            var signer = new CmsSigner(Certificate)
            {
                DigestAlgorithm = DigestType switch
                {
                    PdfMessageDigestType.SHA1 => Oid.FromFriendlyName("sha1", OidGroup.HashAlgorithm),
                    PdfMessageDigestType.SHA256 => Oid.FromFriendlyName("sha256", OidGroup.HashAlgorithm),
                    PdfMessageDigestType.SHA384 => Oid.FromFriendlyName("sha384", OidGroup.HashAlgorithm),
                    PdfMessageDigestType.SHA512 => Oid.FromFriendlyName("sha512", OidGroup.HashAlgorithm),
                    // PdfMessageDigestType.RIPEMD160 => Oid.FromFriendlyName("???"), // ???
                    _ => throw new NotImplementedException($"Digest type {DigestType} not supported by this signer.")
                }
            } /* { IncludeOption = X509IncludeOption.WholeChain } */;
            signer.UnsignedAttributes.Add(new Pkcs9SigningTime());

            signedCms.ComputeSignature(signer, true);

            if (TimeStampAuthorityUri is not null)
            {
#if NET6_0_OR_GREATER
                await AddTimestampFromTSAAsync(signedCms).ConfigureAwait(false);
#else
                // Already checked in constructor.
                await Task.CompletedTask.ConfigureAwait(false);
#endif
            }
            var bytes = signedCms.Encode();

            return bytes;
        }

        X509Certificate2 Certificate { get; init; }

        PdfMessageDigestType DigestType { get; init; }

        Uri? TimeStampAuthorityUri { get; init; }

        bool MustAddTimeStamp { get; init; }

#if NET6_0_OR_GREATER
        async Task AddTimestampFromTSAAsync(SignedCms signedCms)
        {
            // Generate our nonce to identify the pair request-response.

            var nonce = RandomNumberGenerator.GetBytes(8);

            // Create nonce in .NET Framework for future use.
            // If we ever add timestamp code for .NET 4.6.2, here's how to get the nonce.
            //using var cryptoProvider = new RNGCryptoServiceProvider();
            //byte[] nonce;
            //cryptoProvider.GetBytes(nonce = new byte[8]);

            // Get our signing information and create the RFC3161 request.
            var newSignerInfo = signedCms.SignerInfos[0];
            HashAlgorithmName hashAlgorithm = DigestType switch
            {
                PdfMessageDigestType.SHA1 => HashAlgorithmName.SHA1,
                PdfMessageDigestType.SHA256 => HashAlgorithmName.SHA256,
                PdfMessageDigestType.SHA384 => HashAlgorithmName.SHA384,
                PdfMessageDigestType.SHA512 => HashAlgorithmName.SHA512,
                // PdfMessageDigestType.RIPEMD160 => HashAlgorithmName.SHA512, // ???
                _ => throw new NotImplementedException($"Digest type {DigestType} not supported by this signer.")
            };

            // Now we generate the request to send to the RFC3161 signing authority.
            var request = Rfc3161TimestampRequest.CreateFromSignerInfo(
                newSignerInfo,
                hashAlgorithm,
                requestSignerCertificates: true, // Ask TSA to embed its signing certificate in the timestamp token.
                nonce: nonce);

            var client = new HttpClient();
            var content = new ReadOnlyMemoryContent(request.Encode());
            content.Headers.ContentType = new MediaTypeHeaderValue(TimestampQueryContentType);
            var httpResponse = await client.PostAsync(TimeStampAuthorityUri, content).ConfigureAwait(false);

            // Process our response.
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new CryptographicException(
                    $"There was an error from the timestamp authority. It responded with {httpResponse.StatusCode} {(int)httpResponse.StatusCode}: {httpResponse.Content}");
            }

            // Response indicates success, so we assume there is content.
            if (httpResponse.Content.Headers.ContentType?.MediaType != TimestampReplyContentType)
                throw new CryptographicException("The reply from the time stamp server was in an invalid format.");

            var data = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var timestampToken = request.ProcessResponse(data, out _);

            // The RFC3161 sign certificate is separate to the contents that was signed, we need to add it to the unsigned attributes.
            newSignerInfo.AddUnsignedAttribute(new AsnEncodedData(SignatureTimeStampOid, timestampToken.AsSignedCms().Encode()));
        }
#endif
    }
}
