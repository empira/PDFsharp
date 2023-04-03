// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Determines the position of the footnote on the page.
    /// </summary>
    public enum FootnoteLocation
    {
        /// <summary>
        /// Footnote will be rendered on the bottom of the page.
        /// </summary>
        BottomOfPage,

        /// <summary>
        /// Footnote will be rendered immediately after the text.
        /// </summary>
        BeneathText
    }
}
