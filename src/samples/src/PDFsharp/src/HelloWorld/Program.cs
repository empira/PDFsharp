// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Quality;

namespace HelloWorld
{
    /// <summary>
    /// This sample is just a copy and used as a smoke test for PDFsharp development.
    /// For the original sample check the PDFsharp.Samples repository.
    /// </summary>
    class Program
    {
        static void Main(string[] _)
        {
            // Set a logger factory.
            // https://docs.pdfsharp.net/link/logging.html
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("PDFsharp", LogLevel.Warning)
                    .AddFilter(_ => true)
                    .AddConsole();
            });
            LogHost.Factory = loggerFactory;

            GlobalFontSettings.UseWindowsFontsUnderWindows = true;

            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";
            document.Info.Author = "PDFsharp team";
            document.Info.Subject = "Hello, World!";

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            // Draw two lines with a red default pen.
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            // Create a font.
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);

            // Draw the text.
            gfx.DrawString("Hello, PDFsharp!", font, XBrushes.Black,
                new XRect(0, 0, width, height), XStringFormats.Center);

            // Save the document...
            var fullName = PdfFileUtility.GetTempPdfFullFileName("PdfSharpSamples/HelloWorld/HelloWorld" + Capabilities.Build.BuildTag);
            document.Save(fullName);

            // ...and start a viewer.
            PdfFileUtility.ShowDocument(fullName);
        }
    }
}
