// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using PdfSharp.Charting;

namespace MigraDoc.Rendering.ChartMapper
{
    /// <summary>
    /// The PlotAreaMapper class.
    /// </summary>
    public class PlotAreaMapper
    {
        void MapObject(PlotArea plotArea, DocumentObjectModel.Shapes.Charts.PlotArea domPlotArea)
        {
            plotArea.BottomPadding = domPlotArea.BottomPadding.Point;
            plotArea.RightPadding = domPlotArea.RightPadding.Point;
            plotArea.LeftPadding = domPlotArea.LeftPadding.Point;
            plotArea.TopPadding = domPlotArea.TopPadding.Point;

            if (!domPlotArea.Values.LineFormat.IsValueNullOrEmpty())
                LineFormatMapper.Map(plotArea.LineFormat, domPlotArea.LineFormat);
            if (!domPlotArea.Values.FillFormat.IsValueNullOrEmpty())
                FillFormatMapper.Map(plotArea.FillFormat, domPlotArea.FillFormat);
        }

        internal static void Map(PlotArea plotArea, DocumentObjectModel.Shapes.Charts.PlotArea domPlotArea)
        {
            var mapper = new PlotAreaMapper();
            mapper.MapObject(plotArea, domPlotArea);
        }
    }
}
