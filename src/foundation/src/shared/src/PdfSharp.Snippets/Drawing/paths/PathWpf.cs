// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Windows;
#if WPF
using System.Windows.Media;
using SysPoint = System.Windows.Point;
#endif
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class PathWpf : Snippet
    {
        public PathWpf()
        {
            Title = "Path WPF";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Draw rectangle via path.
            BeginBox(gfx, 1, BoxOptions.Tile, "Core");
            {
                var path = new XGraphicsPath();
                path.AddRectangle(20, 20, 184, 104);
                gfx.DrawPath(XPens.DarkBlue, XBrushes.DarkOrange, path);
            }
            EndBox(gfx);

            // Draw rectangle via path.
            BeginBox(gfx, 2, BoxOptions.Tile, "WPF PathGeometry");
            {
#if WPF
                var geometry = new PathGeometry();
                geometry.Figures.Add(new(new(20, 20),
                    [ new LineSegment(new (204, 20), true),
                      new LineSegment(new (204, 124), true),
                      new LineSegment(new (20, 124), true)
                    ], true));

                var path = new XGraphicsPath();
                path.AddGeometry(geometry);
                gfx.DrawPath(XPens.DarkBlue, XBrushes.DarkOrange, path);
#endif
            }
            EndBox(gfx);

            // Draw pentagram via path.
            BeginBox(gfx, 3, BoxOptions.Tile, "Core");
            {
                var path = new XGraphicsPath();
                path.AddPolygon(GetPentagram(52, BoxCenter));
                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Draw pentagram via path.
            BeginBox(gfx, 4, BoxOptions.Tile, "WPF PathGeometry");
            {
#if WPF
                var points = GetPentagram(52, BoxCenter);
                List<LineSegment> segments = new();
                for (var idx = 1; idx < points.Length; idx++)
                    segments.Add(new(new(points[idx].X, points[idx].Y), true));

                var geometry = new PathGeometry();
                geometry.Figures.Add(new(new(points[0].X, points[0].Y), segments, true));

                var path = new XGraphicsPath();
                path.AddGeometry(geometry);
                gfx.DrawPath(XPens.DarkBlue, XBrushes.DarkOrange, path);
#endif
            }
            EndBox(gfx);

            // 
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // 
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            //
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            //
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
            }
            EndBox(gfx);
        }
    }
}
