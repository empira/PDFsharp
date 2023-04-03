// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering.Windows
{
  /// <summary>
  /// Defines a zoom factor used in the preview control.
  /// </summary>
  public enum Zoom
  {
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

    //Percent800 = PdfSharp.Windows.Zoom.Percent800,
    //Percent600 = PdfSharp.Windows.Zoom.Percent600,
    //Percent400 = PdfSharp.Windows.Zoom.Percent400,
    //Percent200 = PdfSharp.Windows.Zoom.Percent200,
    //Percent150 = PdfSharp.Windows.Zoom.Percent150,
    //Percent100 = PdfSharp.Windows.Zoom.Percent100,
    //Percent75 = PdfSharp.Windows.Zoom.Percent75,
    //Percent50 = PdfSharp.Windows.Zoom.Percent50,
    //Percent25 = PdfSharp.Windows.Zoom.Percent25,
    //Percent10 = PdfSharp.Windows.Zoom.Percent10,
    //BestFit = PdfSharp.Windows.Zoom.BestFit,
    //TextFit = PdfSharp.Windows.Zoom.TextFit,
    //FullPage = PdfSharp.Windows.Zoom.FullPage,
    //OriginalSize = PdfSharp.Windows.Zoom.OriginalSize,
    //Mininum = PdfSharp.Windows.Zoom.Mininum,
    //Maximum = PdfSharp.Windows.Zoom.Maximum,
  }
}
