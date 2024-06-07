// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class PathCurves : Snippet
    {
        public PathCurves()
        {
            Title = "Path Curves";
            PathName = "snippets/drawing/paths/Curves";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            var pen1 = new XPen(XColors.DarkGreen, 1) { DashStyle = XDashStyle.Dot };

            // Draw GDI+ style arc via path.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                DrawArtBox(gfx);

                var path = new XGraphicsPath();
                path.AddArc(20, 20, 184, 104, 20, 70);
                gfx.DrawPath(XPens.DarkBlue, path);
            }
            EndBox(gfx);

            // Draw 
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                DrawArtBox(gfx);

                var path = new XGraphicsPath();
                path.AddArc(20, 20, 184, 104, 20, 70);
                gfx.DrawPath(XPens.DarkBlue, path);
            }
            EndBox(gfx);

            // Draw SVG style arc via path.
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                DrawArtBox(gfx);

                var path = new XGraphicsPath();
                path.AddArc(new XPoint(60, 60), new XPoint(180, 120), new XSize(60, 50), 60, false, XSweepDirection.Clockwise);
                gfx.DrawPath(XPens.DarkBlue, path);
            }
            EndBox(gfx);

            // Draw polygon via path in alternate mode.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Draw closed curve via path in winding mode.
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Path: Line-Move- Rect-Move- Line-Move- Polygon-Move- Bézier.
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Path: Line-Line- Line-Line- Line-Line- Line-Line- Bézier.
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Path: Line-Line- Rect-Line- Line-Line- Polygon-Line- Bézier.
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
            }
            EndBox(gfx);
        }
    }
}
