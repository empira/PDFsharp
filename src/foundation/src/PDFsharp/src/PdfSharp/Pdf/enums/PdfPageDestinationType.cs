// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming because we use PDF names.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Specifies the type of a page destination in outline items, annotations, or actions.
    ///  </summary>
    public enum PdfPageDestinationType  // Reference: TABLE 8.2  Destination Syntax / Page 582
    {
        // Except for FitR the documentation text is outdated.

        /// <summary>
        /// Display the page with the coordinates (left, top) positioned at the upper-left corner of
        /// the window and the contents of the page magnified by the factor zoom.
        /// </summary>
        Xyz,

        /// <summary>
        /// Display the page with its contents magnified just enough to fit the 
        /// entire page within the window both horizontally and vertically.
        /// </summary>
        Fit,

        /// <summary>
        /// Display the page with the vertical coordinate top positioned at the top edge of 
        /// the window and the contents of the page magnified just enough to fit the entire
        /// width of the page within the window.
        /// </summary>
        FitH,

        /// <summary>
        /// Display the page with the horizontal coordinate left positioned at the left edge of 
        /// the window and the contents of the page magnified just enough to fit the entire
        /// height of the page within the window.
        /// </summary>
        FitV,

        /// <summary>
        /// Display the page designated by page, with its contents magnified just enough to
        /// fit the rectangle specified by the coordinates left, bottom, right, and top entirely
        /// within the window both horizontally and vertically. If the required horizontal and
        /// vertical magnification factors are different, use the smaller of the two, centering
        /// the rectangle within the window in the other dimension. A null value for any of
        /// the parameters may result in unpredictable behavior.
        /// </summary>
        FitR,

        /// <summary>
        /// Display the page with its contents magnified just enough to fit the rectangle specified
        /// by the coordinates left, bottom, right, and top entirely within the window both 
        /// horizontally and vertically.
        /// </summary>
        FitB,

        /// <summary>
        /// Display the page with the vertical coordinate top positioned at the top edge of
        /// the window and the contents of the page magnified just enough to fit the entire
        /// width of its bounding box within the window.
        /// </summary>
        FitBH,

        /// <summary>
        /// Display the page with the horizontal coordinate left positioned at the left edge of
        /// the window and the contents of the page magnified just enough to fit the entire
        /// height of its bounding box within the window.
        /// </summary>
        FitBV,
    }
}
