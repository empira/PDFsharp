// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal.OpenType;
using PdfSharp.Drawing;
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

namespace PdfSharp.Fonts.Internal
{
    /// <summary>
    /// A bunch of internal functions that do not have a better place.
    /// </summary>
    static class FontHelper
    {
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
                // Unsure how to deal with white-space. Currently count as regular character.
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
