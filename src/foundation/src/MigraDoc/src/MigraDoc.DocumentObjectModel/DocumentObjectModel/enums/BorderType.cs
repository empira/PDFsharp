// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the type of the Border object and therefore its position.
    /// </summary>
    public enum BorderType
    {
        /// <summary>
        /// Top border.
        /// </summary>
        Top,

        /// <summary>
        /// Left border.
        /// </summary>
        Left,

        /// <summary>
        /// Bottom border.
        /// </summary>
        Bottom,

        /// <summary>
        /// Right border.
        /// </summary>
        Right,

        /// <summary>
        /// Horizontal border (currently not used).
        /// </summary>
        [Obsolete("Not used in MigraDoc 1.2")]
        Horizontal,  // Not used in MigraDoc 1.2.

        /// <summary>
        /// Vertical border (currently not used).
        /// </summary>
        [Obsolete("Not used in MigraDoc 1.2")]
        Vertical,    // Not used in MigraDoc 1.2.

        /// <summary>
        /// Diagonal-down border.
        /// </summary>
        DiagonalDown,

        /// <summary>
        /// Diagonal-up border.
        /// </summary>
        DiagonalUp
    }
}
