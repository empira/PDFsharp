// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// The encoding used for data in the data matrix code.
    /// </summary>
    public enum DataMatrixEncoding
    {
        /// <summary>
        /// ASCII text mode.
        /// </summary>
        Ascii,

        /// <summary>
        /// C40 text mode, potentially more compact for short strings.
        /// </summary>
        C40,

        /// <summary>
        /// Text mode.
        /// </summary>
        Text,

        /// <summary>
        /// X12 text mode, potentially more compact for short strings.
        /// </summary>
        X12,

        /// <summary>
        /// EDIFACT mode uses six bits per character, with four characters packed into three bytes.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        EDIFACT,

        /// <summary>
        /// Base 256 mode data starts with a length indicator, followed by a number of data bytes.
        /// A length of 1 to 249 is encoded as a single byte, and longer lengths are stored as two bytes.
        /// </summary>
        Base256
    }
}
