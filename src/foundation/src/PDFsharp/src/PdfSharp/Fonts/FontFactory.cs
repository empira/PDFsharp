// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
#if GDI
using System.Drawing;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
using WpfTypeface = System.Windows.Media.Typeface;
#endif
using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

// Re-Sharper disable RedundantNameQualifier

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Provides functionality to map a font face request to a physical font.
    /// </summary>
    static class FontFactory
    {
        /// <summary>
        /// Converts specified information about a required typeface into a specific font face.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="fontResolvingOptions">The font resolving options.</param>
        /// <param name="typefaceKey">Typeface key if already known by caller, null otherwise.</param>
        /// <param name="useFallbackFontResolver">Use the fallback font resolver instead of regular one.</param>
        /// <returns>
        /// Information about the typeface, or null if no typeface can be found.
        /// </returns>
        public static FontResolverInfo? ResolveTypeface(string familyName, FontResolvingOptions fontResolvingOptions, string typefaceKey, bool useFallbackFontResolver)
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeGtfKey(familyName, fontResolvingOptions);

            try
            {
                var fontResolverInfosByName = Globals.Global.Fonts.FontResolverInfosByName;
                var fontSourcesByName = Globals.Global.Fonts.FontSourcesByName;

                Lock.EnterFontFactory();
                // Was this typeface requested before?
                if (fontResolverInfosByName.TryGetValue(typefaceKey, out var fontResolverInfo))
                    return fontResolverInfo;

                // Case: This typeface was not yet resolved before.

                // Choose the font resolver to invoke.
                IFontResolver? fontResolver;
                if (useFallbackFontResolver)
                {
                    // Use fallback font resolver if one is set.
                    fontResolver = GlobalFontSettings.FallbackFontResolver;

                    if (fontResolver != null)
                    {
                        // Set a flag to prevent the case that a font resolver used as fall back font resolver
                        // illegally tries to invoke PlatformFontResolver.ResolveTypeface.
                        _fallbackFontResolverInvoked = true;
                    }
                }
                else
                {
                    // Use regular font resolver if one is set.
                    fontResolver = GlobalFontSettings.FontResolver;
                }

                if (fontResolver != null)
                {
                    // Case: Use custom font resolver.
                    fontResolverInfo = fontResolver.ResolveTypeface(familyName, fontResolvingOptions.IsBold,
                        fontResolvingOptions.IsItalic);

                    // If resolved by custom font resolver register info and font source.
                    // If the custom font resolver calls the platform font resolver registration is already done.
                    if (fontResolverInfo != null && fontResolverInfo is not PlatformFontResolverInfo)
                    {
                        RegisterResolverResult(fontResolver, familyName, fontResolvingOptions, fontResolverInfo, typefaceKey);
                    }
                }
                else
                {
                    // Case: There was no custom font resolver set.
                    // Use platform font resolver.
                    // If it was successful resolver info and font source are cached
                    // automatically by PlatformFontResolver.ResolveTypeface.
                    if (!useFallbackFontResolver)
                    {
                        fontResolverInfo = PlatformFontResolver.ResolveTypeface(familyName, fontResolvingOptions, typefaceKey);
                    }
                }

                // Return value is null if the typeface could not be resolved.
                // In this case PDFsharp throws an exception.
                return fontResolverInfo;
            }
            finally
            {
                _fallbackFontResolverInvoked = false;
                Lock.ExitFontFactory();
            }
        }
        static bool _fallbackFontResolverInvoked;

        /// <summary>
        /// Register resolver info and font source for a custom font resolver .
        /// </summary>
        /// <param name="fontResolver"></param>
        /// <param name="familyName"></param>
        /// <param name="fontResolvingOptions"></param>
        /// <param name="fontResolverInfo"></param>
        /// <param name="typefaceKey"></param>
        /// <exception cref="InvalidOperationException"></exception>
        internal static void RegisterResolverResult(IFontResolver fontResolver, string familyName, FontResolvingOptions fontResolvingOptions,
            FontResolverInfo fontResolverInfo, string typefaceKey)
        {
#if CORE && DEBUG
            // Should be true in one of the following cases.
            // a) CORE build and no custom font resolver is set.
            // b) CORE build and custom font resolver is set and CFR calls PFR as fallback.
            bool platformInfo = fontResolverInfo is PlatformFontResolverInfo;
            if (platformInfo)
                Debug.Assert(fontResolver is CoreBuildFontResolver);
#endif
            try
            {
                var fontResolverInfosByName = Globals.Global.Fonts.FontResolverInfosByName;
                var fontSourcesByName = Globals.Global.Fonts.FontSourcesByName;

                Lock.EnterFontFactory();

                // OverrideStyleSimulations is true only for internal quality tests.
                // With this code we can simulate bold and/or italic for a font face even if
                // a native font face exists. This is done only vor internal testing.
                if (fontResolvingOptions.OverrideStyleSimulations)
                {
                    // Override style simulation returned by CFR or OSFR.
                    // We do not create a new object here to preserve the fact that the font resolver info
                    // comes from a platform font resolver.
                    fontResolverInfo.MustSimulateBold = fontResolvingOptions.MustSimulateBold;
                    fontResolverInfo.MustSimulateItalic = fontResolvingOptions.MustSimulateItalic;
                    //fontResolverInfo = new FontResolverInfo(fontResolverInfo.FaceName, fontResolvingOptions.MustSimulateBold, fontResolvingOptions.MustSimulateItalic, fontResolverInfo.CollectionNumber);
                }

                // The typeface key was never resolved before, because otherwise we would not come here.
                // But it is possible that it was mapped to the same resolver info as another typeface earlier.
                string resolverInfoKey = fontResolverInfo.Key;
                if (fontResolverInfosByName.TryGetValue(resolverInfoKey, out var existingFontResolverInfo))
                {
                    // Case: A new typeface was resolved to the same info as a previous one.
                    // Discard new object and reuse previous one.
                    fontResolverInfo = existingFontResolverInfo;
                    // Associate existing resolver info with the new typeface key.
                    fontResolverInfosByName.Add(typefaceKey, fontResolverInfo);
#if DEBUG_
                    // The font source should exist.
                    Debug.Assert(fontResolverInfosByName.ContainsKey(fontResolverInfo.FaceName));
#endif
                }
                else
                {
                    // Case: No such font resolver info exists.
                    // Add typeface key to fontResolverInfosByName.
                    // Thereby resolving a typeface with the same key is not needed anymore.
                    fontResolverInfosByName.Add(typefaceKey, fontResolverInfo); // Map TFK immediately to FRI.
                    // Add resolver info key to fontResolverInfosByName.
                    // Thereby a typeface with the same resolver info as a previous one is not cached twice.
                    fontResolverInfosByName.Add(resolverInfoKey, fontResolverInfo);

                    // Create font source if not yet exists.
                    // The face name is considered to be unique. So check if it already exists.
                    // Note that different resolver infos may map to the same font face because
                    // of style simulation.
                    if (fontSourcesByName.TryGetValue(fontResolverInfo.FaceName, out _))
                    {
                        // Case: The font source exists, because a previous font resolver info comes
                        // with the same face name, but was different in style simulation flags.
                        // Nothing to do.
                    }
                    else
                    {
                        // Case: Get font from custom font resolver and create font source.
                        byte[]? bytes = fontResolver.GetFont(fontResolverInfo.FaceName);
                        if (bytes == null || bytes.Length == 0)
                        {
                            var message =
                                $"The custom font resolver '{fontResolver.GetType().FullName}' resolved for typeface '{familyName}" +
                                $"{(fontResolvingOptions.IsItalic ? " italic" : "")}{(fontResolvingOptions.IsBold ? " bold" : "")}' " +
                                $"the face name '{fontResolverInfo.FaceName}', but returned no bytes for this name. " +
                                "This is most likely a bug in your font resolver.";
                            PdfSharpLogHost.Logger.LogCritical(message);
                            throw new InvalidOperationException(message);
                        }

                        // Create a new font source if no suc one exists. It can already exist if a
                        // custom font resolver returns the same byte for more than one face name.
                        var fontSource = XFontSource.GetOrCreateFrom(bytes);

                        // Add font source's font resolver name if it is different to the face name.
                        if (String.Compare(fontResolverInfo.FaceName, fontSource.FontName,
                                StringComparison.OrdinalIgnoreCase) != 0)
                            fontSourcesByName.Add(fontResolverInfo.FaceName, fontSource);
                    }
                }
            }
            finally { Lock.ExitFontFactory(); }
        }
