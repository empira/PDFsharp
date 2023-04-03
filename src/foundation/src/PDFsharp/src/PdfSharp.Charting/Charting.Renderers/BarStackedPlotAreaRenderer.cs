// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a plot area renderer of stacked bars, i. e. all bars are drawn one on another.
    /// </summary>
    class BarStackedPlotAreaRenderer : BarPlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the BarStackedPlotAreaRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal BarStackedPlotAreaRenderer(RendererParameters parms) : base(parms)
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
            double xMajorTick = cri.XAxisRendererInfo?.MajorTick ?? 0;

            int maxPoints = 0;
            foreach (SeriesRendererInfo sri in cri.SeriesRendererInfos)
                maxPoints = Math.Max(maxPoints, sri.Series._seriesElements?.Count ?? 0);

            // Space used by one bar.
            double x = xMax - xMajorTick / 2;
            double columnWidth = xMajorTick * 0.75 / 2;

            XPoint[] points = new XPoint[2];
            for (int pointIdx = 0; pointIdx < maxPoints; ++pointIdx)
            {
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

                        points[0].Y = x0; // top left
                        points[0].X = y0;
                        points[1].Y = x1; // bottom right
                        points[1].X = y1;

                        cri.PlotAreaRendererInfo.Matrix.TransformPoints(points);

                        column.Rect = new XRect(points[0].X,
                                                points[0].Y,
                                                points[1].X - points[0].X,
                                                points[1].Y - points[0].Y);
                    }
                }
                x--; // Next stacked column.
            }
        }

        /// <summary>
        /// If yValue is within the range from yMin to yMax returns true, otherwise false.
        /// </summary>
        protected override bool IsDataInside(double yMin, double yMax, double yValue)
            => yValue <= yMax && yValue >= yMin;
    }
}
