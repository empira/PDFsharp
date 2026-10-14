// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Concurrent;
using System.Text;
using PdfSharp.Internal;
using PdfSharp.Internal.Threading;
using PdfSharp.Fonts;
#if GDI
using GdiFontFamily = System.Drawing.FontFamily;
using PdfSharp.Drawing;
#endif
#if WPF
using WpfFontFamily = System.Windows.Media.FontFamily;
#endif

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Global cache of all internal font family objects.
    /// </summary>
    class PsFontFamilyCache
    {
        internal PsFontFamilyCache(PsGlobals.PsFontStorage storage)
        {
            _storage = storage;
        }

        public  FontFamilyInternal? GetFamilyByName(string familyName)
        {
            try
            {
                Locks.EnterFontManagement();
                _storage.FontFamiliesByName.TryGetValue(familyName, out var family);
                return family;
            }
            finally { Locks.ExitFontManagement(); }
        }

        /// <summary>
        /// Caches the font family or returns a previously cached one.
        /// </summary>
        public FontFamilyInternal CacheOrGetFontFamily(FontFamilyInternal fontFamily)
        {
            try
            {
                Locks.EnterFontManagement();
                // Recall that a font family is uniquely identified by its case-insensitive name.
                if (_storage.FontFamiliesByName.TryGetValue(fontFamily.Name, out var existingFontFamily))
                {
#if DEBUG
                    if (fontFamily.Name == "xxx")
                        _ = typeof(int);
#endif
                    return existingFontFamily;
                }
                _storage.FontFamiliesByName.TryAdd(fontFamily.Name, fontFamily);
                return fontFamily;
            }
            finally { Locks.ExitFontManagement(); }
        }

        //internal static void Reset()
        //{
        //    PsGlobals.Global.Fonts.FontFamiliesByName.Clear();
        //}

        readonly PsGlobals.PsFontStorage _storage;

        internal static string GetCacheState()
        {
            var state = new StringBuilder();
            state.Append("====================\n");
            state.Append("Font families by name\n");
            var familyKeys = PsGlobals.Global.Fonts.FontFamiliesByName.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, PsGlobals.Global.Fonts.FontFamiliesByName[key].DebuggerDisplay);
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
            /// Maps family name to internal font family.
            /// </summary>
            public readonly ConcurrentDictionary<string, FontFamilyInternal> FontFamiliesByName = [];
        }
    }
}
