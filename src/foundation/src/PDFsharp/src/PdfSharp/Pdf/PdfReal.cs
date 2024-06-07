// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct real value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfReal : PdfNumber, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReal"/> class.
        /// </summary>
        public PdfReal()
        {
            IsReal = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReal"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PdfReal(double value)
        {
            if (value is < Single.MinValue or > Single.MaxValue)
                Debug.Assert(false);
            IsReal = true;
            Value = value;
        }

        /// <summary>
        /// Gets the value as double.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Returns the real number as string.
        /// </summary>
        public override string ToString()
            => Value.ToString(Config.SignificantDecimalPlaces3, CultureInfo.InvariantCulture);

        /// <summary>
        /// Writes the real value with up to three digits.
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
            => throw new InvalidCastException();

        float IConvertible.ToSingle(IFormatProvider? provider)
            => Convert.ToSingle(Value);

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
            => Convert.ToInt64(Value);
        
        /// <summary>
        /// Returns TypeCode for 32-bit integers.
        /// </summary>
        public TypeCode GetTypeCode()
            => TypeCode.Double;

        decimal IConvertible.ToDecimal(IFormatProvider? provider)
            => Convert.ToDecimal(Value);

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
