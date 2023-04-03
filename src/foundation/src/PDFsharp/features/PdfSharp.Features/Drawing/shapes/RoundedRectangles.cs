// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Drawing;

namespace PdfSharp.Features.Drawing
{
    public class RoundedRectangles : FeatureBase
    {
        public static void Test1()
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Presentation);

            var shapeTypes = new ShapeTypes();
            shapeTypes.RenderSnippet(gfx);

            //SaveAndShowDocument(doc,"Test.pdf");
            doc.Save("Test.pdf");
        }

        //public override void Execute(Stream stream = null)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
