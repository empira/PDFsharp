// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Reference point of the Left attribute.
    /// </summary>
    public enum RelativeHorizontal
    {
        /// <summary>
        /// Alignment relative to the right side of the previous element.
        /// </summary>
        Character,

        /// <summary>
        /// Alignment relative to the right side of the previous element.
        /// </summary>
        Column,

        /// <summary>
        /// Alignment relative to page margin.
        /// </summary>
        Margin,

        /// <summary>
        /// Alignment relative to page edge.
        /// </summary>
        Page
    }
}
