// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class SymbolFonts : Snippet
    {
        public SymbolFonts()
        {
            Title = "Symbol fonts";
            PathName = "snippets/drawing/text/" + nameof(SymbolFonts);
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            if (!Capabilities.OperatingSystem.IsWindows)
                return;
            //Cleanroom = true;

#if !GDI && !WPF
            var fontSourceSymbol = XFontSource.CreateFromFile("c:/Windows/Fonts/symbol.ttf");
            var glyphTypefaceSymbol = new XGlyphTypeface(fontSourceSymbol);
            var fontSymbol = new XFont(glyphTypefaceSymbol, 10);

            var fontSourceWindings = XFontSource.CreateFromFile("c:/Windows/Fonts/wingding.ttf");
            var glyphTypefaceWindings = new XGlyphTypeface(fontSourceWindings);
            var fontWindings = new XFont(glyphTypefaceWindings, 8);

            var fontSourceWindings2 = XFontSource.CreateFromFile("c:/Windows/Fonts/WINGDNG2.TTF");
            var glyphTypefaceWindings2 = new XGlyphTypeface(fontSourceWindings2);
            var fontWindings2 = new XFont(glyphTypefaceWindings2, 8);

            var fontSourceWindings3 = XFontSource.CreateFromFile("c:/Windows/Fonts/WINGDNG3.TTF");
            var glyphTypefaceWindings3 = new XGlyphTypeface(fontSourceWindings3);
            var fontWindings3 = new XFont(glyphTypefaceWindings3, 8);
#else
            //var fontSourceSymbol = XFontSource.CreateFromFile("c:/Windows/Fonts/symbol.ttf");
            //var glyphTypefaceSymbol = new XGlyphTypeface(fontSourceSymbol);
            var fontSymbol = new XFont("Symbol", 10);

            //var fontSourceWindings = XFontSource.CreateFromFile("c:/Windows/Fonts/wingding.ttf");
            //var glyphTypefaceWindings = new XGlyphTypeface(fontSourceWindings);
            var fontWindings = new XFont("Wingdings", 8);

            //var fontSourceWindings2 = XFontSource.CreateFromFile("c:/Windows/Fonts/WINGDNG2.TTF");
            //var glyphTypefaceWindings2 = new XGlyphTypeface(fontSourceWindings2);
            var fontWindings2 = new XFont("Wingdings 2", 8);

            //var fontSourceWindings3 = XFontSource.CreateFromFile("c:/Windows/Fonts/WINGDNG3.TTF");
            //var glyphTypefaceWindings3 = new XGlyphTypeface(fontSourceWindings3);
            var fontWindings3 = new XFont("Wingdings 3", 8);
#endif

            BeginBox(gfx, 1, BoxOptions.Tile, "Symbol");
            {
                // Draw glyphs of psi, clubs, diamonds, hearts, and spades.

                // ...with the correct symbol font code points. Works, but issues error log because values are larger than 255.
                gfx.DrawString("\uf079\uf0a7\uf0a8\uf0a9\uf0aa", fontSymbol, XBrushes.DarkSlateGray, 10, 30);
                // ...with the 8-bit symbol font code points.
                gfx.DrawString("y\u00A7\u00A8\u00A9\u00AA", fontSymbol, XBrushes.DarkSlateGray, 10, 50);
                var size = gfx.MeasureString("y\u00A7\u00A8\u00A9\u00AA", fontSymbol);
                gfx.DrawRectangle(XPens.Blue, 10, 50, size.Width, -size.Height);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Fill, "All Symbols glyphs");
            {
                double x = 10;
                double y = 10;
                double dx = 2;
                double dy = 18;
                for (int idx = 32; idx < 255; idx++)
                {
                    if (idx == 127)
                        idx += 34;
                    if (x + 15 > BoxWidth)
                    {
                        x = 10;
                        y += dy;
                    }
                    var s = ((char)idx).ToString();
                    gfx.DrawString(s, fontSymbol, XBrushes.DarkBlue, x, y);
                    x += gfx.MeasureString(s, fontSymbol).Width + dx;
                }
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile, "Windings");
            {
                // Draw glyphs of 0 to 9 in a circls.
                var text = "\u008b\u008c\u008d\u008e\u008f\u0090\u0091\u0092\u0093\u0094";
                gfx.DrawString(text, fontWindings, XBrushes.DarkSlateGray, 10, 30);
                gfx.DrawMeasureBox(text, fontWindings, new(10, 30));
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Fill, "All Windings glyphs");
            {
                double x = 10;
                double y = 15;
                double dx = 1;
                double dy = 15;
                for (int idx = 32; idx < 255; idx++)
                {
                    //if (idx == 127)
                    //    idx += 34;
                    if (x + 15 > BoxWidth)
                    {
                        x = 10;
                        y += dy;
                    }
                    var s = ((char)idx).ToString();
                    gfx.DrawString(s, fontWindings, XBrushes.DarkBlue, x, y);
                    x += gfx.MeasureString(s, fontWindings).Width + dx;
                }
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Fill, "All Windings2 glyphs");
            {
                double x = 10;
                double y = 15;
                double dx = 1;
                double dy = 15;
                for (int idx = 32; idx < 250; idx++)
                {
                    //if (idx == 127)
                    //    idx += 34;
                    if (x + 15 > BoxWidth)
                    {
                        x = 10;
                        y += dy;
                    }
                    var s = ((char)idx).ToString();
                    gfx.DrawString(s, fontWindings2, XBrushes.DarkBlue, x, y);
                    x += gfx.MeasureString(s, fontWindings2).Width + dx;
                }
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Fill, "All Windings3 glyphs");
            {
                double x = 10;
                double y = 15;
                double dx = 1;
                double dy = 15;
                for (int idx = 32; idx <= 240; idx++)
                {
                    //if (idx == 127)
                    //    idx += 34;
                    if (x + 15 > BoxWidth)
                    {
                        x = 10;
                        y += dy;
                    }
                    var s = ((char)idx).ToString();
                    gfx.DrawString(s, fontWindings3, XBrushes.DarkBlue, x, y);
                    x += gfx.MeasureString(s, fontWindings3).Width + dx;
                }
            }
            EndBox(gfx);
        }
    }
}
