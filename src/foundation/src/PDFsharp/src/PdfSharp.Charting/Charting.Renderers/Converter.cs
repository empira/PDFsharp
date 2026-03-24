// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Internal;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Provides functions which converts Charting.DOM objects into PdfSharp.Drawing objects.
    /// </summary>
    static class Converter
    {
        /// <summary>
        /// Creates a XFont based on the font. Missing attributes will be taken from the defaultFont
        /// parameter.
        /// </summary>
        internal static XFont ToXFont(Font? font, XFont defaultFont)
        {
#if PSGFX
            return null!;
#else
            var xFont = defaultFont;
            if (font != null)
            {
                string fontFamily = font.Name;
                if (fontFamily == "")
                    fontFamily = defaultFont.FontFamily.Name;

                var fontStyle = defaultFont.Style;
                if (font.Bold)
                    fontStyle |= XFontStyleEx.Bold;
                if (font.Italic)
                    fontStyle |= XFontStyleEx.Italic;

                double size = font.Size.Point; //emSize???
                if (DoubleUtil.IsZero(size))
                    size = defaultFont.Size;

                xFont = new XFont(fontFamily, size, fontStyle);
            }
            return xFont;
#endif
        }

        /// <summary>
        /// Creates a XPen based on the specified line format. If not specified color and width will be taken
        /// from the defaultPen parameter.
        /// </summary>
        internal static XPen ToXPen(LineFormat? lineFormat, XPen defaultPen)
            => ToXPen(lineFormat, defaultPen.Color, defaultPen.Width, defaultPen.DashStyle);

        internal static XPen ToXPen(LineFormat? lineFormat, XColor defaultColor, double defaultWidth)
        {
            return ToXPen(lineFormat,  defaultColor,  defaultWidth,
#if PSGFX
                XDashStyles.Solid);
#else
                XDashStyle.Solid);
#endif  
        }

        /// <summary>
        /// Creates a XPen based on the specified line format. If not specified color, width and dash style
        /// will be taken from the defaultColor, defaultWidth and defaultDashStyle parameters.
        /// </summary>
        internal static XPen ToXPen(LineFormat? lineFormat, XColor defaultColor, double defaultWidth,
#if PSGFX
            XDashStyle? defaultDashStyle)
#else
            XDashStyle defaultDashStyle )
#endif
        {
#if PSGFX
            return XPens.Black;
#else
            XPen pen;
            if (lineFormat == null)
            {
                pen = new XPen(defaultColor, defaultWidth)
                {
                    DashStyle = defaultDashStyle
                };
            }
            else
            {
                var color = defaultColor;
                if (!lineFormat.Color.IsEmpty)
                    color = lineFormat.Color;

                double width = lineFormat.Width.Point;
                if (!lineFormat.Visible)
                    width = 0;
                if (lineFormat.Visible && DoubleUtil.IsZero(width))
                    width = defaultWidth;

                pen = new XPen(color, width)
                {
#if PSGFX
                    DashStyle = lineFormat.DashStyle,
#else
                    DashStyle = lineFormat.DashStyle,
                    DashOffset = 10 * width
#endif
                };
            }
            return pen;
#endif
        }

        /// <summary>
        /// Creates a XBrush based on the specified fill format. If not specified, color will be taken
        /// from the defaultColor parameter.
        /// </summary>
        internal static XBrush ToXBrush(FillFormat? fillFormat, XColor defaultColor)
        {
#if PSGFX
            return XBrushes.Black;
#else
            if (fillFormat == null || fillFormat.Color.IsEmpty)
                return new XSolidBrush(defaultColor);
            return new XSolidBrush(fillFormat.Color);
#endif
        }

        /// <summary>
        /// Creates a XBrush based on the specified font color. If not specified, color will be taken
        /// from the defaultColor parameter.
        /// </summary>
        internal static XBrush ToXBrush(Font? font, XColor defaultColor)
        {
#if PSGFX
            return XBrushes.Black;
#else
            if (font == null || font.Color.IsEmpty)
                return new XSolidBrush(defaultColor);
            return new XSolidBrush(font.Color);
#endif
        }
    }
}
