// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Charting;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering.ChartMapper
{
    /// <summary>
    /// Maps charts from the MigraDoc.DocumentObjectModel to charts from Pdf.Charting.
    /// </summary>
    public class ChartMapper
    {
        ChartFrame MapObject(DocumentObjectModel.Shapes.Charts.Chart domChart)
        {
            var chartFrame = new ChartFrame
            {
                Size = new XSize(domChart.Width.Point, domChart.Height.Point),
                Location = new XPoint(domChart.Left.Position.Point, domChart.Top.Position.Point)
            };

            var chart = new Chart((ChartType)domChart.Type);

            if (!domChart.Values.XAxis.IsValueNullOrEmpty())
                AxisMapper.Map(chart.XAxis, domChart.XAxis);
            if (!domChart.Values.YAxis.IsValueNullOrEmpty())
                AxisMapper.Map(chart.YAxis, domChart.YAxis);

            PlotAreaMapper.Map(chart.PlotArea, domChart.PlotArea);

            SeriesCollectionMapper.Map(chart.SeriesCollection, domChart.SeriesCollection);

            LegendMapper.Map(chart, domChart);

            chart.DisplayBlanksAs = (BlankType)domChart.DisplayBlanksAs;
            chart.HasDataLabel = domChart.HasDataLabel;
            if (!domChart.Values.DataLabel.IsValueNullOrEmpty())
                DataLabelMapper.Map(chart.DataLabel, domChart.DataLabel);

            if (!domChart.Values.Style.IsValueNullOrEmpty())
            {
                Debug.Assert(domChart.Document != null, "domChart.Document != null");
                FontMapper.Map(chart.Font, domChart.Document, domChart.Style);
            }

            if (!domChart.Values.Format.IsValueNullOrEmpty() && !domChart.Format.Values.Font.IsValueNullOrEmpty())
                FontMapper.Map(chart.Font, domChart.Format.Font);
            if (!domChart.Values.XValues.IsValueNullOrEmpty())
                XValuesMapper.Map(chart.XValues, domChart.XValues);

            chartFrame.Add(chart);
            return chartFrame;
        }

        /// <summary>
        /// Maps the specified DOM chart.
        /// </summary>
        /// <param name="domChart">The DOM chart.</param>
        public static ChartFrame Map(DocumentObjectModel.Shapes.Charts.Chart domChart)
        {
            var mapper = new ChartMapper();
            return mapper.MapObject(domChart);
        }
    }
}
