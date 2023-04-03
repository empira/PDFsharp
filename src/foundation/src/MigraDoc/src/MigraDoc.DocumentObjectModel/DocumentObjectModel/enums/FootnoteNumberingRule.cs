// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Determines the behavior of the footnote numbering.
    /// </summary>
    public enum FootnoteNumberingRule
    {
        /// <summary>
        /// Numbering of the footnote restarts on each page.
        /// </summary>
        RestartPage,

        /// <summary>
        /// Numbering does not restart, each new footnote number will be incremented by 1.
        /// </summary>
        RestartContinuous,

        /// <summary>
        /// Numbering of the footnote restarts on each section.
        /// </summary>
        RestartSection
    }
}
