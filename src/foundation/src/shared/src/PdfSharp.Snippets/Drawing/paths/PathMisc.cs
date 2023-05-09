// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class PathMisc : Snippet
    {
        public PathMisc()
        {
            Title = "Path Miscellaneous";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Path.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 120), new XPoint(80, 120));
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                path.AddArc(new XRect(100, 20, 80, 100), 0, -60);
                path.AddBezier(new XPoint(130, 20), new XPoint(100, 20), new XPoint(130, 50), new XPoint(100, 50));
                path.AddCurve(new[] { new XPoint(90, 40), new XPoint(60, 40), new XPoint(90, 10), new XPoint(60, 10) });
                path.AddRectangle(50, 50, 15, 10);
                gfx.DrawPath(XPens.Black, path);

                //var path = new XGraphicsPath();
                //path.AddLine(new XPoint(0, 0), new XPoint(100, 0));
                //path.AddArc(new XRect(50, 50, 100, 100), 0, 90);
                //gfx.DrawPath(XPens.Black, path);
            }
            EndBox(gfx);

            // Closed and filled path.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 120), new XPoint(80, 120));
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                path.AddArc(new XRect(100, 20, 80, 100), 0, -60);
                path.AddBezier(new XPoint(130, 20), new XPoint(100, 20), new XPoint(130, 50), new XPoint(100, 50));
                path.AddCurve(new[] { new XPoint(90, 40), new XPoint(60, 40), new XPoint(90, 10), new XPoint(60, 10) });

                path.CloseFigure();

                path.AddRectangle(50, 50, 15, 10);

                path.FillMode = XFillMode.Alternate;

                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Close- Line-Close- Line-Close- Line-Close- Bézier-Close-   Curve-Close-   Line.
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));
                path.CloseFigure();
                path.AddLine(new XPoint(60, 100), new XPoint(80, 120));
                path.CloseFigure();
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                path.CloseFigure();
                path.AddLine(new XPoint(140, 100), new XPoint(160, 120));
                path.CloseFigure();
                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));
                path.CloseFigure();


                path.AddCurve(new []{new XPoint(190, 50), new XPoint(170, 30), new XPoint(210, 30), new XPoint(190, 10) });
                path.CloseFigure();


                path.AddLine(new XPoint(90, 20), new XPoint(70, 40));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Close- Rect-Close- Line-Close- Polygon-Close- Bézier-Close Ellipse-Close- Curve-Close- RoundedRectangle-Close- Line.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));
                path.CloseFigure();
                path.AddRectangle(60, 100, 20, 20);
                path.CloseFigure();
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                path.CloseFigure();
                path.AddPolygon(new[] { new XPoint(140, 100), new XPoint(180, 100), new XPoint(160, 120) });
                path.CloseFigure();
                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));
                path.CloseFigure();
                path.AddEllipse(170, 60, 40, 20);
                path.CloseFigure();
                path.AddCurve(new[] { new XPoint(190, 50), new XPoint(170, 30), new XPoint(210, 30), new XPoint(190, 10) });
                path.CloseFigure();
                path.AddRoundedRectangle(110, 10, 40, 20, 8, 16);
                path.CloseFigure();
                path.AddLine(new XPoint(90, 20), new XPoint(70, 40));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Move- Line-Move- Line-Move- Line-Move- Bézier-Move-   Curve-Move-   Line.
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));

                path.AddLine(new XPoint(60, 100), new XPoint(80, 120));

                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));

                path.AddLine(new XPoint(140, 100), new XPoint(160, 120));

                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));



                path.AddCurve(new[] { new XPoint(190, 50), new XPoint(170, 30), new XPoint(210, 30), new XPoint(190, 10) });



                path.AddLine(new XPoint(90, 20), new XPoint(70, 40));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Move- Rect-Move- Line-Move- Polygon-Move- Bézier-Move Ellipse-Move- Curve-Move- RoundedRectangle-Move- Line.
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));

                path.AddRectangle(60, 100, 20, 20);

                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));

                path.AddPolygon(new[] { new XPoint(140, 100), new XPoint(180, 100), new XPoint(160, 120) });

                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));

                path.AddEllipse(170, 60, 40, 20);

                path.AddCurve(new[] { new XPoint(190, 50), new XPoint(170, 30), new XPoint(210, 30), new XPoint(190, 10) });

                path.AddRoundedRectangle(110, 10, 40, 20, 8, 16);

                path.AddLine(new XPoint(90, 20), new XPoint(70, 40));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Line- Line-Line- Line-Line- Line-Line- Bézier-Line-   Curve-Line-   Line.
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));
                { path.AddLine(new XPoint(40, 120), new XPoint(60, 100)); } // Lines that connect the elements of box 5 are in brackets.
                path.AddLine(new XPoint(60, 100), new XPoint(80, 120));
                { path.AddLine(new XPoint(80, 120), new XPoint(100, 100)); }
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                { path.AddLine(new XPoint(120, 120), new XPoint(140, 100)); }
                path.AddLine(new XPoint(140, 100), new XPoint(160, 120));
                { path.AddLine(new XPoint(160, 120), new XPoint(160, 60)); }
                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));
                { path.AddLine(new XPoint(160, 80), new XPoint(190, 50)); }


                path.AddCurve(new[] { new XPoint(190, 50), new XPoint(170, 30), new XPoint(210, 30), new XPoint(190, 10) });
                { path.AddLine(new XPoint(190, 10), new XPoint(90, 20)); }


                path.AddLine(new XPoint(90, 20), new XPoint(70, 40));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);

            // Path: Line-Line- Rect-Line- Line-Line- Polygon-Line- Bézier-Line Ellipse-Line- Curve-Line- RoundedRectangle-Line- Line.
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddLine(new XPoint(20, 100), new XPoint(40, 120));
                { path.AddLine(new XPoint(40, 120), new XPoint(60, 100)); } // Lines that connect the elements of box 6 are in brackets.
                path.AddRectangle(60, 100, 20, 20);
                { path.AddLine(new XPoint(80, 120), new XPoint(100, 100)); }
                path.AddLine(new XPoint(100, 100), new XPoint(120, 120));
                { path.AddLine(new XPoint(120, 120), new XPoint(140, 100)); }
                path.AddPolygon(new[] { new XPoint(140, 100), new XPoint(180, 100), new XPoint(160, 120) });
                { path.AddLine(new XPoint(160, 120), new XPoint(160, 60)); }
                path.AddBezier(new XPoint(160, 60), new XPoint(140, 70), new XPoint(180, 70), new XPoint(160, 80));
                { path.AddLine(new XPoint(160, 80), new XPoint(190, 80)); }
                path.AddEllipse(170, 60, 40, 20);
                { path.AddLine(new XPoint(190, 60), new XPoint(190, 50)); }
                path.AddCurve(new[] { new XPoint(190, 50), new XPoint(170, 30), new XPoint(210, 30), new XPoint(190, 10) });
                { path.AddLine(new XPoint(190, 10), new XPoint(150, 20)); }
                path.AddRoundedRectangle(110, 10, 40, 20, 8, 16);
                { path.AddLine(new XPoint(110, 20), new XPoint(90, 20)); }
                path.AddLine(new XPoint(90, 20), new XPoint(70, 40));

                path.FillMode = XFillMode.Alternate;
                gfx.DrawPath(XPens.Black, XBrushes.Gray, path);
            }
            EndBox(gfx);
        }
    }
}
