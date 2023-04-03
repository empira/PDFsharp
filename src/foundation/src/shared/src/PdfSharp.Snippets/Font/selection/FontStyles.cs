// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using System.IO;
//using System.Threading.Tasks;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;
//using PdfSharp.Quality;

//namespace PdfSharpFeatures.Font
//{
//    public class FontStyles : FeatureTestBase
//    {
//        public override async Task Execute(Stream stream = null)
//        {
//            //const string fontName = "Times New Roman";
//            //const string fontName = "Arial";
//            const string fontName = "Arial Narrow";
//            //const string fontName = "Segoe UI";

//            // Create a new PDF document.
//            PdfDocument document = CreateNewPdfDocument();

//            // Create an empty page.
//            PdfPage page = document.AddPage();

//            // Get an XGraphics object for drawing.
//            XGraphics gfx = XGraphics.FromPdfPage(page);

//            //XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);

//            // Create fonts.
//            const double fontSize = 28;
//            var options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
//            var fontRegular = new XFont(fontName, fontSize, XFontStyleEx.Regular);
//            var fontBold = new XFont(fontName, fontSize, XFontStyleEx.Bold, options);
//            var fontItalic = new XFont(fontName, fontSize, XFontStyleEx.Italic, options);
//            var fontBoldItalic = new XFont(fontName, fontSize, XFontStyleEx.BoldItalic, options);
//            var fontUnderline = new XFont(fontName, fontSize, XFontStyleEx.Underline, options);
//            var fontStrikeout = new XFont(fontName, fontSize, XFontStyleEx.Strikeout, options);
//            var fontUnderlineStrikeout = new XFont(fontName, fontSize, XFontStyleEx.Underline | XFontStyleEx.Strikeout | XFontStyleEx.Italic, options);

//            // Draw the text.
//            var x = 60;
//            var y = 100;
//            var dy = 80;
//            gfx.DrawLine(XPens.LightGray, x, 0, x, page.Height);
//            gfx.DrawLine(XPens.LightGray, 0, y, page.Width, y);
//            y += dy;
//            gfx.DrawLine(XPens.LightGray, 0, y, page.Width, y);
//            y += dy;
//            gfx.DrawLine(XPens.LightGray, 0, y, page.Width, y);
//            y += dy;
//            gfx.DrawLine(XPens.LightGray, 0, y, page.Width, y);
//            y += dy;
//            gfx.DrawLine(XPens.LightGray, 0, y, page.Width, y);
//            y += dy;
//            gfx.DrawLine(XPens.LightGray, 0, y, page.Width, y);
//            y += dy;
//            gfx.DrawLine(XPens.LightGray, 0, y, page.Width, y);

//            // Draw the text.
//            y = 100;
//            gfx.DrawString("Text is regular. Äöfg", fontRegular, XBrushes.Black, x, y, XStringFormats.Default);
//            y += dy;
//            gfx.DrawString("Text is bold. Äöfg", fontBold, XBrushes.Black, x, y, XStringFormats.Default);
//            y += dy;
//            gfx.DrawString("Text is italic. Äöfg", fontItalic, XBrushes.Black, x, y, XStringFormats.Default);
//            y += dy;
//            gfx.DrawString("Text is bold and italic. Äöfg", fontBoldItalic, XBrushes.Black, x, y, XStringFormats.Default);
//            y += dy;
//            gfx.DrawString("Text is underlined. Äöfg", fontUnderline, XBrushes.Black, x, y, XStringFormats.Default);
//            y += dy;
//            gfx.DrawString("Text is crossed out. Äöfg", fontStrikeout, XBrushes.Black, x, y, XStringFormats.Default);
//            y += dy;
//            gfx.DrawString("Text is crossed out and underlined. Äöfg", fontUnderlineStrikeout, XBrushes.Black, x, y, XStringFormats.Default);

//            // Save and show the document.
//            string filename = await SaveAndShowDocumentAsync(document, "SimpleFont");
//        }
//    }
//}
