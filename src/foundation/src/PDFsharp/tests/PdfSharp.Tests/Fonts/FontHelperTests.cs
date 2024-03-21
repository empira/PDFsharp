// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Fonts
{
    public class FontHelperTests
    {
        static FontHelperTests()
        {
            GlobalFontSettings.FontResolver ??= SnippetsFontResolver.Get();
        }

        [Fact]
        public void Count_glyphs_of_fonts()
        {
            // U+1F339	🌹

            var font1 = new XFont("Arial", 10);
            int counter1 = CountGlyphs(font1);
            counter1.Should().Be(3361);

            var font2 = new XFont("Segoe UI Emoji", 10);
            int counter2 = CountGlyphs(font2);
            counter2.Should().Be(1962);
        }
        
        static int CountGlyphs(XFont font)
        {
            var sw = Stopwatch.StartNew();
            int counter = 0;
            for (int codePoint = 0; codePoint < 0x10FFFF; codePoint++)
            {
                // Skip surrogates.
                if (codePoint is >= 0xD000 and <= 0xDFFF)
                    continue;

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
