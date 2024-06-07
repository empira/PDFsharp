// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

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
    /// Defines a structure that represents the style of a font face as normal, italic, or oblique.
    /// Note that this struct is new since PDFsharp 6.0. XFontStyle from prior version of PDFsharp is
    /// renamed to XFontStyleEx.
    /// </summary>
    [DebuggerDisplay("'{" + nameof(_style) + "}'")]
    public readonly struct XFontStyle : IFormattable
    {
        internal XFontStyle(XFontStyleValue style)
        {
            _style = style;
        }

        /// <summary>Compares two instances of <see cref="T:System.Windows.XFontStyle" /> for equality.</summary>
        /// <param name="left">The first instance of <see cref="T:System.Windows.XFontStyle" /> to compare.</param>
        /// <param name="right">The second instance of <see cref="T:System.Windows.XFontStyle" /> to compare.</param>
        /// <returns>
        /// <see langword="true" /> to show the specified <see cref="T:System.Windows.XFontStyle" /> objects are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(XFontStyle left, XFontStyle right) 
            => left._style == right._style;

        /// <summary>Evaluates two instances of <see cref="T:System.Windows.XFontStyle" /> to determine inequality.</summary>
        /// <param name="left">The first instance of <see cref="T:System.Windows.XFontStyle" /> to compare.</param>
        /// <param name="right">The second instance of <see cref="T:System.Windows.XFontStyle" /> to compare.</param>
        /// <returns>
        /// <see langword="false" /> to show <paramref name="left" /> is equal to <paramref name="right" />; otherwise, <see langword="true" />.</returns>
        public static bool operator !=(XFontStyle left, XFontStyle right) 
            => !(left == right);

        /// <summary>Compares a <see cref="T:System.Windows.XFontStyle" /> with the current <see cref="T:System.Windows.XFontStyle" /> instance for equality.</summary>
        /// <param name="obj">An instance of <see cref="T:System.Windows.XFontStyle" /> to compare for equality.</param>
        /// <returns>
        /// <see langword="true" /> to show the two instances are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(XFontStyle obj) 
            => this == obj;

        /// <summary>Compares an <see cref="T:System.Object" /> with the current <see cref="T:System.Windows.XFontStyle" /> instance for equality.</summary>
        /// <param name="obj">An <see cref="T:System.Object" /> value that represents the <see cref="T:System.Windows.XFontStyle" /> to compare for equality.</param>
        /// <returns>
        /// <see langword="true" /> to show the two instances are equal; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) 
            => obj is XFontStyle xFontStyle && this == xFontStyle;

        /// <summary>Retrieves the hash code for this object.</summary>
        /// <returns>A 32-bit hash code, which is a signed integer.</returns>
        public override int GetHashCode() => (int)_style;

        /// <summary>Creates a <see cref="T:System.String" /> that represents the current <see cref="T:System.Windows.XFontStyle" /> object and is based on the <see cref="P:System.Globalization.CultureInfo.CurrentCulture" /> property information.</summary>
        /// <returns>A <see cref="T:System.String" /> that represents the value of the <see cref="T:System.Windows.XFontStyle" /> object, such as "Normal", "Italic", or "Oblique".</returns>
        public override string ToString()
            => ConvertToString(null, null);

        string IFormattable.ToString(string? format, IFormatProvider? provider)
            => ConvertToString(format, provider);

        internal int GetStyleForInternalConstruction() => (int)_style;

        string ConvertToString(string? format, IFormatProvider? provider)
        {
            return _style switch
            {
                XFontStyleValue.Normal => "Normal",
                XFontStyleValue.Oblique => "Oblique",
                XFontStyleValue.Italic => "Italic",
                _ => throw new InvalidOperationException(Invariant($"Invalid style value '{(int)_style}'."))
            };

            // why not return _style.ToString() ?
        }

        /// <summary>
        /// Simple hack to make it work...
        /// Returns Normal or Italic - bold, underline and such get lost here.
        /// </summary>
        internal static XFontStyle FromGdiFontStyle(XFontStyleEx style)
        {
            // Mask out Underline, Strikeout, etc.
            return (style & XFontStyleEx.BoldItalic) switch
            {
                XFontStyleEx.Regular => XFontStyles.Normal,
                XFontStyleEx.Bold => XFontStyles.Normal,
                XFontStyleEx.Italic => XFontStyles.Italic,
                XFontStyleEx.BoldItalic => XFontStyles.Italic,
                _ => XFontStyles.Normal
            };
        }

        readonly XFontStyleValue _style;
    }
}
