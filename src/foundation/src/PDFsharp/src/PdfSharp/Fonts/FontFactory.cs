// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Concurrent;
using System.Text;
using PdfSharp.Internal;
using PdfSharp.Internal.Threading;
using PdfSharp.Internal.OpenType;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
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

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Provides functionality to map a typeface request to a physical font face.
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
                var fontResolverInfosByName = PsGlobals.Global.Fonts.FontResolverInfosByName;
                var fontSourcesByName = PsGlobals.Global.Fonts.FontSourcesByName;

                Locks.EnterFontManagement();
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
                Locks.ExitFontManagement();
            }
        }

        /// <summary>
        /// A flag that indicated that the fallback font resolver is invoked during a font resolution.
        /// The flag can safely be a static because it is only temporarily used during font resolving
        /// and that is always running protected by a lock.
        /// </summary>
        static bool _fallbackFontResolverInvoked; // Save to be static declaration.

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
            // a) Core build and no custom font resolver is set.
            // b) Core build and custom font resolver is set and CFR calls PFR as fallback.
            bool platformInfo = fontResolverInfo is PlatformFontResolverInfo;
            if (platformInfo)
                Debug.Assert(fontResolver is WindowsPlatformFontResolver);
#endif
            try
            {
                var fontResolverInfosByName = PsGlobals.Global.Fonts.FontResolverInfosByName;
                var fontSourcesByName = PsGlobals.Global.Fonts.FontSourcesByName;

                Locks.EnterFontManagement();

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
                    fontResolverInfosByName.TryAdd(typefaceKey, fontResolverInfo);
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
                    fontResolverInfosByName.TryAdd(typefaceKey, fontResolverInfo); // Map TFK immediately to FRI.
                    // Add resolver info key to fontResolverInfosByName.
                    // Thereby a typeface with the same resolver info as a previous one is not cached twice.
                    fontResolverInfosByName.TryAdd(resolverInfoKey, fontResolverInfo);

                    // Create font source if it does not yet exist.
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
                                "This is most likely a bug in your custom font resolver.";
                            PdfSharpLogHost.Logger.LogCritical(message);
                            throw new InvalidOperationException(message);
                        }

                        // Create a new font source if no such one exists. It can already exist if a
                        // custom font resolver returns the same bytes for more than one face name.
                        var fontSource = XFontSource.GetOrCreateFrom(bytes);

                        // Add font source’s font resolver name if it is different to the face key.
                        if (String.Compare(fontResolverInfo.FaceName, fontSource.FontFaceKey,
                                StringComparison.OrdinalIgnoreCase) != 0)
                            fontSourcesByName.TryAdd(fontResolverInfo.FaceName, fontSource);
                    }
                }
            }
            finally { Locks.ExitFontManagement(); }
        }

        // ========== FontResolverInfo ==========

        public static bool TryGetFontResolverInfoByTypefaceKey(string typeFaceKey, [MaybeNullWhen(false)] out FontResolverInfo info)
        {
            return PsGlobals.Global.Fonts.FontResolverInfosByName.TryGetValue(typeFaceKey, out info);
        }

        internal static void CacheFontResolverInfo(string typefaceKey, FontResolverInfo fontResolverInfo)
        {
            var fontResolverInfosByName = PsGlobals.Global.Fonts.FontResolverInfosByName;
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
            fontResolverInfosByName.TryAdd(typefaceKey, fontResolverInfo);
            fontResolverInfosByName.TryAdd(fontResolverInfo.Key, fontResolverInfo);
        }

        public static void CacheExistingFontSourceWithNewTypefaceKey(string typefaceKey, XFontSource fontSource)
        {
            try
            {
                Locks.EnterFontManagement();
                PsGlobals.Global.Fonts.FontSourcesByName.TryAdd(typefaceKey, fontSource);
            }
            finally { Locks.ExitFontManagement(); }
        }

        public static void CheckInvocationOfPlatformFontResolver()
        {
            if (!Locks.IsFontManagementLookTaken())
                throw new InvalidOperationException("You must not call PlatformFontResolver.ResolveTypeface if you are not calling from within a font resolver.");

            if (_fallbackFontResolverInvoked)
                throw new InvalidOperationException("You must not call PlatformFontResolver.ResolveTypeface from within a fallback font resolver");
        }

        internal static void Reset()
        {
            try
            {
                Locks.EnterFontManagement();
                PsGlobals.Global.Fonts.FontResolverInfosByName.Clear();
                PsGlobals.Global.Fonts.FontSourcesByName.Clear();
                PsGlobals.Global.Fonts.FontSourcesByKey.Clear();
#if CORE
                // Also requires a reset.
                PlatformFontResolver.Reset();
#endif
            }
            finally { Locks.ExitFontManagement(); }
        }

        internal static string GetFontCachesState()
        {
            var state = new StringBuilder();
            string[] keys;
            int count;

            // FontResolverInfo by name.
            state.Append("====================\n");
            state.Append("Font resolver info by name\n");
            var keyCollection = PsGlobals.Global.Fonts.FontResolverInfosByName.Keys;
            count = keyCollection.Count;
            keys = new string[count];
            keyCollection.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, PsGlobals.Global.Fonts.FontResolverInfosByName[key].DebuggerDisplay);
            state.Append('\n');

            // FontSource by key.
            state.Append("Font source by key and name\n");
            var fontSourceKeys = PsGlobals.Global.Fonts.FontSourcesByKey.Keys;
            count = fontSourceKeys.Count;
            ulong[] ulKeys = new ulong[count];
            fontSourceKeys.CopyTo(ulKeys, 0);
            Array.Sort(ulKeys, (x, y) => x == y ? 0 : (x > y ? 1 : -1));
            foreach (ulong ul in ulKeys)
                state.AppendFormat("  {0}: {1}\n", ul, PsGlobals.Global.Fonts.FontSourcesByKey[ul].DebuggerDisplay);
            var fontSourceNames = PsGlobals.Global.Fonts.FontSourcesByName.Keys;
            count = fontSourceNames.Count;
            keys = new string[count];
            fontSourceNames.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, PsGlobals.Global.Fonts.FontSourcesByName[key].DebuggerDisplay);
            state.Append("--------------------\n\n");

            // FontFamilyInternal by name.
            state.Append(PsFontFamilyCache.GetCacheState());
            // XGlyphTypeface by name.
            state.Append(PsGlyphTypefaceCache.GetCacheState());
            // OpenTypeFontFace by name.
            //state.Append(OpenTypeFontFaceCache.GetCacheState());
            return state.ToString();
        }
    }
}

namespace PdfSharp.Internal
{
    using Fonts;

    partial class PsGlobals
    {
        partial class PsFontStorage
        {
            /// <summary>
            /// Maps typeface key (TFK) to font resolver info (FRI) and
            /// maps font resolver key to font resolver info.
            /// </summary>
            public readonly ConcurrentDictionary<string, FontResolverInfo> FontResolverInfosByName = new(StringComparer.Ordinal);
        }
    }
}
