// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// The one and only class that hold all PDFsharp global stuff.
    /// NYI
    /// 
    /// </summary>
    partial class Globals
    {
        Globals()
        {
            _version = _globalVersionCount++;
        }

        public static Globals Global => _global;

        void RecreateGlobals()
        {
            _global = new();
        }

        /// <summary>
        /// Gets the version of this instance.
        /// </summary>
        public int Version => _version;
        readonly int _version;

        public void CheckVersion(int version)
        {
            if (_version != version)
            {
                LogHost.Logger.LogCritical("...");
                throw new InvalidOperationException("Old instance of a object is used in a newer instance of Globals.");
            }
        }

        /// <summary>
        /// The global version count gives every new instance of Globals a new unique
        /// version number.
        /// </summary>
        static int _globalVersionCount;

        /// <summary>
        /// The container of all global stuff in PDFsharp.
        /// </summary>
        static Globals _global = new();
    }
}
