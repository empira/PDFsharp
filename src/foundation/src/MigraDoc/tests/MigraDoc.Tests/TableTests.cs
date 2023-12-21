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
    public class TableTests
    {
        [Fact]
        public void Create_Table_Cell_with_Top_Border()
        {
#if CORE
            GlobalFontSettings.FontResolver = SnippetsFontResolver.Get();
#endif

            // Create a MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a paragraph to the section.
            var paragraph = section.AddParagraph("Dummy");

            var table = section.AddTable();
            table.AddColumn("5cm");
            var row = table.AddRow();
            var cell = row[0];
            cell.AddParagraph("Cell");
            cell.Borders.Top.Color = Colors.Blue;
            //cell.Borders.Top.Width = 5;

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
    }
}
