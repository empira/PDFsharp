// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies the order for matrix transform operations.
    /// </summary>
    public enum XMatrixOrder
    {
        /// <summary>
        /// The new operation is applied before the old operation.
        /// </summary>
        Prepend = 0,

        /// <summary>
        /// The new operation is applied after the old operation.
        /// </summary>
        Append = 1,
    }
}
