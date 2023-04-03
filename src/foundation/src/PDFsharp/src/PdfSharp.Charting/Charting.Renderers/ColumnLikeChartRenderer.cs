// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents column like chart renderer.
    /// </summary>
    abstract class ColumnLikeChartRenderer : ChartRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnLikeChartRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnLikeChartRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the chart layout.
        /// </summary>
        internal void CalcLayout()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            if (cri.XAxisRendererInfo == null || cri.YAxisRendererInfo == null)
                return;

            // Calculate rects and positions.
            XRect chartRect = LayoutLegend();
            cri.XAxisRendererInfo.X = chartRect.Left + cri.YAxisRendererInfo.Width;
            cri.XAxisRendererInfo.Y = chartRect.Bottom - cri.XAxisRendererInfo.Height;
            cri.XAxisRendererInfo.Width = chartRect.Width - cri.YAxisRendererInfo.Width;
            cri.YAxisRendererInfo.X = chartRect.Left;
            cri.YAxisRendererInfo.Y = chartRect.Top;
            cri.YAxisRendererInfo.Height = cri.XAxisRendererInfo.Y - chartRect.Top;
            cri.PlotAreaRendererInfo.X = cri.XAxisRendererInfo.X;
            cri.PlotAreaRendererInfo.Y = cri.YAxisRendererInfo.InnerRect.Y;
            cri.PlotAreaRendererInfo.Width = cri.XAxisRendererInfo.Width;
            cri.PlotAreaRendererInfo.Height = cri.YAxisRendererInfo.InnerRect.Height;
        }
    }
}
