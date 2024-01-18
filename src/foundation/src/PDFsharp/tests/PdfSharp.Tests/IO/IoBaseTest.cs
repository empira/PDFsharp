using FluentAssertions;
using System.Text;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.IO
{
    public abstract class IoBaseTest
    {
        private readonly string _rootPath = AppDomain.CurrentDomain.BaseDirectory;
        private const string _outputDirName = "Out";

        public void CanReadPdf(string fileName)
        {
            var path = GetOutFilePath(fileName);
            using var fs = File.OpenRead(path);
            var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);
            var info = inputDocument.Info;
            info.Should().NotBeNullOrEmpty();
        }

        protected void SaveDocument(PdfDocument document, string name)
        {
            var outFilePath = GetOutFilePath(name);
            var dir = Path.GetDirectoryName(outFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            document.Save(outFilePath);
        }

        protected void ValidateFileIsPdf(string v)
        {
            var path = GetOutFilePath(v);
            var fileExists = File.Exists(path);
            fileExists.Should().BeTrue();

            var fi = new FileInfo(path);
            fi.Length.Should().BeGreaterThan(1);

            using var stream = File.OpenRead(path);
            ReadStreamAndVerifyPdfHeaderSignature(stream);
        }

        private static void ReadStreamAndVerifyPdfHeaderSignature(Stream stream)
        {
            var readBuffer = new byte[5];
            var pdfSignature = Encoding.ASCII.GetBytes("%PDF-"); // PDF must start with %PDF-

            stream.Read(readBuffer, 0, readBuffer.Length);
            readBuffer.Should().Equal(pdfSignature);
        }

        protected void ValidateTargetAvailable(string file)
        {
            var path = GetOutFilePath(file);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Exists(path).Should().BeFalse();
        }

        protected string GetOutFilePath(string name)
        {
            return Path.Combine(_rootPath, _outputDirName, name);
        }
    }
}