// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Snippets.Font;
using Xunit;

namespace PdfSharp.Tests.Drawing
{
    [Collection("PDFsharp")]
    public class UnicodeHelperTests
    {
        //const int Euro = '€';
        //const int SmilingFaceWithHearts = 0x_0001_F970;  // 😍
        const int RedRose = 0x_0001_F339;  // 🌹

        const char SomeHighSurrogate = '\uD842';
        const char SomeLowSurrogate = '\uDC42';

        const string SomeHighSurrogateString = "\uD842";
        const string SomeLowSurrogateString = "\uDC42";

        [Fact]
        public void Utf32FromString_Test()
        {
            const string rose = "ABC\ud83c\udf39XYZ";
            var r1 = UnicodeHelper.Utf32FromString(rose);
            r1.Should().BeEquivalentTo([(int)'A', (int)'B', (int)'C', RedRose, (int)'X', (int)'Y', (int)'Z',]);

            const string invalidHigh = "ABC" + SomeHighSurrogateString + "XYZ";
            var r10 = UnicodeHelper.Utf32FromString(invalidHigh);
            r10.Should().BeEquivalentTo([(int)'A', (int)'B', (int)'C', (int)SomeHighSurrogate, (int)'X', (int)'Y', (int)'Z',]);
        }

        [Fact]
        public void ConvertToUtf32_Test()
        {
            const string rose = "\ud83c\udf39";
            UnicodeHelper.ConvertToUtf32(rose[0], rose[1]).Should().Be(RedRose);

            UnicodeHelper.ConvertToUtf32(SomeLowSurrogate, SomeHighSurrogate).Should().Be(0);
        }
    }
}
