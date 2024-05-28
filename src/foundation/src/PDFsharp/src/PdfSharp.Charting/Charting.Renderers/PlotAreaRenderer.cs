// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Base class for all plot area renderers.
    /// </summary>
    abstract class PlotAreaRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the PlotAreaRenderer class with the specified renderer parameters.
        /// </summary>
        internal PlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Returns an initialized PlotAreaRendererInfo.
        /// </summary>
        internal override RendererInfo Init()
        {
            var pari = new PlotAreaRendererInfo
            {
                PlotArea = ((ChartRendererInfo)_rendererParms.RendererInfo).Chart._plotArea ?? NRT.ThrowOnNull<PlotArea>()
            };
            InitLineFormat(pari);
            InitFillFormat(pari);
            return pari;
        }

        /// <summary>
        /// Initializes the plot area’s line format common to all derived plot area renderers.
        /// If line format is given all uninitialized values will be set.
        /// </summary>
        protected void InitLineFormat(PlotAreaRendererInfo rendererInfo)
        {
            if (rendererInfo.PlotArea._lineFormat != null)
                rendererInfo.LineFormat = Converter.ToXPen(rendererInfo.PlotArea._lineFormat, XColors.Black, DefaultLineWidth);
        }

        /// <summary>
        /// Initializes the plot area’s fill format common to all derived plot area renderers.
        /// If fill format is given all uninitialized values will be set.
        /// </summary>
        protected void InitFillFormat(PlotAreaRendererInfo rendererInfo)
        {
            if (rendererInfo.PlotArea._fillFormat != null)
                rendererInfo.FillFormat = Converter.ToXBrush(rendererInfo.PlotArea._fillFormat, XColors.White);
        }

        /// <summary>
        /// Represents the default line width for the plot area’s border.
        /// </summary>
        protected const double DefaultLineWidth = 0.15;
    }
}