#if GDI
        /// <summary>
        /// Registers the font face.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static XFontSource RegisterFontFace_unused(byte[] fontBytes)
        {
            try
            {
                var fontSourcesByName = Globals.Global.Fonts.FontSourcesByName;
                var fontSourcesByKey = Globals.Global.Fonts.FontSourcesByKey;

                Lock.EnterFontFactory();
                ulong key = FontHelper.CalcChecksum(fontBytes);
                if (fontSourcesByKey.TryGetValue(key, out var fontSource))
                {
                    throw new InvalidOperationException("Font face already registered.");
                }
                fontSource = XFontSource.GetOrCreateFrom(fontBytes);
                Debug.Assert(fontSourcesByKey.ContainsKey(key));
                Debug.Assert(fontSource.FontFace != null);

                //fontSource.FontFace = new OpenTypeFontFace(fontSource);
                //FontSourcesByKey.Add(checksum, fontSource);
                //FontSourcesByFontName.Add(fontSource.FontName, fontSource);

                XGlyphTypeface glyphTypeface = new XGlyphTypeface(fontSource);
                fontSourcesByName.Add(glyphTypeface.Key, fontSource);
                GlyphTypefaceCache.AddGlyphTypeface(glyphTypeface);
                return fontSource;
            }
            finally { Lock.ExitFontFactory(); }
        }
