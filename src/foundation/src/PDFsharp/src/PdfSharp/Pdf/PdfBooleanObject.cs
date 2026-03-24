// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an indirect boolean value. This type is not used by PDFsharp. If it is imported from
    /// an external PDF file, the value is converted into a direct object.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfBooleanObject : PdfPrimitiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBooleanObject"/> class.
        /// </summary>
        public PdfBooleanObject()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBooleanObject"/> class.
        /// </summary>
        public PdfBooleanObject(bool value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBooleanObject"/> class.
        /// </summary>
        public PdfBooleanObject(PdfDocument document, bool value)
            : base(document, true)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBooleanObject"/> class
        /// without making it indirect.
        /// Used in PDF parser only.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The initial value.</param>
        /// <param name="createIndirect">If true creates an indirect object.</param>
        internal PdfBooleanObject(PdfDocument document, bool value, bool createIndirect)
            : base(document, createIndirect)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value of this instance as boolean value.
        /// </summary>
        public bool Value => _value;

        readonly bool _value;

        /// <summary>
        /// Returns "false" or "true".
        /// </summary>
        public override string ToString()
            => _value ? bool.TrueString : bool.FalseString;

        /// <summary>
        /// Writes the keyword «false» or «true».
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            writer.Write(_value);
            writer.WriteEndObject();
        }
    }
}
