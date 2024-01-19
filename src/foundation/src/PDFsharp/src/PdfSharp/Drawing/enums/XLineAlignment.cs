// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies the vertical alignment of a text string, 
    /// relative to its starting point or rectangle layout
    /// </summary>
    public enum XLineAlignment
    {
        /// <summary>
        /// Using Rectangle layout, text is vertically aligned inside and near the top of the layout.
        /// Using Point, text is vertically aligned below the point.
        /// </summary>
        Near = 0,

       /// <summary>
        /// Using Rectangle layout, text is vertically aligned in the center of the layout.
        /// Using Point, text is vertically aligned on the center of the point.
        /// </summary>
        Center = 1,

        /// <summary>
        /// Using Rectangle layout, text is vertically aligned inside and near the bottom of the layout.
        /// Using Point, text is vertically aligned above the point.
        /// </summary>
        Far = 2,

        /// <summary>
        /// Specifies that text is aligned vertically relative to its base line.
        /// With this option the layout rectangle must have a height of 0.
        /// </summary>
        BaseLine = 3,
    }
}
