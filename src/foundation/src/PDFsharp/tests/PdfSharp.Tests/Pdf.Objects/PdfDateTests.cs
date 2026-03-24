// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using Xunit;
using FluentAssertions;
using PdfSharp.Quality.Testing;

namespace PdfSharp.Tests.Pdf.Objects
{
    [Collection("PDFsharp")]
    public class PdfDateTests : PdfSharpTestBase
    {
        readonly string _tempRoot = GetTempRoot(typeof(DateTests));

        [Fact]
        public void Test_PdfDate()
        {
            // Standard test.
            {
                var now = new DateTimeOffset(2025, 10, 16, 15, 28, 7, new(2, 0, 0));
                var date = new PdfDate(now);
                var result = date.ToString();
                result.Should().Be("D:20251016152807+02'00'");
            }

            // Some more tests.
            {
                var now = DateTimeOffset.Now;
                var d1 = now.ToString();
                var d2 = now.ToString("zzz");

                var date1 = new PdfDate();
                var str1 = date1.ToString();
                date1.Value.Should().BeNull();
                str1.Should().BeEmpty();

                var date2 = new PdfDate(new DateTimeOffset(2025, 9, 22, 15, 16, 17, TimeSpan.Zero));
                var str2 = date2.ToString();
                date2.ToString().Should().Be("D:20250922151617Z");

                var date3 = new PdfDate(new DateTimeOffset(2025, 9, 22, 15, 16, 17, new(2, 30, 0)));
                var str3 = date3.ToString();
                str3.Should().Be("D:20250922151617+02'30'");

                var date4 = new PdfDate(new DateTimeOffset(2025, 9, 22, 15, 16, 17, -new TimeSpan(2, 45, 0)));
                var str4 = date4.ToString();
                str4.Should().Be("D:20250922151617-02'45'");
            }
        }
    }
}
