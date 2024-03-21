// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Internal;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Global table of all glyph typefaces.
    /// </summary>
    static class GlyphTypefaceCache
    {
        public static bool TryGetGlyphTypeface(string key, [MaybeNullWhen(false)] out XGlyphTypeface glyphTypeface)
        {
            try
            {
                Lock.EnterFontFactory();
                bool result = Globals.Global.GlyphTypefacesByKey.TryGetValue(key, out glyphTypeface);
                return result;
            }
            finally { Lock.ExitFontFactory(); }
        }

        public static void AddGlyphTypeface(XGlyphTypeface glyphTypeface)
        {
            try
            {
                Lock.EnterFontFactory();
                Debug.Assert(!Globals.Global.GlyphTypefacesByKey.ContainsKey(glyphTypeface.Key));
                Globals.Global.GlyphTypefacesByKey.Add(glyphTypeface.Key, glyphTypeface);
            }
            finally { Lock.ExitFontFactory(); }
        }

        internal static void Reset()
        {
            Globals.Global.GlyphTypefacesByKey.Clear();
        }

        internal static string GetCacheState()
        {
            var state = new StringBuilder();
            state.Append("====================\n");
            state.Append("Glyph typefaces by name\n");
            Dictionary<string, XGlyphTypeface>.KeyCollection familyKeys = Globals.Global.GlyphTypefacesByKey.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.GlyphTypefacesByKey[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }
    }
}

namespace PdfSharp.Internal
{
    partial class Globals
    {
        /// <summary>
        /// Maps typeface key to glyph typeface.
        /// </summary>
        public readonly Dictionary<string, XGlyphTypeface> GlyphTypefacesByKey = [];
    }
}
