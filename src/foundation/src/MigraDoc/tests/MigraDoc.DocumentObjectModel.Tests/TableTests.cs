using FluentAssertions;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    public class TableTests
    {
        [Fact]
        public void Create_Hello_World_TableTests()
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
            Action rendering = pdfRenderer.RenderDocument;

            rendering.Should().Throw<InvalidOperationException>();

            //// Save the document...
            //var filename = PdfFileHelper.CreateTempFileName("HelloWorld");
            //pdfRenderer.PdfDocument.Save(filename);
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

            var table = section.AddTable();
            table.AddColumn("1cm");
            table.AddColumn("1cm");

            var row = table.AddRow();
            row[0].MergeDown = 3; // This should cause an error while rendering.
            row = table.AddRow();
            row = table.AddRow();

            return document;
        }

        [Fact]
        public void Test_MergeDown_Simple()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            var document = new Document();
            var section = document.AddSection();


            var table = section.AddTable();
            table.Borders.Width = Unit.FromPoint(1);

            table.AddColumn(Unit.FromCentimeter(5));
            table.AddColumn(Unit.FromCentimeter(10));

            var row0 = table.AddRow();
            row0[0].AddParagraph("Row 0 Cell 0 MergeDown 1");
            row0[0].MergeDown = 1;
            row0[1].AddParagraph("Row 0 Cell 1");

            var row1 = table.AddRow();
            row1[1].AddParagraph("Row 1 Cell 1");

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileHelper.CreateTempFileName("Test_MergeDown");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }

        [Fact]
        public void Test_KeepWith_MergeDown_PageBreak()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            var document = new Document();
            var section = document.AddSection();

            var p = section.AddParagraph("Paragraph to force page break. The page break would be in top of the comment line, if there was no MergeDown.");
            p.Format.SpaceAfter = Unit.FromCentimeter(23);

            var table = section.AddTable();
            table.Borders.Width = Unit.FromPoint(1);

            table.AddColumn(Unit.FromCentimeter(5));
            table.AddColumn(Unit.FromCentimeter(10));

            var headingRow = table.AddRow();
            headingRow.HeadingFormat = true;
            headingRow[0].AddParagraph("Heading 0");
            headingRow[1].AddParagraph("Heading 1");

            var subHeadingRow = table.AddRow();
            subHeadingRow.KeepWith = 1;
            subHeadingRow[1].AddParagraph("Subheading 1");

            var dataRow1 = table.AddRow();
            dataRow1[0].AddParagraph("Data 1 Cell 0 MergeDown 1");
            dataRow1[0].MergeDown = 1;
            dataRow1[1].AddParagraph("Data 1 Cell 1");
            var dataRow1CommentRow = table.AddRow();
            dataRow1CommentRow[1].AddParagraph("Comment 1 Cell 1");

            var dataRow2 = table.AddRow();
            dataRow2[0].AddParagraph("Item 2 Cell 0");
            dataRow2[1].AddParagraph("Item 2 Cell 1");

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileHelper.CreateTempFileName("Test_MergeDown");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }


        [Fact/*(Skip = "Fails - cause has to be found and fixed.")*/]
        public void Test_MergeDown_LineBreak_RowHeight()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            var document = new Document();
            var section = document.AddSection();

            var table = section.AddTable();
            table.Borders.Width = Unit.FromPoint(1);

            table.AddColumn(Unit.FromCentimeter(10));
            table.AddColumn(Unit.FromCentimeter(5));

            var row0 = table.AddRow();
            row0[0].AddParagraph("Row 0 Cell 0");
            row0[1].AddParagraph("Row 0 Cell 1");

            var row1 = table.AddRow();
            row1[0].AddParagraph("Row 1 Cell 0 MergeDown 1 with\nline break ID#1"); // The cell's content must end with "ID#1" to allow analysis of the generated PDF below.
            row1[0].MergeDown = 1;
            row1[1].AddParagraph("Row 1 Cell 1");
            var row1CommentRow = table.AddRow();
            row1CommentRow[1].AddParagraph("Comment 1 Cell 1 ID#2"); // The cell's content must end with "ID#2" to allow analysis of the generated PDF below.

            var row2 = table.AddRow();
            row2[0].AddParagraph("Row 2 Cell 0 MergeDown 1");
            row2[0].MergeDown = 1;
            row2[1].AddParagraph("Row 2 Cell 1");
            var row2CommentRow = table.AddRow();
            row2CommentRow[1].AddParagraph("Comment 2 Cell 1");

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileHelper.CreateTempFileName("Test_MergeDown_LineBreak_RowHeight");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);


            // Analyze the drawn borders in the PDF's content stream.
            var page = pdfRenderer.PdfDocument.Pages[0];
            var contentReference = (PdfReference)page.Contents.Elements.Items[0];
            var content = (PdfDictionary)contentReference.Value;
            var contentStream = content.Stream.ToString();
            var contentLines = contentStream.Split("\n");
            var lineCount = contentLines.Length;

            var lineIndex = 0;


            // Find Cell containing ID#1.
            while (lineIndex < lineCount)
            {
                if (contentLines[lineIndex].Contains("<002C002700060014>", StringComparison.Ordinal)) // Representation of "ID#1".
                    break;
                lineIndex++;
            }
            lineIndex.Should().BeLessThan(lineCount, "Representation of \"ID#1\" shall be found");

            // Check the following lines drawing the borders for the correct values.
            contentLines[++lineIndex].Should().Be("ET", "\"ID#1\" shall be the last text of the cell");

            var positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a move operator and two operands are expected");
            positionParts[2].Should().Be("m", "a move operator is expected");
            positionParts[1].Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("732.5266", "this is the value generated with the correct cell height");

            contentLines[++lineIndex].Should().EndWith(" l", "a line operator is expected");

            contentLines[++lineIndex].Should().Be("S", "stroking the path is expected");

            positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a move operator and two operands are expected");
            positionParts[2].Should().Be("m", "a move operator is expected");
            positionParts[1].Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("732.5266", "this is the value generated with the correct cell height");

            contentLines[++lineIndex].Should().EndWith(" l", "a line operator is expected");

            contentLines[++lineIndex].Should().Be("S", "stroking the path is expected");

            positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a move operator and two operands are expected");
            positionParts[2].Should().Be("m", "a move operator is expected");
            positionParts[1].Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("733.0266", "this is the value generated with the correct cell height");

            positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a line operator and two operands are expected");
            positionParts[2].Should().Be("l", "a line operator is expected");
            positionParts[1].Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("733.0266", "this is the value generated with the correct cell height");


            // Find Cell containing ID#2.
            while (lineIndex < lineCount)
            {
                if (contentLines[lineIndex].Contains("<002C002700060015>", StringComparison.Ordinal)) // Representation of "ID#2".
                    break;
                lineIndex++;
            }
            lineIndex.Should().BeLessThan(lineCount, "Representation of \"ID#2\" shall be found");

            // Check the following lines drawing the borders for the correct values.
            contentLines[++lineIndex].Should().Be("ET", "\"ID#2\" shall be the last text of the cell");

            positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a move operator and two operands are expected");
            positionParts[2].Should().Be("m", "a move operator is expected");
            positionParts[1].Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("732.5266", "this is the value generated with the correct cell height");

            contentLines[++lineIndex].Should().EndWith(" l", "a line operator is expected");

            contentLines[++lineIndex].Should().Be("S", "stroking the path is expected");

            positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a move operator and two operands are expected");
            positionParts[2].Should().Be("m", "a move operator is expected");
            positionParts[1].Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("732.5266", "this is the value generated with the correct cell height");

            contentLines[++lineIndex].Should().EndWith(" l", "a line operator is expected");

            contentLines[++lineIndex].Should().Be("S", "stroking the path is expected");

            positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a move operator and two operands are expected");
            positionParts[2].Should().Be("m", "a move operator is expected");
            positionParts[1].Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("733.0266", "this is the value generated with the correct cell height");

            positionParts = contentLines[++lineIndex].Split(" ");
            positionParts.Length.Should().Be(3, "a line operator and two operands are expected");
            positionParts[2].Should().Be("l", "a line operator is expected");
            positionParts[1].Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            positionParts[1].Should().Be("733.0266", "this is the value generated with the correct cell height");
        }

        [Fact]
        public void Test_Border_Inheritance()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            var document = new Document();
            var section = document.AddSection();

            var table = section.AddTable();
            table.Borders.Color = Colors.Gray;
            table.Borders.Width = Unit.FromPoint(1.5);

            table.AddColumn(Unit.FromCentimeter(5));
            table.AddColumn(Unit.FromCentimeter(5));
            table.AddColumn(Unit.FromCentimeter(5));

            var headingRow = table.AddRow();
            headingRow.HeadingFormat = true;
            headingRow.Shading.Color = Colors.Gray;
            headingRow.Format.Font.Color = Colors.White;
            headingRow[0].AddParagraph("Heading 0");
            headingRow[0].Borders.Right.Color = Colors.White;
            headingRow[1].AddParagraph("Heading 1");
            headingRow[1].Borders.Right.Color = Colors.White;
            headingRow[2].AddParagraph("Heading 2");

            var dataRow1 = table.AddRow();
            dataRow1[0].AddParagraph("Item 1 Cell 0");
            dataRow1[1].AddParagraph("Item 1 Cell 1");
            dataRow1[2].AddParagraph("Item 1 Cell 2");

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileHelper.CreateTempFileName("Test_Border_Inheritance");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);

            // Analyze the drawn border widths in the PDF's content stream.
            var page = pdfRenderer.PdfDocument.Pages[0];
            var contentReference = (PdfReference)page.Contents.Elements.Items[0];
            var content = (PdfDictionary)contentReference.Value;
            var contentStream = content.Stream.ToString();
            var contentLines = contentStream.Split("\n");

            // 1.5 is the desired border width. It shall be set only once.
            contentLines.Count(x => x == "1.5 w").Should().Be(1);
            // The border width in general shall be set only once - so changes of border width should not occur.
            contentLines.Count(x => x.EndsWith(" w", StringComparison.Ordinal)).Should().Be(1);
        }

        [Fact]
        public void Test_Huge_MergeDown_Cell()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            var document = new Document();
            var section = document.AddSection();

            var table = section.AddTable();
            table.Borders.Width = Unit.FromPoint(1.5);

            table.AddColumn(Unit.FromCentimeter(5));
            table.AddColumn(Unit.FromCentimeter(5));

            var headingRow = table.AddRow();
            headingRow.HeadingFormat = true;
            headingRow.Format.Font.Bold = true;
            headingRow[0].AddParagraph("Left");
            headingRow[1].AddParagraph("Right");

            var dataRow1 = table.AddRow();
            //dataRow1[0].AddParagraph("Item 1 Cell 0");
            dataRow1[0].AddParagraph("Item 1 Cell 0: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi ipsum magna, consectetur malesuada fermentum in.");
            dataRow1[0].MergeDown = 1;
            dataRow1[1].AddParagraph("Item 1 Cell 1");

            var dataRow2 = table.AddRow();
            dataRow2[0].AddParagraph("Item 2 Cell 0");
            dataRow2[1].AddParagraph("Item 2 Cell 1");

            dataRow1 = table.AddRow();
            dataRow1[0].AddParagraph("Item 1 Cell 0");
            //dataRow1[0].AddParagraph("Item 1 Cell 0: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi ipsum magna, consectetur malesuada fermentum in.");
            dataRow1[0].MergeDown = 1;
            dataRow1[1].AddParagraph("Item 1 Cell 1");

            dataRow2 = table.AddRow();
            dataRow2[0].AddParagraph("Item 2 Cell 0");
            dataRow2[1].AddParagraph("Item 2 Cell 1");

            dataRow1 = table.AddRow();
            dataRow1[0].AddParagraph("Item 1 Cell 0");
            //dataRow1[0].AddParagraph("Item 1 Cell 0: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi ipsum magna, consectetur malesuada fermentum in.");
            dataRow1[0].MergeDown = 1;
            dataRow1[1].AddParagraph("Item 1 Cell 1");

            dataRow2 = table.AddRow();
            dataRow2[0].AddParagraph("Item 2 Cell 0");
            dataRow2[1].AddParagraph("Item 2 Cell 1");
            dataRow1 = table.AddRow();
            //dataRow1[0].AddParagraph("Item 1 Cell 0");
            dataRow1[0].AddParagraph("Item 1 Cell 0: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi ipsum magna, consectetur malesuada fermentum in.");
            dataRow1[0].MergeDown = 1;
            dataRow1[1].AddParagraph("Item 1 Cell 1");

            dataRow2 = table.AddRow();
            dataRow2[0].AddParagraph("Item 2 Cell 0");
            dataRow2[1].AddParagraph("Item 2 Cell 1");
            dataRow1 = table.AddRow();
            //dataRow1[0].AddParagraph("Item 1 Cell 0");
            dataRow1[0].AddParagraph("Item 1 Cell 0: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi ipsum magna, consectetur malesuada fermentum in.");
            dataRow1[0].MergeDown = 1;
            dataRow1[1].AddParagraph("Item 1 Cell 1");

            dataRow2 = table.AddRow();
            dataRow2[0].AddParagraph("Item 2 Cell 0");
            dataRow2[1].AddParagraph("Item 2 Cell 1");
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileHelper.CreateTempFileName("Test_Huge_MergeDown_Cell");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }
    }
}
