using FluentAssertions;
using PdfSharp.Pdf.IO;
using Xunit;

namespace PdfSharp.Tests
{
    public class WriterTests
    {
        [Fact]
        public void Write_import_file()
        {
            var testFile = @"..\..\..\..\..\..\..\..\..\assets\archives\samples-1.5\PDFs\SomeLayout.pdf";
            var outFile = @"SomeLayout_temp.pdf";

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(outFile);
            save.Should().Throw<InvalidOperationException>();
        }
    }
}
