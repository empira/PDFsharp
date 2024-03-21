using System;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;

#if UNITTEST
using Efw.UnitTest;
namespace MigraDoc.RtfRendering.UnitTest
{
  /// <summary>
  /// Summary description for RtfDocumentInfo.
  /// </summary>
  [UnitTestClass]
  public class RtfDocumentArea
  {
    public RtfDocumentArea()
    {
    }

//    [UnitTestFunction]
    public static void TestDocumentProps()
    {
      Document doc = new Document();
      doc.Info.Author = "ich";
      doc.Info.Keywords = "toller typ, kann alles";  // StL/23-03-14 WTF???
      doc.Info.Subject = "Womanizer";
      doc.Info.Title = "K. P.";

      doc.DefaultTabStop = "4cm";
      doc.FootnoteLocation = FootnoteLocation.BeneathText;
      doc.FootnoteNumberingRule = FootnoteNumberingRule.RestartSection;
      doc.FootnoteNumberStyle = FootnoteNumberStyle.UppercaseRoman;
      doc.FootnoteStartingNumber = 12;

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfDocumentArea.txt", null);

      File.Copy("RtfDocumentArea.txt", "RtfDocumentArea.rtf", true);
      System.Diagnostics.Process.St/art("RtfDocumentArea.txt");
    }

//   [UnitTestFunction]
    public static void TestParagraph()
    {
      Document doc = new Document();
      doc.Styles["Heading3"].Font.Color = Color.CadetBlue;
      Section sec = doc.Sections.AddSection();
      Paragraph par = sec.AddParagraph();
      par.Style = "Heading4";
      par.AddText("A bit ");
      FormattedText ft = par.AddFormattedText("differently formatted");
      ft.Style = "InvalidStyleName";
      ft.Font.Size = 40;
      ft.Font.Name = "Courier New";
      //ft.Font.Color = Color.Azure;
      ft.Font.Bold = true;
      ft.Font.Italic = true;
      //ft.Font.Underline = Underline.Dotted;

      par.AddText(" text.");

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfParagraph.txt", null);

      File.Copy("RtfParagraph.txt", "RtfParagraph.rtf", true);
      System.Diagnostics.Process.St/art("RtfParagraph.rtf");
    }

//    [UnitTestFunction]
    public static void TestSpecialCharacters()
    {
      Document doc = new Document();
      Section sec = doc.Sections.AddSection();
      Paragraph par = sec.AddParagraph();
      par.AddCharacter('\x93');
      par.AddCharacter(SymbolName.Blank);
      par.AddCharacter(SymbolName.Bullet);
      par.AddCharacter(SymbolName.Copyright);
      par.AddCharacter(SymbolName.Em);
      par.AddCharacter(SymbolName.Em4);
      par.AddCharacter(SymbolName.EmDash);
      par.AddCharacter(SymbolName.En);
      par.AddCharacter(SymbolName.EnDash);
      par.AddCharacter(SymbolName.Euro);
      par.AddCharacter(SymbolName.HardBlank);
      par.AddCharacter(SymbolName.LineBreak);
      par.AddCharacter(SymbolName.Not);
      par.AddCharacter(SymbolName.ParaBreak);
      par.AddCharacter(SymbolName.RegisteredTrademark);
      par.AddCharacter(SymbolName.Tab);
      par.AddCharacter(SymbolName.Trademark);

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfSpecialChars.txt", null);

      File.Copy("RtfSpecialChars.txt", "RtfSpecialChars.rtf", true);
      System.Diagnostics.Process.S/tart("RtfSpecialChars.txt");
    }

  //  [UnitTestFunction]
    public static void TestFields()
    {
      Document doc = new Document();
      doc.Info.Author = "K.P.";
      Section sec = doc.Sections.AddSection();
      Paragraph par = sec.AddParagraph();

      par.AddBookmark("myBookmark1");

      PageRefField prf = par.AddPageRefField("myBookmark1");
      prf.Format = "ALPHABETIC";

      PageField pf = par.AddPageField();
      pf.Format = "ROMAN";

      SectionField sf = par.AddSectionField();
      sf.Format = "roman";
      
      SectionPagesField spf = par.AddSectionPagesField();
      spf.Format = "roman";

      NumPagesField npf = par.AddNumPagesField();
      npf.Format = "alphabetic";

      InfoField inf = par.AddInfoField(InfoFieldType.Author);
      DateField df = par.AddDateField("D");

      df = par.AddDateField("d");
      df = par.AddDateField("s");
      df = par.AddDateField("r");
      df = par.AddDateField("G");
      df = par.AddDateField("dddd dd.MM.yyyy");

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfFields.txt", null);

      File.Copy("RtfFields.txt", "RtfFields.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfFields.txt");
    }

//    [UnitTestFunction]
    public static void TestHyperlink()
    {
      Document doc = new Document();
      doc.Info.Author = "K.P.";
      Section sec = doc.Sections.AddSection();
      Paragraph par = sec.AddParagraph();

      par.AddBookmark("myBookmark1");
      Hyperlink hp = par.AddHyperlink("myBookmark1");
      hp.AddText ("Hyperlink lokal");

      hp = par.AddHyperlink("http://www.empira.de", HyperlinkType.Web);
      hp.AddText("Hyperlink Web");

      hp = par.AddHyperlink("RtfHyperlinks.txt", HyperlinkType.File);
      hp.AddText("Hyperlink Datei Relativ");
      
      hp = par.AddHyperlink(@"\\Klpo01\D$\Kram\Tabs.pdf", HyperlinkType.File);
      hp.AddText("Hyperlink Datei Netzwerk");

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfHyperlinks.txt", null);

      File.Copy("RtfHyperlinks.txt", "RtfHyperlinks.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfHyperlinks.txt");
    }

