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
            => IsReal = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRealObject"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfRealObject(double value) : this()
            => Value = value;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRealObject"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The value.</param>
        public PdfRealObject(PdfDocument document, double value)
            : base(document, true)
        {
            IsReal = true;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRealObject"/> class
        /// without making it indirect.
        /// Used in PDF parser only.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The initial value.</param>
        /// <param name="createIndirect">If true creates an indirect object.</param>
        internal PdfRealObject(PdfDocument document, double value, bool createIndirect)
            : base(document, createIndirect)
        {
            IsReal = true;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Returns the real as a culture invariant floating point string.
        /// </summary>
        public override string ToString()
        {
            //=> Value.ToString(CultureInfo.InvariantCulture);
            var f = (float)Value;  // See unit test Test_Single_Write_and_Read for explanation.
            return f.ToString(Config.SignificantDecimalPlaces7, CultureInfo.InvariantCulture);
        }


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
