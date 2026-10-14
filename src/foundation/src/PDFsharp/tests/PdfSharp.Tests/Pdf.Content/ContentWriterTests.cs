// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using System.Text;
using Xunit;

namespace PdfSharp.Tests.Pdf.Content
{
    [Collection("PDFsharp")]
    public class ContentWriterTests : IDisposable
    {
        readonly string _tempRoot = PdfFileUtility.GetUnitTestPath(typeof(ContentWriterTests));

        public ContentWriterTests()
        {
            PdfSharpCore.ResetAll();
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [SkippableFact]
        public void Test_one_extracted_page()
        {
            var assets = IOUtility.GetAssetsPath();
            var file = @"D:\repos\empira\PDFsharp\assets\StLa\content\word\Hallo Welt_.pdf";
            Skip.If(!File.Exists(file));

            var name = "PDF-Reference-1.7-Page-1";
            name = "Hallo-2";

            // 1. Read the file, parse contents, write contents.
            var original = PdfReader.Open(file, PdfDocumentOpenMode.Import);
            var doc = new PdfDocument();
            doc.Pages.Add(original.Pages[0]);

            var p1 = doc.Pages[0];
            var cont = p1.Elements.GetDictionary(PdfPage.Keys.Contents);
            cont?.Stream?.TryUncompress();


            var filename = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + name);
            doc.Save(filename);

            //doc = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
            doc = PdfReader.Open(file, PdfDocumentOpenMode.Modify);

            foreach (var page in doc.Pages)
            {
                var newContent = new CSequence();
                var contents = ContentReader.ReadContent(page);
                foreach (var item in contents)
                {
                    newContent.Add(item);
                }
                page.Contents.ReplaceContent(newContent);
            }

            //var filename = PdfFileUtility.GetTempPdfFullFileName("unittest/contentwritertests/read_write_compare");

            filename = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + name + "_2nd");

            // Save the document…
            doc.Save(filename);

            return;

            //// … and start a viewer.
            //PdfFileUtility.ShowDocumentIfDebugging(filename);

            //// 2. Read the file just written, parse contents, write contents.
            //var doc2 = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
            //foreach (var page in doc2.Pages)
            //{
            //    var newContent = new CSequence();
            //    var contents = ContentReader.ReadContent(page);
            //    foreach (var item in contents)
            //    {
            //        newContent.Add(item);
            //    }
            //    page.Contents.ReplaceContent(newContent);
            //}

            //var filename2 = filename.Replace(name, name + "_2nd");
            //doc2.Save(filename2);

            //doc.Pages.Count.Should().Be(doc2.Pages.Count);
            //int pages = doc.Pages.Count;

            //var xformOri = XPdfForm.FromFile(file);
            //var xformWritten = XPdfForm.FromFile(filename2);

            //// 3. Read the file just written, parse contents, write contents.
            //var docCompare = new PdfDocument();
            //for (int idx = 0; idx < pages; ++idx)
            //{
            //    // Add a new page to the output document
            //    PdfPage pageCompare = docCompare.AddPage();
            //    pageCompare.Orientation = PageOrientation.Landscape;
            //    double width = pageCompare.Width.Point;
            //    double height = pageCompare.Height.Point;

            //    var gfx = XGraphics.FromPdfPage(pageCompare);

            //    xformOri.PageNumber = idx + 1;
            //    var box = new XRect(0, 0, width / 2, height);
            //    gfx.DrawImage(xformOri, box);

            //    xformWritten.PageNumber = idx + 1;
            //    box = new XRect(width / 2, 0, width / 2, height);
            //    gfx.DrawImage(xformWritten, box);
            //}

            //var filename3 = filename2.Replace("_2nd", "_compare");
            //docCompare.Save(filename3);
            //PdfFileUtility.ShowDocumentIfDebugging(filename3);
        }


