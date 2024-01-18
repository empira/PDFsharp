using FluentAssertions;
using System.Text;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;
using Xunit.Abstractions;
using PdfSharp.Drawing.Layout;

namespace PdfSharp.Tests.IO
{
    public class LargePDFReadWrite : IoBaseTest
    {
        private readonly ITestOutputHelper output;

        public LargePDFReadWrite(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact(Skip = "Too slow for Unit test runner")] // test takes about 6mins and 10GB RAM peak in KVM with CPU AMD Ryzen 7 5800X (12 from 16 logical cores, 32GB RAM)
        public void CanCreatePdfOver2gb()
        {
            const string outName = "CreateLargePdf.pdf";
            int pageCount = 70000; //2.1gb @ 369sec to create
            ValidateTargetAvailable(outName);

            var document = new PdfDocument();
            var watch = new System.Diagnostics.Stopwatch();
            GlobalFontSettings.FontResolver = new SegoeWpFontResolver();
            var font = new XFont("Segoe Wp", 10);

            watch.Start();
            for (var i = 0; i < pageCount; i++)
            {
                AddAPage(document, font);
            }

            watch.Stop();

            SaveDocument(document, outName);
            output.WriteLine($"CreatePDF took {watch.Elapsed.TotalSeconds} sec");
            ValidateFileIsPdf(outName);
            CanReadPdf(outName);
        }

        private void AddAPage(PdfDocument document, XFont font)
        {
            const int x = 40;
            const int y = 50;
            var page = document.AddPage();
            var renderer = XGraphics.FromPdfPage(page);
            var tf = new XTextFormatter(renderer);
            var width = page.Width.Value - 50 - x;
            var height = page.Height.Value - 50 - y;
            var rect = new XRect(40, 50, width, height);
            renderer.DrawRectangle(XBrushes.SeaShell, rect);
            tf.DrawString(TestData.LoremIpsumText, font, XBrushes.Black, rect, XStringFormats.TopLeft);
        }
    }
}