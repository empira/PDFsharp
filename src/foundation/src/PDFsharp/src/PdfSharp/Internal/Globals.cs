// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// The one and only class that hold all PDFsharp global stuff.
    /// </summary>
    partial class Globals
    {
        Globals()
        {
            _version = _globalVersionCount++;
        }

        public static Globals Global => _global;

        internal void RecreateGlobals()
        {
            _global = new();
        }

        internal void IncrementVersion() => _version = _globalVersionCount++;

        /// <summary>
        /// Gets the version of this instance.
        /// </summary>
        public int Version => _version;
        /*readonly*/
        int _version;

        public readonly FontStorage Fonts = new();

        /// <summary>
        /// The global version count gives every new instance of Globals a new unique
        /// version number.
        /// </summary>
        static int _globalVersionCount;

        /// <summary>
        /// The container of all global stuff in PDFsharp.
        /// </summary>
        static Globals _global = new();

        public partial class FontStorage
        {
            // A partial class within a partial class 😨

            internal FontStorage()
            {
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

            /// <summary>
            /// Gets the version of this instance.
            /// </summary>
            public int Version => _globalFontStorageVersionCount;

            static int _globalFontStorageVersionCount;
        }
    }
}
