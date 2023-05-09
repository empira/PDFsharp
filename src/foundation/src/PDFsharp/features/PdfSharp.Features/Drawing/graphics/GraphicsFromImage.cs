// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Features.Drawing
{
    /// <summary>
    /// Drawing in a image. CGI and WPF only.
    /// </summary>
    public class GraphicsFromImage : Feature
    {
        /// <summary>
        /// Create a PNG.
        /// </summary>
        public static void Test1()
        {
            var image = XBitmapImage.CreateBitmap(400, 300);

            var gfx = XGraphics.FromImage(image);
            if (gfx == null)
                return;

            // TODO: Save state in bitmap and fail here
            //var gfx2 = XGraphics.FromImage(image);

            gfx.DrawLine(XPens.DarkBlue, XUnit.FromMillimeter(0), XUnit.FromMillimeter(150), XUnit.FromMillimeter(210), XUnit.FromMillimeter(150));

            gfx.DrawLine(XPens.DarkBlue, 0, 0, 400, 300);
            gfx.DrawLine(XPens.DarkBlue, 0, 300, 400, 0);
            gfx.DrawEllipse(XBrushes.Red, 0, 0, 400, 300);

            var encoder = XBitmapEncoder.GetPngEncoder();
            encoder.Source = image;

            // Should not be required.
            gfx.Dispose();

            using (var fs = new FileStream(@".\testpng.png", FileMode.Create))
            {
                // Automatically disposes gfx.
                encoder.Save(fs);
            }

            // Must fail.
            gfx.DrawEllipse(XBrushes.Red, 0, 0, 400, 300);
        }
    }
}
