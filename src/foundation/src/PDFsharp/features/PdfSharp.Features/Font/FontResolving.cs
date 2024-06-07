// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;

#pragma warning disable 1591
namespace PdfSharp.Features
{
    public class FontResolvers : Feature
    {
        public void TestSegoeWpFontResolver()
        {
            PdfSharpCore.ResetFontManagement();

            RenderSnippetAsPdf(new SegoeWpFontResolverSnippet());
        }

        public void TestExoticFontResolver()
        {
            PdfSharpCore.ResetFontManagement();

            RenderSnippetAsPdf(new ExoticFontResolverSnippet());
        }
    }
}
