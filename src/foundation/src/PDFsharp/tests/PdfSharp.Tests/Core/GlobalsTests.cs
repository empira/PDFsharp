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

namespace PdfSharp.Tests.Globals
{
    [Collection("PDFsharp")]
    public class GlobalsTests
    {
        [Fact]
        public void ResetAll_Test()
        {
            PdfSharpCore.ResetAll();
            try
            {
                GlobalFontSettings.FontResolver = new FailsafeFontResolver();

                var doc1 = new PdfDocument();
                var page1 = doc1.AddPage();
                var gfx1 = XGraphics.FromPdfPage(page1);
                var font1 = new XFont("Arial", 10);
                gfx1.DrawString("Hallo", font1, XBrushes.Black, new XPoint(100, 100));

                PdfSharpCore.ResetAll();

                var doc2 = new PdfDocument();
                var page2 = doc1.AddPage();
                var gfx2 = XGraphics.FromPdfPage(page2);
                //var font2 = new XFont("Arial", 10);

                Action draw = () => gfx2.DrawString("Hello", font1, XBrushes.Black, new XPoint(100, 100));

                draw.Should().Throw<InvalidOperationException>("abcd");
            }
            finally
            {
                PdfSharpCore.ResetAll();
            }
        }
    }
}
