// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Charting;
using XSeriesElements = MigraDoc.DocumentObjectModel.Shapes.Charts.XSeriesElements;

namespace MigraDoc.Rendering.ChartMapper
{
    /// <summary>
    /// The XValuesMapper class.
    /// </summary>
    public class XValuesMapper
    {
        void MapObject(XValues xValues, DocumentObjectModel.Shapes.Charts.XValues domXValues)
        {
            foreach (DocumentObjectModel.Shapes.Charts.XSeries domXSeries in domXValues.Cast<DocumentObjectModel.Shapes.Charts.XSeries>())
            {
                var xSeries = xValues.AddXSeries();
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                if (domXSeries.Values.XSeriesElements is XSeriesElements domXSeriesElements)
                {
                    foreach (var domXValue in domXSeriesElements.Cast<DocumentObjectModel.Shapes.Charts.XValue>())
                    {
                        if (domXValue == null)
                            xSeries.AddBlank();
                        else
                            xSeries.Add(domXValue.Values.Value!);
                    }
                }
            }
        }

        internal static void Map(XValues xValues, DocumentObjectModel.Shapes.Charts.XValues domXValues)
        {
            var mapper = new XValuesMapper();
            mapper.MapObject(xValues, domXValues);
        }
    }
}
