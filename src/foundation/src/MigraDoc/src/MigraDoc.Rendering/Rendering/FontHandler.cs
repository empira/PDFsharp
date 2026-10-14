// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#define FORCE_MEMORYLEAK_
#define CACHE_FONTS_

using System.Diagnostics;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Helps to measure and handle fonts.
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
            // Check if WeakReference is still valid and points to the font we need.
            if (_lastEntry != null &&
                _lastEntry.TryGetTarget(out var lastEntry))
            {
                if (lastEntry.Font == font)
                    return lastEntry.XFont;
            }

            XFontStyleEx style = GetXStyle(font);

#if DEBUG_
            if (StringComparer.OrdinalIgnoreCase.Compare(font.Name, "Segoe UI Semilight") == 0
                && (style & XFontStyleEx.BoldItalic) == XFontStyleEx.Italic)
                _ = typeof(int);
#endif
            var xFont = new XFont(font.Name, font.Size.Point, style);
#if DEBUG_
            CreateFontCounter++;
#endif
            _lastEntry =new(new(font, xFont));
#if FORCE_MEMORYLEAK
            _lastFont2 = font;
#endif
            return xFont;
        }

        /// <summary>
        /// The single-entry font cache.
        /// </summary>
        internal sealed class FontCacheEntry
        {
            internal FontCacheEntry(Font font, XFont xFont)
            {
                Font = font;
                XFont = xFont;
            }

            internal readonly Font Font;
            internal readonly XFont XFont;
        }

        static volatile WeakReference<FontCacheEntry>? _lastEntry;
#if FORCE_MEMORYLEAK
        static Font? _lastFont2;
#endif

        internal static XFontStyleEx GetXStyle(Font font)
        {
            XFontStyleEx style = XFontStyleEx.Regular;
            if (font.Bold)
                style = font.Italic ? XFontStyleEx.BoldItalic : XFontStyleEx.Bold;
            else if (font.Italic)
                style = XFontStyleEx.Italic;

            return style;
        }

        internal static XUnitPt GetDescent(XFont font)
        {
            XUnitPt descent = font.Metrics.Descent;
            descent *= font.Size;
            descent /= font.FontFamily.GetEmHeight(font.Style);
            return descent;
        }

        internal static XUnitPt GetAscent(XFont font)
        {
            XUnitPt ascent = font.Metrics.Ascent;
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

            return new XFont(font.Name2, size, font.Style, font.PdfOptions);
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
