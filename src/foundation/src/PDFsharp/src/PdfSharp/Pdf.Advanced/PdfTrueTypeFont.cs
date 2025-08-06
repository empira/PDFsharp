﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a OpenType font that is ANSI encoded in the PDF document.
    /// </summary>
    class PdfTrueTypeFont : PdfFont
    {
        public PdfTrueTypeFont(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of PdfTrueTypeFont from an XFont.
        /// </summary>
        public PdfTrueTypeFont(PdfDocument document, XFont font)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Font");
            Elements.SetName(Keys.Subtype, "/TrueType");

            // TrueType with WinAnsiEncoding only.
            OpenTypeDescriptor otDescriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptorFor(font);
            FontDescriptor = document.PdfFontDescriptorCache.GetOrCreatePdfDescriptorFor(otDescriptor, font.GlyphTypeface.GetBaseName());

            // When the font subset is created, the cmap table must be added.
            FontDescriptor.AddCmapTable = true;

            //_fontOptions = font.PdfOptions;
            //Debug.Assert(_fontOptions != null);

            _cmapInfo = new CMapInfo(otDescriptor);
            //FontDescriptor._cmapInfo2 = new(otDescriptor);

            BaseFont = font.GlyphTypeface.GetBaseName();
            // Fonts are always embedded.
            //if (_fontOptions.FontEmbedding == PdfFontEmbedding.Always)
            BaseFont = FontDescriptor.FontName;

            //Debug.Assert(_fontOptions.FontEncoding == PdfFontEncoding.WinAnsi);
            if (!IsSymbolFont)
                Encoding = "/WinAnsiEncoding";

            Owner.IrefTable.TryAdd(FontDescriptor);
            Elements[Keys.FontDescriptor] = FontDescriptor.Reference;

            //FontEncoding = font.PdfOptions.FontEncoding;
            FontEncoding = PdfFontEncoding.WinAnsi;
        }

        public PdfTrueTypeFont(PdfDocument document, XGlyphTypeface glyphTypeface)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Font");
            Elements.SetName(Keys.Subtype, "/TrueType");

            // TrueType with WinAnsiEncoding only.
            OpenTypeDescriptor otDescriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptorFor(glyphTypeface);
            FontDescriptor = document.PdfFontDescriptorCache.GetOrCreatePdfDescriptorFor(otDescriptor, glyphTypeface.GetBaseName());

            // When the font subset is created, the cmap table must be added.
            FontDescriptor.AddCmapTable = true;

            //_fontOptions = font.PdfOptions;
            //Debug.Assert(_fontOptions != null);

            _cmapInfo = new CMapInfo(otDescriptor);
            //FontDescriptor._cmapInfo2 = new(otDescriptor);

            // Fonts are always embedded.
            //if (_fontOptions.FontEmbedding == PdfFontEmbedding.Always)
            BaseFont = FontDescriptor.FontName;

            //Debug.Assert(_fontOptions.FontEncoding == PdfFontEncoding.WinAnsi);
            if (!IsSymbolFont)
                Encoding = "/WinAnsiEncoding";

            Owner.IrefTable.TryAdd(FontDescriptor);
            Elements[Keys.FontDescriptor] = FontDescriptor.Reference;

            //FontEncoding = font.PdfOptions.FontEncoding;
            FontEncoding = PdfFontEncoding.WinAnsi;
        }

        // Not needed
        //XPdfFontOptions FontOptions => _fontOptions;
        //readonly XPdfFontOptions _fontOptions = null!;

        public string BaseFont
        {
            get => Elements.GetName(Keys.BaseFont);
            set => Elements.SetName(Keys.BaseFont, value);
        }

        public int FirstChar
        {
            get => Elements.GetInteger(Keys.FirstChar);
            set => Elements.SetInteger(Keys.FirstChar, value);
        }

        public int LastChar
        {
            get => Elements.GetInteger(Keys.LastChar);
            set => Elements.SetInteger(Keys.LastChar, value);
        }

        public PdfArray Widths => (PdfArray)Elements.GetValue(Keys.Widths, VCF.CreateIndirect)!; // Because of Create.

        public string Encoding
        {
            get => Elements.GetName(Keys.Encoding);
            set => Elements.SetName(Keys.Encoding, value);
        }

        /// <summary>
        /// Prepares the object to get saved.
        /// </summary>
        internal override void PrepareForSave()
        {
            base.PrepareForSave();

            FontDescriptor.PrepareForSave();

            // #NFM TODO_OLD use only used characters
            var min = CMapInfo.MinCodePoint;
            var max = CMapInfo.MaxCodePoint;
            if (min > 32 || max < 255)
                _ = typeof(int);

            FirstChar = 32;
            LastChar = 255;
            PdfArray width = Widths;
#if DEBUG
            width.Comment = Invariant($"Width array[{FirstChar}..{LastChar}] of {typeof(PdfTrueTypeFont)} object {ObjectID}");
#endif
            //width.Elements.Clear();
            for (int idx = 32; idx < 256; idx++)
                width.Elements.Add(new PdfInteger(FontDescriptor.Descriptor.Widths[idx]));
        }

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
            /// (Required) The type of font; must be TrueType for a TrueType font.
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
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
