// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an indirect real value. This type is not used by PDFsharp. If it is imported from
    /// an external PDF file, the value is converted into a direct object.
    /// </summary>
    public sealed class PdfRealObject : PdfNumberObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRealObject"/> class.
        /// </summary>
        public PdfRealObject()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRealObject"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfRealObject(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRealObject"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The value.</param>
        public PdfRealObject(PdfDocument document, double value)
            : base(document)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Returns the real as a culture invariant string.
        /// </summary>
        public override string ToString() 
            => Value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Writes the real literal.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            writer.Write(Value);
            writer.WriteEndObject();
        }
    }
}
