// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Shapes;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents a formatted text frame.
    /// </summary>
    class FormattedTextFrame : IAreaProvider
    {
        internal FormattedTextFrame(TextFrame textframe, DocumentRenderer documentRenderer, FieldInfos? fieldInfos)
        {
            _textFrame = textframe;
            _fieldInfos = fieldInfos;
            _documentRenderer = documentRenderer;
        }

        internal void Format(XGraphics gfx)
        {
            _gfx = gfx;
            _isFirstArea = true;
            _formatter = new TopDownFormatter(this, _documentRenderer, _textFrame.Elements);
            _formatter.FormatOnAreas(gfx, false);
            _contentHeight = RenderInfo.GetTotalHeight(GetRenderInfos());
        }

        Area? IAreaProvider.GetNextArea()
        {
            if (_isFirstArea)
                return CalcContentRect();
            return null;
        }

        Area? IAreaProvider.ProbeNextArea() => null;

        FieldInfos IAreaProvider.AreaFieldInfos => _fieldInfos ?? NRT.ThrowOnNull<FieldInfos>();

        void IAreaProvider.StoreRenderInfos(List<RenderInfo> renderInfos)
            => _renderInfos = renderInfos;

        bool IAreaProvider.IsAreaBreakBefore(LayoutInfo layoutInfo) => false;

        internal RenderInfo[]? GetRenderInfos()
        {
            if (_renderInfos != null)
                return _renderInfos.ToArray();
            return null;
        }

        Rectangle CalcContentRect()
        {
            LineFormatRenderer lfr = new LineFormatRenderer(_textFrame.LineFormat, _gfx);
            XUnit lineWidth = lfr.GetWidth();
            XUnit width;
            XUnit xOffset = lineWidth / 2;
            XUnit yOffset = lineWidth / 2;

            if (_textFrame.Orientation == TextOrientation.Horizontal ||
              _textFrame.Orientation == TextOrientation.HorizontalRotatedFarEast)
            {
                width = _textFrame.Width.Point;
                xOffset += _textFrame.MarginLeft;
                yOffset += _textFrame.MarginTop;
                width -= xOffset;
                width -= _textFrame.MarginRight + lineWidth / 2;
            }
            else
            {
                width = _textFrame.Height.Point;
                if (_textFrame.Orientation == TextOrientation.Upward)
                {
                    xOffset += _textFrame.MarginBottom;
                    yOffset += _textFrame.MarginLeft;
                    width -= xOffset;
                    width -= _textFrame.MarginTop + lineWidth / 2;
                }
                else
                {
                    xOffset += _textFrame.MarginTop;
                    yOffset += _textFrame.MarginRight;
                    width -= xOffset;
                    width -= _textFrame.MarginBottom + lineWidth / 2;
                }
            }
            XUnit height = double.MaxValue;
            return new Rectangle(xOffset, yOffset, width, height);
        }

        XUnit ContentHeight => _contentHeight;

        bool IAreaProvider.PositionVertically(LayoutInfo layoutInfo) => false;

        bool IAreaProvider.PositionHorizontally(LayoutInfo layoutInfo)
        {
            Rectangle rect = CalcContentRect();
            switch (layoutInfo.HorizontalAlignment)
            {
                case ElementAlignment.Near:
                    if (layoutInfo.Left != 0)
                    {
                        layoutInfo.ContentArea.X += layoutInfo.Left;
                        return true;
                    }
                    return false;

                case ElementAlignment.Far:
                    XUnit xPos = rect.X + rect.Width;
                    xPos -= layoutInfo.ContentArea.Width;
                    xPos -= layoutInfo.MarginRight;
                    layoutInfo.ContentArea.X = xPos;
                    return true;

                case ElementAlignment.Center:
                    xPos = rect.Width;
                    xPos -= layoutInfo.ContentArea.Width;
                    xPos = rect.X + xPos / 2;
                    layoutInfo.ContentArea.X = xPos;
                    return true;
            }
            return false;
        }

        readonly TextFrame _textFrame;
        readonly FieldInfos? _fieldInfos;
        TopDownFormatter? _formatter;
        List<RenderInfo>? _renderInfos;
        XGraphics _gfx = null!;
        bool _isFirstArea;
        XUnit _contentHeight;
        readonly DocumentRenderer _documentRenderer;
    }
}
