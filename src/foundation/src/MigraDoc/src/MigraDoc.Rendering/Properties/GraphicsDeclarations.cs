// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591

#if PSGFX
global using float_ = float;
global using FLOAT_ = float;
global using XPoint = PdfSharp.Graphics.Point;
global using XRect = PdfSharp.Graphics.Rect;
global using XSize = PdfSharp.Graphics.Size;
global using XMatrix = PdfSharp.Graphics.Media.Matrix;
global using XColor = PdfSharp.Graphics.Color;
global using XColors = PdfSharp.Graphics.Colors;
global using XPen = PdfSharp.Graphics.Media.Pen;
global using XPens = PdfSharp.Graphics.Media.Pens;
global using XLineJoin = PdfSharp.Graphics.Media.PenLineJoin;
global using XDashStyle = PdfSharp.Graphics.Media.DashStyle;
global using XDashStyles = PdfSharp.Graphics.Media.DashStyles;
global using XBrush = PdfSharp.Graphics.Media.Brush;
global using XBrushes = PdfSharp.Graphics.Media.Brushes;
global using XSolidBrush = PdfSharp.Graphics.Media.SolidColorBrush;
global using XLinearGradientBrush = PdfSharp.Graphics.Media.SolidColorBrush;
global using XUnit = PdfSharp.Graphics.Unit;
global using XUnitPt = PdfSharp.Graphics.UnitPt;
global using XGraphicsState = int;
global using XGraphics = PdfSharp.Graphics.Media.DrawingContext;
//global using XFont = PdfSharp.Graphics.Text.FontFace;
global using XFont = PdfSharp.Graphics.XGfx.XFontGfx;
//global using XFontStyleEx = PdfSharp.Graphics.XGfx.XFontStyleEx;
#else
global using float_ = double;
global using FLOAT_ = double;
using System.Runtime.CompilerServices;
#endif

using System.Runtime.InteropServices;
using PdfSharp.Drawing;

#if PSGFX
using PdfSharp.Graphics;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
#else
public static class IdentityExtensions
{
    extension(XRect rect)
    {
        public PdfSharp.Drawing.XRect AsXRect => rect;
    }

    extension(XPoint point)
    {
        public PdfSharp.Drawing.XPoint AsXPoint => point;
    }
}
#endif
