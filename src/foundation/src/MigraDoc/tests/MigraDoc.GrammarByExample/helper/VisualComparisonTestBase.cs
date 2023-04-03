// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.GrammarByExample;
using MigraDoc.Rendering;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace GdiGrammarByExample
{
    public abstract class VisualComparisonTestBase
    {
        public string CreatePdfFromDocument(string pdfFile, Document document, string testName, string workingDirectory, InitializeDocumentCallbackType? callback = null)
        {
            // Create a renderer for PDF that uses Unicode font encoding.
            var pdfRenderer = new PdfDocumentRenderer();

            // Set the working directory so the renderer finds the referenced images.
            if (!String.IsNullOrEmpty(workingDirectory))
                pdfRenderer.WorkingDirectory = DdlGbeTestBase.WslPathHack(workingDirectory);

            callback?.Invoke(document);

            // Set the MigraDoc document.
            pdfRenderer.Document = document;

            // Create the PDF document.
            pdfRenderer.RenderDocument();

            // Save the PDF document.
            pdfRenderer.Save(DdlGbeTestBase.WslPathHack(pdfFile));

#if DEBUG____
            // Code to generate MDDDL files.
            var dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(pdfFile + ".mdddl");
            dw.WriteDocument(document);
            dw.Close(); 
#endif

#if DEBUG____
            // Code to generate RTF files.
            var rtf = new RtfDocumentRenderer();
            // Set the working directory so the renderer finds the referenced images.
            rtf.Render(document, pdfFile + ".rtf", workingDirectory);
#endif

            return pdfFile;
        }

        public delegate void InitializeDocumentCallbackType(Document document);

        public string CreatePdfFromMdddlFile(string pdfFile, string mdddlPath, string testName, InitializeDocumentCallbackType? callback = null)
        {
            string file = Path.Combine(mdddlPath, Path.GetFileNameWithoutExtension(testName) + ".mdddl");

            if (!File.Exists(file))
            {
                throw new InvalidOperationException("MDDDL file not found. \"dev\\download-assets.ps1\" must be invoked before running the GBE tests.");
            }

            var document = DdlReaderDocumentFromFile(file);

            return CreatePdfFromDocument(pdfFile, document, testName, mdddlPath, callback);
        }

        public string CreatePdfFromDdlString(string pdfFile, string ddlString, string testName, string workingDirectory, InitializeDocumentCallbackType callback = null!)
        {
            var document = DdlReader.DocumentFromString(ddlString);

            return CreatePdfFromDocument(pdfFile, document, testName, workingDirectory, callback);
        }

        Document DdlReaderDocumentFromFile(string file)
        {
            //Console.WriteLine($"DdlReaderDocumentFromFile(string {file})");
            Document document;
            DdlReader? reader = null;
            try
            {
                // The Grammar By Example files still have ANSI encoding.
                using (var streamReader = new StreamReader(DdlGbeTestBase.WslPathHack(file), Encoding.GetEncoding(1252)))
                {
                    reader = new DdlReader(streamReader); //, _errorManager);
                    document = reader.ReadDocument();
                }
            }
            finally
            {
                reader?.Close();
            }
            return document;
        }

        public string? FindReferenceFile(string pathReferenceSource, string testName)
        {
            string file = Path.Combine(pathReferenceSource, Path.GetFileNameWithoutExtension(testName) + ".pdf");
            if (File.Exists(file))
                return file;
            return null;
        }

        public PdfDocument CreateOrOpenResultFile(string resultFileName)
        {
            PdfDocument pdfResultDocument;
            if (File.Exists(DdlGbeTestBase.WslPathHack(resultFileName)))
                pdfResultDocument = PdfReader.Open(DdlGbeTestBase.WslPathHack(resultFileName), PdfDocumentOpenMode.Modify);
            else
            {
                pdfResultDocument = new PdfDocument();
#if CORE
                pdfResultDocument.Info.Title = "PDFsharp Unit Tests based on Core build";
#elif GDI
                pdfResultDocument.Info.Title = "PDFsharp Unit Tests based on GDI+";
#elif WPF
                pdfResultDocument.Info.Title = "PDFsharp Unit Tests based on WPF";
#else
                pdfResultDocument.Info.Title = "PDFsharp Unit Tests";
#warning Unsupported platform.
#endif
                pdfResultDocument.Info.Author = "Stefan Lange";
            }
            return pdfResultDocument;
        }

        /// <summary>
        /// Copy a single page given by pageNr from source to destRect in gfx.
        /// </summary>
        public void CopyPdfPage(XPdfForm? source, XGraphics gfx, int pageNr, XRect destRect)
        {
            if (source != null && pageNr <= source.PageCount)
            {
                // Set page number.
                source.PageNumber = pageNr;
                // Draw the page identified by the page number like an image.
                gfx.DrawImage(source, destRect);
            }
        }

        /// <summary>
        /// Append PDF and bitmap image to result PDF file.
        /// If createSeparateFiles is set, two files will be created. Both files should be opened in Adobe Reader simultaneously - 
        /// quickly flipping between both documents using Ctrl+F6 will reveal differences between the documents.
        /// If createSideBySideFile is set, each test file page and the page from the reference PDF will be placed on one PDF page, side by side.
        /// </summary>
        public void AppendToResultPdf(TestContext testContext, string pdfFile, string? referenceFile, int pages, uint bitmapLandscape, string testName, bool createSideBySideFile, bool createSeparateFiles)
        {
            Debug.Assert(createSideBySideFile || createSeparateFiles, "No result file fill be created.");
            // Open the input files.
            var inputDocument1 = XPdfForm.FromFile(DdlGbeTestBase.WslPathHack(pdfFile));
            var inputDocument2 = referenceFile != null ? XPdfForm.FromFile(DdlGbeTestBase.WslPathHack(referenceFile)) : null;

            int pages1 = inputDocument1.PageCount;
            int pages2 = inputDocument2?.PageCount ?? 0;

            int total = Math.Max(Math.Max(pages, pages1), pages2);

            var font = new XFont("Verdana", 8, XFontStyleEx.Bold);
            var format = XStringFormats.BottomCenter;

            if (createSeparateFiles)
            {
                // Open output file.
                var resultFileName = testContext.AddResultFileEx("!!TestResult.pdf");
                var pdfResultDocument = CreateOrOpenResultFile(resultFileName);

                // Add pdfFile pages and save.
                AddDocument(pdfResultDocument, PdfReader.Open(DdlGbeTestBase.WslPathHack(pdfFile), PdfDocumentOpenMode.Import), testName, font, format);
                pdfResultDocument.Save(DdlGbeTestBase.WslPathHack(resultFileName));


                // Open output file for references.
                var originalFileName = testContext.AddResultFileEx("!!TestResult_References.pdf");
                var pdfOriginalDocument = CreateOrOpenResultFile(DdlGbeTestBase.WslPathHack(originalFileName));

                // Add referenceFile pages and save.
                AddDocument(pdfOriginalDocument, PdfReader.Open(DdlGbeTestBase.WslPathHack(referenceFile ?? throw new ArgumentNullException(nameof(referenceFile))), PdfDocumentOpenMode.Import), testName, font, format);
                pdfOriginalDocument.Save(DdlGbeTestBase.WslPathHack(originalFileName));
            }

            if (createSideBySideFile)
            {
                if (inputDocument2 is null)
                    throw new Exception("Second file for side-by-side comparison not found.");

                // Open output file.
                string resultFileNameSideBySide = testContext.AddResultFileEx("!!TestResult_side_by_side.pdf");
                var pdfResultDocumentSideBySide = CreateOrOpenResultFile(resultFileNameSideBySide);

                for (int i = 0; i < total; ++i)
                {
                    bool landscape = ((1 << i) & bitmapLandscape) != 0;
                    var page = pdfResultDocumentSideBySide.AddPage();
                    page.Orientation = landscape ? PageOrientation.Portrait : PageOrientation.Landscape;
                    var gfx = XGraphics.FromPdfPage(page);

                    double width = page.Width;
                    double height = page.Height;
                    if (landscape)
                    {
                        // Landscape.
                        var box = new XRect(0, 0, width, height / 2);

                        // Copy page.
                        CopyPdfPage(inputDocument1, gfx, i + 1, box);

                        // Write document file name and page number on each page.
                        box.Inflate(0, -10);
                        gfx.DrawString(FormattableString.Invariant($"- {testName} - {i + 1} of {pages1} -"),
                                       font, XBrushes.Red, box, format);

                        box = new XRect(0, height / 2, width, height / 2);
                        // Copy page.
                        CopyPdfPage(inputDocument2, gfx, i + 1, box);

                        // Write document file name and page number on each page.
                        box.Inflate(0, -10);
                        gfx.DrawString(FormattableString.Invariant($"- {i + 1} of {pages2} -"),
                                       font, XBrushes.Red, box, format);
                    }
                    else
                    {
                        // Portrait.
                        var box = new XRect(0, 0, width / 2, height);

                        // Copy page.
                        CopyPdfPage(inputDocument1, gfx, i + 1, box);

                        // Write document file name and page number on each page.
                        box.Inflate(0, -10);
                        gfx.DrawString(FormattableString.Invariant($"- {testName} - {i + 1} of {pages1} -"),
                                       font, XBrushes.Red, box, format);

                        box = new XRect(width / 2, 0, width / 2, height);

                        // Copy page.
                        CopyPdfPage(inputDocument2, gfx, i + 1, box);

                        // Write document file name and page number on each page.
                        box.Inflate(0, -10);
                        gfx.DrawString(FormattableString.Invariant($"- {i + 1} of {pages2} -"),
                                       font, XBrushes.Red, box, format);
                    }
                }
                pdfResultDocumentSideBySide.Save(DdlGbeTestBase.WslPathHack(resultFileNameSideBySide));
            }
        }

        void AddDocument(PdfDocument resultPdf, PdfDocument sourcePdf, string testName, XFont footerFont, XStringFormat footerFormat)
        {
            var resultPagesOffset = resultPdf.PageCount;
            // MAOS4STLA: There are Assertions at PdfPages.InsertRange(). Is there something to do?
            resultPdf.Pages.InsertRange(resultPagesOffset, sourcePdf);
            AddFooter(resultPdf, resultPagesOffset, sourcePdf.PageCount, testName, footerFont, footerFormat);
        }

        void AddFooter(PdfDocument resultPdf, int resultPdfPagesOffset, int sourcePdfPageCount, string testName, XFont font, XStringFormat format)
        {
            for (var i = resultPdfPagesOffset; i < resultPdfPagesOffset + sourcePdfPageCount; i++)
            {
                var page = resultPdf.Pages[i];
                var gfx = XGraphics.FromPdfPage(page);
                var box = new XRect(0, 0, page.Width, page.Height);

                // Write document file name and page number on each page.
                box.Inflate(0, -10);
                gfx.DrawString(String.Format("- {2} - {0} of {1} -", i - resultPdfPagesOffset + 1, sourcePdfPageCount, testName),
                               font, XBrushes.Red, box, format);
            }
        }
    }
}
