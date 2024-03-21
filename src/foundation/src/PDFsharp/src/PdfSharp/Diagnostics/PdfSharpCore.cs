// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;
using PdfSharp.Logging;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member because it is for internal use only.

namespace PdfSharp.Diagnostics
{
    /// <summary>
    /// A helper class that is UNDER CONSTRUCTION.
    /// DO NOT USE IT.
    /// </summary>
    public static class PdfSharpCore
    {
        public static void ResetAll()
        {
            Capabilities.ResetAll();
            GlobalFontSettings.ResetFontResolvers();
            LogHost.ResetLogging();
        }
    }
}
