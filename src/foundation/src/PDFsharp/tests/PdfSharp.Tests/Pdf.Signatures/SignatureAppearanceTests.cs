// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Signatures;
#if CORE
using PdfSharp.Fonts;
using PdfSharp.Quality;
#endif
using Xunit;

namespace PdfSharp.Tests.Pdf.Signatures
{
    [Collection("PDFsharp")]
    public class SignatureAppearanceTests : IDisposable
    {
        public SignatureAppearanceTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Signature_is_printed()
        {
            using var document = new PdfDocument();
            document.AddPage();

            var options = new DigitalSignatureOptions
            {
                Reason = "License Agreement",
                Rectangle = new XRect(36, 36, 200, 50),
                AppearanceHandler = new EmptyAppearanceHandler()
            };
            _ = DigitalSignatureHandler.ForDocument(document, new TestSigner(), options);

            using var stream = new MemoryStream();
            document.Save(stream, false);

            using var signedDocument = PdfReader.Open(new MemoryStream(stream.ToArray()), PdfDocumentOpenMode.Import);
            var field = signedDocument.Catalog.GetAcroForm()!.Elements
                .GetArray(PdfForm.Keys.Fields)!.Elements.GetRequiredDictionary(0);

            var flags = (PdfAnnotationFlags)field.Elements.GetInteger(PdfAnnotation.Keys.F);
            flags.Should().HaveFlag(PdfAnnotationFlags.Print,
                "a signature that is not printed is missing on the paper the document is printed on");
            flags.Should().NotHaveFlag(PdfAnnotationFlags.Hidden);
            flags.Should().NotHaveFlag(PdfAnnotationFlags.NoView);
        }

        /// <summary>
        /// A signer that creates a deterministic dummy signature, so that no certificate is needed.
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
        /// An appearance handler that draws nothing, so that no font is needed.
        /// </summary>
        class EmptyAppearanceHandler : IAnnotationAppearanceHandler
        {
            public void DrawAppearance(XGraphics gfx, XRect rect)
            { }
        }
    }
}
