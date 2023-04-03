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
        }

        /// <summary>
        /// Creates a XPen based on the specified line format. If not specified color and width will be taken
        /// from the defaultPen parameter.
        /// </summary>
        internal static XPen ToXPen(LineFormat? lineFormat, XPen defaultPen)
            => ToXPen(lineFormat, defaultPen.Color, defaultPen.Width, defaultPen.DashStyle);

        /// <summary>
        /// Creates a XPen based on the specified line format. If not specified color, width and dash style
        /// will be taken from the defaultColor, defaultWidth and defaultDashStyle parameters.
        /// </summary>
        internal static XPen ToXPen(LineFormat? lineFormat, XColor defaultColor, double defaultWidth,
            XDashStyle defaultDashStyle = XDashStyle.Solid)
        {
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
                    DashStyle = lineFormat.DashStyle,
                    DashOffset = 10 * width
                };
            }
            return pen;
        }

        /// <summary>
        /// Creates a XBrush based on the specified fill format. If not specified, color will be taken
        /// from the defaultColor parameter.
        /// </summary>
        internal static XBrush ToXBrush(FillFormat? fillFormat, XColor defaultColor)
        {
            if (fillFormat == null || fillFormat.Color.IsEmpty)
                return new XSolidBrush(defaultColor);
            return new XSolidBrush(fillFormat.Color);
        }

        /// <summary>
        /// Creates a XBrush based on the specified font color. If not specified, color will be taken
        /// from the defaultColor parameter.
        /// </summary>
        internal static XBrush ToXBrush(Font? font, XColor defaultColor)
        {
            if (font == null || font.Color.IsEmpty)
                return new XSolidBrush(defaultColor);
            return new XSolidBrush(font.Color);
        }
    }
}
