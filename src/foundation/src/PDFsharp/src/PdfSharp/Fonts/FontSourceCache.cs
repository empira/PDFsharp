// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Internal.OpenType;
using PdfSharp.Internal.Threading;
using System.Collections.Concurrent;
#if GDI
using GdiFontFamily = System.Drawing.FontFamily;
#endif
#if WPF
using WpfFontFamily = System.Windows.Media.FontFamily;
#endif

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Global cache of all internal font family objects.
    /// </summary>
    class PsFontSourceCache
    {
        internal PsFontSourceCache(PsGlobals.PsFontStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        public XFontSource GetFontSourceByFontName(string uniqueFontSourceName)
        {
            if (_storage.FontSourcesByName.TryGetValue(uniqueFontSourceName, out var fontSource))
                return fontSource;

            Debug.Assert(false, $"An XFontSource with the name '{uniqueFontSourceName}' does not exist.");
            return null;
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        public XFontSource GetFontSourceByTypefaceKey(string typefaceKey)
        {
            if (_storage.FontSourcesByName.TryGetValue(typefaceKey, out var fontSource))
                return fontSource;

            Debug.Assert(false, $"An XFontSource with the typeface key '{typefaceKey}' does not exist.");
            return null;
        }

        public bool TryGetFontSourceByKey(ulong key, [MaybeNullWhen(false)] out XFontSource fontSource)
        {
            return _storage.FontSourcesByKey.TryGetValue(key, out fontSource);
        }

        public bool TryGetFontSourceByTypefaceKey(string typefaceKey, [MaybeNullWhen(false)] out XFontSource source)
        {
            return _storage.FontSourcesByName.TryGetValue(typefaceKey, out source);
        }

        /// <summary>
        /// Caches a font source under its face name and its key.
        /// </summary>
        public XFontSource CacheFontSource(XFontSource fontSource)
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
                    // Create OpenType font face for this font source.
                    fontSource.OTFontFace = OpenTypeFontFace.GetOrCreateFrom(fontSource.OTFontSource);
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
        public XFontSource CacheNewFontSource(string typefaceKey, XFontSource fontSource)
        {
            // Debug.Assert(!FontSourcesByFaceName.ContainsKey(fontSource.FaceName));

            // Check whether an identical font source with a different face name already exists.
            if (_storage.FontSourcesByKey.TryGetValue(fontSource.ChecksumKey, out var existingFontSource))
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

            OpenTypeFontFace otFontFace = fontSource.OTFontFace;
            if (Equals(otFontFace, null))
            {
                otFontFace = OpenTypeFontFace.GetOrCreateFrom(fontSource.OTFontSource);
                fontSource.OTFontFace = otFontFace;  // Also sets the font name in fontSource
            }

            _storage.FontSourcesByName.TryAdd(typefaceKey, fontSource);
            _storage.FontSourcesByName.TryAdd(fontSource.FontFaceKey, fontSource);
            _storage.FontSourcesByKey.TryAdd(fontSource.ChecksumKey, fontSource);

            return fontSource;
        }

        /// <summary>
        /// Gets a value indicating whether at least one font source was created.
        /// </summary>
        public bool HasFontSources => _storage.FontSourcesByName.Count > 0;


        readonly PsGlobals.PsFontStorage _storage;

        //internal static string GetCacheState()
        //{
        //    var state = new StringBuilder();
        //    state.Append("====================\n");
        //    state.Append("Font families by name\n");
        //    var familyKeys = PsGlobals.Global.Fonts.FontFamiliesByName.Keys;
        //    int count = familyKeys.Count;
        //    string[] keys = new string[count];
        //    familyKeys.CopyTo(keys, 0);
        //    Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
        //    foreach (string key in keys)
        //        state.AppendFormat("  {0}: {1}\n", key, _storage.FontFamiliesByName[key].DebuggerDisplay);
        //    state.Append("\n");
        //    return state.ToString();
        //}
    }
}

namespace PdfSharp.Internal
{
    partial class PsGlobals
    {
        partial class PsFontStorage
        {
            /// <summary>
            /// Maps font source key (FSK) to font source.
            /// The key is a simple hash code of the font face data.
            /// </summary>
            public readonly ConcurrentDictionary<ulong, XFontSource> FontSourcesByKey = [];

            /// <summary>
            /// Maps typeface key or font name to font source.
            /// </summary>
            public readonly ConcurrentDictionary<string, XFontSource> FontSourcesByName = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
