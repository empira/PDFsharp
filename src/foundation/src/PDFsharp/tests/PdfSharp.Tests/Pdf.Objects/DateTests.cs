// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Metadata;
using Xunit;
using FluentAssertions;
using PdfSharp.Quality.Testing;

namespace PdfSharp.Tests.Pdf.Objects
{
    [Collection("PDFsharp")]
    public class DateTests : PdfSharpTestBase
    {
        readonly string _tempRoot = GetTempRoot(typeof(DateTests));
#if true_
        [Fact]
        public void Test_format_specifiers()
        {
            // How do I use the K and zzz format specifiers?
            // See https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.tostring?view=net-8.0
            var now = DateTimeOffset.Now;
            var d1 = now.ToString("zzz");
            var d2 = now.ToString("zz");
            var d3 = now.ToString("zzz");
            var d4 = now.ToString("ssK");
            var d5 = now.ToString("yyyy-MM-ddTHH:mm:ssK");

             now = new DateTimeOffset(2025, 9, 22, 15, 16, 17, TimeSpan.Zero);
             var d6 = now.ToString("yyyy-MM-ddTHH:mm:ssK");
        }
#endif

        [Fact]
        public void Test_MetadataManager_ToXmpDateString()
        {
            DateTimeOffset? date1 = null;
            var str1 = MetadataManager.ToXmpDateString(date1);
            str1.Should().BeEmpty();

            var date2 = new DateTimeOffset(2025, 9, 22, 15, 16, 17, TimeSpan.Zero);
            var str2 = MetadataManager.ToXmpDateString(date2);
            str2.Should().Be("2025-09-22T15:16:17+00:00");

            var date3 = new DateTimeOffset(2025, 9, 22, 15, 16, 17, new TimeSpan(2, 30, 0));
            var str3 = MetadataManager.ToXmpDateString(date3);
            str3.Should().Be("2025-09-22T15:16:17+02:30");

            var date4 = new DateTimeOffset(2025, 9, 22, 15, 16, 17, -new TimeSpan(2, 45, 0));
            var str4 = MetadataManager.ToXmpDateString(date4);
            str4.Should().Be("2025-09-22T15:16:17-02:45");
        }
    }
}
