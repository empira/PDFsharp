using PdfSharp.Quality;
using PdfSharp.Snippets.Font;

namespace PdfSharp.Features
{
    public class FontResolvers : FeatureBase
    {
        public static void TestSegoeWpFontResolver()
        {
            RenderSnippetAsPdf(new SegoeWpFontResolverSnippet());
        }

        public static void TestExoticFontResolver()
        {
            RenderSnippetAsPdf(new ExoticFontResolverSnippet());
        }
    }
}
