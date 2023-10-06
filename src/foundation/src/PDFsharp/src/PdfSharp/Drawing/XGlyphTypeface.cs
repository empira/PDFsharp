// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
#endif
#if WPF
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
using WpfStyleSimulations = System.Windows.Media.StyleSimulations;
#endif
#if UWP
using Windows.UI.Xaml.Media;
#endif
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies a physical font face that corresponds to a font file on the disk or in memory.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class XGlyphTypeface
    {
        // Implementation Notes
        // XGlyphTypeface is the centerpiece for font management. There is a one to one relationship
        // between XFont an XGlyphTypeface.
        //
        // * Each XGlyphTypeface can belong to one or more XFont objects.
        // * An XGlyphTypeface hold an XFontFamily.
        // * XGlyphTypeface hold a reference to an OpenTypeFontface. 
        // * 
        //

        const string KeyPrefix = "tk:";  // "typeface key"

#if CORE
        XGlyphTypeface(string key, XFontFamily fontFamily, XFontSource fontSource, XStyleSimulations styleSimulations)
        {
            Key = key;
            FontFamily = fontFamily;
            FontSource = fontSource;

            Fontface = OpenTypeFontface.CetOrCreateFrom(fontSource);
            Debug.Assert(ReferenceEquals(FontSource.Fontface, Fontface));

            StyleSimulations = styleSimulations;
            Initialize();
        }
#endif

#if GDI
        XGlyphTypeface(string key, XFontFamily fontFamily, XFontSource fontSource, XStyleSimulations styleSimulations, GdiFont gdiFont)
        {
            Key = key;
            FontFamily = fontFamily;
            FontSource = fontSource;

            Fontface = OpenTypeFontface.CetOrCreateFrom(fontSource);
            Debug.Assert(ReferenceEquals(FontSource.Fontface, Fontface));

            _gdiFont = gdiFont;

            StyleSimulations = styleSimulations;
            Initialize();
        }
#endif

#if GDI
        /// <summary>
        /// Initializes a new instance of the <see cref="XGlyphTypeface"/> class by a font source.
        /// </summary>
        public XGlyphTypeface(XFontSource fontSource)
        {
            string familyName = fontSource.Fontface.name.Name;
            FontFamily = new XFontFamily(familyName, false);
            Fontface = fontSource.Fontface;
            IsBold = Fontface.os2.IsBold;
            IsItalic = Fontface.os2.IsItalic;

            Key = ComputeKey(familyName, IsBold, IsItalic);
            //_fontFamily =xfont  FontFamilyCache.GetFamilyByName(familyName);
            FontSource = fontSource;

            Initialize();
        }
#endif

#if WPF
        XGlyphTypeface(string key, XFontFamily fontFamily, XFontSource fontSource, XStyleSimulations styleSimulations, WpfTypeface? wpfTypeface, WpfGlyphTypeface? wpfGlyphTypeface)
        {
            Key = key;
            FontFamily = fontFamily;
            FontSource = fontSource;
            StyleSimulations = styleSimulations;

            Fontface = OpenTypeFontface.CetOrCreateFrom(fontSource);
            Debug.Assert(ReferenceEquals(FontSource.Fontface, Fontface));

            WpfTypeface = wpfTypeface;
            WpfGlyphTypeface = wpfGlyphTypeface;

            Initialize();
        }
#endif

#if UWP
        XGlyphTypeface(string key, XFontFamily fontFamily, XFontSource fontSource, XStyleSimulations styleSimulations)
        {
            _key = key;
            _fontFamily = fontFamily;
            _fontSource = fontSource;
            _styleSimulations = styleSimulations;

            _fontface = OpenTypeFontface.CetOrCreateFrom(fontSource);
            Debug.Assert(ReferenceEquals(_fontSource.Fontface, _fontface));

            //_wpfTypeface = wpfTypeface;
            //_wpfGlyphTypeface = wpfGlyphTypeface;

            Initialize();
        }
#endif

        internal static XGlyphTypeface GetOrCreateFrom(string familyName, FontResolvingOptions fontResolvingOptions)
        {
            // Check cache for requested type face.
            string typefaceKey = ComputeKey(familyName, fontResolvingOptions);
            XGlyphTypeface? glyphTypeface;
            try
            {
                // Lock around TryGetGlyphTypeface and AddGlyphTypeface.
                Lock.EnterFontFactory();
                if (GlyphTypefaceCache.TryGetGlyphTypeface(typefaceKey, out glyphTypeface))
                {
                    // Just return existing one.
                    return glyphTypeface!;
                }

                // Resolve typeface by FontFactory.
                var fontResolverInfo = FontFactory.ResolveTypeface(familyName, fontResolvingOptions, typefaceKey);
                if (fontResolverInfo == null)
                {
                    // No fallback - just stop.
#if CORE
                    if (GlobalFontSettings.FontResolver is null)
                        throw new InvalidOperationException($"No appropriate font found for family name \"{familyName}\". "+
                                                            "Implement IFontResolver and assign to \"GlobalFontSettings.FontResolver\" to use fonts.");
#endif
                    throw new InvalidOperationException($"No appropriate font found for family name \"{familyName}\".");
                }

#if GDI
                GdiFont gdiFont = default!;
#endif
#if WPF
                WpfFontFamily? wpfFontFamily = null;
                WpfTypeface? wpfTypeface = null;
                WpfGlyphTypeface? wpfGlyphTypeface = null;
#endif
#if UWP
                // Nothing to do.
#endif
                // Now create the font family at the first.
                XFontFamily? fontFamily;
                if (fontResolverInfo is PlatformFontResolverInfo platformFontResolverInfo)
                {
                    // Case: fontResolverInfo was created by platform font resolver
                    // and contains platform specific objects that are reused.
#if CORE
                    // Cannot come here
                    fontFamily = null;
                    Debug.Assert(false);
#endif
#if GDI
                    // Reuse GDI+ font from platform font resolver.
                    gdiFont = platformFontResolverInfo.GdiFont;
                    fontFamily = XFontFamily.GetOrCreateFromGdi(gdiFont);
#endif
#if WPF
                    // Reuse WPF font family created from platform font resolver.
                    wpfFontFamily = platformFontResolverInfo.WpfFontFamily;
                    wpfTypeface = platformFontResolverInfo.WpfTypeface;
                    wpfGlyphTypeface = platformFontResolverInfo.WpfGlyphTypeface;
                    fontFamily = XFontFamily.GetOrCreateFromWpf(wpfFontFamily);
#endif
#if UWP
                    fontFamily = null;
#endif
                }
                else
                {
                    // Case: fontResolverInfo was created by custom font resolver.

                    // Get or create font family for custom font resolver retrieved font source.
                    fontFamily = XFontFamily.GetOrCreateFontFamily(familyName);
                }

                // We have a valid font resolver info. That means we also have an XFontSource object loaded in the cache.
                XFontSource fontSource = FontFactory.GetFontSourceByFontName(fontResolverInfo.FaceName);
                Debug.Assert(fontSource != null);

                // Each font source already contains its OpenTypeFontface.
#if CORE
                glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource, fontResolverInfo.StyleSimulations);
#endif
#if GDI
                glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource, fontResolverInfo.StyleSimulations, gdiFont);
#endif
#if WPF
                glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource, fontResolverInfo.StyleSimulations, wpfTypeface, wpfGlyphTypeface);
