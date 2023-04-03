// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class TransformationXImages : SnippetBase
    {
        public TransformationXImages()
        {
            Title = "Transformation of XImages";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            var demoSquare = new XRect(20 + (BoxWidth - BoxHeight * 2 / 3) / 2, 20 + BoxHeight / 6, BoxHeight * 2 / 3 - 40, BoxHeight * 2 / 3 - 40);

            var image = ImageHelper.GetJpegImage(ImageHelper.JpegImages.TruecolorA);

            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.RotateAtTransform(30, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.RotateAtTransform(45, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.RotateAtTransform(60, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.RotateAtTransform(60, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoSquare);
                gfx.ScaleAtTransform(0.75, 0.25, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                // shearX (5) is the horizontal skew, shearY (-25) is the vertical skew.
                // http://msdn.microsoft.com/en-us/library/system.windows.media.skewtransform.anglex(v=vs.110).aspx
                gfx.SkewAtTransform(5, -25, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.RotateAtTransform(30, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoSquare);
                gfx.SkewAtTransform(5, -25, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.SkewAtTransform(5, -25, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoSquare);
                gfx.RotateAtTransform(30, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                gfx.DrawRectangle(XPens.LightBlue, null, demoSquare);
                gfx.SkewAtTransform(10, 0, BoxCenter);
                gfx.DrawRectangle(XPens.OrangeRed, null, demoSquare);
                gfx.SkewAtTransform(0, -10, BoxCenter);
                gfx.DrawImage(image, demoSquare);
            }
            EndBox(gfx);
        }
    }
}
