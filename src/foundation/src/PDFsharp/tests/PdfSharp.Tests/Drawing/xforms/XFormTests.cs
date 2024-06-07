// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Drawing
{
    [Collection("PDFsharp")]
    public class XFormTests
    {
        [Fact]
        public void Read_PDF_file()
        {
            var pdf1 = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/Table-Layout.pdf")!;
            var pdf2 = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/GDI 1.31/Table-Layout.pdf")!;

            var doc1 = XPdfForm.FromFile(pdf1);
            var num1 = doc1.PageCount;

            var doc2 = XPdfForm.FromFile(pdf2);
            var num2 = doc2.PageCount;

            num1.Should().Be(num2);
        }
    }
}
