// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace MigraDoc.Tests
{
    [Collection("MGD")]
    public class SpaceBeforeTests
    {
        [Fact]
        public void Tests_for_SpaceBefore()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            // Create a MigraDoc document.
            var document = CreateDocument();

            // ----- Unicode encoding in MigraDoc is demonstrated here. -----

            //// A flag indicating whether to create a Unicode PDF or a WinAnsi PDF file.
            //// This setting applies to all fonts used in the PDF document.
            //// This setting has no effect on the RTF renderer.
            //const bool unicode = false;

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileHelper.CreateTempFileName("HelloWorld");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_2.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif
        }

        /// <summary>
        /// Creates an absolutely minimalistic document.
        /// </summary>
        static Document CreateDocument()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a dummy paragraph to the section.
            var paragraph = section.AddParagraph();
            paragraph.Format.LineSpacingRule = LineSpacingRule.Exactly;
            paragraph.Format.LineSpacing = 0;

            // Add a paragraph to the section.
            paragraph = section.AddParagraph();

            // SpaceBefore is ignored for the first paragraph on a page.
            // This is the second paragraph on the page because we added an empty dummy paragraph, so SpaceBefore is effective here.
            // There is a huge header on the first page and we need some space to prevent overlapping.
            paragraph.Format.SpaceBefore = 144; // Two inches.

            // Set font color.
            //paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Color = Colors.DarkBlue;

            // Add some text to the paragraph.
            paragraph.AddFormattedText("Hello, World!", TextFormat.Bold);

            // Create the primary header.
            var header = section.Headers.FirstPage;
            section.PageSetup.DifferentFirstPageHeaderFooter = true;

            // Add content to footer.
            paragraph = header.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            for (int i = 1; i <= 10; ++i)
            {
                paragraph = header.AddParagraph("Paragraph " + i);
                paragraph.Format.Alignment = ParagraphAlignment.Center;
            }

            return document;
        }
    }
}