#endif
#if UWP
                glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource, fontResolverInfo.StyleSimulations);
#endif
                GlyphTypefaceCache.AddGlyphTypeface(glyphTypeface);
            }
            finally { Lock.ExitFontFactory(); }
            return glyphTypeface;
        }

#if GDI        
        /// <summary>
        /// Gets or create an XGlyphTypeface from a GDI+ font.
        /// </summary>
        /// <param name="gdiFont">The GDI+ font.</param>
        public static XGlyphTypeface GetOrCreateFromGdi(GdiFont gdiFont)
        {
            XGlyphTypeface? glyphTypeface;
            try
            {
                // Lock around TryGetGlyphTypeface and AddGlyphTypeface.
                Lock.EnterFontFactory();
                string typefaceKey = ComputeKey(gdiFont);
                if (GlyphTypefaceCache.TryGetGlyphTypeface(typefaceKey, out glyphTypeface))
                {
                    // We have the glyph typeface already in cache.
                    return glyphTypeface;
                }

                XFontFamily fontFamily = XFontFamily.GetOrCreateFromGdi(gdiFont);
                XFontSource fontSource = XFontSource.GetOrCreateFromGdi(typefaceKey, gdiFont);

                // Check if styles must be simulated.
                XStyleSimulations styleSimulations = XStyleSimulations.None;
                if (gdiFont.Bold && !fontSource.Fontface.os2.IsBold)
                    styleSimulations |= XStyleSimulations.BoldSimulation;
                if (gdiFont.Italic && !fontSource.Fontface.os2.IsItalic)
                    styleSimulations |= XStyleSimulations.ItalicSimulation;

                glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource, styleSimulations, gdiFont);
                GlyphTypefaceCache.AddGlyphTypeface(glyphTypeface);
            }
            finally { Lock.ExitFontFactory(); }

            return glyphTypeface;
        }
