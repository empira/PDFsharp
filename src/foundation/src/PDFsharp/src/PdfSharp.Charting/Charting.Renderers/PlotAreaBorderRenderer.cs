// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the border renderer for plot areas.
    /// </summary>
    class PlotAreaBorderRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the PlotAreaBorderRenderer class with the specified
        /// renderer parameters.
        /// </summary>
        internal PlotAreaBorderRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Draws the border around the plot area.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.PlotAreaRendererInfo.LineFormat != null && cri.PlotAreaRendererInfo.LineFormat.Width > 0)
            {
                var gfx = _rendererParms.Graphics;
                var lineFormatRenderer = new LineFormatRenderer(gfx, cri.PlotAreaRendererInfo.LineFormat);
                lineFormatRenderer.DrawRectangle(cri.PlotAreaRendererInfo.Rect);
            }
        }
    }
}
