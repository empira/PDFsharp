// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using PdfSharp.Logging;
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
#if WUI
using Windows.UI.Text;
using Windows.UI.Xaml.Media;
#endif
//using System.Drawing;
//using Microsoft.Extensions.Logging;
//using PdfSharp.Events;
//using PdfSharp.Fonts;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
//using PdfSharp.Fonts.OpenType;
//using PdfSharp.Logging;

namespace PdfSharp.Fonts.Internal
{
    /// <summary>
    /// A bunch of internal functions that do not have a better place.
    /// </summary>
    static class FontHelper
    {
#if true_ // #DELETE 24-12-31
        /// <summary>
        /// Measure string directly from font data.
        /// </summary>
        public static XSize MeasureString(string text, XFont font)
        {
            Debug.Assert(false, "Use the new version with code run parameter.");
            
            if (String.IsNullOrEmpty(text))
                return new(); // not XSize.Empty !

            var cp = UnicodeHelper.Utf32FromString2(text);
            var codePoints = font.OpenTypeDescriptor.GlyphIndicessFromCodepoints(cp);
            //var codeRun = new CharacterCodeRun(codePoints);

            //// Invoke RenderEvent.
            //var args = new RenderCodeRunEventArgs(Owner)
            //{
            //    Font = font,
            //    CodeRun = codeRun
            //};
            //Owner.RenderEvents.OnRenderCodeRun(this, args);
            //codeRun = args.CodeRun;
            //codePoints = codeRun.Items;

            return MeasureString(codePoints, font);
#if true_  // Keep until 2024-12-31 for reference
            var size = new XSize();
            var descriptor = FontDescriptorCache.GetOrCreateDescriptorFor(font) as OpenTypeDescriptor;
            if (descriptor != null)
            {
                // Height is the sum of ascender and descender.
                size.Height = (descriptor.Ascender + descriptor.Descender) * font.Size / font.UnitsPerEm;
                Debug.Assert(descriptor.Ascender > 0, "Ascender must be greater than 0.");

                bool isSymbolFont = descriptor.IsSymbolFont;
                int length = text.Length;
                int width = 0;
                for (int idx = 0; idx < length; idx++)
                {
                    char ch = text[idx];
                    // H/A/C/K: Unclear what to do here.
                    if (ch < 32)
                        continue;

                    if (Char.IsLowSurrogate(ch))
                    {
                        // We only come here when the text contains a low surrogate not preceded by a high surrogate.
                        // This is an error in the UTF-32 text and therefore ignored.
                        PdfSharpLogHost.FontManagementLogger.LogWarning("Unexpected low surrogate found: 0x{Char:X2}", ch);
                        continue;
                    }

                    int glyphIndex;
                    if (isSymbolFont)
                    {
                        ch = descriptor.RemapSymbolChar(ch);
                        glyphIndex = descriptor.BmpCodepointToGlyphIndex(ch);
                    }
                    else if (Char.IsHighSurrogate(ch))
                    {
                        // UTF16 surrogate pair expected.
                        if (++idx < length)
                        {
                            var ch2 = text[idx];
                            if (Char.IsLowSurrogate(ch2) is false)
                            {
                                PdfSharpLogHost.FontManagementLogger.LogWarning("High surrogate 0x{Char:X2} not followed by low surrogate.", ch);
                                continue;
                            }
                            glyphIndex = descriptor.SurrogatePairToGlyphIndex(ch, ch2);
                        }
                        else
                        {
                            PdfSharpLogHost.FontManagementLogger.LogWarning("High surrogate 0x{Char:X2} found at end of string.", ch);
                            continue;
                        }
                    }
                    else
                    {
                        // BMP character.
                        glyphIndex = descriptor.BmpCodepointToGlyphIndex(ch);
                    }
                    width += descriptor.GlyphIDToWidth(glyphIndex);
                }
                size.Width = width * font.Size / descriptor.UnitsPerEm;

                // Adjust bold simulation.
                if ((font.GlyphTypeface.StyleSimulations & XStyleSimulations.BoldSimulation) == XStyleSimulations.BoldSimulation)
                {
                    // Add 2% of the em-size for each character.
                    // Unsure how to deal with white space. Currently count as regular character.
                    size.Width += length * font.Size * Const.BoldEmphasis;
                }
            }
            // BUG_OLD: Is it correct to return an empty size if we have no descriptor?
            Debug.Assert(descriptor != null, "No OpenTypeDescriptor.");

            return size;
#endif
        }
#endif

