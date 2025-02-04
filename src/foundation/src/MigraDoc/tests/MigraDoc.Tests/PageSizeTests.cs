using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Quality;
#if CORE
#endif
using Xunit;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class PageSizeTests : IDisposable
    {
        public PageSizeTests()
        {
            PdfSharpCore.ResetAll();
#if CORE
            GlobalFontSettings.FontResolver = new UnitTestFontResolver();
#endif
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Test_Page_Formats()
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
            var text = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore " +
                       "et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. " +
                       "Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, " +
                       "consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, " +
                       "sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum.";

            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            section.AddParagraph("Default section.", StyleNames.Heading1);
            section.AddParagraph(text);

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            var paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // Create the primary header.
            var header = section.Headers.Primary;

            // Add content to header.
            paragraph = header.AddParagraph("Header");
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
                section.AddParagraph($"Custom section {pf.ToString()}.", StyleNames.Heading1);
                section.AddParagraph(text);

                section = document.AddSection();
                section.PageSetup.PageFormat = pf;
                section.PageSetup.Orientation = Orientation.Landscape;
                section.AddParagraph($"Custom section {pf.ToString()} Landscape.", StyleNames.Heading1);
                var pText = section.AddParagraph(text);
                if (pf == PageFormat.A0)
                    pText.AddText(text);
            }

            section = document.AddSection();
            section.PageSetup.PageWidth = Unit.FromCentimeter(15);
            section.PageSetup.PageHeight = Unit.FromCentimeter(20);
            section.AddParagraph("Custom section user defined 15 x 20 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageWidth = Unit.FromCentimeter(20);
            section.PageSetup.PageHeight = Unit.FromCentimeter(15);
            section.AddParagraph("Custom section user defined 20 x 15 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            return document;
        }

        [Fact]
        public void Test_Page_Formats_Inheritance()
        {
            var text = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore " +
                       "et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. " +
                       "Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, " +
                       "consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, " +
                       "sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum.";

            // Create a MigraDoc document.
            var document = new Document();

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.PageWidth = Unit.FromCentimeter(20);
            section.PageSetup.PageHeight = Unit.FromCentimeter(15);
            section.AddParagraph("Custom section user defined 20 x 15 cm (width and height have priority before format).", StyleNames.Heading1);
            section.AddParagraph(text);

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            var paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // Create the primary header.
            var header = section.Headers.Primary;

            // Add content to header.
            paragraph = header.AddParagraph("Header");
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            section = document.AddSection();
            section.PageSetup.PageHeight = Unit.FromCentimeter(18);
            section.AddParagraph("Custom section user defined 20 (inherited) x 18 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageWidth = Unit.FromCentimeter(25);
            section.AddParagraph("Custom section user defined 25 x 18 (inherited) cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageWidth = Unit.FromCentimeter(15);
            section.AddParagraph("Custom section user defined 15 x 18 (inherited) cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageWidth = Unit.FromCentimeter(30);
            section.AddParagraph("Custom section user defined 30 x 18 (inherited) cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageHeight = Unit.FromCentimeter(40);
            section.AddParagraph("Custom section user defined 30 (inherited) x 40 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.AddParagraph("Custom section Format A4, portrait (inherited).", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageWidth = Unit.FromCentimeter(30);
            section.AddParagraph("Custom section user defined 30 x 29.7 (inherited) cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.AddParagraph("Custom section Format A4, landscape (inherited).", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.Orientation = Orientation.Portrait;
            section.AddParagraph("Custom section Format A4 (inherited), portrait.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageHeight = Unit.FromCentimeter(30);
            section.AddParagraph("Custom section user defined 21 (inherited) x 30 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.AddParagraph("Custom section Format A4, portrait (inherited).", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.Orientation = Orientation.Landscape;
            section.AddParagraph("Custom section Format A4 (inherited), landscape.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageHeight = Unit.FromCentimeter(15);
            section.AddParagraph("Custom section user defined 29.7 (inherited) x 15 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageWidth = Unit.FromCentimeter(15);
            section.AddParagraph("Custom section user defined 15 x 15 (inherited) cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.Orientation = Orientation.Landscape;
            section.AddParagraph("Custom section user defined 15 (inherited) x 15 (inherited) cm, landscape rejected.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.AddParagraph("Custom section Format A4, portrait (inherited (square is portrait by definition)).", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A3;
            section.PageSetup.PageHeight = Unit.FromCentimeter(20);
            section.AddParagraph("Custom section Format 29.7 (calculated by A3, portrait (inherited)) x 20 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A5;
            section.PageSetup.PageHeight = Unit.FromCentimeter(30);
            section.AddParagraph("Custom section Format 21 (calculated by A5, landscape (inherited)) x 30 cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A3;
            section.PageSetup.PageWidth = Unit.FromCentimeter(50);
            section.AddParagraph("Custom section Format 50 x 42 (calculated by A3, portrait (inherited)) cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A5;
            section.PageSetup.PageWidth = Unit.FromCentimeter(10);
            section.AddParagraph("Custom section Format 10 x 14.85 (calculated by A5, landscape (inherited)) cm.", StyleNames.Heading1);
            section.AddParagraph(text);

            section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.AddParagraph("Custom section Format A4, portrait (inherited).", StyleNames.Heading1);
            section.AddParagraph(text);


            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Page_Formats_Inheritance");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
