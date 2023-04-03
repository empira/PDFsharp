// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality;

namespace PdfSharp.Features
{
    public class FontSelection : FeatureBase
    {
        public static void HelloWord1Test()
        {
            RenderSnippetAsPdf(new Snippets.Font.HelloWorld1Snippet());
        }

        public static void DefaultFontConstructionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.DefaultWindowsFontsSnippet());
        }

        public static void FontFamilyConstructionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.FontFamilyConstructionSnippet());
        }

        public static void PlatformFontConstructionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.PlatformFontConstructionSnippet());
        }

        public static void PrivateFontCollectionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.PrivateFontCollectionSnippet());
        }

        public static void FrutigerFontsTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.FrutigerFontsSnippet());
        }

        public static void HelveticaNeueFontsTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.HelveticaNeueFontsSnippet());
        }

        public static void TestExoticFontResolver()
        {
            //RenderSnippetAsPdf(new Snippets.Fonts.FontResolver2());
        }

        public static void PrivateFontCollection1()
        {
            RenderSnippetAsPdf(new Snippets.Font.PrivateFontCollection1Snippet());
        }
    }
}
