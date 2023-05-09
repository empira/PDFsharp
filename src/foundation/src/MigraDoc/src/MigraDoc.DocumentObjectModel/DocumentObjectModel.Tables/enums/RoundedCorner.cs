// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Tables
{
    /// <summary>
    /// Specifies if the Cell should be rendered as a rounded corner.
    /// </summary>
    public enum RoundedCorner
    {
        /// <summary>
        /// No rounded corner.
        /// </summary>
        None,
        /// <summary>
        /// Rounded top-left corner.
        /// </summary>
        TopLeft,
        /// <summary>
        /// Rounded top-right corner.
        /// </summary>
        TopRight,
        /// <summary>
        /// Rounded bottom-left corner.
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Rounded bottom-right corner.
        /// </summary>
        BottomRight
    }
}
