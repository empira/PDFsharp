using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
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
