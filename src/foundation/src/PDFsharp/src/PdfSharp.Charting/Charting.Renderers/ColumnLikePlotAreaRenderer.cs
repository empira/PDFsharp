// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Base class for all plot area renderers.
    /// </summary>
    abstract class ColumnLikePlotAreaRenderer : PlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnLikePlotAreaRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnLikePlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Layouts and calculates the space for column like plot areas.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            double xMin = cri.XAxisRendererInfo?.MinimumScale ?? 0;
            double xMax = cri.XAxisRendererInfo?.MaximumScale ?? 0;
            double yMin = cri.YAxisRendererInfo?.MinimumScale ?? 0;
            double yMax = cri.YAxisRendererInfo?.MaximumScale ?? 0;

            XRect plotAreaBox = cri.PlotAreaRendererInfo.Rect;

            cri.PlotAreaRendererInfo.Matrix = new XMatrix();
            cri.PlotAreaRendererInfo.Matrix.TranslatePrepend(-xMin, yMax);
            cri.PlotAreaRendererInfo.Matrix.Scale(plotAreaBox.Width / xMax, plotAreaBox.Height / (yMax - yMin), XMatrixOrder.Append);
            cri.PlotAreaRendererInfo.Matrix.ScalePrepend(1, -1);
            cri.PlotAreaRendererInfo.Matrix.Translate(plotAreaBox.X, plotAreaBox.Y, XMatrixOrder.Append);
        }
    }
}
