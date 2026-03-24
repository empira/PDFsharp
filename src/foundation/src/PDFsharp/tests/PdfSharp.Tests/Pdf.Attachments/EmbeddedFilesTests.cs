// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Pdf.Attachments;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.Pdf.Attachments
{
    [Collection("PDFsharp")]
    public class EmbeddedFilesTests : IDisposable
    {
        public EmbeddedFilesTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Get_embedded_files_low_level()
        {
            var root = IOUtility.GetAssetsPath("pdfsharp-6.x/pdfs/generated/attachments");
            Debug.Assert(root != null);

            var fileName = Path.Combine(root, "TestFile-3Attachments.pdf");
            var doc = PdfReader.Open(fileName);

            var nameDict = doc.Catalog.Names;
            var has = nameDict.HasEmbeddedFiles;
            var ef = nameDict.GetEmbeddedFiles();

            var count = ef.Elements.Count;
            count.Should().Be(1, @"there is a /Names entry.");

            var keys = ef.Names?.NamesKeys ?? throw new InvalidOperationException("No /Names entry.");
            keys.Length.Should().Be(3);
            keys[0].Should().Be("embedded_file_1");
            keys[1].Should().Be("embedded_file_2");
            keys[2].Should().Be("embedded_file_3");

            count = ef.FileCount;
            count.Should().Be(3);

            var kids = ef.Kids;
            kids.Should().BeNull();

            var names = ef.Names;
            names.Should().NotBeNull();

            var limits = ef.Limits;
            limits.Should().BeNull();

            PdfFileSpecification fileSpecification1 = ef.GetFileSpecification(0);
            PdfFileSpecification fileSpecification2 = ef.GetFileSpecification(1);
            PdfFileSpecification fileSpecification3 = ef.GetFileSpecification(2);

            var fileInfo1 = fileSpecification1.GetFileInfo();
            CheckFileInfo(fileInfo1, "embedded_file_1", "PDFsharp-128x128.png", "/image/png", 6666);


            var fileInfo2 = fileSpecification2.GetFileInfo();
            CheckFileInfo(fileInfo2, "embedded_file_2", "2nd Dummy File", "/text/plain", 19);

            var fileInfo3 = fileSpecification3.GetFileInfo();
            CheckFileInfo(fileInfo3, "embedded_file_3", "3rd Dummy File", "/text/plain", 19);
        }

        [Fact]
        public void Get_embedded_files_using_EmbeddedFilesManager()
        {
            var root = IOUtility.GetAssetsPath("pdfsharp-6.x/pdfs/generated/attachments");
            Debug.Assert(root != null);

            var fileName = Path.Combine(root, "TestFile-3Attachments.pdf");
            var doc = PdfReader.Open(fileName);

            var efm = EmbeddedFilesManager.ForDocument(doc);

            var count = efm.FileCount;
            count.Should().Be(3);

            var keys = efm.NamesKeys;
            keys.Length.Should().Be(3);
            keys[0].Should().Be("embedded_file_1");
            keys[1].Should().Be("embedded_file_2");
            keys[2].Should().Be("embedded_file_3");

            var fileInfo1 = efm.GetEmbeddedFileInfo(0);
            CheckFileInfo(fileInfo1, "embedded_file_1", "PDFsharp-128x128.png", "/image/png", 6666);

            var fileInfo2 = efm.GetEmbeddedFileInfo(1);
            CheckFileInfo(fileInfo2, "embedded_file_2", "2nd Dummy File", "/text/plain", 19);

            var fileInfo3 = efm.GetEmbeddedFileInfo(2);
            CheckFileInfo(fileInfo3, "embedded_file_3", "3rd Dummy File", "/text/plain", 19);
        }

        void CheckFileInfo(EmbeddedFileInfo fileInfo, string namesKey, string fileName, string fileType, int dataLength)
        {
            fileInfo.NamesKey.Should().Be(namesKey);
            fileInfo.FileName.Should().Be(fileName);
            fileInfo.FileType.Should().Be(fileType);
            fileInfo.Data.Length.Should().Be(dataLength);
        }
    }
}
