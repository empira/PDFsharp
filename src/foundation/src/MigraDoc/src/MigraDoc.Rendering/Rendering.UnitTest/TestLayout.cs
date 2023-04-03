// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

#pragma warning disable 1591

namespace MigraDoc.Rendering.UnitTest
{
    /// <summary>
    /// Summary description for TestLayout.
    /// </summary>
    public class TestLayout
    {
        public static void TwoParagraphs(string outputFile)
        {
            var doc = new Document();
            Section sec = doc.Sections.AddSection();

            sec.PageSetup.TopMargin = 0;
            sec.PageSetup.BottomMargin = 0;

            Paragraph par1 = sec.AddParagraph();
            TestParagraphRenderer.FillFormattedParagraph(par1);
            TestParagraphRenderer.GiveBorders(par1);
            par1.Format.SpaceAfter = "2cm";
            par1.Format.SpaceBefore = "3cm";
            Paragraph par2 = sec.AddParagraph();
            TestParagraphRenderer.FillFormattedParagraph(par2);
            TestParagraphRenderer.GiveBorders(par2);
            par2.Format.SpaceBefore = "3cm";

            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = doc;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputFile);
        }

        public static void A1000Paragraphs(string outputFile)
        {
            Document doc = new Document();
            Section sec = doc.Sections.AddSection();

            sec.PageSetup.TopMargin = 0;
            sec.PageSetup.BottomMargin = 0;

            for (int idx = 1; idx <= 1000; ++idx)
            {
                Paragraph par = sec.AddParagraph();
                par.AddText("Paragraph " + idx + ": ");
                TestParagraphRenderer.FillFormattedParagraph(par);
                TestParagraphRenderer.GiveBorders(par);
            }
            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = doc;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputFile);
        }

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
