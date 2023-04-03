// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class TextAlignmentCenter : SnippetBase
    {
        // Some chars with ascenders and descenders.
        const string TestChars = " Îygp";

        public TextAlignmentCenter()
        {
            Title = "Text Alignment Center";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx, true);
                gfx.DrawString("BaseCent" + TestChars, Font, Brush, RectCenterVLine, XStringFormats.BaseLineCenter);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("BaseCent" + TestChars, Font, Brush, BoxCenter, XStringFormats.BaseLineCenter);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("TopCent" + TestChars, Font, Brush, RectBox, XStringFormats.TopCenter);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("TopCent" + TestChars, Font, Brush, BoxCenter, XStringFormats.TopCenter);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("CentCent" + TestChars, Font, Brush, RectBox, XStringFormats.Center);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("CentCent" + TestChars, Font, Brush, BoxCenter, XStringFormats.Center);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                gfx.DrawString("BottCent" + TestChars, Font, Brush, RectBox, XStringFormats.BottomCenter);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                DrawAlignmentGrid(gfx);
                DrawCenterPoint(gfx);
                gfx.DrawString("BottCenter" + TestChars, Font, Brush, BoxCenter, XStringFormats.BottomCenter);
            }
            EndBox(gfx);
        }

        static readonly XFont Font = new XFont(FontNameStd, 16);
        static readonly XBrush Brush = XBrushes.Purple;

        static readonly XRect RectCenterVLine = new XRect(0, BoxHeight / 2, BoxWidth, 0);
    }
}
