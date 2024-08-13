// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class LineStyle : Snippet
    {
        public LineStyle()
        {
            Title = "Line Style";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                gfx.DrawLines(new XPen(XColors.DarkBlue, 1), 18, 18, 204, 18, 100, 100);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Box);
            { }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                var pen = new XPen(XColors.DarkBlue, 3);

                pen.DashStyle = XDashStyle.Dash;
                gfx.DrawLine(pen, 18, 18, 204, 18);

                pen.DashStyle = XDashStyle.Dot;
                gfx.DrawLine(pen, 18, 50, 204, 50);

                pen.DashStyle = XDashStyle.DashDot;
                gfx.DrawLine(pen, 18, 82, 204, 82);

                pen.DashStyle = XDashStyle.DashDotDot;
                gfx.DrawLine(pen, 18, 114, 204, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Box);
            { }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                var pen = new XPen(XColors.DarkBlue, 3);

                // Custom pattern
                //pen.DashStyle = XDashStyle.Custom;
                pen.DashPattern = [3, 1, 2.5, 1.5];
                pen.Width = 7;

                pen.DashOffset = 1;
                gfx.DrawLine(pen, 18, 18, 204, 18);

                pen.DashOffset = 2;
                gfx.DrawLine(pen, 18, 50, 204, 50);

                pen.DashOffset = 4;
                gfx.DrawLine(pen, 18, 82, 204, 82);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Box);
            { }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Fill);
            { }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Fill);
            { }
            EndBox(gfx);
        }
    }
}
