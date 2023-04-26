// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the space between lines in a paragraph.
    /// </summary>
    public enum LineSpacingRule
    {
        /// <summary>
        /// Single line spacing.
        /// </summary>
        Single,

        /// <summary>
        /// Line spacing 1.5 lines.
        /// </summary>
        OnePtFive,

        /// <summary>
        /// Double line spacing.
        /// </summary>
        Double,

        /// <summary>
        /// Minimum value for line spacing. Larger line spacing will be used when needed by large fonts.
        /// </summary>
        AtLeast,

        /// <summary>
        /// Fixed line spacing.
        /// </summary>
        Exactly,

        /// <summary>
        /// Individual line spacing. 1.25 will give 125% line spacing, 3.0 will give 300% line spacing, 2.0 is double line spacing.
        /// </summary>
        Multiple
    }
}
