// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a plot area renderer of clustered columns, i. e. all columns are drawn side by side.
    /// </summary>
    abstract class ColumnPlotAreaRenderer : ColumnLikePlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnPlotAreaRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnPlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Layouts and calculates the space for each column.
        /// </summary>
        internal override void Format()
        {
            base.Format();
            CalcColumns();
        }

        /// <summary>
        /// Draws the content of the column plot area.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            XRect plotAreaBox = cri.PlotAreaRendererInfo.Rect;
            if (plotAreaBox.IsEmpty)
                return;

            var gfx = _rendererParms.Graphics;

            double xMin = cri.XAxisRendererInfo?.MinimumScale ?? 0;
            double xMax = cri.XAxisRendererInfo?.MaximumScale ?? 0;
            double yMin = cri.YAxisRendererInfo?.MinimumScale ?? 0;
            double yMax = cri.YAxisRendererInfo?.MaximumScale ?? 0;

            LineFormatRenderer lineFormatRenderer;

            // Under some circumstances it is possible that no zero base line will be drawn,
            // e. g. because of unfavorable minimum/maximum scale and/or major tick, so force to draw
            // a zero base line if necessary.
            if (cri.YAxisRendererInfo?.MajorGridlinesLineFormat != null ||
                cri.YAxisRendererInfo?.MinorGridlinesLineFormat != null)
            {
                if (yMin < 0 && yMax > 0)
                {
                    var points = new XPoint[2];
                    points[0].X = xMin;
                    points[0].Y = 0;
                    points[1].X = xMax;
                    points[1].Y = 0;
                    cri.PlotAreaRendererInfo.Matrix.TransformPoints(points);

                    if (cri.YAxisRendererInfo.MinorGridlinesLineFormat != null)
                        lineFormatRenderer = new LineFormatRenderer(gfx, cri.YAxisRendererInfo.MinorGridlinesLineFormat);
                    else
                        lineFormatRenderer = new LineFormatRenderer(gfx, cri.YAxisRendererInfo.MajorGridlinesLineFormat);

                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            // Draw columns
            var state = gfx.Save();
            foreach (var sri in cri.SeriesRendererInfos)
            {
                foreach (var info in sri.PointRendererInfos)
                {
                    var column = info as ColumnRendererInfo ??
                                 throw new InvalidOperationException("ColumnRendererInfo expected.");

                    // Do not draw column if value is outside yMin/yMax range. Clipping does not make sense.
                    if (IsDataInside(yMin, yMax, column.Point.Value))
                        gfx.DrawRectangle(column.FillFormat, column.Rect);
                }
            }

            // Draw borders around column.
            // A border can overlap neighbor columns, so it is important to draw borders at the end.
            foreach (var sri in cri.SeriesRendererInfos)
            {
                foreach (var info in sri.PointRendererInfos)
                {
                    var column = info as ColumnRendererInfo ??
                                 throw new InvalidOperationException("ColumnRendererInfo expected.");

                    // Do not draw column if value is outside yMin/yMax range. Clipping does not make sense.
                    if (IsDataInside(yMin, yMax, column.Point.Value) && column.LineFormat.Width > 0)
                    {
                        lineFormatRenderer = new LineFormatRenderer(gfx, column.LineFormat);
                        lineFormatRenderer.DrawRectangle(column.Rect);
                    }
                }
            }
            gfx.Restore(state);
        }

        /// <summary>
        /// Calculates the position, width, and height of each column of all series.
        /// </summary>
        protected abstract void CalcColumns();

        /// <summary>
        /// If yValue is within the range from yMin to yMax returns true, otherwise false.
        /// </summary>
        protected abstract bool IsDataInside(double yMin, double yMax, double yValue);
    }
}
