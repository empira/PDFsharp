// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

#pragma warning disable 1591

namespace MigraDoc.Rendering.UnitTest
{
    /// <summary>
    /// Summary description for ParagraphRenderer.
    /// </summary>
    public class TestParagraphRenderer
    {
        /// <summary>
        /// Tests texts and blanks.
        /// </summary>
        public static void TextAndBlanks(string pdfOutputFile)
        {
            Document document = new Document();
            Section section = document.AddSection();
            Paragraph par = section.AddParagraph("Dies");
            for (int idx = 0; idx <= 40; ++idx)
            {
                par.AddCharacter(SymbolName.Blank);
                par.AddText(idx.ToString());
                par.AddCharacter(SymbolName.Blank);
                par.AddText((idx + 1).ToString());
                par.AddCharacter(SymbolName.Blank);
                par.AddText((idx + 2).ToString());
            }
            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(pdfOutputFile);
        }

        /// <summary>
        /// Tests AddFormattedText.
        /// </summary>
        public static void Formatted(string pdfOutputFile)
        {
            Document document = new Document();
            Section section = document.AddSection();
            Paragraph par = section.AddParagraph();
            FillFormattedParagraph(par);
            PdfDocumentRenderer printer = new PdfDocumentRenderer();
            printer.Document = document;
            printer.RenderDocument();
            printer.PdfDocument.Save(pdfOutputFile);
        }

        internal static void FillFormattedParagraph(Paragraph par)
        {
            for (int idx = 0; idx <= 140; ++idx)
            {
                if (idx < 60)
                {
                    FormattedText formText = par.AddFormattedText((idx).ToString(), TextFormat.Bold);
                    formText.Font.Size = 16;
                    formText.AddText(" ");
                }
                else if (idx < 100)
                {
                    par.AddText((idx).ToString());
                    par.AddText(" ");
                }
                else
                {
                    FormattedText formText = par.AddFormattedText((idx).ToString(), TextFormat.Italic);
                    formText.Font.Size = 6;
                    formText.AddText(" ");
                }
                if (idx % 50 == 0)
                    par.AddLineBreak();
            }
            par.AddText(" ...ready.");
        }

        /// <summary>
        /// Tests alignments.
        /// </summary>
        public static void Alignment(string pdfOutputFile)
        {
            Document document = new Document();
            Section section = document.AddSection();
            section.PageSetup.LeftMargin = 0;
            section.PageSetup.RightMargin = 0;
            Paragraph par = section.AddParagraph();
            //      FillFormattedParagraph(par);
            //      par.Format.Alignment = ParagraphAlignment.Left;

            //      par = section.AddParagraph();
            //      FillFormattedParagraph(par);
            //      par.Format.Alignment = ParagraphAlignment.Right;

            //      par = section.AddParagraph();
            FillFormattedParagraph(par);
            par.Format.Alignment = ParagraphAlignment.Center;
            //
            //      par = section.AddParagraph();
            //      FillFormattedParagraph(par);
            //      par.Format.Alignment = ParagraphAlignment.Justify;

            par.Format.FirstLineIndent = "-2cm";
            par.Format.LeftIndent = "2cm";
            par.Format.RightIndent = "3cm";
            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(pdfOutputFile);
        }

        /// <summary>
        /// Tests tab stops.
        /// </summary>
        public static void Tabs(string pdfOutputFile)
        {
            Document document = new Document();
            Section section = document.AddSection();
            section.PageSetup.LeftMargin = 0;
            section.PageSetup.RightMargin = 0;
            Paragraph par = section.AddParagraph();
            par.Format.TabStops.AddTabStop("20cm", TabAlignment.Right);
            par.AddText(" text before tab bla bla bla. text before tab bla bla bla. text before tab bla bla bla. text before tab bla bla bla.");
            //par.AddTab();
            par.AddText(" ............ after tab bla bla bla.");
            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(pdfOutputFile);
        }

        internal static void GiveBorders(Paragraph par)
        {
            Borders borders = par.Format.Borders;
            borders.Top.Color = Colors.Gray;
            borders.Top.Width = 4;
            borders.Top.Style = BorderStyle.DashDot;
            borders.Left.Color = Colors.Red;
            borders.Left.Style = BorderStyle.Dot;
            borders.Left.Width = 7;
            borders.Bottom.Color = Colors.Red;
            borders.Bottom.Width = 3;
            borders.Bottom.Style = BorderStyle.DashLargeGap;
            borders.Right.Style = BorderStyle.DashSmallGap;
            borders.Right.Width = 3;

            borders.DistanceFromBottom = "1cm";
            borders.DistanceFromTop = "1.5cm";

            borders.DistanceFromLeft = "0.5cm";
            borders.DistanceFromRight = "2cm";

            par.Format.Shading.Color = Colors.LightBlue;
        }

        /// <summary>
        /// Tests borders.
        /// </summary>
        public static void Borders(string outputFile)
        {
            Document document = new Document();
            Section section = document.AddSection();
            Paragraph par = section.AddParagraph();
            FillFormattedParagraph(par);
            GiveBorders(par);

            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputFile);
        }

        /// <summary>
        /// Tests document fields.
        /// </summary>
        public static void Fields(string outputFile)
        {
            Document document = new Document();
            Section section = document.AddSection();
            Paragraph par = section.AddParagraph();
            par.AddText("Section: ");
            par.AddSectionField().Format = "ALPHABETIC";
            par.AddLineBreak();

            par.AddText("SectionPages: ");
            par.AddSectionField().Format = "alphabetic";
            par.AddLineBreak();

            par.AddText("Page: ");
            par.AddPageField().Format = "ROMAN";
            par.AddLineBreak();

            par.AddText("NumPages: ");
            par.AddNumPagesField();
            par.AddLineBreak();

            par.AddText("Date: ");
            par.AddDateField();
            par.AddLineBreak();

            par.AddText("Bookmark: ");
            par.AddBookmark("Egal");
            par.AddLineBreak();

            par.AddText("PageRef: ");
            par.AddPageRefField("Egal");

            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputFile);
        }
    }
}