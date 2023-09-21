// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using WpfFontStyle = System.Windows.FontStyle;
using WpfFontWeight = System.Windows.FontWeight;
using WpfBrush = System.Windows.Media.Brush;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
#endif
#if UWP
using Windows.UI.Text;
using Windows.UI.Xaml.Media;
#endif
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// A bunch of internal functions that do not have a better place.
    /// </summary>
    static class FontHelper
    {
        /// <summary>
        /// Measure string directly from font data.
        /// </summary>
        public static XSize MeasureString(string text, XFont font, XStringFormat stringFormat_notyetused)
        {
            var size = new XSize();

            var descriptor = FontDescriptorCache.GetOrCreateDescriptorFor(font) as OpenTypeDescriptor;
            if (descriptor != null)
            {
                // Height is the sum of ascender and descender.
                size.Height = (descriptor.Ascender + descriptor.Descender) * font.Size / font.UnitsPerEm;
                Debug.Assert(descriptor.Ascender > 0, "Ascender must be greater than 0.");

                bool symbol = descriptor.FontFace.cmap.symbol;
                int length = text.Length;
                int width = 0;
                for (int idx = 0; idx < length; idx++)
                {
                    int glyphIndex;
                    char ch = text[idx];
                    // HACK: Unclear what to do here.
                    if (ch < 32)
                        continue;

                    if (symbol)
                    {
                        // Remap ch for symbol fonts.
                        ch = (char)(ch | (descriptor.FontFace.os2.usFirstCharIndex & 0xFF00));  // @@@ refactor
                        // Used | instead of + because of: http://pdfsharp.codeplex.com/workitem/15954
                        glyphIndex = descriptor.CharCodeToGlyphIndex(ch);
                    }
                    else
                        glyphIndex = descriptor.CharCodeToGlyphIndex(text, ref idx);
                    width += descriptor.GlyphIndexToWidth(glyphIndex);
                }
                // What? size.Width = width * font.Size * (font.Italic ? 1 : 1) / descriptor.UnitsPerEm;
                size.Width = width * font.Size / descriptor.UnitsPerEm;

                // Adjust bold simulation.
                if ((font.GlyphTypeface.StyleSimulations & XStyleSimulations.BoldSimulation) == XStyleSimulations.BoldSimulation)
                {
                    // Add 2% of the em-size for each character.
                    // Unsure how to deal with white space. Currently count as regular character.
                    size.Width += length * font.Size * Const.BoldEmphasis;
                }
            }
            // BUG: Is it correct to return an empty size if we have no constructor?
            Debug.Assert(descriptor != null, "No OpenTypeDescriptor.");

            return size;
        }

#if CORE
        /// <summary>
        /// Creates a typeface.
        /// </summary>
        public static XTypeface CreateTypeface(XFontFamily family, XFontStyleEx style)
        {
            // BUG: does not work with fonts that have others than the four default styles.
            XFontStyle fontStyle = XFontStyle.FromGdiFontStyle((XFontStyleEx)style);
            XFontWeight fontWeight = XFontWeight.FromGdiFontStyle((XFontStyleEx)style);
            var typeface = new XTypeface(family, fontStyle, fontWeight, XFontStretches.Normal);
            return typeface;
        }
#endif
#if GDI
        public static GdiFont CreateFont(string familyName, double emSize, GdiFontStyle style, out XFontSource? fontSource)
        {
            fontSource = null;
            // ReSharper disable once JoinDeclarationAndInitializer
            GdiFont? font;

            // Use font resolver in CORE build. XPrivateFontCollection exists only in GDI and WPF build.
#if GDI
            // Try private font collection first.
            font = XPrivateFontCollection.TryCreateFont(familyName, emSize, style, out fontSource);
            if (font != null)
            {
                // Get font source is different for this font because Win32 does not know it.
                return font;
            }
#endif
            // Create ordinary Win32 font.
            font = new GdiFont(familyName, (float)emSize, style, GraphicsUnit.World);
            return font;
        }
#endif

#if WPF
        public static readonly CultureInfo CultureInfoEnUs = CultureInfo.GetCultureInfo("en-US");
        public static readonly XmlLanguage XmlLanguageEnUs = XmlLanguage.GetLanguage("en-US");

        /// <summary>
        /// Creates a typeface.
        /// </summary>
        public static WpfTypeface CreateTypeface(WpfFontFamily family, XFontStyleEx style)
        {
            // BUG: does not work with fonts that have others than the four default styles
            WpfFontStyle fontStyle = FontStyleFromStyle(style);
            WpfFontWeight fontWeight = FontWeightFromStyle(style);
            WpfTypeface typeface = new WpfTypeface(family, fontStyle, fontWeight, FontStretches.Normal);
            return typeface;
        }

        /// <summary>
        /// Creates the formatted text.
        /// </summary>
        public static FormattedText CreateFormattedText(string text, Typeface typeface, double emSize, WpfBrush brush)
        {
            //FontFamily fontFamily = new FontFamily(testFontName);
            //typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Condensed);
            //List<Typeface> typefaces = new List<Typeface>(fontFamily.GetTypefaces());
            //typefaces.GetType();
            //typeface = s_typefaces[0];

            // BUG: does not work with fonts that have others than the four default styles
            FormattedText formattedText = new FormattedText(text, new CultureInfo("en-us"), FlowDirection.LeftToRight, typeface, emSize, brush, 1);
            // .NET 4.0 feature new NumberSubstitution(), TextFormattingMode.Display);
            //formattedText.SetFontWeight(FontWeights.Bold);
            //formattedText.SetFontStyle(FontStyles.Oblique);
            //formattedText.SetFontStretch(FontStretches.Condensed);
            return formattedText;
        }

        /// <summary>
        /// Simple hack to make it work...
        /// Returns Normal or Italic - bold, underline and such get lost here.
        /// </summary>
        public static WpfFontStyle FontStyleFromStyle(XFontStyleEx style)
        {
            // Mask out Underline, Strikeout, etc.
            return (style & XFontStyleEx.BoldItalic) switch
            {
                XFontStyleEx.Regular => FontStyles.Normal,
                XFontStyleEx.Bold => FontStyles.Normal,
                XFontStyleEx.Italic => FontStyles.Italic,
                XFontStyleEx.BoldItalic => FontStyles.Italic,
                _ => FontStyles.Normal
            };
        }

        /// <summary>
        /// Simple hack to make it work...
        /// </summary>
        public static FontWeight FontWeightFromStyle(XFontStyleEx style)
        {
            // Mask out Underline, Strikeout, etc.
            return (style & XFontStyleEx.BoldItalic) switch
            {
                XFontStyleEx.Regular => FontWeights.Normal,
                XFontStyleEx.Bold => FontWeights.Bold,
                XFontStyleEx.Italic => FontWeights.Normal,
                XFontStyleEx.BoldItalic => FontWeights.Bold,
                _ => FontWeights.Normal
            };
        }

        /// <summary>
        /// Determines whether the style is available as a glyph type face in the specified font family, i.e. the specified style is not simulated.
        /// </summary>
        public static bool IsStyleAvailable(XFontFamily family, XFontStyleEx style)
        {
            style &= XFontStyleEx.BoldItalic;
            // TODOWPF: check for correctness
            // FontDescriptor descriptor = FontDescriptorCache.GetOrCreateDescriptor(family.Name, style);
            //XFontMetrics metrics = descriptor.FontMetrics;

            // style &= XFontStyleEx.Regular | XFontStyleEx.Bold | XFontStyleEx.Italic | XFontStyleEx.BoldItalic; // same as XFontStyleEx.BoldItalic
            List<WpfTypeface> typefaces = new(family.WpfFamily.GetTypefaces());
            foreach (WpfTypeface typeface in typefaces)
            {
                bool bold = typeface.Weight == FontWeights.Bold;
                bool italic = typeface.Style == FontStyles.Italic;
                switch (style)
                {
                    case XFontStyleEx.Regular:
                        if (!bold && !italic)
                            return true;
                        break;

                    case XFontStyleEx.Bold:
                        if (bold && !italic)
                            return true;
                        break;

                    case XFontStyleEx.Italic:
                        if (!bold && italic)
                            return true;
                        break;

                    case XFontStyleEx.BoldItalic:
                        if (bold && italic)
                            return true;
                        break;
                }
                //////                typeface.sty
                //////                bool available = false;
                //////                GlyphTypeface glyphTypeface;
                //////                if (typeface.TryGetGlyphTypeface(out glyphTypeface))
                //////                {
                //////#if DEBUG_
                //////                    glyphTypeface.GetType();
                //////#endif
                //////                    available = true;
                //////                }
                //////                if (available)
                //////                    return true;
            }
            return false;
        }
#endif

        /// <summary>
        /// Calculates an Adler32 checksum combined with the buffer length
        /// in a 64-bit unsigned integer.
        /// </summary>
        public static ulong CalcChecksum(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            const uint prime = 65521; // largest prime smaller than 65536
            uint s1 = 0;
            uint s2 = 0;
            int length = buffer.Length;
            int offset = 0;
            while (length > 0)
            {
                int n = 3800;
                if (n > length)
                    n = length;
                length -= n;
                while (--n >= 0)
                {
                    s1 += buffer[offset++];
                    s2 = s2 + s1;
                }
                s1 %= prime;
                s2 %= prime;
            }
            //return ((ulong)((ulong)(((ulong)s2 << 16) | (ulong)s1)) << 32) | (ulong)buffer.Length;
            ulong ul1 = (ulong)s2 << 16;
            ul1 = ul1 | s1;
            ulong ul2 = (ulong)buffer.Length;
            return (ul1 << 32) | ul2;
        }

        public static XFontStyleEx CreateStyle(bool isBold, bool isItalic)
            => (isBold ? XFontStyleEx.Bold : 0) | (isItalic ? XFontStyleEx.Italic : 0);
    }
}
