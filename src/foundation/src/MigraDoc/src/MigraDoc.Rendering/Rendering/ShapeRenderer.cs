// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders a shape to an XGraphics object.
    /// </summary>
    abstract class ShapeRenderer : Renderer
    {
        internal ShapeRenderer(XGraphics gfx, Shape shape, FieldInfos? fieldInfos)
            : base(gfx, shape, fieldInfos)
        {
            _shape = shape;
            var lf = _shape.Values.LineFormat;
            _lineFormatRenderer = new(lf, gfx);
            _fillFormatRenderer = null!;
        }

        internal ShapeRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos? fieldInfos)
            : base(gfx, renderInfo, fieldInfos)
        {
            _shape = (Shape)renderInfo.DocumentObject;
            var lf = _shape.Values.LineFormat;
            _lineFormatRenderer = new(lf, gfx);
            var ff = _shape.Values.FillFormat;
            _fillFormatRenderer = new(ff, gfx);
        }

        internal override LayoutInfo InitialLayoutInfo
        {
            get
            {
                LayoutInfo layoutInfo = new()
                {
                    MarginTop = _shape.WrapFormat.DistanceTop.Point,
                    MarginLeft = _shape.WrapFormat.DistanceLeft.Point,
                    MarginBottom = _shape.WrapFormat.DistanceBottom.Point,
                    MarginRight = _shape.WrapFormat.DistanceRight.Point,
                    KeepTogether = true,
                    KeepWithNext = false,
                    PageBreakBefore = false,
                    VerticalReference = GetVerticalReference(),
                    HorizontalReference = GetHorizontalReference(),
                    Floating = GetFloating()
                };

                if (layoutInfo.Floating == Floating.TopBottom && !_shape.Top.Position.IsEmpty)
                    layoutInfo.MarginTop = Math.Max(layoutInfo.MarginTop, _shape.Top.Position);
                return layoutInfo;
            }
        }

        Floating GetFloating()
        {
            if (_shape.RelativeVertical != RelativeVertical.Line && _shape.RelativeVertical != RelativeVertical.Paragraph)
                return Floating.None;

            switch (_shape.WrapFormat.Style)
            {
                case WrapStyle.None:
                case WrapStyle.Through:
                    return Floating.None;
            }
            return Floating.TopBottom;
        }

        /// <summary>
        /// Gets the shape width including line width.
        /// </summary>
        protected virtual XUnit ShapeWidth => _shape.Width + _lineFormatRenderer.GetWidth();

        /// <summary>
        /// Gets the shape height including line width.
        /// </summary>
        protected virtual XUnit ShapeHeight => _shape.Height + _lineFormatRenderer.GetWidth();

        /// <summary>
        /// Formats the shape.
        /// </summary>
        /// <param name="area">The area to fit in the shape.</param>
        /// <param name="previousFormatInfo"></param>
        internal override void Format(Area area, FormatInfo? previousFormatInfo)
        {
            var floating = GetFloating();
            bool fits = floating == Floating.None || ShapeHeight <= area.Height;
            ((ShapeFormatInfo)_renderInfo.FormatInfo).Fits = fits;
            FinishLayoutInfo(area);
        }

        void FinishLayoutInfo(Area area)
        {
            var layoutInfo = _renderInfo.LayoutInfo;
            Area contentArea = new Rectangle(area.X, area.Y, ShapeWidth, ShapeHeight);
            layoutInfo.ContentArea = contentArea;
            layoutInfo.MarginTop = _shape.WrapFormat.DistanceTop.Point;
            layoutInfo.MarginLeft = _shape.WrapFormat.DistanceLeft.Point;
            layoutInfo.MarginBottom = _shape.WrapFormat.DistanceBottom.Point;
            layoutInfo.MarginRight = _shape.WrapFormat.DistanceRight.Point;
            layoutInfo.KeepTogether = true;
            layoutInfo.KeepWithNext = false;
            layoutInfo.PageBreakBefore = false;
            layoutInfo.MinWidth = ShapeWidth;

            if (_shape.Top.ShapePosition == ShapePosition.Undefined)
                layoutInfo.Top = _shape.Top.Position.Point;

            layoutInfo.VerticalAlignment = GetVerticalAlignment();
            layoutInfo.HorizontalAlignment = GetHorizontalAlignment();

            if (_shape.Left.ShapePosition == ShapePosition.Undefined)
                layoutInfo.Left = _shape.Left.Position.Point;

            layoutInfo.HorizontalReference = GetHorizontalReference();
            layoutInfo.VerticalReference = GetVerticalReference();
            layoutInfo.Floating = GetFloating();
        }

        HorizontalReference GetHorizontalReference()
        {
            return _shape.RelativeHorizontal switch
            {
                RelativeHorizontal.Margin => HorizontalReference.PageMargin,
                RelativeHorizontal.Page => HorizontalReference.Page,
                _ => HorizontalReference.AreaBoundary
            };
        }

        VerticalReference GetVerticalReference()
        {
            return _shape.RelativeVertical switch
            {
                RelativeVertical.Margin => VerticalReference.PageMargin,
                RelativeVertical.Page => VerticalReference.Page,
                _ => VerticalReference.PreviousElement
            };
        }

        ElementAlignment GetVerticalAlignment()
        {
            return _shape.Top.ShapePosition switch
            {
                ShapePosition.Center => ElementAlignment.Center,
                ShapePosition.Bottom => ElementAlignment.Far,
                _ => ElementAlignment.Near
            };
        }

        protected void RenderFilling()
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            XUnit lineWidth = _lineFormatRenderer.GetWidth();
            // Half of the line is drawn outside the shape, the other half inside the shape.
            // Therefore we have to reduce the position of the filling by 0.5 lineWidth and width and height by 2 lineWidth.
            _fillFormatRenderer.Render(contentArea.X + lineWidth / 2, contentArea.Y + lineWidth / 2,
                contentArea.Width - 2 * lineWidth, contentArea.Height - 2 * lineWidth);
        }

        protected void RenderLine()
        {
            var contentArea = _renderInfo.LayoutInfo.ContentArea;
            XUnit lineWidth = _lineFormatRenderer.GetWidth();
            XUnit width = contentArea.Width - lineWidth;
            XUnit height = contentArea.Height - lineWidth;
            _lineFormatRenderer.Render(contentArea.X, contentArea.Y, width, height);
        }

        ElementAlignment GetHorizontalAlignment()
        {
            return _shape.Left.ShapePosition switch
            {
                ShapePosition.Center => ElementAlignment.Center,
                ShapePosition.Right => ElementAlignment.Far,
                ShapePosition.Outside => ElementAlignment.Outside,
                ShapePosition.Inside => ElementAlignment.Inside,
                _ => ElementAlignment.Near
            };
        }
        protected LineFormatRenderer _lineFormatRenderer;
        protected FillFormatRenderer _fillFormatRenderer;
        protected Shape _shape;
    }
}
