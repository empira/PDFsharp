// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Global table of all OpenType fontfaces cached by their face name and check sum.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    class OpenTypeFontfaceCache
    {
        OpenTypeFontfaceCache()
        {
            _fontfaceCache = new Dictionary<string, OpenTypeFontface>(StringComparer.OrdinalIgnoreCase);
            _fontfacesByCheckSum = new Dictionary<ulong, OpenTypeFontface>();
        }

        /// <summary>
        /// Tries to get fontface by its key.
        /// </summary>
        public static bool TryGetFontface(string key, /*[MaybeNullWhen(false)]*/ out OpenTypeFontface fontface)
        {
            try
            {
                Lock.EnterFontFactory();
                bool result = Singleton._fontfaceCache.TryGetValue(key, out fontface);
                return result;
            }
            finally { Lock.ExitFontFactory(); }
        }

        /// <summary>
        /// Tries to get fontface by its check sum.
        /// </summary>
        public static bool TryGetFontface(ulong checkSum, /*[MaybeNullWhen(false)]*/ out OpenTypeFontface fontface)
        {
            try
            {
                Lock.EnterFontFactory();
                bool result = Singleton._fontfacesByCheckSum.TryGetValue(checkSum, out fontface);
                return result;
            }
            finally { Lock.ExitFontFactory(); }
        }

        public static OpenTypeFontface AddFontface(OpenTypeFontface fontface)
        {
            try
            {
                Lock.EnterFontFactory();
                if (TryGetFontface(fontface.FullFaceName, out var fontfaceCheck))
                {
                    if (fontfaceCheck.CheckSum != fontface.CheckSum)
                        throw new InvalidOperationException("OpenTypeFontface with same signature but different bytes.");
                    return fontfaceCheck;
                }
                Singleton._fontfaceCache.Add(fontface.FullFaceName, fontface);
                Singleton._fontfacesByCheckSum.Add(fontface.CheckSum, fontface);
                return fontface;
            }
            finally { Lock.ExitFontFactory(); }
        }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        static OpenTypeFontfaceCache Singleton
        {
            get
            {
                // ReSharper disable once InvertIf
                if (_singleton == null!)
                {
                    try
                    {
                        Lock.EnterFontFactory();
                        if (_singleton == null)
                            _singleton = new OpenTypeFontfaceCache();
                    }
                    finally { Lock.ExitFontFactory(); }
                }
                return _singleton;
            }
        }
        static volatile OpenTypeFontfaceCache? _singleton;

        internal static string GetCacheState()
        {
            StringBuilder state = new StringBuilder();
            state.Append("====================\n");
            state.Append("OpenType fontfaces by name\n");
            Dictionary<string, OpenTypeFontface>.KeyCollection familyKeys = Singleton._fontfaceCache.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, Singleton._fontfaceCache[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }

        /// <summary>
        /// Maps face name to OpenType fontface.
        /// </summary>
        readonly Dictionary<string, OpenTypeFontface> _fontfaceCache;

        /// <summary>
        /// Maps font source key to OpenType fontface.
        /// </summary>
        readonly Dictionary<ulong, OpenTypeFontface> _fontfacesByCheckSum;

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
            =>
                String.Format(CultureInfo.InvariantCulture, "Fontfaces: {0}", _fontfaceCache.Count);
    }
}
