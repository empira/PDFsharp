// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using PdfSharp.Quality;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using MigraDoc.Tests.Helper;
using Xunit;
using FluentAssertions;
#if CORE
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
#endif
#if WPF
using System.IO;
#endif
#if !NET6_0_OR_GREATER
using PdfSharp.TestHelper;
#endif

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class RtfRendererTests
    {
        [Fact]
        public void Create_Hello_World_RtfRendererTests()
        {
            // Create a MigraDoc document.
            var document = CreateDocument();

            document.LastSection.AddParagraph("Some Unicode text: ƔɣΓγ.");
            document.LastSection.AddParagraph("Some Unicode surrogate pairs: 💩\ud83d\udca9🤺\ud83e\udd3a.");

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
            var rtfRenderer = new RtfDocumentRenderer();

            // Save the document...
            var filename = IOUtility.GetTempFileName("HelloWorld", null);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_0.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            // Layout and render document to RTF.
            rtfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);

#if DEBUG___
            dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_1.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif
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

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            return document;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Test_Tabs(bool doNotUnifyTabStopHandling)
        {
            Capabilities.BackwardCompatibility.DoNotUnifyTabStopHandling = doNotUnifyTabStopHandling;

            var document = new Document();

            var styles = document.Styles;

            var style = styles[StyleNames.Heading1];
            style!.Font.Size = 14;
            style.ParagraphFormat.SpaceBefore = Unit.FromMillimeter(5);
            style.ParagraphFormat.SpaceAfter = Unit.FromMillimeter(2);

            var section = document.AddSection();

            // Values - see Description:
            var value = 1234567.89;
            var valueStr = value.ToString(CultureInfo.InvariantCulture);
            var valueUnifyTabStops = 7654321.98;
            var valueUnifyTabStopsStr = valueUnifyTabStops.ToString(CultureInfo.InvariantCulture);

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

#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
                section.AddParagraph(alignment.ToString());
#else
            foreach (var alignment in Enum.GetValues(typeof(TabAlignment)))
                section.AddParagraph(alignment.ToString());
#endif

            // 1.1 Single tabs in paragraph: Nothing special - every tabstop needs a tab to be reached.
            paragraph = section.AddParagraph("1.1. Single tabs in paragraph");
            paragraph.Style = StyleNames.Heading1;

            TestHelper.DrawHorizontalPosition(section, position1);
#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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
#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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

#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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

#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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

#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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

#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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

#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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

#if NET6_0_OR_GREATER
            foreach (var alignment in Enum.GetValues<TabAlignment>())
#else
            foreach (TabAlignment alignment in Enum.GetValues(typeof(TabAlignment)))
#endif
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

            var pdfFilename = PdfFileUtility.GetTempPdfFileName(filename);
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

                // Get the position of the tabstop’s position element.
                var positionPos = rtf.LastIndexOf(tabStopPositionStr, valuePos, StringComparison.Ordinal);
                positionPos.Should().BeGreaterThan(decTabStopPos, "for the decimal tabstop a position should be defined.");

                // Get the tabstop’s position value and ensure that it’s not 0.
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

                    // Get the position of the tabstop’s position element.
                    positionPos = rtf.LastIndexOf(tabStopPositionStr, decTabStopPos, StringComparison.Ordinal);
                    positionPos.Should().BeGreaterThan(leftTabStopPos, "for the left tabstop a position should be defined.");

                    // Get the tabstop’s position value and ensure that it’s 0.
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

                // Check all tabstop’s position elements for this cell or paragraph.
                var tabStopPositionSearchPos = containerPos;
                // Exit the loop if or positionPos exceeds the position of the value or if no position element could be found.
                while (tabStopPositionSearchPos < valuePos)
                {
                    var positionPos = rtf.IndexOf(tabStopPositionStr, tabStopPositionSearchPos, StringComparison.Ordinal);
                    if (positionPos < 0 || positionPos > valuePos)
                        break;

                    // Get the tabstop’s position value and ensure that it’s not 0, as no additional tabstop should be added here at position 0.
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
        [Fact]

        public void Create_Rtf_with_Image()
        {
            // Create a MigraDoc document.
            var document = new Document();

            var sect = document.AddSection();

            sect.AddImage(IOUtility.GetAssetsPath(@"PDFsharp\images\samples\bmp\Color4A.bmp")!); // Fails: Cannot load resources.
            //sect.AddImage(IOUtility.GetAssetsPath(@"\PDFsharp\images\samples\png\color8A.png")!); // OK
            //sect.AddImage(IOUtility.GetAssetsPath(@"\PDFsharp\images\samples\jpeg\color8A.jpg")!); // OK

            // Create a renderer for the MigraDoc document.
            var rtfRenderer = new RtfDocumentRenderer();

            // Save the document...
            var filename = IOUtility.GetTempFileName("HelloWorld", null);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_0.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            // Layout and render document to PDF.
            rtfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);

#if DEBUG___
            dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_1.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            //// ...and start a viewer.
            //PdfFileUtility.ShowDocumentIfDebugging(filename);

        }

        [Fact]
        public void Create_Rtf_with_Embedded_Base64Image()
        {
            // Create a MigraDoc document.
            var document = new Document();

            var sect = document.AddSection();

            AddImage(document);

            // Create a renderer for the MigraDoc document.
            var rtfRenderer = new RtfDocumentRenderer();

            // Save the document...
            var filename = IOUtility.GetTempFileName("HelloWorldEmbeddedBase64", null);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_0.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            // Layout and render document to PDF.
            rtfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);

#if DEBUG___
            dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_1.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            //// ...and start a viewer.
            //PdfFileUtility.ShowDocumentIfDebugging(filename);

        }

        static void AddImage(Document document)
        {
            var base64 = @"base64:iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAACUlBMVEUAAAABAAACAQADAgAHBAAH
BQANCAALCQARCwARDgAcFQAeFQAeGAAtHAApHgAoHwAuIQAvJQAtJwA4KAA/KQA3LABBLwBGMQBC
NABANwD/AABGOQBSNwD/BABTOAD/BQD/CAD/CQD/DABbPQBPQQD/DQD/EAD/EQBTRgD/FAD/FQBl
RQD/GQD/GgBaTAD/HQD/HgD/IQD/IgD/JQD/JgBlVAD/KQD/KgB3VAD/LQD/LgB1WAB0WQD/MgCI
VgD/MwD/NgD/NwB4YgD/OgD/OwCDYQD/PgD/PwCCZQCKYwD/QgD/QwD/RwCbZQD/SACSaQD/SwD/
TACLbwD/TwD/UAD/UwCRdAD/VACJeQD/VwD/WACtdAD/XACrdQD/YAD/YQCzeAD/ZAD/ZQD/aAD/
aQCchwD/bAD/bQD/cAD/cQDAhACojAD/dQD/dgD/eQD/egCulAD/fQD/fgD/gQCzmAD/ggD/hQD/
hgDTlADikAD/iQD/igDPnAD/jgDBoQD/jwDOngD/kgD/kwD/lgD/lwDvnADdogD/mgDjowD/mwD/
ngD/nwDUrQDpqADcrQD/owD/pAD4pwD2qAD/pwD/qAD/qwD/rAD+rwDktwD/rwD/sAD/swDougD/
tAD/twD/uAD/vAD/vQD/wADjyQD/wQD/xAD/xQD/yAD/yQD1zQDw0AD/zAD/zQD/0QD50wD/0gD+
1AD/1QD/1gD/2QD/2gD/3QD/3gD/4QD/4gD/5QD/5gD/6gD/6wD/7gD/7wD/8gD/8wD/9gD/9wD/
+gD/+wD//wBS8BVmAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3gsbDiwNWTYMaQAAAB1p
VFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAAFpUlEQVRYw3VXiV/URRydsNPu
u+gALJYgTRGDsgMIwsIyaSWii1wIqAQSYiuKkKO2sBuhFgJjk9UCMmGXq2WRNv+vZub7nZnvzG+Z
P+B9vsd77/uGZWZm5eT6CnbtLtxXsv+pssoDB184fKT2tTfeOtrY8u6x9uPdH33a2z8YOvn9qdHx
ydO/RWbOzc1fWIwvr/2zvvHvf5cusczM+7J3+Ap2AkB5ZdXBQ4f9dfUcoIkDdBzvkgBfC4AwBzhj
AJIEIK9g557ComIBwCuo8fMKGgCgUwAMDHGAEQQ4ywFi8ZW1xPrGZkoCyBZkBY9jC37ZQlOzaAEB
hjkAtjDLAZZWeAsXVQU5soW9cgYVooIjdTiDtnZZQb+sYGxcV7DoAOSKFvYWwRCrOQAMUVbQHVQz
GAuTIXKApAbIFgC75QxwC6KCQFOLaqF/KDSsKoie0xVsmBbyrBY4gN4CtqDWOI0VLIkhXrS2wIeo
tgA8CMghdgoeDAzqLUTIDDY2dQs+BQAV+Gvr9Qy6ggpgzGxhcUnyAAGycAZAJAOgZ9DXbwBwBnG5
BRtAzqAcKpBrlADYQuik4kGUt7DAAVYTSdICABTvLxVUVgCNugLJRLpGSSS+xpR3C5SJsoKuoBZT
2J4BWSNUUOSoEcQEauRURjVGoYJlSiRVAdkCEKn1GFB5YBC0MIEzkENMOFRWRKqqpkRqN3IWW5i6
g7G75vQabSLhFqo0kRpbbCKNhSe+yti+PeMXJFLSZWJRyRZM7FNUnniePfEke/m83EJC80DNQKsR
xERbgDWOT97EPjvBbp5fSOsHhUZMZo0d1ho/ZjdEojeyb9whOi0cokwkch4NP8aejcw8x56RlkaG
mJNrAMCRah0qD0kq/3wt+zIS/Y5d93csvrwqtKA8kbewizuSNhTpyspQUI0/nBp9j90qiHQ7+zwG
TNRiMn5QBmusI2tUMxgZfZi9NM0BXmGPLloAuAUzxBoxgwYjJgT48ept34oKft12zV8wg810TORD
rCFqFBX0AcDr7E6phdl72AeSB2qIWdlGC4KJsIUGcKTOLiWmkXuZfg/FLR5k77AdSdt6K+cBMJET
6YvLDcAVf4IWUpatoyNV4xbUDILIxBfZ/dLSzs7OP8jeWXGOaz6/jcKRYAt+che6lRpvY2+CnGfn
32d3r6x67kJhmtOm/IBX8MllV/6kbP2PqzJ+J0wUppoPl6m0rFKtUTBRziDYI9f4NHsAbJ1XcOER
9qrVQq6PzMBooZlepuvZ28bSPmS3rBkqE08sVadNE4kykVymuCPnPB1xKmgL2lDIaQNTjcMQUx45
g6H4UQutMuL04HHllqYAFjGhOGIqUYeFbMGsEQ9LVN4FAZBMV4Gkcg3hAW1BnzYMGB4xFVszEC20
oaVBxAlPTtnH1Y55cBsrdMQJGDkPDKKpTpHbSFxZEYmcd5uJeFic47rutGA8Ud6FhoA5rgNWyCIx
L+UJmngXai1D6SUBI2JClt5CFsa8fZapNgQ8rmwnlERyy8tUAy00wnkPmtNGzju9jXbEMURqMnIe
DKl8oHPiqjvEPUBlctqEJ3YYJuoZzPLbCC0QOeejGstJUlU86On1iMmJuhi2TcQxnthBiMTP+xTN
yjQj+WTYVo6kxNSs7gL+F+yoyyuwDcWdQaApLRNFCwsxO6UpWy82570e1tjmZCTliU4LnIn55tOl
QxbxA0Mk2cICfjg2U3bIKoZPl3YkctpCNpVjngq0I1XYQZPOgFcwNa0AltMAEDViBa3qyyNbwCGK
sB1zeJBLWjhAwnYrzoCk9TMzOupqOWd5wjb9uXZBThzWM5AVIA+2uI1KTM0mYNAfy5z7b9QfT7sF
JWfyZ5owLazYIUtQGXlQ5VwmklTDYgv042nLmX77as23r5v8XCdOT/OAMXfew0TjB0TOR52AIXng
VeP/8gP+s//MzMQAAAAASUVORK5CYII=
";

            var logo = document.LastSection.AddImage(base64);
        }

#if !CORE
        // Not supported by Core build.
        [Theory]
        [InlineData(@"bmp\Test_OS2.bmp")]
        [InlineData(@"png\color4A.png")]
#else
        [Theory]
#endif
        [InlineData(@"jpeg\windows7problem.jpg")]
        [InlineData(@"jpeg\PowerBooks_CMYK.jpg")]
        [InlineData(@"jpeg\grayscaleA.jpg")]
        [InlineData(@"jpeg\Balloons_CMYK.jpg")]
        [InlineData(@"jpeg\Balloons_CMYK - Copy.jpg")]

        [InlineData(@"bmp\BlackwhiteA.bmp")]
        [InlineData(@"bmp\Color4A.bmp")]
        [InlineData(@"bmp\Color8A.bmp")]
        [InlineData(@"bmp\TruecolorA.bmp")]

        [InlineData(@"png\truecolorAlpha.png")]
        [InlineData(@"png\color8A.png")]

        [InlineData(@"MigraDoc.bmp")]
        [InlineData(@"Logo landscape.bmp")]
        [InlineData(@"Logo landscape.png")]
        [InlineData(@"Logo landscape 256.png")]
        public void Create_Rtf_with_Base64Image(string assetName)
        {
            // Create a MigraDoc document.
            var document = new Document();

            var sect = document.AddSection();

            var imagePath = IOUtility.GetAssetsPath(@"PDFsharp\images\samples\" + assetName)!;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                imagePath = imagePath.Replace('\\', '/');

            var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            //Use BinaryReader to read file stream into byte array.
            var br = new BinaryReader(stream, Encoding.Default);

            //When you use BinaryReader, you need to supply number of bytes to read from file.
            //In this case we want to read entire file. So supplying total number of bytes.
            var imageData = br.ReadBytes((int)stream.Length);

            var image = "base64:" + Convert.ToBase64String(imageData);

            sect.AddParagraph(assetName);
            sect.AddImage(image);
            sect.AddParagraph(assetName);

            // Create a renderer for the MigraDoc document.
            var rtfRenderer = new RtfDocumentRenderer();

            // Save the document...
            var filename = IOUtility.GetTempFileName("HelloWorldBase64", null);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_0.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            // Layout and render document to PDF.
            rtfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);

#if DEBUG___
            dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_1.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif

            //// ...and start a viewer.
            //PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Test_Heading_Border()
        {
            var bottomWidth = Unit.FromPoint(2.3);
            var bottomColor = Colors.Blue;
            var rtfBottomString = "\\brdrs\\brdrw46\\brdrcf2";

            var headingBottomWidth = Unit.FromPoint(4.6);
            var headingBottomColor = Colors.Red;
            var rtfHeadingBottomString = "\\brdrs\\brdrw92\\brdrcf3";

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

            var rtfFilename = IOUtility.GetTempFileName("Test_Heading_Border", "rtf");
            var rtfRenderer = new RtfDocumentRenderer();
            rtfRenderer.Render(document, rtfFilename, Environment.CurrentDirectory);

            // Analyze rendered RTF.
            var rtf = File.ReadAllText(rtfFilename);

#if NET6_0_OR_GREATER
            // Split by row identifier and skip the first part, which is no row.
            var splitByRows = rtf.Split("\\trowd").Skip(1).ToArray();
            splitByRows.Length.Should().Be(5, "as there are 5 rows");

            // Get the part before the padding identifier, split it by border identifier and skip the first part, which is no border.
            var rowsByBorders = splitByRows
                .Select(row => row.Split("\\clpad").First()
                    .Split("\\clbrdr").Skip(1).ToArray()
                ).ToArray();
            foreach (var borders in rowsByBorders)
                borders.Length.Should().Be(4, "as there are 4 borders defined");
#else
            // Split by row identifier and skip the first part, which is no row.
            var splitByRows = rtf.Splitter("\\trowd").Skip(1).ToArray();
            splitByRows.Length.Should().Be(5, "as there are 5 rows");

            // Get the part before the padding identifier, split it by border identifier and skip the first part, which is no border.
            var rowsByBorders = splitByRows
                .Select(row => row.Splitter("\\clpad").First()
                    .Splitter("\\clbrdr").Skip(1).ToArray()
                ).ToArray();
            foreach (var borders in rowsByBorders)
                borders.Length.Should().Be(4, "as there are 4 borders defined");
#endif

            // Heading row.
            var rowBorderParts = rowsByBorders[0];

            var topBorderPart = rowBorderParts[0];
            topBorderPart.Should().StartWith("t", "first border should be top border");
            topBorderPart.Length.Should().Be(1, "heading top border should not be defined and only contain the top identifier");

            var bottomBorderPart = rowBorderParts[3];
            bottomBorderPart.Should().StartWith("b", "last border should be bottom border");
            bottomBorderPart[1..].Should().Be(rtfHeadingBottomString, "heading bottom border should be defined heading bottom border");

            // Row 1.
            rowBorderParts = rowsByBorders[1];

            topBorderPart = rowBorderParts[0];
            topBorderPart.Should().StartWith("t", "first border should be top border");
            topBorderPart.Length.Should().Be(1, "row 1 top border should not be defined and only contain the top identifier, as the heading bottom border must not affect a content border," +
                                                "to not copy heading border values when moving or inserting rows in RTF application.");

            bottomBorderPart = rowBorderParts[3];
            bottomBorderPart.Should().StartWith("b", "last border should be bottom border");
            bottomBorderPart[1..].Should().Be(rtfBottomString, "row 1 bottom border should be defined content bottom border");

            // Row 2-4.
            for (var r = 2; r < 5; r++)
            {
                rowBorderParts = rowsByBorders[r];

                topBorderPart = rowBorderParts[0];
                topBorderPart.Should().StartWith("t", "first border should be top border");
                topBorderPart[1..].Should().Be(rtfBottomString, $"row {r} top border should be the defined content bottom border of the top neighbor row");

                bottomBorderPart = rowBorderParts[3];
                bottomBorderPart.Should().StartWith("b", "last border should be bottom border");
                bottomBorderPart[1..].Should().Be(rtfBottomString, $"row {r} bottom border should be defined content bottom border");
            }
        }
    }
}
