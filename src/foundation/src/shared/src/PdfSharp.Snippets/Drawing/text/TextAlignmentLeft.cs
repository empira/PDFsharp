// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawings
{
    public class TextAlignmentLeft : SnippetBase
    {
        // Some chars with ascenders and descenders.
        const string TestChars = " Îygp";

        public TextAlignmentLeft()
        {
            Title = "Text Alignment Left";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx, true);
                gfx.DrawString("BaseLeft" + TestChars, Font, Brush, RectCenterVLine, XStringFormats.BaseLineLeft);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("BaseLeft" + TestChars, Font, Brush, BoxCenter, XStringFormats.BaseLineLeft);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("TopLeft" + TestChars, Font, Brush, RectBox, XStringFormats.TopLeft);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("TopLeft" + TestChars, Font, Brush, BoxCenter, XStringFormats.TopLeft);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("CentLeft" + TestChars, Font, Brush, RectBox, XStringFormats.CenterLeft);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("CentLeft" + TestChars, Font, Brush, BoxCenter, XStringFormats.CenterLeft);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("BottLeft" + TestChars, Font, Brush, RectBox, XStringFormats.BottomLeft);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("BottLeft" + TestChars, Font, Brush, BoxCenter, XStringFormats.BottomLeft);
            }
            EndBox(gfx);
        }

        static readonly XFont Font = new XFont("Segoe UI", 16);
        static readonly XBrush Brush = XBrushes.Purple;

        static readonly XRect RectCenterVLine = new XRect(0, BoxHeight / 2, BoxWidth, 0);
    }
}
