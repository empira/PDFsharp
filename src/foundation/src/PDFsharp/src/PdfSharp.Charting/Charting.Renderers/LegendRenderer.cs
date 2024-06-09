// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the legend renderer for all chart types.
    /// </summary>
    abstract class LegendRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the LegendRenderer class with the specified renderer parameters.
        /// </summary>
        internal LegendRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Layouts and calculates the space used by the legend.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            var lri = cri.LegendRendererInfo;
            if (lri == null)
                return;

            var parms = new RendererParameters
            {
                Graphics = _rendererParms.Graphics
            };

            bool verticalLegend = (lri.Legend._docking == DockingType.Left || lri.Legend._docking == DockingType.Right);
            XSize maxMarkerArea = new XSize();
            var ler = new LegendEntryRenderer(parms);
            foreach (LegendEntryRendererInfo leri in lri.Entries)
            {
                parms.RendererInfo = leri;
                ler.Format();

                maxMarkerArea.Width = Math.Max(leri.MarkerArea.Width, maxMarkerArea.Width);
                maxMarkerArea.Height = Math.Max(leri.MarkerArea.Height, maxMarkerArea.Height);

                if (verticalLegend)
                {
                    lri.Width = Math.Max(lri.Width, leri.Width);
                    lri.Height += leri.Height;
                }
                else
                {
                    lri.Width += leri.Width;
                    lri.Height = Math.Max(lri.Height, leri.Height);
                }
            }

            // Add padding to left, right, top and bottom
            int paddingFactor = 1;
            if (lri.BorderPen != null)
                paddingFactor = 2;
            lri.Width += (LegendRenderer.LeftPadding + LegendRenderer.RightPadding) * paddingFactor;
            lri.Height += (LegendRenderer.TopPadding + LegendRenderer.BottomPadding) * paddingFactor;
            if (verticalLegend)
                lri.Height += LegendRenderer.EntrySpacing * (lri.Entries.Length - 1);
            else
                lri.Width += LegendRenderer.EntrySpacing * (lri.Entries.Length - 1);

            foreach (LegendEntryRendererInfo leri in lri.Entries)
                leri.MarkerArea = maxMarkerArea;
        }

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
            var parms = new RendererParameters();
            parms.Graphics = gfx;

            var ler = new LegendEntryRenderer(parms);

            bool verticalLegend = (lri.Legend._docking == DockingType.Left || lri.Legend._docking == DockingType.Right);
            int paddingFactor = 1;
            if (lri.BorderPen != null)
                paddingFactor = 2;
            XRect legendRect = lri.Rect;
            legendRect.X += LegendRenderer.LeftPadding * paddingFactor;
            legendRect.Y += LegendRenderer.TopPadding * paddingFactor;
            if (cri.LegendRendererInfo != null)
            {
                foreach (var leri in cri.LegendRendererInfo.Entries)
                {
                    XRect entryRect = legendRect;
                    entryRect.Width = leri.Width;
                    entryRect.Height = leri.Height;

                    leri.Rect = entryRect;
                    parms.RendererInfo = leri;
                    ler.Draw();

                    if (verticalLegend)
                        legendRect.Y += entryRect.Height + LegendRenderer.EntrySpacing;
                    else
                        legendRect.X += entryRect.Width + LegendRenderer.EntrySpacing;
                }
            }

            // Draw border around legend
            if (lri.BorderPen != null)
            {
                XRect borderRect = lri.Rect;
                borderRect.X += LegendRenderer.LeftPadding;
                borderRect.Y += LegendRenderer.TopPadding;
                borderRect.Width -= LegendRenderer.LeftPadding + LegendRenderer.RightPadding;
                borderRect.Height -= LegendRenderer.TopPadding + LegendRenderer.BottomPadding;
                gfx.DrawRectangle(lri.BorderPen, borderRect);
            }
        }

        /// <summary>
        /// Used to insert a padding on the left.
        /// </summary>
        protected const double LeftPadding = 6;

        /// <summary>
        /// Used to insert a padding on the right.
        /// </summary>
        protected const double RightPadding = 6;

        /// <summary>
        /// Used to insert a padding at the top.
        /// </summary>
        protected const double TopPadding = 6;

        /// <summary>
        /// Used to insert a padding at the bottom.
        /// </summary>
        protected const double BottomPadding = 6;

        /// <summary>
        /// Used to insert a padding between entries.
        /// </summary>
        protected const double EntrySpacing = 5;

        /// <summary>
        /// Default line width used for the legend’s border.
        /// </summary>
        protected const double DefaultLineWidth = 0.14; // 0.05 mm
    }
}
