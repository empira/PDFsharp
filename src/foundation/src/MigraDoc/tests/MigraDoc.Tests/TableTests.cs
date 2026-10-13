// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Text.RegularExpressions;
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

        [Theory]
        [InlineData(false, null)]
        [InlineData(true, null)]
        [InlineData(false, 143.12345649)]
        [InlineData(true, 143.12345649)]
        [InlineData(false, 143.12345651)]
        [InlineData(true, 143.12345651)]
        public void SharedBorderIsPaintedOnceAfterFollowingCellShading(bool vertical, double? pointWidth)
        {
            var document = new Document();
            document.Styles.Normal.Font.Name = "Arial";
            var table = document.AddSection().AddTable();
            var columnWidth = Unit.FromPoint(pointWidth ?? 5 * 72 / 2.54);
            table.AddColumn(columnWidth);
            if (vertical)
                table.AddColumn(columnWidth);
            for (var index = 0; index < (vertical ? 1 : 2); index++)
            {
                var row = table.AddRow();
                for (var column = 0; column < (vertical ? 2 : 1); column++)
                {
                    var cell = row[column];
                    cell.AddParagraph($"Cell {index} {column}");
                    cell.Shading.Color = Colors.White;
                    cell.Borders.Color = Colors.Red;
                    cell.Borders.Width = 1;
                }
            }

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            var content = PdfFileHelper.GetPageContentStream(renderer.PdfDocument, 0);
            content.Should().Contain("1 1 1 rg");
            content.Should().Contain("1 0 0 RG");
            var sharedAxisLines = BorderStrokes(content)
                .Where(match => vertical
                    ? match.Groups["x1"].Value == match.Groups["x2"].Value &&
                      match.Groups["y1"].Value != match.Groups["y2"].Value
                    : match.Groups["y1"].Value == match.Groups["y2"].Value &&
                      match.Groups["x1"].Value != match.Groups["x2"].Value).ToArray();
            sharedAxisLines.Should().HaveCount(3);
            sharedAxisLines[1].Index.Should().BeGreaterThan(content.LastIndexOf("1 1 1 rg", StringComparison.Ordinal));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SharedBordersOfMergedCellsArePaintedOnce(bool verticalMerge)
        {
            var document = new Document();
            document.Styles.Normal.Font.Name = "Arial";
            var table = document.AddSection().AddTable();
            for (var column = 0; column < (verticalMerge ? 2 : 3); column++)
                table.AddColumn("5cm");
            if (verticalMerge)
            {
                var firstRow = table.AddRow();
                var secondRow = table.AddRow();
                firstRow[0].MergeDown = 1;
                firstRow[0].AddParagraph("Merged");
                firstRow[1].AddParagraph("Upper");
                secondRow[1].AddParagraph("Lower");
                foreach (var cell in new[] { firstRow[0], firstRow[1], secondRow[1] })
                {
                    cell.Borders.Color = Colors.Red;
                    cell.Borders.Width = 1;
                }
            }
            else
            {
                for (var rowIndex = 0; rowIndex < 2; rowIndex++)
                {
                    var row = table.AddRow();
                    row[0].MergeRight = 1;
                    row[0].AddParagraph($"Merged {rowIndex}");
                    row[2].AddParagraph($"Other {rowIndex}");
                    foreach (var cell in new[] { row[0], row[2] })
                    {
                        cell.Borders.Color = Colors.Red;
                        cell.Borders.Width = 1;
                    }
                }
            }

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            var content = PdfFileHelper.GetPageContentStream(renderer.PdfDocument, 0);
            var strokes = BorderStrokes(content)
                .Select(match => match.Value).ToArray();
            strokes.Should().NotBeEmpty();
            strokes.Should().OnlyHaveUniqueItems();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ExplicitlyHiddenFollowingBorderDoesNotTakeOwnershipOfPreviousBorder(bool vertical)
        {
            var document = new Document();
            document.Styles.Normal.Font.Name = "Arial";
            var table = document.AddSection().AddTable();
            table.AddColumn("5cm");
            if (vertical)
                table.AddColumn("5cm");
            var firstRow = table.AddRow();
            var first = firstRow[0];
            first.AddParagraph("First");
            var firstBorder = vertical ? first.Borders.Right : first.Borders.Bottom;
            firstBorder.Color = Colors.Red;
            firstBorder.Width = 0.25;
            var second = vertical ? firstRow[1] : table.AddRow()[0];
            second.AddParagraph("Second");
            if (vertical)
                second.Borders.Left.Visible = false;
            else
                second.Borders.Top.Visible = false;
            second.Shading.Color = Colors.White;

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            var content = PdfFileHelper.GetPageContentStream(renderer.PdfDocument, 0);
            var sharedBorder = BorderStrokes(content)
                .Single(match => vertical
                    ? match.Groups["x1"].Value == match.Groups["x2"].Value &&
                      match.Groups["y1"].Value != match.Groups["y2"].Value
                    : match.Groups["y1"].Value == match.Groups["y2"].Value &&
                      match.Groups["x1"].Value != match.Groups["x2"].Value);
            sharedBorder.Index.Should().BeLessThan(content.LastIndexOf("1 1 1 rg", StringComparison.Ordinal));
        }

        [Fact]
        public void DashedSharedBordersAreNotDeduplicated()
        {
            var document = new Document();
            document.Styles.Normal.Font.Name = "Arial";
            var table = document.AddSection().AddTable();
            table.AddColumn("5cm");
            table.AddColumn("5cm");
            var row = table.AddRow();
            row[0].AddParagraph("Left");
            row[0].Borders.Color = Colors.Red;
            row[0].Borders.Width = 1;
            row[0].Borders.Style = BorderStyle.Dot;
            row[1].AddParagraph("Right");
            row[1].Borders.Color = Colors.Red;
            row[1].Borders.Width = 1;
            row[1].Borders.Style = BorderStyle.Dot;

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            var content = PdfFileHelper.GetPageContentStream(renderer.PdfDocument, 0);
            var verticalLines = BorderStrokes(content)
                .Where(match => match.Groups["x1"].Value == match.Groups["x2"].Value &&
                    match.Groups["y1"].Value != match.Groups["y2"].Value).ToArray();
            verticalLines.Should().HaveCount(4);
        }
        [Fact]
        public void RepeatedHeaderSharedBorderIsPaintedOnce()
        {
            var document = new Document();
            document.Styles.Normal.Font.Name = "Arial";
            var table = document.AddSection().AddTable();
            table.AddColumn("5cm");
            table.AddColumn("5cm");
            var header = table.AddRow();
            header.HeadingFormat = true;
            foreach (var cell in new[] { header[0], header[1] })
            {
                cell.AddParagraph("Header");
                cell.Borders.Color = Colors.Red;
                cell.Borders.Width = 1;
            }
            for (var index = 0; index < 100; index++)
                table.AddRow()[0].AddParagraph($"Body row {index}");

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            renderer.PdfDocument.PageCount.Should().BeGreaterThan(1);
            var content = PdfFileHelper.GetPageContentStream(renderer.PdfDocument, 1);
            content.Should().Contain("(Header) Tj").And.Contain("(Body) Tj");
            content.IndexOf("(Header) Tj", StringComparison.Ordinal)
                .Should().BeLessThan(content.IndexOf("(Body) Tj", StringComparison.Ordinal));
            var verticalLines = BorderStrokes(content)
                .Where(match => match.Groups["x1"].Value == match.Groups["x2"].Value &&
                    match.Groups["y1"].Value != match.Groups["y2"].Value)
                .Select(match => match.Value).ToArray();
            verticalLines.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void HeaderAndBodySharedBorderIsPaintedOnce()
        {
            var document = new Document();
            document.Styles.Normal.Font.Name = "Arial";
            var table = document.AddSection().AddTable();
            table.AddColumn("5cm");
            var header = table.AddRow();
            header.HeadingFormat = true;
            header[0].AddParagraph("Header");
            var body = table.AddRow();
            body[0].AddParagraph("Body");
            foreach (var cell in new[] { header[0], body[0] })
            {
                cell.Borders.Color = Colors.Red;
                cell.Borders.Width = 1;
            }

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            var content = PdfFileHelper.GetPageContentStream(renderer.PdfDocument, 0);
            var horizontalLines = BorderStrokes(content)
                .Where(match => match.Groups["y1"].Value == match.Groups["y2"].Value &&
                    match.Groups["x1"].Value != match.Groups["x2"].Value)
                .Select(match => match.Value).ToArray();
            horizontalLines.Should().OnlyHaveUniqueItems();
        }

        static MatchCollection BorderStrokes(string content) => Regex.Matches(content,
            @"(?m)(?<x1>[\d.]+) (?<y1>[\d.]+) m\s+(?<x2>[\d.]+) (?<y2>[\d.]+) l\s+S");

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

            // Save the document…
            var filename = PdfFileUtility.GetTempPdfFullFileName("unittests/migradoc/tables/TableTopBorder");
            pdfRenderer.PdfDocument.Save(filename);
            // … and start a viewer.
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

            // Save the document…
            var filename = PdfFileUtility.GetTempPdfFullFileName("unittests/migradoc/tables/ClonedTable");
            pdfRenderer.PdfDocument.Save(filename);
            // … and start a viewer.
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

            // Save the document…
            var filename = PdfFileUtility.GetTempPdfFullFileName("unittests/migradoc/tables/ClonedTable");
            pdfRenderer.PdfDocument.Save(filename);
            // … and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}
