// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PdfSharp.Drawing;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Global table of all glyph typefaces.
    /// </summary>
    static class GlyphTypefaceCache
    {
        public static bool TryGetGlyphTypeface(string key, [MaybeNullWhen(false)] out XGlyphTypeface glyphTypeface)
        {
            return Globals.Global.Fonts.GlyphTypefacesByKey.TryGetValue(key, out glyphTypeface);
        }

        public static void AddGlyphTypeface(XGlyphTypeface glyphTypeface)
        {
            Debug.Assert(!Globals.Global.Fonts.GlyphTypefacesByKey.ContainsKey(glyphTypeface.Key));
            Globals.Global.Fonts.GlyphTypefacesByKey.TryAdd(glyphTypeface.Key, glyphTypeface);
        }

        internal static void Reset()
        {
            Globals.Global.Fonts.GlyphTypefacesByKey.Clear();
        }

        internal static string GetCacheState()
        {
            var state = new StringBuilder();
            state.Append("====================\n");
            state.Append("Glyph typefaces by name\n");
            var familyKeys = Globals.Global.Fonts.GlyphTypefacesByKey.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.Fonts.GlyphTypefacesByKey[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }
    }
}

namespace PdfSharp.Internal
{
    partial class Globals
    {
        partial class FontStorage
        {
            /// <summary>
            /// Maps typeface key to glyph typeface.
            /// </summary>
            public readonly ConcurrentDictionary<string, XGlyphTypeface> GlyphTypefacesByKey = new(StringComparer.Ordinal);
        }
    }
}
