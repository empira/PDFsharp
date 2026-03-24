// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using PdfSharp.Internal.Threading;
using System.Collections.Concurrent;

// Re-Sharper disable RedundantNameQualifier

// v7.0 TODO review

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Provides functionality to map a font face request to a physical font.
    /// </summary>
    public class OpenTypeFontSourceCache
    {
        internal OpenTypeFontSourceCache(OtGlobals.OtFontStorage storage)
        {
            _storage = storage;
        }

        ///// <summary>
        ///// Gets the bytes of a physical font with specified face name.
        ///// </summary>
        //public static OpenTypeFontSource GetFontSourceByFontName(string fontName)
        //{
        //    if (OpenTypeGlobals.Global.OTFonts.FontSourcesByName.TryGetValue(fontName, out var fontSource))
        //        return fontSource;

        //    Debug.Assert(false, $"An XFontSource with the name '{fontName}' does not exist.");
        //    return null!;
        //}

        ///// <summary>
        ///// Gets the bytes of a physical font with specified face name.
        ///// </summary>
        //public static OpenTypeFontSource GetFontSourceByTypefaceKey(string typefaceKey)
        //{
        //    if (OpenTypeGlobals.Global.OTFonts.FontSourcesByName.TryGetValue(typefaceKey, out var fontSource))
        //        return fontSource;

        //    Debug.Assert(false, $"An XFontSource with the typeface key '{typefaceKey}' does not exist.");
        //    return null!;
        //}

        public bool TryGetFontSourceByKey(ulong key, [MaybeNullWhen(false)] out OpenTypeFontSource fontSource)
        {
            return _storage.FontSourcesByKey.TryGetValue(key, out fontSource);
        }

        /// <summary>
        /// Gets a value indicating whether at least one font source was created.
        /// </summary>
        public bool HasFontSources => _storage.FontSourcesByName.Count > 0;

        /// <summary>
        /// Caches a font source under its face name and its key.
        /// </summary>
        public OpenTypeFontSource CacheFontSource(OpenTypeFontSource fontSource)
        {
            try
            {
                Locks.EnterFontManagement();
                // Check whether an identical font source with a different face name already exists.
                if (_storage.FontSourcesByKey.TryGetValue(fontSource.ChecksumKey, out var existingFontSource))
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
                    Debug.Assert(existingFontSource.OTFontFace != null);
#endif
                    return existingFontSource;

                    //FontsAreNotIdentical:
                    //// Incredible rare case: Two different fonts have the same size and check sum.
                    //// Give the new one a new key until it do not clash with an existing one.
                    //while (FontSourcesByKey.ContainsKey(fontSource.Key))
                    //    fontSource.IncrementKey();
                }

                OpenTypeFontFace? fontFace = fontSource.OTFontFace;
                if (fontFace == null!)
                {
                    // Case: The OpenTypeFontSource has no OpenTypeFontFace.
                    // The font face cannot be in a cache, because the font source would be in the 
                    // font source cache in the first place. So create and cache a new OpenTypeFontFace.
                    fontSource.OTFontFace = OpenTypeFontFace.GetOrCreateFrom(fontSource);
                }
                _storage.FontSourcesByKey.TryAdd(fontSource.ChecksumKey, fontSource);
                _storage.FontSourcesByName.TryAdd(fontSource.FontFaceKey, fontSource);
                return fontSource;
            }
            finally { Locks.ExitFontManagement(); }
        }

        /// <summary>
        /// Caches a font source under its face name and its key.
        /// </summary>
        public  OpenTypeFontSource CacheNewFontSource(string typefaceKey, OpenTypeFontSource otFontSource)
        {
            // Debug.Assert(!FontSourcesByFaceName.ContainsKey(otFontSource.FaceName));

            // Check whether an identical font source with a different face name already exists.
            if (_storage.FontSourcesByKey.TryGetValue(otFontSource.ChecksumKey, out var existingFontSource))
            {
                //// Fonts have same length and check sum. Now check byte by byte identity.
                //int length = otFontSource.Bytes.Length;
                //for (int idx = 0; idx < length; idx++)
                //{
                //    if (existingFontSource.Bytes[idx] != otFontSource.Bytes[idx])
                //    {
                //        goto FontsAreNotIdentical;
                //    }
                //}
                return existingFontSource;

                ////// The bytes are really identical. Register font source again with the new face name
                ////// but return the existing one to save memory.
                ////FontSourcesByFaceName.Add(otFontSource.FaceName, existingFontSource);
                ////return existingFontSource;

                //FontsAreNotIdentical:
                //// Incredible rare case: Two different fonts have the same size and check sum.
                //// Give the new one a new key until it do not clash with an existing one.
                //while (FontSourcesByKey.ContainsKey(otFontSource.Key))
                //    otFontSource.IncrementKey();
            }

            OpenTypeFontFace fontFace = otFontSource.OTFontFace;
            if (fontFace == null!)
            {
                fontFace = OpenTypeFontFace.GetOrCreateFrom(otFontSource);
                otFontSource.OTFontFace = fontFace;  // Also sets the font name in otFontSource
            }

            OtGlobals.Global.OTFonts.FontSourcesByName.TryAdd(typefaceKey, otFontSource);
            OtGlobals.Global.OTFonts.FontSourcesByName.TryAdd(otFontSource.FontFaceKey, otFontSource);
            OtGlobals.Global.OTFonts.FontSourcesByKey.TryAdd(otFontSource.ChecksumKey, otFontSource);

            return otFontSource;
        }

        public  void CacheExistingFontSourceWithNewTypefaceKey(string typefaceKey, OpenTypeFontSource fontSource)
        {
            try
            {
                Locks.EnterFontManagement();
                _storage.FontSourcesByName.TryAdd(typefaceKey, fontSource);
            }
            finally { Locks.ExitFontManagement(); }
        }

        //internal static void Reset_TODO()
        //{
        //    try
        //    {
        //        OtGlobals.Global.OTFonts.FontSourcesByName.Clear();
        //        OtGlobals.Global.OTFonts.FontSourcesByKey.Clear();
        //    }
        //    finally { Locks.ExitFontManagement(); }
        //}

        readonly OtGlobals.OtFontStorage _storage;

        internal static string GetFontCachesState()
        {
            return "Temporarily out of order";
            //var state = new StringBuilder();
            //string[] keys;
            //int count;

            //// FontResolverInfo by name.
            //state.Append("====================\n");
            //state.Append("Font resolver info by name\n");
            //Dictionary<string, FontResolverInfo>.KeyCollection keyCollection = Globals.Global.Fonts.FontResolverInfosByName.Keys;
            //count = keyCollection.Count;
            //keys = new string[count];
            //keyCollection.CopyTo(keys, 0);
            //Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            //foreach (string key in keys)
            //    state.AppendFormat("  {0}: {1}\n", key, Globals.Global.Fonts.FontResolverInfosByName[key].DebuggerDisplay);
            //state.Append('\n');

            //// FontSource by key.
            //state.Append("Font source by key and name\n");
            //Dictionary<ulong, XFontSource>.KeyCollection fontSourceKeys = Globals.Global.Fonts.FontSourcesByKey.Keys;
            //count = fontSourceKeys.Count;
            //ulong[] ulKeys = new ulong[count];
            //fontSourceKeys.CopyTo(ulKeys, 0);
            //Array.Sort(ulKeys, (x, y) => x == y ? 0 : (x > y ? 1 : -1));
            //foreach (ulong ul in ulKeys)
            //    state.AppendFormat("  {0}: {1}\n", ul, Globals.Global.Fonts.FontSourcesByKey[ul].DebuggerDisplay);
            //Dictionary<string, XFontSource>.KeyCollection fontSourceNames = Globals.Global.Fonts.FontSourcesByName.Keys;
            //count = fontSourceNames.Count;
            //keys = new string[count];
            //fontSourceNames.CopyTo(keys, 0);
            //Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            //foreach (string key in keys)
            //    state.AppendFormat("  {0}: {1}\n", key, Globals.Global.Fonts.FontSourcesByName[key].DebuggerDisplay);
            //state.Append("--------------------\n\n");

            //// FontFamilyInternal by name.
            //state.Append(FontFamilyCache.GetCacheState());
            //// XGlyphTypeface by name.
            //state.Append(GlyphTypefaceCache.GetCacheState());
            //// OpenTypeFontFace by name.
            //state.Append(OpenTypeFontFaceCache.GetCacheState());
            //return state.ToString();
        }
    }
}

namespace PdfSharp.Internal.OpenType
{
    partial class OtGlobals
    {
        partial class OtFontStorage
        {
            /// <summary>
            /// Maps typeface key or font name to font source.
            /// </summary>
            public readonly ConcurrentDictionary<string, OpenTypeFontSource> FontSourcesByName =
                new(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Maps font source key (FSK) to font source.
            /// The key is a simple hash code of the font face data.
            /// </summary>
            public readonly ConcurrentDictionary<ulong, OpenTypeFontSource> FontSourcesByKey = [];
        }
    }
}