// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.UniversalAccessibility;

#pragma warning disable 1591
namespace PdfSharp.Features.Pdf
{
    public class PdfA
    {
        public void CreatePdfA()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("PDFsharp", LogLevel.Warning)
                    .AddFilter(_ => true)
                    .AddConsole();
            });
            LogHost.Factory = loggerFactory;

            // Create a new PDF document.
            var document = new PdfDocument();
            document.ViewerPreferences.FitWindow = true;
            document.PageLayout = PdfPageLayout.SinglePage;
            document.Info.Title = "Created with PDFsharp";
            document.Info.Author = "PDFsharp team";
            document.Info.Subject = "Hello, World!";

            // Get the manager for universal accessibility.
            var uaManager = UAManager.ForDocument(document);
            uaManager.SetDocumentLanguage("en-US");

            // Get structure builder.
            var sb = uaManager.StructureBuilder;

            // Create an empty page in this document.
            var page = document.AddPage();

            sb.BeginElement(PdfBlockLevelElementTag.Paragraph);
            {

                // Get an XGraphics object for drawing on this page.
                var gfx = XGraphics.FromPdfPage(page);

                // Draw two lines with a red default pen.
                var width = page.Width.Point;
                var height = page.Height.Point;
                gfx.DrawLine(XPens.Red, 0, 0, width, height);
                gfx.DrawLine(XPens.Red, width, 0, 0, height);

                // Draw a circle with a red pen which is 1.5 point thick.
                var r = width / 5;
                gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White,
                    new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

                // Create a font.
                var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);

                // Draw the text.
                gfx.DrawString("Hello, PDFsharp!", font, XBrushes.Black,
                    new XRect(0, 0, width, height), XStringFormats.Center);
            }
            sb.End();
            
            // Save the document...
            var fullName = PdfFileUtility.GetTempPdfFullFileName("Features/Pdf-A/Test1" + Capabilities.Build.BuildTag);
            document.Save(fullName);

            // ...and start a viewer.
            PdfFileUtility.ShowDocument(fullName);
        }
    }
}
