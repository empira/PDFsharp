// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a Y axis renderer used for charts of type Column2D or Line.
    /// </summary>
    class VerticalYAxisRenderer : YAxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the VerticalYAxisRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal VerticalYAxisRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Returns a initialized rendererInfo based on the Y axis.
        /// </summary>
        internal override RendererInfo Init()
        {
            var chart = (Chart)_rendererParms.DrawingItem;
            var gfx = _rendererParms.Graphics;

            var yari = new AxisRendererInfo
            {
                Axis = chart._yAxis ?? NRT.ThrowOnNull<Axis>()
            };
            InitScale(yari);
            if (yari.Axis != null)
            {
                ChartRendererInfo cri = (ChartRendererInfo)_rendererParms.RendererInfo;
                InitTickLabels(yari, cri.DefaultFont);
                InitAxisTitle(yari, cri.DefaultFont);
                InitAxisLineFormat(yari);
                InitGridlines(yari);
            }
            return yari;
        }

        /// <summary>
        /// Calculates the space used for the Y axis.
        /// </summary>
        internal override void Format()
        {
            var yari = ((ChartRendererInfo)_rendererParms.RendererInfo).YAxisRendererInfo;
            if (yari?.Axis != null)
            {
                var gfx = _rendererParms.Graphics;

                var size = new XSize(0, 0);

                // height of all tick labels
                double yMin = yari.MinimumScale;
                double yMax = yari.MaximumScale;
                double yMajorTick = yari.MajorTick;
                double lineHeight = Double.MinValue;
                var labelSize = new XSize(0, 0);
                for (double y = yMin; y <= yMax; y += yMajorTick)
                {
                    string str = y.ToString(yari.TickLabelsFormat);
                    labelSize = gfx.MeasureString(str, yari.TickLabelsFont);
                    size.Height += labelSize.Height;
                    size.Width = Math.Max(size.Width, labelSize.Width);
                    lineHeight = Math.Max(lineHeight, labelSize.Height);
                }

                // add space for tickmarks
                size.Width += yari.MajorTickMarkWidth * 1.5;

                // Measure axis title
                XSize titleSize = new XSize(0, 0);
                if (yari.AxisTitleRendererInfo != null)
                {
                    RendererParameters parms = new RendererParameters();
                    parms.Graphics = gfx;
                    parms.RendererInfo = yari;
                    AxisTitleRenderer atr = new AxisTitleRenderer(parms);
                    atr.Format();
                    titleSize.Height = yari.AxisTitleRendererInfo.Height;
                    titleSize.Width = yari.AxisTitleRendererInfo.Width;
                }

                yari.Height = Math.Max(size.Height, titleSize.Height);
                yari.Width = size.Width + titleSize.Width;

                yari.InnerRect = yari.Rect;
                yari.InnerRect.Y += yari.TickLabelsFont.Height / 2;
                yari.LabelSize = labelSize;
            }
        }

        /// <summary>
        /// Draws the vertical Y axis.
        /// </summary>
        internal override void Draw()
        {
            var yari = ((ChartRendererInfo)_rendererParms.RendererInfo).YAxisRendererInfo;
            if (yari == null) 
                return;

            double yMin = yari.MinimumScale;
            double yMax = yari.MaximumScale;
            double yMajorTick = yari.MajorTick;
            double yMinorTick = yari.MinorTick;

            XMatrix matrix = new XMatrix();
            matrix.TranslatePrepend(-yari.InnerRect.X, yMax);
            matrix.Scale(1, yari.InnerRect.Height / (yMax - yMin), XMatrixOrder.Append);
            matrix.ScalePrepend(1, -1); // mirror horizontal
            matrix.Translate(yari.InnerRect.X, yari.InnerRect.Y, XMatrixOrder.Append);

            // Draw axis.
            // First draw tick marks, second draw axis.
            double majorTickMarkStart = 0, majorTickMarkEnd = 0,
                   minorTickMarkStart = 0, minorTickMarkEnd = 0;
            GetTickMarkPos(yari, ref majorTickMarkStart, ref majorTickMarkEnd, ref minorTickMarkStart, ref minorTickMarkEnd);

            var gfx = _rendererParms.Graphics;
            var lineFormatRenderer = new LineFormatRenderer(gfx, yari.LineFormat);
            var minorTickMarkLineFormat = new LineFormatRenderer(gfx, yari.MinorTickMarkLineFormat);
            var majorTickMarkLineFormat = new LineFormatRenderer(gfx, yari.MajorTickMarkLineFormat);
            var points = new XPoint[2];

            // Draw minor tick marks.
            if (yari.MinorTickMark != TickMarkType.None)
            {
                for (double y = yMin + yMinorTick; y < yMax; y += yMinorTick)
                {
                    points[0].X = minorTickMarkStart;
                    points[0].Y = y;
                    points[1].X = minorTickMarkEnd;
                    points[1].Y = y;
                    matrix.TransformPoints(points);
                    minorTickMarkLineFormat.DrawLine(points[0], points[1]);
                }
            }

            double lineSpace = yari.TickLabelsFont.GetHeight(); // old: yari.TickLabelsFont.GetHeight(gfx);
            int cellSpace = yari.TickLabelsFont.FontFamily.GetLineSpacing(yari.TickLabelsFont.Style);
            double xHeight = yari.TickLabelsFont.Metrics.XHeight;

            XSize labelSize = new XSize(0, 0);
            labelSize.Height = lineSpace * xHeight / cellSpace;

            int countTickLabels = (int)((yMax - yMin) / yMajorTick) + 1;
            for (int i = 0; i < countTickLabels; ++i)
            {
                double y = yMin + yMajorTick * i;
                string str = y.ToString(yari.TickLabelsFormat);

                labelSize.Width = gfx.MeasureString(str, yari.TickLabelsFont).Width;

                // Draw major tick marks.
                if (yari.MajorTickMark != TickMarkType.None)
                {
                    labelSize.Width += yari.MajorTickMarkWidth * 1.5;
                    points[0].X = majorTickMarkStart;
                    points[0].Y = y;
                    points[1].X = majorTickMarkEnd;
                    points[1].Y = y;
                    matrix.TransformPoints(points);
                    majorTickMarkLineFormat.DrawLine(points[0], points[1]);
                }
                else
                    labelSize.Width += SpaceBetweenLabelAndTickmark;

                // Draw label text.
                var layoutText = new XPoint[1];
                layoutText[0].X = yari.InnerRect.X + yari.InnerRect.Width - labelSize.Width;
                layoutText[0].Y = y;
                matrix.TransformPoints(layoutText);
                layoutText[0].Y += labelSize.Height / 2; // Center text vertically.
                gfx.DrawString(str, yari.TickLabelsFont, yari.TickLabelsBrush, layoutText[0]);
            }

            // Draw axis.
            if (yari.LineFormat != null && yari.LineFormat.Width > 0)
            {
                points[0].X = yari.InnerRect.X + yari.InnerRect.Width;
                points[0].Y = yMin;
                points[1].X = yari.InnerRect.X + yari.InnerRect.Width;
                points[1].Y = yMax;
                matrix.TransformPoints(points);
                if (yari.MajorTickMark != TickMarkType.None)
                {
                    // yMax is at the upper side of the axis
                    points[1].Y -= yari.LineFormat.Width / 2;
                    points[0].Y += yari.LineFormat.Width / 2;
                }
                lineFormatRenderer.DrawLine(points[0], points[1]);
            }

            // Draw axis title
            if (yari.AxisTitleRendererInfo != null! && yari.AxisTitleRendererInfo.AxisTitleText != "")
            {
                var parms = new RendererParameters
                {
                    Graphics = gfx,
                    RendererInfo = yari
                };
                double width = yari.AxisTitleRendererInfo.Width;
                yari.AxisTitleRendererInfo.Rect = yari.InnerRect;
                yari.AxisTitleRendererInfo.Width = width;
                AxisTitleRenderer atr = new AxisTitleRenderer(parms);
                atr.Draw();
            }
        }

        /// <summary>
        /// Calculates all values necessary for scaling the axis like minimum/maximum scale or
        /// minor/major tick.
        /// </summary>
        void InitScale(AxisRendererInfo rendererInfo)
        {
            CalcYAxis(out var yMin, out var yMax);
            FineTuneYAxis(rendererInfo, yMin, yMax);

            rendererInfo.MajorTickMarkWidth = DefaultMajorTickMarkWidth;
            rendererInfo.MinorTickMarkWidth = DefaultMinorTickMarkWidth;
        }

        /// <summary>
        /// Gets the top and bottom position of the major and minor tick marks depending on the
        /// tick mark type.
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
                    majorTickMarkStart = rect.X + rect.Width;
                    majorTickMarkEnd = rect.X + rect.Width + majorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    majorTickMarkStart = rect.X + rect.Width;
                    majorTickMarkEnd = rect.X + rect.Width - majorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    majorTickMarkStart = rect.X + rect.Width - majorTickMarkWidth;
                    majorTickMarkEnd = rect.X + rect.Width + majorTickMarkWidth;
                    break;

                //TickMarkType.None:
                default:
                    majorTickMarkStart = 0;
                    majorTickMarkEnd = 0;
                    break;
            }

            switch (rendererInfo.MinorTickMark)
            {
                case TickMarkType.Inside:
                    minorTickMarkStart = rect.X + rect.Width;
                    minorTickMarkEnd = rect.X + rect.Width + minorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    minorTickMarkStart = rect.X + rect.Width;
                    minorTickMarkEnd = rect.X + rect.Width - minorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    minorTickMarkStart = rect.X + rect.Width - minorTickMarkWidth;
                    minorTickMarkEnd = rect.X + rect.Width + minorTickMarkWidth;
                    break;

                //TickMarkType.None:
                default:
                    minorTickMarkStart = 0;
                    minorTickMarkEnd = 0;
                    break;
            }
        }

        /// <summary>
        /// Determines the smallest and the largest number from all series of the chart.
        /// </summary>
        protected virtual void CalcYAxis(out double yMin, out double yMax)
        {
            yMin = Double.MaxValue;
            yMax = Double.MinValue;

            foreach (Series series in ((Chart)_rendererParms.DrawingItem).SeriesCollection)
            {
                foreach (Point point in series.Elements)
                {
                    if (!Double.IsNaN(point.Value))
                    {
                        yMin = Math.Min(yMin, point.Value);
                        yMax = Math.Max(yMax, point.Value);
                    }
                }
            }
        }
    }
}
