// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Fonts
{
    [Collection("PDFsharp")]
    public class FontHelperTests
    {
        [Fact]
        public void Count_glyphs_of_fonts()
        {
            // U+1F339	🌹

            var font1 = new XFont("Arial", 10);
            int counter1 = CountGlyphs(font1);
            counter1.Should().Be(3361);

            if (Capabilities.Build.IsGdiBuild || Capabilities.Build.IsWpfBuild)
            {
                var font2 = new XFont("Segoe UI Emoji", 10);

                int counter2 = CountGlyphs(font2);
                counter2.Should().Be(1962);
            }
        }

        public static int CountGlyphs(XFont font)
        {
            var sw = Stopwatch.StartNew();
            int counter = 0;
            for (int codePoint = 0; codePoint < 0x10FFFF; codePoint++)
            {
                // Skip surrogates.
                //if (codePoint is >= 0xD800 and <= 0xDFFF)
                //    continue;
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
