// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the legend renderer specific to bar charts.
    /// </summary>
    class BarClusteredLegendRenderer : ColumnLikeLegendRenderer
    {
        /// <summary>
        /// Initializes a new instance of the BarClusteredLegendRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal BarClusteredLegendRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Draws the legend.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            var lri = cri.LegendRendererInfo;
            if (lri == null)
                return;

            var gfx = _rendererParms.Graphics;
            var parms = new RendererParameters
            {
                Graphics = gfx
            };

            var ler = new LegendEntryRenderer(parms);

            bool verticalLegend = (lri.Legend._docking == DockingType.Left || lri.Legend._docking == DockingType.Right);
            int paddingFactor = 1;
            if (lri.BorderPen != null!)
                paddingFactor = 2;
            XRect legendRect = lri.Rect;
            legendRect.X += LeftPadding * paddingFactor;
            if (verticalLegend)
                legendRect.Y = legendRect.Bottom - BottomPadding * paddingFactor;
            else
                legendRect.Y += TopPadding * paddingFactor;

            if (cri.LegendRendererInfo != null)
            {
                foreach (var leri in cri.LegendRendererInfo.Entries)
                {
                    if (verticalLegend)
                        legendRect.Y -= leri.Height;

                    XRect entryRect = legendRect;
                    entryRect.Width = leri.Width;
                    entryRect.Height = leri.Height;

                    leri.Rect = entryRect;
                    parms.RendererInfo = leri;
                    ler.Draw();

                    if (verticalLegend)
                        legendRect.Y -= EntrySpacing;
                    else
                        legendRect.X += entryRect.Width + EntrySpacing;
                }
            }

            // Draw border around legend
            if (lri.BorderPen != null)
            {
                XRect borderRect = lri.Rect;
                borderRect.X += LeftPadding;
                borderRect.Y += TopPadding;
                borderRect.Width -= LeftPadding + RightPadding;
                borderRect.Height -= TopPadding + BottomPadding;
                gfx.DrawRectangle(lri.BorderPen, borderRect);
            }
        }
    }
}
