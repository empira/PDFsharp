// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using PdfSharp.Charting;
using Chart = MigraDoc.DocumentObjectModel.Shapes.Charts.Chart;
using PlotArea = MigraDoc.DocumentObjectModel.Shapes.Charts.PlotArea;
using VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders a chart to an XGraphics object.
    /// </summary>
    class ChartRenderer : ShapeRenderer
    {
        internal ChartRenderer(XGraphics gfx, Chart chart, FieldInfos? fieldInfos)
            : base(gfx, chart, fieldInfos)
        {
            _chart = chart;
            var renderInfo = new ChartRenderInfo();
            renderInfo.DocumentObject = _shape;
            _renderInfo = renderInfo;
        }

        internal ChartRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _chart = (Chart)renderInfo.DocumentObject;
        }

        FormattedTextArea? GetFormattedTextArea(TextArea? area, XUnit width)
        {
            if (area == null)
                return null;

            var formattedTextArea = new FormattedTextArea(_documentRenderer, area, _fieldInfos);

            if (!double.IsNaN(width))
                formattedTextArea.InnerWidth = width;

            formattedTextArea.Format(_gfx);
            return formattedTextArea;
        }

        FormattedTextArea? GetFormattedTextArea(TextArea? area)
        {
            return GetFormattedTextArea(area, Double.NaN);
        }

        void GetLeftRightVerticalPosition(out XUnit top, out XUnit bottom)
        {
            //REM: Line width is still ignored while layouting charts.
            Area contentArea = _renderInfo.LayoutInfo.ContentArea;
            ChartFormatInfo formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;
            top = contentArea.Y;

            if (formatInfo.FormattedHeader != null)
                top += formatInfo.FormattedHeader.InnerHeight;

            bottom = contentArea.Y + contentArea.Height;
            if (formatInfo.FormattedFooter != null)
                bottom -= formatInfo.FormattedFooter.InnerHeight;
        }

        Rectangle GetLeftRect()
        {
            Area contentArea = _renderInfo.LayoutInfo.ContentArea;
            ChartFormatInfo formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;
            XUnit top;
            XUnit bottom;
            GetLeftRightVerticalPosition(out top, out bottom);

            XUnit left = contentArea.X;
            XUnit width = formatInfo.FormattedLeft?.InnerWidth ?? NRT.ThrowOnNull<XUnit>();

            return new Rectangle(left, top, width, bottom - top);
        }

        Rectangle GetRightRect()
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            var formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;
            GetLeftRightVerticalPosition(out var top, out var bottom);

            //XUnit left = contentArea.X + contentArea.Width - formatInfo.FormattedRight.InnerWidth;
            //XUnit width = formatInfo.FormattedRight.InnerWidth;
            XUnit left = contentArea.X + contentArea.Width - formatInfo.FormattedRight?.InnerWidth ?? NRT.ThrowOnNull<XUnit, FormattedTextArea>();
            XUnit width = formatInfo.FormattedRight?.InnerWidth ?? NRT.ThrowOnNull<XUnit, FormattedTextArea>();

            return new Rectangle(left, top, width, bottom - top);
        }

        Rectangle GetHeaderRect()
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            var formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;

            var left = contentArea.X;
            var top = contentArea.Y;
            var width = contentArea.Width;
            var height = formatInfo.FormattedHeader?.InnerHeight ?? NRT.ThrowOnNull<XUnit>();

            return new Rectangle(left, top, width, height);
        }

        Rectangle GetFooterRect()
        {
            Area contentArea = _renderInfo.LayoutInfo.ContentArea;
            ChartFormatInfo formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;

            var left = contentArea.X;
            var top = contentArea.Y + contentArea.Height - formatInfo.FormattedFooter?.InnerHeight ?? NRT.ThrowOnNull<XUnit>();
            var width = contentArea.Width;
            var height = formatInfo.FormattedFooter?.InnerHeight ?? NRT.ThrowOnNull<XUnit>();

            return new Rectangle(left, top, width, height);
        }

        Rectangle GetTopRect()
        {
            Area contentArea = _renderInfo.LayoutInfo.ContentArea;
            ChartFormatInfo formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;

            XUnit left;
            XUnit right;
            GetTopBottomHorizontalPosition(out left, out right);

            XUnit top = contentArea.Y;
            if (formatInfo.FormattedHeader != null)
                top += formatInfo.FormattedHeader.InnerHeight;

            XUnit height = formatInfo.FormattedTop?.InnerHeight ?? NRT.ThrowOnNull<XUnit>();

            return new Rectangle(left, top, right - left, height);
        }

        Rectangle GetBottomRect()
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            var formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;

            GetTopBottomHorizontalPosition(out var left, out var right);

            XUnit top = contentArea.Y + contentArea.Height - formatInfo.FormattedBottom?.InnerHeight ?? NRT.ThrowOnNull<XUnit>();
            if (formatInfo.FormattedFooter != null)
                top -= formatInfo.FormattedFooter.InnerHeight;

            XUnit height = formatInfo.FormattedBottom?.InnerHeight ?? NRT.ThrowOnNull<XUnit>();
            return new Rectangle(left, top, right - left, height);
        }

        Rectangle GetPlotRect()
        {
            Area contentArea = _renderInfo.LayoutInfo.ContentArea;
            ChartFormatInfo formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;
            XUnit top = contentArea.Y;
            if (formatInfo.FormattedHeader != null)
                top += formatInfo.FormattedHeader.InnerHeight;

            if (formatInfo.FormattedTop != null)
                top += formatInfo.FormattedTop.InnerHeight;

            XUnit bottom = contentArea.Y + contentArea.Height;
            if (formatInfo.FormattedFooter != null)
                bottom -= formatInfo.FormattedFooter.InnerHeight;

            if (formatInfo.FormattedBottom != null)
                bottom -= formatInfo.FormattedBottom.InnerHeight;

            XUnit left = contentArea.X;
            if (formatInfo.FormattedLeft != null)
                left += formatInfo.FormattedLeft.InnerWidth;

            XUnit right = contentArea.X + contentArea.Width;
            if (formatInfo.FormattedRight != null)
                right -= formatInfo.FormattedRight.InnerWidth;

            return new Rectangle(left, top, right - left, bottom - top);
        }

        internal override void Format(Area area, FormatInfo? previousFormatInfo)
        {
            var formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;

            var textArea = _chart.Values.HeaderArea;
            formatInfo.FormattedHeader = GetFormattedTextArea(textArea, _chart.Width.Point);

            textArea = _chart.Values.FooterArea;
            formatInfo.FormattedFooter = GetFormattedTextArea(textArea, _chart.Width.Point);

            textArea = _chart.Values.LeftArea;
            formatInfo.FormattedLeft = GetFormattedTextArea(textArea);

            textArea = _chart.Values.RightArea;
            formatInfo.FormattedRight = GetFormattedTextArea(textArea);

            textArea = _chart.Values.TopArea;
            formatInfo.FormattedTop = GetFormattedTextArea(textArea, GetTopBottomWidth());

            textArea = _chart.Values.BottomArea;
            formatInfo.FormattedBottom = GetFormattedTextArea(textArea, GetTopBottomWidth());

            base.Format(area, previousFormatInfo);
            formatInfo.ChartFrame = ChartMapper.ChartMapper.Map(_chart);

            //FormattedTextArea Throw() // StL: Added to make it compile without warnings.
            //    => throw new InvalidOperationException("Must not be null here.");
        }

        XUnit AlignVertically(VerticalAlignment vAlign, XUnit top, XUnit bottom, XUnit height)
        {
            switch (vAlign)
            {
                case VerticalAlignment.Bottom:
                    return bottom - height;

                case VerticalAlignment.Center:
                    return (top + bottom - height) / 2;

                default:
                    return top;
            }
        }

        /// <summary>
        /// Gets the width of the top and bottom area.
        /// Used while formatting.
        /// </summary>
        /// <returns>The width of the top and bottom area</returns>
        XUnit GetTopBottomWidth()
        {
            ChartFormatInfo formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;
            XUnit width = _chart.Width.Point;
            if (formatInfo.FormattedRight != null)
                width -= formatInfo.FormattedRight.InnerWidth;
            if (formatInfo.FormattedLeft != null)
                width -= formatInfo.FormattedLeft.InnerWidth;
            return width;
        }

        /// <summary>
        /// Gets the horizontal boundaries of the top and bottom area.
        /// Used while rendering.
        /// </summary>
        /// <param name="left">The left boundary of the top and bottom area</param>
        /// <param name="right">The right boundary of the top and bottom area</param>
        void GetTopBottomHorizontalPosition(out XUnit left, out XUnit right)
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            var formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;
            left = contentArea.X;
            right = contentArea.X + contentArea.Width;

            if (formatInfo.FormattedRight != null)
                right -= formatInfo.FormattedRight.InnerWidth;
            if (formatInfo.FormattedLeft != null)
                left += formatInfo.FormattedLeft.InnerWidth;
        }

        void RenderArea(FormattedTextArea? area, Rectangle rect)
        {
            if (area == null)
                return;

            var textArea = area.TextArea;

            var fillFormatRenderer = new FillFormatRenderer(textArea.Values.FillFormat, _gfx);
            fillFormatRenderer.Render(rect.X, rect.Y, rect.Width, rect.Height);

            XUnit top = rect.Y;
            top += textArea.TopPadding;
            XUnit bottom = rect.Y + rect.Height;
            bottom -= textArea.BottomPadding;
            top = AlignVertically(textArea.VerticalAlignment, top, bottom, area.ContentHeight);

            XUnit left = rect.X;
            left += textArea.LeftPadding;

            RenderInfo[] renderInfos = area.GetRenderInfos();
            RenderByInfos(left, top, renderInfos);

            var lineFormatRenderer = new LineFormatRenderer(textArea.Values.LineFormat/* ?? NRT.ThrowOnNull<LineFormat>()*/, _gfx);
            lineFormatRenderer.Render(rect.X, rect.Y, rect.Width, rect.Height);
        }

        internal override void Render()
        {
            RenderFilling();
            var contentArea = _renderInfo.LayoutInfo.ContentArea;

            var formatInfo = (ChartFormatInfo)_renderInfo.FormatInfo;
            if (formatInfo.FormattedHeader != null)
                RenderArea(formatInfo.FormattedHeader, GetHeaderRect());

            if (formatInfo.FormattedFooter != null)
                RenderArea(formatInfo.FormattedFooter, GetFooterRect());

            if (formatInfo.FormattedTop != null)
                RenderArea(formatInfo.FormattedTop, GetTopRect());

            if (formatInfo.FormattedBottom != null)
                RenderArea(formatInfo.FormattedBottom, GetBottomRect());

            if (formatInfo.FormattedLeft != null)
                RenderArea(formatInfo.FormattedLeft, GetLeftRect());

            if (formatInfo.FormattedRight != null)
                RenderArea(formatInfo.FormattedRight, GetRightRect());

            var plotArea = _chart.Values.PlotArea;
            if (plotArea != null)
                RenderPlotArea(plotArea, GetPlotRect());

            RenderLine();
        }

        void RenderPlotArea(PlotArea area, Rectangle rect)
        {
            var chartFrame = ((ChartFormatInfo)_renderInfo.FormatInfo).ChartFrame ?? NRT.ThrowOnNull<ChartFrame>("BUG");

            XUnit top = rect.Y;
            top += area.TopPadding;

            XUnit bottom = rect.Y + rect.Height;
            bottom -= area.BottomPadding;

            XUnit left = rect.X;
            left += area.LeftPadding;

            XUnit right = rect.X + rect.Width;
            right -= area.RightPadding;

            chartFrame.Location = new XPoint(left, top);
            chartFrame.Size = new XSize(right - left, bottom - top);
            chartFrame.DrawChart(_gfx);
        }

        readonly Chart _chart;
    }
}
