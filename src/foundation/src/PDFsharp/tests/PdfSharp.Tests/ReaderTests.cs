using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Xunit;

namespace PdfSharp.Tests
{
    public class ReaderTests
    {
        [Fact]
        public void Read_empty_file()
        {
            var data = Array.Empty<byte>();
            using var stream = new MemoryStream(data);

            // A file with 0 bytes is not valid and an exception should occur.
            // ReSharper disable once AccessToDisposedClosure
            Action open = () => PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
            open.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Read_invalid_file()
        {
            var data = new byte[32];
            for(int i = 0; i < data.Length;i++)
                data[i] = (byte)(33 + i);

            using var stream = new MemoryStream(data);

            // A file with 32 random bytes is not valid and an exception should occur.
            // ReSharper disable once AccessToDisposedClosure
            Action open = () => PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
            open.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Write_and_read_test()
        {
            var doc = new PdfDocument();
            doc.AddPage();
            doc.Info.Author = "empira";
            doc.Info.Title = "Test";
            using var stream = new MemoryStream();
            doc.Save(stream, false);
            stream.Position = 0;
            stream.Length.Should().BeGreaterThan(0);

            var doc2 = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);

            using var stream2 = new MemoryStream();
            doc2.Save(stream2, false);
            stream2.Position = 0;
            stream2.Length.Should().BeGreaterThan(0);

            var doc3 = PdfReader.Open(stream2, PdfDocumentOpenMode.Modify);

            using var stream3 = new MemoryStream();
            doc3.Save(stream3, false);
            stream3.Position = 0;
            stream3.Length.Should().BeGreaterThan(0);

            doc3.Info.Author.Should().Be("empira");
            doc3.Info.Title.Should().Be("Test");
        }
    }
}
