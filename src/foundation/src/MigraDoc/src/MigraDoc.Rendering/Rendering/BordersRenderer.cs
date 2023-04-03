// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders a single Border.
    /// </summary>
    class BordersRenderer
    {
        internal BordersRenderer(Borders borders, XGraphics gfx)
        {
            Debug.Assert(borders.Document != null);
            _gfx = gfx;
            _borders = borders;
        }

        Border? GetBorder(BorderType type)
            => _borders.GetBorder(type);

        XColor GetColor(BorderType type)
        {
            var clr = Colors.Black;

            var border = GetBorder(type);
            if (border != null && !border.Color.IsEmpty)
                clr = border.Color;
            else if (!_borders.Color.IsEmpty)
                clr = _borders.Color;

#if noCMYK
            return XColor.FromArgb((int)clr.Argb);
#else
            //      bool cmyk = false; // BUG CMYK
            //      if (_borders.Document != null)
            //        cmyk = _borders.Document.UseCmykColor;
            //#if DEBUG
            //      else
            //        GetT ype();
            //#endif
            Debug.Assert(_borders.Document != null, "_borders.Document != null");
            return ColorHelper.ToXColor(clr, _borders.Document.UseCmykColor);
#endif
        }

        BorderStyle GetStyle(BorderType type)
        {
            var style = BorderStyle.Single;

            var border = GetBorder(type);
            //if (border != null && !border._style.IsNull)
            if (border?.Values.Style != null)
                style = border.Style;
            //else if (!_borders._style.IsNull)
            else if (_borders.Values.Style is not null)
                style = _borders.Style;
            return style;
        }

        internal XUnit GetWidth(BorderType type)
        {
            if (_borders == null!)
                return 0;

            var border = GetBorder(type);

            if (border != null)
            {
                var values = border.Values;

                //if (!border._visible.IsNull && !border.Visible)
                if (values.Visible is not null && !values.Visible.Value)
                    return 0;

                //if (!border._width.IsNull)
                //if (border.Values.Width is not null)
                if (!values.Width.IsValueNullOrEmpty())
                    return values.Width!.Value.Point;

                //if (!border._color.IsNull || !border._style.IsNull || border.Visible)
                //if (border.Values.Color is not null || border.Values.Style is not null || border.Visible)
                if (!values.Color.IsValueNullOrEmpty() || values.Style is not null || border.Visible)
                {
                    //if (!_borders._width.IsNull)
                    //if (_borders.Values.Width is not null)
                    if (!_borders.Values.Width.IsValueNullOrEmpty())
                        return _borders.Values.Width!.Value.Point;

                    return 0.5;
                }
            }
            else if (!(type == BorderType.DiagonalDown || type == BorderType.DiagonalUp))
            {
                var values = _borders.Values;
                //if (!_borders._visible.IsNull && !_borders.Visible)
                if (values.Visible is not null && !values.Visible.Value)
                    return 0;

                //if (!_borders._width.IsNull)
                //if (_borders.Values.Width is not null)
                if (!values.Width.IsValueNullOrEmpty())
                    return values.Width!.Value.Point;

                //if (!_borders._color.IsNull || !_borders._style.IsNull || _borders.Visible)
                //if (_borders.Values.Color is not null || _borders.Values.Style is not null || _borders.Visible)
                if (!values.Color.IsValueNullOrEmpty() || values.Style is not null || _borders.Visible)
                    return 0.5;
            }
            return 0;
        }

        /// <summary>
        /// Renders the border top down.
        /// </summary>
        /// <param name="type">The type of the border.</param>
        /// <param name="left">The left position of the border.</param>
        /// <param name="top">The top position of the border.</param>
        /// <param name="height">The height on which to render the border.</param>
        internal void RenderVertically(BorderType type, XUnit left, XUnit top, XUnit height)
        {
            XUnit borderWidth = GetWidth(type);
            if (borderWidth == 0)
                return;

            left += borderWidth / 2;
            var pen = GetPen(type);
            if (pen != null)
                _gfx.DrawLine(pen, left, top + height, left, top);
        }

        /// <summary>
        /// Renders the border top down.
        /// </summary>
        /// <param name="type">The type of the border.</param>
        /// <param name="left">The left position of the border.</param>
        /// <param name="top">The top position of the border.</param>
        /// <param name="width">The width on which to render the border.</param>
        internal void RenderHorizontally(BorderType type, XUnit left, XUnit top, XUnit width)
        {
            XUnit borderWidth = GetWidth(type);
            if (borderWidth == 0)
                return;

            top += borderWidth / 2;
            var pen = GetPen(type);
            if (pen != null)
                _gfx.DrawLine(pen, left + width, top, left, top);
        }


        internal void RenderDiagonally(BorderType type, XUnit left, XUnit top, XUnit width, XUnit height)
        {
            XUnit borderWidth = GetWidth(type);
            if (borderWidth == 0)
                return;

            XGraphicsState state = _gfx.Save();
            _gfx.IntersectClip(new XRect(left, top, width, height));

            if (type == BorderType.DiagonalDown)
            {
                var pen = GetPen(type);
                if (pen != null)
                    _gfx.DrawLine(pen, left, top, left + width, top + height);
            }
            else if (type == BorderType.DiagonalUp)
            {
                var pen = GetPen(type);
                if (pen != null)
                    _gfx.DrawLine(pen, left, top + height, left + width, top);
            }

            _gfx.Restore(state);
        }

        internal void RenderRounded(RoundedCorner roundedCorner, XUnit x, XUnit y, XUnit width, XUnit height)
        {
            if (roundedCorner == RoundedCorner.None)
                return;

            // As source we use the vertical borders.
            // If not set originally, they have been set to the horizontal border values in TableRenderer.EqualizeRoundedCornerBorders().
            BorderType borderType = BorderType.Top;
            if (roundedCorner == RoundedCorner.TopLeft || roundedCorner == RoundedCorner.BottomLeft)
                borderType = BorderType.Left;
            if (roundedCorner == RoundedCorner.TopRight || roundedCorner == RoundedCorner.BottomRight)
                borderType = BorderType.Right;

            var borderWidth = GetWidth(borderType);
            var borderPen = GetPen(borderType);

            if (borderWidth == 0 || borderPen == null)
                return;

            x -= borderWidth / 2;
            y -= borderWidth / 2;
            XUnit ellipseWidth = width * 2 + borderWidth;
            XUnit ellipseHeight = height * 2 + borderWidth;

            switch (roundedCorner)
            {
                case RoundedCorner.TopLeft:
                    _gfx.DrawArc(borderPen, new XRect(x, y, ellipseWidth, ellipseHeight), 180, 90);
                    break;
                case RoundedCorner.TopRight:
                    _gfx.DrawArc(borderPen, new XRect(x - width, y, ellipseWidth, ellipseHeight), 270, 90);
                    break;
                case RoundedCorner.BottomRight:
                    _gfx.DrawArc(borderPen, new XRect(x - width, y - height, ellipseWidth, ellipseHeight), 0, 90);
                    break;
                case RoundedCorner.BottomLeft:
                    _gfx.DrawArc(borderPen, new XRect(x, y - height, ellipseWidth, ellipseHeight), 90, 90);
                    break;
            }
        }

        XPen? GetPen(BorderType type)
        {
            var borderWidth = GetWidth(type);
            if (borderWidth == 0)
                return null;

            var pen = new XPen(GetColor(type), borderWidth);
            var style = GetStyle(type);
            switch (style)
            {
                case BorderStyle.DashDot:
                    pen.DashStyle = XDashStyle.DashDot;
                    break;

                case BorderStyle.DashDotDot:
                    pen.DashStyle = XDashStyle.DashDotDot;
                    break;

                case BorderStyle.DashLargeGap:
                    pen.DashPattern = new double[] { 3, 3 };
                    break;

                case BorderStyle.DashSmallGap:
                    pen.DashPattern = new double[] { 5, 1 };
                    break;

                case BorderStyle.Dot:
                    pen.DashStyle = XDashStyle.Dot;
                    break;

                case BorderStyle.Single:
                default:
                    pen.DashStyle = XDashStyle.Solid;
                    break;
            }
            return pen;
        }

        internal bool IsRendered(BorderType borderType)
        {
            if (_borders == null!)
                return false;

            switch (borderType)
            {
                case BorderType.Left:
                    //if (_borders._left == null || _borders._left.IsNull())
                    if (_borders.Values.Left == null /*|| _borders._left.IsNull()*/)
                        return false;
                    return GetWidth(borderType) > 0;

                case BorderType.Right:
                    //if (_borders._right == null || _borders._right.IsNull())
                    if (_borders.Values.Right == null /*|| _borders._right.IsNull()*/)
                        return false;
                    return GetWidth(borderType) > 0;

                case BorderType.Top:
                    //if (_borders._top == null || _borders._top.IsNull())
                    if (_borders.Values.Top == null /*|| _borders._top.IsNull()*/)
                        return false;
                    return GetWidth(borderType) > 0;

                case BorderType.Bottom:
                    //if (_borders._bottom == null || _borders._bottom.IsNull())
                    if (_borders.Values.Bottom == null /*|| _borders._bottom.IsNull()*/)
                        return false;

                    return GetWidth(borderType) > 0;

                case BorderType.DiagonalDown:
                    //if (_borders._diagonalDown == null || _borders._diagonalDown.IsNull())
                    if (_borders.Values.DiagonalDown == null /*|| _borders._diagonalDown.IsNull()*/)
                        return false;
                    return GetWidth(borderType) > 0;

                case BorderType.DiagonalUp:
                    //if (_borders._diagonalUp == null || _borders._diagonalUp.IsNull())
                    if (_borders.Values.DiagonalUp == null /*|| _borders._diagonalUp.IsNull()*/)
                        return false;

                    return GetWidth(borderType) > 0;
            }
            return false;
        }

        readonly XGraphics _gfx;
        readonly Borders _borders;
    }
}
