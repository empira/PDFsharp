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

// Re-Sharper disable RedundantNameQualifier

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Provides functionality to map a font face request to a physical font.
    /// </summary>
    static class FontFactory
    {
        /// <summary>
        /// Converts specified information about a required typeface into a specific font.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="fontResolvingOptions">The font resolving options.</param>
        /// <param name="typefaceKey">Typeface key if already known by caller, null otherwise.</param>
        /// <returns>
        /// Information about the typeface, or null if no typeface can be found.
        /// </returns>
        public static FontResolverInfo? ResolveTypeface(string familyName, FontResolvingOptions fontResolvingOptions, string typefaceKey)
        {
            if (String.IsNullOrEmpty(typefaceKey))
                typefaceKey = XGlyphTypeface.ComputeGtfKey(familyName, fontResolvingOptions);

            try
            {
                var fontResolverInfosByName = Globals.Global.FontResolverInfosByName;
                var fontSourcesByName = Globals.Global.FontSourcesByName;

                Lock.EnterFontFactory();
                // Was this typeface requested before?
                if (fontResolverInfosByName.TryGetValue(typefaceKey, out var fontResolverInfo))
                    return fontResolverInfo;

                // Case: This typeface was not yet resolved before.

                // Is there a custom font resolver available?
                var customFontResolver = GlobalFontSettings.FontResolver;
                if (customFontResolver != null)
                {
                    // Case: Use custom font resolver.
                    fontResolverInfo = customFontResolver.ResolveTypeface(familyName, fontResolvingOptions.IsBold, fontResolvingOptions.IsItalic);

                    // If resolved by custom font resolver register info and font source.
                    if (fontResolverInfo != null && fontResolverInfo is not PlatformFontResolverInfo)
                    {
                        // OverrideStyleSimulations is true only for internal quality tests.
                        if (fontResolvingOptions.OverrideStyleSimulations)
                        {
                            // Override style simulation returned by custom font resolver.
                            fontResolverInfo = new FontResolverInfo(fontResolverInfo.FaceName, fontResolvingOptions.MustSimulateBold, fontResolvingOptions.MustSimulateItalic, fontResolverInfo.CollectionNumber);
                        }

                        string resolverInfoKey = fontResolverInfo.Key;
                        if (fontResolverInfosByName.TryGetValue(resolverInfoKey, out var existingFontResolverInfo))
                        {
                            // Case: A new typeface was resolved with the same info as a previous one.
                            // Discard new object and reuse previous one.
                            fontResolverInfo = existingFontResolverInfo;
                            // Associate with typeface key.
                            fontResolverInfosByName.Add(typefaceKey, fontResolverInfo);
#if DEBUG_
                            // The font source should exist.
                            Debug.Assert(fontResolverInfosByName.ContainsKey(fontResolverInfo.FaceName));
#endif
                        }
                        else
                        {
                            // Case: No such font resolver info exists.
                            // Add to both dictionaries.
                            fontResolverInfosByName.Add(typefaceKey, fontResolverInfo);
                            Debug.Assert(resolverInfoKey == fontResolverInfo.Key);
                            fontResolverInfosByName.Add(resolverInfoKey, fontResolverInfo);

                            // Create font source if not yet exists.
                            if (fontSourcesByName.TryGetValue(fontResolverInfo.FaceName, out _))
                            {
                                // Case: The font source exists, because a previous font resolver info comes
                                // with the same face name, but was different in style simulation flags.
                                // Nothing to do.
                            }
                            else
                            {
                                // Case: Get font from custom font resolver and create font source.
                                byte[] bytes = customFontResolver.GetFont(fontResolverInfo.FaceName) ?? NRT.ThrowOnNull<byte[]>();
                                var fontSource = XFontSource.GetOrCreateFrom(bytes);

                                // Add font source's font resolver name if it is different to the face name.
                                if (string.Compare(fontResolverInfo.FaceName, fontSource.FontName, StringComparison.OrdinalIgnoreCase) != 0)
                                    fontSourcesByName.Add(fontResolverInfo.FaceName, fontSource);
                            }
                        }
                    }
                }
                else
                {
                    // Case: There was no custom font resolver set.
                    // Use platform font resolver.
                    // If it was successful resolver info and font source are cached
                    // automatically by PlatformFontResolver.ResolveTypeface.
                    fontResolverInfo = PlatformFontResolver.ResolveTypeface(familyName, fontResolvingOptions, typefaceKey);
                }

                // Return value is null if the typeface could not be resolved.
                // In this case PDFsharp stops.
                return fontResolverInfo;
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
                var fontSourcesByName = Globals.Global.FontSourcesByName;
                var fontSourcesByKey = Globals.Global.FontSourcesByKey;

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
            if (Globals.Global.FontSourcesByName.TryGetValue(fontName, out var fontSource))
                return fontSource;

            Debug.Assert(false, $"An XFontSource with the name '{fontName}' does not exist.");
            return null;
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        public static XFontSource GetFontSourceByTypefaceKey(string typefaceKey)
        {
            if (Globals.Global.FontSourcesByName.TryGetValue(typefaceKey, out var fontSource))
                return fontSource;

            Debug.Assert(false, $"An XFontSource with the typeface key '{typefaceKey}' does not exist.");
            return null;
        }

        public static bool TryGetFontSourceByKey(ulong key, [MaybeNullWhen(false)] out XFontSource fontSource)
        {
            return Globals.Global.FontSourcesByKey.TryGetValue(key, out fontSource);
        }

        /// <summary>
        /// Gets a value indicating whether at least one font source was created.
        /// </summary>
        public static bool HasFontSources => Globals.Global.FontSourcesByName.Count > 0;

        public static bool TryGetFontResolverInfoByTypefaceKey(string typeFaceKey, [MaybeNullWhen(false)] out FontResolverInfo info)
        {
            return Globals.Global.FontResolverInfosByName.TryGetValue(typeFaceKey, out info);
        }

        public static bool TryGetFontSourceByTypefaceKey(string typefaceKey, [MaybeNullWhen(false)] out XFontSource source)
        {
            return Globals.Global.FontSourcesByName.TryGetValue(typefaceKey, out source);
        }

        //public static bool TryGetFontSourceByFaceName(string faceName, out XFontSource source)
        //{
        //    return FontSourcesByName.TryGetValue(faceName, out source);
        //}

        internal static void CacheFontResolverInfo(string typefaceKey, FontResolverInfo fontResolverInfo)
        {
            var fontResolverInfosByName = Globals.Global.FontResolverInfosByName;
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
                if (Globals.Global.FontSourcesByKey.TryGetValue(fontSource.Key, out var existingFontSource))
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
                Globals.Global.FontSourcesByKey.Add(fontSource.Key, fontSource);
                Globals.Global.FontSourcesByName.Add(fontSource.FontName, fontSource);
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
            if (Globals.Global.FontSourcesByKey.TryGetValue(fontSource.Key, out var existingFontSource))
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
            if (fontFace == null!)
            {
                fontFace = new OpenTypeFontFace(fontSource);
                fontSource.FontFace = fontFace;  // Also sets the font name in fontSource
            }

            Globals.Global.FontSourcesByName.Add(typefaceKey, fontSource);
            Globals.Global.FontSourcesByName.Add(fontSource.FontName, fontSource);
            Globals.Global.FontSourcesByKey.Add(fontSource.Key, fontSource);

            return fontSource;
        }

        public static void CacheExistingFontSourceWithNewTypefaceKey(string typefaceKey, XFontSource fontSource)
        {
            try
            {
                Lock.EnterFontFactory();
                Globals.Global.FontSourcesByName.Add(typefaceKey, fontSource);
            }
            finally { Lock.ExitFontFactory(); }
        }

        internal static void Reset()
        {
            Globals.Global.FontResolverInfosByName.Clear();
            Globals.Global.FontSourcesByName.Clear();
            Globals.Global.FontSourcesByKey.Clear();
        }

        internal static string GetFontCachesState()
        {
            var state = new StringBuilder();
            string[] keys;
            int count;

            // FontResolverInfo by name.
            state.Append("====================\n");
            state.Append("Font resolver info by name\n");
            Dictionary<string, FontResolverInfo>.KeyCollection keyCollection = Globals.Global.FontResolverInfosByName.Keys;
            count = keyCollection.Count;
            keys = new string[count];
            keyCollection.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.FontResolverInfosByName[key].DebuggerDisplay);
            state.Append('\n');

            // FontSource by key.
            state.Append("Font source by key and name\n");
            Dictionary<ulong, XFontSource>.KeyCollection fontSourceKeys = Globals.Global.FontSourcesByKey.Keys;
            count = fontSourceKeys.Count;
            ulong[] ulKeys = new ulong[count];
            fontSourceKeys.CopyTo(ulKeys, 0);
            Array.Sort(ulKeys, (x, y) => x == y ? 0 : (x > y ? 1 : -1));
            foreach (ulong ul in ulKeys)
                state.AppendFormat("  {0}: {1}\n", ul, Globals.Global.FontSourcesByKey[ul].DebuggerDisplay);
            Dictionary<string, XFontSource>.KeyCollection fontSourceNames = Globals.Global.FontSourcesByName.Keys;
            count = fontSourceNames.Count;
            keys = new string[count];
            fontSourceNames.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.FontSourcesByName[key].DebuggerDisplay);
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
        /// <summary>
        /// Maps font typeface key to font resolver info.
        /// </summary>
        public readonly Dictionary<string, FontResolverInfo> FontResolverInfosByName = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Maps typeface key or font name to font source.
        /// </summary>
        public readonly Dictionary<string, XFontSource> FontSourcesByName = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Maps font source key to font source.
        /// The key is a hash code of the font face data.
        /// </summary>
        public readonly Dictionary<ulong, XFontSource> FontSourcesByKey = [];
    }
}
