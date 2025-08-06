// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents an axis renderer used for charts of type Column2D or Line.
    /// </summary>
    class HorizontalXAxisRenderer : XAxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the HorizontalXAxisRenderer class with the specified renderer parameters.
        /// </summary>
        internal HorizontalXAxisRenderer(RendererParameters parms) : base(parms)
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
                InitTickLabels(xari, cri.DefaultFont);
                InitXValues(xari);
                InitAxisTitle(xari, cri.DefaultFont);
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

                //if (atri is { AxisTitleText: { Length: > 0 } }) better readyble?
                if (atri != null && atri.AxisTitleText != null && atri.AxisTitleText.Length > 0)
                {
                    titleSize = _rendererParms.Graphics.MeasureString(atri.AxisTitleText, atri.AxisTitleFont);
                    atri.AxisTitleSize = titleSize;
                }

                // Calculate space used for tick labels.
                var size = new XSize(0, 0);
                if (xari.XValues?.Count > 0)
                {
                    XSeries xs = xari.XValues[0];
                    foreach (XValue xv in xs)
                    {
                        if (xv != null)
                        {
                            string tickLabel = xv.ValueField;
                            XSize valueSize = _rendererParms.Graphics.MeasureString(tickLabel, xari.TickLabelsFont);
                            size.Height = Math.Max(valueSize.Height, size.Height);
                            size.Width += valueSize.Width;
                        }
                    }
                }

                // Remember space for later drawing.
                xari.TickLabelsHeight = size.Height;
                xari.Height = titleSize.Height + size.Height + xari.MajorTickMarkWidth;
                xari.Width = Math.Max(titleSize.Width, size.Width);
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
            double tickLabelStep = xari.Width;
            if (countTickLabels != 0)
                tickLabelStep = xari.Width / countTickLabels;

            //XPoint startPos = new XPoint(xari.X + tickLabelStep / 2, xari.Y + /*xari.TickLabelsHeight +*/ xari.MajorTickMarkWidth);
            XPoint startPos = new XPoint(xari.X + tickLabelStep / 2, xari.Y + xari.TickLabelsHeight);
            if (xari.MajorTickMark != TickMarkType.None)
                startPos.Y += xari.MajorTickMarkWidth;
            foreach (var xs in (xari.XValues ?? throw new InvalidOperationException()).Cast<XSeries>()) // BUG_OLD???
            {
                for (int idx = 0; idx < countTickLabels && idx < xs.Count; idx++)
                {
                    var xv = xs[idx];
                    if (xv != null!)
                    {
                        string tickLabel = xv.ValueField;
                        XSize size = gfx.MeasureString(tickLabel, xari.TickLabelsFont);
                        gfx.DrawString(tickLabel, xari.TickLabelsFont, xari.TickLabelsBrush, startPos.X - size.Width / 2, startPos.Y);
                    }
                    startPos.X += tickLabelStep;
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
                double minorTickMarkStep = xari.Width / countMinorTickMarks;
                startPos.X = xari.X;
                for (int x = 0; x <= countMinorTickMarks; x++)
                {
                    points[0].X = startPos.X + minorTickMarkStep * x;
                    points[0].Y = minorTickMarkStart;
                    points[1].X = points[0].X;
                    points[1].Y = minorTickMarkEnd;
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            // Major ticks.
            if (xari.MajorTickMark != TickMarkType.None)
            {
                int countMajorTickMarks = (int)(xMax / xMajorTick);
                double majorTickMarkStep = xari.Width;
                if (countMajorTickMarks != 0)
                    majorTickMarkStep = xari.Width / countMajorTickMarks;
                startPos.X = xari.X;
                for (int x = 0; x <= countMajorTickMarks; x++)
                {
                    points[0].X = startPos.X + majorTickMarkStep * x;
                    points[0].Y = majorTickMarkStart;
                    points[1].X = points[0].X;
                    points[1].Y = majorTickMarkEnd;
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            // Axis.
            if (xari.LineFormat != null)
            {
                points[0].X = xari.X;
                points[0].Y = xari.Y;
                points[1].X = xari.X + xari.Width;
                points[1].Y = xari.Y;
                if (xari.MajorTickMark != TickMarkType.None)
                {
                    points[0].X -= xari.LineFormat.Width / 2;
                    points[1].X += xari.LineFormat.Width / 2;
                }
                lineFormatRenderer.DrawLine(points[0], points[1]);
            }

            // Draw axis title.
            var atri = xari.AxisTitleRendererInfo;
            if (atri != null && atri.AxisTitleText != null && atri.AxisTitleText.Length > 0)
            {
                XRect rect = new XRect(xari.Rect.Right / 2 - atri.AxisTitleSize.Width / 2, xari.Rect.Bottom,
                                       atri.AxisTitleSize.Width, 0);
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
            var seriesCollection = ((Chart?)rendererInfo.Axis?.Parent)?._seriesCollection ?? NRT.ThrowOnNull<SeriesCollection>();
            int count = 0;
            foreach (Series series in seriesCollection)
                count = Math.Max(count, series.Count);

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
            rendererInfo.XValues = ((Chart?)rendererInfo.Axis?.Parent)?._xValues;
            if (rendererInfo.XValues == null)
            {
                rendererInfo.XValues = new XValues();
                XSeries xs = rendererInfo.XValues.AddXSeries();
                for (double idx = rendererInfo.MinimumScale + 1; idx <= rendererInfo.MaximumScale; idx++)
                    xs.Add(idx.ToString(rendererInfo.TickLabelsFormat));
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
            XRect rect = rendererInfo.Rect;

            switch (rendererInfo.MajorTickMark)
            {
                case TickMarkType.Inside:
                    majorTickMarkStart = rect.Y;
                    majorTickMarkEnd = rect.Y - majorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    majorTickMarkStart = rect.Y;
                    majorTickMarkEnd = rect.Y + majorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    majorTickMarkStart = rect.Y + majorTickMarkWidth;
                    majorTickMarkEnd = rect.Y - majorTickMarkWidth;
                    break;

                case TickMarkType.None:
                    majorTickMarkStart = 0;
                    majorTickMarkEnd = 0;
                    break;
            }

            switch (rendererInfo.MinorTickMark)
            {
                case TickMarkType.Inside:
                    minorTickMarkStart = rect.Y;
                    minorTickMarkEnd = rect.Y - minorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    minorTickMarkStart = rect.Y;
                    minorTickMarkEnd = rect.Y + minorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    minorTickMarkStart = rect.Y + minorTickMarkWidth;
                    minorTickMarkEnd = rect.Y - minorTickMarkWidth;
                    break;

                case TickMarkType.None:
                    minorTickMarkStart = 0;
                    minorTickMarkEnd = 0;
                    break;
            }
        }
    }
}
