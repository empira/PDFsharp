// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Determines where the data label will be positioned.
    /// </summary>
    public enum DataLabelPosition
    {
        /// <summary>
        /// DataLabel will be centered inside the bar or pie.
        /// </summary>
        Center,

        /// <summary>
        /// Inside the bar or pie at the origin.
        /// </summary>
        InsideBase,

        /// <summary>
        /// Inside the bar or pie at the edge.
        /// </summary>
        InsideEnd,

        /// <summary>
        /// Outside the bar or pie.
        /// </summary>
        OutsideEnd
    }
}
