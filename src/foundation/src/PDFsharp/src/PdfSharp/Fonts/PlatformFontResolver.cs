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
//using System.Windows;
//using System.Windows.Documents;
//using System.Windows.Media;
using System.IO;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
using WpfStyleSimulations = System.Windows.Media.StyleSimulations;
#endif
using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
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
        /// <param name="isBold">Indicates whether a bold font is requested.</param>
        /// <param name="isItalic">Indicates whether an italic font is requested.</param>
        public static FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var fontResolvingOptions = new FontResolvingOptions(FontHelper.CreateStyle(isBold, isItalic));
            return ResolveTypeface(familyName, fontResolvingOptions, XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions));
        }

        /// <summary>
        /// Internal implementation.
        /// </summary>
        internal static FontResolverInfo? ResolveTypeface(string familyName, FontResolvingOptions fontResolvingOptions, string typefaceKey)
        {
            // Internally we often have the typeface key already.
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions);

            // The user may call ResolveTypeface anytime from anywhere, so check cache in FontFactory in the first place.
            if (FontFactory.TryGetFontResolverInfoByTypefaceKey(typefaceKey, out var fontResolverInfo))
                return fontResolverInfo;

            // Let the platform create the requested font source and save both PlatformResolverInfo
            // and XFontSource in FontFactory cache.
            // It is possible that we already have the correct font source. E.g. we already have the regular typeface in cache
            // and look now for the italic typeface, but no such font exists. In this case we get the regular font source
            // and cache it again with the italic typeface key. Furthermore, in glyph typeface style simulation for italic is set.
#if CORE
#if true_
            XFontSource fontSource = CreateFontSource(familyName, fontResolvingOptions,
                out XFontFamily fontFamily, out XTypeface typeface, out XGlyphTypeface glyphTypeface, typefaceKey); // #NET/CORE31
#else
            XFontSource? fontSource = null;
#endif
#endif
#if GDI && !WPF
            GdiFont gdiFont;
            XFontSource fontSource = CreateFontSource(familyName, fontResolvingOptions, out gdiFont, typefaceKey);
#endif
#if WPF
            var fontSource = CreateFontSource(familyName, fontResolvingOptions,
                out var wpfFontFamily, out var wpfTypeface, out var wpfGlyphTypeface, typefaceKey);
#endif
#if UWP
            //GlyphTypeface wpfGlyphTypeface;
            XFontSource fontSource = null;//CreateFontSource(familyName, isBold, isItalic, out wpfGlyphTypeface, typefaceKey);
#endif
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
#if GDI && !WPF
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
                bool mustSimulateBold = gdiFont.Bold && !fontSource.Fontface.os2.IsBold;
                bool mustSimulateItalic = gdiFont.Italic && !fontSource.Fontface.os2.IsItalic;
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
                    string typefaceKeyBold = XGlyphTypeface.ComputeKey(familyName, true, false);
                    FontResolverInfo? infoBold = ResolveTypeface(familyName,
                        new FontResolvingOptions(FontHelper.CreateStyle(true, false)), typefaceKeyBold);
                    // Use it if it does not base on simulation.
                    if (infoBold != null && infoBold.StyleSimulations == XStyleSimulations.None)
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
        }

#if CORE
        /// <summary>
        /// Creates an XGlyphTypeface.
        /// </summary>
        internal static XFontSource CreateFontSource(string familyName, FontResolvingOptions fontResolvingOptions,
            out XFontFamily? fontFamily, out XTypeface? typeface, out XGlyphTypeface? glyphTypeface, string typefaceKey)
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions);
            var style = fontResolvingOptions.FontStyle;

#if DEBUG_
            if (StringComparer.OrdinalIgnoreCase.Compare(familyName, "Segoe UI Semilight") == 0
                && (style & XFontStyleEx.BoldItalic) == XFontStyleEx.Italic)
                familyName.GetType();
#endif

            // Use WPF technique to create font data.
            typeface = XPrivateFontCollection.TryCreateTypeface(familyName, style, out fontFamily);
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
            fontFamily ??= new XFontFamily(familyName);

            typeface ??= FontHelper.CreateTypeface(fontFamily, (XFontStyleEx)style);

            // Let WPF choose the right glyph typeface.
            if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                throw new InvalidOperationException(PSSR.CannotGetGlyphTypeface(familyName));

            // Get or create the font source and cache it under the specified typeface key.
            var fontSource = XFontSource.GetOrCreateFromGlyphTypeface(typefaceKey, glyphTypeface);
            return fontSource;
        }
