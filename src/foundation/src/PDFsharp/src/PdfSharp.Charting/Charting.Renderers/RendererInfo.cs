// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the base class of all renderer infos.
    /// Renderer infos are used to hold all necessary information and time consuming calculations
    /// between rendering cycles.
    /// </summary>
    abstract class RendererInfo
    { }

    /// <summary>
    /// Base class for all renderer infos which defines an area.
    /// </summary>
    abstract class AreaRendererInfo : RendererInfo
    {
        /// <summary>
        /// Gets or sets the x coordinate of this rectangle.
        /// </summary>
        public virtual double X
        {
            get => _rect.X;
            set => _rect.X = value;
        }

        /// <summary>
        /// Gets or sets the y coordinate of this rectangle.
        /// </summary>
        public virtual double Y
        {
            get => _rect.Y;
            set => _rect.Y = value;
        }

        /// <summary>
        /// Gets or sets the width of this rectangle.
        /// </summary>
        public virtual double Width
        {
            get => _rect.Width;
            set => _rect.Width = value;
        }

        /// <summary>
        /// Gets or sets the height of this rectangle.
        /// </summary>
        public virtual double Height
        {
            get => _rect.Height;
            set => _rect.Height = value;
        }

        /// <summary>
        /// Gets the area's size.
        /// </summary>
        public XSize Size
        {
            get => _rect.Size;
            set => _rect.Size = value;
        }

        /// <summary>
        /// Gets the area's rectangle.
        /// </summary>
        public XRect Rect
        {
            get => _rect;
            set => _rect = value;
        }
        XRect _rect;
    }

    /// <summary>
    /// A ChartRendererInfo stores information of all main parts of a chart like axis renderer info or
    /// plot area renderer info.
    /// </summary>
    class ChartRendererInfo : AreaRendererInfo
    {
        public Chart Chart = null!;

        public AxisRendererInfo? XAxisRendererInfo = null!;

        public AxisRendererInfo? YAxisRendererInfo = null!;

        //public AxisRendererInfo zAxisRendererInfo; // not yet used

        public PlotAreaRendererInfo PlotAreaRendererInfo = null!;

        public LegendRendererInfo? LegendRendererInfo = null!;

        public SeriesRendererInfo[] SeriesRendererInfos = null!;

        /// <summary>
        /// Gets the chart's default font for rendering.
        /// </summary>
        public XFont DefaultFont
            => _defaultFont ??= Converter.ToXFont(Chart._font, new XFont("Arial", 12, XFontStyleEx.Regular));

        XFont? _defaultFont;

        /// <summary>
        /// Gets the chart's default font for rendering data labels.
        /// </summary>
        public XFont DefaultDataLabelFont
            => _defaultDataLabelFont ??= Converter.ToXFont(Chart._font, new XFont("Arial", 10, XFontStyleEx.Regular));

        XFont? _defaultDataLabelFont;
    }

    /// <summary>
    /// A CombinationRendererInfo stores information for rendering combination of charts.
    /// </summary>
    class CombinationRendererInfo : ChartRendererInfo
    {
        public SeriesRendererInfo[] CommonSeriesRendererInfos = null!;

        public SeriesRendererInfo[]? AreaSeriesRendererInfos = null!;

        public SeriesRendererInfo[]? ColumnSeriesRendererInfos = null!;

        public SeriesRendererInfo[]? LineSeriesRendererInfos = null!;
    }

    /// <summary>
    /// PointRendererInfo is used to render one single data point which is part of a data series.
    /// </summary>
    class PointRendererInfo : RendererInfo
    {
        public Point Point = null!;

        public XPen LineFormat = null!;

        public XBrush FillFormat = null!;

        public void Check()
        {
            if (Point == null || LineFormat == null || FillFormat == null)
                throw new InvalidOperationException(PSCSR.RenderInfoNotInitialized(typeof(PointRendererInfo)));
        }
    }

    /// <summary>
    /// Represents one sector of a series used by a pie chart.
    /// </summary>
    class SectorRendererInfo : PointRendererInfo
    {
        public XRect Rect;

        public double StartAngle;

        public double SweepAngle;

        public new void Check() => base.Check();
    }

    /// <summary>
    /// Represents one data point of a series and the corresponding rectangle.
    /// </summary>
    class ColumnRendererInfo : PointRendererInfo
    {
        public XRect Rect;
    }

    /// <summary>
    /// Stores rendering specific information for one data label entry.
    /// </summary>
    class DataLabelEntryRendererInfo : AreaRendererInfo
    {
        public string Text = null!;
    }

    /// <summary>
    /// Stores data label specific rendering information.
    /// </summary>
    class DataLabelRendererInfo : RendererInfo
    {
        public DataLabelEntryRendererInfo[] Entries = null!;

        public string Format = null!;

        public XFont Font = null!;

        public XBrush FontColor = null!;

        public DataLabelPosition Position;

        public DataLabelType Type;
    }

    /// <summary>
    /// SeriesRendererInfo holds all data series specific rendering information.
    /// </summary>
    class SeriesRendererInfo : RendererInfo
    {
        public Series Series = null!;

        public DataLabelRendererInfo? DataLabelRendererInfo = null;

        public PointRendererInfo[] PointRendererInfos = null!;

        public XPen LineFormat = null!;

        public XBrush FillFormat = null!;

        // Used if ChartType is set to Line
        public MarkerRendererInfo MarkerRendererInfo = null!;

        /// <summary>
        /// Gets the sum of all points in PointRendererInfo.
        /// </summary>
        public double SumOfPoints
        {
            get
            {
                double sum = 0;
                foreach (var pri in PointRendererInfos)
                {
                    if (pri.Point != null && !Double.IsNaN(pri.Point.Value))
                        sum += Math.Abs(pri.Point.Value);
                }
                return sum;
            }
        }
    }

    /// <summary>
    /// Represents a description of a marker for a line chart.
    /// </summary>
    class MarkerRendererInfo : RendererInfo
    {
        public XUnit MarkerSize;

        public MarkerStyle MarkerStyle;

        public XColor MarkerForegroundColor;

        public XColor MarkerBackgroundColor;
    }

    /// <summary>
    /// An AxisRendererInfo holds all axis specific rendering information.
    /// </summary>
    class AxisRendererInfo : AreaRendererInfo
    {
        public Axis? Axis = default!;  // StL: Can be null

        public double MinimumScale;

        public double MaximumScale;

        public double MajorTick;

        public double MinorTick;

        public TickMarkType MinorTickMark;

        public TickMarkType MajorTickMark;

        public double MajorTickMarkWidth;

        public double MinorTickMarkWidth;

        public XPen MajorTickMarkLineFormat = null!;

        public XPen MinorTickMarkLineFormat = null!;

        //Gridlines
        public XPen? MajorGridlinesLineFormat;

        public XPen? MinorGridlinesLineFormat;
        //AxisTitle
        public AxisTitleRendererInfo AxisTitleRendererInfo = null!;

        //TickLabels
        public string? TickLabelsFormat = null!;

        public XFont TickLabelsFont = null!;

        public XBrush TickLabelsBrush = null!;

        public double TickLabelsHeight;

        //LineFormat
        internal XPen? LineFormat = null;

        //Chart.XValues, used for X axis only.
        public XValues? XValues = null!;

        /// <summary>
        /// Sets the x coordinate of the inner rectangle.
        /// </summary>
        public override double X
        {
            set
            {
                base.X = value;
                InnerRect.X = value;
            }
        }

        /// <summary>
        /// Sets the y coordinate of the inner rectangle.
        /// </summary>
        public override double Y
        {
            set
            {
                base.Y = value;
                InnerRect.Y = value + LabelSize.Height / 2;
            }
        }

        /// <summary>
        /// Sets the height of the inner rectangle.
        /// </summary>
        public override double Height
        {
            set
            {
                base.Height = value;
                InnerRect.Height = value - (InnerRect.Y - Y);
            }
        }

        /// <summary>
        /// Sets the width of the inner rectangle.
        /// </summary>
        public override double Width
        {
            set
            {
                base.Width = value;
                InnerRect.Width = value - LabelSize.Width / 2;
            }
        }

        public XRect InnerRect;

        public XSize LabelSize;
    }

    class AxisTitleRendererInfo : AreaRendererInfo
    {
        public AxisTitle AxisTitle = null!;

        public string AxisTitleText = null!;

        public XFont AxisTitleFont = null!;

        public XBrush AxisTitleBrush = null!;

        public double AxisTitleOrientation;

        public HorizontalAlignment AxisTitleAlignment;

        public VerticalAlignment AxisTitleVerticalAlignment;

        public XSize AxisTitleSize;
    }

    /// <summary>
    /// Represents one description of a legend entry.
    /// </summary>
    class LegendEntryRendererInfo : AreaRendererInfo
    {
        public SeriesRendererInfo SeriesRendererInfo = null!;

        public LegendRendererInfo LegendRendererInfo = null!;

        public string EntryText = null!;

        /// <summary>
        /// Size for the marker only.
        /// </summary>
        public XSize MarkerSize;

        public XPen MarkerPen = null!;

        public XBrush MarkerBrush = null!;

        /// <summary>
        /// Width for marker area. Extra spacing for line charts are considered.
        /// </summary>
        public XSize MarkerArea;

        /// <summary>
        /// Size for text area.
        /// </summary>
        public XSize TextSize;
    }

    /// <summary>
    /// Stores legend specific rendering information.
    /// </summary>
    class LegendRendererInfo : AreaRendererInfo
    {
        public Legend Legend = null!;

        public XFont Font = null!;

        public XBrush FontColor = null!;

        public XPen BorderPen = null!;

        public LegendEntryRendererInfo[] Entries = null!;
    }

    /// <summary>
    /// Stores rendering information common to all plot area renderers.
    /// </summary>
    class PlotAreaRendererInfo : AreaRendererInfo
    {
        public PlotArea PlotArea = null!;

        /// <summary>
        /// Saves the plot area's matrix.
        /// </summary>
        public XMatrix Matrix;

        public XPen LineFormat = null!;

        public XBrush FillFormat = null!;
    }
}
