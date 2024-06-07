// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct 64-bit signed integer value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfLongInteger : PdfNumber, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLongInteger"/> class.
        /// </summary>
        public PdfLongInteger()
        {
            IsLongInteger = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLongInteger"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfLongInteger(long value)
        {
            IsLongInteger = true;
            Value = value;
        }

        /// <summary>
        /// Gets the value as 64-bit integer.
        /// </summary>
        public long Value { get; }

        /// <summary>
        /// Returns the 64-bit integer as string.
        /// </summary>
        public override string ToString()
            => Value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Writes the 64-bit integer as string.
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
            => Convert.ToInt32(Value);

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
        /// Returns TypeCode for 64-bit integers.
        /// </summary>
        public TypeCode GetTypeCode()
            => TypeCode.Int64;

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