#endif

#if WPF
        /// <summary>
        /// Gets or create an XGlyphTypeface from a WPF typeface.
        /// </summary>
        /// <param name="wpfTypeface">The WPF typeface.</param>
        public static XGlyphTypeface? GetOrCreateFromWpf(WpfTypeface wpfTypeface)
        {
#if DEBUG_
            if (wpfTypeface.FontFamily.Source == "Segoe UI Semilight")
                wpfTypeface.GetType();
#endif
            //string typefaceKey = ComputeKey(wpfTypeface);
            //XGlyphTypeface glyphTypeface;
            //if (GlyphTypefaceCache.TryGetGlyphTypeface(typefaceKey, out glyphTypeface))
            //{
            //    // We have the glyph typeface already in cache.
            //    return glyphTypeface;
            //}

            // Lock around TryGetGlyphTypeface and AddGlyphTypeface.
            try
            {
                Lock.EnterFontFactory();

                // Create WPF glyph typeface.
                if (!wpfTypeface.TryGetGlyphTypeface(out var wpfGlyphTypeface))
                    return null;

                string typefaceKey = ComputeKey(wpfGlyphTypeface);

                string name1 = wpfGlyphTypeface.DesignerNames[FontHelper.CultureInfoEnUs];
                string name2 = wpfGlyphTypeface.FaceNames[FontHelper.CultureInfoEnUs];
                string name3 = wpfGlyphTypeface.FamilyNames[FontHelper.CultureInfoEnUs];
                string name4 = wpfGlyphTypeface.ManufacturerNames[FontHelper.CultureInfoEnUs];
                string name5 = wpfGlyphTypeface.Win32FaceNames[FontHelper.CultureInfoEnUs];
                string name6 = wpfGlyphTypeface.Win32FamilyNames[FontHelper.CultureInfoEnUs];

                if (GlyphTypefaceCache.TryGetGlyphTypeface(typefaceKey, out var glyphTypeface))
                {
                    // We have the glyph typeface already in cache.
                    return glyphTypeface;
                }

                var fontFamily = XFontFamily.GetOrCreateFromWpf(wpfTypeface.FontFamily);
                var fontSource = XFontSource.GetOrCreateFromWpf(typefaceKey, wpfGlyphTypeface);

                glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource,
                    (XStyleSimulations)wpfGlyphTypeface.StyleSimulations,
                    wpfTypeface, wpfGlyphTypeface);
                GlyphTypefaceCache.AddGlyphTypeface(glyphTypeface);

                return glyphTypeface;
            }
            finally { Lock.ExitFontFactory(); }
        }
