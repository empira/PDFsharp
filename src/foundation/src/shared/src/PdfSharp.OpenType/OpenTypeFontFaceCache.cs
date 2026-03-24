// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using PdfSharp.Internal.Threading;

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Global table of all OpenType font faces cached by their face name and check sum.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class OpenTypeFontFaceCache
    {
        internal OpenTypeFontFaceCache(OtGlobals.OtFontStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Tries to get font face by its key.
        /// </summary>
        public bool TryGetFontFace(string key,
            [MaybeNullWhen(false)]
            out OpenTypeFontFace fontFace)
        {
            try
            {
                Locks.EnterFontManagement();
                var result = _storage.FontFaceCache.TryGetValue(key, out fontFace);
                return result;
            }
            finally { Locks.ExitFontManagement(); }
        }

        /// <summary>
        /// Tries to get font face by its font source check sum.
        /// </summary>
        public bool TryGetFontFace(ulong checkSum,
            [MaybeNullWhen(false)]
            out OpenTypeFontFace fontFace)
        {
            try
            {
                Locks.EnterFontManagement();
                var result = _storage.FontFacesByCheckSum.TryGetValue(checkSum, out fontFace);
                return result;
            }
            finally { Locks.ExitFontManagement(); }
        }

        public OpenTypeFontFace AddFontFace(OpenTypeFontFace fontFace)
        {
            try
            {
                Locks.EnterFontManagement();
                if (TryGetFontFace(fontFace.FullFaceName, out var fontFaceCheck))
                {
                    if (fontFaceCheck.CheckSum != fontFace.CheckSum)
                        throw new InvalidOperationException("OpenTypeFontFace with same signature but different bytes.");
                    return fontFaceCheck;
                }
                _storage.FontFaceCache.TryAdd(fontFace.FullFaceName, fontFace);
                _storage.FontFacesByCheckSum.TryAdd(fontFace.CheckSum, fontFace);
                return fontFace;
            }
            finally { Locks.ExitFontManagement(); }
        }

        //public static void Reset()
        //{
        //    try
        //    {
        //        var fontStorage = OtGlobals.Global.OTFonts;

        //        Locks.EnterFontManagement();
        //        fontStorage.FontFaceCache.Clear();
        //        fontStorage.FontFacesByCheckSum.Clear();
        //    }
        //    finally { Locks.ExitFontManagement(); }
        //}

        readonly OtGlobals.OtFontStorage _storage;

        public string GetCacheState()
        {
            StringBuilder state = new StringBuilder();
            state.Append("====================\n");
            state.Append("OpenType font faces by name\n");
            var familyKeys = _storage.FontFaceCache.Keys;
            int count = familyKeys.Count;
            string[] keys = new string[count];
            familyKeys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                state.AppendFormat("  {0}: {1}\n", key, _storage.FontFaceCache[key].DebuggerDisplay);
            state.Append("\n");
            return state.ToString();
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        static string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
            => String.Format(CultureInfo.InvariantCulture, "Font faces: {0}", OtGlobals.Global.OTFonts.FontFaceCache.Count);
    }
}

namespace PdfSharp.Internal.OpenType
{
    partial class OtGlobals
    {
        partial class OtFontStorage
        {
            /// <summary>
            /// Maps face name to OpenType font face.
            /// </summary>
            public readonly ConcurrentDictionary<string, OpenTypeFontFace> FontFaceCache = [];

            /// <summary>
            /// Maps font source key to OpenType font face.
            /// </summary>
            public readonly ConcurrentDictionary<ulong, OpenTypeFontFace> FontFacesByCheckSum = [];

            /// <summary>
            /// Maps font descriptor key to OpenTypeFontDescriptor.
            /// TODO: Replace by FontFaceCache.
            /// </summary>
            public readonly ConcurrentDictionary<string, OpenTypeFontDescriptor> FontDescriptorCache = [];
        }
    }
}
