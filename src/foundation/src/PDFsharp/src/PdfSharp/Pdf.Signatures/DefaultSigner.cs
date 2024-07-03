﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
#if NET6_0_OR_GREATER
using System.Net.Http;
using System.Net.Http.Headers;
#endif
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

        private X509Certificate2 _certificate { get; init; }
        private Uri? _timeStampAuthorityUri { get; init; }

        public DefaultSigner(X509Certificate2 Certificate)
        {
            _certificate = Certificate;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// using a TimeStamp Authority to add timestamp to signature, only on net6+ for now due to available classes for Rfc3161
        /// </summary>
        /// <param name="Certificate"></param>
        /// <param name="timeStampAuthorityUri"></param>
        public DefaultSigner(X509Certificate2 Certificate, Uri? timeStampAuthorityUri = null)
        {
            _certificate = Certificate;
            _timeStampAuthorityUri = timeStampAuthorityUri;
        }
#endif

        public byte[] GetSignedCms(Stream stream, int pdfVersion)
        {
            var range = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(range, 0, range.Length);

            // Sign the byte range
            var contentInfo = new ContentInfo(range);
            SignedCms signedCms = new SignedCms(contentInfo, true);
            CmsSigner signer = new CmsSigner(_certificate)/* { IncludeOption = X509IncludeOption.WholeChain }*/;
            signer.UnsignedAttributes.Add(new Pkcs9SigningTime());

            signedCms.ComputeSignature(signer, true);

#if NET6_0_OR_GREATER
            if (_timeStampAuthorityUri is not null)
                Task.Run(() => AddTimestampFromTSAAsync(signedCms)).Wait();
#endif

            var bytes = signedCms.Encode();

            return bytes;
        }

        public string GetName()
        {
            return _certificate.GetNameInfo(X509NameType.SimpleName, false);
        }

#if NET6_0_OR_GREATER
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
            var httpResponse = await client.PostAsync(_timeStampAuthorityUri, content).ConfigureAwait(false);

            // Process our response
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new CryptographicException(
                    $"There was a error from the timestamp authority. It responded with {httpResponse.StatusCode} {(int)httpResponse.StatusCode}: {httpResponse.Content}");
            }
            if (httpResponse.Content.Headers.ContentType.MediaType != TimestampReplyContentType)
            {
                throw new CryptographicException("The reply from the time stamp server was in a invalid format.");
            }
            var data = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var timestampToken = request.ProcessResponse(data, out _);

            // The RFC3161 sign certificate is separate to the contents that was signed, we need to add it to the unsigned attributes.
#if NET6_0_OR_GREATER
            newSignerInfo.AddUnsignedAttribute(new AsnEncodedData(SignatureTimeStampOin, timestampToken.AsSignedCms().Encode()));
#else
            newSignerInfo.UnsignedAttributes.Add(new AsnEncodedData(SignatureTimeStampOin, timestampToken.AsSignedCms().Encode()));
#endif
        }
#endif
    }
}
