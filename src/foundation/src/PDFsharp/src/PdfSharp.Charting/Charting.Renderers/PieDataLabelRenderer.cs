// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a data label renderer for pie charts.
    /// </summary>
    class PieDataLabelRenderer : DataLabelRenderer
    {
        /// <summary>
        /// Initializes a new instance of the PieDataLabelRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal PieDataLabelRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the space used by the data labels.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.SeriesRendererInfos.Length == 0)
                return;

            var sri = cri.SeriesRendererInfos[0];
            if (sri.DataLabelRendererInfo == null)
                return;

            double sumValues = sri.SumOfPoints;
            var gfx = _rendererParms.Graphics;

            sri.DataLabelRendererInfo.Entries = new DataLabelEntryRendererInfo[sri.PointRendererInfos.Length];
            int index = 0;
            foreach (var sector in sri.PointRendererInfos)
            {
                sector.Check();
                var dleri = new DataLabelEntryRendererInfo();
                if (sri.DataLabelRendererInfo.Type != DataLabelType.None)
                {
                    if (sri.DataLabelRendererInfo.Type == DataLabelType.Percent)
                    {
                        double percent = 100 / (sumValues / Math.Abs(sector.Point!.Value));
                        dleri.Text = percent.ToString(sri.DataLabelRendererInfo.Format) + "%";
                    }
                    else if (sri.DataLabelRendererInfo.Type == DataLabelType.Value)
                        dleri.Text = sector.Point!.Value.ToString(sri.DataLabelRendererInfo.Format);

                    if (dleri.Text.Length > 0)
                        dleri.Size = gfx.MeasureString(dleri.Text, sri.DataLabelRendererInfo.Font);
                }
                sri.DataLabelRendererInfo.Entries[index++] = dleri;
            }
        }

        /// <summary>
        /// Draws the data labels of the pie chart.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            if (cri.SeriesRendererInfos.Length == 0)
                return;

            var sri = cri.SeriesRendererInfos[0];
            if (sri == null || sri.DataLabelRendererInfo == null)
                return;

            var gfx = _rendererParms.Graphics;
            var font = sri.DataLabelRendererInfo.Font;
            var fontColor = sri.DataLabelRendererInfo.FontColor;
            var format = XStringFormats.Center;
            format.LineAlignment = XLineAlignment.Center;
            foreach (DataLabelEntryRendererInfo dataLabel in sri.DataLabelRendererInfo.Entries)
            {
                if (dataLabel.Text != null!)
                    gfx.DrawString(dataLabel.Text, font, fontColor, dataLabel.Rect, format);
            }
        }

        /// <summary>
        /// Calculates the data label positions specific for pie charts.
        /// </summary>
        internal override void CalcPositions()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            var gfx = _rendererParms.Graphics;

            if (cri.SeriesRendererInfos.Length > 0)
            {
                var sri = cri.SeriesRendererInfos[0];
                if (sri != null && sri.DataLabelRendererInfo != null)
                {
                    int sectorIndex = 0;
                    foreach (SectorRendererInfo sector in sri.PointRendererInfos)
                    {
                        // Determine output rectangle
                        double midAngle = sector.StartAngle + sector.SweepAngle / 2;
                        double radMidAngle = midAngle / 180 * Math.PI;
                        XPoint origin = new XPoint(sector.Rect.X + sector.Rect.Width / 2,
                          sector.Rect.Y + sector.Rect.Height / 2);
                        double radius = sector.Rect.Width / 2;
                        double halfradius = radius / 2;

                        var dleri = sri.DataLabelRendererInfo.Entries[sectorIndex++];
                        switch (sri.DataLabelRendererInfo.Position)
                        {
                            case DataLabelPosition.OutsideEnd:
                                // Outer border of the circle.
                                dleri.X = origin.X + (radius * Math.Cos(radMidAngle));
                                dleri.Y = origin.Y + (radius * Math.Sin(radMidAngle));
                                if (dleri.X < origin.X)
                                    dleri.X -= dleri.Width;
                                if (dleri.Y < origin.Y)
                                    dleri.Y -= dleri.Height;
                                break;

                            case DataLabelPosition.InsideEnd:
                                // Inner border of the circle.
                                dleri.X = origin.X + (radius * Math.Cos(radMidAngle));
                                dleri.Y = origin.Y + (radius * Math.Sin(radMidAngle));
                                if (dleri.X > origin.X)
                                    dleri.X -= dleri.Width;
                                if (dleri.Y > origin.Y)
                                    dleri.Y -= dleri.Height;
                                break;

                            case DataLabelPosition.Center:
                                // Centered
                                dleri.X = origin.X + (halfradius * Math.Cos(radMidAngle));
                                dleri.Y = origin.Y + (halfradius * Math.Sin(radMidAngle));
                                dleri.X -= dleri.Width / 2;
                                dleri.Y -= dleri.Height / 2;
                                break;

                            case DataLabelPosition.InsideBase:
                                // Aligned at the base/center of the circle
                                dleri.X = origin.X;
                                dleri.Y = origin.Y;
                                if (dleri.X < origin.X)
                                    dleri.X -= dleri.Width;
                                if (dleri.Y < origin.Y)
                                    dleri.Y -= dleri.Height;
                                break;
                        }
                    }
                }
            }
        }
    }
}
