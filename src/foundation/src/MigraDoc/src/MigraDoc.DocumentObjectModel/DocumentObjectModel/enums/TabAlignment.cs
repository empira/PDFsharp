// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Determines the alignment of the tab.
    /// </summary>
    public enum TabAlignment
    {
        /// <summary>
        /// Tab will be left aligned.
        /// </summary>
        Left,

        /// <summary>
        /// Tab will be centered.
        /// </summary>
        Center,

        /// <summary>
        /// Tab will be right aligned.
        /// </summary>
        Right,

        /// <summary>
        /// Positioned at the last dot or comma.
        /// </summary>
        Decimal,

        //Bar     = 4,  // MigraDoc 2.0
        //List    = 6,  // MigraDoc 2.0
    }
}
