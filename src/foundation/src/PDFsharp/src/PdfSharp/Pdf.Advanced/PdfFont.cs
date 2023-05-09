// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Fonts;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a PDF font.
    /// </summary>
    public class PdfFont : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFont"/> class.
        /// </summary>
        public PdfFont(PdfDocument document)
            : base(document)
        { }

        internal PdfFontDescriptor FontDescriptor
        {
            get
            {
                Debug.Assert(_fontDescriptor != null);
                return _fontDescriptor ?? NRT.ThrowOnNull<PdfFontDescriptor>();
            }
            set => _fontDescriptor = value;
        }
        PdfFontDescriptor? _fontDescriptor;

        internal PdfFontEncoding FontEncoding;

        /// <summary>
        /// Gets a value indicating whether this instance is symbol font.
        /// </summary>
        public bool IsSymbolFont => FontDescriptor.IsSymbolFont;

        internal void AddChars(string text)
        {
            if (_cmapInfo != null)
                _cmapInfo.AddChars(text);
        }

        internal void AddGlyphIndices(string glyphIndices)
        {
            if (_cmapInfo != null)
                _cmapInfo.AddGlyphIndices(glyphIndices);
        }

        /// <summary>
        /// Gets or sets the CMapInfo.
        /// </summary>
        internal CMapInfo CMapInfo
        {
            get => _cmapInfo ?? NRT.ThrowOnNull<CMapInfo>();
            set => _cmapInfo = value;
        }
        internal CMapInfo? _cmapInfo;

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
        /// Adds a tag of exactly six uppercase letters to the font name 
        /// according to PDF Reference Section 5.5.3 'Font Subsets'
        /// </summary>
        internal static string CreateEmbeddedFontSubsetName(string name)
        {
            var s = new StringBuilder(64);
            byte[] bytes = Guid.NewGuid().ToByteArray();
            for (int idx = 0; idx < 6; idx++)
                s.Append((char)('A' + bytes[idx] % 26));
            s.Append('+');
            if (name.StartsWith("/", StringComparison.Ordinal))
                s.Append(name.Substring(1));
            else
                s.Append(name);
            return s.ToString();
        }

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
