// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Drawing;

#pragma warning disable 1591
namespace PdfSharp.Features.Drawing
{
    public class SurrogateChars : Feature
    {
        public void Test1()
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();
            page.Orientation = PageOrientation.Landscape;
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            var shapeTypes = new Surrogates();
            shapeTypes.RenderSnippet(gfx);

            var filename = PdfFileUtility.GetTempPdfFileName(nameof(SurrogateChars));

            PdfFileUtility.SaveAndShowDocument(doc, filename);
        }
    }
}
