// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
//using System.Drawing;
//using System.Drawing.Drawing2D;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
#endif
#if WPF
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
using WpfStyleSimulations = System.Windows.Media.StyleSimulations;
#endif
using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
using PdfSharp.Internal;
using System.Linq;
using System.Runtime.InteropServices;

// Re-Sharper disable RedundantNameQualifier

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Default platform specific font resolving.
    /// </summary>
    public static class PlatformFontResolver
    {
        /// <summary>
        /// Resolves the typeface by generating a font resolver info.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="bold">Indicates whether a bold font is requested.</param>
        /// <param name="italic">Indicates whether an italic font is requested.</param>
        public static FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            FontFactory.CheckInvocationOfPlatformFontResolver();

            var fontResolvingOptions = new FontResolvingOptions(FontHelper.CreateStyle(bold, italic));
            return ResolveTypeface(familyName, fontResolvingOptions, XGlyphTypeface.ComputeGtfKey(familyName, fontResolvingOptions));
        }

        /// <summary>
        /// Internal implementation.
        /// </summary>
        internal static FontResolverInfo? ResolveTypeface(string familyName, FontResolvingOptions fontResolvingOptions, string typefaceKey)
        {
            // Internally we often have the typeface key already.
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeGtfKey(familyName, fontResolvingOptions);

            // The user may call ResolveTypeface anytime from anywhere, so check cache in FontFactory in the first place.
            if (FontFactory.TryGetFontResolverInfoByTypefaceKey(typefaceKey, out var fontResolverInfo))
                return fontResolverInfo;

            // Let the platform create the requested font source and save both PlatformResolverInfo
            // and XFontSource in FontFactory cache.
            // It is possible that we already have the correct font source. E.g. we already have the regular typeface in cache
            // and look now for the italic typeface, but no such font exists. In this case we get the regular font source
            // and cache it again with the italic typeface key. Furthermore, in glyph typeface style simulation for italic is set.
#if CORE
            var resolverInfo = TryCoreBuildFontResolver(familyName, fontResolvingOptions, typefaceKey);
            return resolverInfo;
#endif
#if GDI
            var fontSource = CreateFontSource(familyName, fontResolvingOptions, out var gdiFont, typefaceKey);

            if (fontSource == null || gdiFont == null )
                return null;
#endif
#if WPF
            var fontSource = CreateFontSource(familyName, fontResolvingOptions,
                out var wpfFontFamily, out var wpfTypeface, out var wpfGlyphTypeface, typefaceKey);
            
            if (fontSource == null || wpfFontFamily == null || wpfTypeface == null || wpfGlyphTypeface == null)
                return null;
#endif
#if UWP
            //GlyphTypeface wpfGlyphTypeface;
            XFontSource fontSource = null;//CreateFontSource(familyName, isBold, isItalic, out wpfGlyphTypeface, typefaceKey);
#endif
#if GDI || WPF || UWP
            // If no such font exists return null. PDFsharp will fail.
            // Re/Sharper disable once ConditionIsAlwaysTrueOrFalse because code is under construction.
            if (fontSource == null)
                return null;

            //#if (CORE || GDI) && !WPF
            //            // TODO: Support style simulation for GDI+ platform fonts.
            //            fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, false, false, gdiFont);
            //#endif
            if (fontResolvingOptions.OverrideStyleSimulations)
            {
#if GDI
                // TODO: Support style simulation for GDI+ platform fonts.
                fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, fontResolvingOptions.MustSimulateBold, fontResolvingOptions.MustSimulateItalic, gdiFont);
#endif
#if WPF
                fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, fontResolvingOptions.MustSimulateBold, fontResolvingOptions.MustSimulateItalic,
                    wpfFontFamily, wpfTypeface, wpfGlyphTypeface);
#endif
            }
            else
            {
#if GDI && !WPF
                bool mustSimulateBold = gdiFont.Bold && !fontSource.FontFace.os2.IsBold;
                bool mustSimulateItalic = gdiFont.Italic && !fontSource.FontFace.os2.IsItalic;
                fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, mustSimulateBold, mustSimulateItalic, gdiFont);
