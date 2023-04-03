// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class ClippingMisc : SnippetBase
    {
        public ClippingMisc()
        {
            Title = "Clipping Miscellaneous";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Clipping only.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddPolygon(GetPentagram(70, BoxCenter));
                
                gfx.IntersectClip(path);

                const int offset = 50;
                for (var y = -offset; y < BoxHeight + offset; y += 5)
                    gfx.DrawLine(XPens.DarkBlue, new XPoint(0, y - offset), new XPoint(BoxWidth, y + offset));

                for (var x = -offset; x < BoxWidth + offset; x += 5)
                    gfx.DrawLine(XPens.DarkRed, new XPoint(x - offset, 0), new XPoint(x + offset, BoxHeight));
            }
            EndBox(gfx);

            // Clipping with Container.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
                var path = new XGraphicsPath();
                path.AddPolygon(GetPentagram(70, BoxCenter));

                const int offset = 50;

                var cont = gfx.BeginContainer();
                {
                    gfx.IntersectClip(path);

                    for (var y = -offset; y < BoxHeight + offset; y += 5)
                        gfx.DrawLine(XPens.DarkBlue, new XPoint(0, y - offset), new XPoint(BoxWidth, y + offset));
                }
                gfx.EndContainer(cont);

                for (var x = offset; x < BoxWidth -offset; x += 10)
                    gfx.DrawLine(XPens.DarkRed, new XPoint(x, 0.75 * BoxHeight), new XPoint(x + 0.25 * offset, BoxHeight));
            }
            EndBox(gfx);

            BeginBox(gfx, 3, BoxOptions.Tile);
            { }
            EndBox(gfx);

            BeginBox(gfx, 4, BoxOptions.Tile);
            { }
            EndBox(gfx);

            BeginBox(gfx, 5, BoxOptions.Tile);
            { }
            EndBox(gfx);

            BeginBox(gfx, 6, BoxOptions.Tile);
            { }
            EndBox(gfx);

            BeginBox(gfx, 7, BoxOptions.Tile);
            { }
            EndBox(gfx);

            BeginBox(gfx, 8, BoxOptions.Tile);
            { }
            EndBox(gfx);
        }
    }
}
