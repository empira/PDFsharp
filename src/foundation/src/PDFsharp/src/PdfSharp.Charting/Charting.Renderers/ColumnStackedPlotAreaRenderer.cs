// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a plot area renderer of stacked columns, i. e. all columns are drawn one on another.
    /// </summary>
    class ColumnStackedPlotAreaRenderer : ColumnPlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnStackedPlotAreaRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnStackedPlotAreaRenderer(RendererParameters parms) : base(parms)
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
            double xMajorTick = cri.XAxisRendererInfo?.MajorTick ?? 0;

            int maxPoints = 0;
            foreach (var sri in cri.SeriesRendererInfos)
                maxPoints = Math.Max(maxPoints, sri.Series._seriesElements?.Count ?? NRT.ThrowOnNull<int>());

            double x = xMin + xMajorTick / 2;

            // Space used by one column.
            double columnWidth = xMajorTick * 0.75 / 2;

            XPoint[] points = new XPoint[2];
            for (int pointIdx = 0; pointIdx < maxPoints; ++pointIdx)
            {
                // Set x to first clustered column for each series.
                double yMin = 0, yMax = 0, y0 = 0, y1 = 0;
                double x0 = x - columnWidth;
                double x1 = x + columnWidth;

                foreach (var sri in cri.SeriesRendererInfos)
                {
                    if (sri.PointRendererInfos.Length <= pointIdx)
                        break;

                    var column = (ColumnRendererInfo)sri.PointRendererInfos[pointIdx];
                    if (column.Point != null && !Double.IsNaN(column.Point.Value))
                    {
                        double y = column.Point.Value;
                        if (y < 0)
                        {
                            y0 = yMin + y;
                            y1 = yMin;
                            yMin += y;
                        }
                        else
                        {
                            y0 = yMax;
                            y1 = yMax + y;
                            yMax += y;
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
                }
                x++; // Next stacked column.
            }
        }

        /// <summary>
        /// Stacked columns are always inside.
        /// </summary>
        protected override bool IsDataInside(double yMin, double yMax, double yValue) 
            => true;
    }
}
