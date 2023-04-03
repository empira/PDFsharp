// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a Y axis renderer used for charts of type BarStacked2D.
    /// </summary>
    class HorizontalStackedYAxisRenderer : HorizontalYAxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the HorizontalStackedYAxisRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal HorizontalStackedYAxisRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Determines the sum of the smallest and the largest stacked bar
        /// from all series of the chart.
        /// </summary>
        protected override void CalcYAxis(out double yMin, out double yMax)
        {
            yMin = Double.MaxValue;
            yMax = Double.MinValue;

            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            int maxPoints = 0;
            foreach (var sri in cri.SeriesRendererInfos)
                maxPoints = Math.Max(maxPoints, sri.Series._seriesElements?.Count ?? NRT.ThrowOnNull<int>());

            for (int pointIdx = 0; pointIdx < maxPoints; ++pointIdx)
            {
                double valueSumPos = 0, valueSumNeg = 0;
                foreach (var sri in cri.SeriesRendererInfos)
                {
                    if (sri.PointRendererInfos.Length <= pointIdx)
                        break;

                    var column = (ColumnRendererInfo)sri.PointRendererInfos[pointIdx];
                    if (column.Point != null && !Double.IsNaN(column.Point.Value))
                    {
                        if (column.Point.Value < 0)
                            valueSumNeg += column.Point.Value;
                        else
                            valueSumPos += column.Point.Value;
                    }
                }
                yMin = Math.Min(valueSumNeg, yMin);
                yMax = Math.Max(valueSumPos, yMax);
            }
        }
    }
}
