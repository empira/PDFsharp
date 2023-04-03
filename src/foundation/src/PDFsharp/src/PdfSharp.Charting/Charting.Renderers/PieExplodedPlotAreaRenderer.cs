// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a exploded pie plot area renderer.
    /// </summary>
    class PieExplodedPlotAreaRenderer : PiePlotAreaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the PieExplodedPlotAreaRenderer class
        /// with the specified renderer parameters.
        /// </summary>
        internal PieExplodedPlotAreaRenderer(RendererParameters parms) : base(parms)
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
                foreach (var dleri in sri.DataLabelRendererInfo.Entries)
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

            XPoint origin = new XPoint(pieRect.X + pieRect.Width / 2, pieRect.Y + pieRect.Height / 2);
            XRect innerRect = new XRect();
            XPoint p1 = new XPoint();

            double midAngle = 0, sectorStartAngle = 0, sectorSweepAngle = 0,
                   deltaAngle = 2, startAngle = 270, sweepAngle = 0,
                   rInnerCircle = pieRect.Width / 15,
                   rOuterCircle = pieRect.Width / 2;

            foreach (var info in sri.PointRendererInfos)
            {
                var sector = info as SectorRendererInfo ??
                             throw new InvalidOperationException("SectorRendererInfo expected.");
                sector.Check();

                if (!Double.IsNaN(sector.Point!.Value) && sector.Point.Value != 0)
                {
                    sweepAngle = 360 / (sumValues / Math.Abs(sector.Point.Value));

                    midAngle = startAngle + sweepAngle / 2;
                    sectorStartAngle = Math.Max(0, startAngle + deltaAngle);
                    sectorSweepAngle = Math.Max(sweepAngle, sweepAngle - deltaAngle);

                    p1.X = origin.X + rInnerCircle * Math.Cos(midAngle / 180 * Math.PI);
                    p1.Y = origin.Y + rInnerCircle * Math.Sin(midAngle / 180 * Math.PI);
                    innerRect.X = p1.X - rOuterCircle + rInnerCircle;
                    innerRect.Y = p1.Y - rOuterCircle + rInnerCircle;
                    innerRect.Width = (rOuterCircle - rInnerCircle) * 2;
                    innerRect.Height = innerRect.Width;

                    sector.Rect = innerRect;
                    sector.StartAngle = sectorStartAngle;
                    sector.SweepAngle = sectorSweepAngle;

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
