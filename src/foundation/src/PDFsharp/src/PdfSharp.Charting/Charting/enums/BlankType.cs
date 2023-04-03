// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Determines how null values will be handled in a chart.
    /// </summary>
    public enum BlankType
    {
        /// <summary>
        /// Null value is not plotted.
        /// </summary>
        NotPlotted,

        /// <summary>
        /// Null value will be interpolated.
        /// </summary>
        Interpolated,

        /// <summary>
        /// Null value will be handled as zero.
        /// </summary>
        Zero
    }
}
