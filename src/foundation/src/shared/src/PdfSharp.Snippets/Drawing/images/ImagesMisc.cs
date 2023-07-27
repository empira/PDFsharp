// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class ImagesMisc : Snippet
    {
        public ImagesMisc()
        {
            Title = "Draw diverse Images";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                // PNG always worked. However WPF build only gets 16 shades of gray while image has 256 shades.
                var img = ImageHelper.GetPngImage(ImageHelper.PngImages.Windows7Problem);
                gfx.DrawImage(img, 28, 18, 152, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                // This JPEG didn't work under Windows 7 and higher with PDFsharp 1.32 - should work now.
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.Windows7Problem);
                gfx.DrawImage(img, 28, 18, 152, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Box);
            {
                // Test Interpolate.
                var img = ImageHelper.GetPngImage(ImageHelper.PngImages.Color4A);
                img.Interpolate = true;
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Box);
            {
                // Test Interpolate.
                var img = ImageHelper.GetPngImage(ImageHelper.PngImages.Color4A);
                img.Interpolate = false;
                // THHO4STLA: Adds a second reference with "img.Interpolate = true;" - bug or feature? BUG!
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Box);
            {
                // Test Interpolate.
                var img = ImageHelper.GetPngImage(ImageHelper.PngImages.BlackwhiteA);
                img.Interpolate = true;
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Box);
            {
                // Test Interpolate.
                var img = ImageHelper.GetPngImage(ImageHelper.PngImages.BlackwhiteA);
                img.Interpolate = false;
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Fill);
            { }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Fill);
            { }
            EndBox(gfx);
        }

        public void RenderTestPage2(XGraphics gfx)
        {
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                // CMYK (?) test.
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.Balloons_CMYK);
                gfx.DrawImage(img, 28, 18, 114 * 350 / 263d, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
#if WPF || GDI
                // TIFF test.
                var img = ImageHelper.GetTiffImage(ImageHelper.TiffImages.Balloons_CMYK);
                gfx.DrawImage(img, 28, 18, 114 * 350 / 263d, 114);
#endif
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                // TIFF test.
                var img = ImageHelper.GetTiffImage(ImageHelper.TiffImages.Rose_RGB8);
                gfx.DrawImage(img, 28, 18, 114 * 269 / 215d, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
#if WPF || GDI
                // TIFF test.
                var img = ImageHelper.GetTiffImage(ImageHelper.TiffImages.Rose_CMYK);
                gfx.DrawImage(img, 28, 18, 114 * 269 / 215d, 114);
#endif
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Box);
            {
                // CMYK test.
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.PowerBooks_CMYK);
                gfx.DrawImage(img, 28, 18, 114 * 269 / 215d, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Box);
            {
                // OS2 BMP test. StL@THHO: Do we really need OS2 bitmaps anymore?
                var img = ImageHelper.GetBmpImage(ImageHelper.BmpImages.Test_OS2);
                gfx.DrawImage(img, 28, 18, 114 * 510 / 358d, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Box);
            {
                // GIF test.
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.Image009);
                gfx.DrawImage(img, 28, 18, 152, 152 * 82 / 683d);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
//#warning TODO Duplicate test.
                // GIF test.
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.Image009);
                gfx.DrawImage(img, 28, 18, 152, 152 * 82 / 683);
            }
            EndBox(gfx);
        }
    }
}
