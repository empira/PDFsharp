// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Specifies the dash style of the LineFormat object.
    /// </summary>
    public enum DashStyle
    {
        /// <summary>
        /// A solid line.
        /// </summary>
        Solid,

        /// <summary>
        /// A dashed line.
        /// </summary>
        Dash,

        /// <summary>
        /// Alternating dashes and dots.
        /// </summary>
        DashDot,

        /// <summary>
        /// A dash followed by two dots.
        /// </summary>
        DashDotDot,

        /// <summary>
        /// Square dots.
        /// </summary>
        SquareDot
    }
}
