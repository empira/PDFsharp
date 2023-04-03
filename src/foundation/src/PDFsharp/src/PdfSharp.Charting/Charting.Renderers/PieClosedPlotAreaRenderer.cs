// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a closed pie plot area renderer.
    /// </summary>
    class PieClosedPlotAreaRenderer : PiePlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the PiePlotAreaRenderer class
        /// with the specified renderer parameters.
        /// </summary>
        internal PieClosedPlotAreaRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculate angles for each sector.
        /// </summary>
        protected override void CalcSectors()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.SeriesRendererInfos.Length == 0)
                return;

            var sri = cri.SeriesRendererInfos[0];

            double sumValues = sri.SumOfPoints;
            if (sumValues == 0)
                return;

            double textMeasure = 0;
            if (sri.DataLabelRendererInfo != null && sri.DataLabelRendererInfo.Position == DataLabelPosition.OutsideEnd)
            {
                foreach (DataLabelEntryRendererInfo dleri in sri.DataLabelRendererInfo.Entries)
                {
                    textMeasure = Math.Max(textMeasure, dleri.Width);
                    textMeasure = Math.Max(textMeasure, dleri.Height);
                }
            }

            XRect pieRect = cri.PlotAreaRendererInfo.Rect;
            if (textMeasure != 0)
            {
                pieRect.X += textMeasure;
                pieRect.Y += textMeasure;
                pieRect.Width -= 2 * textMeasure;
                pieRect.Height -= 2 * textMeasure;
            }

            double startAngle = 270;
            foreach (SectorRendererInfo sector in sri.PointRendererInfos)
            {
                if (!Double.IsNaN(sector.Point.Value) && sector.Point.Value != 0)
                {
                    var sweepAngle = 360 / (sumValues / Math.Abs(sector.Point.Value));

                    sector.Rect = pieRect;
                    sector.StartAngle = startAngle;
                    sector.SweepAngle = sweepAngle;

                    startAngle += sweepAngle;
                }
                else
                {
                    sector.StartAngle = Double.NaN;
                    sector.SweepAngle = Double.NaN;
                }
            }
        }
    }
}
