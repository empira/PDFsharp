// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.FontResolver;
using PdfSharp.Fonts;
using Xunit;

namespace PdfSharp.Tests.Fonts
{
    [Collection("PDFsharp")]
    public class FontHelperTests : IDisposable
    {
        public FontHelperTests()
        {
            GlobalFontSettings.ResetFontManagement();
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
        }

        public void Dispose()
        {
            GlobalFontSettings.ResetFontManagement();
        }

        [Fact]
        public void Count_glyphs_of_fonts()
        {
            // U+1F339	🌹 ‚

            var font1 = new XFont("Arial", 10);
            int counter1 = CountGlyphs(font1);
            counter1.Should().Be(3361);

            //if (Capabilities.Build.IsGdiBuild || Capabilities.Build.IsWpfBuild)
            {
                var font2 = new XFont("Segoe UI Emoji", 10);

                int counter2 = CountGlyphs(font2);
                counter2.Should().Be(1962);
            }

            const int comma = '\'';
            const int apostrophy = (int)'\u201A'; //PdfEncoders.WinAnsiEncoding.‚
            const int apostrophy2 = '‚';
            var gi1 = GlyphHelper.GlyphIndexFromCodePoint(comma, font1);
            var gi2 = GlyphHelper.GlyphIndexFromCodePoint(apostrophy, font1);
            var gi3 = GlyphHelper.GlyphIndexFromCodePoint(apostrophy2, font1);
        }

        public static int CountGlyphs(XFont font)
        {
            var sw = Stopwatch.StartNew();
            int counter = 0;
            for (int codePoint = 0; codePoint < 0x10FFFF; codePoint++)
            {
                // Skip surrogates.
                if (codePoint == 0xD800)
                    codePoint += 0x0800;

                var glyphIndex = GlyphHelper.GlyphIndexFromCodePoint(codePoint, font);
                if (glyphIndex != 0)
                    counter++;
            }

            sw.Stop();
            // About 3 seconds if the font has a cmap table of format 12.
            var ms = sw.ElapsedMilliseconds;
            return counter;
        }
    }
}
