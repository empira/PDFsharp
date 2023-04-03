using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    public class ParagraphTests
    {
        [Fact]
        public void Test_Empty_Paragraph()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            var document = new Document();
            var section = document.AddSection();

            // Empty ParagraphElements in Paragraph may cause problems.
            section.AddParagraph();

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileHelper.CreateTempFileName("Test_Empty_Paragraph");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }

        [Fact]
        public void Test_Empty_FormattedText()
        {
#if CORE
            GlobalFontSettings.FontResolver = NewFontResolver.Get();
#endif

            var document = new Document();
            var section = document.AddSection();

            // Empty ParagraphElements in FormattedText may cause problems.
            var p = section.AddParagraph();
            p.AddFormattedText();

            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            var filename = PdfFileHelper.CreateTempFileName("Test_Empty_FormattedText");
            pdfRenderer.PdfDocument.Save(filename);
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
        }
    }
}
