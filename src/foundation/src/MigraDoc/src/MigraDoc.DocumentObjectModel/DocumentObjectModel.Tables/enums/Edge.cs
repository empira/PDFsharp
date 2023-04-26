// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Combinable flags to set Borders using the SetEdge function.
    /// </summary>
    [Flags]
    public enum Edge
    {
        /// <summary>
        /// Border at top edge.
        /// </summary>
        Top = 0x0001,
        /// <summary>
        /// Border at left edge.
        /// </summary>
        Left = 0x0002,
        /// <summary>
        /// Border at bottom edge.
        /// </summary>
        Bottom = 0x0004,
        /// <summary>
        /// Border at right edge.
        /// </summary>
        Right = 0x0008,
        /// <summary>
        /// Horizontal border.
        /// </summary>
        Horizontal = 0x0010,
        /// <summary>
        /// Vertical border.
        /// </summary>
        Vertical = 0x0020,
        /// <summary>
        /// Diagonal-down border.
        /// </summary>
        DiagonalDown = 0x0040,
        /// <summary>
        /// Diagonal-up border.
        /// </summary>
        DiagonalUp = 0x0080,
        /// <summary>
        /// Box means Borders at top, left, right, and bottom edge.
        /// </summary>
        Box = Top | Left | Bottom | Right,
        /// <summary>
        /// Interior means horizontal and vertical borders.
        /// </summary>
        Interior = Horizontal | Vertical,
        /// <summary>
        /// Interior means diagonal-down and diagonal-up borders.
        /// </summary>
        Cross = DiagonalDown | DiagonalUp,
    }
}
