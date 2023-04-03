// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Windows
{
    /// <summary>
    /// Defines a zoom factor used in the preview control.
    /// </summary>
    public enum Zoom
    {
        /// <summary>
        /// The smallest possible zoom factor.
        /// </summary>
        Mininum = 10,

        /// <summary>
        /// The largest possible zoom factor.
        /// </summary>
        Maximum = 800,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent800 = 800,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent600 = 600,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent400 = 400,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent200 = 200,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent150 = 150,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent100 = 100,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent75 = 75,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent50 = 50,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent25 = 25,

        /// <summary>
        /// A pre-defined zoom factor.
        /// </summary>
        Percent10 = 10,

        /// <summary>
        /// Sets the percent value such that the document fits horizontally into the window.
        /// </summary>
        BestFit = -1,

        /// <summary>
        /// Sets the percent value such that the printable area of the document fits horizontally into the window.
        /// Currently not yet implemented and the same as ZoomBestFit.
        /// </summary>
        TextFit = -2,

        /// <summary>
        /// Sets the percent value such that the whole document fits completely into the window.
        /// </summary>
        FullPage = -3,

        /// <summary>
        /// Sets the percent value such that the document is displayed in its real physical size.
        /// </summary>
        OriginalSize = -4,
    }
}