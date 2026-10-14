// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Collections.Concurrent;
using PdfSharp.Internal.Threading;

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Global table of all OpenType font faces cached by their face name and check sum.
    /// </summary>
    //[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class OpenTypeFontFamilyCache
    {
        internal OpenTypeFontFamilyCache(OtGlobals.OtFontStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Tries to get font face by its family name.
        /// </summary>
        public bool TryGetFontFamily(string familyName, [MaybeNullWhen(false)] out OpenTypeFontFamily fontFamily)
        {
            try
            {
                familyName = familyName.ToLowerInvariant();
                Locks.EnterFontManagement();
                var result = _storage.FontFamilyByName.TryGetValue(familyName, out fontFamily);
                return result;
            }
            finally { Locks.ExitFontManagement(); }
        }

        public OpenTypeFontFamily AddFontFamily(OpenTypeFontFamily otFontFamily)
        {
            try
            {
                Locks.EnterFontManagement();
                if (_storage.FontFamilyByName.TryGetValue(otFontFamily.FamilyName, out var otFontFamilyCheck))
                {
                    return otFontFamilyCheck;
                }
                _storage.FontFamilyByName.TryAdd(otFontFamily.FamilyName, otFontFamily);
                return otFontFamily;
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

        ///// <summary>
        ///// Gets the DebuggerDisplayAttribute text.
        ///// </summary>
        //// ReSharper disable UnusedMember.Local
        //static string DebuggerDisplay
        //// ReSharper restore UnusedMember.Local
        //    => String.Format(CultureInfo.InvariantCulture, "Font faces: {0}", OpenTypeGlobals.Global.OTFonts.FontFaceCache.Count);
    }
}

namespace PdfSharp.Internal.OpenType
{
    partial class OtGlobals
    {
        partial class OtFontStorage
        {
            /// <summary>
            /// Maps family name to OpenType font family.
            /// </summary>
            public readonly ConcurrentDictionary<string, OpenTypeFontFamily> FontFamilyByName = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
