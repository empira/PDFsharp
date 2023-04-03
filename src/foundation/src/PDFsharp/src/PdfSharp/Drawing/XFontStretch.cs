// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Describes the degree to which a font has been stretched compared to the normal aspect ratio of that font.
    /// </summary>
    // [DebuggerDisplay("'{Name}', {Size}")]
    public readonly struct XFontStretch : IFormattable
    {
        internal XFontStretch(XFontStretchValue stretch)
        {
            _stretch = (int)stretch - 5;
        }

        /// <summary>Creates a new instance of <see cref="T:System.Windows.XFontStretch" /> that corresponds to the OpenType usStretchClass value.</summary>
        /// <param name="stretchValue">An integer value between one and nine that corresponds to the usStretchValue definition in the OpenType specification.</param>
        /// <returns>A new instance of <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static XFontStretch FromOpenTypeStretch(int stretchValue)
        {
            if (stretchValue is < 1 or > 9)
                //throw new ArgumentOutOfRangeException(nameof(stretchValue), MS.Internal.PresentationCore.SR.Get("ParameterMustBeBetween", (object)1, (object)9));
                throw new ArgumentOutOfRangeException(nameof(stretchValue), "Parameter must be between 1 and 9.");

            return new XFontStretch((XFontStretchValue)stretchValue);
        }

        /// <summary>Returns a value that represents the OpenType usStretchClass for this <see cref="T:System.Windows.XFontStretch" /> object.</summary>
        /// <returns>An integer value between 1 and 999 that corresponds to the usStretchClass definition in the OpenType specification.</returns>
        public int ToOpenTypeStretch() => RealStretch;

        /// <summary>Compares two instances of <see cref="T:System.Windows.XFontStretch" /> objects.</summary>
        /// <param name="left">The first <see cref="T:System.Windows.XFontStretch" /> object to compare.</param>
        /// <param name="right">The second <see cref="T:System.Windows.XFontStretch" /> object to compare.</param>
        /// <returns>An <see cref="T:System.Int32" /> value that represents the relationship between the two instances of <see cref="T:System.Windows.XFontStretch" />.</returns>
        public static int Compare(XFontStretch left, XFontStretch right)
            => left._stretch - right._stretch;

        /// <summary>Evaluates two instances of <see cref="T:System.Windows.XFontStretch" /> to determine whether one instance is less than the other.</summary>
        /// <param name="left">The first instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <param name="right">The second instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
        public static bool operator <(XFontStretch left, XFontStretch right)
            => Compare(left, right) < 0;

        /// <summary>Evaluates two instances of <see cref="T:System.Windows.XFontStretch" /> to determine whether one instance is less than or equal to the other.</summary>
        /// <param name="left">The first instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <param name="right">The second instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
        public static bool operator <=(XFontStretch left, XFontStretch right)
            => Compare(left, right) <= 0;

        /// <summary>Evaluates two instances of <see cref="T:System.Windows.XFontStretch" /> to determine if one instance is greater than the other.</summary>
        /// <param name="left">First instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <param name="right">Second instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >(XFontStretch left, XFontStretch right)
            => Compare(left, right) > 0;

        /// <summary>Evaluates two instances of <see cref="T:System.Windows.XFontStretch" /> to determine whether one instance is greater than or equal to the other.</summary>
        /// <param name="left">The first instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <param name="right">The second instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left" /> is greater than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >=(XFontStretch left, XFontStretch right)
            => Compare(left, right) >= 0;

        /// <summary>Compares two instances of <see cref="T:System.Windows.XFontStretch" /> for equality.</summary>
        /// <param name="left">First instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <param name="right">Second instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <returns>
        /// <see langword="true" /> when the specified <see cref="T:System.Windows.XFontStretch" /> objects are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(XFontStretch left, XFontStretch right)
            => Compare(left, right) == 0;

        /// <summary>Evaluates two instances of <see cref="T:System.Windows.XFontStretch" /> to determine inequality.</summary>
        /// <param name="left">The first instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <param name="right">The second instance of <see cref="T:System.Windows.XFontStretch" /> to compare.</param>
        /// <returns>
        /// <see langword="false" /> if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, <see langword="true" />.</returns>
        public static bool operator !=(XFontStretch left, XFontStretch right)
            => !(left == right);

        /// <summary>Compares a <see cref="T:System.Windows.XFontStretch" /> object with the current <see cref="T:System.Windows.XFontStretch" /> object.</summary>
        /// <param name="obj">The instance of the <see cref="T:System.Windows.XFontStretch" /> object to compare for equality.</param>
        /// <returns>
        /// <see langword="true" /> if two instances are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(XFontStretch obj)
            => this == obj;

        /// <summary>Compares a <see cref="T:System.Object" /> with the current <see cref="T:System.Windows.XFontStretch" /> object.</summary>
        /// <param name="obj">The instance of the <see cref="T:System.Object" /> to compare for equality.</param>
        /// <returns>
        /// <see langword="true" /> if two instances are equal; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj)
            => obj is XFontStretch xFontStretch && this == xFontStretch;

        /// <summary>Retrieves the hash code for this object.</summary>
        /// <returns>An <see cref="T:System.Int32" /> value representing the hash code for the object.</returns>
        public override int GetHashCode()
            => RealStretch;

        /// <summary>Creates a <see cref="T:System.String" /> representation of the current <see cref="T:System.Windows.XFontStretch" /> object based on the current culture.</summary>
        /// <returns>A <see cref="T:System.String" /> value representation of the object.</returns>
        public override string ToString()
            => ConvertToString(null, null);

        string IFormattable.ToString(string? format, IFormatProvider? provider)
            => ConvertToString(format, provider);

        string ConvertToString(string? format, IFormatProvider? provider)
        {
            if (!XFontStretches.XFontStretchToString((XFontStretchValue)RealStretch, out var convertedValue))
                Debug.Assert(false);
            return convertedValue ?? "";
        }

        int RealStretch => _stretch + 5;

        readonly int _stretch;
    }
}
