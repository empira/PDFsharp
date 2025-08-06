// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents an axis renderer used for charts of type Bar2D.
    /// </summary>
    class VerticalXAxisRenderer : XAxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the VerticalXAxisRenderer class with the specified renderer parameters.
        /// </summary>
        internal VerticalXAxisRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Returns an initialized rendererInfo based on the X axis.
        /// </summary>
        internal override RendererInfo Init()
        {
            var chart = (Chart)_rendererParms.DrawingItem;

            var xari = new AxisRendererInfo
            {
                Axis = chart._xAxis
            };
            if (xari.Axis != null)
            {
                var cri = (ChartRendererInfo)_rendererParms.RendererInfo;

                CalculateXAxisValues(xari);
                InitXValues(xari);
                InitAxisTitle(xari, cri.DefaultFont);
                InitTickLabels(xari, cri.DefaultFont);
                InitAxisLineFormat(xari);
                InitGridlines(xari);
            }
            return xari;
        }

        /// <summary>
        /// Calculates the space used for the X axis.
        /// </summary>
        internal override void Format()
        {
            var xari = ((ChartRendererInfo)_rendererParms.RendererInfo).XAxisRendererInfo;
            if (xari?.Axis != null)
            {
                var atri = xari.AxisTitleRendererInfo;

                // Calculate space used for axis title.
                XSize titleSize = new XSize(0, 0);
                if (atri != null && atri.AxisTitleText != null && atri.AxisTitleText.Length > 0)
                    titleSize = _rendererParms.Graphics.MeasureString(atri.AxisTitleText, atri.AxisTitleFont);

                // Calculate space used for tick labels.
                var size = new XSize(0, 0);
                if (xari.XValues != null)
                {
                    foreach (XSeries xs in xari.XValues)
                    {
                        foreach (XValue xv in xs)
                        {
                            XSize valueSize = _rendererParms.Graphics.MeasureString(xv.ValueField, xari.TickLabelsFont);
                            size.Height += valueSize.Height;
                            size.Width = Math.Max(valueSize.Width, size.Width);
                        }
                    }
                }

                // Remember space for later drawing.
                if (atri != null)
                    atri.AxisTitleSize = titleSize;
                xari.TickLabelsHeight = size.Height;
                xari.Height = size.Height;
                xari.Width = titleSize.Width + size.Width + xari.MajorTickMarkWidth;
            }
        }

        /// <summary>
        /// Draws the horizontal X axis.
        /// </summary>
        internal override void Draw()
        {
            var gfx = _rendererParms.Graphics;
            var cri = (ChartRendererInfo)_rendererParms.RendererInfo;
            var xari = cri.XAxisRendererInfo;
            if (xari == null)
                return;

            double xMin = xari.MinimumScale;
            double xMax = xari.MaximumScale;
            double xMajorTick = xari.MajorTick;
            double xMinorTick = xari.MinorTick;
            double xMaxExtension = xari.MajorTick;

            // Draw tick labels. Each tick label will be aligned centered.
            int countTickLabels = (int)xMax;
            double tickLabelStep = xari.Height / countTickLabels;
            XPoint startPos = new XPoint(xari.X + xari.Width - xari.MajorTickMarkWidth, xari.Y + tickLabelStep / 2);
            if (xari.XValues != null)
            {
                foreach (XSeries xs in xari.XValues)
                {
                    for (int idx = countTickLabels - 1; idx >= 0; --idx)
                    {
                        var xv = xs[idx] ?? NRT.ThrowOnNull<XValue>();
                        string tickLabel = xv.ValueField;
                        var size = gfx.MeasureString(tickLabel, xari.TickLabelsFont);
                        gfx.DrawString(tickLabel, xari.TickLabelsFont, xari.TickLabelsBrush, startPos.X - size.Width, startPos.Y + size.Height / 2);
                        startPos.Y += tickLabelStep;
                    }
                }
            }

            // Draw axis.
            // First draw tick marks, second draw axis.
            double majorTickMarkStart = 0, majorTickMarkEnd = 0,
                   minorTickMarkStart = 0, minorTickMarkEnd = 0;
            GetTickMarkPos(xari, ref majorTickMarkStart, ref majorTickMarkEnd, ref minorTickMarkStart, ref minorTickMarkEnd);

            var lineFormatRenderer = new LineFormatRenderer(gfx, xari.LineFormat);
            XPoint[] points = new XPoint[2];

            // Minor ticks.
            if (xari.MinorTickMark != TickMarkType.None)
            {
                int countMinorTickMarks = (int)(xMax / xMinorTick);
                double minorTickMarkStep = xari.Height / countMinorTickMarks;
                startPos.Y = xari.Y;
                for (int x = 0; x <= countMinorTickMarks; x++)
                {
                    points[0].X = minorTickMarkStart;
                    points[0].Y = startPos.Y + minorTickMarkStep * x;
                    points[1].X = minorTickMarkEnd;
                    points[1].Y = points[0].Y;
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            // Major ticks.
            if (xari.MajorTickMark != TickMarkType.None)
            {
                int countMajorTickMarks = (int)(xMax / xMajorTick);
                double majorTickMarkStep = xari.Height / countMajorTickMarks;
                startPos.Y = xari.Y;
                for (int x = 0; x <= countMajorTickMarks; x++)
                {
                    points[0].X = majorTickMarkStart;
                    points[0].Y = startPos.Y + majorTickMarkStep * x;
                    points[1].X = majorTickMarkEnd;
                    points[1].Y = points[0].Y;
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            // Axis.
            if (xari.LineFormat != null)
            {
                points[0].X = xari.X + xari.Width;
                points[0].Y = xari.Y;
                points[1].X = xari.X + xari.Width;
                points[1].Y = xari.Y + xari.Height;
                if (xari.MajorTickMark != TickMarkType.None)
                {
                    points[0].Y -= xari.LineFormat.Width / 2;
                    points[1].Y += xari.LineFormat.Width / 2;
                }
                lineFormatRenderer.DrawLine(points[0], points[1]);
            }

            // Draw axis title.
            var atri = xari.AxisTitleRendererInfo;
            if (atri != null && atri.AxisTitleText != null && atri.AxisTitleText.Length > 0)
            {
                XRect rect = new XRect(xari.X, xari.Y + xari.Height / 2, atri.AxisTitleSize.Width, 0);
                gfx.DrawString(atri.AxisTitleText, atri.AxisTitleFont, atri.AxisTitleBrush, rect);
            }
        }

        /// <summary>
        /// Calculates the X axis describing values like minimum/maximum scale, major/minor tick and
        /// major/minor tick mark width.
        /// </summary>
        void CalculateXAxisValues(AxisRendererInfo rendererInfo)
        {
            // Calculates the maximum number of data points over all series.
            var seriesCollection = ((Chart?)rendererInfo.Axis?.Parent)?._seriesCollection;
            int count = 0;
            if (seriesCollection != null)
            {
                foreach (Series series in seriesCollection)
                    count = Math.Max(count, series.Count);
            }

            rendererInfo.MinimumScale = 0;
            rendererInfo.MaximumScale = count; // At least 0
            rendererInfo.MajorTick = 1;
            rendererInfo.MinorTick = 0.5;
            rendererInfo.MajorTickMarkWidth = DefaultMajorTickMarkWidth;
            rendererInfo.MinorTickMarkWidth = DefaultMinorTickMarkWidth;
        }

        /// <summary>
        /// Initializes the rendererInfo’s xvalues. If not set by the user xvalues will be simply numbers
        /// from minimum scale + 1 to maximum scale.
        /// </summary>
        void InitXValues(AxisRendererInfo rendererInfo)
        {
            rendererInfo.XValues = ((Chart?)rendererInfo.Axis?.Parent)?._xValues ?? NRT.ThrowOnNull<XValues>();
            if (rendererInfo.XValues == null!)
            {
                rendererInfo.XValues = new XValues();
                XSeries xs = rendererInfo.XValues.AddXSeries();
                for (double idx = rendererInfo.MinimumScale + 1; idx <= rendererInfo.MaximumScale; idx++)
                    xs.Add(idx.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Calculates the starting and ending y position for the minor and major tick marks.
        /// </summary>
        void GetTickMarkPos(AxisRendererInfo rendererInfo,
                            ref double majorTickMarkStart, ref double majorTickMarkEnd,
                            ref double minorTickMarkStart, ref double minorTickMarkEnd)
        {
            double majorTickMarkWidth = rendererInfo.MajorTickMarkWidth;
            double minorTickMarkWidth = rendererInfo.MinorTickMarkWidth;
            double x = rendererInfo.Rect.X + rendererInfo.Rect.Width;

            switch (rendererInfo.MajorTickMark)
            {
                case TickMarkType.Inside:
                    majorTickMarkStart = x;
                    majorTickMarkEnd = x + majorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    majorTickMarkStart = x - majorTickMarkWidth;
                    majorTickMarkEnd = x;
                    break;

                case TickMarkType.Cross:
                    majorTickMarkStart = x - majorTickMarkWidth;
                    majorTickMarkEnd = x + majorTickMarkWidth;
                    break;

                case TickMarkType.None:
                    majorTickMarkStart = 0;
                    majorTickMarkEnd = 0;
                    break;
            }

            switch (rendererInfo.MinorTickMark)
            {
                case TickMarkType.Inside:
                    minorTickMarkStart = x;
                    minorTickMarkEnd = x + minorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    minorTickMarkStart = x - minorTickMarkWidth;
                    minorTickMarkEnd = x;
                    break;

                case TickMarkType.Cross:
                    minorTickMarkStart = x - minorTickMarkWidth;
                    minorTickMarkEnd = x + minorTickMarkWidth;
                    break;

                case TickMarkType.None:
                    minorTickMarkStart = 0;
                    minorTickMarkEnd = 0;
                    break;
            }
        }
    }
}
