// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Globalization;
using FluentAssertions;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using PdfSharp.Fonts;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace MigraDoc.Tests
{
    [Collection("MGD")]
    public class TextTests
    {
        [Fact]
        public void Surrogate_Pairs_Test()
        {
#if CORE
            GlobalFontSettings.FontResolver = SnippetsFontResolver.Get();
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
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileHelper.CreateTempFileName("HelloEmoji");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);

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

            paragraph = section.AddParagraph("111😢😞💪");
            paragraph.Format.Font.Name = "Segoe UI Emoji";
            paragraph.AddLineBreak();
            paragraph.AddText("💩💩💩✓✔✅🐛👌🆗🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻");

            paragraph = section.AddParagraph("111😢😞💪");
            paragraph.Format.Font.Name = "Segoe UI Emoji";
            paragraph.AddLineBreak();
            paragraph.AddText("💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕🖕🖕🖕 🦄 🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇 🍆🍆🍆🍆🍆🍆🍆🍆🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂🦂🦂🦂🦂🦂 🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻" +
                "💩💩💩✓✔✅ 🐛👌🆗🖕 🦄 🦂 🍇🍇🍇🍇🍇🍇🍇🍇🍇🍇 🍆 ☕ 🚂 \U0001f6f8 ☁ ☢ ♌ ♏ ✅ ☑ ✔ ™ 🆒 ◻");

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
#if CORE
            GlobalFontSettings.FontResolver = SnippetsFontResolver.Get();
#endif

            // Create a new MigraDoc document.
            var document = new Document();

            var section = document.AddSection();
            var paragraph = section.AddParagraph("No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   12345  " +
                                                 "No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   12345  " +
                                                 "No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   No\u2011break\u2011hyphen-Test   12345  ");
            paragraph.Format.Font.Name = "Segoe UI";
            paragraph.Format.Font.Bold = true;


            var filename = PdfFileHelper.CreateTempFileNameWithoutExtension("DocumentWithNoBreakHyphen");

            var pdfFilename = filename + ".pdf";
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var pdfDocument = pdfRenderer.PdfDocument;
            pdfDocument.Options.CompressContentStreams = false;
            pdfRenderer.Save(pdfFilename);

            PdfFileHelper.StartPdfViewerIfDebugging(pdfFilename);

            var rtfRenderer = new RtfDocumentRenderer();
            rtfRenderer.Render(document, filename + ".rtf", Environment.CurrentDirectory);


            // Analyze the drawn text in the PDF's content stream.
            var pdfPage = pdfDocument.Pages[0];
            var contentReference = (PdfReference)pdfPage.Contents.Elements.Items[0];
            var content = (PdfDictionary)contentReference.Value;
            var contentStream = content.Stream.ToString();
            var contentLines = contentStream.Split('\n');

            // Get the PDF encoded text between "Td <" and "> Tj" from contentLines.
            const string startTag = "Td <";
            const string endTag = "> Tj";
            var textObjectLines = contentLines.Where(l => l.Contains(startTag) && l.Contains(endTag));

            var lastObjectEndsWithHyphen = false;
            var pageContainsHyphenBreak = false;

            foreach (var textObjectLine in textObjectLines)
            {
                var parts = textObjectLine.Split(' ');

                var textPart = parts[3];

                var pdfEncodedText = textPart.Trim('<', '>');

                // Separate the PDF encoded text in its 4 digit byte representations.
                var pdfEncodedChars = pdfEncodedText.Chunk(4).Select(x => new String(x)).ToList();

                var yMovement = double.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat);
                var isNewLine = yMovement != 0;


                pdfEncodedChars.Should().NotContain("0000", "no char (and no no-break hyphen should be converted to an invalid glyph");

                var count = pdfEncodedChars.Count;
                var endsWithHyphen = pdfEncodedChars[count - 1] == "0010";
                if (endsWithHyphen)
                    pdfEncodedChars[count - 2].Should().Be("0051", "in text objects ending with a hyphen the char before should be an 'n', because the other hyphens where no-break hyphens");

                if (isNewLine && lastObjectEndsWithHyphen)
                {
                    pageContainsHyphenBreak = true;
                    pdfEncodedChars.First().Should().Be("0037", "a new line after a hyphen should continue with a 'T', because the other hyphens where no-break hyphens");
                }

                lastObjectEndsWithHyphen = endsWithHyphen;
            }

            lastObjectEndsWithHyphen.Should().Be(false, "each line ending with a hyphen should be followed by another line");
            pageContainsHyphenBreak.Should().Be(true, "test should contain a line break at a hyphen to show that breaks occur at hyphens while not occurring at no-break hyphens");
        }

        [Fact]
        public static void DecimalTabulatorTest()
        {
#if CORE
            GlobalFontSettings.FontResolver = SnippetsFontResolver.Get();
#endif
            var cultureInfos = new[] { null, CultureInfo.GetCultureInfo("en-us"), CultureInfo.GetCultureInfo("de-de") };

            // Numbers separated by page breaks to let all numbers of a page start at the same x position.
            // For easier analysis, '1' should and should only occur at the first digit of each number.
            var numbersByPage = new List<List<double>>
            {
                new() {1, 1.3, 1.33},
                new() {1234, 1234.4, 1234.44},
                new() {.3},
                new() {-1, -1.3, -1.33},
                new() {-1234, -1234.4, -1234.44},
                new() {-.3}
            };

            var units = new[] { null, "%", " %", "mm", " mm" };

            // Simulate references to footnotes by superscript text numbers.
            var appendReferences = new[]
            {
                new Action<Paragraph>(_ => {}),
                // For numbers without decimal separator that are directly followed by a FormattedText conatining a superscript number, e. g. "1⁹", Word interprets the superscript text as an exponent.
                // As a result the superscript is positioned before the decimal tabstop in RTF. The PDF renderer positions the superscript text at the decimal tabstop instead,
                // as it can currently take only a single Text object for alignment position calculation.
                // As the intention of the superscript text is not clear, none of these positionings is wrong.
                // For alignment before the decimal tabstop as an exponent in PDF, use superscript digit unicode characters instead of FormattedText.
                // For alignment at the decimal tabstop as footnote reference in RTF, insert e. g. a Hair Space as first character of the superscript.
                new Action<Paragraph>(p => p.AddFormattedText("9").Superscript = true),
                new Action<Paragraph>(p =>
                {
                    var t = (Text)p.Elements.LastObject!;
                    t.Content += " ";
                    p.AddFormattedText("9").Superscript = true;
                })
            };

            var filenamePattern = PdfFileHelper.CreateTempFileName("DecimalTabulator{0}", "{1}");

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

                PdfFileHelper.StartPdfViewerIfDebugging(pdfFilename);

                // In RTF, decimal tabstop alignment is done in the viewing application and always depends on the system regional settings,
                // as culture or separators cannot be saved in rtf. Therefore we don't need to assign cultureInfo and output RTF only once.
                if (cultureInfo == null)
                {
                    var rtfFilename = String.Format(filenamePattern, null, "rtf");
                    var rtfRenderer = new RtfDocumentRenderer();
                    rtfRenderer.Render(document, rtfFilename, Environment.CurrentDirectory);
                }


                // Analyze the drawn text in the PDF's content stream.

                // The horizontal offset of the first line ("0.0").
                var firstLineXOffset = 150.344;
                // The horizontal offsets of the numbers on the page relatively to the position of the first line.
                var expectedFirstOneXOffsetsByPage = new[] { 0, -19.4629, 5.5615, -3.3301, -22.793, 2.2314 };

                for (var pageIdx = 0; pageIdx < expectedFirstOneXOffsetsByPage.Length; pageIdx++)
                {
                    var expectedFirstOneXOffset = expectedFirstOneXOffsetsByPage[pageIdx];

                    var pdfPage = pdfDocument.Pages[pageIdx];
                    var contentReference = (PdfReference)pdfPage.Contents.Elements.Items[0];
                    var content = (PdfDictionary)contentReference.Value;
                    var contentStream = content.Stream.ToString();
                    var contentLines = contentStream.Split('\n');

                    // Get the PDF encoded text between "Td <" and "> Tj" from contentLines.
                    const string startTag = "Td <";
                    const string endTag = "> Tj";
                    var textObjectLines = contentLines.Where(l => l.Contains(startTag) && l.Contains(endTag)).ToList();

                    var currentOffsetRelativeToFirstOne = 0d;
                    const double precision = 0.001;

                    var pageNr = pageIdx + 1;
                    var numberLineCount = pageNr % 3 == 0 ? 16 : 46; // Page 3 and 6 have less lines.
                    var textObjectLinesCount = textObjectLines.Count;
                    textObjectLinesCount.Should().BeGreaterOrEqualTo(numberLineCount,
                        $"the number of the text object lines should be equal or greater than the number of text lines they represent ({numberLineCount} lines on page {pageNr})");

                    // Attention: Some lines in the PDF consist of multiple object lines.
                    for (var lineObjectIdx = 0; lineObjectIdx < textObjectLinesCount; lineObjectIdx++)
                    {
                        var textObjectLine = textObjectLines[lineObjectIdx];

                        var parts = textObjectLine.Split(' ');

                        var xOffsetStr = parts[0];
                        var xOffset = Double.Parse(xOffsetStr, CultureInfo.InvariantCulture);
                        var textPart = parts[3];

                        if (lineObjectIdx == 0)
                        {
                            xOffset.Should().BeApproximately(firstLineXOffset, precision, $"first text line's (\"0.0\") horizontal offset should be '{firstLineXOffset}'");
                            continue;
                        }
                        if (lineObjectIdx == 1)
                        {
                            xOffset.Should().BeApproximately(expectedFirstOneXOffset, precision,
                                $"horizontal offset of the first text line starting with '1' or '-' should be '{expectedFirstOneXOffset}'");
                            continue;
                        }

                        // Beginning with line object 2, currentOffsetRelativeToFirstOne is changed by each line's horizontal offset.
                        currentOffsetRelativeToFirstOne += xOffset;

                        // Use '1' and '-' to recognize real line beginnings and check their horizontal offset relative to the first line starting with '1' or '-'.
                        var textStartsWithOne = textPart.StartsWith("<0014");
                        var textStartsWithMinus = textPart.StartsWith("<0010");
                        if (textStartsWithOne || textStartsWithMinus)
                            currentOffsetRelativeToFirstOne.Should().BeApproximately(0, precision,
                                "for text lines staring with '1' or '-' relative horizontal offset to the first line starting with '1' or '-' should be '0'");

                    }
                }
            }
        }

        [Fact]
        static void FooterLayoutTest()
        {
#if CORE
            GlobalFontSettings.FontResolver = SnippetsFontResolver.Get();
#endif
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
                PdfDocument = new PdfDocument
                {
                    // PageLayout = PdfPageLayout.SinglePage
                }
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = IOHelper.CreateTemporaryPdfFileName("FooterTest");
            pdfRenderer.PdfDocument.Save(filename);
            //// ...and start a viewer.
            //Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
    }
}
