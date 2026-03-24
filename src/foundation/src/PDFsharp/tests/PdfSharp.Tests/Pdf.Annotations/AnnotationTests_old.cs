// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace PdfSharp.Tests.Pdf
{
    [Collection("PDFsharp")]
    public class AnnotationTests
    {
        const string TempRoot = "unittests/annotations/";

        /// <summary>
        /// Tests the test.
        /// </summary>
        [SkippableFact]
        public void Test_all_annotations()
        {
            string src = @"C:\Users\StLa\Desktop\PDFsharp\ISO_32000-2_2020(en)_.pdf";

            if (!File.Exists(src))
            {
                Skip.If(true);
            }

            var doc = PdfReader.Open(src, PdfDocumentOpenMode.Modify);

            var pageCount = doc.PageCount;
            int annotCount = 0;
            for (int idx = 0; idx < pageCount; idx++)
            {
                var page = doc.Pages[idx];
                if (page.HasAnnotations)
                {
                    var annots = page.Annotations;
                    var count = annots.Count;

                    annotCount += count;
                }
            }

            ////C1.CreateAcroFormObjects(doc);
            ////C1.CreateAnnotationObjects(doc);

            ////var fields = C2.CreateFieldList(doc);

            //var objs = doc.Internals.GetAllObjects();
            //for (int idx = 0; idx < objs.Length; idx++)
            //{
            //    if (objs[idx] is PdfDictionary { Stream: not null } dict)
            //    {
            //        dict.Stream.TryUncompress();
            //    }

            var dest = PdfFileUtility.GetTempPdfFullFileName(TempRoot + nameof(Test_all_annotations));
            doc.Options.Layout = PdfWriterLayout.Verbose;
            doc.Save(dest);
        }

        [Fact]
        public void Exception_with_duplicate_annotations()
        {
            var document1 = new PdfDocument();
            var page1 = document1.AddPage();
            page1.AddDocumentLink(new(new XRect(new XSize(1, 1))), 1);
            var stream1 = new MemoryStream();
            document1.Save(stream1, false);
            stream1.Position = 0;

            var document2 = new PdfDocument();
            var page2 = document2.AddPage();
            page2.AddDocumentLink(new(new XRect(new XSize(1, 1))), 1);
            var stream2 = new MemoryStream();
            document2.Save(stream2, false);
            stream2.Position = 0;

            var documentInput1 = PdfReader.Open(stream1, PdfDocumentOpenMode.Import);
            var documentInput2 = PdfReader.Open(stream2, PdfDocumentOpenMode.Import);

            var documentMerged = new PdfDocument();
            var resultPagesOffset = documentMerged.PageCount;
            documentMerged.Pages.InsertRange(resultPagesOffset, documentInput1);
            resultPagesOffset = documentMerged.PageCount;
            documentMerged.Pages.InsertRange(resultPagesOffset, documentInput2);
            documentMerged.Pages.Count.Should().Be(2);
        }
    }
}
