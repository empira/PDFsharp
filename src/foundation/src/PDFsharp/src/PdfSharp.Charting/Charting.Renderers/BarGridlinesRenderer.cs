// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents gridlines used by bar charts, i. e. X axis grid will be rendered
    /// from left to right and Y axis grid will be rendered from top to bottom of the plot area.
    /// </summary>
    class BarGridlinesRenderer : GridlinesRenderer
    {
        /// <summary>
        /// Initializes a new instance of the BarGridlinesRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal BarGridlinesRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Draws the gridlines into the plot area.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            XRect plotAreaRect = cri.PlotAreaRendererInfo.Rect;
            if (plotAreaRect.IsEmpty)
                return;

            var xari = cri.XAxisRendererInfo;
            var yari = cri.YAxisRendererInfo;

            if (xari == null || yari == null)
                return;

            double xMin = xari.MinimumScale;
            double xMax = xari.MaximumScale;
            double yMin = yari.MinimumScale;
            double yMax = yari.MaximumScale;
            double xMajorTick = xari.MajorTick;
            double yMajorTick = yari.MajorTick;
            double xMinorTick = xari.MinorTick;
            double yMinorTick = yari.MinorTick;
            double xMaxExtension = xari.MajorTick;

            XMatrix matrix = cri.PlotAreaRendererInfo.Matrix;

            LineFormatRenderer lineFormatRenderer;
            var gfx = _rendererParms.Graphics;

            XPoint[] points = new XPoint[2];
            if (xari.MinorGridlinesLineFormat != null)
            {
                lineFormatRenderer = new LineFormatRenderer(gfx, xari.MinorGridlinesLineFormat);
                for (double x = xMin + xMinorTick; x < xMax; x += xMinorTick)
                {
                    points[0].Y = x;
                    points[0].X = yMin;
                    points[1].Y = x;
                    points[1].X = yMax;
                    matrix.TransformPoints(points);
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            if (xari.MajorGridlinesLineFormat != null)
            {
                lineFormatRenderer = new LineFormatRenderer(gfx, xari.MajorGridlinesLineFormat);
                for (double x = xMin; x <= xMax; x += xMajorTick)
                {
                    points[0].Y = x;
                    points[0].X = yMin;
                    points[1].Y = x;
                    points[1].X = yMax;
                    matrix.TransformPoints(points);
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            if (yari.MinorGridlinesLineFormat != null)
            {
                lineFormatRenderer = new LineFormatRenderer(gfx, yari.MinorGridlinesLineFormat);
                for (double y = yMin + yMinorTick; y < yMax; y += yMinorTick)
                {
                    points[0].Y = xMin;
                    points[0].X = y;
                    points[1].Y = xMax;
                    points[1].X = y;
                    matrix.TransformPoints(points);
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            if (yari.MajorGridlinesLineFormat != null)
            {
                lineFormatRenderer = new LineFormatRenderer(gfx, yari.MajorGridlinesLineFormat);
                for (double y = yMin; y <= yMax; y += yMajorTick)
                {
                    points[0].Y = xMin;
                    points[0].X = y;
                    points[1].Y = xMax;
                    points[1].X = y;
                    matrix.TransformPoints(points);
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }
        }
    }
}
