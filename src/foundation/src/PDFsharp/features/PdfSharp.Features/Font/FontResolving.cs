// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality;
using PdfSharp.Snippets.Font;

#pragma warning disable 1591
namespace PdfSharp.Features
{
    public class FontResolvers : Feature
    {
        public void TestSegoeWpFontResolver()
        {
            RenderSnippetAsPdf(new SegoeWpFontResolverSnippet());
        }

        public void TestExoticFontResolver()
        {
            RenderSnippetAsPdf(new ExoticFontResolverSnippet());
        }
    }
}
