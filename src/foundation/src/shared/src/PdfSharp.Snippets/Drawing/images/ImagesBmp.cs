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
    public class ImagesBmp : Snippet
    {
        public ImagesBmp()
        {
            Title = "Drawing Windows Bitmap Images";
        }

        public override void RenderSnippet(XGraphics gfx)
        {

            BeginBox(gfx, 1, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Box);
            {
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                var img = ImageHelper.GetBmpImage(ImageHelper.BmpImages.TruecolorA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                var img = ImageHelper.GetBmpImage(ImageHelper.BmpImages.GrayscaleA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                var img = ImageHelper.GetBmpImage(ImageHelper.BmpImages.Color8A);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                var img = ImageHelper.GetBmpImage(ImageHelper.BmpImages.IndexedmonoA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                var img = ImageHelper.GetBmpImage(ImageHelper.BmpImages.Color4A);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                var img = ImageHelper.GetBmpImage(ImageHelper.BmpImages.BlackwhiteA);
                img.Interpolate = false;
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);
        }
    }
}
