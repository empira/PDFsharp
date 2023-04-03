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
    public class ImagesJpeg : SnippetBase
    {
        public ImagesJpeg()
        {
            Title = "Drawing JPEG Images";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // The PNG images were converted to JPEG - color palettes and transparency were lost in conversion, but images should look the same (except for JPEG artefacts).

            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.TruecolorNoAlpha);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.GrayscaleNoAlpha);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.TruecolorA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.GrayscaleA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.Color8A);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.IndexedmonoA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.Color4A);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                var img = ImageHelper.GetJpegImage(ImageHelper.JpegImages.BlackwhiteA);
                img.Interpolate = false;
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);
        }
    }
}
