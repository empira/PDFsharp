// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.StandardFonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using Xunit;

namespace PdfSharp.Tests.Fonts
{
    [Collection("PDFsharp")]
    public class FontEmbeddingTests : IDisposable
    {
        public FontEmbeddingTests()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
        }

        public void Dispose()
        {
            GlobalFontSettings.ResetFontManagement();
        }

        [Fact]
        public void Embed_font_subset()
        {
            var font1 = new XFont("Arial", 10, XFontStyleEx.Regular, new(PdfFontEncoding.WinAnsi, PdfFontEmbedding.TryComputeSubset));
            var font2 = new XFont("Arial", 10, XFontStyleEx.Regular, new(PdfFontEncoding.Unicode, PdfFontEmbedding.TryComputeSubset));

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString("Hello, World!", font1, XBrushes.Black, 50, 100);
            gfx.DrawString("Hello, World!", font2, XBrushes.Black, 50, 150);

            gfx.DrawString("Köln† \u20AC € \u00a9©†   (Köln† \u20ac \u20ac \u00a9\u00a9†)", font1, XBrushes.Black, 50, 200);

            var fileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/Fonts/FontEmbeddingTests/" + nameof(FontEmbeddingTests));
            doc.Save(fileName);
            var info = new FileInfo(fileName);
            var size = info.Length;

            size.Should().BeLessThan(35_000);
        }

        [Fact]
        public void Embed_font_complete_file()
        {
            var font = new XFont("Arial", 10, XFontStyleEx.Regular, new(PdfFontEmbedding.EmbedCompleteFontFile));
            var font1 = new XFont("Arial", 10, XFontStyleEx.Regular, new(PdfFontEncoding.WinAnsi, PdfFontEmbedding.EmbedCompleteFontFile));
            var font2 = new XFont("Arial", 10, XFontStyleEx.Regular, new(PdfFontEncoding.Unicode, PdfFontEmbedding.EmbedCompleteFontFile));

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString("Hello, World!", font1, XBrushes.Black, 50, 100);
            gfx.DrawString("Hello, World!", font2, XBrushes.Black, 50, 150);
            gfx.DrawString("Köln† \u20AC € \u00a9©†   (Köln† \u20ac \u20ac \u00a9\u00a9†)", font1, XBrushes.Black, 50, 200);

            var fileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/Fonts/FontEmbeddingTests/" + nameof(Embed_font_complete_file));
            doc.Save(fileName);
            var info = new FileInfo(fileName);
            var size = info.Length;

            size.Should().BeGreaterThan(500_000);
        }

        [Fact]
        public void Embed_Segoe_UI_Emoji_file()
        {
            //const int SmilingFaceWithHearts = 0x_0001_F970;  // 😍
            //const int RedRose = 0x_0001_F339;  // 🌹

            var fontEmoji = new XFont("Segoe UI Emoji", 10, XFontStyleEx.Regular, new(PdfFontEncoding.Unicode, PdfFontEmbedding.EmbedCompleteFontFile));

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString("🌹 😍 \ud83c\udf39 \ud83d\ude0d", fontEmoji, XBrushes.Black, 50, 100);

            var fileName = PdfFileUtility.GetTempPdfFullFileName("PDFsharp/UnitTest/Fonts/FontEmbeddingTests/" + nameof(Embed_Segoe_UI_Emoji_file));
            doc.Save(fileName);
            var info = new FileInfo(fileName);
            var size = info.Length;
            var count = FontHelperTests.CountGlyphs(fontEmoji);

            size.Should().BeGreaterThan(1_000_000);
        }

        [Fact]
        public void Omit_Standard_Fonts()
        {
            GlobalFontSettings.FontResolver = new StandardFontResolver();

            var doc = new PdfDocument();

            // squeeze out an additional KB of data by NOT creating a metadata-stream
            doc.Options.CreateMetadata = false;

            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double x = 50, y = 50;
            foreach (var fontName in StandardFontData.FontNames)
            {
                if (fontName == StandardFontNames.Symbol || fontName == StandardFontNames.ZapfDingbats)
                    continue;

                var font = new XFont(fontName, 12, XFontStyleEx.Regular,
                    new XPdfFontOptions(PdfFontEmbedding.OmitStandardFont));

                gfx.DrawString(fontName, font, XBrushes.Black, x, y);
                y += 20;
                // english pangram
                //gfx.DrawString("The quick brown fox jumps over the lazy dog.", font, XBrushes.Black, x, y);
                // german pangram
                gfx.DrawString("Typograf Jakob zürnt schweißgequält vom öden Text.", font, XBrushes.Black, x, y);

                y += 40;
            }

            var fileName = PdfFileUtility.GetTempPdfFullFileName(
                "PDFsharp/UnitTest/Fonts/FontEmbeddingTests/" + nameof(Omit_Standard_Fonts));
            doc.Save(fileName);
            var info = new FileInfo(fileName);
            var size = info.Length;
#if DEBUG
            size.Should().BeLessThan(10_000);
#else
            size.Should().BeLessThan(5_000);
#endif
        }
    }
}
