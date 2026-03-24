// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using System.Numerics;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Build
{
    /// <summary>
    /// Is Vector2 and Matrix3x2 available in all builds?
    /// </summary>
    [Collection("PDFsharp")]
    public class NumericsTests
    {
        [Fact]
        public void Test_String_Extensions()
        {
            Vector2 vector = new();
            Matrix3x2 matrix = new();

            _ = vector;
            _ = matrix;
        }

        [Fact]
        public void Numeric_formats()
        {
            const string format = ".##;-.##;0";

            const double number0001 = 0.0001d;
            const double number001 = 0.001d;
            const double number01 = 0.01d;
            const double number1 = 0.1d;

            string str = String.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", number0001);
            str.Should().Be("0");
            str = string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", -number0001);
            str.Should().Be("0");

            str = String.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", number001);
            str.Should().Be("0");
            str = string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", -number001);
            str.Should().Be("0");

            str = String.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", number01);
            str.Should().Be(".01");
            str = string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", -number01);
            str.Should().Be("-.01");

            str = String.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", number1);
            str.Should().Be(".1");
            str = string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", -number1);
            str.Should().Be("-.1");

            str = Invariant($"{number0001:.##;-.##;0}");
            str.Should().Be("0");
            str = Invariant($"{-number0001:.##;-.##;0}");
            str.Should().Be("0");

            str = Invariant($"{number001:.##;-.##;0}");
            str.Should().Be("0");
            str = Invariant($"{-number001:.##;-.##;0}");
            str.Should().Be("0");

            str = Invariant($"{number01:.##;-.##;0}");
            str.Should().Be(".01");
            str = Invariant($"{-number01:.##;-.##;0}");
            str.Should().Be("-.01");

            str = Invariant($"{number1:.##;-.##;0}");
            str.Should().Be(".1");
            str = Invariant($"{-number1:.##;-.##;0}");
            str.Should().Be("-.1");
        }
    }
}
