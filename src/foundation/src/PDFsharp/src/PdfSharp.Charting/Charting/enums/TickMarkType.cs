// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting
{
    /// <summary>
    /// Determines the position where the Tickmarks will be rendered.
    /// </summary>
    public enum TickMarkType
    {
        /// <summary>
        /// Tickmarks are not rendered.
        /// </summary>
        None,

        /// <summary>
        /// Tickmarks are rendered inside the plot area.
        /// </summary>
        Inside,

        /// <summary>
        /// Tickmarks are rendered outside the plot area.
        /// </summary>
        Outside,

        /// <summary>
        /// Tickmarks are rendered inside and outside the plot area.
        /// </summary>
        Cross
    }
}
