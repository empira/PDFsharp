// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Internal
{
    // In PDFsharp hybrid build both GDI and WPF is defined.
    // This is for development and testing only.
#if GDI && WPF
    /// <summary>
    /// Internal switch indicating what context has to be used if both GDI and WPF are defined.
    /// </summary>
    static class TargetContextHelper
    {
        public static XGraphicTargetContext TargetContext = XGraphicTargetContext.WPF;
    }
#endif
}
