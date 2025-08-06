// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using System.IO;
//using System.Threading.Tasks;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;
//using PdfSharp.Quality;

//namespace PdfSharpFeatures.Fonts
//{
//    public class ChineseFont : FeatureTestBase
//    {
//        public override async Task Execute(Stream stream = null)
//        {
//            // Create a new PDF document.
//            PdfDocument document = CreateNewPdfDocument();

//            // Create an empty page.
//            PdfPage page = document.AddPage();

//            // Get an XGraphics object for drawing.
//            XGraphics gfx = XGraphics.FromPdfPage(page);

//            var options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);

//            var font = new XFont("Arial Unicode MS", 20, XFontStyleEx.BoldItalic, options);

//            // Draw the text
//            gfx.DrawString("123 你好!", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

//            document.Outlines.Add("456 你好!", page);

//            // Save and show the document.
//            string filename = await SaveAndShowDocumentAsync(document, "ChineseFont").ConfigureAwait(false);
//        }
//    }
//}
