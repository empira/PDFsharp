// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// The class that holds all OpenType global stuff in a singleton.
    /// </summary>
    public partial class OtGlobals
    {
        OtGlobals()
        {
            IncrementVersion();
        }

        public static OtGlobals Global => _global;

        public static void RecreateOtGlobals()
        {
            _global = new();
        }

        internal void IncrementVersion() => _version = _globalVersionCount++;

        /// <summary>
        /// Gets the version of this instance.
        /// </summary>
        public int Version => _version;
        int _version;

        public readonly OtFontStorage OTFonts = new();

        /// <summary>
        /// The global version count gives every new instance of OtGlobals a new unique
        /// version number.
        /// </summary>
        static int _globalVersionCount;

        /// <summary>
        /// The container of all global stuff in PDFsharp.OpenType.
        /// By creating a new instance of OtGlobals all caches are reset
        /// as if an application starts in a new process.
        /// </summary>
        static OtGlobals _global = new();

        /// <summary>
        /// Stores font information from PdfSharp.OpenType assembly.
        /// </summary>
        public partial class OtFontStorage
        {
            internal OtFontStorage()
            {
                OpenTypeFontSourceCache = new(this);
                OpenTypeFontFamilyCache = new(this);
                OpenTypeFontFaceCache = new(this);
                OpenTypeGlyphTypefaceCache = new(this);

                // Each instance gets a unique version number.
                Version = _openTypeFontStorageVersionCount++;
            }

            public readonly OpenTypeFontSourceCache OpenTypeFontSourceCache;
            public readonly OpenTypeFontFamilyCache OpenTypeFontFamilyCache;
            public readonly OpenTypeFontFaceCache OpenTypeFontFaceCache;
            public readonly OpenTypeGlyphTypefaceCache OpenTypeGlyphTypefaceCache;

            public void CheckVersion(int version)
            {
                if (_openTypeFontStorageVersionCount != version)
                {
                    LogHost.Logger.LogCritical("Your XFont object is outdated because you have reset the font management.");
                    throw new InvalidOperationException("Old instance of an object is used in a newer instance of Globals.");
                }
            }

            /// <summary>
            /// Gets the version of this instance.
            /// </summary>
            public int Version { get; }
        }

        static int _openTypeFontStorageVersionCount;
    }
}
