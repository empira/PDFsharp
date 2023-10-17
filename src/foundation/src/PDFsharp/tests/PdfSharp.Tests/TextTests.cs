// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests
{
    public class TextTests
    {
        [Fact/*(Skip = "Not working in Core build")*/]
        public void PDF_with_Emojis()
        {
            GlobalFontSettings.FontResolver ??= NewFontResolver.Get();

            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";
            document.Info.Author = "111😢😞💪";
            document.Info.Subject = "111😢😞💪";

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            XFont font = new XFont("Segoe UI Emoji", 12, XFontStyleEx.Regular, options);
            gfx.DrawString("111😢😞💪", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            gfx.DrawString("\ud83d\udca9\ud83d\udca9\ud83d\udca9\u2713\u2714\u2705\ud83d\udc1b\ud83d\udc4c\ud83c\udd97\ud83d\udd95 \ud83e\udd84 \ud83e\udd82 \ud83c\udf47 \ud83c\udf46 \u2615 \ud83d\ude82 \ud83d\udef8 \u2601 \u2622 \u264c \u264f \u2705 \u2611 \u2714 \u2122 \ud83c\udd92 \u25fb", font, XBrushes.Black, new XRect(0, 50, page.Width, page.Height), XStringFormats.Center);

            // Save the document...
            string filename = PdfFileHelper.CreateTempFileName("HelloEmoji");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }
    }
}
