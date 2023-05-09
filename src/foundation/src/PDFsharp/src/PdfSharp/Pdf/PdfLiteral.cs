// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents text that is written 'as it is' into the PDF stream. This class can lead to invalid PDF files.
    /// E.g. strings in a literal are not encrypted when the document is saved with a password.
    /// </summary>
    public sealed class PdfLiteral : PdfItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLiteral"/> class.
        /// </summary>
        public PdfLiteral()
        { }

        /// <summary>
        /// Initializes a new instance with the specified string.
        /// </summary>
        public PdfLiteral(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance with the culture invariant formatted specified arguments.
        /// </summary>
        public PdfLiteral(string format, params object[] args)
        {
            Value = PdfEncoders.Format(format, args);
        }

        /// <summary>
        /// Creates a literal from an XMatrix
        /// </summary>
        public static PdfLiteral FromMatrix(XMatrix matrix)
        {
            return new PdfLiteral($"[{PdfEncoders.ToString(matrix)}]");
        }

        /// <summary>
        /// Gets the value as literal string.
        /// </summary>
        public string Value { get; } = String.Empty;

        /// <summary>
        /// Returns a string that represents the current value.
        /// </summary>
        public override string ToString() => Value;

        internal override void WriteObject(PdfWriter writer) 
            => writer.Write(this);
    }
}
