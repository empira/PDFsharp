// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Internal
{
    // TODO Move to new file.
    /// <summary>
    /// Represents a PDF item literally as a string.
    /// This class exists for debugging and testing purposes only,
    /// e.g. to create a non-existing reference for parser testing.
    /// It is not needed to create valid PDF documents.
    /// </summary>
    public sealed class PdfDebugItem : PdfPrimitive
    {
        // Note that this class does the same as PdfLiteral, but it has a different intended purpose.

        /// <summary>
        /// Creates a new PdfDebugItem from a string.
        /// </summary>
        public PdfDebugItem(string value)
        {
            _value = value;
        }

        internal override void WriteObject(PdfWriter writer)
        {
            var literal = new PdfLiteral(_value);
            writer.Write(literal);
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override String ToString() => _value;

        readonly string _value;
    }
}
