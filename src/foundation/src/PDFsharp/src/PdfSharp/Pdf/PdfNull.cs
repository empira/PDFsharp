// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct reference that is not in the cross-reference table.
    /// </summary>
    public sealed class PdfNull : PdfPrimitive
    {
        // Reference: 3.2.8  Null Object / Page 63

        /// <summary>
        /// Use PdfNull.Value to get an instance of this class.
        /// </summary>
        PdfNull()
        { }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString() => "null";

        /// <summary>
        /// Writes ‘null’.
        /// </summary>

        internal override void WriteObject(PdfWriter writer)
            => writer.Write(this);

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly PdfNull Value = new();
    }
}
