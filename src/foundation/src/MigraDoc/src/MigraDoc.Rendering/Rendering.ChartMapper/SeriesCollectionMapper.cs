// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Charting;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering.ChartMapper
{
    /// <summary>
    /// The SeriesCollectionMapper class.
    /// </summary>
    public class SeriesCollectionMapper
    {
        void MapObject(SeriesCollection seriesCollection, DocumentObjectModel.Shapes.Charts.SeriesCollection domSeriesCollection)
        {
            foreach (DocumentObjectModel.Shapes.Charts.Series domSeries in domSeriesCollection.Cast<DocumentObjectModel.Shapes.Charts.Series>())
            {
                var series = seriesCollection.AddSeries();
                series.Name = domSeries.Name;

                if (domSeries.Values.ChartType is null)
                {
                    var chart = (DocumentObjectModel.Shapes.Charts.Chart?)DocumentObjectModel.DocumentRelations.GetParentOfType(domSeries, typeof(DocumentObjectModel.Shapes.Charts.Chart))
                        ?? NRT.ThrowOnNull<DocumentObjectModel.Shapes.Charts.Chart>();
                    series.ChartType = (ChartType)chart.Type;
                }
                else
                    series.ChartType = (ChartType)domSeries.ChartType;

                if (!domSeries.Values.DataLabel.IsValueNullOrEmpty())
                    DataLabelMapper.Map(series.DataLabel, domSeries.DataLabel);
                if (!domSeries.Values.LineFormat.IsValueNullOrEmpty())
                    LineFormatMapper.Map(series.LineFormat, domSeries.LineFormat);
                if (!domSeries.Values.FillFormat.IsValueNullOrEmpty())
                    FillFormatMapper.Map(series.FillFormat, domSeries.FillFormat);

                series.HasDataLabel = domSeries.HasDataLabel;
                if (domSeries.MarkerBackgroundColor.IsEmpty)
                    series.MarkerBackgroundColor = XColor.Empty;
                else
                {
#if noCMYK
                    series.MarkerBackgroundColor = XColor.FromArgb(domSeries.MarkerBackgroundColor.Argb);
#else
                    Debug.Assert(domSeries.Document != null, "domSeries.Document != null");
                    series.MarkerBackgroundColor =
                      ColorHelper.ToXColor(domSeries.MarkerBackgroundColor, domSeries.Document.UseCmykColor);
#endif
                }
                if (domSeries.MarkerForegroundColor.IsEmpty)
                    series.MarkerForegroundColor = XColor.Empty;
                else
                {
#if noCMYK
                    series.MarkerForegroundColor = XColor.FromArgb(domSeries.MarkerForegroundColor.Argb);
#else
                    Debug.Assert(domSeries.Document != null, "domSeries.Document != null");
                    series.MarkerForegroundColor =
                      ColorHelper.ToXColor(domSeries.MarkerForegroundColor, domSeries.Document.UseCmykColor);
#endif
                }
                series.MarkerSize = domSeries.MarkerSize.Point;
                if (domSeries.Values.MarkerStyle is not null)
                    series.MarkerStyle = (MarkerStyle)domSeries.MarkerStyle;

                foreach (DocumentObjectModel.Shapes.Charts.Point domPoint in domSeries.Elements.Cast<DocumentObjectModel.Shapes.Charts.Point>())
                {
                    if (domPoint != null)
                    {
                        Point point = series.Add(domPoint.Value);
                        FillFormatMapper.Map(point.FillFormat, domPoint.FillFormat);
                        LineFormatMapper.Map(point.LineFormat, domPoint.LineFormat);
                    }
                    else
                        series.Add(double.NaN);
                }
            }
        }

        internal static void Map(SeriesCollection seriesCollection, DocumentObjectModel.Shapes.Charts.SeriesCollection domSeriesCollection)
        {
            var mapper = new SeriesCollectionMapper();
            mapper.MapObject(seriesCollection, domSeriesCollection);
        }
    }
}
