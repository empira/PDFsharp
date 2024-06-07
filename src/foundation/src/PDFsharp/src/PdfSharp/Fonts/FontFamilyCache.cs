// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
#if GDI
using GdiFontFamily = System.Drawing.FontFamily;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Drawing;
#endif
#if WPF
using WpfFontFamily = System.Windows.Media.FontFamily;
#endif
using PdfSharp.Internal;
using PdfSharp.Fonts;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Global cache of all internal font family objects.
    /// </summary>
    static class FontFamilyCache
    {
        public static FontFamilyInternal? GetFamilyByName(string familyName)
        {
            try
            {
                Lock.EnterFontFactory();
                Globals.Global.Fonts.FontFamiliesByName.TryGetValue(familyName, out var family);
                return family;
            }
            finally { Lock.ExitFontFactory(); }
        }

        /// <summary>
        /// Caches the font family or returns a previously cached one.
        /// </summary>
        public static FontFamilyInternal CacheOrGetFontFamily(FontFamilyInternal fontFamily)
        {
            try
            {
                Lock.EnterFontFactory();
                // Recall that a font family is uniquely identified by its case-insensitive name.
                if (Globals.Global.Fonts.FontFamiliesByName.TryGetValue(fontFamily.Name, out var existingFontFamily))
                {
#if DEBUG
                    if (fontFamily.Name == "xxx")
                        _ = typeof(int);
#endif
                    return existingFontFamily;
                }
                Globals.Global.Fonts.FontFamiliesByName.Add(fontFamily.Name, fontFamily);
                return fontFamily;
            }
            finally { Lock.ExitFontFactory(); }
        }

        internal static void Reset()
        {
            Globals.Global.Fonts.FontFamiliesByName.Clear();
        }

        internal static string GetCacheState()
        {
            var state = new StringBuilder();
            state.Append("====================\n");
            state.Append("Font families by name\n");
            Dictionary<string, FontFamilyInternal>.KeyCollection familyKeys = Globals.Global.Fonts.FontFamiliesByName.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.Fonts.FontFamiliesByName[key].DebuggerDisplay);
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
            /// Maps family name to internal font family.
            /// </summary>
            public readonly Dictionary<string, FontFamilyInternal> FontFamiliesByName = [];
        }
    }
}
