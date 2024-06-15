// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace PdfSharp.Pdf.Signatures
{
    public class DefaultSigner : ISigner
    {
        private static readonly Oid SignatureTimeStampOin = new Oid("1.2.840.113549.1.9.16.2.14");
        private static readonly string TimestampQueryContentType = "application/timestamp-query";
        private static readonly string TimestampReplyContentType = "application/timestamp-reply";

        private readonly PdfSignatureOptions options;

        public DefaultSigner(PdfSignatureOptions signatureOptions)
        {
            if (signatureOptions?.Certificate is null)
                throw new ArgumentException("Missing certificate in signature options");

            options = signatureOptions;
        }

        public byte[] GetSignedCms(Stream documentStream, PdfDocument document)
        {
            var range = new byte[documentStream.Length];
            documentStream.Position = 0;
            documentStream.Read(range, 0, range.Length);

            return GetSignedCms(range, document);
        }

        public byte[] GetSignedCms(byte[] range, PdfDocument document)
        {
            // Sign the byte range
            var contentInfo = new ContentInfo(range);
            var signedCms = new SignedCms(contentInfo, true);
            var signer = new CmsSigner(options.Certificate)/* { IncludeOption = X509IncludeOption.WholeChain }*/;
            signer.UnsignedAttributes.Add(new Pkcs9SigningTime());

            signedCms.ComputeSignature(signer, true);

            if (options.TimestampAuthorityUri is not null)
                Task.Run(() => AddTimestampFromTSAAsync(signedCms)).Wait();

            var bytes = signedCms.Encode();

            return bytes;
        }

        public string? GetName()
        {
            return options.Certificate?.GetNameInfo(X509NameType.SimpleName, false);
        }

        private async Task AddTimestampFromTSAAsync(SignedCms signedCms)
        {
            // Generate our nonce to identify the pair request-response
            byte[] nonce = new byte[8];
#if NET6_0_OR_GREATER
            nonce = RandomNumberGenerator.GetBytes(8);
#else
            using var cryptoProvider = new RNGCryptoServiceProvider();
            cryptoProvider.GetBytes(nonce = new Byte[8]);
#endif
#if NET6_0_OR_GREATER
            // Get our signing information and create the RFC3161 request
            SignerInfo newSignerInfo = signedCms.SignerInfos[0];
            // Now we generate our request for us to send to our RFC3161 signing authority.
            var request = Rfc3161TimestampRequest.CreateFromSignerInfo(
                newSignerInfo,
                HashAlgorithmName.SHA256,
                requestSignerCertificates: true, // ask TSA to embed its signing certificate in the timestamp token
                nonce: nonce);

            var client = new HttpClient();
            var content = new ReadOnlyMemoryContent(request.Encode());
            content.Headers.ContentType = new MediaTypeHeaderValue(TimestampQueryContentType);
            var httpResponse = await client.PostAsync(options.TimestampAuthorityUri, content).ConfigureAwait(false);

            // Process our response
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new CryptographicException(
                    $"There was a error from the timestamp authority. It responded with {httpResponse.StatusCode} {(int)httpResponse.StatusCode}: {httpResponse.Content}");
            }
            if (httpResponse.Content.Headers.ContentType?.MediaType != TimestampReplyContentType)
            {
                throw new CryptographicException("The reply from the time stamp server was in a invalid format.");
            }
            var data = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var timestampToken = request.ProcessResponse(data, out _);

            // The RFC3161 sign certificate is separate to the contents that was signed, we need to add it to the unsigned attributes.
            newSignerInfo.AddUnsignedAttribute(new AsnEncodedData(SignatureTimeStampOin, timestampToken.AsSignedCms().Encode()));
#endif
        }
    }
}