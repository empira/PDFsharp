// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#define CACHE_FONTS_

using System.Diagnostics;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Helps measuring and handling fonts.
    /// </summary>
    class FontHandler
    {
#if DEBUG_
        internal static int CreateFontCounter;
#endif

        /// <summary>
        /// Converts a DOM Font to an XFont.
        /// </summary>
        internal static XFont FontToXFont(Font font)
        {
            if (_lastXFont != null && font == _lastFont)
                return _lastXFont;

            XFontStyleEx style = GetXStyle(font);

#if DEBUG_
            if (StringComparer.OrdinalIgnoreCase.Compare(font.Name, "Segoe UI Semilight") == 0
                && (style & XFontStyleEx.BoldItalic) == XFontStyleEx.Italic)
                font.GetType();
#endif
            var xFont = new XFont(font.Name, font.Size, style);
#if DEBUG_
            CreateFontCounter++;
#endif
            _lastFont = font;
            _lastXFont = xFont;
            return xFont;
        }

        static XFont? _lastXFont;
        static Font? _lastFont;

        internal static XFontStyleEx GetXStyle(Font font)
        {
            XFontStyleEx style = XFontStyleEx.Regular;
            if (font.Bold)
                style = font.Italic ? XFontStyleEx.BoldItalic : XFontStyleEx.Bold;
            else if (font.Italic)
                style = XFontStyleEx.Italic;

            return style;
        }

        internal static XUnit GetDescent(XFont font)
        {
            XUnit descent = font.Metrics.Descent;
            descent *= font.Size;
            descent /= font.FontFamily.GetEmHeight(font.Style);
            return descent;
        }

        internal static XUnit GetAscent(XFont font)
        {
            XUnit ascent = font.Metrics.Ascent;
            ascent *= font.Size;
            ascent /= font.FontFamily.GetEmHeight(font.Style);
            return ascent;
        }

        internal static double GetSubSuperScaling(XFont font)
        {
            return 0.8 * GetAscent(font) / font.GetHeight();
        }

        internal static XFont ToSubSuperFont(XFont font)
        {
            double size = font.Size * GetSubSuperScaling(font);

            return new XFont(font.Name, size, font.Style, font.PdfOptions);
        }

        internal static XBrush FontColorToXBrush(Font font)
        {
#if noCMYK
            return new XSolidBrush(XColor.FromArgb((int)font.Color.A, (int)font.Color.R, (int)font.Color.G, (int)font.Color.B));
#else
            Debug.Assert(font.Document != null, "font.Document != null");
            return new XSolidBrush(ColorHelper.ToXColor(font.Color, font.Document.UseCmykColor));
#endif
        }

#if CACHE_FONTS
    static XFont XFontFromCache(Font font, bool unicode, PdfFontEmbedding fontEmbedding)
    {
      XFont xFont = null;

      XPdfFontOptions options = null;
      options = new XPdfFontOptions(fontEmbedding, unicode);
      XFontStyleEx style = GetXStyle(font);
      xFont = new XFont(font.Name, font.Size, style, options);

      return xFont;
    }

    static string BuildSignature(Font font, bool unicode, PdfFontEmbedding fontEmbedding)
    {
      StringBuilder signature = new StringBuilder(128);
      signature.Append(font.Name.ToLower());
      signature.Append(font.Size.Point.ToString("##0.0"));
      return signature.ToString();
    }

    static Hash_table fontCache = new Hash_table();
#endif
    }
}
