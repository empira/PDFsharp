// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
#if GDI
using System.Drawing;
using GdiFontFamily = System.Drawing.FontFamily;
#endif
#if WPF
using System.Windows.Media;
using System.Windows.Markup;
using WpfFontFamily = System.Windows.Media.FontFamily;
#endif
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;
using PdfSharp.Pdf;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Global cache of all internal font family objects.
    /// </summary>
    sealed class FontFamilyCache
    {
        FontFamilyCache()
        {
            _familiesByName = new Dictionary<string, FontFamilyInternal>(StringComparer.OrdinalIgnoreCase);
        }

        public static FontFamilyInternal? GetFamilyByName(string familyName)
        {
            try
            {
                Lock.EnterFontFactory();
                Singleton._familiesByName.TryGetValue(familyName, out var family);
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
                // Recall that a font family is uniquely identified by its case insensitive name.
                if (Singleton._familiesByName.TryGetValue(fontFamily.Name, out var existingFontFamily))
                {
#if DEBUG_
                    if (fontFamily.Name == "xxx")
                        fontFamily.GetType();
#endif
                    return existingFontFamily;
                }
                Singleton._familiesByName.Add(fontFamily.Name, fontFamily);
                return fontFamily;
            }
            finally { Lock.ExitFontFactory(); }
        }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        static FontFamilyCache Singleton
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
                            _singleton = new FontFamilyCache();
                    }
                    finally { Lock.ExitFontFactory(); }
                }
                return _singleton;
            }
        }

        static volatile FontFamilyCache? _singleton;

        internal static string GetCacheState()
        {
            var state = new StringBuilder();
            state.Append("====================\n");
            state.Append("Font families by name\n");
            Dictionary<string, FontFamilyInternal>.KeyCollection familyKeys = Singleton._familiesByName.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Singleton._familiesByName[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }

        /// <summary>
        /// Maps family name to internal font family.
        /// </summary>
        readonly Dictionary<string, FontFamilyInternal> _familiesByName;
    }
}
