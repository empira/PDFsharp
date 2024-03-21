// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.InteropServices;
using FluentAssertions;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using Xunit;

namespace PdfSharp.Tests
{
    public class WriterTests
    {
        [Fact]
        public void Write_import_file()
        {
            var testFile = IOUtility.GetAssetsPath(@"archives\samples-1.5\PDFs\SomeLayout.pdf")!;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                testFile = testFile.Replace('\\', '/');

            var outFile = @"SomeLayout_temp.pdf";

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(outFile);
            save.Should().Throw<InvalidOperationException>();
        }
    }
}
