// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering.Forms
{
    /// <summary>
    /// Defines a zoom factor used in the preview control.
    /// </summary>
    public enum Zoom
    {
#if GDI && !WPF
        /// <summary>
        /// Zoom factor 800%.
        /// </summary>
        Percent800 = PdfSharp.Forms.Zoom.Percent800,
        /// <summary>
        /// Zoom factor 600%.
        /// </summary>
        Percent600 = PdfSharp.Forms.Zoom.Percent600,
        /// <summary>
        /// Zoom factor 400%.
        /// </summary>
        Percent400 = PdfSharp.Forms.Zoom.Percent400,
        /// <summary>
        /// Zoom factor 400%.
        /// </summary>
        Percent200 = PdfSharp.Forms.Zoom.Percent200,
        /// <summary>
        /// Zoom factor 150%.
        /// </summary>
        Percent150 = PdfSharp.Forms.Zoom.Percent150,
        /// <summary>
        /// Zoom factor 100%.
        /// </summary>
        Percent100 = PdfSharp.Forms.Zoom.Percent100,
        /// <summary>
        /// Zoom factor 75%.
        /// </summary>
        Percent75 = PdfSharp.Forms.Zoom.Percent75,
        /// <summary>
        /// Zoom factor 50%.
        /// </summary>
        Percent50 = PdfSharp.Forms.Zoom.Percent50,
        /// <summary>
        /// Zoom factor 25%.
        /// </summary>
        Percent25 = PdfSharp.Forms.Zoom.Percent25,
        /// <summary>
        /// Zoom factor 10%.
        /// </summary>
        Percent10 = PdfSharp.Forms.Zoom.Percent10,
        /// <summary>
        /// Sets the zoom factor so that the document fits horizontally into the window.
        /// </summary>
        BestFit = PdfSharp.Forms.Zoom.BestFit,
        /// <summary>
        /// Sets the zoom factor so that the printable area of the document fits horizontally into the window.
        /// Currently not yet implemented and the same as ZoomBestFit.
        /// </summary>
        TextFit = PdfSharp.Forms.Zoom.TextFit,
        /// <summary>
        /// Sets the zoom factor so that the whole document fits completely into the window.
        /// </summary>
        FullPage = PdfSharp.Forms.Zoom.FullPage,
        /// <summary>
        /// Sets the zoom factor so that the document is displayed in its real physical size (based on the DPI information returned from the OS for the current monitor).
        /// </summary>
        OriginalSize = PdfSharp.Forms.Zoom.OriginalSize,
        /// <summary>
        /// The smallest possible zoom factor.
        /// </summary>
        Mininum = PdfSharp.Forms.Zoom.Mininum,
        /// <summary>
        /// The largest possible zoom factor.
        /// </summary>
        Maximum = PdfSharp.Forms.Zoom.Maximum,
#endif
#if WPF
    /// <summary>
    /// Zoom factor 800%.
    /// </summary>
    Percent800 = PdfSharp.Windows.Zoom.Percent800,
    /// <summary>
    /// Zoom factor 600%.
    /// </summary>
    Percent600 = PdfSharp.Windows.Zoom.Percent600,
    /// <summary>
    /// Zoom factor 400%.
    /// </summary>
    Percent400 = PdfSharp.Windows.Zoom.Percent400,
    /// <summary>
    /// Zoom factor 200%.
    /// </summary>
    Percent200 = PdfSharp.Windows.Zoom.Percent200,
    /// <summary>
    /// Zoom factor 150%.
    /// </summary>
    Percent150 = PdfSharp.Windows.Zoom.Percent150,
    /// <summary>
    /// Zoom factor 100%.
    /// </summary>
    Percent100 = PdfSharp.Windows.Zoom.Percent100,
    /// <summary>
    /// Zoom factor 75%.
    /// </summary>
    Percent75 = PdfSharp.Windows.Zoom.Percent75,
    /// <summary>
    /// Zoom factor 50%.
    /// </summary>
    Percent50 = PdfSharp.Windows.Zoom.Percent50,
    /// <summary>
    /// Zoom factor 25%.
    /// </summary>
    Percent25 = PdfSharp.Windows.Zoom.Percent25,
    /// <summary>
    /// Zoom factor 10%.
    /// </summary>
    Percent10 = PdfSharp.Windows.Zoom.Percent10,
    /// <summary>
    /// Sets the zoom factor so that the document fits horizontally into the window.
    /// </summary>
    BestFit = PdfSharp.Windows.Zoom.BestFit,
    /// <summary>
    /// Sets the zoom factor so that the printable area of the document fits horizontally into the window.
    /// Currently not yet implemented and the same as ZoomBestFit.
    /// </summary>
    TextFit = PdfSharp.Windows.Zoom.TextFit,
    /// <summary>
    /// Sets the zoom factor so that the whole document fits completely into the window.
    /// </summary>
    FullPage = PdfSharp.Windows.Zoom.FullPage,
    /// <summary>
    /// Sets the zoom factor so that the document is displayed in its real physical size (based on the DPI information returned from the OS for the current monitor).
    /// </summary>
    OriginalSize = PdfSharp.Windows.Zoom.OriginalSize,
    /// <summary>
    /// The smallest possible zoom factor.
    /// </summary>
    Mininum = PdfSharp.Windows.Zoom.Mininum,
    /// <summary>
    /// The largest possible zoom factor.
    /// </summary>
    Maximum = PdfSharp.Windows.Zoom.Maximum,
#endif
    }
}
