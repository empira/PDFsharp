// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class ShapeTypes : SnippetBase
    {
        public ShapeTypes()
        {
            Title = "Shape Types";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.DarkBlue, XBrushes.Yellow, RectBig);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                gfx.DrawRoundedRectangle(XPens.DarkBlue, XBrushes.Yellow, RectBig, new XSize(40, 20));
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                gfx.DrawEllipse(XPens.DarkBlue, XBrushes.Yellow, RectBig);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                gfx.DrawPolygon(XPens.DarkBlue, XBrushes.Yellow, GetPentagram(70, BoxCenter), XFillMode.Winding);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                gfx.DrawClosedCurve(XPens.DarkBlue, XBrushes.Yellow, new[] { new XPoint(20, 20), new XPoint(40, 60), new XPoint(80, 20), new XPoint(200, 100), new XPoint(100, 80), new XPoint(40, 100) });
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                gfx.DrawPie(XPens.DarkBlue, XBrushes.Yellow, RectBig, 0, -120);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            { }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            { }
            EndBox(gfx);
        }

        static readonly XRect RectBig = new XRect(20, 20, BoxWidth - 40, BoxHeight - 40);
    }
}
