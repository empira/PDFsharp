// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.TestHelper;

#pragma warning disable 1591
namespace PdfSharp.Features.IO
{
    /// <summary>
    ///  Move to PDFsharp.Tests
    /// </summary>
    public class ObjectStreams
    {
        public static void ReadPdfWithObjectStreams()
        {
            //var path = PathHelper.FindPath("internal\\PDFsharp", "assets\\PDFs\\object-streams\\ObjectStreams.pdf");
            //const string path = @"C:\Users\StLa\Desktop\pdf_reference_1-7.pdf";
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"2013-samsung-electronic-report.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"2014 Global Peace Index REPORT.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"919.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"avc_fdt_201403_en.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"EYFS_framework_from_1_September_2014__with_clarification_note.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"gii-2014-v5.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"hdr14-report-en-1.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"i-9.pdf");  // Encryption not supported.
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"pdf-test.pdf");  // Owner password required.
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"PDFA-kompakt-20.pdf");  // Failed
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"PdfFafsa14-15.pdf");  // Failed
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"texlive-en.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"wdi-2014-book.pdf");  // Failed
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"wr2014_web_0.pdf");
            //string path = Path.Combine(@"D:\!StLa\empira1\edf\! PDF files with ObjStm and XRef", @"UBT-PB 625 400V50Hz mit Bypass.pdf");

            const string rootPath =
                  //@"D:\repos\emp\PDFsharp.Assets\assets\to-be-categorized";
                  @"D:\repos\emp\PDFsharp.Assets\assets\to-be-categorized";
            //@"C:\Users\StLa\Desktop\PDFsharp";

            string path = Path.Combine(rootPath, "ISO_32000-2_2020(en).pdf");
            //string path = Path.Combine(rootPath, "ISO_32000-2_2020(en)_.pdf");
            //string path = Path.Combine(rootPath, "pdf_reference_1-7.pdf");

            var files = Directory.GetFiles(rootPath);

            foreach (var file in files)
            {

                switch (Path.GetFileName(file))
                {
                    case "avc_fdt_201403_en.pdf":
                    case "i-9.pdf":
                    case "pdf-test.pdf":
                        continue;
                }
                var fileName = Path.Combine(rootPath, file);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                PdfDocument document = default!;
                for (int i = 0; i < 1; i++)
                {
                    try
                    {
                        document = PdfReader.Open(fileName, new() { });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }
                stopwatch.Stop();
                var x = stopwatch.ElapsedMilliseconds;
#if DEBUG
                Console.WriteLine($"Time: {x}ms");
                Console.WriteLine($"Try: {LexerHelper.TryCheckReferenceCount}, success: {LexerHelper.TryCheckReferenceSuccessCount}");
#endif

                //document.Info.Author = "empira";
#if true_
                var prefix = Path.GetFileNameWithoutExtension(file);
                var filename = PdfFileUtility.GetTempPdfFileName(prefix);
                document.Save(filename);
#endif
            }
        }
    }
}