#endif

        /// <summary>
        /// Gets the font family of this glyph typeface.
        /// </summary>
        public XFontFamily FontFamily { get; }

        internal OpenTypeFontface Fontface { get; }

        /// <summary>
        /// Gets the font source of this glyph typeface.
        /// </summary>
        public XFontSource FontSource { get; }

        void Initialize()
        {
            FamilyName = Fontface.name.Name;
            if (String.IsNullOrEmpty(FaceName) || FaceName.StartsWith("?", StringComparison.Ordinal))
                FaceName = FamilyName;
            StyleName = Fontface.name.Style;
            DisplayName = Fontface.name.FullFontName;
            if (String.IsNullOrEmpty(DisplayName))
            {
                DisplayName = FamilyName;
                if (String.IsNullOrEmpty(StyleName))
                    DisplayName += " (" + StyleName + ")";
            }

            // Bold, as defined in OS/2 table.
            IsBold = Fontface.os2.IsBold;
            // Debug.Assert(_isBold == (_fontface.os2.usWeightClass > 400), "Check font weight.");

            // Italic, as defined in OS/2 table.
            IsItalic = Fontface.os2.IsItalic;
        }

        /// <summary>
        /// Gets the name of the font face. This can be a file name, an URI, or a GUID.
        /// </summary>
        internal string FaceName { get; private set; } = default!;

        /// <summary>
        /// Gets the English family name of the font, for example "Arial".
        /// </summary>
        public string FamilyName { get; private set; } = default!;

        /// <summary>
        /// Gets the English subfamily name of the font,
        /// for example "Bold".
        /// </summary>
        public string StyleName { get; private set; } = default!;

        /// <summary>
        /// Gets the English display name of the font,
        /// for example "Arial italic".
        /// </summary>
        public string DisplayName { get; private set; } = default!;

        /// <summary>
        /// Gets a value indicating whether the font weight is bold.
        /// </summary>
        public bool IsBold { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the font style is italic.
        /// </summary>
        public bool IsItalic { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the style bold, italic, or both styles must be simulated.
        /// </summary>
        public XStyleSimulations StyleSimulations { get; }

        /// <summary>
        /// Gets the suffix of the face name in a PDF font and font descriptor.
        /// The name based on the effective value of bold and italic from the OS/2 table.
        /// </summary>
        string GetFaceNameSuffix()
        {
            // Use naming of Microsoft Word.
            if (IsBold)
                return IsItalic ? ",BoldItalic" : ",Bold";
            return IsItalic ? ",Italic" : "";
        }

        internal string GetBaseName()
        {
            string name = DisplayName;
            int ich = name.IndexOf("bold", StringComparison.OrdinalIgnoreCase);
            if (ich > 0)
                name = name.Substring(0, ich) + name.Substring(ich + 4, name.Length - ich - 4);
            ich = name.IndexOf("italic", StringComparison.OrdinalIgnoreCase);
            if (ich > 0)
                name = name.Substring(0, ich) + name.Substring(ich + 6, name.Length - ich - 6);
            //name = name.Replace(" ", "");
            name = name.Trim();
            name += GetFaceNameSuffix();
            return name;
        }

        /// <summary>
        /// Computes the bijective key for a typeface.
        /// </summary>
        internal static string ComputeKey(string familyName, FontResolvingOptions fontResolvingOptions)
        {
            // Compute a human readable key.
            string simulationSuffix = null!;
            if (fontResolvingOptions.OverrideStyleSimulations)
            {
                switch (fontResolvingOptions.StyleSimulations)
                {
                    case XStyleSimulations.BoldSimulation: simulationSuffix = "|b+/i-"; break;
                    case XStyleSimulations.ItalicSimulation: simulationSuffix = "|b-/i+"; break;
                    case XStyleSimulations.BoldItalicSimulation: simulationSuffix = "|b+/i+"; break;
                    case XStyleSimulations.None: break;
                    default: throw new ArgumentOutOfRangeException(nameof(fontResolvingOptions));
                }
            }
#if true
            // Attempt to make it faster.
            var bold = fontResolvingOptions.IsBold;
            var italic = fontResolvingOptions.IsItalic;
            if (!bold && !italic)
                return KeyPrefix + familyName.ToLowerInvariant()
                                 + "/n/400/5"
                                 + simulationSuffix;
            if (bold && !italic)
                return KeyPrefix + familyName.ToLowerInvariant()
                                 + "/n/700/5"
                                 + simulationSuffix;
            if (!bold && italic)
                return KeyPrefix + familyName.ToLowerInvariant()
                                 + "/i/400/5"
                                 + simulationSuffix;
            /*if (bold && italic)*/
            return KeyPrefix + familyName.ToLowerInvariant()
                             + "/i/700/5"
                             + simulationSuffix;
#else
            string key = KeyPrefix + familyName.ToLowerInvariant()
                + (fontResolvingOptions.IsItalic ? "/i" : "/n") // normal / oblique / italic  
                + (fontResolvingOptions.IsBold ? "/700" : "/400") + "/5" // Stretch.Normal
                + simulationSuffix;
            return key;
#endif
        }

        /// <summary>
        /// Computes the bijective key for a typeface.
        /// </summary>
        internal static string ComputeKey(string familyName, bool isBold, bool isItalic)
        {
            return ComputeKey(familyName, new FontResolvingOptions(FontHelper.CreateStyle(isBold, isItalic)));
        }

#if GDI
        internal static string ComputeKey(GdiFont gdiFont)
        {
            string name1 = gdiFont.Name;
            string name2 = gdiFont.OriginalFontName ?? "???";
            string name3 = gdiFont.SystemFontName;

            string name = name1;
            GdiFontStyle style = gdiFont.Style;

            string key = KeyPrefix + name.ToLowerInvariant() + ((style & GdiFontStyle.Italic) == GdiFontStyle.Italic ? "/i" : "/n") + ((style & GdiFontStyle.Bold) == GdiFontStyle.Bold ? "/700" : "/400") + "/5"; // Stretch.Normal
            return key;
        }
#endif
#if WPF
        internal static string ComputeKey(WpfGlyphTypeface wpfGlyphTypeface)
        {
            string name1 = wpfGlyphTypeface.DesignerNames[FontHelper.CultureInfoEnUs];
            string faceName = wpfGlyphTypeface.FaceNames[FontHelper.CultureInfoEnUs];
            string familyName = wpfGlyphTypeface.FamilyNames[FontHelper.CultureInfoEnUs];
            string name4 = wpfGlyphTypeface.ManufacturerNames[FontHelper.CultureInfoEnUs];
            string name5 = wpfGlyphTypeface.Win32FaceNames[FontHelper.CultureInfoEnUs];
            string name6 = wpfGlyphTypeface.Win32FamilyNames[FontHelper.CultureInfoEnUs];

            string name = familyName.ToLower() + '/' + faceName.ToLowerInvariant();
            string style = wpfGlyphTypeface.Style.ToString();
            string weight = wpfGlyphTypeface.Weight.ToString();
            string stretch = wpfGlyphTypeface.Stretch.ToString();
            string simulations = wpfGlyphTypeface.StyleSimulations.ToString();

            //string key = name + '/' + style + '/' + weight + '/' + stretch + '/' + simulations;

            // Consider using StringBuilder.
            string key = (KeyPrefix
                    + name
                    + '/'
                    + style[..1]
                    + '/'
                    + wpfGlyphTypeface.Weight.ToOpenTypeWeight().ToString(CultureInfo.InvariantCulture)
                    + '/'
                    + wpfGlyphTypeface.Stretch.ToOpenTypeStretch().ToString(CultureInfo.InvariantCulture)).ToLowerInvariant();
            switch (wpfGlyphTypeface.StyleSimulations)
            {
                case WpfStyleSimulations.BoldSimulation:
                    return key + "|b+/i-";
                case WpfStyleSimulations.ItalicSimulation:
                    return key + "|b-/i+";
                case WpfStyleSimulations.BoldItalicSimulation:
                    return key + "|b+/i+";
                case WpfStyleSimulations.None:
                    break;
            }
            return key;
        }
#endif

        internal string Key { get; }

#if GDI
        internal GdiFont GdiFont => _gdiFont ?? NRT.ThrowOnNull<GdiFont>();

        readonly GdiFont? _gdiFont;
#endif

#if WPF
        internal WpfTypeface? WpfTypeface { get; }

        internal WpfGlyphTypeface? WpfGlyphTypeface { get; }
#endif

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        internal string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
            => Invariant($"{FamilyName} - {StyleName} ({FaceName})");
    }
}
