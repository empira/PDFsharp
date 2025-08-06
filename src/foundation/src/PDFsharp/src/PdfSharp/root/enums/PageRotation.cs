// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp
{
    /// <summary>
    /// Identifies the rotation of a page in a PDF document.
    /// </summary>
    public enum PageRotation
    {
        /// <summary>
        /// The page is displayed with no rotation by a viewer.
        /// </summary>
        None = 0,

        /// <summary>
        /// The page is displayed rotated by 90 degrees clockwise by a viewer.
        /// </summary>
        Rotate90DegreesRight = 90,

        /// <summary>
        /// The page is displayed rotated by 180 degrees by a viewer.
        /// </summary>
        RotateUpsideDown = 180,

        /// <summary>
        /// The page is displayed rotated by 270 degrees clockwise by a viewer.
        /// </summary>
        Rotate90DegreesLeft = 270,
    }
}