#endif
#if WPF
                // WPF knows what styles have to be simulated.
                bool mustSimulateBold = (wpfGlyphTypeface.StyleSimulations & WpfStyleSimulations.BoldSimulation) == WpfStyleSimulations.BoldSimulation;
                bool mustSimulateItalic = (wpfGlyphTypeface.StyleSimulations & WpfStyleSimulations.ItalicSimulation) == WpfStyleSimulations.ItalicSimulation;

                // Weird behavior of WPF is fixed here in case we request a bold italic typeface.
                // If only italic is available, bold is simulated based on italic.
                // If only bold is available, italic is simulated based on bold.
                // But if both bold and italic is available, italic face is used and bold is simulated.
                // The latter case is reversed here, i.e. bold face is used and italic is simulated.
                if (fontResolvingOptions.IsBoldItalic && mustSimulateBold && !mustSimulateItalic)
                {
                    // Try to get the bold typeface.
                    string typefaceKeyBold = XGlyphTypeface.ComputeGtfKey(familyName, true, false);
                    FontResolverInfo? infoBold = ResolveTypeface(familyName,
                        new FontResolvingOptions(FontHelper.CreateStyle(true, false)), typefaceKeyBold);
                    // Use it if it does not base on simulation.
                    if (infoBold is { StyleSimulations: XStyleSimulations.None })
                    {
                        // Use existing bold typeface and simulate italic.
                        fontResolverInfo = new PlatformFontResolverInfo(typefaceKeyBold, false, true,
                            wpfFontFamily, wpfTypeface, wpfGlyphTypeface);
                    }
                    else
                    {
                        // Simulate both.
                        fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, true, true,
                            wpfFontFamily, wpfTypeface, wpfGlyphTypeface);
                    }
                }
                else
                {
                    fontResolverInfo = new PlatformFontResolverInfo(typefaceKey, mustSimulateBold, mustSimulateItalic,
                        wpfFontFamily, wpfTypeface, wpfGlyphTypeface);
                }
#endif
            }

            FontFactory.CacheFontResolverInfo(typefaceKey, fontResolverInfo ?? NRT.ThrowOnNull<FontResolverInfo>());

            // Register font data under the platform specific face name.
            // Already done in CreateFontSource.
            // FontFactory.CacheNewFontSource(typefaceKey, fontSource);

            return fontResolverInfo;
#endif
        }

#if CORE
        /// <summary>
        /// Creates an XGlyphTypeface.
        /// </summary>
        internal static FontResolverInfo? TryCoreBuildFontResolver(string familyName, FontResolvingOptions fontResolvingOptions, string typefaceKey) // #RENAME
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeGtfKey(familyName, fontResolvingOptions);
            var style = fontResolvingOptions.FontStyle;

            var fontResolverInfosByName = Globals.Global.Fonts.FontResolverInfosByName;
            var fontSourcesByName = Globals.Global.Fonts.FontSourcesByName;

            // Was this typeface requested before?
            if (fontResolverInfosByName.TryGetValue(typefaceKey, out var fontResolverInfo))
            {
                return null!; //todo
            }

            var resolverInfo = CoreBuildFontResolver.ResolveTypeface(familyName, fontResolvingOptions.IsBold, fontResolvingOptions.IsItalic);
            if (resolverInfo == null)
            {
                // No font face found.
                return null;
            }

            var type = resolverInfo.GetType();
            Debug.Assert(resolverInfo is PlatformFontResolverInfo);

            FontFactory.RegisterResolverResult(CoreBuildFontResolver, familyName, fontResolvingOptions, resolverInfo, typefaceKey);

            return resolverInfo;
        }
        static readonly IFontResolver CoreBuildFontResolver = new CoreBuildFontResolver();
#endif

