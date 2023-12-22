// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
//using PdfSharp.Pdf.IO;
using PdfSharp.Snippets.Font;
using PdfSharp.UniversalAccessibility.Drawing;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] _)
        {
            TargetFrameworkAttribute targetFrameworkAttribute = (TargetFrameworkAttribute)Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .SingleOrDefault()!;

            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";
            document.Info.Author = "xxxöäüß";  // TODO Write a test for non ASCII characters in Info.
            document.Info.Subject = "s xxxöäüß";

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
            //var font = new XFont("Segoe WP", 20, XFontStyleEx.Regular);
            //var font = new XFont("Arial", 20, XFontStyleEx.Regular);

            // Draw the text.
            gfx.DrawString("Hello, PDFsharp!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

            // Draw framework name.
            gfx.DrawString(targetFrameworkAttribute.FrameworkDisplayName!, font, XBrushes.Black,
                new XRect(0, 25, page.Width, page.Height), XStringFormats.Center);

            // Draw branch.
            gfx.DrawString(GitVersionInformation.BranchName, font, XBrushes.Black,
                new XRect(0, 50, page.Width, page.Height), XStringFormats.Center);

            // Save the document...
            var dir = System.IO.Directory.GetCurrentDirectory();
            var filename = $"HelloWorld-{Guid.NewGuid().ToString("N").ToUpperInvariant()}_tempfile.pdf";
            filename = System.IO.Path.Combine(dir, filename);
            //Console.WriteLine($"Filename='{filename}'");
            document.Save(filename);
            // ...and start a viewer.
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
    }
}
