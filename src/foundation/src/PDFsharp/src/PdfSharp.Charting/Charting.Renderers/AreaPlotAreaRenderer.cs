// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a plot area renderer of areas.
    /// </summary>
    class AreaPlotAreaRenderer : ColumnLikePlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the AreaPlotAreaRenderer class
        /// with the specified renderer parameters.
        /// </summary>
        internal AreaPlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Draws the content of the area plot area.
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

                XMatrix matrix = cri.PlotAreaRendererInfo.Matrix;
                double xMajorTick = cri.XAxisRendererInfo?.MajorTick ?? 0;
                foreach (var sri in cri.SeriesRendererInfos)
                {
                    int count = sri.Series.Elements.Count;
                    var points = new XPoint[count + 2];
                    points[0] = new XPoint(xMajorTick / 2, 0);
                    for (int idx = 0; idx < count; idx++)
                    {
                        double pointValue = sri.Series.Elements[idx].Value;
                        if (Double.IsNaN(pointValue))
                            pointValue = 0;
                        points[idx + 1] = new XPoint(idx + xMajorTick / 2, pointValue);
                    }

                    points[count + 1] = new XPoint(count - 1 + xMajorTick / 2, 0);
                    matrix.TransformPoints(points);
                    gfx.DrawPolygon(sri.LineFormat, sri.FillFormat, points, XFillMode.Winding);
                }

                //gfx.ResetClip();
            }
            gfx.Restore(state);
        }
    }
}
