// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    enum XFontStyleValue
    {
        // Values taken from WPF.
        Normal = 0,
        Oblique = 1,
        Italic = 3
    }

    /// <summary>
    /// Provides a set of static predefined font style /> values.
    /// </summary>
    public static class XFontStyles
    {
        /// <summary>
        /// Specifies a normal font style. />
        /// </summary>
        public static XFontStyle Normal => new(XFontStyleValue.Normal);

        /// <summary>
        /// Specifies an oblique font style.
        /// </summary>
        public static XFontStyle Oblique => new(XFontStyleValue.Oblique);

        /// <summary>
        /// Specifies an italic font style. />
        /// </summary>
        public static XFontStyle Italic => new(XFontStyleValue.Italic);

        internal static bool XFontStyleStringToKnownStyle(string style, IFormatProvider provider, ref XFontStyle xFontStyle)
        {
            if (style.Equals("Normal", StringComparison.OrdinalIgnoreCase))
            {
                xFontStyle = Normal;
                return true;
            }
            if (style.Equals("Italic", StringComparison.OrdinalIgnoreCase))
            {
                xFontStyle = Italic;
                return true;
            }
            if (!style.Equals("Oblique", StringComparison.OrdinalIgnoreCase))
                return false;

            xFontStyle = Oblique;
            return true;
        }
    }
}
