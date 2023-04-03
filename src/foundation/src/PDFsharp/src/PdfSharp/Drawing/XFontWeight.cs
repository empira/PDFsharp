// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Defines the density of a typeface, in terms of the lightness or heaviness of the strokes.
    /// </summary>
    [DebuggerDisplay("'{Weight}'")]
    public readonly struct XFontWeight : IFormattable
    {
        internal XFontWeight(int weight)
        {
            Weight = weight;
        }

        /// <summary>
        /// Gets the weight of the font, a value between 1 and 999.
        /// </summary>
        public int Weight { get; }

        //public static XFontWeight FromOpenTypeWeight(int weightValue)
        //{
        //  if (weightValue < 1 || weightValue > 999)
        //    throw new ArgumentOutOfRangeException("weightValue", "Parameter must be between 1 and 999.");
        //  return new XFontWeight(weightValue);
        //}

        /// <summary>
        /// Compares the specified font weights.
        /// </summary>
        public static int Compare(XFontWeight left, XFontWeight right) 
            => left.Weight - right.Weight;

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        public static bool operator <(XFontWeight left, XFontWeight right) 
            => Compare(left, right) < 0;

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        public static bool operator <=(XFontWeight left, XFontWeight right) 
            => Compare(left, right) <= 0;

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        public static bool operator >(XFontWeight left, XFontWeight right) 
            => Compare(left, right) > 0;

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        public static bool operator >=(XFontWeight left, XFontWeight right)
            => Compare(left, right) >= 0;

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(XFontWeight left, XFontWeight right) 
            => Compare(left, right) == 0;

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(XFontWeight left, XFontWeight right) 
            => !(left == right);

        /// <summary>
        /// Determines whether the specified <see cref="XFontWeight"/> is equal to the current <see cref="XFontWeight"/>.
        /// </summary>
        public bool Equals(XFontWeight obj) 
            => this == obj;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        public override bool Equals(object? obj) 
            => obj is XFontWeight weight && this == weight;

        /// <summary>
        /// Serves as a hash function for this type.
        /// </summary>
        public override int GetHashCode() => Weight;

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        public override string ToString() 
            => ConvertToString(null, null);

        string IFormattable.ToString(string? format, IFormatProvider? provider) 
            => ConvertToString(format, provider);

        internal string ConvertToString(string? format, IFormatProvider? provider)
        {
            provider ??= CultureInfo.InvariantCulture;
            if (!XFontWeights.FontWeightToString(Weight, out var weight))
                return Weight.ToString(format, provider);
            return weight ?? "";  // BUG?
        }

        /// <summary>
        /// Simple hack to make it work...
        /// </summary>
        internal static XFontWeight FromGdiFontStyle(XFontStyleEx style)
        {
            // Mask out Underline, Strikeout, etc.
            return (style & XFontStyleEx.BoldItalic) switch
            {
                XFontStyleEx.Regular => XFontWeights.Normal,
                XFontStyleEx.Bold => XFontWeights.Bold,
                XFontStyleEx.Italic => XFontWeights.Normal,
                XFontStyleEx.BoldItalic => XFontWeights.Bold,
                _ => XFontWeights.Normal
            };
        }
    }
}
