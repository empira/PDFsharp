// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a data label renderer for column charts.
    /// </summary>
    class ColumnDataLabelRenderer : DataLabelRenderer
    {
        /// <summary>
        /// Initializes a new instance of the ColumnDataLabelRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal ColumnDataLabelRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the space used by the data labels.
        /// </summary>
        internal override void Format()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            foreach (var sri in cri.SeriesRendererInfos)
            {
                if (sri.DataLabelRendererInfo == null)
                    continue;

                var gfx = _rendererParms.Graphics;

                sri.DataLabelRendererInfo.Entries = new DataLabelEntryRendererInfo[sri.PointRendererInfos.Length];
                int index = 0;
                foreach (ColumnRendererInfo column in sri.PointRendererInfos)
                {
                    var dleri = new DataLabelEntryRendererInfo();
                    if (sri.DataLabelRendererInfo.Type != DataLabelType.None)
                    {
                        if (sri.DataLabelRendererInfo.Type == DataLabelType.Value)
                            dleri.Text = column.Point.Value.ToString(sri.DataLabelRendererInfo.Format);
                        else if (sri.DataLabelRendererInfo.Type == DataLabelType.Percent)
                            throw new InvalidOperationException(PSCSR.PercentNotSupportedByColumnDataLabel);

                        if (dleri.Text.Length > 0)
                            dleri.Size = gfx.MeasureString(dleri.Text, sri.DataLabelRendererInfo.Font);
                    }

                    sri.DataLabelRendererInfo.Entries[index++] = dleri;
                }
            }
            CalcPositions();
        }

        /// <summary>
        /// Draws the data labels of the column chart.
        /// </summary>
        internal override void Draw()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

            foreach (var sri in cri.SeriesRendererInfos)
            {
                if (sri.DataLabelRendererInfo == null)
                    continue;

                XGraphics gfx = _rendererParms.Graphics;
                XFont font = sri.DataLabelRendererInfo.Font;
                XBrush fontColor = sri.DataLabelRendererInfo.FontColor;
                XStringFormat format = XStringFormats.Center;
                format.LineAlignment = XLineAlignment.Center;
                foreach (DataLabelEntryRendererInfo dataLabel in sri.DataLabelRendererInfo.Entries)
                {
                    if (dataLabel.Text != null!)
                        gfx.DrawString(dataLabel.Text, font, fontColor, dataLabel.Rect, format);
                }
            }
        }

        /// <summary>
        /// Calculates the data label positions specific for column charts.
        /// </summary>
        internal override void CalcPositions()
        {
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            //var gfx = _rendererParms.Graphics;

            foreach (var sri in cri.SeriesRendererInfos)
            {
                if (sri.DataLabelRendererInfo == null)
                    continue;

                int columnIndex = 0;
                foreach (ColumnRendererInfo column in sri.PointRendererInfos)
                {
                    DataLabelEntryRendererInfo dleri = sri.DataLabelRendererInfo.Entries[columnIndex++];

                    dleri.X = column.Rect.X + column.Rect.Width / 2 - dleri.Width / 2; // Always the same...
                    switch (sri.DataLabelRendererInfo.Position)
                    {
                        case DataLabelPosition.InsideEnd:
                            // Inner border of the column.
                            dleri.Y = column.Rect.Y;
                            if (column.Point.Value < 0)
                                dleri.Y = column.Rect.Y + column.Rect.Height - dleri.Height;
                            break;

                        case DataLabelPosition.Center:
                            // Centered inside the column.
                            dleri.Y = column.Rect.Y + column.Rect.Height / 2 - dleri.Height / 2;
                            break;

                        case DataLabelPosition.InsideBase:
                            // Aligned at the base of the column.
                            dleri.Y = column.Rect.Y + column.Rect.Height - dleri.Height;
                            if (column.Point.Value < 0)
                                dleri.Y = column.Rect.Y;
                            break;

                        case DataLabelPosition.OutsideEnd:
                            // Outer border of the column.
                            dleri.Y = column.Rect.Y - dleri.Height;
                            if (column.Point.Value < 0)
                                dleri.Y = column.Rect.Y + column.Rect.Height;
                            break;
                    }
                }
            }
        }
    }
}
