// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Create_Hello_World_BasicTests()
        {
            GlobalFontSettings.FontResolver ??= SnippetsFontResolver.Get();

            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            // Draw two lines with a red default pen.
            var width = page.Width;
            var height = page.Height;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            bool snippetsFontResolver = GlobalFontSettings.FontResolver!.GetType()!.FullName!.EndsWith("SnippetsFontResolver");

            // Create a font.
            var font = new XFont(snippetsFontResolver ? "Times New Roman" : "segoe wp", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
            gfx.DrawString("Hello, dotnet 6.0!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("HelloWorld");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
