// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a plot area renderer of clustered bars, i. e. all bars are drawn side by side.
    /// </summary>
    class BarClusteredPlotAreaRenderer : BarPlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the BarClusteredPlotAreaRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal BarClusteredPlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the position, width, and height of each bar of all series.
        /// </summary>
        protected override void CalcBars()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.SeriesRendererInfos.Length == 0)
                return;

            double xMax = cri.XAxisRendererInfo?.MaximumScale ?? 0;
            double yMin = cri.YAxisRendererInfo?.MinimumScale ?? 0;
            double yMax = cri.YAxisRendererInfo?.MaximumScale ?? 0;

            int pointCount = 0;
            foreach (var sri in cri.SeriesRendererInfos)
                pointCount += sri.Series._seriesElements?.Count ?? 0;

            // Space shared by one clustered bar.
            double groupWidth = cri.XAxisRendererInfo?.MajorTick ?? 0;

            // Space used by one bar.
            double columnWidth = groupWidth * 0.75 / cri.SeriesRendererInfos.Length;

            int seriesIdx = 0;
            XPoint[] points = new XPoint[2];
            foreach (var sri in cri.SeriesRendererInfos)
            {
                // Set x to first clustered bar for each series.
                double x = xMax - groupWidth / 2;

                // Offset for bars of a particular series from the start of a clustered bar.
                double dx = (columnWidth * seriesIdx) - (columnWidth / 2 * cri.SeriesRendererInfos.Length);
                double y0 = yMin;

                foreach (var info in sri.PointRendererInfos)
                {
                    var column = info as ColumnRendererInfo ??
                                 throw new InvalidOperationException("ColumnRendererInfo expected.");

                    if (column.Point != null!)
                    {
                        double x0 = x - dx;
                        double x1 = x - dx - columnWidth;
                        double y1 = column.Point.Value;

                        // Draw from zero base line, if it exists.
                        if (y0 < 0 && yMax >= 0)
                            y0 = 0;

                        // y0 should always be lower than y1, i. e. draw bar from bottom to top.
                        if (y1 < 0 && y1 < y0)
                        {
                            (y0, y1) = (y1, y0);
                        }

                        points[0].X = y0; // upper left
                        points[0].Y = x0;
                        points[1].X = y1; // lower right
                        points[1].Y = x1;

                        cri.PlotAreaRendererInfo.Matrix.TransformPoints(points);

                        column.Rect = new XRect(points[0].X,
                          points[1].Y,
                          points[1].X - points[0].X,
                          points[0].Y - points[1].Y);
                    }
                    x--; // Next clustered bar.
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
