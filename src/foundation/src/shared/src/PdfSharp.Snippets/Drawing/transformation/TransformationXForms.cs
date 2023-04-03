// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class TransformationXForms : SnippetBase
    {
        public TransformationXForms()
        {
            Title = "Transformation of XForms";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            var height = BoxHeight * 2 / 3;
            var width = height / 1.4142135623730950488016887242097;
            var demoSquare = new XRect((BoxWidth - width) / 2, (BoxHeight - height) / 2, width, height);

            var doc = ImageHelper.GetPdfForm(ImageHelper.PdfFiles.SomeLayout);
            var image = doc;

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
