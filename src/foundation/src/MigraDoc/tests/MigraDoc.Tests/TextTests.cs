// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using PdfSharp.Fonts;
using PdfSharp.Logging;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;
using FluentAssertions;

namespace MigraDoc.Tests
{
    [Collection("PDFsharp")]
    public class TextTests
    {
        [Fact]
        public void Surrogate_Pairs_Test()
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
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("HelloEmoji");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);

#if DEBUG___
            MigraDoc.DocumentObjectModel.IO.DdlWriter dw = new MigraDoc.DocumentObjectModel.IO.DdlWriter(filename + "_2.mdddl");
            dw.WriteDocument(document);
            dw.Close();
#endif
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

        [Fact]
        public void Document_with_No_Break_Hyphen()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            var section = document.AddSection();
            var paragraph = section.AddParagraph("No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test 12345 " +
                                                 "No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test 12345 " +
                                                 "No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test No\u2011break\u2011hyphen-Test 12345 ");
            paragraph.Format.Font.Bold = true;

            var filename = IOUtility.GetTempFileName("DocumentWithNoBreakHyphen", null);

            var pdfFilename = filename + ".pdf";
            var pdfRenderer = new PdfDocumentRenderer { Document = document };

            var hasReplaced = false;

            pdfRenderer.PdfDocument.RenderEvents.RenderTextEvent += (sender, args) =>
                {
                    var length = args.CodePointGlyphIndexPairs.Length;
                    for (var idx = 0; idx < length; idx++)
                    {
                        ref var item = ref args.CodePointGlyphIndexPairs[idx];
                        if (item is { GlyphIndex: 0, CodePoint: '\u2011' })
                        {
                            item.CodePoint = '-';
                            args.ReevaluateGlyphIndices = true;
                            hasReplaced = true;
                        }
                    }
                };
            pdfRenderer.RenderDocument();

            hasReplaced.Should().BeTrue();

            var pdfDocument = pdfRenderer.PdfDocument;
            pdfDocument.Options.CompressContentStreams = false;
            pdfRenderer.Save(pdfFilename);

            PdfFileUtility.ShowDocumentIfDebugging(pdfFilename);

