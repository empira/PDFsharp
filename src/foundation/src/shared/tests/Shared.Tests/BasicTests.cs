// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Quality.Testing;
using Xunit;

namespace Shared.Tests
{
    public class BasicTests : PdfSharpTestBase
    {
        readonly string _tempRoot = GetTempRoot(typeof(BasicTests));

        public BasicTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        protected override void Dispose(bool disposing)
        {
            PdfSharpCore.ResetAll();
        }

        [Fact(/*Skip = "Just a placeholder"*/)]
        public void Create_Hello_World_BasicTests()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            // Draw two lines with a red default pen.
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            // Create a font.
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);

            //var font2 = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);

#if NET8_0_OR_GREATER
            gfx.DrawString($"Hello, dotnet {Environment.Version.Major}.{Environment.Version.Minor}!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#else
            gfx.DrawString("Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);
#endif

            string filename = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + nameof(Create_Hello_World_BasicTests));
            document.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
