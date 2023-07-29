// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering.UnitTest
{
    /// <summary>
    /// Summary description for TestLayout.
    /// </summary>
    public class TestLayout
    {
        /// <summary>
        /// Creates two paragraphs.
        /// </summary>
        public static void TwoParagraphs(string outputFile)
        {
            var doc = new Document();
            var sec = doc.Sections.AddSection();

            sec.PageSetup.TopMargin = 0;
            sec.PageSetup.BottomMargin = 0;

            var par1 = sec.AddParagraph();
            TestParagraphRenderer.FillFormattedParagraph(par1);
            TestParagraphRenderer.GiveBorders(par1);
            par1.Format.SpaceAfter = "2cm";
            par1.Format.SpaceBefore = "3cm";
            var par2 = sec.AddParagraph();
            TestParagraphRenderer.FillFormattedParagraph(par2);
            TestParagraphRenderer.GiveBorders(par2);
            par2.Format.SpaceBefore = "3cm";

            var renderer = new PdfDocumentRenderer
            {
                Document = doc
            };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputFile);
        }

        /// <summary>
        /// Creates thousand paragraphs.
        /// </summary>
        public static void A1000Paragraphs(string outputFile)
        {
            var doc = new Document();
            var sec = doc.Sections.AddSection();

            sec.PageSetup.TopMargin = 0;
            sec.PageSetup.BottomMargin = 0;

            for (int idx = 1; idx <= 1000; ++idx)
            {
                var par = sec.AddParagraph();
                par.AddText("Paragraph " + idx + ": ");
                TestParagraphRenderer.FillFormattedParagraph(par);
                TestParagraphRenderer.GiveBorders(par);
            }
            var renderer = new PdfDocumentRenderer
            {
                Document = doc
            };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputFile);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public static string DumpParagraph()
        {
            return "";
            //      Document doc = new Document();
            //      Paragraph par = doc.Sections.AddSection().AddParagraph();
            //      par.Format.SpaceAfter = "3cm";
            //      par.Format.SpaceBefore = "2cm";
            //      TestParagraphRenderer.FillFormattedParagraph(par);
            //      PdfFlattenVisitor visitor = new PdfFlattenVisitor(doc);
            //      visitor.Visit();
            //
            //      XGraphics gfx = XGraphics.FromGraphics(Graphics.FromHwnd(IntPtr.Zero), new XSize(2000, 2000));
            //      //Renderer rndrr = Renderer.Create(gfx, par, new FieldInfos(new Hashtable()));
            //      rndrr.Format(new Rectangle(0, 0, XUnit.FromCentimeter(21), XUnit.FromCentimeter(29)), null);
            //      string retVal = ValueDumper.DumpValues(rndrr.RenderInfo.LayoutInfo);
            //      retVal += "\r\n";
            //
            //      retVal += ValueDumper.DumpValues(rndrr.RenderInfo.LayoutInfo.ContentArea);
            //      return retVal;
        }
    }
}
