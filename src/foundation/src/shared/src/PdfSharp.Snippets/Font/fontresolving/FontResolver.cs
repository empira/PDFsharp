// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Internal;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Font
{
    public class SegoeWpFontResolverSnippet : Snippet
    {
        public SegoeWpFontResolverSnippet()
        {
            // Ponder about this...
            PdfSharpCore.ResetFontManagement();
            GlobalFontSettings.FontResolver = new SegoeWpFontResolver();

            NoText = true;
            Title = "SegoeWpFontResolverSnippet";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            //// TODO_OLD: implement this.
            ////XFont.PdfFontDefaultOptions = XPdfFontOptions.UnicodeDefault;

            //// Create a new PDF document.
            //PdfDocument document = CreateNewPdfDocument();

            //// Create an empty page.
            //PdfPage page = document.AddPage();

            //// Get an XGraphics object for drawing.
            //XGraphics gfx = XGraphics.FromPdfPage(page);

            //XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            XPdfFontOptions pdfOptions = XPdfFontOptions.WinAnsiDefault;
            //options = XPdfFontOptions.WinAnsiDefault;

            // Create a font.
            const double fontSize = 16;
#if false
            XFont times = new XFont("Times New Roman", fontSize, XFontStyleEx.Regular, pdfOptions);
#endif
            //var wpLightFont = new XFont("Segoe WP Light", fontSize, XFontStyleEx.Regular, pdfOptions);
            var wpLightFont = new XFont("Times New Roman", fontSize, XFontStyleEx.Regular, pdfOptions);
            var wpSemilightFont = new XFont("Segoe WP Semilight", fontSize, XFontStyleEx.Regular, pdfOptions);
            //var wpRegularFont = new XFont("Segoe WP", fontSize, XFontStyleEx.Regular, pdfOptions);
            //var wpSemiboldFont = new XFont("Segoe WP Semibold", fontSize, XFontStyleEx.Regular, pdfOptions);
            //var wpBoldFont = new XFont("Segoe WP", fontSize, XFontStyleEx.Bold, pdfOptions);
            //var wpBlackFont = new XFont("Segoe WP Black", fontSize, XFontStyleEx.Regular, pdfOptions);

            //var wpLightItalicFont = new XFont("Segoe WP Light", fontSize, XFontStyleEx.Italic, pdfOptions);
            //var wpLightBoldFont = new XFont("Segoe WP Light", fontSize, XFontStyleEx.Bold, pdfOptions);
            //// We need FontsDevHelper to force a style simulation. This is for development and testing only.
            //var wpLightBoldSimulatedFont = FontsDevHelper.CreateSpecialFont("Segoe WP Light", fontSize, XFontStyleEx.Regular, pdfOptions, XStyleSimulations.BoldSimulation);
            //var wpLightBoldItalicFont = new XFont("Segoe WP Light", fontSize, XFontStyleEx.BoldItalic, pdfOptions);
            //var wpLightBoldSimulatedItalicSimulatedFont = FontsDevHelper.CreateSpecialFont("Segoe WP Light", fontSize, XFontStyleEx.Regular, pdfOptions, XStyleSimulations.BoldItalicSimulation);
            //var wpBlackItalicFont = new XFont("Segoe WP Black", fontSize, XFontStyleEx.BoldItalic, pdfOptions);

            // Draw the text.
            string text = "Sphinx ";
            //string text = Pangrams.TwelveBoxer; // WP fonts contains German umlauts and Eszett.
            double x = 40;
            double y = 50;
            double dy = 35;
            gfx.DrawString(text + "(Segoe WP Light - regular) - ϰ", wpLightFont, XBrushes.Black, x, y);
            y += dy;

            gfx.DrawString(text + "(Segoe WP Semilight - regular)", wpSemilightFont, XBrushes.Black, x, y);
            y += dy;

            //gfx.DrawString(text + "(Segoe WP - regular)", wpRegularFont, XBrushes.Black, x, y);
            //y += dy;

            //gfx.DrawString(text + "(Segoe WP Semibold - regular)", wpSemiboldFont, XBrushes.Black, x, y);
            //y += dy;

            //gfx.DrawString(text + "(Segoe WP - bold)", wpBoldFont, XBrushes.Black, x, y);
            //y += dy;

            //gfx.DrawString(text + "(Segoe WP Black - regular)", wpBlackFont, XBrushes.Black, x, y);
            //y += 2 * dy;

            //gfx.DrawString(text + "(Segoe WP Light - with italic simulated)", wpLightItalicFont, XBrushes.Black, x, y);
            //y += dy;

            ///gfx.DrawString(text + "(Segoe WP Light - bold)", wpLightBoldFont, XBrushes.Black, x, y);
            //y += dy;

            //gfx.DrawString(text + "(Segoe WP Light - bold simulated)", wpLightBoldSimulatedFont, XBrushes.Black, x, y);
            //y += dy;

            //gfx.DrawString(text + "(Segoe WP Light - bold with italic simulated)", wpLightBoldItalicFont, XBrushes.Black, x, y);
            //y += dy;

            //gfx.DrawString(text + "(Segoe WP Light - with bold and italic simulated)", wpLightBoldSimulatedItalicSimulatedFont, XBrushes.Black, x, y);
            //y += dy;

            //gfx.DrawString(text + "(Segoe WP Black - with bold and italic simulated)", wpBlackItalicFont, XBrushes.Black, x, y);
            //y += dy;
        }
    }
}
