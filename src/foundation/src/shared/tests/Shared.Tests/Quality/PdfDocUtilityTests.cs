// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Quality;
using Xunit;

#pragma warning disable IDE0059

namespace Shared.Tests.Quality
{
    public class PdfDocUtilityTests
    {
        [Fact]
        public void GetTempPdfFileName_Test()
        {
            var name1 = PdfFileUtility.GetTempPdfFileName("", false);
            var name2 = PdfFileUtility.GetTempPdfFileName("", true);
            var name3 = PdfFileUtility.GetTempPdfFileName("MyFile", false);
            var name4 = PdfFileUtility.GetTempPdfFileName("MyFile", true);

            // Test was reviewed manually.
            true.Should().BeTrue();
        }

        [Fact]
        public void GetTempPdfFullFileName_Test()
        {
            var name1 = PdfFileUtility.GetTempPdfFullFileName("", false);
            var name2 = PdfFileUtility.GetTempPdfFullFileName("", true);
            var name3 = PdfFileUtility.GetTempPdfFullFileName("MyFile", false);
            var name4 = PdfFileUtility.GetTempPdfFullFileName("MyFile", true);
            var name5 = PdfFileUtility.GetTempPdfFullFileName("aaa/MyFile", false);
            var name6 = PdfFileUtility.GetTempPdfFullFileName("aaa/MyFile", true);
            var name7 = PdfFileUtility.GetTempPdfFullFileName("aaa/bbb/MyFile", false);
            var name8 = PdfFileUtility.GetTempPdfFullFileName("aaa/bbb/MyFile", true);

            // Test was reviewed manually.
            true.Should().BeTrue();
        }

        [Fact]
        public void FindLatestPdfTempFile_Test()
        {
            var root = IOUtility.GetSolutionRoot();
            root.Should().NotBeNull();

            var file = IOUtility.FindLatestTempFile(null, root!, "pdf", true);

            // Test was reviewed manually.
            true.Should().BeTrue();
        }
    }
}
