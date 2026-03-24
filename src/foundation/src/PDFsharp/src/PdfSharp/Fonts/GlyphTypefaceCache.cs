// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Concurrent;
using System.Text;
using PdfSharp.Internal;
using PdfSharp.Internal.Threading;
using PdfSharp.Drawing;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Global table of all glyph typefaces.
    /// </summary>
    class PsGlyphTypefaceCache
    {
        internal PsGlyphTypefaceCache(PsGlobals.PsFontStorage storage)
        {
            _storage = storage;
        }

        public bool TryGetGlyphTypeface(string key, [MaybeNullWhen(false)] out XGlyphTypeface glyphTypeface)
        {
            try
            {
                Locks.EnterFontManagement();
                bool result = _storage.GlyphTypefacesByKey.TryGetValue(key, out glyphTypeface);
                return result;
            }
            finally { Locks.ExitFontManagement(); }
        }

        public void AddGlyphTypeface(XGlyphTypeface glyphTypeface)
        {
            try
            {
                Locks.EnterFontManagement();
                Debug.Assert(!_storage.GlyphTypefacesByKey.ContainsKey(glyphTypeface.Key));
                _storage.GlyphTypefacesByKey.TryAdd(glyphTypeface.Key, glyphTypeface);
            }
            finally { Locks.ExitFontManagement(); }
        }

        readonly PsGlobals.PsFontStorage _storage;

        internal static string GetCacheState()
        {
            var state = new StringBuilder();
            state.Append("====================\n");
            state.Append("Glyph typefaces by name\n");
            var familyKeys = PsGlobals.Global.Fonts.GlyphTypefacesByKey.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, PsGlobals.Global.Fonts.GlyphTypefacesByKey[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }
    }
}

namespace PdfSharp.Internal
{
    partial class PsGlobals
    {
        partial class PsFontStorage
        {
            /// <summary>
            /// Maps typeface key to glyph typeface.
            /// </summary>
            public readonly ConcurrentDictionary<string, XGlyphTypeface> GlyphTypefacesByKey = new(StringComparer.Ordinal);
        }
    }
}
