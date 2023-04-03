using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PdfSharp.Pdf;
using Xunit;

namespace PdfSharp.Tests
{
    public class CreationTests
    {
        [Fact]
        public void Create_for_Stream()
        {
            using (var outputStream = new FileStream("test0.pdf", FileMode.Create, FileAccess.ReadWrite))
            {
                PdfDocument document = new PdfDocument(outputStream);
                document.AddPage();
                document.Close();
                document.Version.Should().BeGreaterThan(0);
            }
        }

        //[Fact]
        //public void Create_for_Stream_with_Version_0()
        //{
        //    using (var outputStream = new FileStream("test0.pdf", FileMode.Create, FileAccess.ReadWrite))
        //    {
        //        PdfDocument document = new PdfDocument(outputStream);
        //        document.Version = 0;
        //        document.AddPage();
        //        document.Close();
        //        document.Version.Should().BeGreaterThan(0);
        //    }
        //}
    }
}
