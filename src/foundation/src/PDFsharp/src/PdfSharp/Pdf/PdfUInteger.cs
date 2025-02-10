// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if false // DELETE 2025-12-31 - PDF has no explicit unsigned number type.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct 32-bit unsigned integer value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    [Obsolete("This class is deprecated and will be removed.")]
    public sealed class PdfUInteger : PdfNumber, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfUInteger"/> class.
        /// </summary>
        public PdfUInteger()
        {
            IsInteger = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfUInteger"/> class.
        /// </summary>
        public PdfUInteger(uint value)
        {
            IsInteger = true;
            Value = value;
        }

        /// <summary>
        /// Gets the value as integer.
        /// </summary>
        public uint Value { get; }

        /// <summary>
        /// Returns the unsigned integer as string.
        /// </summary>
        public override string ToString()
        {
            // ToString is impure but does not change the value of _value.
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            return Value.ToString(CultureInfo.InvariantCulture);
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
        }

        /// <summary>
        /// Writes the integer as string.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
            => writer.Write(this);

        #region IConvertible Members

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer.
        /// </summary>
        public ulong ToUInt64(IFormatProvider? provider)
            => Convert.ToUInt64(Value);

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer.
        /// </summary>
        public sbyte ToSByte(IFormatProvider? provider)
            => throw new InvalidCastException();

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number.
        /// </summary>
        public double ToDouble(IFormatProvider? provider) => Value;

        /// <summary>
        /// Returns an undefined DateTime structure.
        /// </summary>
        public DateTime ToDateTime(IFormatProvider? provider)
        {
            // TODO_OLD:  Add PdfUInteger.ToDateTime implementation
            return new DateTime();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number.
        /// </summary>
        public float ToSingle(IFormatProvider? provider) => Value;

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value.
        /// </summary>
        public bool ToBoolean(IFormatProvider? provider)
            => Convert.ToBoolean(Value);

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer.
        /// </summary>
        public int ToInt32(IFormatProvider? provider)
            => Convert.ToInt32(Value);

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer.
        /// </summary>
        public ushort ToUInt16(IFormatProvider? provider)
            => Convert.ToUInt16(Value);

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer.
        /// </summary>
        public short ToInt16(IFormatProvider? provider)
            => Convert.ToInt16(Value);

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.String"></see>.
        /// </summary>
        string IConvertible.ToString(IFormatProvider? provider)
            => Value.ToString(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer.
        /// </summary>
        public byte ToByte(IFormatProvider? provider)
            => Convert.ToByte(Value);

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character.
        /// </summary>
        public char ToChar(IFormatProvider? provider)
            => Convert.ToChar(Value);

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer.
        /// </summary>
        public long ToInt64(IFormatProvider? provider) => Value;

        /// <summary>
        /// Returns type code for 32-bit integers.
        /// </summary>
        public TypeCode GetTypeCode() => TypeCode.Int32;

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.Decimal"></see> number.
        /// </summary>
        public decimal ToDecimal(IFormatProvider? provider) => Value;

        /// <summary>
        /// Returns null.
        /// </summary>
        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            // TODO_OLD: Add PdfUInteger.ToType implementation
            //return null!;
            throw new NotImplementedException(nameof(ToType));
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer.
        /// </summary>
        public uint ToUInt32(IFormatProvider? provider)
            => Convert.ToUInt32(Value);

        #endregion
    }
}
#endif
