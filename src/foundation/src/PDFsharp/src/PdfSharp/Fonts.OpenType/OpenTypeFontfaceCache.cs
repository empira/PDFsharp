// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Internal;
using System.Diagnostics.CodeAnalysis;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Global table of all OpenType font faces cached by their face name and check sum.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    static class OpenTypeFontFaceCache
    {
        /// <summary>
        /// Tries to get font face by its key.
        /// </summary>
        public static bool TryGetFontFace(string key,
            [MaybeNullWhen(false)]
            out OpenTypeFontFace fontFace)
        {
            try
            {
                Lock.EnterFontFactory();
                var result = Globals.Global.FontFaceCache.TryGetValue(key, out fontFace);
                return result;
            }
            finally { Lock.ExitFontFactory(); }
        }

        /// <summary>
        /// Tries to get font face by its check sum.
        /// </summary>
        public static bool TryGetFontFace(ulong checkSum,
            [MaybeNullWhen(false)]
            out OpenTypeFontFace fontFace)
        {
            try
            {
                Lock.EnterFontFactory();
                var result = Globals.Global.FontFacesByCheckSum.TryGetValue(checkSum, out fontFace);
                return result;
            }
            finally { Lock.ExitFontFactory(); }
        }

        public static OpenTypeFontFace AddFontFace(OpenTypeFontFace fontFace)
        {
            try
            {
                Lock.EnterFontFactory();
                if (TryGetFontFace(fontFace.FullFaceName, out var fontFaceCheck))
                {
                    if (fontFaceCheck.CheckSum != fontFace.CheckSum)
                        throw new InvalidOperationException("OpenTypeFontFace with same signature but different bytes.");
                    return fontFaceCheck;
                }
                Globals.Global.FontFaceCache.Add(fontFace.FullFaceName, fontFace);
                Globals.Global.FontFacesByCheckSum.Add(fontFace.CheckSum, fontFace);
                return fontFace;
            }
            finally { Lock.ExitFontFactory(); }
        }

        internal static string GetCacheState()
        {
            StringBuilder state = new StringBuilder();
            state.Append("====================\n");
            state.Append("OpenType font faces by name\n");
            Dictionary<string, OpenTypeFontFace>.KeyCollection familyKeys = Globals.Global.FontFaceCache.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Globals.Global.FontFaceCache[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        static string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
            => String.Format(CultureInfo.InvariantCulture, "Font faces: {0}", Globals.Global.FontFaceCache.Count);
    }
}

namespace PdfSharp.Internal
{
    partial class Globals
    {
        /// <summary>
        /// Maps face name to OpenType font face.
        /// </summary>
        public readonly Dictionary<string, OpenTypeFontFace> FontFaceCache = [];

        /// <summary>
        /// Maps font source key to OpenType font face.
        /// </summary>
        public readonly Dictionary<ulong, OpenTypeFontFace> FontFacesByCheckSum = [];
    }
}
