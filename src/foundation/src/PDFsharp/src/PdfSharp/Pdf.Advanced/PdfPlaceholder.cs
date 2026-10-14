// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;

// v7.0.0 TODO review

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents text that is written ‘as it is’ into the PDF stream.
    /// Using this class can lead to invalid PDF files.
    /// E.g. strings in a literal are not encrypted when the document is saved with a password.
    /// </summary>
    sealed class PdfPlaceholder(int length, char chPlaceholder = '?') : PdfPrimitive
    {
        public void SetValue(string value)
        {
            EnsureLength(value.Length);
            Value = value;
            _isEffectiveValueSet = true;
        }

        public void SetValue(byte[] bytes)
        {
            EnsureLength(bytes.Length);
            Value = PdfEncoders.RawEncoding.GetString(bytes);
            _isEffectiveValueSet = true;
        }

        /// <summary>
        /// Gets the number of bytes of the signature.
        /// </summary>
        public int Length { get; init; } = length;

        /// <summary>
        /// Gets the value as literal string.
        /// </summary>
        public string Value { get; private set; } = new(chPlaceholder, length);

        /// <summary>
        /// Returns the current value.
        /// </summary>
        public override string ToString() => Value;

        /// <summary>
        /// Writes the placeholder item.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
            => writer.Write(this, out _startPosition, out _endPosition);

        /// <summary>
        /// Writes the effective value at the placeholder position.
        /// </summary>
        public void WriteEffectiveValue(PdfWriter writer)
        {
            if (!_isEffectiveValueSet)
                throw new InvalidOperationException("PdfPlaceholder cannot write the effective value because it is not set.");

            // Save current writer position.
            var oldPosition = writer.Position;

            // Write Value at StartPosition.
            Debug.Assert(Value.Length == Length);
            writer.Stream.Position = StartPosition;
            writer.WriteRaw(Value);

            // Restore old writer position.
            writer.Stream.Position = oldPosition;
        }

        /// <summary>
        /// Position of the first byte of this item in PdfWriter’s stream.
        /// </summary>
        public SizeType StartPosition => _startPosition;
        SizeType _startPosition;

        /// <summary>
        /// Position of the last byte of this item in PdfWriter’s stream.
        /// </summary>
        public SizeType EndPosition => _endPosition;
        SizeType _endPosition;

        bool _isEffectiveValueSet;

        void EnsureLength(int length)
        {
            if (Length != length)
            {
                throw new InvalidOperationException(
                    Invariant($"The length of the item must be '{Length}', but it is '{length}'."));
            }
        }
    }
}
