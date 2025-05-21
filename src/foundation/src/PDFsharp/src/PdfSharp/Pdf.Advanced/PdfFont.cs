// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the base class of a PDF font.
    /// </summary>
    public class PdfFont : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFont"/> class.
        /// </summary>
        protected PdfFont(PdfDocument document)
            : base(document)
        { }

        internal PdfFont(PdfDictionary dict, PdfFontDescriptor fontDescriptor, PdfFontEncoding encoding)
            :base(dict)
        {
            FontDescriptor = fontDescriptor;
            FontEncoding = encoding;
            _cmapInfo = new CMapInfo(fontDescriptor.Descriptor);
            _toUnicodeMap = new PdfToUnicodeMap(Owner);
        }

        internal PdfFontDescriptor FontDescriptor
        {
            get
            {
                Debug.Assert(_fontDescriptor != null);
                return _fontDescriptor ?? NRT.ThrowOnNull<PdfFontDescriptor>();
            }
            set => _fontDescriptor = value;
        }
        PdfFontDescriptor _fontDescriptor = default!;

        internal PdfFontEncoding FontEncoding { get; init; }

        /// <summary>
        /// Gets a value indicating whether this instance is symbol font.
        /// </summary>
        public bool IsSymbolFont => FontDescriptor.IsSymbolFont;

        internal virtual void AddChars(CodePointGlyphIndexPair[] codePoints)
        {
            _cmapInfo.AddChars(codePoints);
            _fontDescriptor.CMapInfo.AddChars(codePoints);
        }

        /// <summary>
        /// Gets or sets the CMapInfo of a PDF font.
        /// For a Unicode font only this characters come to the ToUnicode map.
        /// </summary>
        internal CMapInfo CMapInfo
        {
            get => _cmapInfo ?? NRT.ThrowOnNull<CMapInfo>();
            set => _cmapInfo = value;
        }
        internal CMapInfo _cmapInfo = default!;

        /// <summary>
        /// Gets or sets ToUnicodeMap.
        /// </summary>
        internal PdfToUnicodeMap ToUnicodeMap
        {
            get => _toUnicodeMap ?? NRT.ThrowOnNull<PdfToUnicodeMap>();
            set => _toUnicodeMap = value;
        }
        // ReSharper disable once InconsistentNaming
        internal PdfToUnicodeMap? _toUnicodeMap;

        /// <summary>
        /// Predefined keys common to all font dictionaries.
        /// </summary>
        public class Keys : KeysBase
        {
            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes;
            /// must be Font for a font dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Font")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of font.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string Subtype = "/Subtype";

            /// <summary>
            /// (Required) The PostScript name of the font.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string BaseFont = "/BaseFont";

            /// <summary>
            /// (Required except for the standard 14 fonts; must be an indirect reference)
            /// A font descriptor describing the font’s metrics other than its glyph widths.
            /// Note: For the standard 14 fonts, the entries FirstChar, LastChar, Widths, and 
            /// FontDescriptor must either all be present or all be absent. Ordinarily, they are
            /// absent; specifying them enables a standard font to be overridden.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.MustBeIndirect, typeof(PdfFontDescriptor))]
            public const string FontDescriptor = "/FontDescriptor";
        }
    }
}
