// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Determines the format of the footnote number.
    /// </summary>
    public enum FootnoteNumberStyle
    {
        /// <summary>
        /// Numbering like: 1, 2, 3, 4.
        /// </summary>
        Arabic,

        /// <summary>
        /// Lower case letters like: a, b, c, d.
        /// </summary>
        LowercaseLetter,

        /// <summary>
        /// Upper case letters like: A, B, C, D.
        /// </summary>
        UppercaseLetter,

        /// <summary>
        /// Lower case roman numbers: i, ii, iii, iv.
        /// </summary>
        LowercaseRoman,

        /// <summary>
        /// Upper case roman numbers: I, II, III, IV.
        /// </summary>
        UppercaseRoman
    }
}
