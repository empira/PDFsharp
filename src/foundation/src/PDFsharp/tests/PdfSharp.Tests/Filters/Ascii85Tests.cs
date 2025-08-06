// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using FluentAssertions;
using PdfSharp.Pdf.Filters;
using Xunit;

namespace PdfSharp.Tests.Filters
{
    [Collection("PDFsharp")]
    public class Ascii85Tests
    {
        [Fact]
        public void Check_Wikipedia_example()
        {
            // From Wikipedia: https://en.wikipedia.org/wiki/Ascii85#Example_for_Ascii85
            // Ensure that our code has no endianness issues.

            var filter = new Ascii85Decode();

            string textOriginal =
                "Man is distinguished, not only by his reason, but by this singular passion from other animals, " +
                "which is a lust of the mind, that by a perseverance of delight in the continued and indefatigable generation of knowledge, " +
                "exceeds the short vehemence of any carnal pleasure.";
            string textEncrypted =
                "9jqo^BlbD-BleB1DJ+*+F(f,q/0JhKF<GL>Cj@.4Gp$d7F!,L7@<6@)/0JDEF<G%<+EV:2F!,O<" +
                """DJ+*.@<*K0@<6L(Df-\0Ec5e;DffZ(EZee.Bl.9pF"AGXBPCsi+DGm>@3BB/F*&OCAfu2/AKYi(""" +
                "DIb:@FD,*)+C]U=@3BN#EcYf8ATD3s@q?d$AftVqCh[NqF<G:8+EV:.+Cf>-FD5W8ARlolDIal(" +
                "DId<j@<?3r@:F%a+D58'ATD4$Bl@l3De:,-DJs`8ARoFb/0JMK@qB4^F!,R<AKZ&-DfTqBG%G>u" +
                "D.RTpAKYo'+CT/5+Cei#DII?(E,9)oF*2M7/c~>";

            var test = filter.Decode(StringToBytes(textEncrypted));
            var testString = BytesToString(test);

            var (e, d) = EncodeDecode(StringToBytes(textOriginal), filter);

            string encoded = BytesToString(e);
            string decoded = BytesToString(d);
            int encryptedLength1 = textEncrypted.Length;
            int encryptedLength2 = encoded.Length;

            textOriginal.Should().Be(decoded);
            textEncrypted.Should().Be(encoded);
        }

        [Fact]
        public void Check_empty_data()
        {
            var filter = new Ascii85Decode();

            var (e, d) = EncodeDecode([], filter);
            e.Length.Should().Be(2);
            d.Length.Should().Be(0);
        }

        [Fact]
        public void Check_no_padding()
        {
            var filter = new Ascii85Decode();
            var bytes = new byte[4];

            TestRange(0, 500);
            //TestRange(0, 0xffffffff);  // Takes long, but succeeds.
            TestRange(0, 700);
            TestRange(0xfffff000, 0xffffffff);
            return;

            void TestRange(uint from, uint to)
            {
                for (long val = from; val <= to; val++)
                {
                    bytes[0] = (byte)(val >> 24);
                    bytes[1] = (byte)(val >> 16);
                    bytes[2] = (byte)(val >> 8);
                    bytes[3] = (byte)(val);
                    var (e, d) = EncodeDecode(bytes, filter);
                    e.Length.Should().BeLessThanOrEqualTo(5 + 2);
                    d.Length.Should().Be(4);

                    val.Should().Be(((uint)(d[0] << 24) | (uint)(d[1] << 16) | (uint)(d[2] << 8) | d[3]));
                }
            }
        }

        [SkippableFact]
        public void Check_padding()
        {
            Skip.If(SkippableTests.SkipSlowTests());

            var filter = new Ascii85Decode();

            // One byte.
            var bytes = new byte[1];
            for (uint val = 0; val <= 0xff; val++)
            {
                bytes[0] = (byte)val;
                var (e, d) = EncodeDecode(bytes, filter);
                e.Length.Should().Be(2 + 2);
                d.Length.Should().Be(1);
                d[0].Should().Be((byte)val);
            }

            // Two bytes.
            bytes = new byte[2];
            for (uint val = 0; val <= 0xffff; val++)
            {
                bytes[0] = (byte)(val >> 8);
                bytes[1] = (byte)val;
                var (e, d) = EncodeDecode(bytes, filter);
                e.Length.Should().Be(3 + 2);
                d.Length.Should().Be(2);
                d[0].Should().Be((byte)(val >> 8));
                d[1].Should().Be((byte)val);
            }

            // Three bytes.
            bytes = new byte[3];
            for (uint val = 0; val <= 0xffffff; val++)
            {
                //val += 0xc79ab;

                bytes[0] = (byte)(val >> 16);
                bytes[1] = (byte)(val >> 8);
                bytes[2] = (byte)val;
                var (e, d) = EncodeDecode(bytes, filter);
                e.Length.Should().Be(4 + 2);
                d.Length.Should().Be(3);
                d[0].Should().Be((byte)(val >> 16));
                d[1].Should().Be((byte)(val >> 8));
                d[2].Should().Be((byte)val);
            }
        }

        [Fact]
        public void Check_literal_data()
        {
            var filter = new Ascii85Decode();

            byte[][] bytes =
            [
                StringToBytes("The quick fox."),
                [255],  // Largest padding value
                [],
                [0x12],
                [0x12, 0x23],
                [0x12, 0x23, 0x34],
                [0x12, 0x23, 0x34, 0x45],
                [0x12, 0x23, 0x34, 0x45, 0x56],
                [0x12, 0x23, 0x34, 0x45, 0x56, 0x67],
                [0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78],
                [0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89],
                [0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89, 0x9a],
                [0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89, 0x9a, 0xab],
            ];

            for (int idx = 0; idx < bytes.Length; idx++)
            {
                var source = bytes[idx];
                var (e, d) = EncodeDecode(source, filter);
                source.SequenceEqual(d).Should().BeTrue();
                string s = BytesToString(d);

                var size = e.Length;
                var e2 = new Byte[3 * e.Length];
                var sb = new StringBuilder();
                for (int idxChar = 0; idxChar < size; idxChar++)
                {
                    sb.Append(' ');
                    sb.Append((char)e[idxChar]);
                    sb.Append('\n');
                }
                var d2 = filter.Decode(StringToBytes(sb.ToString()), (FilterParms?)null);
                source.SequenceEqual(d2).Should().BeTrue();
                s = BytesToString(d);
            }
        }

        static (byte[] Encoded, byte[] Decoded) EncodeDecode(byte[] bytes, Ascii85Decode filter)
        {
            var encoded = filter.Encode(bytes);
            var decoded = filter.Decode(encoded, (FilterParms?)null);
            return (encoded, decoded);
        }

        static byte[] StringToBytes(string s)
        {
            var bytes = new byte[s.Length];
            for (int idx = 0; idx < s.Length; idx++)
                bytes[idx] = (byte)s[idx];
            return bytes;
        }

        static string BytesToString(byte[] bytes)
        {
            var sb = new StringBuilder();
            for (int idx = 0; idx < bytes.Length; idx++)
                sb.Append((char)bytes[idx]);
            return sb.ToString();
        }
    }
}
