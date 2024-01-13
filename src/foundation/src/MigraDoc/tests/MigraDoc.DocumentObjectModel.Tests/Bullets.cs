using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    public class Bullets
    {
        [Fact]
        public void Create_Bullets()
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

#if DEBUG___
            var ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
            //GetType();
#endif

            // Create a renderer for the MigraDoc document.
            var pdfRenderer = new PdfDocumentRenderer()
            {
                // Associate the MigraDoc document with a renderer.
                Document = document
            };

            // Layout and render document to PDF.
            pdfRenderer.RenderDocument();

            // Save the document...
            var filename = PdfFileHelper.CreateTempFileName("HelloWorld");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileHelper.StartPdfViewerIfDebugging(filename);
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

            paragraph = section.AddParagraph("Lorem ipsum.");

            paragraph = section.AddParagraph("Premium");
            paragraph.Format.ListInfo.ListType = ListType.BulletList1;

            paragraph = section.AddParagraph("Secundum");
            paragraph.Format.ListInfo.ListType = ListType.BulletList1;

            paragraph = section.AddParagraph("Tertium");
            paragraph.Format.ListInfo.ListType = ListType.BulletList1;

            paragraph = section.AddParagraph("Lorem ipsum.");

            paragraph = section.AddParagraph("Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait eratie modions quisciduisi euissed magna feugiam quat alit amet alit, quamcon henibh eum veros nullan eugiat. Ut ipsummod esto exer in hent lortio odolore dolorer iustis am vullam, quating eugueraestie etumsan ute modions.");
            paragraph.Format.ListInfo.ListType = ListType.BulletList1;
            paragraph.Format.LeftIndent = "2cm";

            paragraph = section.AddParagraph("Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait eratie modions quisciduisi euissed magna feugiam quat alit amet alit, quamcon henibh eum veros nullan eugiat. Ut ipsummod esto exer in hent lortio odolore dolorer iustis am vullam, quating eugueraestie etumsan ute modions.");
            paragraph.Format.ListInfo.ListType = ListType.BulletList1;
            paragraph.Format.ListInfo.NumberPosition = "1cm";
            paragraph.Format.LeftIndent = "2cm";

            // Create the primary footer.
            var footer = section.Footers.Primary;

            // Add content to footer.
            paragraph = footer.AddParagraph();
            paragraph.Add(new DateField { Format = "yyyy/MM/dd HH:mm:ss" });
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            return document;
        }
    }
}
