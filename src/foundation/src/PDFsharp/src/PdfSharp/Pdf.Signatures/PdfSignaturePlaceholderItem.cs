// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfSignaturePlaceholderItem"/> class.
    /// It represents a placeholder for a digital signature.
    /// Note that the placeholder must be completely overridden, if necessary the signature
    /// must be padded with trailing zeros. Blanks between the end of the hex-sting and
    /// the end of the reserved space is considered as a certificate error by Acrobat.
    /// </summary>
    /// <param name="size">The size of the signature in bytes.</param>
    [DebuggerDisplay("({" + nameof(Size) + "})")]
    sealed class PdfSignaturePlaceholderItem(int size) : PdfItem
    {
        /// <summary>
        /// Returns the placeholder string padded with question marks to ensure that the code
        /// fails if it is not correctly overridden.
        /// </summary>
        public override string ToString() => "<" + new string('?', 2 * Size)+ ">";

        /// <summary>
        /// Writes the item DocEncoded.
        /// </summary>
        internal override void WriteObject(PdfWriter writer) 
            => writer.Write(this, out _startPosition, out _endPosition);

        /// <summary>
        /// Gets the number of bytes of the signature.
        /// </summary>
        public int Size { get; init; } = size;

        /// <summary>
        /// Position of the first byte of this item in PdfWriter’s stream.
        /// Precisely: The index of the '&lt;'.
        /// </summary>
        public SizeType StartPosition => _startPosition;
        SizeType _startPosition;

        /// <summary>
        /// Position of the last byte of this item in PdfWriter’s stream.
        /// Precisely: The index of the line feed behind '&gt;'.
        /// For timestamped signatures, the maximum length must be used.
        /// </summary>
        public SizeType EndPosition => _endPosition;
        SizeType _endPosition;
    }
}
