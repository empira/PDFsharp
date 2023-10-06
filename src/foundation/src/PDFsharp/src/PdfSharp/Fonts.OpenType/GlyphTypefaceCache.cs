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
    class GlyphTypefaceCache
    {
        public static bool TryGetGlyphTypeface(string key, /*[MaybeNullWhen(false)]*/ out XGlyphTypeface glyphTypeface)
        {
            try
            {
                Lock.EnterFontFactory();
                bool result = Singleton._glyphTypefacesByKey.TryGetValue(key, out glyphTypeface);
                return result;
            }
            finally { Lock.ExitFontFactory(); }
        }

        public static void AddGlyphTypeface(XGlyphTypeface glyphTypeface)
        {
            try
            {
                Lock.EnterFontFactory();
                GlyphTypefaceCache cache = Singleton;
                Debug.Assert(!cache._glyphTypefacesByKey.ContainsKey(glyphTypeface.Key));
                cache._glyphTypefacesByKey.Add(glyphTypeface.Key, glyphTypeface);
            }
            finally { Lock.ExitFontFactory(); }
        }

        internal static void Reset()
        {
            if (_singleton != null)
            {
                _singleton._glyphTypefacesByKey.Clear();
                _singleton = null;
            }

        }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        static GlyphTypefaceCache Singleton
        {
            get
            {
                // ReSharper disable once InvertIf
                if (_singleton == null)
                {
                    try
                    {
                        Lock.EnterFontFactory();
                        if (_singleton == null)
                            _singleton = new GlyphTypefaceCache();
                    }
                    finally { Lock.ExitFontFactory(); }
                }
                return _singleton;
            }
        }

        static volatile GlyphTypefaceCache? _singleton;

        internal static string GetCacheState()
        {
            var state = new StringBuilder();
            state.Append("====================\n");
            state.Append("Glyph typefaces by name\n");
            Dictionary<string, XGlyphTypeface>.KeyCollection familyKeys = Singleton._glyphTypefacesByKey.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Singleton._glyphTypefacesByKey[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }

        /// <summary>
        /// Maps typeface key to glyph typeface.
        /// </summary>
        readonly Dictionary<string, XGlyphTypeface> _glyphTypefacesByKey = new();
    }
}
