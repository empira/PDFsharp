// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Reference point of the Top attribute.
    /// </summary>
    public enum RelativeVertical
    {
        /// <summary>
        /// Alignment relative to the bottom side of the previous element.
        /// </summary>
        Line,

        /// <summary>
        /// Alignment relative to page margin.
        /// </summary>
        Margin,

        /// <summary>
        /// Alignment relative to page edge.
        /// </summary>
        Page,

        /// <summary>
        /// Alignment relative to the bottom line of the previous element.
        /// </summary>
        Paragraph
    }
}
