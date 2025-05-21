// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.


using PdfSharp.Fonts;
using PdfSharp.Internal;
using System;
using System.Collections;
using System.Text;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Type1 font used for the 14 standard fonts defined in the PDF-specification.
    /// </summary>
    internal sealed class PdfType1Font : PdfFont
    {
        public PdfType1Font(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "Font");
            Elements.SetName(Keys.Subtype, "Type1");
            Elements.SetName(Keys.Encoding, "WinAnsiEncoding");
        }

        public string BaseFont
        {
            get { return Elements.GetName(Keys.BaseFont); }
            set { Elements.SetName(Keys.BaseFont, value); }
        }

        internal override void AddChars(CodePointGlyphIndexPair[] codePoints)
        {
            // nothing to do here
        }

        //    internal override void AssignObjectID(ref int objectID)
        //    {
        //      SetObjectID(ref objectID);
        //    }
        //
        //    internal override void WriteObject(Stream stream)
        //    {
        //      base.WriteObject(stream);
        //      StringBuilder pdf = new StringBuilder();
        //      pdf.AppendFormat("{0} 0 obj\n<<\n/Type /Font\n/Subtype /Type1\n/BaseFont /Helvetica\n/Encoding /WinAnsiEncoding\n>>\nendobj\n", ObjectID);
        //      WriteString(stream, pdf.ToString());
        //    }
        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new sealed class Keys : PdfFont.Keys
        {
            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes;
            /// must be Font for a font dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Font")]
            public new const string Type = "/Type";

            /// <summary>
            /// (Required) The type of font; must be Type1 for a Type 1 font.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public new const string Subtype = "/Subtype";

            /// <summary>
            /// (Required in PDF 1.0; optional otherwise) The name by which this font is 
            /// referenced in the Font subdictionary of the current resource dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            /// <summary>
            /// (Required) The PostScript name of the font. For Type 1 fonts, this is usually
            /// the value of the FontName entry in the font program; for more information.
            /// The Post-Script name of the font can be used to find the font’s definition in 
            /// the consumer application or its environment. It is also the name that is used when
            /// printing to a PostScript output device.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public new const string BaseFont = "/BaseFont";

            /// <summary>
            /// (Required except for the standard 14 fonts) The first character code defined 
            /// in the font’s Widths array.
            /// </summary>
            [KeyInfo(KeyType.Integer)]
            public const string FirstChar = "/FirstChar";

            /// <summary>
            /// (Required except for the standard 14 fonts) The last character code defined
            /// in the font’s Widths array.
            /// </summary>
            [KeyInfo(KeyType.Integer)]
            public const string LastChar = "/LastChar";

            /// <summary>
            /// (Required except for the standard 14 fonts; indirect reference preferred)
            /// An array of (LastChar - FirstChar + 1) widths, each element being the glyph width
            /// for the character code that equals FirstChar plus the array index. For character
            /// codes outside the range FirstChar to LastChar, the value of MissingWidth from the 
            /// FontDescriptor entry for this font is used. The glyph widths are measured in units 
            /// in which 1000 units corresponds to 1 unit in text space. These widths must be 
            /// consistent with the actual widths given in the font program. 
            /// </summary>
            [KeyInfo(KeyType.Array, typeof(PdfArray))]
            public const string Widths = "/Widths";

            /// <summary>
            /// (Required except for the standard 14 fonts; must be an indirect reference)
            /// A font descriptor describing the font’s metrics other than its glyph widths.
            /// Note: For the standard 14 fonts, the entries FirstChar, LastChar, Widths, and 
            /// FontDescriptor must either all be present or all be absent. Ordinarily, they are
            /// absent; specifying them enables a standard font to be overridden.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.MustBeIndirect, typeof(PdfFontDescriptor))]
            public new const string FontDescriptor = "/FontDescriptor";

            /// <summary>
            /// (Optional) A specification of the font’s character encoding if different from its
            /// built-in encoding. The value of Encoding is either the name of a predefined
            /// encoding (MacRomanEncoding, MacExpertEncoding, or WinAnsiEncoding, as described in 
            /// Appendix D) or an encoding dictionary that specifies differences from the font’s
            /// built-in encoding or from a specified predefined encoding.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Dictionary)]
            public const string Encoding = "/Encoding";

            /// <summary>
            /// (Optional; PDF 1.2) A stream containing a CMap file that maps character
            /// codes to Unicode values.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string ToUnicode = "/ToUnicode";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta
            {
                get
                {
                    if (_meta == null)
                        _meta = CreateMeta(typeof(Keys));
                    return _meta;
                }
            }
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta
        {
            get { return Keys.Meta; }
        }
    }
}
