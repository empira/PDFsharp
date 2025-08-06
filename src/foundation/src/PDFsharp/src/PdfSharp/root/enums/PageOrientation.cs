// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp
{
    /// <summary>
    /// Specifies the orientation of a page.
    /// </summary>
    public enum PageOrientation
    {
        /// <summary>
        /// The default page orientation.
        /// The top and bottom width is less than or equal to the
        /// left and right side.
        /// </summary>
        Portrait = 0,

        /// <summary>
        /// The width and height of the page are reversed.
        /// </summary>
        Landscape = 1
    }
}