        /// <summary>
        /// Measure string directly from font data.
        /// This function expects that the code run is ready to be measured.
        /// The RenderEvent is not invoked.
        /// </summary>
        public static XSize MeasureString(CodePointGlyphIndexPair[] codeRun, XFont font)
        {
            var length = codeRun.Length;
            if (length == 0)
                return new(); // not XSize.Empty !

            var descriptor = font.OpenTypeDescriptor;
            var size = new XSize
            {
                // Height is the sum of ascender and descender.
                Height = (descriptor.Ascender + descriptor.Descender) * font.Size / font.UnitsPerEm
            };
            Debug.Assert(descriptor.Ascender > 0, "Ascender must be greater than 0.");

            int width = 0;
            //var items = codeRun..Items;
            for (int idx = 0; idx < length; idx++)
            {
                var glyphIndex = codeRun[idx].GlyphIndex;
                width += descriptor.GlyphIndexToWidth(glyphIndex);
            }
            size.Width = width * font.Size / descriptor.UnitsPerEm;

            // Adjust bold simulation.
            if ((font.GlyphTypeface.StyleSimulations & XStyleSimulations.BoldSimulation) == XStyleSimulations.BoldSimulation)
            {
                // Add 2% of the em-size for each character.
                // Unsure how to deal with white space. Currently count as regular character.
                size.Width += length * font.Size * Const.BoldEmphasis;
            }
            return size;
        }

#if CORE
        /// <summary>
        /// Creates a typeface from XFontStyleEx.
        /// </summary>
        public static XTypeface CreateTypeface(XFontFamily family, XFontStyleEx style)
        {
            // Does not work with fonts that have others than the four default styles.
            XFontStyle fontStyle = XFontStyle.FromGdiFontStyle((XFontStyleEx)style);
            XFontWeight fontWeight = XFontWeight.FromGdiFontStyle((XFontStyleEx)style);
            var typeface = new XTypeface(family, fontStyle, fontWeight, XFontStretches.Normal);
            return typeface;
        }
#endif
#if GDI
        public static GdiFont? CreateFont(string familyName, double emSize, GdiFontStyle style, out XFontSource? fontSource)
        {
            fontSource = null;
            // ReSharper disable once JoinDeclarationAndInitializer
            GdiFont? font;

            // Create ordinary Win32 font.
            font = new GdiFont(familyName, (float)emSize, style, GraphicsUnit.World);

            // var name = font.Name; same as family name.
            var gdiFamilyName = font.FontFamily.Name;
#if true
            // Check for substitution if font is 'Microsoft Sans Serif'.
            if (gdiFamilyName.Equals("Microsoft Sans Serif"))
            {
                // Is this the family we indeed requested?
                if (!gdiFamilyName.Equals(familyName, StringComparison.OrdinalIgnoreCase))
                    return null;
            }
#else
            if (!gdiFamilyName.Equals(familyName, StringComparison.OrdinalIgnoreCase))
            {
                var message = Invariant($"GDI request for font '' returns ''.");
                PdfSharpLogHost.Logger.LogError(message);
                return null;
            }
#endif
            return font;
        }
#endif

#if WPF
        public static readonly CultureInfo CultureInfoEnUs = CultureInfo.GetCultureInfo("en-US");
        public static readonly XmlLanguage XmlLanguageEnUs = XmlLanguage.GetLanguage("en-US");

        /// <summary>
        /// Creates a typeface from XFontStyleEx.
        /// </summary>
        public static WpfTypeface CreateTypeface(WpfFontFamily family, XFontStyleEx style)
        {
            // Does not work with fonts that have others than the four default styles.
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

            // Does not work with fonts that have others than the four default styles
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
        public static WpfFontWeight FontWeightFromStyle(XFontStyleEx style)
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

            // Must only be called if font is not created by a font resolver.
            var wpfFamily = family.WpfFamily;
            Debug.Assert(wpfFamily != null);
            var typefaces = new List<WpfTypeface>(wpfFamily.GetTypefaces());
            foreach (var typeface in typefaces)
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
                    s2 += s1;
                }
                s1 %= prime;
                s2 %= prime;
            }
            //return ((ulong)((ulong)(((ulong)s2 << 16) | (ulong)s1)) << 32) | (ulong)buffer.Length;
            ulong ul1 = ((ulong)s2 << 16) | s1;
            //ul1 |= s1;
            uint ui2 = (uint)buffer.Length;
            return (ul1 << 32) | ui2;
        }

        public static XFontStyleEx CreateStyle(bool isBold, bool isItalic)
            => (isBold ? XFontStyleEx.Bold : 0) | (isItalic ? XFontStyleEx.Italic : 0);

        public static int CountGlyphs(XFont font)
        {
            int counter = 0;
            for (int codePoint = 0; codePoint < 0x10FFFF; codePoint++)
            {
                // Skip surrogates.
                if (codePoint is >= 0xD000 and <= 0xDFFF)
                    continue;

                var glyphIndex = GlyphHelper.GlyphIndexFromCodePoint(codePoint, font);
                if (glyphIndex != 0)
                    counter++;
            }
            return counter;
        }
    }
}