 //   [UnitTestFunction]
    public static void TestTable()
    {
      Document doc = new Document();
      Section sec = doc.Sections.AddSection();
      Table table = sec.AddTable();

      table.Borders.Visible = true;
      for(int clmIdx = 0; clmIdx <= 8; ++clmIdx)
      {
        Column clm = table.Columns.AddColumn();
        clm.Format.Font.Color = clmIdx >2 ? Color.Red : Color.Green;
      }
      for(int rowIdx = 0; rowIdx <= 1000; ++rowIdx)
      {
        Row row = table.AddRow();
        row.Format.Font.Size = (float)(rowIdx / 10 + 10);
      }
      for(int clmIdx = 0; clmIdx <= 8; ++clmIdx)
      {
        for(int rowIdx = 0; rowIdx <= 1000; ++rowIdx)
        {
          table[rowIdx, clmIdx].AddParagraph(rowIdx + "," + clmIdx);
        }
      }

//      cell.AddParagraph("Tabelle");
//      cell.Shading.Color = Color.Blue;
//      cell.Borders.Right.Visible = false;
//      sec.AddParagraph("Paragraph After.");
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfTable.txt", null);

      File.Copy("RtfTable.txt", "RtfTable.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfTable.rtf");
    }

//    [UnitTestFunction]
    public static void TestParagraphLayout()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("paragraph-layout.mdddl", null);

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfParagraphLayout.txt", null);
      File.Copy("RtfParagraphLayout.txt", "RtfParagraphLayout.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfParagraphLayout.txt", null);
      DdlWriter.WriteToFile(doc, "RtfParagraphLayout.mdddl");
      */
    }

 //   [UnitTestFunction]
    public static void TestRechnung()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("Rechnung.mdddl", null);

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfRechnung.txt", null);
      File.Copy("RtfRechnung.txt", "RtfRechnung.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfRechnung.txt");
      DdlWriter.WriteToFile(doc, "RtfRechnung.mdddl");
      */
    }

//    [UnitTestFunction]
    public static void TestHeaderFooter()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("HeaderFooterTest.mdddl", null);

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfHeaderFooterTest.txt", null);
      File.Copy("RtfHeaderFooterTest.txt", "RtfHeaderFooterTest.rtf", true);
      System.Diagnostics.Process.Star/t("RtfHeaderFooterTest.txt");
      DdlWriter.WriteToFile(doc, "RtfHeaderFooterTest.mdddl");
      */
    }

//    [UnitTestFunction]
    public static void TestSection()
    {
      Document doc = new Document();
      Section sec = doc.AddSection();
      Table tbl = sec.AddTable();
      tbl.AddColumn();
      Row rw = tbl.AddRow();
      rw.Cells[0].AddParagraph("Table 1");

      sec = doc.AddSection();
      tbl = sec.AddTable();
      tbl.AddColumn();
      rw = tbl.AddRow();
      rw.Cells[0].AddParagraph("Table 2");

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfSection.txt", null);
      File.Copy("RtfSection.txt", "RtfSection.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfSection.txt");
      DdlWriter.WriteToFile(doc, "RtfSection.mdddl");
    }

 //   [UnitTestFunction]
    public static void TestTableLayout()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("Table-Layout.mdddl", null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfTableLayout.txt", null);
      File.Copy("RtfTableLayout.txt", "RtfTableLayout.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfTableLayout.txt");
      DdlWriter.WriteToFile(doc, "RtfTableLayout.mdddl");
      */
    }

