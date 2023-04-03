// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents a Y axis renderer used for charts of type Bar2D.
    /// </summary>
    class HorizontalYAxisRenderer : YAxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the HorizontalYAxisRenderer class with the
        /// specified renderer parameters.
        /// </summary>
        internal HorizontalYAxisRenderer(RendererParameters parms) : base(parms)
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
                Axis = chart._yAxis
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

                XSize size = new XSize(0, 0);

                // Height of all tick labels.
                double yMin = yari.MinimumScale;
                double yMax = yari.MaximumScale;
                double yMajorTick = yari.MajorTick;
                double lineHeight = Double.MinValue;
                XSize labelSize = new XSize(0, 0);
                for (double y = yMin; y <= yMax; y += yMajorTick)
                {
                    string str = y.ToString(yari.TickLabelsFormat);
                    labelSize = gfx.MeasureString(str, yari.TickLabelsFont);
                    size.Width += labelSize.Width;
                    size.Height = Math.Max(labelSize.Height, size.Height);
                    lineHeight = Math.Max(lineHeight, labelSize.Width);
                }

                // Add space for tick marks.
                size.Height += yari.MajorTickMarkWidth * 1.5;

                // Measure axis title.
                XSize titleSize = new XSize(0, 0);
                if (yari.AxisTitleRendererInfo != null!)
                {
                    var parms = new RendererParameters();
                    parms.Graphics = gfx;
                    parms.RendererInfo = yari;
                    var atr = new AxisTitleRenderer(parms);
                    atr.Format();
                    titleSize.Height = yari.AxisTitleRendererInfo.Height;
                    titleSize.Width = yari.AxisTitleRendererInfo.Width;
                }

                yari.Height = size.Height + titleSize.Height;
                yari.Width = Math.Max(size.Width, titleSize.Width);

                yari.InnerRect = yari.Rect;
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
            matrix.TranslatePrepend(-yMin, -yari.Y);
            matrix.Scale(yari.InnerRect.Width / (yMax - yMin), 1, XMatrixOrder.Append);
            matrix.Translate(yari.X, yari.Y, XMatrixOrder.Append);

            // Draw axis.
            // First draw tick marks, second draw axis.
            double majorTickMarkStart = 0, majorTickMarkEnd = 0,
                   minorTickMarkStart = 0, minorTickMarkEnd = 0;
            GetTickMarkPos(yari, ref majorTickMarkStart, ref majorTickMarkEnd, ref minorTickMarkStart, ref minorTickMarkEnd);

            var gfx = _rendererParms.Graphics;
            var lineFormatRenderer = new LineFormatRenderer(gfx, yari.LineFormat);
            XPoint[] points = new XPoint[2];
            if (yari.MinorTickMark != TickMarkType.None)
            {
                for (double y = yMin + yMinorTick; y < yMax; y += yMinorTick)
                {
                    points[0].X = y;
                    points[0].Y = minorTickMarkStart;
                    points[1].X = y;
                    points[1].Y = minorTickMarkEnd;
                    matrix.TransformPoints(points);
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }
            }

            var xsf = new XStringFormat();
            xsf.LineAlignment = XLineAlignment.Near;
            int countTickLabels = (int)((yMax - yMin) / yMajorTick) + 1;
            for (int i = 0; i < countTickLabels; ++i)
            {
                double y = yMin + yMajorTick * i;
                string str = y.ToString(yari.TickLabelsFormat);

                XSize labelSize = gfx.MeasureString(str, yari.TickLabelsFont);
                if (yari.MajorTickMark != TickMarkType.None)
                {
                    labelSize.Height += 1.5f * yari.MajorTickMarkWidth;
                    points[0].X = y;
                    points[0].Y = majorTickMarkStart;
                    points[1].X = y;
                    points[1].Y = majorTickMarkEnd;
                    matrix.TransformPoints(points);
                    lineFormatRenderer.DrawLine(points[0], points[1]);
                }

                XPoint[] layoutText = new XPoint[1];
                layoutText[0].X = y;
                layoutText[0].Y = yari.Y + 1.5 * yari.MajorTickMarkWidth;
                matrix.TransformPoints(layoutText);
                layoutText[0].X -= labelSize.Width / 2; // Center text vertically.
                gfx.DrawString(str, yari.TickLabelsFont, yari.TickLabelsBrush, layoutText[0], xsf);
            }

            if (yari.LineFormat != null)
            {
                points[0].X = yMin;
                points[0].Y = yari.Y;
                points[1].X = yMax;
                points[1].Y = yari.Y;
                matrix.TransformPoints(points);
                if (yari.MajorTickMark != TickMarkType.None)
                {
                    // yMax is at the upper side of the axis
                    points[0].X -= yari.LineFormat.Width / 2;
                    points[1].X += yari.LineFormat.Width / 2;
                }
                lineFormatRenderer.DrawLine(points[0], points[1]);
            }

            // Draw axis title
            if (yari.AxisTitleRendererInfo != null)
            {
                RendererParameters parms = new RendererParameters();
                parms.Graphics = gfx;
                parms.RendererInfo = yari;
                XRect rcTitle = yari.Rect;
                rcTitle.Height = yari.AxisTitleRendererInfo.Height;
                rcTitle.Y += yari.Rect.Height - rcTitle.Height;
                yari.AxisTitleRendererInfo.Rect = rcTitle;
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
            double yMin, yMax;
            CalcYAxis(out yMin, out yMax);
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
            double y = rendererInfo.Rect.Y;

            switch (rendererInfo.MajorTickMark)
            {
                case TickMarkType.Inside:
                    majorTickMarkStart = y - majorTickMarkWidth;
                    majorTickMarkEnd = y;
                    break;

                case TickMarkType.Outside:
                    majorTickMarkStart = y;
                    majorTickMarkEnd = y + majorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    majorTickMarkStart = y - majorTickMarkWidth;
                    majorTickMarkEnd = y + majorTickMarkWidth;
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
                    minorTickMarkStart = y - minorTickMarkWidth;
                    minorTickMarkEnd = y;
                    break;

                case TickMarkType.Outside:
                    minorTickMarkStart = y;
                    minorTickMarkEnd = y + minorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    minorTickMarkStart = y - minorTickMarkWidth;
                    minorTickMarkEnd = y + minorTickMarkWidth;
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
