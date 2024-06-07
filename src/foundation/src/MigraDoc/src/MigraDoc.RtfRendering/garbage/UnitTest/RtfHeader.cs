// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using MigraDoc.DocumentObjectModel;

#if UNITTEST
using Efw.UnitTest;

namespace MigraDoc.RtfRendering.UnitTest
{
  /// <summary>
  /// Summary description for FontTable.
  /// </summary>
  [UnitTestClass]
  public class RtfHeader
  {
    public RtfHeader()
    {
    }

//    [UnitTestFunction]
    public static void Test()
    {
      Document doc = new Document();
      Styles styles = doc.Styles;
      Style style = styles.AddStyle("MyTestA", "Heading7"); 
      style.ParagraphFormat.Shading.Color = Color.Aqua;
      style.ParagraphFormat.SpaceAfter = 5;
      style.ParagraphFormat.SpaceBefore = 12;
      style.ParagraphFormat.WidowControl = false;
      style.ParagraphFormat.Borders.Width = 2;
      style.Font.Bold = true;
      style.Font.Italic = true;
      style.Font.Color = Color.DarkGray;
      style.Font.Name = "Courier New";
      Style styleB = styles.AddStyle("MyTestB", "MyTestA"); 
      styleB.ParagraphFormat.OutlineLevel = OutlineLevel.BodyText;
      styleB.ParagraphFormat.Borders.Left.Color = Color.Goldenrod;
      style.ParagraphFormat.TabStops.AddTabStop(100, TabAlignment.Right, TabLeader.MiddleDot);
      DocumentRenderer docRenderer = new DocumentRenderer();
      styleB.ParagraphFormat.TabStops.RemoveTabStop(100);
      docRenderer.Render(doc, "RtfHeader.txt", null);
      System.IO.File.Copy("RtfHeader.txt", "RtfHeader.rtf", true);
      System.Diagnostics.Process.Sta/rt("RtfHeader.txt");
    }
  }
}
#endif