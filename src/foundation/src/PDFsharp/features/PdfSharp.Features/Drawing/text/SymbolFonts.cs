// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality;
using PdfSharp.Snippets.Drawing;

#pragma warning disable 1591
namespace PdfSharp.Features.Drawing
{
    public class SymbolFontFeature : Feature
    {
        public void Symbols()
        {
            RenderSnippetAsPdf(new SymbolFonts());
        }

        protected override void RenderAllSnippets()
        {
            Symbols();
        }
    }
}
