// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Internal  // #FOLDER Pdf.Internal
{
    // TODO Move to new file.
    /// <summary>
    /// Represents a PDF item literally as a string.
    /// This class exists for debugging and testing purposes only,
    /// e.g. to create a non-existing reference for parser testing.
    /// It is not needed to create valid PDF documents.
    /// </summary>
    public sealed class PdfDebugObject : PdfPrimitiveObject
    {
        /// <summary>
        /// Creates a new PdfRawItem from a raw string.
        /// </summary>
        public PdfDebugObject(PdfDocument doc, string value)
            : base(doc, true)
        {
            _value = value;
        }

        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            writer.Write(new PdfLiteral(_value));
            writer.WriteEndObject();
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override String ToString() => _value;

        readonly string _value;
    }
}