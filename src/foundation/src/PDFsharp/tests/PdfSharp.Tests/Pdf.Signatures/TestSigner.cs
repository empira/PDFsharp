// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Signatures;

namespace PdfSharp.Tests.Pdf.Signatures
{
    /// <summary>
    /// A signer that creates a deterministic dummy signature, so that tests need no certificate.
    /// </summary>
    class TestSigner : IDigitalSigner
    {
        public string CertificateName => "PDFsharp unit test";

        public Task<int> GetSignatureSizeAsync() => Task.FromResult(SignatureSize);

        public Task<byte[]> GetSignatureAsync(Stream stream)
        {
            // Read the stream to ensure the ranges to be signed are readable.
            var buffer = new byte[4096];
            while (stream.Read(buffer, 0, buffer.Length) > 0)
            { }

            var signature = new byte[SignatureSize];
            for (int idx = 0; idx < signature.Length; idx++)
                signature[idx] = (byte)idx;
            return Task.FromResult(signature);
        }

        const int SignatureSize = 512;
    }

    /// <summary>
    /// An appearance handler that draws nothing, so that tests need no font.
    /// </summary>
    class EmptyAppearanceHandler : IAnnotationAppearanceHandler
    {
        public void DrawAppearance(XGraphics gfx, XRect rect)
        { }
    }
}
