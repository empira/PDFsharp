// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an indirect 32-bit signed integer value. This type is not used by PDFsharp. If it is imported from
    /// an external PDF file, the value is converted into a direct object.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfIntegerObject : PdfNumberObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        /// </summary>
        public PdfIntegerObject()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        /// </summary>
        public PdfIntegerObject(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        /// </summary>
        public PdfIntegerObject(PdfDocument document, int value)
            : base(document)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value as integer.
        /// </summary>
        public int Value { get; }

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
