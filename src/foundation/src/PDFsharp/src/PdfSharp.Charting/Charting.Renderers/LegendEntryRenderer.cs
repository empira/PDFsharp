// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the renderer for a legend entry.
    /// </summary>
    class LegendEntryRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the LegendEntryRenderer class with the specified renderer
        /// parameters.
        /// </summary>
        internal LegendEntryRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates the space used by the legend entry.
        /// </summary>
        internal override void Format()
        {
            var gfx = _rendererParms.Graphics;
            var leri = (LegendEntryRendererInfo)_rendererParms.RendererInfo;

            // Initialize
            leri.MarkerArea.Width = (float_)MaxLegendMarkerWidth;
            leri.MarkerArea.Height = (float_)MaxLegendMarkerHeight;
            leri.MarkerSize = new XSize();
            leri.MarkerSize.Width = leri.MarkerArea.Width;
            leri.MarkerSize.Height = leri.MarkerArea.Height;
            if (leri.SeriesRendererInfo.Series._chartType == ChartType.Line)
                leri.MarkerArea.Width *= 3;
            leri.Width = leri.MarkerArea.Width;
            leri.Height = leri.MarkerArea.Height;

            if (leri.EntryText != "")
            {
                leri.TextSize = gfx.MeasureString(leri.EntryText, leri.LegendRendererInfo.Font);
                if (leri.SeriesRendererInfo.Series._chartType == ChartType.Line)
                {
                    leri.MarkerSize.Width = (float_)leri.SeriesRendererInfo.MarkerRendererInfo.MarkerSize.Value;
                    leri.MarkerArea.Width = Math.Max(3 * leri.MarkerSize.Width, leri.MarkerArea.Width);
                }

                leri.MarkerArea.Height = Math.Min(leri.MarkerArea.Height, leri.TextSize.Height);
                leri.MarkerSize.Height = Math.Min(leri.MarkerSize.Height, leri.TextSize.Height);
                leri.Width = leri.TextSize.Width + leri.MarkerArea.Width + SpacingBetweenMarkerAndText;
                leri.Height = leri.TextSize.Height;
            }
        }

        /// <summary>
        /// Draws one legend entry.
        /// </summary>
        internal override void Draw()
        {
            var gfx = _rendererParms.Graphics;
            var leri = (LegendEntryRendererInfo)_rendererParms.RendererInfo;

            XRect rect;
            if (leri.SeriesRendererInfo.Series._chartType == ChartType.Line)
            {
                // Draw line.
                XPoint posLineStart = new XPoint((float_)leri.X, (float_)(leri.Y + leri.Height / 2));
                XPoint posLineEnd = new XPoint((float_)(leri.X + leri.MarkerArea.Width), (float_)(leri.Y + leri.Height / 2));
                gfx.DrawLine(new XPen(((XSolidBrush)leri.MarkerBrush).Color), posLineStart, posLineEnd);

                // Draw marker.
                double x = leri.X + leri.MarkerArea.Width / 2;
                XPoint posMarker = new XPoint((float_)x, (float_)(leri.Y + leri.Height / 2));
                MarkerRenderer.Draw(gfx, posMarker, leri.SeriesRendererInfo.MarkerRendererInfo);
            }
            else
            {
                // Draw series rectangle for column, bar or pie charts.
                rect = new XRect((float_)leri.X, (float_)leri.Y, leri.MarkerArea.Width, leri.MarkerArea.Height);
                rect.Y += (float_)((leri.Height - leri.MarkerArea.Height) / 2);
                gfx.DrawRectangle(leri.MarkerPen, leri.MarkerBrush, rect);
            }

            // Draw text
            if (leri.EntryText.Length > 0)
            {
                rect = leri.Rect;
                rect.X += (float_)(leri.MarkerArea.Width + LegendEntryRenderer.SpacingBetweenMarkerAndText);
                XStringFormat format = new XStringFormat();
                format.LineAlignment = XLineAlignment.Near;
                gfx.DrawString(leri.EntryText, leri.LegendRendererInfo.Font,
                               leri.LegendRendererInfo.FontColor, rect, format);
            }
        }

        /// <summary>
        /// Absolute width for markers (including line) in point.
        /// </summary>
        const double MarkerWidth = 4.3; // 1.5 mm

        /// <summary>
        /// Maximum legend marker width in point.
        /// </summary>
        const double MaxLegendMarkerWidth = 7; // 2.5 mm

        /// <summary>
        /// Maximum legend marker height in point.
        /// </summary>
        const double MaxLegendMarkerHeight = 7; // 2.5 mm

        /// <summary>
        /// Insert spacing between marker and text in point.
        /// </summary>
        const double SpacingBetweenMarkerAndText = 4.3; // 1.5 mm
    }
}
