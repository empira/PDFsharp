using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Quality;
using Xunit;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class PageSizeTests
    {
        [Fact]
        public void Test_Image_Formats()
        {
            // Create a MigraDoc document.
            var document = CreateDocument();

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Page_Formats");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        private Document CreateDocument()
        {
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            section.AddParagraph("Default section.");

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            var paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

#if DEBUG___
            for (PageFormat pf = PageFormat.Ledger; pf <= PageFormat.Ledger; pf++)
            {
                section = document.AddSection();
                section.PageSetup.PageFormat = pf;
                section.PageSetup.Orientation = Orientation.Portrait;
                section.AddParagraph($"Custom section {pf.ToString()}.");

                section = document.AddSection();
                section.PageSetup.PageFormat = pf;
                section.PageSetup.Orientation = Orientation.Landscape;
                section.AddParagraph($"Custom section {pf.ToString()} Landscape.");
            }
#endif

            for (PageFormat pf = PageFormat.A0; pf <= PageFormat.P11x17; pf++)
            {
                section = document.AddSection();
                section.PageSetup.PageFormat = pf;
                section.PageSetup.Orientation = Orientation.Portrait;
                section.AddParagraph($"Custom section {pf.ToString()}.");

                section = document.AddSection();
                section.PageSetup.PageFormat = pf;
                section.PageSetup.Orientation = Orientation.Landscape;
                section.AddParagraph($"Custom section {pf.ToString()} Landscape.");
            }

            return document;
        }
    }
}
