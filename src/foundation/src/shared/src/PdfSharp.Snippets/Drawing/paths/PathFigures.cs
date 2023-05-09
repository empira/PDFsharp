// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PdfSharp.Snippets.Drawing
{
    public class PathFigures : Snippet
    {
        public PathFigures()
        {
            Title = "Path Figures";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            // Draw rectangle via path.
            BeginBox(gfx, 1, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Draw ellipse via path.
            BeginBox(gfx, 2, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Draw polygon via path in winding mode.
            BeginBox(gfx, 3, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Draw polygon via path in alternate mode.
            BeginBox(gfx, 4, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Draw closed curve via path in winding mode.
            BeginBox(gfx, 5, BoxOptions.Tile);
            {
             
            }
            EndBox(gfx);

            // Path: Line-Move- Rect-Move- Line-Move- Polygon-Move- Bézier.
            BeginBox(gfx, 6, BoxOptions.Tile);
            {
            }
            EndBox(gfx);

            // Path: Line-Line- Line-Line- Line-Line- Line-Line- Bézier.
            BeginBox(gfx, 7, BoxOptions.Tile);
            {
              
            }
            EndBox(gfx);

            // Path: Line-Line- Rect-Line- Line-Line- Polygon-Line- Bézier.
            BeginBox(gfx, 8, BoxOptions.Tile);
            {
              
            }
            EndBox(gfx);
        }
    }
}
