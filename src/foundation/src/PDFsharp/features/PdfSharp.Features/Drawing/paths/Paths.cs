// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality;

namespace PdfSharp.Features.Drawing
{
    public class Paths : FeatureBase
    {
        public static void PathCurves()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathCurves());
        }

        public static void PathMisc()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathMisc());
        }

        public static void PathShapes()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathShapes());
        }

        public static void PathText()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathText());
        }
    }
}
