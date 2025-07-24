// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

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
    }

    //[System.Runtime.CompilerServices.InlineArray(10)]  ??? does not compile ???
    //public struct Buffer
    //{
    //    private int _element0;
    //}
}
#endif
