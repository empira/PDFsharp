// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using PdfSharp.Quality;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class ParagraphTests
    {
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
                        section.PageSetup.BottomMargin= Unit.FromCentimeter(14.7);

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
                    catch (Exception e)
                    {
                        var message = $"Exception while generating test document with {offset} mm offset and {oneOrMultipleWordsStr}.";

                        // Add exception to list to continue tests and throw one AggregatedException at the end.
                        exceptions.Add(new Exception(message, e));

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
                        section.AddParagraph(e.Message);
                        section.AddParagraph(e.StackTrace ?? "Empty stacktrace");

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
            PdfSharpCore.ResetAll();
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
                catch (Exception e)
                {
                    var message = $"Exception while generating test document with trailing object {trailingObject.Value}.";

                    // Add exception to list to continue tests and throw one AggregatedException at the end.
                    exceptions.Add(new Exception(message, e));

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
                    section.AddParagraph(e.Message);
                    section.AddParagraph(e.StackTrace ?? "Empty stacktrace");

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
    }
}
