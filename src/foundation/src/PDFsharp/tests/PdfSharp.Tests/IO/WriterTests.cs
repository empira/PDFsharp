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

            var filename = PdfFileUtility.GetTempPdfFileName("ImportTest");

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(filename);
            save.Should().Throw<InvalidOperationException>();
        }
    }
}
