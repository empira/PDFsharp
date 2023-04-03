// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Used to determine the leader for the tab.
    /// </summary>
    public enum TabLeader
    {
        /// <summary>
        /// Blanks are used as leader.
        /// </summary>
        Spaces,

        /// <summary>
        /// Dots at the baseline.
        /// </summary>
        Dots,

        /// <summary>
        /// Dashes are used as leader.
        /// </summary>
        Dashes,

        /// <summary>
        /// Same as Heavy.
        /// </summary>
        Lines,

        /// <summary>
        /// Leader will be underlined.
        /// </summary>
        Heavy,

        /// <summary>
        /// Dots in the middle (vertical) of the line.
        /// </summary>
        MiddleDot
    }
}
