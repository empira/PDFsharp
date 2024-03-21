// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct 32-bit signed integer value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfInteger : PdfNumber, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteger"/> class.
        /// </summary>
        public PdfInteger()
        {
            IsInteger = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteger"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfInteger(int value)
        {
            IsInteger = true;
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
        /// Writes the integer as string.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
            => writer.Write(this);

        #region IConvertible Members

        ulong IConvertible.ToUInt64(IFormatProvider? provider)
            => Convert.ToUInt64(Value);

        sbyte IConvertible.ToSByte(IFormatProvider? provider)
            => throw new InvalidCastException();

        double IConvertible.ToDouble(IFormatProvider? provider)
            => Value;

        DateTime IConvertible.ToDateTime(IFormatProvider? provider)
            //// TO-DO: Add PdfInteger.ToDateTime implementation
            // => new DateTime();
            => throw new InvalidCastException();

        float IConvertible.ToSingle(IFormatProvider? provider)
            => Value;

        bool IConvertible.ToBoolean(IFormatProvider? provider)
            => Convert.ToBoolean(Value);

        int IConvertible.ToInt32(IFormatProvider? provider)
            => Value;

        ushort IConvertible.ToUInt16(IFormatProvider? provider)
            => Convert.ToUInt16(Value);

        short IConvertible.ToInt16(IFormatProvider? provider)
            => Convert.ToInt16(Value);

        string IConvertible.ToString(IFormatProvider? provider)
            => Value.ToString(provider);

        byte IConvertible.ToByte(IFormatProvider? provider)
            => Convert.ToByte(Value);

        char IConvertible.ToChar(IFormatProvider? provider)
            => Convert.ToChar(Value);

        long IConvertible.ToInt64(IFormatProvider? provider)
            => Value;

        /// <summary>
        /// Returns TypeCode for 32-bit integers.
        /// </summary>
        public TypeCode GetTypeCode()
            => TypeCode.Int32;

        decimal IConvertible.ToDecimal(IFormatProvider? provider)
            => Value;

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
        {
            // TODO: Add PdfInteger.ToType implementation
            return null!;
        }

        uint IConvertible.ToUInt32(IFormatProvider? provider)
            => Convert.ToUInt32(Value);

        #endregion
    }
}
