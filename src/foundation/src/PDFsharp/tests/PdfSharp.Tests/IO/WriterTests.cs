// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
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
    }
}
