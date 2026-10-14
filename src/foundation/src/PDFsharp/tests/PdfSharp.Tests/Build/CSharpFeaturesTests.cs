// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;
using FluentAssertions;

#if true

//using MyPoint = (int X, int Y);

namespace PdfSharp.Tests.Build
{
    /// <summary>
    /// Test what features of C# 11 to C# 12 can be used with
    /// .NET 6/8 and C# 11 to C# 12 with .NET 4.62 / .NET Standard 2.0.
    /// That’s amazing!
    /// </summary>
    [Collection("PDFsharp")]
    public class CSharpFeaturesTests
    {

        [Fact]
        public void Test_String_Extensions() // C# 14
        {
            "Hello".NullOrEmpty.Should().BeFalse();
            ((String)null!).NullOrEmpty.Should().BeTrue();
        }

        [Fact]
        public void Range_and_Index() // C# 8
        {
            string[] words =
            [
                // index from start    index from end
                "The",   // 0                   ^9
                "quick", // 1                   ^8
                "brown", // 2                   ^7
                "fox",   // 3                   ^6
                "jumps", // 4                   ^5
                "over",  // 5                   ^4
                "the",   // 6                   ^3
                "lazy",  // 7                   ^2
                "dog"    // 8                   ^1
            ];

            string[] quickBrownFox = words[1..4];
        }

        //[Fact]
        //public void Alias_any_type()  // C# 12
        //{
        //    MyPoint point = (7, 8);
        //    point.X.Should().Be(7);
        //    point.Y.Should().Be(8);
        //}

        [Fact]
        public void Collection_expressions()  // C# 12
        {
            // Create an array.
            int[] a = [1, 2, 3, 4, 5, 6, 7, 8];
            a.Length.Should().Be(8);

            // Create a list.
            List<string> b = ["one", "two", "three"];
            b.Count.Should().Be(3);
            b.ToArray()[1..^1].Length.Should().Be(1);

            //// Create a span
            //Span<char> c = ['a', 'b', 'c', 'd', 'e', 'f', 'h', 'i'];
            //c.Length.Should().Be(8);

            // Create a jagged 2D array.
            int[][] twoD = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];
            twoD.Length.Should().Be(3);
            twoD[1].Length.Should().Be(3);

            // Create a jagged 2D array from variables.
            int[] row0 = [1, 2, 3];
            int[] row1 = [4, 5, 6];
            int[] row2 = [7, 8, 9];
            int[][] twoDFromVariables = [row0, row1, row2];
            row0.Length.Should().Be(3);
            twoDFromVariables.Length.Should().Be(3);
            twoDFromVariables[1].Length.Should().Be(3);
        }

        //[System.Runtime.CompilerServices.InlineArray(10)]  ??? does not compile ???
        //public struct Buffer
        //{
        //    int _element0;
        //}

        [Fact]
        public void Everything_you_always_wanted_to_know_about_type_compatibility()
        {
            // Works like expected…

            // ReSharper disable CanSimplifyIsInstanceOfType

            // Note that “typeof(A).IsInstanceOfType(new B())” <=> “new B() is A”

            var result11 = typeof(A).IsInstanceOfType(new A());
            result11.Should().BeTrue();
            var result12 = typeof(A).IsInstanceOfType(new B());
            result12.Should().BeTrue();
            var result13 = typeof(B).IsInstanceOfType(new A());
            result13.Should().BeFalse();
            var result14 = typeof(A).IsInstanceOfType((object)new C());  // Expression is always false.
            result14.Should().BeFalse();
            var result15 = typeof(C).IsInstanceOfType((object)new A());  // Expression is always false.
            result15.Should().BeFalse();

            // ReSharper restore CanSimplifyIsInstanceOfType

            var result21 = typeof(A).IsSubclassOf(typeof(A));
            result21.Should().BeFalse();
            var result22 = typeof(A).IsSubclassOf(typeof(B));
            result22.Should().BeFalse();
            var result23 = typeof(B).IsSubclassOf(typeof(A));
            result23.Should().BeTrue();
            var result24 = typeof(A).IsSubclassOf(typeof(C));
            result24.Should().BeFalse();
            var result25 = typeof(C).IsSubclassOf(typeof(A));
            result25.Should().BeFalse();

            var result31 = typeof(A).IsAssignableFrom(typeof(A));
            result31.Should().BeTrue();
            var result32 = typeof(A).IsAssignableFrom(typeof(B));
            result32.Should().BeTrue();
            var result33 = typeof(B).IsAssignableFrom(typeof(A));
            result33.Should().BeFalse();
            var result34 = typeof(A).IsAssignableFrom(typeof(C));
            result34.Should().BeFalse();
            var result35 = typeof(C).IsAssignableFrom(typeof(A));
            result35.Should().BeFalse();

#if NET8_0_OR_GREATER
            var result41 = typeof(A).IsAssignableTo(typeof(A));
            result41.Should().BeTrue();
            var result42 = typeof(A).IsAssignableTo(typeof(B));
            result42.Should().BeFalse();
            var result43 = typeof(B).IsAssignableTo(typeof(A));
            result43.Should().BeTrue();
            var result44 = typeof(A).IsAssignableTo(typeof(C));
            result44.Should().BeFalse();
            var result45 = typeof(C).IsAssignableTo(typeof(A));
            result45.Should().BeFalse();
#endif
        }

