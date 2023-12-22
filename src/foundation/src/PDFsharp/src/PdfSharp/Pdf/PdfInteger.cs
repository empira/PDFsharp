// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct integer value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfInteger : PdfNumber, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteger"/> class.
        /// </summary>
        public PdfInteger()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteger"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfInteger(int value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value as integer.
        /// </summary>
        public int Value => _value;

        readonly int _value;

        /// <summary>
        /// Returns the integer as string.
        /// </summary>
        public override string ToString()
            => _value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Writes the integer as string.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
            => writer.Write(this);

        #region IConvertible Members

        ulong IConvertible.ToUInt64(IFormatProvider? provider)
            => Convert.ToUInt64(_value);

        sbyte IConvertible.ToSByte(IFormatProvider? provider)
            => throw new InvalidCastException();

        double IConvertible.ToDouble(IFormatProvider? provider)
            => _value;

        DateTime IConvertible.ToDateTime(IFormatProvider? provider)
            //// TO-DO: Add PdfInteger.ToDateTime implementation
            // => new DateTime();
            => throw new InvalidCastException();

        float IConvertible.ToSingle(IFormatProvider? provider)
            => _value;

        bool IConvertible.ToBoolean(IFormatProvider? provider)
            => Convert.ToBoolean(_value);

        int IConvertible.ToInt32(IFormatProvider? provider)
            => _value;

        ushort IConvertible.ToUInt16(IFormatProvider? provider)
            => Convert.ToUInt16(_value);

        short IConvertible.ToInt16(IFormatProvider? provider)
            => Convert.ToInt16(_value);

        string IConvertible.ToString(IFormatProvider? provider)
            => _value.ToString(provider);

        byte IConvertible.ToByte(IFormatProvider? provider)
            => Convert.ToByte(_value);

        char IConvertible.ToChar(IFormatProvider? provider)
            => Convert.ToChar(_value);

        long IConvertible.ToInt64(IFormatProvider? provider)
            => _value;

        /// <summary>
        /// Returns TypeCode for 32-bit integers.
        /// </summary>
        public TypeCode GetTypeCode()
            => TypeCode.Int32;

        decimal IConvertible.ToDecimal(IFormatProvider? provider)
            => _value;

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
        {
            // TODO:  Add PdfInteger.ToType implementation
            return null!;
        }

        uint IConvertible.ToUInt32(IFormatProvider? provider)
            => Convert.ToUInt32(_value);

        #endregion
    }
}
