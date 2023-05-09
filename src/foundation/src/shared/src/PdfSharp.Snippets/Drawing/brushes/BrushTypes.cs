// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawings
{
    public class BrushTypes : Snippet
    {
        public BrushTypes()
        {
            Title = "Brush Types";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // SolidBrush.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                var pen = XPens.Black;
                var brush = XBrushes.Blue;
                gfx.DrawRectangle(pen, brush, RectBig);
            }
            EndBox(gfx);

            // LinearGradientBrush with two points.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                var pen = XPens.Black;
                var hOffset = 100;
                var point1 = new XPoint(40 + hOffset, 40);
                var point2 = new XPoint(70 + hOffset, 90);
                //var point1 = new XPoint(RectSmall.X, RectSmall.Y);
                //var point2 = new XPoint(RectSmall.X + RectSmall.Width, RectSmall.Y + RectSmall.Height);
                var brush = new XLinearGradientBrush(point1, point2, XColors.Red, XColors.Blue);
                gfx.DrawRectangle(pen, brush, RectBig);

                gfx.DrawLine(XPens.Lime, point1, point2);
            }
            EndBox(gfx);

            // LinearGradientBrush with bounding rect and direction.
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                var pen = XPens.Black;
                var brush = new XLinearGradientBrush(RectSmall, XColors.Red, XColors.Blue, XLinearGradientMode.ForwardDiagonal);
                gfx.DrawRectangle(pen, brush, RectBig);

                gfx.DrawRectangle(XPens.Lime, RectSmall);
            }
            EndBox(gfx);

            // LinearGradientBrush with bounding rect and direction.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                var pen = XPens.Black;
                var brush = new XLinearGradientBrush(RectSmall, XColors.Red, XColors.Blue, XLinearGradientMode.BackwardDiagonal);
                gfx.DrawRectangle(pen, brush, RectBig);

                gfx.DrawRectangle(XPens.Lime, RectSmall);
            }
            EndBox(gfx);

            // LinearGradientBrush with bounding rect and direction.
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                var pen = XPens.Black;
                var brush = new XLinearGradientBrush(RectSmall, XColors.Red, XColors.Blue, XLinearGradientMode.Horizontal);
                gfx.DrawRectangle(pen, brush, RectBig);

                gfx.DrawRectangle(XPens.Lime, RectSmall);
            }
            EndBox(gfx);

            // LinearGradientBrush with bounding rect and direction.
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                var pen = XPens.Black;
                var brush = new XLinearGradientBrush(RectSmall, XColors.Red, XColors.Blue, XLinearGradientMode.Vertical);
                gfx.DrawRectangle(pen, brush, RectBig);

                gfx.DrawRectangle(XPens.Lime, RectSmall);
            }
            EndBox(gfx);

            // LinearGradientBrush with two points in rotated box.
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                gfx.RotateAtTransform(15, BoxCenter);

                var pen = XPens.Black;
                var hOffset = 100;
                var point1 = new XPoint(40 + hOffset, 40);
                var point2 = new XPoint(70 + hOffset, 90);
                //var point1 = new XPoint(RectSmall.X, RectSmall.Y);
                //var point2 = new XPoint(RectSmall.X + RectSmall.Width, RectSmall.Y + RectSmall.Height);
                var brush = new XLinearGradientBrush(point1, point2, XColors.Red, XColors.Blue);
                gfx.DrawRectangle(pen, brush, RectBig);

                gfx.DrawLine(XPens.Lime, point1, point2);
            }
            EndBox(gfx);

            // LinearGradientBrush with bounding rect and direction in rotated box.
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                gfx.RotateAtTransform(15, BoxCenter);

                var pen = XPens.Black;
                var brush = new XLinearGradientBrush(RectSmall, XColors.Red, XColors.Blue, XLinearGradientMode.BackwardDiagonal);
                gfx.DrawRectangle(pen, brush, RectBig);

                gfx.DrawRectangle(XPens.Lime, RectSmall);
            }
            EndBox(gfx);
        }

        static readonly XRect RectBig = new XRect(20, 20, BoxWidth - 40, BoxHeight - 40);
        static readonly XRect RectSmall = new XRect(50, 50, BoxWidth - 100, BoxHeight - 100);
    }
}
