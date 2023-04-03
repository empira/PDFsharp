// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class TextMisc : SnippetBase
    {
        public TextMisc()
        {
            Title = "Text Miscellaneous";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Text Styles (resulting in own Type Faces).
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                const string facename = "Segoe UI";
                var options = new XPdfFontOptions(PdfFontEncoding.Unicode);

                var fontRegular = new XFont(facename, 20, XFontStyleEx.Regular, options);
                var fontBold = new XFont(facename, 20, XFontStyleEx.Bold, options);

                gfx.DrawString(String.Format("{0} (regular)", facename), fontRegular, XBrushes.DarkSlateGray, 0, 75);
                gfx.DrawString(String.Format("{0} (bold)", facename), fontBold, XBrushes.DarkSlateGray, 0, 112.5);
            }
            EndBox(gfx);

            // Type Faces.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                const string facename = "Segoe UI";
                var options = new XPdfFontOptions(PdfFontEncoding.Unicode);

                // TODO: Define if XFontStyleEx.Bold should have an effect for bold fonts. For Segoe UI Semibold and Black it currently (15-12-01) has an effect in CORE and GDI, but not in WPF. $MaOs

                var fontLight = new XFont(String.Format("{0} light", facename), 20, XFontStyleEx.Regular, options);
                var fontSemilight = new XFont(String.Format("{0} semilight", facename), 20, XFontStyleEx.Regular, options);
                var fontRegular = new XFont(String.Format("{0}", facename), 20, XFontStyleEx.Regular, options);
                var fontSemibold = new XFont(String.Format("{0} semibold", facename), 20, XFontStyleEx.Bold, options);
                var fontBlack = new XFont(String.Format("{0} black", facename), 20, XFontStyleEx.Bold, options);

                // The default alignment is baseline left (that differs from GDI+)
                gfx.DrawString(String.Format("{0} light", facename), fontLight, XBrushes.DarkSlateGray, 0, 25);
                gfx.DrawString(String.Format("{0} semilight", facename), fontSemilight, XBrushes.DarkSlateGray, 0, 50);
                gfx.DrawString(String.Format("{0}", facename), fontRegular, XBrushes.DarkSlateGray, 0, 75);
                gfx.DrawString(String.Format("{0} semibold", facename), fontSemibold, XBrushes.DarkSlateGray, 0, 100);
                gfx.DrawString(String.Format("{0} black", facename), fontBlack, XBrushes.DarkSlateGray, 0, 125);
            }
            EndBox(gfx);

            // Text Styles (not resulting own Type Faces).
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                // TODO: Include bold for FontResolver without own Type Face for.

                const string facename = "Segoe UI";
                var options = new XPdfFontOptions(PdfFontEncoding.Unicode);

                var fontRegular = new XFont(facename, 20, XFontStyleEx.Regular, options);
                var fontItalic = new XFont(facename, 20, XFontStyleEx.Italic, options);
                var fontUnderline = new XFont(facename, 20, XFontStyleEx.Underline, options);
                var fontStrikeout = new XFont(facename, 20, XFontStyleEx.Strikeout, options);
                var fontIUS = new XFont(facename, 20, XFontStyleEx.Italic | XFontStyleEx.Underline | XFontStyleEx.Strikeout, options);

                gfx.DrawString(String.Format("{0} (regular)", facename), fontRegular, XBrushes.DarkSlateGray, 0, 25);
                gfx.DrawString(String.Format("{0} (italic)", facename), fontItalic, XBrushes.DarkSlateGray, 0, 50);
                gfx.DrawString(String.Format("{0} (underline)", facename), fontUnderline, XBrushes.DarkSlateGray, 0, 75);
                gfx.DrawString(String.Format("{0} (strokeout)", facename), fontStrikeout, XBrushes.DarkSlateGray, 0, 100);
                gfx.DrawString(String.Format("{0} (ita, und, str)", facename), fontIUS, XBrushes.DarkSlateGray, 0, 125);
            }
            EndBox(gfx);

            // Measure Text / font.GetHeight / GetCellAscent / GetCellDescent.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                // Set font encoding to Unicode.
                var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
                const XFontStyleEx style = XFontStyleEx.Regular;
                var font = new XFont("Segoe UI", 80, style, options);

                const string text = "Îygpq";
                const double x = 20, y = 100;
                var size = gfx.MeasureString(text, font);

                //var lineSpace = font.GetHeight(gfx);
                var lineSpace = font.GetHeight();
                var cellSpace = font.FontFamily.GetLineSpacing(style);
                var cellAscent = font.FontFamily.GetCellAscent(style);
                var cellDescent = font.FontFamily.GetCellDescent(style);
                var cellLeading = cellSpace - cellAscent - cellDescent;

                // Get effective ascent.
                var ascent = lineSpace * cellAscent / cellSpace;
                gfx.DrawRectangle(XBrushes.Bisque, x, y - ascent, size.Width, ascent);

                // Get effective descent.
                var descent = lineSpace * cellDescent / cellSpace;
                gfx.DrawRectangle(XBrushes.LightGreen, x, y, size.Width, descent);

                // Get effective leading.
                var leading = lineSpace * cellLeading / cellSpace;
                gfx.DrawRectangle(XBrushes.Yellow, x, y + descent, size.Width, leading);

                // Draw text half transparent.
                var color = XColors.DarkSlateBlue;
                color.A = 0.6;
                gfx.DrawString(text, font, new XSolidBrush(color), x, y);
            }
            EndBox(gfx);

            // Different glyphs.
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                // Set font encoding to Unicode.
                var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
                const XFontStyleEx style = XFontStyleEx.Regular;
                var font = new XFont("Segoe UI", 16, style, options);

                gfx.DrawString("äöüßÄÖÜéèêÉÈÊ€", font, XBrushes.Black, 0, 25);
                gfx.DrawString("ÆĈƉəΘЖԄᴓᶌḙἒ‼₫℗⅜←∙√○Ⱨ", font, XBrushes.Black, 0, 50);
                gfx.DrawString("aːa˓aˉaʿa˘ãāa͜aa͕a︢a", font, XBrushes.Black, 0, 75);
            }
            EndBox(gfx);
        }
    }
}
