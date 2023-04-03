// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace MigraDoc.Tests
{
    public class RtfRendererTests
    {
        [Fact]
        public void Create_Hello_World_RtfRendererTests()
        {
            // TODO Register encoding here or in RtfDocumentRenderer?
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            // Create a MigraDoc document.
            var document = CreateDocument();

            // ----- Unicode encoding in MigraDoc is demonstrated here. -----

            // A flag indicating whether to create a Unicode PDF or a WinAnsi PDF file.
            // This setting applies to all fonts used in the PDF document.
            // This setting has no effect on the RTF renderer.
            //const bool unicode = false;

            // Create a renderer for the MigraDoc document.
            //var pdfRenderer = new PdfDocumentRenderer(unicode)
            //{
            //    // Associate the MigraDoc document with a renderer.
            //    Document = document
            //};

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new RtfDocumentRenderer();

            // Save the document...
            var filename = PdfFileHelper.CreateTempFileName("HelloWorld");

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_0.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            // Layout and render document to PDF.
            pdfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);

#if DEBUG___
            dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_1.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            //// ...and start a viewer.
            //PdfFileHelper.StartPdfViewerIfDebugging(filename);

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

#if true
            // Add a simple table.
            var table = section.AddTable();
            table.Borders.Visible = true;
            table.Borders.Width = 0;
            table.Borders.Color = Colors.Blue;
            table.Columns.AddColumn().Width = "2cm";
            table.Columns.AddColumn().Width = "4cm";
            table.Columns.AddColumn().Width = "3cm";
            var brd = table.Columns[1].Borders;
            brd.Left.Width = 0;
            brd.Right.Width = 0;
            var row = table.AddRow();
            row.Format.Shading.Color = Colors.LightPink;
            row.HeadingFormat = true;
            row[0].AddParagraph("Left");
            row[1].AddParagraph("Center");
            row[2].AddParagraph("Right");
            AddRowBorder(row);

            row = table.AddRow();
            row.Format.Shading.Color = Colors.LightYellow;
            row[0].AddParagraph("Lorem");
            row[1].AddParagraph("Ipsum");
            row[2].AddParagraph("Foo or bar");
            AddRowBorder(row);

            row = table.AddRow();
            row.Format.Shading.Color = Colors.LightGoldenrodYellow;
            row[0].AddParagraph("More lorem");
            row[1].AddParagraph("More ipsum");
            row[2].AddParagraph("More foo or bar");
#endif

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            return document;
        }

        static void AddRowBorder(Row row)
        {
#if true
            row.Borders.Bottom.Width = 1;
            row.Borders.Bottom.Color = Colors.Blue;

            //foreach (Cell cell in row.Cells)
            //{
            //    cell.Borders.Visible = true;
            //    cell.Borders.Bottom.Visible = true;
            //    cell.Borders.Bottom.Width = 2;
            //    cell.Borders.Bottom.Color = Colors.BlueViolet;
            //}
#else
            row.Borders.Visible = true;
            row.Borders.Bottom.Visible = true;
            row.Borders.Bottom.Width = 2;
            row.Borders.Bottom.Color = Colors.BlueViolet;
#endif
        }
    }
}
