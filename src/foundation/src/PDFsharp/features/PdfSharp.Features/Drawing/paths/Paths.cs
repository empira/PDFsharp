// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Quality;

#pragma warning disable 1591
namespace PdfSharp.Features.Drawing
{
    public class Paths : Feature
    {
        public void PathCurves()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathCurves());
        }

        public void PathMisc()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathMisc());
        }

        public void PathShapes()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathShapes());
        }

        public void PathText()
        {
            RenderSnippetAsPdf(new Snippets.Drawing.PathText());
        }
    }
}
