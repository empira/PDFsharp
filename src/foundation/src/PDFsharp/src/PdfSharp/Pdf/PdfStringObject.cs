// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an indirect text string value. This type is not created by PDFsharp.
    /// If it is imported from an external PDF file, the value is converted into a direct object.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfStringObject : PdfPrimitiveObject  // #HEX_STRING_FIX DELETE
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfStringObject"/> class.
        /// </summary>
        public PdfStringObject()
        {
            _flags = PdfStringFlags.RawEncoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfStringObject"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfStringObject(string value)
        {
            _value = value;
            _flags = PdfStringFlags.RawEncoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfStringObject"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The value.</param>
        public PdfStringObject(PdfDocument document, string value)
            : base(document, true)
        {
            _value = value;
            _flags = PdfStringFlags.RawEncoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfStringObject"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encoding">The encoding.</param>
        public PdfStringObject(string value, PdfStringEncoding encoding)
        {
            _value = value;
            _flags = (PdfStringFlags)encoding;
        }

        internal PdfStringObject(string value, PdfStringFlags flags)
        {
            _value = value;
            _flags = flags;
        }

#if PRESERVE_PARSED_VALUES_
        /// <summary>
        /// Gets or sets the string that was read from the lexer.
        /// </summary>
        internal string? ParsedValue { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfStringObject"/> class
        /// without making it indirect.
        /// Used in PDF parser only.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="value">The initial value.</param>
        /// <param name="createIndirect"></param>
        internal PdfStringObject(PdfDocument document, string value, bool createIndirect)
            : base(document, createIndirect)
        {
            _value = value;
            _flags = PdfStringFlags.RawEncoding;
        }

        /// <summary>
        /// Gets the number of characters in this string.
        /// </summary>
        public int Length => _value == null! ? 0 : _value.Length;

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        public PdfStringEncoding Encoding
        {
            get => (PdfStringEncoding)(_flags & PdfStringFlags.EncodingMask);
            set => _flags = (_flags & ~PdfStringFlags.EncodingMask) | ((PdfStringFlags)value & PdfStringFlags.EncodingMask);
        }

        /// <summary>
        /// Gets a value indicating whether the string is a hexadecimal literal.
        /// </summary>
        public bool HexLiteral
        {
            get => (_flags & PdfStringFlags.HexLiteral) != 0;
            set => _flags = value ? _flags | PdfStringFlags.HexLiteral : _flags & ~PdfStringFlags.HexLiteral;
        }
        PdfStringFlags _flags;

        /// <summary>
        /// Gets or sets the value as string
        /// </summary>
        public string Value
        {
            get => _value;
            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            set => _value = value ?? "";
        }
        string _value = "";

        internal byte[] GetRawBytes() => PdfString.ToRawBytes(_value);

        internal void SetRawBytes(byte[] value, PdfStringEncoding? encoding = null)
        {
            _value = PdfString.FromRawBytes(value, ref _flags, encoding);
        }

        /// <summary>
        /// Checks this PdfStringObject for valid BOMs and rereads it with the specified Unicode encoding.
        /// </summary>
        internal bool TryRereadAsUnicode()
        {
            return PdfString.TryRereadAsUnicode(ref _value!, ref _flags);
        }

        /// <summary>
        /// Returns the string.
        /// </summary>
        public override string ToString() => Value;

        /// <summary>
        /// Writes the string literal with encoding DOCEncoded.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            writer.Write(new PdfString(_value, _flags));
            writer.WriteEndObject();
        }
    }
}
