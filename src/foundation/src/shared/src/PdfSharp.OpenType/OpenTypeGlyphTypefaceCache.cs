// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Collections.Concurrent;
using System.Globalization;
using PdfSharp.Internal.Threading;

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Global table of all OpenType glyph typefaces cached by their face name and check sum.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public  class OpenTypeGlyphTypefaceCache
    {
        internal OpenTypeGlyphTypefaceCache(OtGlobals.OtFontStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Tries to get an OpenTypeGlyphTypeface by its typeface key.
        /// </summary>
        public  bool TryGetGlyphTypeface(string key,
            [MaybeNullWhen(false)]
            out OpenTypeGlyphTypeface otGlyphTypeface)
        {
            try
            {
                Locks.EnterFontManagement();
                var result = _storage.GlyphTypefaceByKey.TryGetValue(key, out otGlyphTypeface);
                return result;
            }
            finally { Locks.ExitFontManagement(); }
        }

        public  bool TryAddGlyphTypeface(OpenTypeGlyphTypeface otGlyphTypeface)
        {
            try
            {
                Locks.EnterFontManagement();

                return _storage.GlyphTypefaceByKey.TryAdd(otGlyphTypeface.Key, otGlyphTypeface);
            }
            finally { Locks.ExitFontManagement(); }
        }

        public static void Reset_todo()
        {
            try
            {
                var fontStorage = OtGlobals.Global.OTFonts;

                Locks.EnterFontManagement();
                fontStorage.FontFaceCache.Clear();
                fontStorage.FontFacesByCheckSum.Clear();
            }
            finally { Locks.ExitFontManagement(); }
        }

        readonly OtGlobals.OtFontStorage _storage;

        //public static string GetCacheState()
        //{
        //    StringBuilder state = new StringBuilder();
        //    state.Append("====================\n");
        //    state.Append("OpenType font faces by name\n");
        //    var familyKeys = OpenTypeGlobals.Global.OTFonts.FontFaceCache.Keys;
        //    int count = familyKeys.Count;
        //    string[] keys = new string[count];
        //    familyKeys.CopyTo(keys, 0);
        //    Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
        //    foreach (string key in keys)
        //        state.AppendFormat("  {0}: {1}\n", key, OpenTypeGlobals.Global.OTFonts.FontFaceCache[key].DebuggerDisplay);
        //    state.Append("\n");
        //    return state.ToString();
        //}

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        static string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
            => String.Format(CultureInfo.InvariantCulture, "Font faces: {0}", OtGlobals.Global.OTFonts.FontFaceCache.Count);
    }
}

namespace PdfSharp.Internal.OpenType
{
    partial class OtGlobals
    {
        partial class OtFontStorage
        {
            /// <summary>
            /// Maps typeface key to OpenType glyph typeface.
            /// </summary>
            public readonly ConcurrentDictionary<string, OpenTypeGlyphTypeface> GlyphTypefaceByKey = [];
        }
    }
}
