// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering.Extensions;
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
            plotArea.BottomPadding = domPlotArea.BottomPadding.ToXUnit();
            plotArea.RightPadding = domPlotArea.RightPadding.ToXUnit();
            plotArea.LeftPadding = domPlotArea.LeftPadding.ToXUnit();
            plotArea.TopPadding = domPlotArea.TopPadding.ToXUnit();

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
