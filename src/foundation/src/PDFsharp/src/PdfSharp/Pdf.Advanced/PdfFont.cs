// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal.OpenType;
using PdfSharp.Fonts;
using CodePointGlyphIndexPair = PdfSharp.Internal.OpenType.CodePointGlyphIndexPair;

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
        {
            CMapInfo = null!;
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFont(PdfDictionary dict)
            : base(dict)
        {
            CMapInfo = null!;
        }

        internal PdfFontDescriptor FontDescriptor
        {
            get
            {
                Debug.Assert(_fontDescriptor != null);
                return _fontDescriptor ?? NRT.ThrowOnNull<PdfFontDescriptor>();
            }
            init => _fontDescriptor = value;
        }
        readonly PdfFontDescriptor _fontDescriptor = null!;

        internal PdfFontEncoding FontEncoding { get; init; }

        /// <summary>
        /// Gets a value indicating whether this instance is symbol font.
        /// </summary>
        public bool IsSymbolFont => FontDescriptor.IsSymbolFont;

        internal void AddChars(CodePointGlyphIndexPair[] codePoints)
        {
            CMapInfo.AddChars(codePoints);
            _fontDescriptor.CMapInfo.AddChars(codePoints);
        }

        /// <summary>
        /// Gets or sets the CMapInfo of a PDF font.
        /// For a Unicode font only these characters come to the ToUnicode map.
        /// </summary>
        internal CMapInfo CMapInfo { get; init; }

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

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// An internal hack to allow to interact PdfSharp.Graphics.FontFace with
    /// PDFsharp core lib.
    /// </summary>
    interface IFontProxy // #PSGFX  IXGlyphTypeFaceProxy
    {
        void CheckVersion();

        string Key { get; }

        OpenTypeFontFace FontFace { get; }

        string FaceName { get; }

        string GetBaseName();
    }
}
