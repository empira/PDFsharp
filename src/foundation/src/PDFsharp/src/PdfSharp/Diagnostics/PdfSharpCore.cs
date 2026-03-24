// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Fonts;
using PdfSharp.Internal.OpenType;
using PdfSharp.Logging;

namespace PdfSharp.Diagnostics
{
    /// <summary>
    /// A helper class for central configuration.
    /// </summary>
    public static class PdfSharpCore
    {
        /// <summary>
        /// Resets PDFsharp to a state equivalent to the state after
        /// the assemblies are loaded.
        /// Intended for unit testing only.
        /// </summary>
        public static void ResetAll()
        {
            Capabilities.ResetAll();
            GlobalFontSettings.ResetAll();
            PdfSharpLogHost.ResetLogging();
            PsGlobals.RecreatePsGlobals();
            OtGlobals.RecreateOtGlobals();

            //if (FontFactory.HasFontSources)
            //    throw new InvalidOperationException("Internal error.");
        }

        /// <summary>
        /// Resets the font management equivalent to the state after
        /// the assemblies are loaded.
        /// Intended for unit testing only.
        /// </summary>
        public static void ResetFontManagement()
        {
            GlobalFontSettings.ResetAll(true);
            PsGlobals.Global.IncrementVersion();
        }
    }
}
