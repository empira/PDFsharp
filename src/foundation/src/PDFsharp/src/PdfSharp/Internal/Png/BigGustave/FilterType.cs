// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// BigGustave is distributed with PDFsharp, but was published under a different license.
// See file LICENSE in the folder containing this file.

namespace PdfSharp.Internal.Png.BigGustave
{
    internal enum FilterType
    {
        /// <summary>
        /// The raw byte is unaltered.
        /// </summary>
        None = 0,
        /// <summary>
        /// The byte to the left.
        /// </summary>
        Sub = 1,
        /// <summary>
        /// The byte above.
        /// </summary>
        Up = 2,
        /// <summary>
        /// The mean of bytes left and above, rounded down.
        /// </summary>
        Average = 3,
        /// <summary>
        /// Byte to the left, above or top-left based on Paeth’s algorithm.
        /// </summary>
        Paeth = 4
    }
}