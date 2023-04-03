// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Charting;

namespace MigraDoc.Rendering.ChartMapper
{
    class LegendMapper
    {
        void MapObject(Chart chart, DocumentObjectModel.Shapes.Charts.Chart domChart)
        {
            DocumentObjectModel.Shapes.Charts.Legend? domLegend = null;
            DocumentObjectModel.Shapes.Charts.TextArea? textArea = null;

            foreach (var domObj in domChart.BottomArea.Elements.Cast<DocumentObjectModel.DocumentObject>())
            {
                if (domObj is DocumentObjectModel.Shapes.Charts.Legend legend)
                {
                    chart.Legend.Docking = DockingType.Bottom;
                    domLegend = legend;
                    textArea = domChart.BottomArea;
                }
            }

            foreach (var domObj in domChart.RightArea.Elements.Cast<DocumentObjectModel.DocumentObject>())
            {
                if (domObj is DocumentObjectModel.Shapes.Charts.Legend legend)
                {
                    chart.Legend.Docking = DockingType.Right;
                    domLegend = legend;
                    textArea = domChart.RightArea;
                }
            }

            foreach (var domObj in domChart.LeftArea.Elements.Cast<DocumentObjectModel.DocumentObject>())
            {
                if (domObj is DocumentObjectModel.Shapes.Charts.Legend legend)
                {
                    chart.Legend.Docking = DockingType.Left;
                    domLegend = legend;
                    textArea = domChart.LeftArea;
                }
            }

            foreach (var domObj in domChart.TopArea.Elements.Cast<DocumentObjectModel.DocumentObject>())
            {
                if (domObj is DocumentObjectModel.Shapes.Charts.Legend legend)
                {
                    chart.Legend.Docking = DockingType.Top;
                    domLegend = legend;
                    textArea = domChart.TopArea;
                }
            }

            foreach (var domObj in domChart.HeaderArea.Elements.Cast<DocumentObjectModel.DocumentObject>())
            {
                if (domObj is DocumentObjectModel.Shapes.Charts.Legend legend)
                {
                    chart.Legend.Docking = DockingType.Top;
                    domLegend = legend;
                    textArea = domChart.HeaderArea;
                }
            }

            foreach (var domObj in domChart.FooterArea.Elements.Cast<DocumentObjectModel.DocumentObject>())
            {
                if (domObj is DocumentObjectModel.Shapes.Charts.Legend legend)
                {
                    chart.Legend.Docking = DockingType.Bottom;
                    domLegend = legend;
                    textArea = domChart.FooterArea;
                }
            }

            if (domLegend != null && textArea != null)
            {
                if (!domLegend.Values.LineFormat.IsValueNullOrEmpty())
                    LineFormatMapper.Map(chart.Legend.LineFormat, domLegend.LineFormat);
                if (!textArea.Values.Style.IsValueNullOrEmpty())
                {
                    Debug.Assert(textArea.Document != null, "textArea.Document != null");
                    FontMapper.Map(chart.Legend.Font, textArea.Document, textArea.Style);
                }

                if (!(domLegend.Values.Format?.Values.Font).IsValueNullOrEmpty())
                    FontMapper.Map(chart.Legend.Font, domLegend.Format.Font);
            }
        }

        internal static void Map(Chart chart, DocumentObjectModel.Shapes.Charts.Chart domChart)
        {
            var mapper = new LegendMapper();
            mapper.MapObject(chart, domChart);
        }
    }
}
