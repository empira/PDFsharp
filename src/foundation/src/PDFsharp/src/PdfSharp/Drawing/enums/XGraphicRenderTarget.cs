// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Drawing
{
    ///<summary>
    /// Determines whether rendering based on GDI+ or WPF.
    /// For internal use in hybrid build only.
    /// </summary>
    enum XGraphicTargetContext
    {
        NONE = 0,

        /// <summary>
        /// Rendering does not depend on a particular technology.
        /// </summary>
        CORE = 1,

        /// <summary>
        /// Renders using GDI+.
        /// </summary>
        GDI = 2,

        /// <summary>
        /// Renders using WPF.
        /// </summary>
        WPF = 3,

        /// <summary>
        /// Universal Windows Platform.
        /// </summary>
        WUI = 10,
    }
}