            var rtfRenderer = new RtfDocumentRenderer();
            rtfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);


            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, 0);

            streamEnumerator.Text.MoveAndGetNext(true, out var textInfo).Should().BeTrue();

            double lineYPosition = 0;

            var lastObjectEndsWithHyphen = false;
            var pageContainsHyphenBreak = false;

            do
            {
                var isNewLine = !textInfo!.IsAtYPosition(lineYPosition);
                if (isNewLine)
                    lineYPosition = textInfo.Y;

                var text = textInfo.Text;
                text.Should().NotBeNull();

                var isHex = textInfo.IsHex;
                if (isHex)
                {
                    var glyphIds = PdfFileHelper.GetHexStringAsGlyphIndices(text);
                    glyphIds.Should().NotContain("0", "no char (and no no-break hyphen) should be converted to an invalid glyph (\"0\")");
                }
                isHex.Should().BeFalse("for strings without not available characters we expect literal strings here for further investigation");

                var length = text.Length;
                var endsWithHyphen = text[length - 1] == '-';
                if (endsWithHyphen)
                    text[length - 2].Should().Be('n', "in text objects ending with a hyphen, the char before should be an 'n', because the other hyphens where no-break hyphens");

                if (isNewLine && lastObjectEndsWithHyphen)
                {
                    pageContainsHyphenBreak = true;
                    text.First().Should().Be('T', "a new line after a hyphen should continue with a 'T', because the other hyphens where no-break hyphens");
                }

                lastObjectEndsWithHyphen = endsWithHyphen;
            } while (streamEnumerator.Text.MoveAndGetNext(true, out textInfo));

            lastObjectEndsWithHyphen.Should().Be(false, "each line ending with a hyphen should be followed by another line");
            pageContainsHyphenBreak.Should().Be(true, "test should contain a line break at a hyphen to show that breaks occur at hyphens while not occurring at no-break hyphens");
        }

        [Fact]
        public static void Document_with_No_Break_Hyphen_before_Tabs()
        {
            LogHost.Factory = LoggerFactory.Create(builder => builder.AddConsole());

            // Create a new MigraDoc document.
            var document = new Document();

            var style = document.Styles[StyleNames.Normal];
            style!.Font.Name = "Arial";

            style = document.Styles[StyleNames.Heading1];
            style!.Font.Size = 20;
            style.ParagraphFormat.TabStops.ClearAll();
            style.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(2));

            style = document.Styles[StyleNames.Heading2];
            style!.Font.Size = 16;
            style.ParagraphFormat.TabStops.ClearAll();
            style.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(2));

            var section = document.AddSection();

            var paragraph = section.AddParagraph("A");
            paragraph.Style = StyleNames.Heading1;
            paragraph.AddTab();
            paragraph.AddText("Heading level 1 without any hyphen");

            paragraph = section.AddParagraph("A-1");
            paragraph.AddTab();
            paragraph.AddText("Heading level 2 with usual hyphen");
            paragraph.Style = StyleNames.Heading2;

            paragraph = section.AddParagraph("A\u20111");
            paragraph.AddTab();
            paragraph.AddText("Heading level 2 with no-break hyphen replaced by hyphen character");
            paragraph.Style = StyleNames.Heading2;

            var filename = IOUtility.GetTempFileName("DocumentWithNoBreakHyphenBeforeTabs", null);

            var pdfFilename = filename + ".pdf";
            var pdfRenderer = new PdfDocumentRenderer { Document = document };

            pdfRenderer.PdfDocument.RenderEvents.RenderTextEvent += (sender, args) =>
            {
                var length = args.CodePointGlyphIndexPairs.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    ref var item = ref args.CodePointGlyphIndexPairs[idx];
                    if (item is { CodePoint: '\u2011', GlyphIndex: 0 })
                    {
                        item.CodePoint = '-';
                        item.GlyphIndex = GlyphHelper.GlyphIndexFromCodePoint('-', args.Font);
                    }
                }
            };

            pdfRenderer.RenderDocument();

            var pdfDocument = pdfRenderer.PdfDocument;
            pdfDocument.Options.CompressContentStreams = false;
            pdfRenderer.Save(pdfFilename);

            PdfFileUtility.ShowDocumentIfDebugging(pdfFilename);

            var rtfRenderer = new RtfDocumentRenderer();
            rtfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);

            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, 0);

            // Ensure that all Heading text objects are positioned at the same x value.
            streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Heading", true, out var textInfo).Should().BeTrue();
            var firstHeadingTabX = textInfo!.X;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Heading", true, out textInfo).Should().BeTrue();
            textInfo!.IsAtXPosition(firstHeadingTabX).Should().BeTrue();

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Heading", true, out textInfo).Should().BeTrue();
            textInfo!.IsAtXPosition(firstHeadingTabX).Should().BeTrue();
        }

        [Fact]
        public static void DecimalTabulatorTest()
        {
            var cultureInfos = new[] { null, CultureInfo.GetCultureInfo("en-us"), CultureInfo.GetCultureInfo("de-de") };

            // Numbers separated by page breaks to let all numbers of a page start at the same x position.
            // For easier analysis, '1' should and should only occur at the first digit of each number.
            var numbersByPage = new List<List<double>>
            {
                new() { 1, 1.3, 1.33 },
                new() { 1234, 1234.4, 1234.44 },
                new() { .3 },
                new() { -1, -1.3, -1.33 },
                new() { -1234, -1234.4, -1234.44 },
                new() { -.3 }
            };

            var units = new[] { null, "%", " %", "mm", " mm" };

            // Simulate references to footnotes by superscript text numbers.
            var appendReferences = new[]
            {
                new Action<Paragraph>(_ => {}),
                // For numbers without decimal separator that are directly followed by a FormattedText containing a superscript number, e.g. "1⁹", Word interprets the superscript text as an exponent.
                // As a result the superscript is positioned before the decimal tabstop in RTF. The PDF renderer positions the superscript text at the decimal tabstop instead,
                // as it can currently take only a single Text object for alignment position calculation.
                // As the intention of the superscript text is not clear, none of these positions is wrong.
                // For alignment before the decimal tabstop as an exponent in PDF, use superscript digit unicode characters instead of FormattedText.
                // For alignment at the decimal tabstop as footnote reference in RTF, insert e.g. a Hair Space as first character of the superscript.
                new Action<Paragraph>(p => p.AddFormattedText("9").Superscript = true),
                new Action<Paragraph>(p =>
                {
                    var t = (Text)p.Elements.LastObject!;
                    t.Content += " ";
                    p.AddFormattedText("9").Superscript = true;
                })
            };

            var filenamePattern = IOUtility.GetTempFileName("DecimalTabulator{0}", "{1}");

            foreach (var cultureInfo in cultureInfos)
            {
                // Create a new MigraDoc document.
                var document = new Document { Culture = cultureInfo };

                var section = document.AddSection();

                for (var pageIdx = 0; pageIdx < numbersByPage.Count; pageIdx++)
                {
                    if (pageIdx != 0)
                        section.AddPageBreak();

                    var p = section.AddParagraph();
                    p.AddTab();
                    p.AddText(0.0.ToString("0.0", cultureInfo));
                    p.Format.TabStops.AddTabStop(Unit.FromCentimeter(3), TabAlignment.Decimal);

                    foreach (var number in numbersByPage[pageIdx])
                    {
                        var numberStr = number.ToString("#,###.##", cultureInfo);

                        foreach (var unit in units)
                        {
                            var numberAndUnitStr = numberStr + unit;

                            foreach (var appendReference in appendReferences)
                            {
                                p = section.AddParagraph();
                                p.AddTab();
                                p.AddText(numberAndUnitStr);
                                appendReference(p);
                                p.Format.TabStops.AddTabStop(Unit.FromCentimeter(3), TabAlignment.Decimal);
                            }
                        }
                    }
                }

                var pdfFilename = String.Format(filenamePattern, "-" + (cultureInfo?.Name ?? "CurrentCulture"), "pdf");
                var pdfRenderer = new PdfDocumentRenderer { Document = document };
                pdfRenderer.RenderDocument();

                var pdfDocument = pdfRenderer.PdfDocument;
                pdfDocument.Options.CompressContentStreams = false;
                pdfRenderer.Save(pdfFilename);

                PdfFileUtility.ShowDocumentIfDebugging(pdfFilename);

                // In RTF, decimal tabstop alignment is done in the viewing application and always depends on the system regional settings,
                // as culture or separators cannot be saved in rtf. Therefore, we don’t need to assign cultureInfo and output RTF only once.
                if (cultureInfo == null)
                {
                    var rtfFilename = String.Format(filenamePattern, null, "rtf");
                    var rtfRenderer = new RtfDocumentRenderer();
                    rtfRenderer.Render(document, rtfFilename, Environment.CurrentDirectory);
                }


                // Analyze the drawn text in the PDF’s content stream.

                // The horizontal position of the first line ("0.0").
                var firstLineX = 150.344;
                // The horizontal position of the other lines.
                var expectedSecondLineXOffsetsByPage = new[] { 0, -19.4629, 5.5615, -3.3301, -22.793, 2.2314 };

                for (var pageIdx = 0; pageIdx < expectedSecondLineXOffsetsByPage.Length; pageIdx++)
                {
                    var pageNr = pageIdx + 1;
                    var expectedSecondLineXOffset = expectedSecondLineXOffsetsByPage[pageIdx];
                    var expectedOtherLinesX = firstLineX + expectedSecondLineXOffset;

                    var expectedLineCount = pageNr % 3 == 0 ? 16 : 46; // Page 3 and 6 have fewer lines.
                    var lineCount = 0;
                    double lineYPosition = 0;
                    var lineHeight = 11.499;

                    var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, pageIdx);
                    while (streamEnumerator.Text.MoveAndGetNext(true, out var textInfo))
                    {
                        var isNewLine = lineCount == 0 || textInfo!.IsAtYPosition(lineYPosition - lineHeight);
                        if (isNewLine)
                        {
                            lineCount++;
                            lineYPosition = textInfo!.Y;

                            if (lineCount == 1)
                            {
                                var isAtFirstLineX = textInfo.IsAtXPosition(firstLineX);
                                isAtFirstLineX.Should().BeTrue($"first text line’s (\"0.0\") horizontal offset should be '{firstLineX}' instead of '{textInfo.X}'");
                                continue;
                            }

                            var isAtOtherLinesX = textInfo.IsAtXPosition(expectedOtherLinesX);
                            isAtOtherLinesX.Should().BeTrue($"the other line’s horizontal offset should be '{expectedOtherLinesX}' instead of '{textInfo.X}'");
                        }
                    }

                    lineCount.Should().Be(expectedLineCount,
                        $"the expected number of text lines on page {pageNr} is {expectedLineCount}");
                }
            }
        }

        [Fact]
        static void FooterLayoutTest()
        {
            var document = new Document();
            var Section = document.AddSection();
            Section.PageSetup.PageWidth = "210mm";
            Section.PageSetup.PageHeight = "297mm";
            Section.PageSetup.TopMargin = 0;
            Section.PageSetup.BottomMargin = Unit.FromCentimeter(3);
            Section.PageSetup.FooterDistance = 0;
            Section.PageSetup.HeaderDistance = 0;
            //for (int i = 0; i < 77; i++)
            //    Section.AddParagraph("paragraph " + i);
            var par1 = Section.Footers.Primary.AddParagraph("Unexpected blank space after footer text 1");
            par1.Format.SpaceBefore = Unit.FromCentimeter(2);
            par1.Format.SpaceAfter = 0;
            //par1 = Section.Footers.Primary.AddParagraph("Unexpected blank space after footer text 2");
            //par1 = Section.Footers.Primary.AddParagraph("Unexpected blank space after footer text 3");
            var pdfRenderer = new PdfDocumentRenderer
            {
                Document = document,
                PdfDocument =
                {
                    // PageLayout = PdfPageLayout.SinglePage
                }
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("FooterTest");
            pdfRenderer.PdfDocument.Save(filename);
            //// ...and start a viewer.
            //Process.Sta/rt(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
    }
}
