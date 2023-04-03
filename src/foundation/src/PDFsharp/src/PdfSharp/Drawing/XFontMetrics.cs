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
            _unitsPerEm = unitsPerEm;
            _ascent = ascent;
            _descent = descent;
            _leading = leading;
            _lineSpacing = lineSpacing;
            _capHeight = capHeight;
            _xHeight = xHeight;
            _stemV = stemV;
            _stemH = stemH;
            _averageWidth = averageWidth;
            _maxWidth = maxWidth;
            _underlinePosition = underlinePosition;
            _underlineThickness = underlineThickness;
            _strikethroughPosition = strikethroughPosition;
            _strikethroughThickness = strikethroughThickness;
        }

        /// <summary>
        /// Gets the font name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the ascent value.
        /// </summary>
        public int UnitsPerEm => _unitsPerEm;

        readonly int _unitsPerEm;

        /// <summary>
        /// Gets the ascent value.
        /// </summary>
        public int Ascent => _ascent;

        readonly int _ascent;

        /// <summary>
        /// Gets the descent value.
        /// </summary>
        public int Descent => _descent;

        readonly int _descent;

        /// <summary>
        /// Gets the average width.
        /// </summary>
        public int AverageWidth => _averageWidth;

        readonly int _averageWidth;

        /// <summary>
        /// Gets the height of capital letters.
        /// </summary>
        public int CapHeight => _capHeight;

        readonly int _capHeight;

        /// <summary>
        /// Gets the leading value.
        /// </summary>
        public int Leading => _leading;

        readonly int _leading;

        /// <summary>
        /// Gets the line spacing value.
        /// </summary>
        public int LineSpacing => _lineSpacing;

        readonly int _lineSpacing;

        /// <summary>
        /// Gets the maximum width of a character.
        /// </summary>
        public int MaxWidth => _maxWidth;

        readonly int _maxWidth;

        /// <summary>
        /// Gets an internal value.
        /// </summary>
        public int StemH => _stemH;

        readonly int _stemH;

        /// <summary>
        /// Gets an internal value.
        /// </summary>
        public int StemV => _stemV;

        readonly int _stemV;

        /// <summary>
        /// Gets the height of a lower-case character.
        /// </summary>
        public int XHeight => _xHeight;

        readonly int _xHeight;

        /// <summary>
        /// Gets the underline position.
        /// </summary>
        public int UnderlinePosition => _underlinePosition;

        readonly int _underlinePosition;

        /// <summary>
        /// Gets the underline thickness.
        /// </summary>
        public int UnderlineThickness => _underlineThickness;

        readonly int _underlineThickness;

        /// <summary>
        /// Gets the strikethrough position.
        /// </summary>
        public int StrikethroughPosition => _strikethroughPosition;

        readonly int _strikethroughPosition;

        /// <summary>
        /// Gets the strikethrough thickness.
        /// </summary>
        public int StrikethroughThickness => _strikethroughThickness;

        readonly int _strikethroughThickness;
    }
}