        [Fact]
        public void Test_format_strings()
        {
            var zero = 0d;
            var a = String.Format(CultureInfo.InvariantCulture, "{0}", 0d);
            var b = String.Format(CultureInfo.InvariantCulture, "{0}", -0d);
            var c = String.Format(CultureInfo.InvariantCulture, "{0:0;0;0}", -zero);
            var d = String.Format(CultureInfo.InvariantCulture, "{0:0;0}", -(-7 - (-7)));
            var e = String.Format(CultureInfo.InvariantCulture, "{0}", -(-7d - -7d));
            var f = String.Format(CultureInfo.InvariantCulture, "{0:0;0}", -(-7d - -7d));
            var g = String.Format(CultureInfo.InvariantCulture, "{0:0;0;0}", -(-7d - -7d));

            bool b1 = zero == -zero;
            bool b2 = zero.Equals(-zero);
        }

        [Fact]
        public void Test_new_field_keyword()
        {
            var x = Hours;
            var y = PropA;
        }

        public A PropA
        {
            get => field; //??= new A();
            set
            {
                field = value;
            }
        } = new A();

        public double Hours
        {
            get;
            set
            {
                field = (value >= 0)
                    ? value
                    : throw new ArgumentOutOfRangeException(nameof(value), "The value must not be negative");
            }
        }

        public class A
        { }

        class B : A
        { }

        class C
        { }

#if true_
        [Fact]
        public void Generic_function_with_nullable_enum_values()
        {
            MyEnum e1 = default;
            MyEnum e2 = MyEnum.Foo;
            MyEnum? e3 = null;
            MyEnum? e4 = default;
            MyEnum? e5 = MyEnum.Foo;

            MyEnum r11 = GetEnum1<MyEnum>(e1);
            //MyEnum? r12 = GetEnum1<MyEnum>(e3);  // does not compile, OK
            MyEnum? r12 = GetEnum1<MyEnum>((MyEnum)e3);  // does not compile, OK
            //var r13 = GetEnum1<int>(42);  // does not compile, OK

            MyEnum? r21 = GetEnum2<MyEnum>(e1);
            MyEnum? r22 = GetEnum2<MyEnum>(e3);
            var r23 = GetEnum2<nint>(42);
            var r24 = GetEnum2<nint>(null);
        }

        enum MyEnum { Foo = 42 }

        TEnum? GetEnum1<TEnum>(TEnum? value) where TEnum : System.Enum
        {
            //_ = value.HasValue;  // Works, because v is a value type.
            //_ = value!.Value;  // Works, because v is a value type.
            //return null;
            //return (TEnum?)(object?)null;  // compiles, but crashes at runtime
            return default;                     // return always 0
            return value;             // return never null because value cannot be null.
        }

        TEnum? GetEnum2<TEnum>(TEnum? value) where TEnum : struct, Enum
        {
            _ = value.HasValue;  // Works, because v is a value type.
            //_ = value!.Value;  // Works, because v is a value type.
            return null;
        }
#endif

        [Fact]
        public void Test_extension_properties()
        {
            var s = "Hello, World!";
            var len = s.GetTheLength;
            len.Should().Be(13);

            var s2 = "Hello, World" + "!" * 3;
            s2.Should().Be("Hello, World!!!");

            double inches = 3;
            var points = inches.InchToPoint;
            points.Should().Be(3 * 72);

            inches.InchToPoint = 144;
            inches.Should().Be(2);

            char[] chars = ['a', 'b', 'c', 'd'];
            var c = chars.LastItem;
            c.Should().Be('d');
            chars.LastItem = 'z';
            var c2 = chars.LastItem;
            c2.Should().Be('z');

            chars.LastItem -= (char)1;
            var c3 = chars.LastItem;
            c3.Should().Be('y');
        }
    }

    /// <summary>
    /// Extension property for tests.
    /// </summary>
    public static class TestExtensions
    {
        extension(String str)
        {
            public int GetTheLength =>
                str.Length;

            public static string operator *(string source, int count)
            {
                return string.Concat(Enumerable.Repeat(source, count));
            }
        }

        extension(ref double d)
        {
            // Simple unit conversion helper. Needs "ref" above.
            public double InchToPoint
            {
                get { return d * 72; }
                set { d = value / 72; }
            }
        }

        extension(char[] chars)
        {
            public char LastItem
            {
                get { return chars[^1]; }
                set { chars[^1] = value; }
            }
        }
    }
}
#endif
