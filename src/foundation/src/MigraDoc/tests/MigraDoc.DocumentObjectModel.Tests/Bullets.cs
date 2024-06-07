// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Rendering;
using PdfSharp.Quality;
using Xunit;

namespace MigraDoc.DocumentObjectModel.Tests
{
    [Collection("PDFsharp")]
    public class Bullets
    {
        [Fact]
        public void Create_Bullets()
        {
            PdfSharpCore.ResetAll();

            // Create a MigraDoc document.
            var document = CreateDocument();

            // ----- Unicode encoding in MigraDoc is demonstrated here. -----

            //// A flag indicating whether to create a Unicode PDF or a WinAnsi PDF file.
            //// This setting applies to all fonts used in the PDF document.
            //// This setting has no effect on the RTF renderer.
            //const bool unicode = false;

#if DEBUG_
            var ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
            //_ = typeof(int);
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
            var filename = PdfFileUtility.GetTempPdfFileName("HelloWorld");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);
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

        [Fact]
        public static void BulletLineSpacingTest()
        {
            // Ensures that, without any spacial formatting, each bullet list line and each line caused by a line break in a list item has the same offset to the previous line.
            // The descent of the bullet itself shall be ignored in calculating the line spacing.

            var document = new Document();

            var bulletListStyle1 = "BulletList1";
            var bulletListStyle2 = "BulletList2";
            var bulletListStyle3 = "BulletList3";

            var style = document.Styles.AddStyle(bulletListStyle1, StyleNames.List);
            style.ParagraphFormat.ListInfo.ListType = ListType.BulletList1;
            style.ParagraphFormat.LeftIndent = "15mm";
            style.ParagraphFormat.FirstLineIndent = "-7.5mm";
            style.ParagraphFormat.TabStops.AddTabStop("15mm", TabAlignment.Left);
            style.ParagraphFormat.TabStops.AddTabStop("30mm", TabAlignment.Left);
            style.ParagraphFormat.TabStops.AddTabStop("45mm", TabAlignment.Left);

            style = document.Styles.AddStyle(bulletListStyle2, StyleNames.List);
            style.ParagraphFormat.ListInfo.ListType = ListType.BulletList2;
            style.ParagraphFormat.LeftIndent = "30mm";
            style.ParagraphFormat.FirstLineIndent = "-7.5mm";
            style.ParagraphFormat.TabStops.AddTabStop("30mm", TabAlignment.Left);
            style.ParagraphFormat.TabStops.AddTabStop("45mm", TabAlignment.Left);

            style = document.Styles.AddStyle(bulletListStyle3, StyleNames.List);
            style.ParagraphFormat.ListInfo.ListType = ListType.BulletList3;
            style.ParagraphFormat.LeftIndent = "45mm";
            style.ParagraphFormat.FirstLineIndent = "-7.5mm";
            style.ParagraphFormat.TabStops.AddTabStop("45mm", TabAlignment.Left);

            var section = document.AddSection();


            var paragraph = section.AddParagraph("Item 1");
            paragraph.Style = bulletListStyle1;

            paragraph = section.AddParagraph("Item 2");
            paragraph.Style = bulletListStyle1;

            paragraph = section.AddParagraph("Item 3 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait eratie modi Item quisciduisi euissed magna feugiam.");
            paragraph.Style = bulletListStyle1;

            paragraph = section.AddParagraph("Item 4");
            paragraph.Style = bulletListStyle1;

            paragraph = section.AddParagraph("Item 5 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait eratie modi Item quisciduisi euissed magna feugiam.");
            paragraph.Style = bulletListStyle1;

            paragraph = section.AddParagraph("Item 6 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait eratie modi Item quisciduisi euissed magna feugiam.");
            paragraph.Style = bulletListStyle1;

            paragraph = section.AddParagraph("Item 7");
            paragraph.AddTab();
            paragraph.AddText("ItemRef");
            paragraph.Style = bulletListStyle1;
            {
                paragraph = section.AddParagraph("Item 7.1");
                paragraph.Style = bulletListStyle2;

                paragraph = section.AddParagraph("Item 7.2");
                paragraph.Style = bulletListStyle2;

                paragraph = section.AddParagraph("Item 7.3 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait Item eratie modions quisciduisi euissed magna feugiam.");
                paragraph.Style = bulletListStyle2;

                paragraph = section.AddParagraph("Item 7.4");
                paragraph.Style = bulletListStyle2;

                paragraph = section.AddParagraph("Item 7.5 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait Item eratie modions quisciduisi euissed magna feugiam.");
                paragraph.Style = bulletListStyle2;

                paragraph = section.AddParagraph("Item 7.6 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit utat augait Item eratie modions quisciduisi euissed magna feugiam.");
                paragraph.Style = bulletListStyle2;

                paragraph = section.AddParagraph("Item 7.7");
                paragraph.AddTab();
                paragraph.AddText("ItemRef");
                paragraph.Style = bulletListStyle2;
                {
                    paragraph = section.AddParagraph("Item 7.7.1");
                    paragraph.Style = bulletListStyle3;

                    paragraph = section.AddParagraph("Item 7.7.2");
                    paragraph.Style = bulletListStyle3;

                    paragraph = section.AddParagraph("Item 7.7.3 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit Item utat augait eratie modions quisciduisi euissed magna feugiam.");
                    paragraph.Style = bulletListStyle3;

                    paragraph = section.AddParagraph("Item 7.7.4");
                    paragraph.Style = bulletListStyle3;

                    paragraph = section.AddParagraph("Item 7.7.5 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit Item utat augait eratie modions quisciduisi euissed magna feugiam.");
                    paragraph.Style = bulletListStyle3;

                    paragraph = section.AddParagraph("Item 7.7.6 Loreet ad et luptat. Duis niamconsecte digna facilla reros delit Item utat augait eratie modions quisciduisi euissed magna feugiam.");
                    paragraph.Style = bulletListStyle3;

                    paragraph = section.AddParagraph("Item 7.7.7");
                    paragraph.Style = bulletListStyle3;
                }

                paragraph = section.AddParagraph("Item 7.8");
                paragraph.AddTab();
                paragraph.AddText("ItemRef");
                paragraph.Style = bulletListStyle2;
            }

            paragraph = section.AddParagraph("Item 8");
            paragraph.AddTab();
            paragraph.AddText("ItemRef");
            paragraph.Style = bulletListStyle1;
            {
                paragraph = section.AddParagraph("Item 8.1");
                paragraph.AddTab();
                paragraph.AddText("ItemRef");
                paragraph.Style = bulletListStyle2;
                {
                    paragraph = section.AddParagraph("Item 8.1.1");
                    paragraph.Style = bulletListStyle3;
                }
            }

            paragraph = section.AddParagraph("Item 9");
            paragraph.AddTab();
            paragraph.AddTab();
            paragraph.AddText("ItemRef");
            paragraph.Style = bulletListStyle1;


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
            var filename = PdfFileUtility.GetTempPdfFileName("BulletLineSpacingTest");
            pdfRenderer.PdfDocument.Save(filename);
            // ...and start a viewer.
            PdfFileUtility.ShowDocumentIfDebugging(filename);


            // Analyze the drawn text in the PDF’s content stream.
            var streamEnumerator = PdfFileHelper.GetPageContentStreamEnumerator(pdfDocument, 0);

            // Ensure that all "Item" text objects have the same y offset.
            streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Item", true, out var textInfo).Should().BeTrue();
            var lastY = textInfo!.Y;

            streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Item", true, out textInfo).Should().BeTrue();
            var firstOffset = textInfo!.Y - lastY;
            lastY = textInfo.Y;

            while (streamEnumerator.Text.MoveAndGetNext(x => x.Text == "Item", true, out textInfo))
            {
                var offset = textInfo!.Y - lastY;
                offset.Should().BeApproximately(firstOffset, 0.01);
                lastY = textInfo.Y;
            }
        }
    }
}
