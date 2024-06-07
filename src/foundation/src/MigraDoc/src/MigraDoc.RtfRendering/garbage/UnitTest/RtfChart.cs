//// MigraDoc - Creating Documents on the Fly
//// See the LICENSE file in the solution root for more information.

//using MigraDoc.DocumentObjectModel;
//using MigraDoc.DocumentObjectModel.IO;

//namespace MigraDoc.RtfRendering.UnitTest
//{
//  /// <summary>
//  /// Summary description for FontTable.
//  /// </summary>
//  [UnitTestClass]
//  public class RtfChart
//  {
//    public RtfChart()
//    {
//    }

//    [UnitTestFunction]
//    public static void Test()
//    {
//      Document doc = new Document();
//      Section sec = doc.AddSection();

//      Chart chart = sec.AddChart(ChartType.Line);
//      chart.Width = "13cm";
//      chart.Height = "8cm";
//      chart.FillFormat.Color = Color.Linen;
//      chart.LineFormat.Width = 2;

//      chart.PlotArea.FillFormat.Color = Color.Blue;

////      chart.XAxis.Title.Caption = "X-Axis";
////      chart.YAxis.Title.Caption = "Y-Axis";

//      DocumentRenderer docRenderer = new DocumentRenderer();
//      docRenderer.Render(doc, "RtfChart.txt");
//      DdlWriter.SerializeToFile(doc, "RtfChart.mdddl");
//      System.IO.File.Copy("RtfChart.txt", "RtfChart.rtf", true);
//      System.Diagnostics.Process.St art("RtfChart.txt");
//    }
//  }
//}
