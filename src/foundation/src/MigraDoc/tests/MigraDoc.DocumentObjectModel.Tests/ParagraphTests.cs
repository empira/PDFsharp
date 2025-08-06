// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.TestHelper;
using PdfSharp.Quality;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using Xunit;
#if CORE
#endif
using FluentAssertions;
using PdfSharp.Fonts;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class ParagraphTests : IDisposable
    {
        public ParagraphTests()
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
        public void Test_Empty_Paragraph()
        {
            var document = new Document();
            var section = document.AddSection();

            // Empty ParagraphElements in Paragraph may cause problems.
            section.AddParagraph();

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_Empty_Paragraph");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Test_Empty_FormattedText()
        {
            var document = new Document();
            var section = document.AddSection();

            // Empty ParagraphElements in FormattedText may cause problems.
            var p = section.AddParagraph();
            p.AddFormattedText();

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileUtility.GetTempPdfFileName("Test_Empty_FormattedText");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        /// <summary>
        /// Creates a series of documents with a paragraph containing several lines and top and bottom border.
        /// Another paragraph with a height increasing from document to document, moves the test paragraph by this offset
        /// and forces a page break at different positions. 
        /// </summary>
        [Fact]
        public void Test_Multiline_Border_Paragraph_PageBreaks()
        {
            // Create one document containing all results.
            var sumDoc = new PdfDocument();
            var exceptions = new List<Exception>();

            // Create documents for different offsets.
            for (var offset = 15; offset <= 95; offset += 5)
            {
                // As the original problem, this test is written for, occurred only if there was only one word after the line breaks, we try it separately with one and multiple words.
                for (var i = 0; i < 2; i++)
                {
                    var isOneWord = i == 0;
                    var oneOrMultipleWordsStr = (isOneWord ? "one word" : "multiple words") + " after line breaks";

                    try
                    {
                        var document = new Document();

                        var style = document.Styles[StyleNames.Heading1];
                        style!.Font.Size = Unit.FromPoint(14);
                        style.Font.Bold = true;

                        var section = document.AddSection();

                        // Leave 10 cm for content.
                        section.PageSetup.TopMargin = Unit.FromCentimeter(5);
                        section.PageSetup.BottomMargin = Unit.FromCentimeter(14.7);

                        // Add informational content.
                        var tf = section.AddTextFrame();
                        tf.RelativeHorizontal = RelativeHorizontal.Margin;
                        tf.RelativeVertical = RelativeVertical.Margin;
                        tf.Top = Unit.FromCentimeter(-1.5);
                        tf.Left = 0;
                        tf.Height = Unit.FromCentimeter(1);
                        tf.Width = Unit.FromCentimeter(16);
                        tf.FillFormat.Color = Colors.LightGreen;
                        var p = tf.AddParagraph($"Offset: {offset} mm, {oneOrMultipleWordsStr}");
                        p.Style = StyleNames.Heading1;

                        tf = section.AddTextFrame();
                        tf.RelativeHorizontal = RelativeHorizontal.Margin;
                        tf.RelativeVertical = RelativeVertical.Margin;
                        tf.Top = 0;
                        tf.Left = Unit.FromCentimeter(-1.5);
                        tf.Height = Unit.FromCentimeter(10);
                        tf.Width = Unit.FromCentimeter(0.5);
                        tf.FillFormat.Color = Colors.LightGreen;
                        tf.Orientation = TextOrientation.Upward;
                        p = tf.AddParagraph("10 cm space for content");
                        p.Format.Alignment = ParagraphAlignment.Center;

                        // Add offset inserting paragraph.
                        p = section.AddParagraph($"Paragraph to achieve an offset of {offset} mm.");
                        p.Format.LineSpacingRule = LineSpacingRule.Exactly;
                        p.Format.LineSpacing = Unit.FromMillimeter(offset);
                        p.Format.Shading.Color = Colors.LightGray;

                        // Add test paragraph.
                        var spaceOrNoSpace = isOneWord ? "" : " ";
                        p = section.AddParagraph("Paragraph with four 1 cm lines and 1.5 cm top (green) and bottom (red) border.");
                        p.AddLineBreak();
                        p.AddText($"Second{spaceOrNoSpace}line");
                        p.AddLineBreak();
                        p.AddText($"Third{spaceOrNoSpace}line");
                        p.AddLineBreak();
                        p.AddText($"Fourth{spaceOrNoSpace}line");
                        p.Format.LineSpacingRule = LineSpacingRule.Exactly;
                        p.Format.LineSpacing = Unit.FromCentimeter(1);
                        p.Format.Shading.Color = Colors.LightBlue;

                        // Set test paragraph borders.
                        var border = p.Format.Borders.Top;
                        border.Width = Unit.FromCentimeter(1.5);
                        border.Color = Colors.Green;

                        border = p.Format.Borders.Bottom;
                        border.Width = Unit.FromCentimeter(1.5);
                        border.Color = Colors.Red;

                        // Render document and add it to sumDoc.
                        var pdfRenderer = new PdfDocumentRenderer { Document = document };
                        pdfRenderer.RenderDocument();

                        var stream = new MemoryStream();
                        pdfRenderer.PdfDocument.Save(stream);

                        var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                        foreach (var page in pdfDocument.Pages)
                            sumDoc.AddPage(page);

                        // Always add an even count of pages for better comparability in PDF reader.
                        if (pdfDocument.PageCount % 2 == 1)
                            sumDoc.AddPage();

                    }
                    catch (Exception ex)
                    {
                        var message = $"Exception while generating test document with {offset} mm offset and {oneOrMultipleWordsStr}.";

                        // Add exception to list to continue tests and throw one AggregatedException at the end.
                        exceptions.Add(new Exception(message, ex));

                        // Create temporary document with the exception and stacktrace.
                        var document = new Document();

                        var style = document.Styles[StyleNames.Normal];
                        style!.Font.Color = Colors.Red;
                        style.ParagraphFormat.SpaceAfter = Unit.FromMillimeter(5);

                        style = document.Styles[StyleNames.Heading1];
                        style!.Font.Size = Unit.FromPoint(14);
                        style.Font.Bold = true;

                        var section = document.AddSection();
                        var p = section.AddParagraph(message);
                        p.Style = StyleNames.Heading1;
                        section.AddParagraph(ex.Message);
                        section.AddParagraph(ex.StackTrace ?? "Empty stacktrace");

                        // Render document and add it to sumDoc.
                        var pdfRenderer = new PdfDocumentRenderer { Document = document };
                        pdfRenderer.RenderDocument();

                        var stream = new MemoryStream();
                        pdfRenderer.PdfDocument.Save(stream);

                        var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                        foreach (var page in pdfDocument.Pages)
                            sumDoc.AddPage(page);

                        // Always add an even count of pages for better comparability in PDF reader.
                        if (pdfDocument.PageCount % 2 == 1)
                            sumDoc.AddPage();
                    }
                }
            }

            // Save sumDoc.
            var filename = PdfFileUtility.GetTempPdfFileName("Test_Multiline_Border_Paragraph_PageBreaks");
            sumDoc.PageLayout = PdfPageLayout.TwoPageLeft;
            sumDoc.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Finally throw occurred exceptions.
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Creates a series of documents with a bordered paragraph, that is positioned to force a page break at the height of its DistanceFromBottom spacing.
        /// The paragraph ends with different objects, that partly caused exceptions before.
        /// </summary>
        [Fact]
        public void Test_Trailing_Objects_Border_Paragraph_PageBreak()
        {
            var trailingObjects = new Dictionary<DocumentObject, string>
            {
                { new Text(" "), "Text with one space only" },
                { new Character { SymbolName = SymbolName.Blank }, "Blank Character"},
                { new Character { SymbolName = SymbolName.Em }, "Em blank Character"},
                { new Character { SymbolName = SymbolName.Em4 }, "Quarter Em blank Character"},
                { new Character { SymbolName = SymbolName.En }, "En blank Character"},
                { new Character { SymbolName = SymbolName.NonBreakableBlank }, "No break space Character"},
                { new Character { SymbolName = SymbolName.Tab }, "Tab Character"},
                { new InfoField { Name = "Title" }, "Empty InfoField"},
                { new BookmarkField("bookmarkName") , "BookmarkField"},
                { new Text("\u00AD"), "Text with one soft hyphen only" },
                { new Character { SymbolName = SymbolName.LineBreak }, "Line break Character"}
            };

            // Create one document containing all results.
            var sumDoc = new PdfDocument();
            var exceptions = new List<Exception>();

            // Create documents for different trailing objects.
            foreach (var trailingObject in trailingObjects)
            {
                try
                {
                    var document = new Document();

                    var style = document.Styles[StyleNames.Heading1];
                    style!.Font.Size = Unit.FromPoint(14);
                    style.Font.Bold = true;

                    var section = document.AddSection();

                    // Leave 20 cm for content.
                    section.PageSetup.TopMargin = Unit.FromMillimeter(70);
                    section.PageSetup.BottomMargin = Unit.FromMillimeter(27);

                    // Add informational content.
                    var tf = section.AddTextFrame();
                    tf.RelativeHorizontal = RelativeHorizontal.Margin;
                    tf.RelativeVertical = RelativeVertical.Margin;
                    tf.Top = Unit.FromCentimeter(-1.5);
                    tf.Left = 0;
                    tf.Height = Unit.FromCentimeter(1);
                    tf.Width = Unit.FromCentimeter(16);
                    tf.FillFormat.Color = Colors.LightGreen;
                    var p = tf.AddParagraph($"Trailing object: {trailingObject.Value}");
                    p.Style = StyleNames.Heading1;

                    tf = section.AddTextFrame();
                    tf.RelativeHorizontal = RelativeHorizontal.Margin;
                    tf.RelativeVertical = RelativeVertical.Margin;
                    tf.Top = 0;
                    tf.Left = Unit.FromCentimeter(-1.5);
                    tf.Height = Unit.FromCentimeter(20);
                    tf.Width = Unit.FromCentimeter(0.5);
                    tf.FillFormat.Color = Colors.LightGreen;
                    tf.Orientation = TextOrientation.Upward;
                    p = tf.AddParagraph("20 cm space for content");
                    p.Format.Alignment = ParagraphAlignment.Center;

                    // Add offset inserting paragraph.
                    p = section.AddParagraph("Paragraph to achieve an offset of 175 mm.");
                    p.Format.LineSpacingRule = LineSpacingRule.Exactly;
                    p.Format.LineSpacing = Unit.FromMillimeter(175);
                    p.Format.Shading.Color = Colors.LightGray;

                    // Add test paragraph.
                    p = section.AddParagraph("Text");
                    p.Format.LineSpacingRule = LineSpacingRule.Exactly;
                    p.Format.LineSpacing = Unit.FromCentimeter(1);
                    p.Format.Shading.Color = Colors.LightBlue;

                    // Add trailing object that may have caused exceptions before.
                    p.Elements.Add(trailingObject.Key);

                    // Set test paragraph borders.
                    var borders = p.Format.Borders;
                    borders.Visible = true;
                    borders.DistanceFromTop = Unit.FromMillimeter(10);
                    borders.DistanceFromBottom = Unit.FromMillimeter(10);

                    // Render document and add it to sumDoc.
                    var pdfRenderer = new PdfDocumentRenderer { Document = document };
                    pdfRenderer.RenderDocument();

                    var stream = new MemoryStream();
                    pdfRenderer.PdfDocument.Save(stream);

                    var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                    foreach (var page in pdfDocument.Pages)
                        sumDoc.AddPage(page);

                    // Always add an even count of pages for better comparability in PDF reader.
                    if (pdfDocument.PageCount % 2 == 1)
                        sumDoc.AddPage();

                }
                catch (Exception ex)
                {
                    var message = $"Exception while generating test document with trailing object {trailingObject.Value}.";

                    // Add exception to list to continue tests and throw one AggregatedException at the end.
                    exceptions.Add(new Exception(message, ex));

                    // Create temporary document with the exception and stacktrace.
                    var document = new Document();

                    var style = document.Styles[StyleNames.Normal];
                    style!.Font.Color = Colors.Red;
                    style.ParagraphFormat.SpaceAfter = Unit.FromMillimeter(5);

                    style = document.Styles[StyleNames.Heading1];
                    style!.Font.Size = Unit.FromPoint(14);
                    style.Font.Bold = true;

                    var section = document.AddSection();
                    var p = section.AddParagraph(message);
                    p.Style = StyleNames.Heading1;
                    section.AddParagraph(ex.Message);
                    section.AddParagraph(ex.StackTrace ?? "Empty stacktrace");

                    // Render document and add it to sumDoc.
                    var pdfRenderer = new PdfDocumentRenderer { Document = document };
                    pdfRenderer.RenderDocument();

                    var stream = new MemoryStream();
                    pdfRenderer.PdfDocument.Save(stream);

                    var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                    foreach (var page in pdfDocument.Pages)
                        sumDoc.AddPage(page);

                    // Always add an even count of pages for better comparability in PDF reader.
                    if (pdfDocument.PageCount % 2 == 1)
                        sumDoc.AddPage();
                }
            }

            // Save sumDoc.
            var filename = PdfFileUtility.GetTempPdfFileName("Test_Trailing_Objects_Border_Paragraph_PageBreak");
            sumDoc.PageLayout = PdfPageLayout.TwoPageLeft;
            sumDoc.Save(filename);
            PdfFileUtility.ShowDocumentIfDebugging(filename);

            // Finally throw occurred exceptions.
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        [Fact]
        public void Test_LineSpacingRule()
        {
            var document = new Document();

            var style = document.Styles[StyleNames.Heading1];
            style!.ParagraphFormat.Font.Size = 16;
            style.ParagraphFormat.Font.Bold = true;
            style.ParagraphFormat.SpaceBefore = Unit.FromPoint(8);
            style.ParagraphFormat.SpaceAfter = Unit.FromPoint(8);

            var section = document.AddSection();

            const string newLineWord = "Loreetp";
            const string bigWord = "Loreetq";

            const string headingFontSizes = "FontSize10_14_10";
            var headingsLineSpacing = new Dictionary<LineSpacingRule, String>();


            var paragraph = section.AddParagraph(headingFontSizes);
            paragraph.Style = StyleNames.Heading1;
            {
                paragraph = section.AddParagraph($"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod. " +
                                                 $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod. " +
                                                 $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod.");
                paragraph.Format.Font.Size = 10;

                paragraph = section.AddParagraph($"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla. " +
                                                 $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla. " +
                                                 $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla.");
                paragraph.Format.Font.Size = 14;

                paragraph = section.AddParagraph($"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod. " +
                                                 $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod. " +
                                                 $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod.");
                paragraph.Format.Font.Size = 10;
            }


            var lineSpacingRules = new List<LineSpacingRule> { LineSpacingRule.Single, LineSpacingRule.Exactly, LineSpacingRule.AtLeast, LineSpacingRule.OnePtFive, LineSpacingRule.Double, LineSpacingRule.Multiple };

            foreach (var lineSpacingRule in lineSpacingRules)
            {
                if (lineSpacingRule == LineSpacingRule.OnePtFive)
                    section.AddPageBreak();

                var heading = $"LineSpacing{lineSpacingRule}";
                headingsLineSpacing.Add(lineSpacingRule, heading);

                paragraph = section.AddParagraph(heading);
                paragraph.Style = StyleNames.Heading1;
                {
                    paragraph = section.AddParagraph($"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod. " +
                                                     $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod. " +
                                                     $"{newLineWord} ad et luptat. ");
                    paragraph.Format.Font.Size = 10;
                    paragraph.Format.LineSpacingRule = lineSpacingRule;
                    if (lineSpacingRule == LineSpacingRule.Multiple)
                        paragraph.Format.LineSpacing = 3;
                    else if (lineSpacingRule == LineSpacingRule.Exactly)
                        paragraph.Format.LineSpacing = 10;
                    if (lineSpacingRule == LineSpacingRule.AtLeast)
                        paragraph.Format.LineSpacing = 14;
                    var ft = paragraph.AddFormattedText(bigWord);
                    ft.Font.Size = 14;
                    paragraph.AddText(" ad et. Duis niamconsecte digna facilla reros delit utat augait eratie. " +
                                      $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod. " +
                                      $"{newLineWord} ad et luptat. {bigWord} ad et. Duis niamconsecte digna facilla reros delit utat augait eratie mod.");
                }
            }


            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            var pdfDocument = pdfRenderer.PdfDocument;
            pdfDocument.Options.CompressContentStreams = false;

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Test_LineSpacingRule");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);


            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, 0);

            var offsetPrecision = 0.01;


            // Get default line spacings for 10 and 14 pt and calculate ascender and descender differences.
            streamEnumerator.Text.MoveAndGetNext(x => x.Text == headingFontSizes, true, out _).Should().BeTrue();

            // Lines with font size 10.
            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out var textInfo).Should().BeTrue();
            var lastY = textInfo!.Y;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            var offsetSize10 = lastY - textInfo!.Y;
            lastY = textInfo.Y;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            var offset = lastY - textInfo!.Y;
            offset.Should().BeApproximately(offsetSize10, offsetPrecision);
            lastY = textInfo.Y;

            // Lines with font size 14.
            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            var offsetSize14After10 = lastY - textInfo!.Y;
            offsetSize14After10.Should().BeGreaterThan(offsetSize10, "offset to 14 pt line after 10 pt line should be greater than offset between 10 pt lines.");
            lastY = textInfo.Y;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            var offsetSize14 = lastY - textInfo!.Y;
            offsetSize14.Should().BeGreaterThan(offsetSize10, "offset between 14 pt lines should be greater than offset between 10 pt lines.");
            offsetSize14.Should().BeGreaterThan(offsetSize14After10, "offset between 14 pt lines should be greater than offset to 14 pt line after 10 pt line.");
            lastY = textInfo.Y;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            offset = lastY - textInfo!.Y;
            offset.Should().BeApproximately(offsetSize14, offsetPrecision);
            lastY = textInfo.Y;

            // Lines with font size 10.
            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            var offsetSize10After14 = lastY - textInfo!.Y;
            offsetSize10After14.Should().BeGreaterThan(offsetSize10, "offset to 10 pt line after 14 pt line should be greater than offset between 10 pt lines.");
            offsetSize10After14.Should().BeLessThan(offsetSize14, "offset to 10 pt line after 14 pt line should be less than offset between 14 pt lines.");
            offsetSize10After14.Should().BeLessThan(offsetSize14After10, "offset to 10 pt line after 14 pt line (contains 10 pt ascender and 14 pt descender) should be less than " +
                                                                         "offset to 14 pt line after 10 pt line (contains 14 pt ascender and 10 pt descender).");
            lastY = textInfo.Y;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            offset = lastY - textInfo!.Y;
            offset.Should().BeApproximately(offsetSize10, offsetPrecision);
            lastY = textInfo.Y;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
            offset = lastY - textInfo!.Y;
            offset.Should().BeApproximately(offsetSize10, offsetPrecision);

            var ascenderDifference = offsetSize14After10 - offsetSize10; // The 14 pt ascender is bigger as the 10 pt ascender by this value.
            var descenderDifference = offsetSize10After14 - offsetSize10; // The 14 pt descender is bigger as the 10 pt descender by this value.


            // Inspect the line spacing examples.
            foreach (var lineSpacingRule in lineSpacingRules)
            {
                if (lineSpacingRule == LineSpacingRule.OnePtFive)
                    streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, 1);

                var heading = headingsLineSpacing[lineSpacingRule];
                streamEnumerator.Text.MoveAndGetNext(x => x.Text == heading, true, out _).Should().BeTrue();


                double usualOffset, before14Offset, after14Offset;

                if (lineSpacingRule == LineSpacingRule.Exactly)
                {
                    // Exact value is set.
                    usualOffset = 10;
                    before14Offset = 10;
                    after14Offset = 10;
                }
                else if (lineSpacingRule == LineSpacingRule.AtLeast)
                {
                    usualOffset = 14; // At least value 14 is greater than arial.GetHeight() for 10 pt (11.499023 pt).
                    before14Offset = offsetSize10 + ascenderDifference; // Default calculation (see below) is greater than 14.
                    after14Offset = offsetSize14 - ascenderDifference + (14 - offsetSize10); // Default calculation (see below) plus the difference the at least 14 pt line is greater than a 10 pt line.
                }
                else
                {
                    // Default calculation.
                    var factor = lineSpacingRule switch
                    {
                        LineSpacingRule.OnePtFive => 1.5,
                        LineSpacingRule.Double => 2,
                        LineSpacingRule.Multiple => 3,
                        _ => 1
                    };

                    usualOffset = offsetSize10 * factor; // Between 10 pt lines, multiply the 10 pt offset with the factor.
                    before14Offset = offsetSize10 * factor + ascenderDifference; // The offset to the big word line is the line spacing of the line before (the 10 pt offset * factor)
                                                                                 // plus the ascenderDifference, as the big word line’s first word starts lower by this.
                    after14Offset = offsetSize14 * factor - ascenderDifference; // The offset after the big word line is its line spacing (the 14 pt offset * factor)
                                                                                // minus the ascenderDifference, as the big word line’s first word started lower by this.
                }

                // Inspect the lines.
                streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
                lastY = textInfo!.Y;

                streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
                offset = lastY - textInfo!.Y;
                offset.Should().BeApproximately(usualOffset, offsetPrecision);
                lastY = textInfo.Y;

                streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
                offset = lastY - textInfo!.Y;
                offset.Should().BeApproximately(before14Offset, offsetPrecision);
                lastY = textInfo.Y;

                streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
                offset = lastY - textInfo!.Y;
                offset.Should().BeApproximately(after14Offset, offsetPrecision);
                lastY = textInfo.Y;

                streamEnumerator.Text.MoveAndGetNext(x => x.Text == newLineWord, true, out textInfo).Should().BeTrue();
                offset = lastY - textInfo!.Y;
                offset.Should().BeApproximately(usualOffset, offsetPrecision);
            }
        }

        [Fact]
        public void Test_PageBreak_And_Fitting_Line_Height()
        {
            // For 6.2.0-preview-1 this test caused an endless loop, because a new page was added due to ParagraphRenderer.StartNewLine
            // assuming the second line would have the same height as the first one and would therefore not fit on the first page.
            // However, TopDownFormatter.PreviousRendererNeedsRemoveEnding correctly calculated that both lines would fit on a next page.
            // So, the first line was removed from the page and the loop was restarted trying to place both lines on the next page.

            // Create a MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            Section section = document.AddSection();

            var paragraph = section.AddParagraph();

            // Paragraph containing two lines shall be placed on one page.
            paragraph.Format.WidowControl = true;

            // Fits on the first line and on the page.
            var ft = paragraph.AddFormattedText("1");
            ft.Font.Size = Unit.FromCentimeter(15);
            ft.Font.Italic = true;

            // Needs a break to the second line and fits on it.
            // Fits on the page, but would not, if it had the same height as the first line.
            ft = paragraph.AddFormattedText("2345");
            ft.Font.Size = Unit.FromCentimeter(5);
            ft.Font.Bold = true;

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document,
                // Let the PDF viewer show this PDF with full pages.
                PdfDocument =
                {
                    PageLayout = PdfPageLayout.TwoPageLeft,
                    ViewerPreferences =
                    {
                        FitWindow = true
                    }
                }
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Test_PageBreak_And_Fitting_Line_Height");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }

        [Fact]
        public void Test_Line_And_PageBreak_Big_Words()
        {
            // For 6.2.0-preview-1 this test showed empty lines that should not occur after a line break caused by one word longer than the line.
            // For WidowControl = true it showed also an empty line that should not occur after the last page break.

            // Create a MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            Section section = document.AddSection();

            var paragraph = section.AddParagraph("123 456 789 0AB");
            paragraph.Format.Font.Size = Unit.FromCentimeter(10);
            paragraph.Format.WidowControl = true;

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document,
                // Let the PDF viewer show this PDF with full pages.
                PdfDocument =
                {
                    PageLayout = PdfPageLayout.TwoPageLeft,
                    ViewerPreferences =
                    {
                        FitWindow = true
                    }
                }
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileUtility.GetTempPdfFileName("Test_Line_And_PageBreak_Big_Words");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
        }
    }
}