// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using GdiFont = System.Drawing.Font;

#endif
#if WPF
using System.Windows.Media;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
#endif

// ReSharper disable ConvertToPrimaryConstructor

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Represents a font resolver info created by the platform font resolver if,
    /// and only if, the font is resolved by a platform-specific flavor (GDI+ or WPF).
    /// The point is that PlatformFontResolverInfo contains the platform-specific objects
    /// like the GDI font or the WPF glyph typeface.
    /// </summary>
    class PlatformFontResolverInfo : FontResolverInfo
    {
#if CORE
        public PlatformFontResolverInfo(string faceName, bool mustSimulateBold, bool mustSimulateItalic)
            : base(faceName, mustSimulateBold, mustSimulateItalic)
        { }
#endif
#if GDI
        public PlatformFontResolverInfo(string faceName, bool mustSimulateBold, bool mustSimulateItalic, GdiFont gdiFont)
            : base(faceName, mustSimulateBold, mustSimulateItalic)
        {
            GdiFont = gdiFont;
        }
#endif
#if WPF
        public PlatformFontResolverInfo(string faceName, bool mustSimulateBold, bool mustSimulateItalic, WpfFontFamily wpfFontFamily,
            WpfTypeface wpfTypeface, WpfGlyphTypeface wpfGlyphTypeface)
            : base(faceName, mustSimulateBold, mustSimulateItalic)
        {
            WpfFontFamily = wpfFontFamily;
            WpfTypeface = wpfTypeface;
            WpfGlyphTypeface = wpfGlyphTypeface;
        }
#endif

#if GDI
        /// <summary>
        /// Gets the GDI font.
        /// </summary>
        public GdiFont GdiFont { get; }

#endif
#if WPF
        /// <summary>
        /// Gets the WPF font family.
        /// </summary>
        public WpfFontFamily WpfFontFamily { get; }

        /// <summary>
        /// Gets the WPF typeface.
        /// </summary>
        public WpfTypeface WpfTypeface { get; }

        /// <summary>
        /// Gets the WPF glyph typeface.
        /// </summary>
        public WpfGlyphTypeface WpfGlyphTypeface { get; }
#endif
#if UWP
        public PlatformFontResolverInfo(string faceName, bool mustSimulateBold, bool mustSimulateItalic)
            : base(faceName, mustSimulateBold, mustSimulateItalic)
        {
            //_gdiFont = gdiFont;
        }
#endif
    }
}
