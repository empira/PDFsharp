// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Fonts;
using PdfSharp.Internal.OpenType;
using PdfSharp.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// The one and only class that holds all PDFsharp global stuff.
    /// </summary>
    partial class PsGlobals
    {
        PsGlobals()
        {
            IncrementVersion();
        }

        public static PsGlobals Global => _global;

        internal static void RecreatePsGlobals()
        {
            _global = new();
        }

        internal void IncrementVersion() => _version = _globalVersionCount++;

        /// <summary>
        /// Gets the version of this instance.
        /// </summary>
        public int Version => _version;
        int _version;

        public readonly PsFontStorage Fonts = new();

        /// <summary>
        /// The global version count gives every new instance of Globals a new unique
        /// version number.
        /// </summary>
        static int _globalVersionCount;

        /// <summary>
        /// The container of all global stuff in PDFsharp.
        /// By creating a new instance of Globals all caches are reset
        /// as if PDFsharp starts in a new process.
        /// </summary>
        static PsGlobals _global = new();

        public partial class PsFontStorage
        {
            // A partial class within a partial class 😨

            internal PsFontStorage()
            {
                FontSourceCache = new(this);
                GlyphTypefaceCache = new(this);
                FontDescriptorCache = new(this);
                FontFamilyCache = new(this);

                _globalFontStorageVersionCount++;
            }

            public void CheckVersion(int version)
            {
                if (_globalFontStorageVersionCount != version)
                {
                    PdfSharpLogHost.Logger.LogCritical("Your XFont object is outdated because you have reset the font management.");
                    throw new InvalidOperationException("Old instance of an object is used in a newer instance of Globals.");
                }
            }

            public readonly PsFontSourceCache FontSourceCache;
            public readonly PsGlyphTypefaceCache GlyphTypefaceCache;
            public readonly PsFontDescriptorCache FontDescriptorCache;
            public readonly PsFontFamilyCache FontFamilyCache;

            /// <summary>
            /// Gets the version of this instance.
            /// </summary>
            public int Version => _globalFontStorageVersionCount;

            static int _globalFontStorageVersionCount;
        }
    }
}
