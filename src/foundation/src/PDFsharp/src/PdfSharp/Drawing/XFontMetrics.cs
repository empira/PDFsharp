// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Collects information of a font.
    /// </summary>
    public sealed class XFontMetrics
    {
        internal XFontMetrics(string name, int unitsPerEm, int ascent, int descent, int leading, int lineSpacing,
            int capHeight, int xHeight, int stemV, int stemH, int averageWidth, int maxWidth,
            int underlinePosition, int underlineThickness, int strikethroughPosition, int strikethroughThickness)
        {
            Name = name;
            UnitsPerEm = unitsPerEm;
            Ascent = ascent;
            Descent = descent;
            Leading = leading;
            LineSpacing = lineSpacing;
            CapHeight = capHeight;
            XHeight = xHeight;
            StemV = stemV;
            StemH = stemH;
            AverageWidth = averageWidth;
            MaxWidth = maxWidth;
            UnderlinePosition = underlinePosition;
            UnderlineThickness = underlineThickness;
            StrikethroughPosition = strikethroughPosition;
            StrikethroughThickness = strikethroughThickness;
        }

        /// <summary>
        /// Gets the font name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the ascent value.
        /// </summary>
        public int UnitsPerEm { get; }

        /// <summary>
        /// Gets the ascent value.
        /// </summary>
        public int Ascent { get; }

        /// <summary>
        /// Gets the descent value.
        /// </summary>
        public int Descent { get; }

        /// <summary>
        /// Gets the average width.
        /// </summary>
        public int AverageWidth { get; }

        /// <summary>
        /// Gets the height of capital letters.
        /// </summary>
        public int CapHeight { get; }

        /// <summary>
        /// Gets the leading value.
        /// </summary>
        public int Leading { get; }

        /// <summary>
        /// Gets the line spacing value.
        /// </summary>
        public int LineSpacing { get; }

        /// <summary>
        /// Gets the maximum width of a character.
        /// </summary>
        public int MaxWidth { get; }

        /// <summary>
        /// Gets an internal value.
        /// </summary>
        public int StemH { get; }

        /// <summary>
        /// Gets an internal value.
        /// </summary>
        public int StemV { get; }

        /// <summary>
        /// Gets the height of a lower-case character.
        /// </summary>
        public int XHeight { get; }

        /// <summary>
        /// Gets the underline position.
        /// </summary>
        public int UnderlinePosition { get; }

        /// <summary>
        /// Gets the underline thickness.
        /// </summary>
        public int UnderlineThickness { get; }

        /// <summary>
        /// Gets the strikethrough position.
        /// </summary>
        public int StrikethroughPosition { get; }

        /// <summary>
        /// Gets the strikethrough thickness.
        /// </summary>
        public int StrikethroughThickness { get; }
    }
}
