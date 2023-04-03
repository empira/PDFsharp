// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders a line format to an XGraphics object.
    /// </summary>
    class LineFormatRenderer
    {
        public LineFormatRenderer(LineFormat? lineFormat, XGraphics gfx)
        {
            _lineFormat = lineFormat;
            _gfx = gfx;
        }

        XColor GetColor()
        {
            var clr = Colors.Black;

            //if (_lineFormat != null && !_lineFormat.Color.IsEmpty)
            if (_lineFormat != null && !_lineFormat.Values.Color.IsValueNullOrEmpty())
                clr = _lineFormat.Color;

#if noCMYK
            return XColor.FromArgb((int)clr.Argb);
#else
            Debug.Assert(_lineFormat is null || _lineFormat?.Document != null, "_lineFormat?.Document != null");
            return ColorHelper.ToXColor(clr, _lineFormat?.Document!.UseCmykColor ?? NRT.ThrowOnNull<bool>());
#endif
        }

        internal XUnit GetWidth()
        {
            if (_lineFormat == null!)
                return 0;
            if (_lineFormat.Values.Visible is not null && !_lineFormat.Visible)
                return 0;

            //if (_lineFormat.Values.Width is not null)
            if (!_lineFormat.Values.Width.IsValueNullOrEmpty())
                return _lineFormat.Width.Point;

            //if (_lineFormat.Values.Color is not null || _lineFormat.Values.Style is not null || _lineFormat.Visible)
            if (!_lineFormat.Values.Color.IsValueNullOrEmpty() || _lineFormat.Values.Style is not null || _lineFormat.Visible)
                return 1;

            return 0;
        }

        internal void Render(XUnit xPosition, XUnit yPosition, XUnit width, XUnit height)
        {
            XUnit lineWidth = GetWidth();
            if (lineWidth > 0)
            {
                var pen = GetPen(lineWidth);
                if (pen != null)
                    _gfx.DrawRectangle(pen, xPosition, yPosition, width, height);
            }
        }

        XPen? GetPen(XUnit width)
        {
            if (width == 0)
                return null;

            var pen = new XPen(GetColor(), width);
            pen.DashStyle = (_lineFormat?.DashStyle ?? NRT.ThrowOnNull<DashStyle>()) switch
            {
                DashStyle.Dash => XDashStyle.Dash,
                DashStyle.DashDot => XDashStyle.DashDot,
                DashStyle.DashDotDot => XDashStyle.DashDotDot,
                DashStyle.Solid => XDashStyle.Solid,
                DashStyle.SquareDot => XDashStyle.Dot,
                _ => pen.DashStyle
            };
            return pen;
        }

        readonly LineFormat? _lineFormat;
        readonly XGraphics _gfx;
    }
}
