// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
#if CORE
using PdfSharp.Snippets.Font;
#endif
using PdfSharp.TestHelper;
using Xunit;

namespace MigraDoc.Tests
{
    [Collection("MGD")]
    public class MultipleFooters
    {
        [Fact]
        public void Create_Multiple_Footers()
        {
#if CORE
            GlobalFontSettings.FontResolver = SnippetsFontResolver.Get();
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

            // Add a paragraph to the section.
            var paragraph = section.AddParagraph();

            // Set font color.
            //paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Color = Colors.DarkBlue;

            // Add some text to the paragraph.
            paragraph.AddFormattedText("Hello, World!", TextFormat.Bold);

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            paragraph = footer.AddParagraph();
            paragraph.AddText("Footer 1");
            paragraph.Format.Alignment = ParagraphAlignment.Left;

            paragraph = footer.AddParagraph();
            paragraph.AddText("Footer 2");
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            paragraph = footer.AddParagraph();
            paragraph.AddText("Footer 3");
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            paragraph = footer.AddParagraph();
            paragraph.AddText("          Footer 4");
            paragraph.Format.Alignment = ParagraphAlignment.Left;

            return document;
        }
    }
}
