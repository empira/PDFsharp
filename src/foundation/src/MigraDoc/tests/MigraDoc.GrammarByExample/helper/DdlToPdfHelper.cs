// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace MigraDocTests.Helpers
{
    public class DdlToPdfHelper
    {
        DdlToPdfHelper()
        { }

        public DdlToPdfHelper(string outputDirectory)
        {
            OutputDirectory = outputDirectory;
        }

        public static string CreatePdfFromDocument(Document document, string testName, string workingDirectory)
        {
            string pdfFile = testName + ".pdf";

            // Create a renderer for PDF that uses Unicode font encoding.
            var pdfRenderer = new PdfDocumentRenderer();

            // Set the working directory so the renderer finds the referenced images.
            if (!String.IsNullOrEmpty(workingDirectory))
                pdfRenderer.WorkingDirectory = workingDirectory;

            // Set the MigraDoc document.
            pdfRenderer.Document = document;

            // Create the PDF document.
            pdfRenderer.RenderDocument();

            // Save the PDF document...
            pdfRenderer.Save(pdfFile);

            return pdfFile;
        }

        public static string CreatePdfFromMdddlFile(string mdddlPath, string testName)
        {
            string file = Path.Combine(mdddlPath, Path.GetFileNameWithoutExtension(testName) + ".mdddl");

            var document = MigraDoc.DocumentObjectModel.IO.DdlReader.DocumentFromFile(file/*, Encoding.GetEncoding(1252)*/);

            return CreatePdfFromDocument(document, testName, mdddlPath);
        }

        public static string CreatePdfFromDdlString(string ddlString, string testName, string workingDirectory)
        {
            var document = MigraDoc.DocumentObjectModel.IO.DdlReader.DocumentFromString(ddlString);

            return CreatePdfFromDocument(document, testName, workingDirectory);
        }

        public static string? FindReferenceFile(string pathReferenceSource, string testName)
        {
            string file = Path.Combine(pathReferenceSource, Path.GetFileNameWithoutExtension(testName) + ".pdf");
            return File.Exists(file) ? file : null;
        }

        public static PdfDocument CreateOrOpenResultFile(string resultFileName)
        {
            PdfDocument pdfResultDocument;
            if (File.Exists(resultFileName))
                pdfResultDocument = PdfReader.Open(resultFileName, PdfDocumentOpenMode.Modify);
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
        public static void CopyPdfPage(XPdfForm? source, XGraphics gfx, int pageNr, XRect destRect)
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
        /// </summary>
        public void AppendToResultPdf(string pdfFile, string referenceFile, int pages, uint bitmapLandscape, string testName, bool createSideBySideFile, bool createSeparateFiles)
        {
            Debug.Assert(createSideBySideFile || createSeparateFiles, "No result file will be created.");
            // Open the input files.
#if false
            PdfDocument inputDocument1 = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import);
            PdfDocument inputDocument2 = referenceFile != null ? PdfReader.Open(referenceFile, PdfDocumentOpenMode.Import) : null;

            int pages1 = inputDocument1.PageCount;
            int pages2 = inputDocument2 != null ? inputDocument2.PageCount : 0;
#else
            XPdfForm inputDocument1 = XPdfForm.FromFile(pdfFile);
            XPdfForm inputDocument2 = /*referenceFile != null ?*/ XPdfForm.FromFile(referenceFile) /*: null*/;

            int pages1 = inputDocument1.PageCount;
            int pages2 = inputDocument2?.PageCount ?? 0;
#endif
            int total = Math.Max(Math.Max(pages, pages1), pages2);

            var font = new XFont("Verdana", 8, XFontStyleEx.Bold);
            var format = new XStringFormat { Alignment = XStringAlignment.Center, LineAlignment = XLineAlignment.Far };

            if (createSeparateFiles)
            {
                if (inputDocument2 is null)
                    throw new Exception("Reference document not found.");

                // Open output file.
                string resultFileName = Path.Combine(OutputDirectory, "!!TestResult.pdf");
                var pdfResultDocument = CreateOrOpenResultFile(resultFileName);
                // Open output file for references.
                string originalFileName = Path.Combine(OutputDirectory, "!!TestResult_References.pdf");
                var pdfOriginalDocument = CreateOrOpenResultFile(originalFileName);
                PdfPage pageOrg = pdfOriginalDocument.AddPage(); // Page from original/reference PDF.
                for (int i = 0; i < total; ++i)
                {
                    bool landscape = ((1 << i) & bitmapLandscape) != 0;

                    var page = pdfResultDocument.AddPage(); // Page from generated PDF.
                    page.Orientation = landscape ? PageOrientation.Landscape : PageOrientation.Portrait;
                    var gfx = XGraphics.FromPdfPage(page);
                    var box = new XRect(0, 0, page.Width, page.Height);

                    // Copy page.
                    CopyPdfPage(inputDocument1, gfx, i + 1, box);

                    // Write document file name and page number on each page.
                    box.Inflate(0, -10);
                    gfx.DrawString(String.Format("- {2} - {0} of {1} -", i + 1, pages1, testName),
                                   font, XBrushes.Red, box, format);

                    pageOrg.Orientation = page.Orientation;
                    var gfxOrg = XGraphics.FromPdfPage(pageOrg);
                    var boxOrg = new XRect(0, 0, pageOrg.Width, pageOrg.Height);

                    // Copy page.
                    CopyPdfPage(inputDocument2, gfxOrg, i + 1, boxOrg);

                    // Write document file name and page number on each page.
                    boxOrg.Inflate(0, -10);
                    gfxOrg.DrawString(String.Format("- {0} of {1} -", i + 1, pages2),
                                      font, XBrushes.Red, boxOrg, format);
                }
                pdfOriginalDocument.Save(originalFileName);
                pdfResultDocument.Save(resultFileName);
            }

            if (createSideBySideFile)
            {
                if (inputDocument2 is null)
                    throw new Exception("Reference document not found.");

                // Open output file.
                string resultFileNameSideBySide = Path.Combine(OutputDirectory, "!!TestResult_side_by_side.pdf");
                var pdfResultDocumentSideBySide = CreateOrOpenResultFile(resultFileNameSideBySide);

                for (int i = 0; i < total; ++i)
                {
                    bool landscape = ((1 << i) & bitmapLandscape) != 0;
                    var page = pdfResultDocumentSideBySide.AddPage();
                    page.Orientation = landscape ? PageOrientation.Portrait : PageOrientation.Landscape;
                    var gfx = XGraphics.FromPdfPage(page);
                    //gfx.DrawRectangle(XBrushes.GhostWhite, new XRect(0, 0, 1000, 1000));

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
                        // Copy page
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

                        // Copy page
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
                pdfResultDocumentSideBySide.Save(resultFileNameSideBySide);
            }
        }

        public readonly string OutputDirectory = ".";
    }
}
