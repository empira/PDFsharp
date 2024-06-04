// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using MigraDoc.Rendering;
using Xunit;
using FluentAssertions;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class TableTests
    {
        [Fact]
        public void Create_Hello_World_TableTests()
        {
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
            //var filename = PdfFileUtility.GetTempPdfFileName("HelloWorld");
            //pdfRenderer.PdfDocument.Save(filename);
            //// ...and start a viewer.
            //PdfFileUtility.ShowDocumentIfDebugging(filename);
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
            table.AddRow();
            table.AddRow();

            return document;
        }

        PdfDocumentRenderer CreateReadablePdfDocumentRenderer(Document document)
        {
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.PdfDocument.Options.CompressContentStreams = false;
            return pdfRenderer;
        }

        [Fact]
        public void Test_MergeDown_Simple()
        {
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

            var pdfRenderer = CreateReadablePdfDocumentRenderer(document);
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_MergeDown");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Test_KeepWith_MergeDown_PageBreak()
        {
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

            var pdfRenderer = CreateReadablePdfDocumentRenderer(document);
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_MergeDown_PageBreak");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact/*(Skip = "Fails - cause has to be found and fixed.")*/]
        public void Test_MergeDown_LineBreak_RowHeight()
        {
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
            row1[0].AddParagraph("Row 1 Cell 0 MergeDown 1 with\nline break ID#1"); // The cell’s content must end with "ID#1" to allow analysis of the generated PDF below.
            row1[0].MergeDown = 1;
            row1[1].AddParagraph("Row 1 Cell 1");
            var row1CommentRow = table.AddRow();
            row1CommentRow[1].AddParagraph("Comment 1 Cell 1 ID#2"); // The cell’s content must end with "ID#2" to allow analysis of the generated PDF below.

            var row2 = table.AddRow();
            row2[0].AddParagraph("Row 2 Cell 0 MergeDown 1");
            row2[0].MergeDown = 1;
            row2[1].AddParagraph("Row 2 Cell 1");
            var row2CommentRow = table.AddRow();
            row2CommentRow[1].AddParagraph("Comment 2 Cell 1");

            var pdfRenderer = CreateReadablePdfDocumentRenderer(document);
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_MergeDown_LineBreak_RowHeight");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Analyze the drawn borders in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfRenderer.PdfDocument, 0);

            // Find "ID#1" text object.
            var id1Found = streamEnumerator.Text.MoveAndGetNext(x => x.Text == "ID#1", true, out _);
            id1Found.Should().BeTrue("text object \"ID#1\" shall be found");

            // Check the following lines drawing the borders for the correct values.
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("ET", "\"ID#1\" shall be the last text of the cell");

            streamEnumerator.Line.MoveAndGetNext(6, false, out var lineInfo).Should().BeTrue("the sixth next element should be a line element (l)");
            lineInfo!.Y1Str.Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            lineInfo.Y1Str.Should().Be("732.5266", "this is the value generated with the correct cell height");
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("S", "stroking the path is expected");

            streamEnumerator.Line.MoveAndGetNext(6, false, out lineInfo).Should().BeTrue("the sixth next element should be a line element (l)");
            lineInfo!.Y1Str.Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            lineInfo.Y1Str.Should().Be("732.5266", "this is the value generated with the correct cell height");
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("S", "stroking the path is expected");

            streamEnumerator.Line.MoveAndGetNext(6, false, out lineInfo).Should().BeTrue("the sixth next element should be a line element (l)");
            lineInfo!.Y1Str.Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            lineInfo.Y1Str.Should().Be("733.0266", "this is the value generated with the correct cell height");
            lineInfo.Y2Str.Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            lineInfo.Y2Str.Should().Be("733.0266", "this is the value generated with the correct cell height");
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("S", "stroking the path is expected");

            // Find "ID#2" text object.
            var id2Found = streamEnumerator.Text.MoveAndGetNext(x => x.Text == "ID#2", true, out _);
            id2Found.Should().BeTrue("text object \"ID#2\" shall be found");
            
            // Check the following lines drawing the borders for the correct values.
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("ET", "\"ID#1\" shall be the last text of the cell");

            streamEnumerator.Line.MoveAndGetNext(6, false, out lineInfo).Should().BeTrue("the sixth next element should be a line element (l)");
            lineInfo!.Y1Str.Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            lineInfo.Y1Str.Should().Be("732.5266", "this is the value generated with the correct cell height");
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("S", "stroking the path is expected");

            streamEnumerator.Line.MoveAndGetNext(6, false, out lineInfo).Should().BeTrue("the sixth next element should be a line element (l)");
            lineInfo!.Y1Str.Should().NotBe("721.0276", "this is the value generated with an incorrect cell height");
            lineInfo.Y1Str.Should().Be("732.5266", "this is the value generated with the correct cell height");
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("S", "stroking the path is expected");

            streamEnumerator.Line.MoveAndGetNext(6, false, out lineInfo).Should().BeTrue("the sixth next element should be a line element (l)");
            lineInfo!.Y1Str.Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            lineInfo.Y1Str.Should().Be("733.0266", "this is the value generated with the correct cell height");
            lineInfo.Y2Str.Should().NotBe("721.5276", "this is the value generated with an incorrect cell height");
            lineInfo.Y2Str.Should().Be("733.0266", "this is the value generated with the correct cell height");
            streamEnumerator.MoveNext().Should().BeTrue();
            streamEnumerator.Current.Should().Be("S", "stroking the path is expected");
        }

        [Fact]
        public void Test_Border_Inheritance()
        {
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

            var pdfRenderer = CreateReadablePdfDocumentRenderer(document);
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_Border_Inheritance");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Analyze the drawn border widths in the PDF’s content stream.
            var page = pdfRenderer.PdfDocument.Pages[0];
            var contentReference = (PdfReference)page.Contents.Elements.Items[0];
            var content = (PdfDictionary)contentReference.Value;
            var contentStream = content.Stream.ToString();
            var contentLines = contentStream.Split('\n');

            // 1.5 is the desired border width. It shall be set only once.
            contentLines.Count(x => x == "1.5 w").Should().Be(1);
            // The border width in general shall be set only once - so changes of border width should not occur.
            contentLines.Count(x => x.EndsWith(" w", StringComparison.Ordinal)).Should().Be(1);
        }

        [Fact]
        public void Test_Huge_MergeDown_Cell()
        {
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
            var pdfRenderer = CreateReadablePdfDocumentRenderer(document);
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_Huge_MergeDown_Cell");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Test_Repeated_Heading_Border()
        {
            var bottomWidth = Unit.FromPoint(2.3);
            var bottomColor = Colors.Blue;
            var contentStreamBottomWidth = "2.3 w";
            var contentStreamBottomColor = "0 0 1 RG";

            var headingBottomWidth = Unit.FromPoint(4.6);
            var headingBottomColor = Colors.Red;
            var contentStreamHeadingBottomWidth = "4.6 w";
            var contentStreamHeadingBottomColor = "1 0 0 RG";

            var document = new Document();
            var section = document.AddSection();

            var table = section.AddTable();
            table.Borders.Bottom.Width = bottomWidth;
            table.Borders.Bottom.Color = bottomColor;

            table.AddColumn(Unit.FromCentimeter(16));

            var headingRow = table.AddRow();
            headingRow.HeadingFormat = true;
            headingRow.Cells[0].AddParagraph("Heading");
            headingRow.Borders.Bottom.Width = headingBottomWidth;
            headingRow.Borders.Bottom.Color = headingBottomColor;

            // Add 4 rows with a height forcing a page break after the first two rows.
            for (var rowNr = 1; rowNr <= 4; rowNr++)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph($"Row {rowNr}");
                row.Height = Unit.FromCentimeter(10);
            }

            var pdfRenderer = CreateReadablePdfDocumentRenderer(document);
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_Repeated_Heading_Border");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);


            // Analyze the drawn border widths and colors in the PDF’s pages content streams.
            // The two parts the page break breaks the table into should be identical (except the row numbers).
            for (var pageIdx = 0; pageIdx < pdfRenderer.PageCount; pageIdx++)
            {
                var contentStream = PdfFileHelper.GetPageContentStream(pdfRenderer.PdfDocument, pageIdx);

#if NET6_0_OR_GREATER
                // Split ContentStream where the "Row" text is rendered.
                var contentByRows = contentStream.Split("(Row) Tj");
                contentByRows.Length.Should().Be(3, "as \"Row\" occurs twice per page, the stream should be split into 3 parts");

                var rowsByDrawLinesByLines = contentByRows // Content split by "Row" text ...
                    .Select(r => r.Split(" l\n") // ... and that parts split by drawn lines ...
                        .Select(drawLine => drawLine.Split('\n')).ToArray() // ... and that parts split by line breaks.
                    ).ToArray();
#else
                // Split ContentStream where the "Row" text is rendered.
                var contentByRows = contentStream.Splitter("(Row) Tj");
                contentByRows.Length.Should().Be(3, "as \"Row\" occurs twice per page, the stream should be split into 3 parts");

                var rowsByDrawLinesByLines = contentByRows // Content split by "Row" text ...
                    .Select(r => r.Splitter(" l\n") // ... and that parts split by drawn lines ...
                            .Select(drawLine => drawLine.Split('\n')).ToArray() // ... and that parts split by line breaks.
                    ).ToArray();
#endif

                // Heading row.
                var contentRowDrawLineParts = rowsByDrawLinesByLines[0];
                contentRowDrawLineParts.Length.Should().Be(2, "for the heading row only one bottom border should split the content into 2 parts");

                // The part before the first draw line contains the data for the bottom border.
                var bottomBorderDrawLinePartLines = contentRowDrawLineParts[0];
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamHeadingBottomWidth, "heading bottom border should be of heading bottom border width");
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamHeadingBottomColor, "heading bottom border should be of heading bottom border color");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamBottomWidth, "heading bottom border should not be of content bottom border width");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamBottomColor, "heading bottom border should not be of content bottom border color");

                
                // Row 1.
                contentRowDrawLineParts = rowsByDrawLinesByLines[1];
                contentRowDrawLineParts.Length.Should().Be(3, "for the content rows one bottom and one top border should split the content into 3 parts");

                // The part before the first draw line contains the data for the bottom border.
                bottomBorderDrawLinePartLines = contentRowDrawLineParts[0];
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamBottomWidth, "row 1 bottom border should be of content bottom border width");
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamBottomColor, "row 1 bottom border should be of content bottom border color");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamHeadingBottomWidth, "row 1 bottom border should not be of heading bottom border width");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamHeadingBottomColor, "row 1 bottom border should not be of heading bottom border color");

                // The part before the second draw line contains the data for the top border. Attention: Row 1 top border is equal to heading bottom border and should therefore have its values.
                bottomBorderDrawLinePartLines = contentRowDrawLineParts[1];
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamHeadingBottomWidth, "row 1 top border should be of heading bottom border width, as this is the same border");
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamHeadingBottomColor, "row 1 top border should be of heading bottom border color, as this is the same border");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamBottomWidth, "row 1 top border should not be of content bottom border width, as this border is the same like heading bottom");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamBottomColor, "row 1 top border should not be of content bottom border color, as this border is the same like heading bottom");
                // Row 2.
                contentRowDrawLineParts = rowsByDrawLinesByLines[2];
                contentRowDrawLineParts.Length.Should().Be(3, "for the content rows one bottom and one top border should split the content into 3 parts");

                // The part before the first draw line contains the data for the bottom border.
                bottomBorderDrawLinePartLines = contentRowDrawLineParts[0];
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamBottomWidth, "row 2 bottom border should be of content bottom border width");
                bottomBorderDrawLinePartLines.Should().Contain(contentStreamBottomColor, "row 2 bottom border should be of content bottom border color");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamHeadingBottomWidth, "row 2 bottom border should not be of heading bottom border width");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamHeadingBottomColor, "row 2 bottom border should not be of heading bottom border color");

                // The part before the second draw line contains the data for the top border. Attention: This should not be set as the values of the bottom border should remain unchanged.
                bottomBorderDrawLinePartLines = contentRowDrawLineParts[1];
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamBottomWidth, "row 2 top border should not be set as the values should be the same as for the bottom border");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamBottomColor, "row 2 top border should not be set as the values should be the same as for the bottom border");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamHeadingBottomWidth, "row 2 top border should not be set as the values should be the same as for the bottom border");
                bottomBorderDrawLinePartLines.Should().NotContain(contentStreamHeadingBottomColor, "row 2 top border should not be set as the values should be the same as for the bottom border");
            }
        }
    }
}
