// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Renders a Shading to an XGraphics object.
    /// </summary>
    class ShadingRenderer
    {
        public ShadingRenderer(XGraphics gfx, Shading shading)
        {
            _gfx = gfx;
            _shading = shading;
            RealizeBrush();
        }

        internal void Render(XUnit x, XUnit y, XUnit width, XUnit height)
        {
            if (/*_shading == null! ||*/ _brush == null)
                return;

            _gfx.DrawRectangle(_brush, x.Point, y.Point, width.Point, height.Point);
        }

        internal void Render(XUnit x, XUnit y, XUnit width, XUnit height, RoundedCorner roundedCorner)
        {
            // If there is no rounded corner, we can use the usual Render method.
            if (roundedCorner == RoundedCorner.None)
            {
                Render(x, y, width, height);
                return;
            }

            if (/*_shading == null! ||*/ _brush == null)
                return;

            var path = new XGraphicsPath();

            switch (roundedCorner)
            {
                case RoundedCorner.TopLeft:
                    path.AddArc(new XRect(x, y, width * 2, height * 2), 180, 90); // Error in CORE: _corePath.AddArc().
                    path.AddLine(new XPoint(x + width, y), new XPoint(x + width, y + height));
                    break;
                case RoundedCorner.TopRight:
                    path.AddArc(new XRect(x - width, y, width * 2, height * 2), 270, 90); // Error in CORE: _corePath.AddArc().
                    path.AddLine(new XPoint(x + width, y + height), new XPoint(x, y + height));
                    break;
                case RoundedCorner.BottomRight:
                    path.AddArc(new XRect(x - width, y - height, width * 2, height * 2), 0, 90); // Error in CORE: _corePath.AddArc().
                    path.AddLine(new XPoint(x, y + height), new XPoint(x, y));
                    break;
                case RoundedCorner.BottomLeft:
                    path.AddArc(new XRect(x, y - height, width * 2, height * 2), 90, 90); // Error in CORE: _corePath.AddArc().
                    path.AddLine(new XPoint(x, y), new XPoint(x + width, y));
                    break;
            }
            path.CloseFigure();
            _gfx.DrawPath(_brush, path);
        }

        bool IsVisible()
        {
            //if (!_shading._visible.IsNull)
            if (_shading.Values.Visible is not null)
                return _shading.Visible;
            else
                //return !_shading._color.IsNull;
                //return !_shading.Values.Color?.IsNull ?? false;
                return !_shading.Values.Color.IsValueNullOrEmpty();
        }

        void RealizeBrush()
        {
            if (_shading == null!)
                return;
            if (IsVisible())
            {
#if noCMYK
                this.brush = new XSolidBrush(XColor.FromArgb((int)this.shading.Color.Argb));
#else
                Debug.Assert(_shading.Document != null, "_shading.Document != null");
                _brush = new XSolidBrush(ColorHelper.ToXColor(_shading.Color, _shading.Document.UseCmykColor));
#endif
            }
        }

        readonly Shading _shading;
        XBrush? _brush;
        readonly XGraphics _gfx;
    }
}
