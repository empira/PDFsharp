// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the page break in a new section.
    /// </summary>
    public enum BreakType
    {
        /// <summary>
        /// Breaks at the next page.
        /// </summary>
        BreakNextPage,

        /// <summary>
        /// Breaks at the next even page.
        /// </summary>
        BreakEvenPage,

        /// <summary>
        /// Breaks at the next odd page.
        /// </summary>
        BreakOddPage
    }
}