//    [UnitTestFunction]
    public static void TestPageFormat()
    {
      Document doc = new Document();
      Section sec1 = doc.Sections.AddSection();
      sec1.PageSetup.PageFormat = PageFormat.A4;
      sec1.AddParagraph("PageFormat a4");
      Section sec2 = doc.Sections.AddSection();
      sec2.PageSetup.PageFormat = PageFormat.A6;
      sec2.PageSetup.Orientation = Orientation.Landscape;

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfPageFormat.txt", null);
      File.Copy("RtfPageFormat.txt", "RtfPageFormat.rtf", true);
      System.Diagnostics.Process.St/art("RtfPageFormat.txt");
      DdlWriter.WriteToFile(doc, "RtfPageFormat.mdddl");
    }

//    [UnitTestFunction]
    public static void TestRowHeight()
    {
      Document doc = new Document();
      Section sec = doc.Sections.AddSection();
      Table tbl = sec.AddTable();
      tbl.AddColumn();
      tbl.Rows.Height = 40;
      tbl.Rows.AddRow();
      tbl.Borders.Visible = true;

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfRowHeight.txt", null);
      File.Copy("RtfRowHeight.txt", "RtfRowHeight.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfRowHeight.txt");
      DdlWriter.WriteToFile(doc, "RtfRowHeight.mdddl");
    }
//    [UnitTestFunction]
    public static void TestVerticalAlign()
    {
      Document doc = new Document();
      Section sec = doc.Sections.AddSection();
      Table tbl = sec.AddTable();
      tbl.AddColumn();
      tbl.Rows.Height = 40;
      tbl.Rows.AddRow().VerticalAlignment = VerticalAlignment.Bottom;
      tbl[0, 0].AddParagraph("Text");
      tbl.Borders.Visible = true;

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfVerticalAlign.txt", null);
      File.Copy("RtfVerticalAlign.txt", "RtfVerticalAlign.rtf", true);
      System.Diagnostics.Process.St/art("RtfVerticalAlign.txt");
      DdlWriter.WriteToFile(doc, "RtfVerticalAlign.mdddl");
    }
    //[UnitTestFunction]
    public static void TestBorderDistances()
    {
      Document doc = new Document();
      Section sec = doc.Sections.AddSection();
      Paragraph par = sec.AddParagraph("left dist = 10");
      par.Format.Borders.DistanceFromLeft = 10;

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfBorderDistances.txt", null);
      File.Copy("RtfBorderDistances.txt", "RtfBorderDistances.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfBorderDistances.txt");
      DdlWriter.WriteToFile(doc, "RtfBorderDistances.mdddl");
    }

//    [UnitTestFunction]
    public static void TestInvalidStyle()
    {
      Document doc = new Document();
      Section sec = doc.Sections.AddSection();
      Paragraph par = sec.AddParagraph();
      par.AddFormattedText("text", "_invalid_");

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfInvalidStyle.txt", null);
      File.Copy("RtfInvalidStyle.txt", "RtfInvalidStyle.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfInvalidStyle.txt", null);
      DdlWriter.WriteToFile(doc, "RtfInvalidStyle.mdddl");
    }

//    [UnitTestFunction]
    public static void TestShapeLayout()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("Shape-Layout.mdddl", null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfShapeLayout.txt", null);
      File.Copy("RtfShapeLayout.txt", "RtfShapeLayout.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfShapeLayout.txt");
      DdlWriter.WriteToFile(doc, "RtfShapeLayout.mdddl");
      */
    }

//    [UnitTestFunction]
    public static void TestTextFramePos()
    {
      Document doc = new Document();
      Section sec = doc.Sections.AddSection();
      sec.AddParagraph("paragraphBefore");
      TextFrame txtFrm = sec.AddTextFrame();
      txtFrm.RelativeHorizontal = RelativeHorizontal.Page;
      txtFrm.WrapFormat.Style = WrapStyle.Through;
      txtFrm.WrapFormat.DistanceRight = "5cm";
      txtFrm.Left = ShapePosition.Right;
      sec.AddParagraph("paragraphAfter");

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfTextFramePos.txt", null);
      File.Copy("RtfTextFramePos.txt", "RtfTextFramePos.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfTextFramePos.txt");
      DdlWriter.WriteToFile(doc, "RtfTextFramePos.mdddl");
    }

 //   [UnitTestFunction]
    public static void TestCharacterStyleAsParagraphStyle()
    {
      Document doc = new Document();
      doc.Styles.AddStyle("charstyle", Style.DefaultParagraphFontName).Font.Italic = true;
      Section sec = doc.Sections.AddSection();

      sec.AddParagraph("CharStyle").Style = "charstyle";

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfCharacterStyleAsParagraphStyle.txt", null);
      File.Copy("RtfCharacterStyleAsParagraphStyle.txt", "RtfCharacterStyleAsParagraphStyle.rtf", true);
      System.Diagnostics.Process.S/tart("RtfCharacterStyleAsParagraphStyle.txt");
      DdlWriter.WriteToFile(doc, "RtfCharacterStyleAsParagraphStyle.mdddl");
    }

