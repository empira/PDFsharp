// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class WriterTests
    {

#if true
        [Fact]
        public void Test_issues()
        {
            var document = CreateDocument("CompactLayout-IssuesXXX");
            //var dict = new PdfDictionary();
            document.Options.Layout = PdfWriterLayout.Compact;

            var catalog = document.Catalog;

            var dict = new PdfDictionary(document/*, true*/);
            document.Internals.AddObject(dict);
            var bytes = PdfEncoders.RawEncoding.GetBytes("ABC");
            dict.CreateStream(bytes);

            catalog.Elements.Add("/TestStream", dict);

            Save(document);
            //Reload();
            //ReloadedDocument.Options.Layout = PdfWriterLayout.Compact;
            //Resave();

            //CreateDocument("CompactLayout-Issues");
        }
        protected PdfDocument CreateDocument(string name)
        {
            var _document = PdfDocUtility.CreateNewPdfDocument(name);
            _document.Info.Title = name;
            var _page = _document.AddPage();

            return _document;
        }
        protected string Save(PdfDocument document, string? filename = null)
        {
            filename ??= document.Info.Title;
            var _fullFilename = PdfFileUtility.GetTempPdfFullFileName("Pdf-ObjectModel/" + filename);
            //Document.Options.Layout = PdfWriterLayout.Compact;
            document.Save(_fullFilename);
            return _fullFilename;
        }
#endif


        [Fact]
        public void Write_import_file()
        {
            var testFile = IOUtility.GetAssetsPath("archives/samples-1.5/PDFs/SomeLayout.pdf")!;

            var filename = PdfFileUtility.GetTempPdfFileName("ImportTest");

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(filename);
            save.Should().Throw<InvalidOperationException>();
        }
    }
}