#endif

#if GDI && !WPF
        /// <summary>
        /// Create a GDI+ font and use its handle to retrieve font data using native calls.
        /// </summary>
        internal static XFontSource CreateFontSource(string familyName, FontResolvingOptions fontResolvingOptions, out GdiFont font, string typefaceKey)
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions);
#if true_
            if (familyName == "Cambria")
                Debug-Break.Break();
#endif
            GdiFontStyle gdiStyle = (GdiFontStyle)(fontResolvingOptions.FontStyle & XFontStyleEx.BoldItalic);

            // Create a 10 point GDI+ font as an exemplar.
            font = FontHelper.CreateFont(familyName, 10, gdiStyle, out var fontSource);

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
                // Therefore the font source was created when the private font is added to the private font collection.
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
        internal static XFontSource CreateFontSource(string familyName, FontResolvingOptions fontResolvingOptions,
            out WpfFontFamily wpfFontFamily, out WpfTypeface wpfTypeface, out WpfGlyphTypeface wpfGlyphTypeface, string typefaceKey)
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeKey(familyName, fontResolvingOptions);
            XFontStyleEx style = fontResolvingOptions.FontStyle;

#if DEBUG_
            if (StringComparer.OrdinalIgnoreCase.Compare(familyName, "Segoe UI Semilight") == 0
                && (style & XFontStyleEx.BoldItalic) == XFontStyleEx.Italic)
                familyName.GetType();
#endif

            // Use WPF technique to create font data.
            wpfTypeface = XPrivateFontCollection.TryCreateTypeface(familyName, style, out wpfFontFamily);
