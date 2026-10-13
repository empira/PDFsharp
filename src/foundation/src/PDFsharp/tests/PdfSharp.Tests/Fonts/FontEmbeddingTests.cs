// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Text;
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.TestHelper;
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

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void Ligature_glyph_can_extract_as_original_text(bool reevaluateGlyphIndex, bool explicitUnicode,
            bool normalFirst)
        {
            var document = new PdfDocument();
            document.Options.CompressContentStreams = false;
            document.RenderEvents.RenderTextEvent += (_, args) =>
            {
                if (args.CodePointGlyphIndexPairs.Length != 2)
                    return;
                var ligature = reevaluateGlyphIndex
                    ? new PdfSharp.Internal.OpenType.CodePointGlyphIndexPair(0xFB02, 0)
                    : args.Font.OpenTypeDescriptor.GlyphIndicesFromCodePoints([0xFB02])[0];
                if (!reevaluateGlyphIndex)
                    ligature.CodePoint = 'f';
                ligature.UnicodeText = "fl";
                args.CodePointGlyphIndexPairs = [ligature];
                args.ReevaluateGlyphIndices = reevaluateGlyphIndex;
            };
            var page = document.AddPage();
            var font = explicitUnicode
                ? new XFont("Arial", 12, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode))
                : new XFont("Arial", 12, XFontStyleEx.Regular);
            using (var graphics = XGraphics.FromPdfPage(page))
            {
                graphics.DrawString(normalFirst ? "f" : "fl", font, XBrushes.Black, 50, 80);
                graphics.DrawString(normalFirst ? "fl" : "f", font, XBrushes.Black, 50, 100);
            }

            using var stream = new MemoryStream();
            document.Save(stream, false);
            var pdf = Encoding.ASCII.GetString(stream.ToArray());
            pdf.Should().Contain("<0066006C>");
            var content = PdfFileHelper.GetPageContentStreamEnumerator(document, 0);
            content.Text.MoveAndGetNext(true, out var first).Should().BeTrue();
            content.Text.MoveAndGetNext(true, out var second).Should().BeTrue();
            var shaped = normalFirst ? second! : first!;
            var shapedGlyph = PdfFileHelper.GetHexStringAsGlyphIndices(shaped.Text).Single();
            shapedGlyph.Should().NotBe("0");
            var shapedCid = shapedGlyph.PadLeft(4, '0').ToUpperInvariant();
            pdf.Should().Contain($"<{shapedCid}><{shapedCid}><0066006C>");
            if (explicitUnicode)
            {
                var normal = normalFirst ? first! : second!;
                var normalGlyph = PdfFileHelper.GetHexStringAsGlyphIndices(normal.Text).Single();
                var normalCid = normalGlyph.PadLeft(4, '0').ToUpperInvariant();
                pdf.Should().Contain($"<{normalCid}><{normalCid}><0066>");
            }
        }

        [Fact]
        public void Unchanged_render_text_event_preserves_default_glyph_content()
        {
            string Render(bool subscribe)
            {
                var document = new PdfDocument();
                if (subscribe)
                    document.RenderEvents.RenderTextEvent += (_, _) => { };
                var page = document.AddPage();
                var font = new XFont("Arial", 12, XFontStyleEx.Regular,
                    new XPdfFontOptions(PdfFontEncoding.Unicode));
                using (var graphics = XGraphics.FromPdfPage(page))
                    graphics.DrawString("fl", font, XBrushes.Black, 50, 100);

                var content = PdfFileHelper.GetPageContentStreamEnumerator(document, 0);
                content.Text.MoveAndGetNext(true, out var text).Should().BeTrue();
                return text!.Text;
            }

            Render(true).Should().Be(Render(false));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Conflicting_ligature_mapping_is_rejected(bool literalFirst, bool reevaluateGlyphIndex)
        {
            var document = new PdfDocument();
            document.RenderEvents.RenderTextEvent += (_, args) =>
            {
                if (args.CodePointGlyphIndexPairs.Length != 2)
                    return;
                var ligature = reevaluateGlyphIndex
                    ? new PdfSharp.Internal.OpenType.CodePointGlyphIndexPair(0xFB02, 0)
                    : args.Font.OpenTypeDescriptor.GlyphIndicesFromCodePoints([0xFB02])[0];
                if (!reevaluateGlyphIndex)
                    ligature.CodePoint = 'f';
                ligature.UnicodeText = "fl";
                args.CodePointGlyphIndexPairs = [ligature];
                args.ReevaluateGlyphIndices = reevaluateGlyphIndex;
            };
            var page = document.AddPage();
            var font = new XFont("Arial", 12, XFontStyleEx.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));

            Action draw = () =>
            {
                using var graphics = XGraphics.FromPdfPage(page);
                graphics.DrawString(literalFirst ? "ﬂ" : "fl", font, XBrushes.Black, 50, 80);
                graphics.DrawString(literalFirst ? "fl" : "ﬂ", font, XBrushes.Black, 50, 100);
            };

            draw.Should().Throw<InvalidOperationException>();
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
    }
}
