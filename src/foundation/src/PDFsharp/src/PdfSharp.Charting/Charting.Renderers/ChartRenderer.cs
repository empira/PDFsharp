// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the base class for all chart renderers.
    /// </summary>
    abstract class ChartRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the ChartRenderer class with the specified renderer parameters.
        /// </summary>
        internal ChartRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the space used by the legend and returns the remaining space available for the
        /// other parts of the chart.
        /// </summary>
        protected XRect LayoutLegend()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            XRect remainingRect = _rendererParms.Box;
            if (cri.LegendRendererInfo != null!)
            {
                switch (cri.LegendRendererInfo.Legend.Docking)
                {
                    case DockingType.Left:
                        cri.LegendRendererInfo.X = remainingRect.Left;
                        cri.LegendRendererInfo.Y = remainingRect.Height / 2 - cri.LegendRendererInfo.Height / 2;
                        double width = cri.LegendRendererInfo.Width + LegendSpacing;
                        remainingRect.X += width;
                        remainingRect.Width -= width;
                        break;

                    case DockingType.Right:
                        cri.LegendRendererInfo.X = remainingRect.Right - cri.LegendRendererInfo.Width;
                        cri.LegendRendererInfo.Y = remainingRect.Height / 2 - cri.LegendRendererInfo.Height / 2;
                        remainingRect.Width -= cri.LegendRendererInfo.Width + LegendSpacing;
                        break;

                    case DockingType.Top:
                        cri.LegendRendererInfo.X = remainingRect.Width / 2 - cri.LegendRendererInfo.Width / 2;
                        cri.LegendRendererInfo.Y = remainingRect.Top;
                        double height = cri.LegendRendererInfo.Height + LegendSpacing;
                        remainingRect.Y += height;
                        remainingRect.Height -= height;
                        break;

                    case DockingType.Bottom:
                        cri.LegendRendererInfo.X = remainingRect.Width / 2 - cri.LegendRendererInfo.Width / 2;
                        cri.LegendRendererInfo.Y = remainingRect.Bottom - cri.LegendRendererInfo.Height;
                        remainingRect.Height -= cri.LegendRendererInfo.Height + LegendSpacing;
                        break;
                }
            }
            return remainingRect;
        }

        /// <summary>
        /// Used to separate the legend from the plot area.
        /// </summary>
        const double LegendSpacing = 0;

        /// <summary>
        /// Represents the default width for all series lines, like borders in column/bar charts.
        /// </summary>
        protected static readonly double DefaultSeriesLineWidth = 0.15;
    }
}
