// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Diagnostics;
using PdfSharp.Fonts;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
#if CORE
#endif
using Xunit;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class TableTests : IDisposable
    {
        public TableTests()
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
        public void Create_Table_Cell_with_Top_Border()
        {
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
            var filename = PdfFileUtility.GetTempPdfFileName("TableTopBorder");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_2.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif
        }

        [Fact]
        public void Create_a_cloned_table()
        {
            // Create a MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a paragraph to the section.
            var paragraph = section.AddParagraph("Dummy");

            Style style = document.Styles[StyleNames.Normal]!;
            style.Font.Name = "Arial";

            var sec = document.LastSection;
            var p = sec.AddParagraph("Creating a table");
            var table = sec.AddTable();
            table.Tag = "Original";
            table.AddColumn("2cm");
            table.AddColumn("2cm");
            var row = table.AddRow();
            row[0].AddParagraph("H1");
            row[1].AddParagraph("H2");
            row.HeadingFormat = true;
            row = table.AddRow();
            row[0].AddParagraph("C1");
            row[1].AddParagraph("C2");
            sec.AddParagraph("Cloned table");
            var clone = table.Clone();
            clone.Tag = "Clone";
            sec.Add(clone);
            sec.AddParagraph("End of table test");

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("ClonedTable");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Create_a_table_with_cloned_rows()
        {
            // Create a MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a paragraph to the section.
            var paragraph = section.AddParagraph("Dummy");

            Style style = document.Styles[StyleNames.Normal]!;
            style.Font.Name = "Arial";

            var sec = document.LastSection;
            var p = sec.AddParagraph("Creating a table");
            var table = sec.AddTable();
            table.Tag = "Original";
            table.AddColumn("2cm");
            table.AddColumn("2cm");
            var row = table.AddRow();
            row[0].AddParagraph("H1");
            row[1].AddParagraph("H2");
            row.HeadingFormat = true;
            row = table.AddRow();
            row[0].AddParagraph("C1");
            row[1].AddParagraph("C2");
            row = table.Rows[1].Clone();
            table.Rows.Add(row);
            row = table.Rows[2].Clone();
            table.Rows.Add(row);
            row = table.Rows[1].Clone();
            table.Rows.Add(row);
            sec.AddParagraph("End of table test");

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("ClonedTable");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
