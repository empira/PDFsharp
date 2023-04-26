// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class TransformationText : Snippet
    {
        public TransformationText()
        {
            Title = "Transformation of Text";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            const string facename = "Segoe UI";
            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            const string text = "ABCxyz";
            var font = new XFont(facename, 24, XFontStyleEx.Regular, options);
            var brush = XBrushes.DarkBlue;
            var axesPen = new XPen(XColors.Black, 3);

            var axes = new[] { new XPoint(24, 24), new XPoint(24, 120), new XPoint(180, 120) };
            var origin = axes[1];

            // No transformation.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                gfx.DrawLines(axesPen, axes);
                gfx.DrawLines(XPens.Firebrick, axes);
                gfx.DrawString(text, font, brush, axes[1]);
            }
            EndBox(gfx);

            // Translation.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                gfx.DrawLines(axesPen, axes);
                gfx.TranslateTransform(30, -20);
                gfx.DrawLines(XPens.Firebrick, axes);
                gfx.DrawString(text, font, brush, axes[1]);
            }
            EndBox(gfx);

            // Scaling.
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                gfx.DrawLines(axesPen, axes);
                gfx.ScaleAtTransform(1.3, 1.2, origin.X, origin.Y);
                gfx.DrawLines(XPens.Firebrick, axes);
                gfx.DrawString(text, font, brush, axes[1]);
            }
            EndBox(gfx);

            // Rotation.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                gfx.DrawLines(axesPen, axes);
                gfx.RotateAtTransform(-45, origin);
                gfx.DrawLines(XPens.Firebrick, axes);
                gfx.DrawString(text, font, brush, axes[1]);
            }
            EndBox(gfx);

            // Skewing (or shearing).
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                gfx.DrawLines(axesPen, axes);
                gfx.SkewAtTransform(-30, -20, origin.X, origin.Y);
                gfx.DrawLines(XPens.Firebrick, axes);
                gfx.DrawString(text, font, brush, axes[1]);
            }
            EndBox(gfx);
        }
        
        static void DrawAxes(XGraphics gfx, XPen pen, XPoint origin, double length)
        {
            gfx.DrawLines(pen, origin.X, origin.Y - length, origin.X, origin.Y, origin.X + length, origin.Y);
        }
    }
}
