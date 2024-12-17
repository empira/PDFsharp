// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.UniversalAccessibility;
using Xunit;

namespace PdfSharp.Tests.PDF
{
    [Collection("PDFsharp")]
    public class PdfATests : IDisposable
    {
        public PdfATests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Simple_PDF_A_document()
        {
            var document = new PdfDocument();
            document.SetPdfA();

            // Get the manager for universal accessibility.
            var uaManager = UAManager.ForDocument(document);
            // Get structure builder.
            var sb = uaManager.StructureBuilder;

            document.ViewerPreferences.FitWindow = true;
            document.PageLayout = PdfPageLayout.SinglePage;

            var font = new XFont("Verdana", 10, XFontStyleEx.Regular);
            var pdfPage = document.AddPage();
            var xGraphics = XGraphics.FromPdfPage(pdfPage);

            // Create article element in document.
            sb.BeginElement(PdfGroupingElementTag.Article);
            {
                // Create paragraph element.
                sb.BeginElement(PdfBlockLevelElementTag.Paragraph);
                var layoutRectangle = new XRect(0, 72, pdfPage.Width.Point, pdfPage.Height.Point);
                xGraphics.DrawString("PDF/A Test", font, XBrushes.Black, layoutRectangle, XStringFormats.TopCenter);
                sb.End();
            }
            sb.End();

            string filename = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/pdf-a/PdfATest");
            document.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
