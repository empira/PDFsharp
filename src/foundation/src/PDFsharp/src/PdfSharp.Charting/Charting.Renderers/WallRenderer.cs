// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a renderer for the plot area background.
    /// </summary>
    class WallRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the WallRenderer class with the specified renderer parameters.
        /// </summary>
        internal WallRenderer(RendererParameters parameters) : base(parameters)
        { }

        /// <summary>
        /// Draws the wall.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.PlotAreaRendererInfo.FillFormat != null!)
            {
                var plotAreaBox = cri.PlotAreaRendererInfo.Rect;
                if (plotAreaBox.IsEmpty)
                    return;

                _rendererParms.Graphics.DrawRectangle(cri.PlotAreaRendererInfo.FillFormat, plotAreaBox);
            }
        }
    }
}
