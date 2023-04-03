// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a plot area renderer of clustered columns, i. e. all columns are drawn side by side.
    /// </summary>
    class ColumnClusteredPlotAreaRenderer : ColumnPlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnClusteredPlotAreaRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnClusteredPlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the position, width, and height of each column of all series.
        /// </summary>
        protected override void CalcColumns()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.SeriesRendererInfos.Length == 0)
                return;

            double xMin = cri.XAxisRendererInfo?.MinimumScale ?? 0;
            double yMin = cri.YAxisRendererInfo?.MinimumScale ?? 0;
            double yMax = cri.YAxisRendererInfo?.MaximumScale ?? 0;

            int pointCount = 0;
            foreach (SeriesRendererInfo sr in cri.SeriesRendererInfos)
                pointCount += sr.Series._seriesElements?.Count ?? 0;

            // Space shared by one clustered column.
            double groupWidth = cri.XAxisRendererInfo?.MajorTick ?? 0;

            // Space used by one column.
            double columnWidth = groupWidth * 3 / 4 / cri.SeriesRendererInfos.Length;

            int seriesIdx = 0;
            XPoint[] points = new XPoint[2];
            foreach (var sri in cri.SeriesRendererInfos)
            {
                // Set x to first clustered column for each series.
                double x = xMin + groupWidth / 2;

                // Offset for columns of a particular series from the start of a clustered cloumn.
                double dx = (columnWidth * seriesIdx) - (columnWidth / 2 * cri.SeriesRendererInfos.Length);

                foreach (var info in sri.PointRendererInfos)
                {
                    var column = info as ColumnRendererInfo ??
                                 throw new InvalidOperationException("ColumnRendererInfo expected.");

                    if (column.Point != null)
                    {
                        double x0 = x + dx;
                        double x1 = x + dx + columnWidth;
                        double y0 = yMin;
                        double y1 = column.Point.Value;

                        // Draw from zero base line, if it exists.
                        if (y0 < 0 && yMax >= 0)
                            y0 = 0;

                        // y0 should always be lower than y1, i. e. draw column from bottom to top.
                        if (y1 < 0 && y1 < y0)
                        {
                            (y0, y1) = (y1, y0);
                        }

                        points[0].X = x0; // upper left
                        points[0].Y = y1;
                        points[1].X = x1; // lower right
                        points[1].Y = y0;

                        cri.PlotAreaRendererInfo.Matrix.TransformPoints(points);

                        column.Rect = new XRect(points[0].X,
                                                points[0].Y,
                                                points[1].X - points[0].X,
                                                points[1].Y - points[0].Y);
                    }
                    x++; // Next clustered column.
                }
                seriesIdx++;
            }
        }

        /// <summary>
        /// If yValue is within the range from yMin to yMax returns true, otherwise false.
        /// </summary>
        protected override bool IsDataInside(double yMin, double yMax, double yValue) 
            => yValue <= yMax && yValue >= yMin;
    }
}
