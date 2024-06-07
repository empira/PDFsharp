// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using FluentAssertions;
using PdfSharp.Pdf.Internal;
using Xunit;

namespace PdfSharp.Tests.Encodings
{
    [Collection("PDFsharp")]
    public class EncodingTests
    {
        [Fact]
        public void Check_UTF_8_overlong_encodings()
        {
            // This test actually test my knowledge of UTF-8 encoding. 
            // Reference: https://en.wikipedia.org/wiki/UTF-8
            // Proof that overlong encodings are not allowed.
            var charA = 'A';
            var hexA = '\x41';
            var unicodeA = '\u0041';
            var stringA = charA.ToString();

            hexA.Should().Be(charA);
            unicodeA.Should().Be(charA);

            byte[] oneByteA = [0b_0100_0001];
            // Invalid encodings of 'A'.
            byte[] twoByteA = [0b_110_00001, 0b_10_00_0001];
            byte[] threeByteA = [0b_1110_0000, 0b_10_00_0001, 0b_10_00_0001];
            byte[] fourByteA = [0b_11110_000, 0b_10_00_0000, 0b_10_00_0001, 0b_10_00_0001];

            var a1 = Encoding.UTF8.GetString(oneByteA);
            var a2 = Encoding.UTF8.GetString(twoByteA);
            var a3 = Encoding.UTF8.GetString(threeByteA);
            var a4 = Encoding.UTF8.GetString(fourByteA);

            a1.Should().Be(stringA);
            a2.Should().NotBe(stringA);
            a3.Should().NotBe(stringA);
            a4.Should().NotBe(stringA);
        }

        [Fact]
        public void AnsiEncodingTest()
        {
            var copyright = (int)'©';
            copyright.Should().Be('\u00A9');

            var euro = (int)'€';
            euro.Should().Be('\u20AC');

            var ansiEncoding = PdfEncoders.WinAnsiEncoding;

            // Test syntax of collection expression.
            var xx = ansiEncoding.GetBytes((char[])['©', '€'], 0, 2);
            var yy = new[] { '©', '€' };
            char[] zz = ['©', '€'];

            var bytes = ansiEncoding.GetBytes(new[] { '©', '€' }, 0, 2);
            bytes[0].Should().Be((int)'\u00A9');
            bytes[1].Should().Be((int)'\u0080');
        }

        [Fact]
        public void AnsiEncoding_ANSI_to_Unicode_test_implementation()
        {
            int[] nonAnsi = [0x81, 0x8D, 0x8F, 0x90, 0x9D];

            // Implementation was verified with .NET Ansi encoding.
            Encoding dotnetImplementation = GetDotNetAnsiEncoding()!;
            Encoding pdfSharpImplementation = PdfEncoders.WinAnsiEncoding;
            //if (dotnetImplementation == null!)
            //    return;

            // It took some time for me to understand why this cannot work :-)
            // int[] x = [0..255, 333];  // error CS0029: Cannot implicitly convert type 'system.Range' to 'int'

            // Check ANSI characters.
            for (int i = 0; i <= 255; i++)
            {
                byte[] b = [(byte)i];
                char[] ch1 = dotnetImplementation.GetChars(b, 0, 1);
                char[] ch2 = pdfSharpImplementation.GetChars(b, 0, 1);

                var char1 = ch1[0];
                var char2 = ch2[0];

                //if (i == 0x81)
                if (nonAnsi.FirstOrDefault(x => x == i) != default)
                    _ = typeof(int);  // A NOP for a break point.

                if (ch1[0] != ch2[0])
                    _ = typeof(int);

                ch1[0].Should().Be(ch2[0]);

                //if (i switch { 0x81 => true, 0x8D => true, 0x8F => true, 0x90 => true, 0x9D => true, _ => false })
                //    continue;

                //if (nonAnsi.FirstOrDefault(x => x == i) != default)
                //    continue;

                byte[] b1 = dotnetImplementation.GetBytes(ch1, 0, 1);
                byte[] b2 = pdfSharpImplementation.GetBytes(ch1, 0, 1);

                if (b1.Length != b2.Length || b1.Length > 1 || b1[0] != b2[0])
                    _ = typeof(int);  // A NOP for a break point.

                b1.Length.Should().Be(b2.Length);

                if (false && nonAnsi.FirstOrDefault(x => x == i) != default)
                {
                    b1[0].Should().Be((byte)i);
                    b2[0].Should().Be(0xFF);
                }
                else
                {
                    b1[0].Should().Be(b2[0]);
                }
            }
        }

        [Fact]
        public void AnsiEncoding_Unicode_to_Unicode_test_implementation()
        {
            int[] nonAnsi = [0x81, 0x8D, 0x8F, 0x90, 0x9D];

            // Implementation was verified with .NET Ansi encoding.
            Encoding dotnetImplementation = GetDotNetAnsiEncoding()!;
            Encoding pdfSharpImplementation = PdfEncoders.WinAnsiEncoding;
            //if (dotnetImplementation == null!)
            //    return;

            int[] abc = new int[128];
            for (int i = 0, ach = 128; i <= 127; i++, ach++)
                abc[i] = pdfSharpImplementation.GetChars(new byte[] { (byte)ach }, 0, 1)[0];

            // Check Unicode chars.
            for (int i = 0; i <= 65535; i++)
            {
                //if (i >= 256)
                //    break;
                if (i == 0x80)
                    _ = typeof(int);  // A NOP for a break point.

                char[] ch = [(char)i];
                byte[] b1 = dotnetImplementation.GetBytes(ch, 0, 1);
                byte[] b2 = pdfSharpImplementation.GetBytes(ch, 0, 1);
                char ch2 = (char)b2[0];
                int l1 = b1.Length;
                int l2 = b2.Length;

                if (b1.Length != b2.Length || b1.Length > 1 || b1[0] != b2[0])
                    _ = typeof(int);  // A NOP for a break point.

                l1.Should().Be(1);
                l1.Should().Be(l2);
                if (i <= 127)
                {
                    b1[0].Should().Be(b2[0]);
                }
                else //if (i <= 255) 
                {
                    if (ch2 == '?')
                        continue;

                    var b = abc.FirstOrDefault(x => x == i);
                    b.Should().NotBe(0);
                }
            }
        }

        Encoding? GetDotNetAnsiEncoding()
        {
#if NET6_0_OR_GREATER
            return CodePagesEncodingProvider.Instance.GetEncoding(1252);
#else
            return Encoding.GetEncoding(1252);
#endif
        }
    }
}
