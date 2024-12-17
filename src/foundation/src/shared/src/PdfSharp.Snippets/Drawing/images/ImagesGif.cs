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
    public class ImagesGif : Snippet
    {
        public ImagesGif()
        {
            Title = "Drawing GIF Images";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // The PNG images were converted to GIF - alpha transparency was converted to a transparency mask.

            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.TruecolorTransparency);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                // TODO_OLD 2014-12-01 PDFsharp does not yet support transparency for grayscale images.
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.GrayscaleTransparency);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            {
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.TruecolorA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            {
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.GrayscaleA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            {
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.Color8A);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            {
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.IndexedmonoA);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            {
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.Color4A);
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            {
                var img = ImageHelper.GetGifImage(ImageHelper.GifImages.BlackwhiteA);
                img.Interpolate = false;
                gfx.DrawImage(img, 68, 18, 114, 114);
            }
            EndBox(gfx);
        }
    }
}
