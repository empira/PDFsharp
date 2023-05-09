// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class ColorsRgb : Snippet
    {
        public ColorsRgb()
        {
            Title = "RGB Colors";
        }

        public XRect GetDeciRect(int i)
        {
            var left = i * BoxWidth / 10;
            var right = (i + 1) * BoxWidth / 10;
            return new XRect(left, 0, right - left, BoxHeight);
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // White.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 255, 255, 255)), rect); 
                }
            }
            EndBox(gfx);

            // Yellow.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 255, 255, 0)), rect);
                }
            }
            EndBox(gfx);

            // Cyan.
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 0, 255, 255)), rect);
                }
            }
            EndBox(gfx);

            // Green.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 0, 255, 0)), rect);
                }
            }
            EndBox(gfx);

            // Violet.
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 255, 0, 255)), rect);
                }
            }
            EndBox(gfx);

            // Red.
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 255, 0, 0)), rect);
                }
            }
            EndBox(gfx);

            // Blue.
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 0, 0, 255)), rect);
                }
            }
            EndBox(gfx);

            // Black.
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.Black, 1), 0, 0, BoxWidth, BoxHeight);
                for (int i = 0; i < 10; ++i)
                {
                    var rect = GetDeciRect(i);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(i * 255 / 9, 0, 0, 0)), rect);
                }
            }
            EndBox(gfx);
        }
    }
}
