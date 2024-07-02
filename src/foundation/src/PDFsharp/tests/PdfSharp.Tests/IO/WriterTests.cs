// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Signatures;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class WriterTests
    {
        [Fact]
        public void Write_import_file()
        {
            var testFile = IOUtility.GetAssetsPath("archives/samples-1.5/PDFs/SomeLayout.pdf")!;

            var filename = PdfFileUtility.GetTempPdfFileName("ImportTest");

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(filename);
            save.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Append_To_File()
        {
            var sourceFile = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/Table-Layout.pdf")!;
            var targetFile = Path.Combine(Path.GetTempPath(), "AA-Append.pdf");
            File.Copy(sourceFile, targetFile, true);

            using var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite);
            var doc = PdfReader.Open(fs, PdfDocumentOpenMode.Append);
            var numPages = doc.PageCount;
            var numContentsPerPage = new List<int>();
            foreach (var page in doc.Pages)
            {
                // remember count of existing contents
                numContentsPerPage.Add(page.Contents.Elements.Count);
                // add new content
                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawString("I was added", new XFont("Arial", 16), new XSolidBrush(XColors.Red), 40, 40);
            }

            doc.Save(fs, true);

            // verify that the new content is picked up
            var idx = 0;
            doc = PdfReader.Open(targetFile, PdfDocumentOpenMode.Import);
            doc.PageCount.Should().Be(numPages);
            foreach (var page in doc.Pages)
            {
                var c = page.Contents.Elements.Count;
                c.Should().Be(numContentsPerPage[idx] + 1);
                idx++;
            }
        }

        [Fact]
        public void Sign()
        {
            var cert = new X509Certificate2(@"C:\Data\packdat.pfx", "1234");
            var options = new PdfSignatureOptions
            {
                Certificate = cert,
                FieldName = "Signature-" + Guid.NewGuid().ToString("N"),
                PageIndex = 0,
                Rectangle = new XRect(120, 10, 100, 60),
                Location = "My PC",
                Reason = "Approving Rev #2",
                // Signature appearances can also consist of an image (Rectangle should be adapted to image size)
                //Image = XImage.FromFile(@"C:\Data\stamp.png")
            };

            // first signature
            //var sourceFile = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/Table-Layout.pdf")!;
            //var targetFile = Path.Combine(Path.GetTempPath(), "AA-Signed.pdf");

            // second signature
            var sourceFile = Path.Combine(Path.GetTempPath(), "AA-Signed.pdf");
            var targetFile = Path.Combine(Path.GetTempPath(), "AA-Signed-2.pdf");
            File.Copy(sourceFile, targetFile, true);

            using var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite);
            var signer = new PdfSigner(fs, options);
            var resultStream = signer.Sign();
            // overwrite input document
            fs.Seek(0, SeekOrigin.Begin);
            resultStream.CopyTo(fs);
        }
    }
}
