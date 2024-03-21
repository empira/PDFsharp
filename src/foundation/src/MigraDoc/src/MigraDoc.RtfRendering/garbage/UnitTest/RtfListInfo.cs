using System;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;

#if UNITTEST
using Efw.UnitTest;

namespace MigraDoc.RtfRendering.UnitTest
{
  /// <summary>
  /// Summary description for FontTable.
  /// </summary>
  [UnitTestClass]
  public class RtfListInfo
  {
    public RtfListInfo()
    {
    }

   // [UnitTestFunction]
    public static void Test()
    {
      Document doc = new Document();
      Style styl = doc.AddStyle("TestStyle1", Style.DefaultParagraphFontName);
      styl.Font.Bold = true;
      Section sec = doc.AddSection();

      sec.PageSetup.PageHeight = "30cm";

      sec.Headers.FirstPage.Format.Font.Bold = true;
      sec.Headers.Primary.AddParagraph("This is the Primary Header.");
      sec.Headers.FirstPage.AddParagraph("This is the First Page Header.");
      sec.Headers.EvenPage.AddParagraph("This is the Even Page Header.");

      Paragraph par = sec.AddParagraph("Paragraph 1");
//      par.Style = "TestStyle1";
      par.Format.ListInfo.NumberPosition = 2;

      par = sec.AddParagraph("Paragraph 2");
      par.Format.ListInfo.ListType = ListType.BulletList3;
      Image img1 = par.AddImage("logo.gif");
//      Image img1 = par.AddImage("tick_green.png");
      img1.ScaleHeight = 5;
      img1.ScaleWidth = 2;
      img1.Height = "0.3cm";
      img1.Width = "5cm";
      img1.PictureFormat.CropLeft = "-2cm";
      img1.FillFormat.Color = Color.PowderBlue;
      img1.LineFormat.Width = 2;

      par = sec.AddParagraph("Paragraph 3");
      par.AddLineBreak();
      par.Format.ListInfo.NumberPosition = 2;

      TextFrame tf = sec.AddTextFrame();
      tf.WrapFormat.Style = WrapStyle.None;
      tf.RelativeHorizontal = RelativeHorizontal.Page;
      tf.RelativeVertical = RelativeVertical.Page;

      tf.Top = Unit.FromCm(2);
      tf.Left = ShapePosition.Center;
      tf.Height = "20cm";
      tf.Width = "10cm";
      tf.FillFormat.Color = Color.LemonChiffon;
      tf.LineFormat.Color = Color.BlueViolet;
      tf.LineFormat.DashStyle = DashStyle.DashDotDot;
      tf.LineFormat.Width = 2;
      tf.AddParagraph("in a text frame");
      tf.MarginTop = "3cm";
      tf.Orientation = TextOrientation.Downward;

      Image img = sec.AddImage("test1.jpg");
      img.ScaleHeight = 500;
      img.ScaleWidth = 200;
      img.Height = "10cm";
      img.Width = "10cm";
      img.PictureFormat.CropLeft = "-2cm";
      img.FillFormat.Color = Color.LawnGreen;
      img.LineFormat.Width = 3;
      img.WrapFormat.Style = WrapStyle.None;

      sec = doc.AddSection();//.AddParagraph("test");
      sec.PageSetup.PageWidth = "30cm";
      sec.AddParagraph("Section 2");

      DocumentRenderer docRenderer = new DocumentRenderer();
      docRenderer.Render(doc, "RtfListInfo.txt", null);
      DdlWriter.WriteToFile(doc, "RtfListInfo.mdddl");
      System.IO.File.Copy("RtfListInfo.txt", "RtfListInfo.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfListInfo.txt");
    }
  }
}
#endif