#if GDI
        /// <summary>
        /// Create a GDI+ font and use its handle to retrieve font data using native calls.
        /// </summary>
        internal static XFontSource? CreateFontSource(string familyName, FontResolvingOptions fontResolvingOptions, out GdiFont? font, string typefaceKey)
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeGtfKey(familyName, fontResolvingOptions);

            var gdiStyle = (GdiFontStyle)(fontResolvingOptions.FontStyle & XFontStyleEx.BoldItalic);

            // Create a 10 point GDI+ font as an exemplar.
            font = FontHelper.CreateFont(familyName, 10, gdiStyle, out var fontSource);
            if (font == null)
                return null;

            if (fontSource != null)
            {
                Debug.Assert(font != null);
                // Case: Font was created by a GDI+ private font collection.
#if true
#if DEBUG_
                XFontSource existingFontSource;
                Debug.Assert(FontFactory.TryGetFontSourceByTypefaceKey(typefaceKey, out existingFontSource) &&
                    ReferenceEquals(fontSource, existingFontSource));
#endif
#else
                // Win32 API cannot get font data from fonts created by private font collection,
                // because this is handled internally in GDI+.
                // Therefore, the font source was created when the private font is added to the private font collection.
                if (!FontFactory.TryGetFontSourceByTypefaceKey(typefaceKey, out fontSource))
                {
                    // Simplify styles.
                    // (The code is written for clarity - do not rearrange for optimization)
                    if (font.Bold && font.Italic)
                    {
                        if (FontFactory.TryGetFontSourceByTypefaceKey(XGlyphTypeface.ComputeKey(font.Name, true, false), out fontSource))
                        {
                            // Use bold font.
                            FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
                        }
                        else if (FontFactory.TryGetFontSourceByTypefaceKey(XGlyphTypeface.ComputeKey(font.Name, false, true), out fontSource))
                        {
                            // Use italic font.
                            FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
                        }
                        else if (FontFactory.TryGetFontSourceByTypefaceKey(XGlyphTypeface.ComputeKey(font.Name, false, false), out fontSource))
                        {
                            // Use regular font.
                            FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
                        }
                    }
                    else if (font.Bold || font.Italic)
                    {
                        // Use regular font.
                        if (FontFactory.TryGetFontSourceByTypefaceKey(XGlyphTypeface.ComputeKey(font.Name, false, false), out fontSource))
                        {
                            FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
                        }
                    }
                    else
                    {
                        if (FontFactory.TryGetFontSourceByTypefaceKey(XGlyphTypeface.ComputeKey(font.Name, false, false), out fontSource))
                        {
                            // Should never come here...
                            FontFactory.CacheExistingFontSourceWithNewTypefaceKey(typefaceKey, fontSource);
                        }
                    }
                }
#endif
            }
            else
            {
                // Get or create the font source and cache it under the specified typeface key.
                fontSource = XFontSource.GetOrCreateFromGdi(typefaceKey, font);
            }
            return fontSource;
        }
#endif

#if WPF
        /// <summary>
        /// Create a WPF GlyphTypeface and retrieve font data from it.
        /// </summary>
        internal static XFontSource? CreateFontSource(string familyName, FontResolvingOptions fontResolvingOptions,
            out WpfFontFamily? wpfFontFamily, out WpfTypeface? wpfTypeface, out WpfGlyphTypeface? wpfGlyphTypeface, string typefaceKey)
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeGtfKey(familyName, fontResolvingOptions);
            XFontStyleEx style = fontResolvingOptions.FontStyle;

#if DEBUG_
            if (StringComparer.OrdinalIgnoreCase.Compare(familyName, "Segoe UI Semilight") == 0
                && (style & XFontStyleEx.BoldItalic) == XFontStyleEx.Italic)
                familyName.GetType();
#endif
            wpfFontFamily = null;
            wpfTypeface = null;
            wpfGlyphTypeface = null;

            // Use WPF technique to create font data.
            // No more XPrivateFontCollection.
            //wpfTypeface = XPrivateFontCollection.TryCreateTypeface(familyName, style, out wpfFontFamily);
#if DEBUG_
            if (wpfTypeface != null)
            {
                WpfGlyphTypeface glyphTypeface;
                ICollection<WpfTypeface> list = wpfFontFamily.GetTypefaces();
                foreach (WpfTypeface tf in list)
                {
                    if (!tf.TryGetGlyphTypeface(out glyphTypeface))
                        Debug-Break.Break();
                }

                //if (!WpfTypeface.TryGetGlyphTypeface(out glyphTypeface))
                //    throw new InvalidOperationException(PSSR.CannotGetGlyphTypeface(familyName));
            }
#endif
            wpfFontFamily ??= new WpfFontFamily(familyName);

            wpfTypeface ??= FontHelper.CreateTypeface(wpfFontFamily, style);

            // Let WPF choose the right glyph typeface.
            if (!wpfTypeface.TryGetGlyphTypeface(out wpfGlyphTypeface))
                return null;

            // Get or create the font source and cache it under the specified typeface key.
            var fontSource = XFontSource.GetOrCreateFromWpf(typefaceKey, wpfGlyphTypeface);
            return fontSource;
        }
#endif

#if UWP
        internal static XFontSource CreateFontSource(string familyName, bool isBold, bool isItalic, string typefaceKey)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
