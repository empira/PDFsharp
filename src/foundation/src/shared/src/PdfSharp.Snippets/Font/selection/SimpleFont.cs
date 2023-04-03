// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using System.IO;
//using System.Threading.Tasks;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;
//using PdfSharp.QualityTools;

//namespace PdfSharpFeatures.Font
//{
//    public class SimpleFont : FeatureTestBase
//    {
//        public override async Task Execute(Stream stream = null)
//        {
//            // Create a new PDF document.
//            PdfDocument document = CreateNewPdfDocument();

//            // Create an empty page.
//            PdfPage page = document.AddPage();

//            // Get an XGraphics object for drawing.
//            XGraphics gfx = XGraphics.FromPdfPage(page);

//            //XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);

//            // Create a font.
//            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic, new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always));

//            // Draw the text
//            gfx.DrawString("Hello, World!", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

//            // Save and show the document.
//            string filename = await SaveAndShowDocumentAsync(document, "SimpleFont");
//        }
//    }
//}
