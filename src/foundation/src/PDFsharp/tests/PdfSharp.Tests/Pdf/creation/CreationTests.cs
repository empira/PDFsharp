// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests
{
    [Collection("PDFsharp")]
    public class CreationTests
    {
        [Fact]
        public void Create_for_Stream()
        {
            var filename = PdfFileUtility.GetTempPdfFullFileName("unittests/pdfsharp/PDF/creation/CreationTest");

            using (var outputStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                PdfDocument document = new PdfDocument(outputStream);
                document.AddPage();
                document.Close();
                document.Version.Should().BeGreaterThan(0);
            }
        }
    }
}
