// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawings
{
    public class TextAlignmentRight : Snippet
    {
        // Some chars with ascenders and descenders.
        const string TestChars = " Îygp";

        public TextAlignmentRight()
        {
            Title = "Text Alignment Right";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx, true);
                gfx.DrawString("BaseRight" + TestChars, Font, Brush, RectCenterVLine, XStringFormats.BaseLineRight);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("BaseRight" + TestChars, Font, Brush, BoxCenter, XStringFormats.BaseLineRight);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("TopRight" + TestChars, Font, Brush, RectBox, XStringFormats.TopRight);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("TopRight" + TestChars, Font, Brush, BoxCenter, XStringFormats.TopRight);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("CentRight" + TestChars, Font, Brush, RectBox, XStringFormats.CenterRight);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("CentRight" + TestChars, Font, Brush, BoxCenter, XStringFormats.CenterRight);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("BottRight" + TestChars, Font, Brush, RectBox, XStringFormats.BottomRight);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("BottRight" + TestChars, Font, Brush, BoxCenter, XStringFormats.BottomRight);
            }
            EndBox(gfx);
        }

        static readonly XFont Font = new XFont("Segoe UI", 16);
        static readonly XBrush Brush = XBrushes.Purple;

        static readonly XRect RectCenterVLine = new XRect(0, BoxHeight / 2, BoxWidth, 0);
    }
}
