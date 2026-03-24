// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using FluentAssertions;
using PdfSharp.Pdf;
using Xunit;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    [Collection("PDFsharp")]
    public class RealNumberTests : ObjectModelTestsBase
    {
        [Fact]
        public void Leading_zeros()
        {
            // We had a bug in the format string of PageDestinations.
            const string format = "#.#;-#.#;0";

            var x1 = 0.ToString(format, CultureInfo.InvariantCulture);
            x1.Should().Be("0");

            var x2 = 0.2.ToString(format, CultureInfo.InvariantCulture);
            x2.Should().Be(".2");

            var x3 = 123.0.ToString(format, CultureInfo.InvariantCulture);
            x3.Should().Be("123");

            var x4 = (-.445).ToString(format, CultureInfo.InvariantCulture);
            x4.Should().Be("-.4");
        }

        [Fact]
        public void Test_Single_Write_and_Read()
        {
            /*
            Setting a PdfReal to Single.MaxValue / Single.MinValue could not be parsed and a unit test failed.
            Here is why.

            Single.MaxValue in IEEE 754 binary format 32 bit:
                0b_0_11111110_11111111111111111111111 (0x7F7FFFFF)
            Sign       (1 bit) :   0 -> positive
            Exponent  (8 bits) : 254 -> 2^127  (254 - 127 = 127)
            Mantissa (23 bits) : (1.)11111111111111111111111 base 2, '1.' is implicitly defined.

            Formally this calculates to 
                340282346638528859811704183484516925440 (39 decimal places)
                       ^- gibberish starts approximately here because
                          only the first ~7 decimal places are significant

            Now lets see what is this in Double.
            Single.MaxValue in IEEE 754 binary format expanded to 64 bit:
                0b_0_10001111110_1111111111111111111111100000000000000000000000000000
            Sign       (1 bit) :    0 -> positive
            Exponent (11 bits) : 1150 -> 2^127  (1150 - 1023 = 127)
            Mantissa (52 bits) : (1.)1111111111111111111111100000000000000000000000000000 base 2, '1.' is implicitly defined.

            This value formally also calculates to 
                340282346638528859811704183484516925440 (39 decimal places)
                               ^- gibberish starts approximately here because
                                  now the first ~15 decimal places are significant.
            This additional 'virtual' precision comes from adding the 29 binary zeros to the mantissa.
            It multiplies the Single value with 2^29 and makes the gibberish part of the value.

            This explains why Single.MaxValue is larger in Double when formatted as string:

            Single.MaxValue.ToString("#");           // is "340282300000000000000000000000000000000"
            ((double)Single.MaxValue).ToString("#"); // is "340282346638529000000000000000000000000"

            I know the IEEE 754 number format since I programmed a Tektronix 4051 desktop computer 
            in the early 80s in 6800 assembler, but this result perplexed me.

            For nerds: Play around with IEEE 754 number: https://numeral-systems.com/ieee-754-converter/

            Solution for PDFsharp

            PDFsharp stores floating point values as Double, but writes them to PDF formatted as Single.
            */
#if true_
            var s0 = PdfReal.ToPdfLiteral(Math.PI, 0, false);
            var s8 = PdfReal.ToPdfLiteral(Math.PI, 8, false);
            var s12 = PdfReal.ToPdfLiteral(Math.PI, 34, false);
#endif

            //long longMax = Int64.MaxValue;
            //long longSingleMax = (unchecked((long)Single.MaxValue));

            float fMax = Single.MaxValue;
            string s1 = fMax.ToString("#");
            string s2 = ((double)fMax).ToString("#");
            string sx = ((double)fMax).ToString("#");
            string s3 = ((double)Single.MaxValue).ToString("#");
            //string s4 = ((decimal)fMax).ToString("#");

            // From .NET source code.
            // public const float MaxValue = 3.40282347E+38;                 // .NET Framework
            // public const float MaxValue = (float)3.40282346638528859e+38; // .NET 6/8

            // 340282346638528859811704183484516925440
            string max1 = Single.MaxValue.ToString("#");           // is "340282300000000000000000000000000000000"
            string max2 = ((double)Single.MaxValue).ToString("#"); // is "340282346638529000000000000000000000000"
            string max3 = ((float)(double)Single.MaxValue).ToString("#"); // is "340282346638529000000000000000000000000"
            string max4 = ((float)Double.MaxValue).ToString("#"); // is "340282346638529000000000000000000000000"

            var parsed1 = Single.Parse(((double)Single.MaxValue).ToString("#"));
            var parsed2 = Double.Parse(((double)Single.MaxValue).ToString("#"));

#if NET8_0_OR_GREATER
            // Throws in .NET Framework
            var parsed3 = Single.Parse("3402823466385288598117041834845169259990");
#endif
            var parsed4 = Double.Parse(((double)Single.MaxValue).ToString("#"));
            var xxxx = new PdfReal(Single.MaxValue);
            var yyyy = new PdfReal(Single.MinValue);
        }
    }
}