//    [UnitTestFunction]
    public static void TestDummyChart()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("DummyChart.mdddl", null);

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfDummyChart.txt", null);
      File.Copy("RtfDummyChart.txt", "RtfDummyChart.rtf", true);
      System.Diagnostics.Process.St/art("RtfDummyChart.txt");
      DdlWriter.WriteToFile(doc, "RtfDummyChart.mdddl");
      */
    }

    [UnitTestFunction]
    public static void TestVermögensverwaltung()
    {
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("Vermögensverwaltung.mdddl"); //, null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfVermögensverwaltung.txt", null);
      File.Copy("RtfVermögensverwaltung.txt", "RtfVermögensverwaltung.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfVermögensverwaltung.txt");
      DdlWriter.WriteToFile(doc, "RtfVermögensverwaltung.mdddl");
    }

 //[UnitTestFunction]
    public static void TestKnowledgeBase()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("KnowledgeBase.mdddl", null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfKnowledgeBase.txt", null);
      File.Copy("RtfKnowledgeBase.txt", "RtfKnowledgeBase.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfKnowledgeBase.txt");
      DdlWriter.WriteToFile(doc, "RtfKnowledgeBase.mdddl");
      */
    }

//    [UnitTestFunction]
    public static void TestFootnote()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("Paragraph-Footnotes.mdddl", null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfFootnote.txt", null);
      File.Copy("RtfFootnote.txt", "RtfFootnote.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfFootnote.txt");
      DdlWriter.WriteToFile(doc, "RtfFootnote.mdddl");
      */
    }

//    [UnitTestFunction]
    public static void TestImage()
    {
      Document doc = new Document();
      doc.AddSection().AddImage("logo.jpg").ScaleHeight = 0.5f;
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfImage.txt", null);
      File.Copy("RtfImage.txt", "RtfImage.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfImage.txt");
      DdlWriter.WriteToFile(doc, "RtfImage.mdddl");
    }

//    [UnitTestFunction]
    public static void TestHeaderParagraphStyle()
    {
      Document doc = new Document();
      Style myHdrStl = doc.Styles.AddStyle("MyHeaderStyle","Normal");
      myHdrStl.ParagraphFormat.TabStops.AddTabStop("10cm");
      Paragraph par = doc.AddSection().Headers.Primary.AddParagraph();
      par.Style = "MyHeaderStyle";
      par.Elements.AddTab();
      par.Elements.AddText("Hallo");

      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfHeaderParagraphStyle.txt", null);
      File.Copy("RtfHeaderParagraphStyle.txt", "RtfHeaderParagraphStyle.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfHeaderParagraphStyle.txt");
      DdlWriter.WriteToFile(doc, "RtfHeaderParagraphStyle.mdddl");
    }

    //[UnitTestFunction]
    public static void TestDiagrammBroschüre()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("DiagrammBroschüre.mdddl", null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfDiagrammBroschüre.txt", null);
      File.Copy("RtfDiagrammBroschüre.txt", "RtfDiagrammBroschüre.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfDiagrammBroschüre.txt");
      DdlWriter.WriteToFile(doc, "RtfDiagrammBroschüre.mdddl");
      */
    }

    //[UnitTestFunction]
    public static void TestImagePath()
    {
      Document doc = new Document();
      doc.ImagePath = "Images";
      doc.Sections.AddSection().AddImage("logo.gif");
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfImagePath.txt", null);
      File.Copy("RtfImagePath.txt", "RtfImagePath.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfImagePath.txt");
      DdlWriter.WriteToFile(doc, "RtfImagePath.mdddl");
    }

//    [UnitTestFunction]
    public static void TestIndexPage()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("!Index.mdddl", null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfIndex.txt", null);
      File.Copy("RtfIndex.txt", "RtfIndex.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfIndex.txt");
      DdlWriter.WriteToFile(doc, "RtfIndex.mdddl");
      */
    }

    [UnitTestFunction]
    public static void TestKlPo()
    {
      /*
      Document doc = DocumentObjectModel.IO.DdlReader.DocumentFromFile("KlPoTest.mdddl", null);
      DocumentRenderer docRndrr = new DocumentRenderer();
      docRndrr.Render(doc, "RtfKlPo.txt", null);
      File.Copy("RtfKlPo.txt", "RtfKlPo.rtf", true);
      System.Diagnostics.http://localhost:8093/link/download-assets.html("RtfKlPo.txt");
      DdlWriter.WriteToFile(doc, "RtfKlPo.mdddl");
      */
    }
  }
}
#endif