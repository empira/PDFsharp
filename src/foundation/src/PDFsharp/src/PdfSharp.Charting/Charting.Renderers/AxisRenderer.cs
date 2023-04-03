// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the base for all specialized axis renderer. Initialization common to all
    /// axis renderers should come here.
    /// </summary>
    abstract class AxisRenderer : Renderer
    {
        /// <summary>
        /// Initializes a new instance of the AxisRenderer class with the specified renderer parameters.
        /// </summary>
        internal AxisRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Initializes the axis title of the rendererInfo. All missing font attributes will be taken
        /// from the specified defaultFont.
        /// </summary>
        protected void InitAxisTitle(AxisRendererInfo rendererInfo, XFont defaultFont)
        {
            if (rendererInfo.Axis?._title != null)
            {
                var atri = new AxisTitleRendererInfo();
                rendererInfo.AxisTitleRendererInfo = atri;

                atri.AxisTitle = rendererInfo.Axis._title;
                atri.AxisTitleText = rendererInfo.Axis._title.Caption;
                atri.AxisTitleAlignment = rendererInfo.Axis._title.Alignment;
                atri.AxisTitleVerticalAlignment = rendererInfo.Axis._title.VerticalAlignment;
                atri.AxisTitleFont = Converter.ToXFont(rendererInfo.Axis._title.Font, defaultFont);
                var fontColor = XColors.Black;
                if (rendererInfo.Axis._title._font is { Color.IsEmpty: false })
                    fontColor = rendererInfo.Axis._title._font.Color;
                atri.AxisTitleBrush = new XSolidBrush(fontColor);
                atri.AxisTitleOrientation = rendererInfo.Axis._title.Orientation;
            }
        }

        /// <summary>
        /// Initializes the tick labels of the rendererInfo. All missing font attributes will be taken
        /// from the specified defaultFont.
        /// </summary>
        protected void InitTickLabels(AxisRendererInfo rendererInfo, XFont defaultFont)
        {
            if (rendererInfo.Axis?._tickLabels != null)
            {
                rendererInfo.TickLabelsFont = Converter.ToXFont(rendererInfo.Axis._tickLabels._font, defaultFont);
                XColor fontColor = XColors.Black;
                if (rendererInfo.Axis._tickLabels._font is { Color.IsEmpty: false })
                    fontColor = rendererInfo.Axis._tickLabels._font.Color;
                rendererInfo.TickLabelsBrush = new XSolidBrush(fontColor);

                rendererInfo.TickLabelsFormat = rendererInfo.Axis._tickLabels.Format;
                if (rendererInfo.TickLabelsFormat == null)
                    rendererInfo.TickLabelsFormat = GetDefaultTickLabelsFormat();
            }
            else
            {
                rendererInfo.TickLabelsFont = defaultFont;
                rendererInfo.TickLabelsBrush = new XSolidBrush(XColors.Black);
                rendererInfo.TickLabelsFormat = GetDefaultTickLabelsFormat();
            }
        }

        /// <summary>
        /// Initializes the line format of the rendererInfo.
        /// </summary>
        protected void InitAxisLineFormat(AxisRendererInfo rendererInfo)
        {
            if (rendererInfo.Axis?._minorTickMarkInitialized ?? false)
                rendererInfo.MinorTickMark = rendererInfo.Axis.MinorTickMark;

            if (rendererInfo.Axis?._majorTickMarkInitialized ?? false)
                rendererInfo.MajorTickMark = rendererInfo.Axis.MajorTickMark;
            else
                rendererInfo.MajorTickMark = TickMarkType.Outside;

            if (rendererInfo.MinorTickMark != TickMarkType.None)
                rendererInfo.MinorTickMarkLineFormat = Converter.ToXPen(rendererInfo.Axis!._lineFormat, XColors.Black, DefaultMinorTickMarkLineWidth);

            if (rendererInfo.MajorTickMark != TickMarkType.None)
                rendererInfo.MajorTickMarkLineFormat = Converter.ToXPen(rendererInfo.Axis!._lineFormat, XColors.Black, DefaultMajorTickMarkLineWidth);

            if (rendererInfo.Axis?._lineFormat != null)
            {
                rendererInfo.LineFormat = Converter.ToXPen(rendererInfo.Axis.LineFormat, XColors.Black, DefaultLineWidth);
                if (!rendererInfo.Axis._majorTickMarkInitialized)
                    rendererInfo.MajorTickMark = TickMarkType.Outside;
            }
        }

        /// <summary>
        /// Initializes the gridlines of the rendererInfo.
        /// </summary>
        protected void InitGridlines(AxisRendererInfo rendererInfo)
        {
            if (rendererInfo.Axis?._minorGridlines != null)
            {
                rendererInfo.MinorGridlinesLineFormat =
                  Converter.ToXPen(rendererInfo.Axis._minorGridlines._lineFormat, XColors.Black, DefaultGridLineWidth);
            }
            else if (rendererInfo.Axis?.HasMinorGridlines ?? false)
            {
                // No minor gridlines object are given, but user asked for.
                rendererInfo.MinorGridlinesLineFormat = new XPen(XColors.Black, DefaultGridLineWidth);
            }

            if (rendererInfo.Axis?._majorGridlines != null)
            {
                rendererInfo.MajorGridlinesLineFormat =
                  Converter.ToXPen(rendererInfo.Axis._majorGridlines._lineFormat, XColors.Black, DefaultGridLineWidth);
            }
            else if (rendererInfo.Axis?.HasMajorGridlines ?? false)
            {
                // No major gridlines object are given, but user asked for.
                rendererInfo.MajorGridlinesLineFormat = new XPen(XColors.Black, DefaultGridLineWidth);
            }
        }

        /// <summary>
        /// Default width for a variety of lines.
        /// </summary>
        protected const double DefaultLineWidth = 0.4; // 0.15 mm

        /// <summary>
        /// Default width for gridlines.
        /// </summary>
        protected const double DefaultGridLineWidth = 0.15;

        /// <summary>
        /// Default width for major tick marks.
        /// </summary>
        protected const double DefaultMajorTickMarkLineWidth = 1;

        /// <summary>
        /// Default width for minor tick marks.
        /// </summary>
        protected const double DefaultMinorTickMarkLineWidth = 1;

        /// <summary>
        /// Default width of major tick marks.
        /// </summary>
        protected const double DefaultMajorTickMarkWidth = 4.3; // 1.5 mm

        /// <summary>
        /// Default width of minor tick marks.
        /// </summary>
        protected const double DefaultMinorTickMarkWidth = 2.8; // 1 mm

        /// <summary>
        /// Default width of space between label and tick mark.
        /// </summary>
        protected const double SpaceBetweenLabelAndTickmark = 2.1; // 0.7 mm

        protected abstract string GetDefaultTickLabelsFormat();
    }
}
