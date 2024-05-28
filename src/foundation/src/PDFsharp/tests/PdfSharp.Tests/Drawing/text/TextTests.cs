// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.FontResolver;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Drawing
{
    [Collection("PDFsharp")]
    public class TextTests : IDisposable
    {
        public TextTests()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
        }

        public void Dispose()
        {
            GlobalFontSettings.ResetFontManagement();
        }

        [Fact]
        public void PDF_with_Emojis()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";
            document.Info.Author = "111😢😞💪";
            document.Info.Subject = "111😢😞💪";

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            var width = page.Width.Point;
            var height = page.Height.Point;

            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            XFont font = new XFont(UnitTestFontResolver.EmojiFont, 12, XFontStyleEx.Regular, options);
            gfx.DrawString("111😢😞💪", font, XBrushes.Black, new XRect(0, 0, width, height), XStringFormats.Center);
            gfx.DrawString("\ud83d\udca9\ud83d\udca9\ud83d\udca9\u2713\u2714\u2705\ud83d\udc1b\ud83d\udc4c\ud83c\udd97\ud83d\udd95 \ud83e\udd84 \ud83e\udd82 \ud83c\udf47 \ud83c\udf46 \u2615 \ud83d\ude82 \ud83d\udef8 \u2601 \u2622 \u264c \u264f \u2705 \u2611 \u2714 \u2122 \ud83c\udd92 \u25fb", font, XBrushes.Black, new XRect(0, 50, width, height), XStringFormats.Center);

            // Save the document...
            string filename = PdfFileUtility.GetTempPdfFileName("HelloEmoji");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void PDF_with_No_Break_Hyphen()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;

            document.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];
                    if (item.CodePoint is '\u2011')
                    {
                        item.CodePoint = '-';
                        args.ReevaluateGlyphIndices = true;
                    }
                }
            };

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var font = new XFont("Arial", 12, XFontStyleEx.Bold, options);
            gfx.DrawString("No\u2011break\u2011hyphen-Test", font, XBrushes.Black, new XRect(0, 50, page.Width.Point, page.Height.Point), XStringFormats.Center);

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("PdfWithNoBreakHyphen");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(document, 0);

            streamEnumerator.Text.MoveAndGetNext(true, out var textInfo).Should().BeTrue();
            textInfo!.IsHex.Should().BeTrue();
            var hexString = textInfo.Text;
            hexString.Should().NotBeNull();

            var glyphIds = PdfFileHelper.GetHexStringAsGlyphIndices(hexString);
            glyphIds.Should().NotContain("0", "no char (and no no-break hyphen) should be converted to an invalid glyph (\"0\")");
        }

        [Fact/*(Skip = "Not yet working")*/]
        public void PDF_with_Wingdings()
        {
            // Create a new PDF document.
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;

            var containsNotFoundGlyphs = false;

            document.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];

                    // Check for not found glyphs.
                    if (item.GlyphIndex == 0)
                        containsNotFoundGlyphs = true;
                }
            };

            // Create an empty page in this document.
            var page = document.AddPage();

            // Get an XGraphics object for drawing on this page.
            var gfx = XGraphics.FromPdfPage(page);

            var options = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var font = new XFont("Arial", 12, XFontStyleEx.Bold, options);
            gfx.DrawString("1 þ", font, XBrushes.Black, new XRect(50, 50, 20, 20), XStringFormats.TopLeft);

            if (!PdfSharp.Capabilities.Build.IsCoreBuild)
            {
                font = new XFont("Wingdings", 12, XFontStyleEx.Regular, options);
                gfx.DrawString("1 þ", font, XBrushes.Black, new XRect(50, 100, 20, 20), XStringFormats.Center);
            }

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("PdfWithWingdings");
            document.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            containsNotFoundGlyphs.Should().BeFalse();
        }
    }
}
