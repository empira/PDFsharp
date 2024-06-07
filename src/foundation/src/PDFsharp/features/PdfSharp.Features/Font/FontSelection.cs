// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality;

#pragma warning disable 1591

namespace PdfSharp.Features
{
    public class FontSelection : Feature
    {
        public void HelloWord1Test()
        {
            RenderSnippetAsPdf(new Snippets.Font.HelloWorld1Snippet());
        }

        public void DefaultFontConstructionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.DefaultWindowsFontsSnippet());
        }

        public void FontFamilyConstructionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.FontFamilyConstructionSnippet());
        }

        public void PlatformFontConstructionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.PlatformFontConstructionSnippet());
        }

        public void PrivateFontCollectionTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.PrivateFontCollectionSnippet());
        }

        public void FrutigerFontsTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.FrutigerFontsSnippet());
        }

        public void HelveticaNeueFontsTest()
        {
            RenderSnippetAsPdf(new Snippets.Font.HelveticaNeueFontsSnippet());
        }

        public void TestExoticFontResolver()
        {
            //RenderSnippetAsPdf(new Snippets.Fonts.FontResolver2());
        }

        public void PrivateFontCollection1()
        {
            RenderSnippetAsPdf(new Snippets.Font.PrivateFontCollection1Snippet());
        }
    }
}
