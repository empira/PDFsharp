// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an indirect integer value. This type is not used by PDFsharp. If it is imported from
    /// an external PDF file, the value is converted into a direct object.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfUIntegerObject : PdfNumberObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfUIntegerObject"/> class.
        /// </summary>
        public PdfUIntegerObject()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfUIntegerObject"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfUIntegerObject(uint value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfUIntegerObject"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The value.</param>
        public PdfUIntegerObject(PdfDocument document, uint value)
            : base(document)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value as unsigned integer.
        /// </summary>
        public uint Value => _value;

        readonly uint _value;

        /// <summary>
        /// Returns the integer as string.
        /// </summary>
        public override string ToString() 
            => _value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Writes the integer literal.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            writer.Write(_value);
            writer.WriteEndObject();
        }
    }
}
