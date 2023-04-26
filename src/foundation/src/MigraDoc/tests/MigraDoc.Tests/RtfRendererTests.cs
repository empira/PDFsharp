// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using FluentAssertions;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using MigraDoc.Tests.Helper;
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Test_Tabs(bool doNotUnifyTabStopHandling)
        {
            Capabilities.BackwardCompatibility.DoNotUnifyTabStopHandling = doNotUnifyTabStopHandling;

#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif
            // TODO Register encoding here or in RtfDocumentRenderer?
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            
            var document = new Document();

            var styles = document.Styles;

            var style = styles[StyleNames.Heading1];
            style!.Font.Size = 14;
            style.ParagraphFormat.SpaceBefore = Unit.FromMillimeter(5);
            style.ParagraphFormat.SpaceAfter = Unit.FromMillimeter(2);
            
            var section = document.AddSection();

            // Values - see Description:
            var value = 1234567.89;
            var valueStr = $"{value:N}";
            var valueUnifyTabStops = 7654321.98;
            var valueUnifyTabStopsStr = $"{valueUnifyTabStops:N}";

            var position1 = Unit.FromCentimeter(3);
            var position2 = Unit.FromCentimeter(8);

            // Description:
            section.AddParagraph("This document contains several tabstop test cases. For each tabstop a tab is used to reach it.\n" +
                                 $"By default the value \"{valueStr}\" is used for the content that should be aligned by the tabstops.\n" +
                                 "For the special cases where an additional left aligned tabstop at position 0 is required for correct RTF generation, " +
                                 $"the value \"{valueUnifyTabStopsStr}\" is used instead.\n" +
                                 "This special case occurs, if a single decimal tabstop is used in a table rendered in RTF." +
                                 "In that case RTF requires no tab to reach the tabstop.");


            // Print alignment order.
            var paragraph = section.AddParagraph("Alignment order");
            paragraph.Style = StyleNames.Heading1;
            
            foreach (var alignment in Enum.GetValues<TabAlignment>())
                section.AddParagraph(alignment.ToString());


            // 1.1 Single tabs in paragraph: Nothing special - every tabstop needs a tab to be reached.
            paragraph = section.AddParagraph("1.1. Single tabs in paragraph");
            paragraph.Style = StyleNames.Heading1;

            TestHelper.DrawHorizontalPosition(section, position1);
            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                paragraph = section.AddParagraph();

                paragraph.Format.TabStops.ClearAll();
                paragraph.Format.TabStops.AddTabStop(position1, alignment);
                paragraph.AddTab();
                paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, position1);


            // 1.2. Multiple tabs in paragraph: Nothing special - every tabstop needs a tab to be reached.
            paragraph = section.AddParagraph("1.2. Multiple tabs in paragraph");
            paragraph.Style = StyleNames.Heading1;

            TestHelper.DrawHorizontalPosition(section, position1, position2);
            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                paragraph = section.AddParagraph();

                paragraph.Format.TabStops.ClearAll();
                paragraph.Format.TabStops.AddTabStop(position1, alignment);
                paragraph.Format.TabStops.AddTabStop(position2, alignment);
                paragraph.AddTab();
                paragraph.AddText(valueStr);
                paragraph.AddTab();
                paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, position1, position2);


            // 2.1. Single tabs in a table:
            // Decimal tab in a table is a special case in RTF, because here is no tabstop required to reach the tab.
            paragraph = section.AddParagraph("2.1. Single tabs in table\n" +
                                             "(special case in RTF for decimal tabstop!)");
            paragraph.Style = StyleNames.Heading1;
            
            TestHelper.DrawHorizontalPosition(section, position1);

            var table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(16));

            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                var row = table.AddRow();

                var cell = row[0];
                paragraph = cell.AddParagraph();
                paragraph.Format.TabStops.ClearAll();
                paragraph.Format.TabStops.AddTabStop(position1, alignment);
                
                paragraph.AddTab();
                // For the special case an additional left aligned tabstop will be inserted add position 0 in RTF TabStopsRenderer.
                // We mark this special case with a special value and a colored paragraph.
                if (alignment == TabAlignment.Decimal)
                {
                    paragraph.AddText(valueUnifyTabStopsStr);
                    paragraph.Format.Font.Color = Colors.Red;
                }
                else
                    paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, position1);


            // 2.2. Single tabs in a second table column:
            // Decimal tab in a table is a special case in RTF, because here is no tabstop required to reach the tab.
            paragraph = section.AddParagraph("2.2. Single tabs in second table column\n" +
                                             "(special case in RTF for decimal tabstop!)");
            paragraph.Style = StyleNames.Heading1;

            var absolutePosition = position1 + Unit.FromCentimeter(2);
            TestHelper.DrawHorizontalPosition(section, absolutePosition);

            table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(2));
            table.AddColumn(Unit.FromCentimeter(14));

            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                var row = table.AddRow();

                var cell = row[0];
                cell.AddParagraph("Cell 0");

                cell = row[1];
                paragraph = cell.AddParagraph();
                paragraph.Format.TabStops.ClearAll();
                paragraph.Format.TabStops.AddTabStop(position1, alignment);
                
                paragraph.AddTab();
                // For the special case an additional left aligned tabstop will be inserted add position 0 in RTF TabStopsRenderer.
                // We mark this special case with a special value and a colored paragraph.
                if (alignment == TabAlignment.Decimal)
                {
                    paragraph.AddText(valueUnifyTabStopsStr);
                    paragraph.Format.Font.Color = Colors.Red;
                }
                else
                    paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, absolutePosition);


            // 2.3. Multiple tabs in a table: Nothing special - every tabstop needs a tab to be reached.
            paragraph = section.AddParagraph("2.3. Multiple tabs in table");
            paragraph.Style = StyleNames.Heading1;

            TestHelper.DrawHorizontalPosition(section, position1, position2);

            table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(16));

            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                var row = table.AddRow();

                var cell = row[0];
                paragraph = cell.AddParagraph();
                paragraph.Format.TabStops.ClearAll();
                paragraph.Format.TabStops.AddTabStop(position1, alignment);
                paragraph.Format.TabStops.AddTabStop(position2, alignment);
                
                paragraph.AddTab();
                paragraph.AddText(valueStr);
                paragraph.AddTab();
                paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, position1, position2);


            // 2.4. Single tabs in a table with a bundle of empty Paragraphs, FormattedTexts and Texts:
            // Decimal tab in a table is a special case in RTF, because here is no tabstop required to reach the tab.
            paragraph = section.AddParagraph("2.4. Single tabs in table with a bundle of empty Paragraphs, FormattedTexts and Texts\n" +
                                             "(special case in RTF for decimal tabstop!)");
            paragraph.Style = StyleNames.Heading1;

            TestHelper.DrawHorizontalPosition(section, position1);

            table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(16));

            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                var row = table.AddRow();

                var cell = row[0];
                paragraph = cell.AddParagraph(); 
                paragraph.AddText("");
                var ft = paragraph.AddFormattedText("");
                ft.AddText("");

                paragraph = cell.AddParagraph();
                paragraph.Format.TabStops.ClearAll();
                paragraph.Format.TabStops.AddTabStop(position1, alignment);

                paragraph.AddText("");
                ft = paragraph.AddFormattedText("");
                ft.AddText("");
                paragraph.AddTab();
                // For the special case an additional left aligned tabstop will be inserted add position 0 in RTF TabStopsRenderer.
                // We mark this special case with a special value and a colored paragraph.
                if (alignment == TabAlignment.Decimal)
                {
                    paragraph.AddText("");
                    ft = paragraph.AddFormattedText("");
                    ft.AddText("");
                    ft.AddText(valueUnifyTabStopsStr);
                    paragraph.Format.Font.Color = Colors.Red;
                }
                else
                    paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, position1);


            // 2.5. Single tabs in a table with preceding text:
            // Decimal tab in a table is a special case in RTF, because here is no tabstop required to reach the tab.
            paragraph = section.AddParagraph("2.5. Single tabs in a table with preceding text\n" +
                                             "(special case in RTF for decimal tabstop!)");
            paragraph.Style = StyleNames.Heading1;

            TestHelper.DrawHorizontalPosition(section, position1);

            table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(16));

            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                var row = table.AddRow();

                var cell = row[0];
                paragraph = cell.AddParagraph();
                paragraph.Format.TabStops.ClearAll();
                paragraph.Format.TabStops.AddTabStop(position1, alignment);

                paragraph.AddText("Text");
                paragraph.AddTab();
                // For the special case an additional left aligned tabstop will be inserted add position 0 in RTF TabStopsRenderer.
                // We mark this special case with a special value and a colored paragraph.
                if (alignment == TabAlignment.Decimal)
                {
                    paragraph.AddText(valueUnifyTabStopsStr);
                    paragraph.Format.Font.Color = Colors.Red;
                }
                else
                    paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, position1);


            // 2.6. Single tabs in a table with tabstops declared in style:
            // Decimal tab in a table is a special case in RTF, because here is no tabstop required to reach the tab.
            paragraph = section.AddParagraph("2.6. Single tabs in table with tabstops declared in style\n" +
                                             "(special case in RTF for decimal tabstop!)");
            paragraph.Style = StyleNames.Heading1;

            TestHelper.DrawHorizontalPosition(section, position1);

            table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(16));

            foreach (var alignment in Enum.GetValues<TabAlignment>())
            {
                var row = table.AddRow();

                var cell = row[0];
                paragraph = cell.AddParagraph();
                
                var styleName = $"TabStyle{alignment}";
                style = styles.AddStyle(styleName, StyleNames.Normal);
                style.ParagraphFormat.TabStops.ClearAll();
                style.ParagraphFormat.TabStops.AddTabStop(position1, alignment);
                paragraph.Style = styleName;

                paragraph.AddTab();
                // For the special case an additional left aligned tabstop will be inserted add position 0 in RTF TabStopsRenderer.
                // We mark this special case with a special value and a colored paragraph.
                if (alignment == TabAlignment.Decimal)
                {
                    paragraph.AddText(valueUnifyTabStopsStr);
                    paragraph.Format.Font.Color = Colors.Red;
                }
                else
                    paragraph.AddText(valueStr);
            }
            TestHelper.DrawHorizontalPosition(section, position1);


            // Render PDF and RTF files.
            var filename = "Test_Tabs_";
            filename += Capabilities.BackwardCompatibility.DoNotUnifyTabStopHandling ? "DoNotUnifyTabStopHandling" : "UnifyTabStopHandling";

            var pdfFilename = PdfFileHelper.CreateTempFileName(filename);
            var rtfFilename = pdfFilename.Replace(".pdf", ".rtf");

            var rtfRenderer = new RtfDocumentRenderer();
            rtfRenderer.Render(document, rtfFilename, Environment.CurrentDirectory);

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();
            pdfRenderer.Save(pdfFilename);


            // Check if additional tabStops to achieve a consistent behavior in PDFsharp are correctly added not added for single decimal tabstops in RTF tables,
            // according to the DoNotUnifyTabStopHandling backward compatibility capability setting.
            var rtf = File.ReadAllText(rtfFilename);

            // Do not include description text in search.
            var startSearchPos = rtf.IndexOf("Alignment order", StringComparison.Ordinal);
            
            const string cellStr = "\\cell\\";
            const string paragraphStr = "\\par\\";
            const string decimalTabStopStr = "\\tqdec\\";
            const string leftTabStopStr = "\\tql\\";
            const string tabStopPositionStr = "\\tx"; // Do not include Backslash of next element as a number is expected first.

            int valuePos;

            // Search for valueRtfRemovedTabStr - the occurrences for that an additional tabstop should be eventually added.
            var searchPos = startSearchPos;
            for (var i = 0; i < 5; i++)
            {
                // Get a position of valueUnifyTabStopsStr.
                valuePos = rtf.IndexOf(valueUnifyTabStopsStr, searchPos, StringComparison.Ordinal);
                valuePos.Should().BeGreaterThan(0, $"\"{valueUnifyTabStopsStr}\" should occur 5 times in the RTF file after the description text.");

                // Get the position of the cell containing the value.
                var cellPos = rtf.LastIndexOf(cellStr, valuePos, StringComparison.Ordinal);
                cellPos.Should().BeGreaterThan(0, $"\"{valueUnifyTabStopsStr}\" should occur inside of a cell.");

                // Get the position of the decimal tabstop for this cell.
                var decTabStopPos = rtf.LastIndexOf(decimalTabStopStr, valuePos, StringComparison.Ordinal);
                decTabStopPos.Should().BeGreaterThan(cellPos, "for the cell a decimal tabstop should be defined.");

                // Get the position of the tabstop's position element.
                var positionPos = rtf.LastIndexOf(tabStopPositionStr, valuePos, StringComparison.Ordinal);
                positionPos.Should().BeGreaterThan(decTabStopPos, "for the decimal tabstop a position should be defined.");

                // Get the tabstop's position value and ensure that it's not 0.
                var positionValuePos = positionPos + tabStopPositionStr.Length;
                var positionValueEndPos = rtf.IndexOf("\\", positionValuePos, StringComparison.Ordinal);
                var positionValueStr = rtf.Substring(positionValuePos, positionValueEndPos - positionValuePos);
                var parseSuccess = int.TryParse(positionValueStr, out var positionValue);
                parseSuccess.Should().BeTrue();
                positionValue.Should().NotBe(0);


                // Get the position of the left tabstop previous to the decimal tabstop for this cell.
                var leftTabStopPos = rtf.LastIndexOf(leftTabStopStr, decTabStopPos, StringComparison.Ordinal);
                if (Capabilities.BackwardCompatibility.DoNotUnifyTabStopHandling)
                {
                    // For DoNotUnifyTabStopHandling == true, no such tabstop should exist.
                    leftTabStopPos.Should().BeLessThan(cellPos, "with DoNotUnifyTabStopHandling disabled, no additional tabstop should be defined for the cell.");
                }
                else
                {
                    // For DoNotUnifyTabStopHandling == false, the tabstop should exist.
                    leftTabStopPos.Should().BeGreaterThan(cellPos, "with DoNotUnifyTabStopHandling enabled, an additional left tabstop should be defined for the cell.");

                    // Get the position of the tabstop's position element.
                    positionPos = rtf.LastIndexOf(tabStopPositionStr, decTabStopPos, StringComparison.Ordinal);
                    positionPos.Should().BeGreaterThan(leftTabStopPos, "for the left tabstop a position should be defined.");

                    // Get the tabstop's position value and ensure that it's 0.
                    positionValuePos = positionPos + tabStopPositionStr.Length;
                    positionValueEndPos = rtf.IndexOf("\\", positionValuePos, StringComparison.Ordinal);
                    positionValueStr = rtf.Substring(positionValuePos, positionValueEndPos - positionValuePos);
                    parseSuccess = int.TryParse(positionValueStr, out positionValue);
                    parseSuccess.Should().BeTrue("Tabstop position should define a number.");
                    positionValue.Should().Be(0, "The additional left tabstop should be at position 0.");
                }

                searchPos = valuePos + valueUnifyTabStopsStr.Length;
            }
            valuePos = rtf.IndexOf(valueUnifyTabStopsStr, searchPos, StringComparison.Ordinal);
            valuePos.Should().BeLessThan(0, $"\"{valueUnifyTabStopsStr}\" should occur only 5 times in a cell in the RTF file after the description text.");

            // Search for valueStr - the occurrences for that no additional tabstop should be added.
            searchPos = startSearchPos;
            for (var i = 0; i < 35; i++)
            {
                // Get a position of valueStr.
                valuePos = rtf.IndexOf(valueStr, searchPos, StringComparison.Ordinal);
                valuePos.Should().BeGreaterThan(0, $"\"{valueStr}\" should occur 35 times in the RTF file after the description text.");

                // Get the position of the cell or paragraph containing the value.
                var cellPos = rtf.LastIndexOf(cellStr, valuePos, StringComparison.Ordinal);
                var parPos = rtf.LastIndexOf(paragraphStr, valuePos, StringComparison.Ordinal);
                var containerPos = Math.Max(cellPos, parPos);
                containerPos.Should().BeGreaterThan(0, $"\"{valueStr}\" should occur inside of a cell or paragraph.");

                // Check all tabstop's position elements for this cell or paragraph.
                var tabStopPositionSearchPos = containerPos;
                // Exit the loop if or positionPos exceeds the position of the value or if no position element could be found.
                while (tabStopPositionSearchPos < valuePos)
                {
                    var positionPos = rtf.IndexOf(tabStopPositionStr, tabStopPositionSearchPos, StringComparison.Ordinal);
                    if (positionPos < 0 || positionPos > valuePos)
                        break;

                    // Get the tabstop's position value and ensure that it's not 0, as no additional tabstop should be added here at position 0.
                    var positionValuePos = positionPos + tabStopPositionStr.Length;
                    var positionValueEndPos = rtf.IndexOf("\\", positionValuePos, StringComparison.Ordinal);
                    var positionValueStr = rtf.Substring(positionValuePos, positionValueEndPos - positionValuePos);
                    var parseSuccess = int.TryParse(positionValueStr, out var positionValue);
                    parseSuccess.Should().BeTrue("Tabstop position should define a number.");
                    positionValue.Should().NotBe(0, "no additional tabstop should be added at position 0.");

                    tabStopPositionSearchPos = positionValueEndPos;
                }

                searchPos = valuePos + valueStr.Length;
            }
        }
    }
}