#if DEBUG__
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
                throw new InvalidOperationException(PSSR.CannotGetGlyphTypeface(familyName));

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

    /// <summary>
    /// Not yet implemented.
    /// </summary>
    /// <seealso cref="PdfSharp.Fonts.IFontResolver" />
    internal class EvenNewerFontResolver : IFontResolver
    {
        internal class/*record*/ TypefaceInfo
        {
            public string TypefaceName { get; private set; }
            public string FileName { get; private set; }
            public string LinuxFileName { get; private set; }
            public string[] LinuxSubstituteFaceNames { get; private set; }

            internal TypefaceInfo(
            string typefaceName,
            string fileName,
            string linuxFileName = "",
            params string[] linuxSubstituteFaceNames)
            {
                TypefaceName = typefaceName;
                FileName = fileName;
                LinuxFileName = linuxFileName;
                LinuxSubstituteFaceNames = linuxSubstituteFaceNames;
            }
        }

        internal static readonly List<TypefaceInfo> TypefaceInfos = new()
        {
            // ReSharper disable StringLiteralTypo

            new ("Arial", "arial", "Arial", "FreeSans"),
            new ("Arial Black", "ariblk", "Arial-Black"),
            new ("Arial Bold", "arialbd", "Arial-Bold", "FreeSansBold"),
            new ("Arial Italic", "ariali", "Arial-Italic", "FreeSansOblique"),
            new ("Arial Bold Italic", "arialbi", "Arial-BoldItalic", "FreeSansBoldOblique"),

            new ("Courier New", "cour", "Courier-Bold", "DejaVu Sans Mono", "Bitstream Vera Sans Mono", "FreeMono"),
            new ("Courier New Bold", "courbd", "CourierNew-Bold", "DejaVu Sans Mono Bold", "Bitstream Vera Sans Mono Bold", "FreeMonoBold"),
            new ("Courier New Italic", "couri", "CourierNew-Italic", "DejaVu Sans Mono Oblique", "Bitstream Vera Sans Mono Italic", "FreeMonoOblique"),
            new ("Courier New Bold Italic", "courbi", "CourierNew-BoldItalic", "DejaVu Sans Mono Bold Oblique", "Bitstream Vera Sans Mono Bold Italic", "FreeMonoBoldOblique"),

            new ("Verdana", "verdana", "Verdana", "DejaVu Sans", "Bitstream Vera Sans"),
            new ("Verdana Bold", "verdanab", "Verdana-Bold", "DejaVu Sans Bold", "Bitstream Vera Sans Bold"),
            new ("Verdana Italic", "verdanai", "Verdana-Italic", "DejaVu Sans Oblique", "Bitstream Vera Sans Italic"),
            new ("Verdana Bold Italic", "verdanaz", "Verdana-BoldItalic", "DejaVu Sans Bold Oblique", "Bitstream Vera Sans Bold Italic"),

            new ("Times New Roman", "times", "TimesNewRoman", "FreeSerif"),
            new ("Times New Roman Bold", "timesbd", "TimesNewRoman-Bold", "FreeSerifBold"),
            new ("Times New Roman Italic", "timesi", "TimesNewRoman-Italic", "FreeSerifItalic"),
            new ("Times New Roman Bold Italic", "timesbi", "TimesNewRoman-BoldItalic", "FreeSerifBoldItalic"),

            new ("Lucida Console", "lucon", "LucidaConsole", "DejaVu Sans Mono"),

            new ("Symbol", "symbol", "", "Noto Sans Symbols Regular"), // Noto Symbols may not replace exactly

            new ("Wingdings", "wingding"), // No Linux substitute

            // Linux Substitute Fonts
            // TODO Nimbus and Liberation are only readily available as OTF.

            // Ubuntu packages: fonts-dejavu fonts-dejavu-core fonts-dejavu-extra
            new ("DejaVu Sans", "DejaVuSans"),
            new ("DejaVu Sans Bold", "DejaVuSans-Bold"),
            new ("DejaVu Sans Oblique", "DejaVuSans-Oblique"),
            new ("DejaVu Sans Bold Oblique", "DejaVuSans-BoldOblique"),
            new ("DejaVu Sans Mono", "DejaVuSansMono"),
            new ("DejaVu Sans Mono Bold", "DejaVuSansMono-Bold"),
            new ("DejaVu Sans Mono Oblique", "DejaVuSansMono-Oblique"),
            new ("DejaVu Sans Mono Bold Oblique", "DejaVuSansMono-BoldOblique"),

            // Ubuntu packages: fonts-freefont-ttf
            new ("FreeSans", "FreeSans"),
            new ("FreeSansBold", "FreeSansBold"),
            new ("FreeSansOblique", "FreeSansOblique"),
            new ("FreeSansBoldOblique", "FreeSansBoldOblique"),
            new ("FreeMono", "FreeMono"),
            new ("FreeMonoBold", "FreeMonoBold"),
            new ("FreeMonoOblique", "FreeMonoOblique"),
            new ("FreeMonoBoldOblique", "FreeMonoBoldOblique"),
            new ("FreeSerif", "FreeSerif"),
            new ("FreeSerifBold", "FreeSerifBold"),
            new ("FreeSerifItalic", "FreeSerifItalic"),
            new ("FreeSerifBoldItalic", "FreeSerifBoldItalic"),

            // Ubuntu packages: ttf-bitstream-vera
            new ("Bitstream Vera Sans", "Vera"),
            new ("Bitstream Vera Sans Bold", "VeraBd"),
            new ("Bitstream Vera Sans Italic", "VeraIt"),
            new ("Bitstream Vera Sans Bold Italic", "VeraBI"),
            new ("Bitstream Vera Sans Mono", "VeraMono"),
            new ("Bitstream Vera Sans Mono Bold", "VeraMoBd"),
            new ("Bitstream Vera Sans Mono Italic", "VeraMoIt"),
            new ("Bitstream Vera Sans Mono Bold Italic", "VeraMoBI"),

            // Ubuntu packages: fonts-noto-core
            new ("Noto Sans Symbols Regular", "NotoSansSymbols-Regular"),
            new ("Noto Sans Symbols Bold", "NotoSansSymbols-Bold"),

            // ReSharper restore StringLiteralTypo
        };

        static EvenNewerFontResolver()
        {
            var fcp = Environment.GetEnvironmentVariable("FONTCONFIG_PATH");
            if (fcp is not null && !LinuxFontLocations.Contains(fcp))
                LinuxFontLocations.Add(fcp);
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var typefaces = TypefaceInfos.Where(f => f.TypefaceName.StartsWith(familyName, StringComparison.OrdinalIgnoreCase));
            var baseFamily = TypefaceInfos.FirstOrDefault();

            if (isBold)
                typefaces = typefaces.Where(f => f.TypefaceName.IndexOf("bold", StringComparison.OrdinalIgnoreCase) > 0 || f.TypefaceName.IndexOf("heavy", StringComparison.OrdinalIgnoreCase) > 0);

            if (isItalic)
                typefaces = typefaces.Where(f => f.TypefaceName.IndexOf("italic", StringComparison.OrdinalIgnoreCase) > 0 || f.TypefaceName.IndexOf("oblique", StringComparison.OrdinalIgnoreCase) > 0);

            var family = typefaces.FirstOrDefault();
            if (family is not null)
                return new FontResolverInfo(family.FileName);

            if (baseFamily is not null)
                return new FontResolverInfo(baseFamily.FileName, isBold, isItalic);

            return null;
        }

        public byte[]? GetFont(string faceName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetFontWindows(faceName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetFontLinux(faceName);

            return null;
        }

        byte[]? GetFontWindows(string faceName)
        {
            foreach (var fontLocation in WindowsFontLocations)
            {
                var filepath = Path.Combine(fontLocation, faceName + ".ttf");
                if (File.Exists(filepath))
                    return File.ReadAllBytes(filepath);
            }
            return null;
        }

        static readonly List<string> WindowsFontLocations = new()
        {
            @"C:\Windows\Fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Windows\\Fonts")
        };

        byte[]? GetFontLinux(string faceName)
        {
            // TODO Query fontconfig.
            // Fontconfig is the de facto standard for indexing and managing fonts on linux.
            // Example command that should return a full file path to FreeSansBoldOblique.ttf:
            //     fc-match -f '%{file}\n' 'FreeSans:Bold:Oblique:fontformat=TrueType' : file
            //
            // Caveat: fc-match *always* returns a "next best" match or default font, even if it's bad.
            // Caveat: some preprocessing/refactoring needed to produce a pattern fc-match can understand.
            // Caveat: fontconfig needs additional configuration to know about WSL having Windows Fonts available at /mnt/c/Windows/Fonts.
            
            foreach (var fontLocation in LinuxFontLocations)
            {
                if (!Directory.Exists(fontLocation))
                    continue;

                var fontPath = FindFileRecursive(fontLocation, faceName);
                if (fontPath is not null && File.Exists(fontPath))
                    return File.ReadAllBytes(fontPath);
            }

            return null;
        }

        static readonly List<string> LinuxFontLocations = new()
        {
            "/mnt/c/Windows/Fonts", // WSL first or substitutes will be found.
            "/usr/share/fonts",
            "/usr/share/X11/fonts",
            "/usr/X11R6/lib/X11/fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "/.fonts"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "/.local/share/fonts"),
        };

        /// <summary>
        /// Finds filename candidates recursively on Linux, as organizing fonts into arbitrary subdirectories is allowed.
        /// </summary>
        string? FindFileRecursive(string basePath, string faceName)
        {
            var filenameCandidates = FaceNameToFilenameCandidates(faceName);

            foreach (var file in Directory.GetFiles(basePath).Select(Path.GetFileName))
                foreach (var filenameCandidate in filenameCandidates)
                {
                    // Most programs treat fonts case-sensitive on Linux. We ignore case because we also target WSL.
                    if (!String.IsNullOrEmpty(file) && file.Equals(filenameCandidate, StringComparison.OrdinalIgnoreCase))
                        return Path.Combine(basePath, filenameCandidate);
                }

            // Linux allows arbitrary subdirectories for organizing fonts.
            foreach (var directory in Directory.GetDirectories(basePath).Select(Path.GetFileName))
            {
                if (String.IsNullOrEmpty(directory))
                    continue;

                var file = FindFileRecursive(Path.Combine(basePath, directory), faceName);
                if (file is not null)
                    return file;
            }

            return null;
        }

        /// <summary>
        /// Generates filename candidates for Linux systems.
        /// </summary>
        string[] FaceNameToFilenameCandidates(string faceName)
        {
            const string fileExtension = ".ttf";
            // TODO OTF Fonts are popular on Linux too.

            var candidates = new List<string>
            {
                faceName + fileExtension // We need to look for Windows face name too in case of WSL or copied files.
            };

            var family = TypefaceInfos.FirstOrDefault(f => f.FileName == faceName);
            if (family is null)
                return candidates.ToArray();

            if (!String.IsNullOrEmpty(family.LinuxFileName))
                candidates.Add(family.LinuxFileName + fileExtension);
            candidates.Add(family.TypefaceName + fileExtension);

            // Add substitute fonts as last candidates.
            foreach (var replacement in family.LinuxSubstituteFaceNames)
            {
                var replacementFamily = TypefaceInfos.FirstOrDefault(f => f.TypefaceName == replacement);
                if (replacementFamily is null)
                    continue;

                candidates.Add(replacementFamily.TypefaceName + fileExtension);
                if (!String.IsNullOrEmpty(replacementFamily.FileName))
                    candidates.Add(replacementFamily.FileName + fileExtension);
                if (!String.IsNullOrEmpty(replacementFamily.LinuxFileName))
                    candidates.Add(replacementFamily.LinuxFileName + fileExtension);
            }

            return candidates.ToArray();
        }
    }
}