        [SkippableTheory]
        ////// Crashes: [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\CPP 1.10\Attributes-Border.pdf")]
        ////// Crashes: [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\CPP 1.10\Attributes-Color.pdf")]
        ////// Crashes: [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\CPP 1.10\Attributes-LineAndFillFormat.pdf")]
        ////// Crashes: [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\CPP 1.10\Attributes-Shading.pdf")]
        ////// Crashes: [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\CPP 1.10\Attributes-Units.pdf")]
        [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.30\Attributes-Border.pdf")]
        [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.30\Attributes-Color.pdf")]
        [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.30\Attributes-LineAndFillFormat.pdf")]
        [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.30\Attributes-Shading.pdf")]
        [InlineData(@"$assets$\archives\grammar-by-example\GBE\ReferencePDFs\GDI 1.30\Attributes-Units.pdf")]
        [InlineData(@"C:\Users\StLa\Desktop\PDFsharp\PDF-References\pdfreference1.7.pdf")]
        public void Read_Write_Compare_Sample_Files(string file0)
        {
            TestConfig.CheckManually(new(2025, 11, 9));

            var assets = IOUtility.GetAssetsPath();
            var file = Environment.ExpandEnvironmentVariables(file0.Replace("$assets$", assets));
            if (!File.Exists(file))
            {
                Skip.If(true);
            }
            var name = Path.GetFileNameWithoutExtension(file);

            // 1. Read the file, parse contents, write contents.
            var doc = PdfReader.Open(file, PdfDocumentOpenMode.Modify);
            foreach (var page in doc.Pages)
            {
                var newContent = new CSequence();
                var contents = ContentReader.ReadContent(page);
                foreach (var item in contents)
                {
                    newContent.Add(item);
                }
                page.Contents.ReplaceContent(newContent);
            }

            //var filename = PdfFileUtility.GetTempPdfFullFileName("unittest/contentwritertests/read_write_compare");

            var filename = PdfFileUtility.GetTempPdfFullFileName(_tempRoot + name);

            // Save the document…
            doc.Save(filename);
            //// … and start a viewer.
            //PdfFileUtility.ShowDocumentIfDebugging(filename);

            // 2. Read the file just written, parse contents, write contents.
            var doc2 = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
            foreach (var page in doc2.Pages)
            {
                var newContent = new CSequence();
                var contents = ContentReader.ReadContent(page);
                foreach (var item in contents)
                {
                    newContent.Add(item);
                }
                page.Contents.ReplaceContent(newContent);
            }

            var filename2 = filename.Replace(name, name + "_2nd");
            doc2.Save(filename2);

            doc.Pages.Count.Should().Be(doc2.Pages.Count);
            int pages = doc.Pages.Count;

            var xformOri = XPdfForm.FromFile(file);
            var xformWritten = XPdfForm.FromFile(filename2);

            // 3. Read the file just written, parse contents, write contents.
            var docCompare = new PdfDocument();
            for (int idx = 0; idx < pages; ++idx)
            {
                // Add a new page to the output document
                PdfPage pageCompare = docCompare.AddPage();
                pageCompare.Orientation = PageOrientation.Landscape;
                double width = pageCompare.Width.Point;
                double height = pageCompare.Height.Point;

                var gfx = XGraphics.FromPdfPage(pageCompare);

                xformOri.PageNumber = idx + 1;
                var box = new XRect(0, 0, width / 2, height);
                gfx.DrawImage(xformOri, box);

                xformWritten.PageNumber = idx + 1;
                box = new XRect(width / 2, 0, width / 2, height);
                gfx.DrawImage(xformWritten, box);
            }

            var filename3 = filename2.Replace("_2nd", "_compare");
            docCompare.Save(filename3);
            PdfFileUtility.ShowDocumentIfDebugging(filename3);
        }

        [Fact]
        public void Create_simple_PDF()
        {
            var doc = new PdfDocument();
            var page = doc.AddPage();

            var newContent = new CSequence();
            var comment = new CComment
            {
                Text = "Begin page content"
            };
            newContent.Add(comment);

            // TODO Dies ist kein sinnvoller Befehl, sondern nur ein Writer-Test.
            var @operator = OpCodes.OperatorFromName("Tj");
            var array = new CArray();
            var cint = new CInteger(1);
            array.Add(cint);
            var creal = new CReal(2);
            array.Add(creal);
            creal = new CReal(3);
            array.Add(creal);

            // This line adds 3 operands: the array members, not the CArray itself.
            @operator.Operands.Add(array);

            // This line adds one operand, the CArray.
            //@operator.Operands.Add((CObject)array);
            newContent.Add(@operator);

            comment = new CComment
            {
                Text = "End page content"
            };
            newContent.Add(comment);
            page.Contents.ReplaceContent(newContent);

            var filename = PdfFileUtility.GetTempPdfFullFileName("unittest/contentwritertests/create_simple_PDF");

            // Save the document…
            doc.Save(filename);
            // … and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            {
                doc = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
                var pages = doc.Pages.Count;
                pages.Should().Be(1);

                // Create a PDF with readable contents.
                doc.Options.Layout = PdfWriterLayout.Verbose;
                doc.Options.CompressContentStreams = false;

                foreach (var page2 in doc.Pages)
                {
                    var content = new CSequence();
                    // Clone page contents.
                    foreach (var item in ContentReader.ReadContent(page))
                        content.Add(item);
                    // Force serializer to write contents.
                    page.Contents.ReplaceContent(content);
                }

                var filename2 = filename.Replace(".pdf", "_.pdf");
                doc.Save(filename2);
            }
        }

        [Fact]
        public void Modify_HelloWorld()
        {
            const int requiredAssets = 1032;
            IOUtility.EnsureAssetsVersion(requiredAssets);

            const string pdf = @"archives/samples-1.5/PDFs/HelloWorld.pdf";
            var testFile = IOUtility.GetAssetsPath(pdf)!;

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Modify);

            doc.PageCount.Should().Be(1);
            doc.Pages.Count.Should().Be(1);
            var page = doc.Pages[0];
            var contents = ContentReader.ReadContent(page);
            int comments = 0, names = 0, operators = 0, sequences = 0, strings = 0, arrays = 0, integers = 0, reals = 0;
            var sb = new StringBuilder();
            //int text = 0;
            var newContent = new CSequence();
            int idx = 0;
            foreach (var item in contents)
            {
                string info = item.GetType().Name switch
                {
                    nameof(CComment) => $"Comment {++comments}",
                    nameof(CName) => $"Name {++names}",
                    nameof(COperator) => $"Operator {++operators}: {((COperator)item).Name} with {((COperator)item).Operands.Count} operands",
                    nameof(CSequence) => $"Sequence {++sequences}",
                    nameof(CString) => $"String {++strings}",
                    nameof(CArray) => $"Array {++arrays}",
                    nameof(CInteger) => $"Integer {++integers}",
                    nameof(CReal) => $"Real {++reals}",
                    _ => throw new NotImplementedException($"Type {item.GetType().Name} was unexpected.")
                };
                sb.AppendLine(info);

                if (item is COperator @operator)
                {
                    if (@operator.OpCode.OpCodeName == OpCodeName.Tj)
                    {
                        foreach (var op in @operator.Operands)
                        {
                            if (op is CString @string)
                            {
                                ++idx;
                            }
                        }
                        newContent.Add(item);
                    }
                    else
                    {
                        newContent.Add(item);
                    }
                }
                else
                {
                    newContent.Add(item);
                }
            }
            var infos = sb.ToString();
            comments.Should().Be(0);
            names.Should().Be(0);
            operators.Should().Be(12);
            sequences.Should().Be(0);
            strings.Should().Be(0);
            arrays.Should().Be(0);
            integers.Should().Be(0);
            reals.Should().Be(0);

            page.Contents.ReplaceContent(newContent);
            var filename = Path.GetFileName(pdf).Replace(".pdf", "_.pdf");
            var path = IOUtility.GetTempPath("unittest/contentwritertests")!;
            path = Path.Combine(path, filename);

            // Save the document…
            doc.Save(path);
            // … and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(path);
        }
    }
}
