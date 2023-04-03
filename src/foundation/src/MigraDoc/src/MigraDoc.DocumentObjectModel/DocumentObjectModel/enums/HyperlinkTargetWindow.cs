// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the target window a Hyperlink shall open an external PDF in.
    /// </summary>
    public enum HyperlinkTargetWindow
    {
        /// <summary>
        /// The viewer application should behave in accordance with the current user preference.
        /// </summary>
        UserPreference,

        /// <summary>
        /// The destination document replaces the current document in the same window.
        /// </summary>
        SameWindow,

        /// <summary>
        /// Opens the destination document in a new window.
        /// </summary>
        NewWindow
    }
}
