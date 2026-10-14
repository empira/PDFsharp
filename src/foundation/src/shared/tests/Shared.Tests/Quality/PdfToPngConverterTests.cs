// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Quality;
using PdfSharp.Quality.Ghostscript;
using Xunit;

namespace Shared.Tests.Quality
{
    public class PdfToPngConverterTests
    {
        [SkippableFact]
        public void Convert_PDF_to_PNG_Test()
        {
            var testFile = IOUtility.GetAssetsPath("archives/samples-1.5/PDFs/SomeLayout.pdf")!;
            var filename = IOUtility.GetTempFullFileName("unittests/pdfsharp/quality/PdfToPngConverterTest1", "png");

            var pdfConvert = new PdfToPngConverter(testFile, 150, true);

            Skip.If(!pdfConvert.IsInstalled());

            // Page index is 0-based.
            pdfConvert.ConvertPages(filename);

            // Test was reviewed manually.
            true.Should().BeTrue();
        }
    }
}
