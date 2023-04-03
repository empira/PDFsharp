// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class PathShapes : SnippetBase
    {
        public PathShapes()
        {
            Title = "Path Shapes";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Draw rectangle via path.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddRectangle(20, 20, 184, 104);
                gfx.DrawPath(XPens.DarkBlue, XBrushes.DarkOrange, path);
            }
            EndBox(gfx);

            // Draw ellipse via path.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddEllipse(20, 20, 184, 104);
                gfx.DrawPath(XPens.DarkBlue, XBrushes.DarkOrange, path);
            }
            EndBox(gfx);

            // Draw polygon via path in winding mode.
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddPolygon(GetPentagram(52, BoxCenter));
                path.FillMode = XFillMode.Winding;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Draw polygon via path in alternate mode.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddPolygon(GetPentagram(52, BoxCenter));
                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Draw closed curve via path in winding mode.
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));

                path.AddLine(new XPoint(60, 100), new XPoint(80, 120));

                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));

                path.AddLine(new XPoint(140, 100), new XPoint(160, 120));

                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Move- Rect-Move- Line-Move- Polygon-Move- Bézier.
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));

                path.AddRectangle(60, 100, 20, 20);

                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));

                path.AddPolygon(new[] { new XPoint(140, 100), new XPoint(180, 100), new XPoint(160, 120) });

                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Line- Line-Line- Line-Line- Line-Line- Bézier.
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));
                path.AddLine(new XPoint(40, 120), new XPoint(60, 100));
                path.AddLine(new XPoint(60, 100), new XPoint(80, 120));
                path.AddLine(new XPoint(80, 120), new XPoint(100, 100));
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                path.AddLine(new XPoint(120, 120), new XPoint(140, 100));
                path.AddLine(new XPoint(140, 100), new XPoint(160, 120));
                path.AddLine(new XPoint(160, 120), new XPoint(160, 60));
                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Line- Rect-Line- Line-Line- Polygon-Line- Bézier.
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));
                path.AddLine(new XPoint(40, 120), new XPoint(60, 100));
                path.AddRectangle(60, 100, 20, 20);
                path.AddLine(new XPoint(80, 120), new XPoint(100, 100));
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                path.AddLine(new XPoint(120, 120), new XPoint(140, 100));
                path.AddPolygon(new[] { new XPoint(140, 100), new XPoint(180, 100), new XPoint(160, 120) });
                path.AddLine(new XPoint(160, 120), new XPoint(160, 60));
                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);
        }
    }
}
