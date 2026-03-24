// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an indirect 64-bit signed integer value. This type is not created by PDFsharp.
    /// If it is imported from an external PDF file, the value is converted into a direct object.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfLongIntegerObject : PdfNumberObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLongIntegerObject"/> class.
        /// </summary>
        public PdfLongIntegerObject()
            => IsLongInteger = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLongIntegerObject"/> class.
        /// </summary>
        public PdfLongIntegerObject(long value) : this()
            => Value = value;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLongIntegerObject"/> class.
        /// </summary>
        public PdfLongIntegerObject(PdfDocument document, long value)
            : base(document, true)
        {
            IsLongInteger = true;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLongIntegerObject"/> class
        /// without making it indirect.
        /// Used in PDF parser only.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The initial value.</param>
        /// <param name="createIndirect">If true creates an indirect object.</param>
        internal PdfLongIntegerObject(PdfDocument document, long value, bool createIndirect)
           : base(document, createIndirect)
        {
            IsLongInteger = true;
            Value = value;
        }

        /// <summary>
        /// Gets the value as 64-bit integer.
        /// </summary>
        public long Value { get; }

        /// <summary>
        /// Returns the integer as string.
        /// </summary>
        public override string ToString()
            => Value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Writes the integer literal.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            writer.Write(Value);
            writer.WriteEndObject();
        }
    }
}