#endif

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        public static XFontSource GetFontSourceByFontName(string fontName)
        {
            if (Globals.Global.Fonts.FontSourcesByName.TryGetValue(fontName, out var fontSource))
                return fontSource;

            Debug.Assert(false, $"An XFontSource with the name '{fontName}' does not exist.");
            return null;
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        public static XFontSource GetFontSourceByTypefaceKey(string typefaceKey)
        {
            if (Globals.Global.Fonts.FontSourcesByName.TryGetValue(typefaceKey, out var fontSource))
                return fontSource;

            Debug.Assert(false, $"An XFontSource with the typeface key '{typefaceKey}' does not exist.");
            return null;
        }

        public static bool TryGetFontSourceByKey(ulong key, [MaybeNullWhen(false)] out XFontSource fontSource)
        {
            return Globals.Global.Fonts.FontSourcesByKey.TryGetValue(key, out fontSource);
        }

        /// <summary>
        /// Gets a value indicating whether at least one font source was created.
        /// </summary>
        public static bool HasFontSources => Globals.Global.Fonts.FontSourcesByName.Count > 0;

        public static bool TryGetFontResolverInfoByTypefaceKey(string typeFaceKey, [MaybeNullWhen(false)] out FontResolverInfo info)
        {
            return Globals.Global.Fonts.FontResolverInfosByName.TryGetValue(typeFaceKey, out info);
        }

        public static bool TryGetFontSourceByTypefaceKey(string typefaceKey, [MaybeNullWhen(false)] out XFontSource source)
        {
            return Globals.Global.Fonts.FontSourcesByName.TryGetValue(typefaceKey, out source);
        }

        //public static bool TryGetFontSourceByFaceName(string faceName, out XFontSource source)
        //{
        //    return FontSourcesByName.TryGetValue(faceName, out source);
        //}

        internal static void CacheFontResolverInfo(string typefaceKey, FontResolverInfo fontResolverInfo)
        {
            var fontResolverInfosByName = Globals.Global.Fonts.FontResolverInfosByName;
            // Check whether identical font is already registered.
            if (fontResolverInfosByName.TryGetValue(typefaceKey, out _))
            {
                // Should never come here.
                throw new InvalidOperationException($"A font file with different content already exists with the specified face name '{typefaceKey}'.");
            }
            if (fontResolverInfosByName.TryGetValue(fontResolverInfo.Key, out _))
            {
                // Should never come here.
                throw new InvalidOperationException($"A font resolver already exists with the specified key '{fontResolverInfo.Key}'.");
            }
            // Add to both dictionaries.
            fontResolverInfosByName.Add(typefaceKey, fontResolverInfo);
            fontResolverInfosByName.Add(fontResolverInfo.Key, fontResolverInfo);
        }

        /// <summary>
        /// Caches a font source under its face name and its key.
        /// </summary>
        public static XFontSource CacheFontSource(XFontSource fontSource)
        {
            try
            {
                Lock.EnterFontFactory();
                // Check whether an identical font source with a different face name already exists.
                if (Globals.Global.Fonts.FontSourcesByKey.TryGetValue(fontSource.Key, out var existingFontSource))
                {
#if DEBUG
                    // Fonts have same length and check sum. Now check byte by byte identity.
                    int length = fontSource.Bytes.Length;
                    for (int idx = 0; idx < length; idx++)
                    {
                        if (existingFontSource.Bytes[idx] != fontSource.Bytes[idx])
                        {
                            //Debug.Assert(false,"Two fonts with identical checksum found.");
                            break;
                            //goto FontsAreNotIdentical;
                        }
                    }
                    Debug.Assert(existingFontSource.FontFace != null);
#endif
                    return existingFontSource;

                    //FontsAreNotIdentical:
                    //// Incredible rare case: Two different fonts have the same size and check sum.
                    //// Give the new one a new key until it do not clash with an existing one.
                    //while (FontSourcesByKey.ContainsKey(fontSource.Key))
                    //    fontSource.IncrementKey();
                }

                OpenTypeFontFace? fontFace = fontSource.FontFace;
                if (fontFace == null!)
                {
                    // Create OpenType font face for this font source.
                    fontSource.FontFace = new OpenTypeFontFace(fontSource);
                }
                Globals.Global.Fonts.FontSourcesByKey.Add(fontSource.Key, fontSource);
                Globals.Global.Fonts.FontSourcesByName.Add(fontSource.FontName, fontSource);
                return fontSource;
            }
            finally { Lock.ExitFontFactory(); }
        }

        /// <summary>
        /// Caches a font source under its face name and its key.
        /// </summary>
        public static XFontSource CacheNewFontSource(string typefaceKey, XFontSource fontSource)
        {
            // Debug.Assert(!FontSourcesByFaceName.ContainsKey(fontSource.FaceName));

            // Check whether an identical font source with a different face name already exists.
            if (Globals.Global.Fonts.FontSourcesByKey.TryGetValue(fontSource.Key, out var existingFontSource))
            {
                //// Fonts have same length and check sum. Now check byte by byte identity.
                //int length = fontSource.Bytes.Length;
                //for (int idx = 0; idx < length; idx++)
                //{
                //    if (existingFontSource.Bytes[idx] != fontSource.Bytes[idx])
                //    {
                //        goto FontsAreNotIdentical;
                //    }
                //}
                return existingFontSource;

                ////// The bytes are really identical. Register font source again with the new face name
                ////// but return the existing one to save memory.
                ////FontSourcesByFaceName.Add(fontSource.FaceName, existingFontSource);
                ////return existingFontSource;

                //FontsAreNotIdentical:
                //// Incredible rare case: Two different fonts have the same size and check sum.
                //// Give the new one a new key until it do not clash with an existing one.
                //while (FontSourcesByKey.ContainsKey(fontSource.Key))
                //    fontSource.IncrementKey();
            }

            OpenTypeFontFace fontFace = fontSource.FontFace;
            if (Equals(fontFace, null))
            {
                fontFace = new OpenTypeFontFace(fontSource);
                fontSource.FontFace = fontFace;  // Also sets the font name in fontSource
            }

            Globals.Global.Fonts.FontSourcesByName.Add(typefaceKey, fontSource);
            Globals.Global.Fonts.FontSourcesByName.Add(fontSource.FontName, fontSource);
            Globals.Global.Fonts.FontSourcesByKey.Add(fontSource.Key, fontSource);

            return fontSource;
        }

        public static void CacheExistingFontSourceWithNewTypefaceKey(string typefaceKey, XFontSource fontSource)
        {
            try
            {
                Lock.EnterFontFactory();
                Globals.Global.Fonts.FontSourcesByName.Add(typefaceKey, fontSource);
            }
            finally { Lock.ExitFontFactory(); }
        }

        public static void CheckInvocationOfPlatformFontResolver()
        {
            if (!Lock.IsFontFactoryLookTaken())
                throw new InvalidOperationException("You must not call PlatformFontResolver.ResolveTypeface if you are not calling from within a font resolver.");

            if (_fallbackFontResolverInvoked)
                throw new InvalidOperationException("You must not call PlatformFontResolver.ResolveTypeface from within a fallback font resolver");
        }

        internal static void Reset()
        {
            Globals.Global.Fonts.FontResolverInfosByName.Clear();
            Globals.Global.Fonts.FontSourcesByName.Clear();
            Globals.Global.Fonts.FontSourcesByKey.Clear();
        }

        internal static string GetFontCachesState()
        {
            var state = new StringBuilder();
            string[] keys;
            int count;

            // FontResolverInfo by name.
            state.Append("====================\n");
            state.Append("Font resolver info by name\n");
            Dictionary<string, FontResolverInfo>.KeyCollection keyCollection = Globals.Global.Fonts.FontResolverInfosByName.Keys;
            count = keyCollection.Count;
            keys = new string[count];
            keyCollection.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.Fonts.FontResolverInfosByName[key].DebuggerDisplay);
            state.Append('\n');

            // FontSource by key.
            state.Append("Font source by key and name\n");
            Dictionary<ulong, XFontSource>.KeyCollection fontSourceKeys = Globals.Global.Fonts.FontSourcesByKey.Keys;
            count = fontSourceKeys.Count;
            ulong[] ulKeys = new ulong[count];
            fontSourceKeys.CopyTo(ulKeys, 0);
            Array.Sort(ulKeys, (x, y) => x == y ? 0 : (x > y ? 1 : -1));
            foreach (ulong ul in ulKeys)
                state.AppendFormat("  {0}: {1}\n", ul, Globals.Global.Fonts.FontSourcesByKey[ul].DebuggerDisplay);
            Dictionary<string, XFontSource>.KeyCollection fontSourceNames = Globals.Global.Fonts.FontSourcesByName.Keys;
            count = fontSourceNames.Count;
            keys = new string[count];
            fontSourceNames.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.Fonts.FontSourcesByName[key].DebuggerDisplay);
            state.Append("--------------------\n\n");

            // FontFamilyInternal by name.
            state.Append(FontFamilyCache.GetCacheState());
            // XGlyphTypeface by name.
            state.Append(GlyphTypefaceCache.GetCacheState());
            // OpenTypeFontFace by name.
            state.Append(OpenTypeFontFaceCache.GetCacheState());
            return state.ToString();
        }
    }
}

namespace PdfSharp.Internal
{
    using Fonts;

    partial class Globals
    {
        partial class FontStorage
        {
            /// <summary>
            /// Maps typeface key (TFK) to font resolver info (FRI) and
            /// maps font resolver key to font resolver info.
            /// </summary>
            public readonly Dictionary<string, FontResolverInfo> FontResolverInfosByName = new(StringComparer.Ordinal);

            /// <summary>
            /// Maps typeface key or font name to font source.
            /// </summary>
            public readonly Dictionary<string, XFontSource> FontSourcesByName = new(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Maps font source key (FSK) to font source.
            /// The key is a simple hash code of the font face data.
            /// </summary>
            public readonly Dictionary<ulong, XFontSource> FontSourcesByKey = [];
        }
    }
}
