// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the base for all pie plot area renderer.
    /// </summary>
    abstract class PiePlotAreaRenderer : PlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the PiePlotAreaRenderer class
        /// with the specified renderer parameters.
        /// </summary>
        internal PiePlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Layouts and calculates the space used by the pie plot area.
        /// </summary>
        internal override void Format() 
            => CalcSectors();

        /// <summary>
        /// Draws the content of the pie plot area.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            XRect plotAreaRect = cri.PlotAreaRendererInfo.Rect;
            if (plotAreaRect.IsEmpty)
                return;

            if (cri.SeriesRendererInfos.Length == 0)
                return;

            var gfx = _rendererParms.Graphics;
            var state = gfx.Save();

            // Draw sectors.
            var sri = cri.SeriesRendererInfos[0];
            foreach (SectorRendererInfo sector in sri.PointRendererInfos)
            {
                if (!Double.IsNaN(sector.StartAngle) && !Double.IsNaN(sector.SweepAngle))
                    gfx.DrawPie(sector.FillFormat, sector.Rect, sector.StartAngle, sector.SweepAngle);
            }

            // Draw border of the sectors.
            foreach (SectorRendererInfo sector in sri.PointRendererInfos)
            {
                if (!Double.IsNaN(sector.StartAngle) && !Double.IsNaN(sector.SweepAngle))
                    gfx.DrawPie(sector.LineFormat, sector.Rect, sector.StartAngle, sector.SweepAngle);
            }

            gfx.Restore(state);
        }

        /// <summary>
        /// Calculates the specific positions for each sector.
        /// </summary>
        protected abstract void CalcSectors();
    }
}
