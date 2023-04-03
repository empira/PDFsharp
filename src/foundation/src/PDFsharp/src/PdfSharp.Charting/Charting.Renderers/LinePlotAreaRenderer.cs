// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Renders the plot area used by line charts. 
    /// </summary>
    class LinePlotAreaRenderer : ColumnLikePlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the LinePlotAreaRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal LinePlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Draws the content of the line plot area.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            XRect plotAreaRect = cri.PlotAreaRendererInfo.Rect;
            if (plotAreaRect.IsEmpty)
                return;

            var gfx = _rendererParms.Graphics;
            var state = gfx.Save();
            {
                //gfx.SetClip(plotAreaRect, XCombineMode.Intersect);
                gfx.IntersectClip(plotAreaRect);

                //TODO Treat null values correctly.
                //     Points can be missing. Treat null values accordingly (NotPlotted, Interpolate etc.)

                // Draw lines and markers for each data series.
                XMatrix matrix = cri.PlotAreaRendererInfo.Matrix;

                double xMajorTick = cri.XAxisRendererInfo?.MajorTick ?? 0;
                foreach (var sri in cri.SeriesRendererInfos)
                {
                    int count = sri.Series.Elements.Count;
                    XPoint[] points = new XPoint[count];
                    for (int idx = 0; idx < count; idx++)
                    {
                        double v = sri.Series.Elements[idx].Value;
                        if (Double.IsNaN(v))
                            v = 0;
                        points[idx] = new XPoint(idx + xMajorTick / 2, v);
                    }

                    matrix.TransformPoints(points);
                    gfx.DrawLines(sri.LineFormat, points);
                    DrawMarker(gfx, points, sri);
                }

                //gfx.ResetClip();
            }
            gfx.Restore(state);
        }

        /// <summary>
        /// Draws all markers given in rendererInfo at the positions specified by points.
        /// </summary>
        void DrawMarker(XGraphics graphics, XPoint[] points, SeriesRendererInfo rendererInfo)
        {
            foreach (var pos in points)
                MarkerRenderer.Draw(graphics, pos, rendererInfo.MarkerRendererInfo);
        }
    }
}
