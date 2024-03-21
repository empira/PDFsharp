// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Logging;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] _)
        {

            TargetFrameworkAttribute targetFrameworkAttribute = (TargetFrameworkAttribute)Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .SingleOrDefault()!;

            // Set a logger factory.
            // https://docs.pdfsharp.net/link/logging.html
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("PDFsharp", LogLevel.Warning)
                    .AddFilter(level => true)
                    .AddConsole();
            });
            LogHost.Factory = loggerFactory;

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
            var width = page.Width;
            var height = page.Height;
            gfx.DrawLine(XPens.Red, 0, 0, width, height);
            gfx.DrawLine(XPens.Red, width, 0, 0, height);

            // Draw a circle with a red pen which is 1.5 point thick.
            var r = width / 5;
            gfx.DrawEllipse(new XPen(XColors.Red, 1.5), XBrushes.White, new XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            //var bytes = PdfSharp.WPFonts.FontDataHelper.SegoeWP;

            if (Capabilities.Build.IsCoreBuild)
                GlobalFontSettings.FontResolver = new FailsafeFontResolver();

            // Create a font.
            var font = new XFont("Times New Roman", 20, XFontStyleEx.BoldItalic);

#if true
            // Draw the text.
            gfx.DrawString("Hello, PDFsharp!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
#else
            // Draw the text.
            gfx.DrawString("Hello, PDFsharp!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height - 50), XStringFormats.Center);

            // Draw framework name.
            gfx.DrawString(targetFrameworkAttribute.FrameworkDisplayName!, font, XBrushes.Black,
                new XRect(0, 25, page.Width, page.Height - 50), XStringFormats.Center);

            // Draw branch.
            gfx.DrawString(GitVersionInformation.BranchName, font, XBrushes.Black,
                new XRect(0, 50, page.Width, page.Height - 50), XStringFormats.Center);
#endif

            // Save the document...
            var fullName = PdfFileUtility.GetTempPdfFullFileName("PdfSharpSamples/HelloWorld/HelloWorld" + Capabilities.Build.BuildTag);
            document.Save(fullName);
            // ...and start a viewer.
            PdfFileUtility.ShowDocument(fullName);
        }
    }
}
