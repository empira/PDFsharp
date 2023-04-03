// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class LineTestsNew : SnippetBase
    {
        public LineTestsNew()
        {
            Title = "Line Types";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                gfx.DrawLine(XPens.Black, new XPoint(60, 20), new XPoint(160, 120));
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                gfx.DrawLines(XPens.Black, new[] { new XPoint(60, 20), new XPoint(160, 120), new XPoint(160, 20) });
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                gfx.DrawBezier(XPens.Black, new XPoint(60, 20), new XPoint(60, 120), new XPoint(160, 20), new XPoint(160, 120));
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                gfx.DrawBeziers(XPens.Black, new []{new XPoint(160, 20), new XPoint(310, -30), new XPoint(-90, 170), new XPoint(60, 120), new XPoint(20, 120), new XPoint(60, 20), new XPoint(20, 20)});
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                gfx.DrawCurve(XPens.Black, new[] { new XPoint(60, 20), new XPoint(60, 120), new XPoint(160, 20), new XPoint(160, 120) });
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                gfx.DrawArc(XPens.Black, new XRect(new XPoint(20, 20), new XPoint(200, 120)), 0, 135);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            { }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            { }
            EndBox(gfx);
        }
    }
}
