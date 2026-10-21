// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
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

            var filename = PdfFileUtility.GetTempPdfFullFileName("unittests/pdfsharp/IO/ImportTest");

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(filename);
            save.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Write_rectangle_with_the_precision_of_a_real_number()
        {
            // ISO A4 in points. The values have four decimal places, which must survive a round-trip,
            // because a changed /MediaBox is a changed page for every digital signature of the document.
            const double width = 595.2756, height = 841.8898;

            using var stream = new MemoryStream();
            using (var document = new PdfDocument())
            {
                var page = document.AddPage();
                page.MediaBox = new PdfRectangle(new XPoint(0, 0), new XPoint(width, height));
                document.Save(stream, false);
            }

            var pdf = stream.ToArray();
            var chars = new char[pdf.Length];
            for (int idx = 0; idx < pdf.Length; idx++)
                chars[idx] = (char)pdf[idx];
            new String(chars).Should().Contain("[0 0 595.2756 841.8898]");

            using var writtenDocument = PdfReader.Open(new MemoryStream(pdf), PdfDocumentOpenMode.Import);
            var mediaBox = writtenDocument.Pages[0].MediaBox;
            mediaBox.X2.Should().Be(width);
            mediaBox.Y2.Should().Be(height);
        }
    }
}
