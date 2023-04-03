// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders fill formats.
    /// </summary>
    class FillFormatRenderer
    {
        public FillFormatRenderer(FillFormat? fillFormat, XGraphics gfx)
        {
            _gfx = gfx;
            _fillFormat = fillFormat;
        }

        internal void Render(XUnit x, XUnit y, XUnit width, XUnit height)
        {
            var brush = GetBrush();
            if (brush == null)
                return;

            _gfx.DrawRectangle(brush, x.Point, y.Point, width.Point, height.Point);
        }

        bool IsVisible()
        {
            if (_fillFormat?.Values.Visible is not null)
                return _fillFormat.Visible;
            //return _fillFormat.Values.Color is not null;
            return !_fillFormat?.Values.Color.IsValueNullOrEmpty() ?? NRT.ThrowOnNull<bool, FillFormat>();
        }

        XBrush? GetBrush()
        {
            if (_fillFormat == null || !IsVisible())
                return null;

#if noCMYK
            return new XSolidBrush(XColor.FromArgb(_fillFormat.Color.Argb));
#else
            Debug.Assert(_fillFormat.Document != null, "_fillFormat.Document != null");
            return new XSolidBrush(ColorHelper.ToXColor(_fillFormat.Color, _fillFormat.Document.UseCmykColor));
#endif
        }

        readonly XGraphics _gfx;
        readonly FillFormat? _fillFormat;
    }
}
