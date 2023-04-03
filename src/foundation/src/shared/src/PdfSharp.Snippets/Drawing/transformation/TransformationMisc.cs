// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class TransformationMisc : SnippetBase
    {
        public TransformationMisc()
        {
            Title = "Transformation Miscellaneous";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Make a demo rect, width is less than box height, height is 2/3 of width. This rect can be rotated within the box.
            var demoRect = new XRect(20 + (BoxWidth - BoxHeight) / 2, 20 + BoxHeight / 6, BoxHeight - 40, BoxHeight * 2 / 3 - 40);
            // A semi-transparent brush.
            var demoBrush = new XSolidBrush(XColor.FromArgb(128, 255, 255, 0));

            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                gfx.RotateAtTransform(30, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                gfx.RotateAtTransform(45, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                gfx.RotateAtTransform(60, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                gfx.RotateAtTransform(60, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoRect);
                gfx.ScaleAtTransform(0.75, 0.25, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                // shearX (5) is the horizontal skew, shearY (-25) is the vertical skew.
                // http://msdn.microsoft.com/en-us/library/system.windows.media.skewtransform.anglex(v=vs.110).aspx
                gfx.SkewAtTransform(5, -25, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                gfx.RotateAtTransform(30, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoRect);
                gfx.SkewAtTransform(5, -25, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                gfx.SkewAtTransform(5, -25, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoRect);
                gfx.RotateAtTransform(30, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoRect);
                gfx.SkewAtTransform(10, 0, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoRect);
                gfx.SkewAtTransform(0, -10, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoRect);
            }
            EndBox(gfx);
        }

        public void RenderPage2(XGraphics gfx)
        {
            // Make a demo rect, width is less than box height, height is 2/3 of width. This rect can be rotated within the box.
            var demoRect = new XRect(20 + (BoxWidth - BoxHeight) / 2, 20 + BoxHeight / 6, BoxHeight - 40, BoxHeight * 2 / 3 - 40);
            var demoSquare = new XRect(20 + (BoxWidth - BoxHeight * 2 / 3) / 2, 20 + BoxHeight / 6, BoxHeight * 2 / 3 - 40, BoxHeight * 2 / 3 - 40);
            // A semi-transparent brush.
            var demoBrush = new XSolidBrush(XColor.FromArgb(128, 255, 255, 0));

            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                // For comparison with MS WPF sample.
                // http://msdn.microsoft.com/en-us/library/system.windows.media.skewtransform.anglex(v=vs.110).aspx
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.SkewAtTransform(45, 0, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                // For comparison with MS WPF sample.
                // http://msdn.microsoft.com/en-us/library/system.windows.media.skewtransform.anglex(v=vs.110).aspx
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.SkewAtTransform(0, 45, BoxCenter);
                gfx.DrawRectangle(XPens.DarkBlue, demoBrush, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
            }
            EndBox(gfx);
        }
    }
}
