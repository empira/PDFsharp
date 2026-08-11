// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#define CACHE_FONTS_

using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;

// v7.0.0 REVIEW PSG

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Helps to measure and handle fonts.
    /// </summary>
    static class FontHandler
    {
#if DEBUG_
        internal static int CreateFontCounter;
#endif
        /// <summary>
        /// Converts a DOM Font to an XFont.
        /// </summary>
        internal static XFont FontToXFont(Font font)
        {
            // Read the whole cache entry once. The Font and the XFont are published together by a single
            // reference assignment, so a concurrent update can never be observed half-applied and this can
            // never return an XFont belonging to a different Font. See FontCacheEntry.
            var entry = _lastEntry;
            if (entry != null &&
                entry.FontRef.TryGetTarget(out var lastFont) && font == lastFont &&
                entry.XFontRef.TryGetTarget(out var lastXFont))
                return lastXFont;

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
            _lastEntry = new FontCacheEntry(font, xFont);
#if FORCE_MEMORYLEAK
            _lastFont2 = font;
#endif
            return xFont;
        }

        /// <summary>
        /// The single-entry font cache. Both WeakReferences are assigned in the constructor and the instance
        /// is published by one atomic reference assignment, so a reader sees either the complete previous
        /// entry or the complete new one — never the Font from one and the XFont from another.
        /// </summary>
        sealed class FontCacheEntry
        {
            internal FontCacheEntry(Font font, XFont xFont)
            {
                FontRef = new WeakReference<Font>(font);
                XFontRef = new WeakReference<XFont>(xFont);
            }

            internal readonly WeakReference<Font> FontRef;
            internal readonly WeakReference<XFont> XFontRef;
        }

        static volatile FontCacheEntry? _lastEntry;
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
#if PSGFX
            var descent = (font.FontFace.Height - font.FontFace.Baseline) * font.Size;
            return new(descent);
#else
            XUnitPt descent = font.Metrics.Descent;
            descent *= font.Size;
            descent /= font.FontFamily.GetEmHeight(font.Style);
            return descent;
#endif
        }

        internal static XUnitPt GetAscent(XFont font)
        {
#if PSGFX
            var descent = font.FontFace.Baseline * font.Size;
            return new(descent);
#else
            XUnitPt ascent = font.Metrics.Ascent;
            ascent *= font.Size;
            ascent /= font.FontFamily.GetEmHeight(font.Style);
            return ascent;
#endif
        }

        internal static double GetSubSuperScaling(XFont font)
        {
#if PSGFX
            return 0.8 * font.Size;  // TODO #PSG
#else
            return 0.8 * GetAscent(font) / font.GetHeight();
#endif
        }

        internal static XFont ToSubSuperFont(XFont font)
        {
#if PSGFX
            double size = font.Size * GetSubSuperScaling(font);
            return new XFont(font.FontFace.FamilyName, size, font.Style/*, font.PdfOptions*/);  // TODO #PSG
#else
            double size = font.Size * GetSubSuperScaling(font);
            return new XFont(font.Name, size, font.Style, font.PdfOptions);
#endif
        }

        internal static XBrush FontColorToXBrush(Font font)
        {
#if PSGFX
            Debug.Assert(font.Document != null, "font.Document != null");
            return new XSolidBrush(ColorHelper.ToXColor(font.Color, font.Document.UseCmykColor));
#else
#if noCMYK
            return new XSolidBrush(XColor.FromArgb((int)font.Color.A, (int)font.Color.R, (int)font.Color.G, (int)font.Color.B));
#else
            Debug.Assert(font.Document != null, "font.Document != null");
            return new XSolidBrush(ColorHelper.ToXColor(font.Color, font.Document.UseCmykColor));
#endif
#endif
        }

#if CACHE_FONTS || true_
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
