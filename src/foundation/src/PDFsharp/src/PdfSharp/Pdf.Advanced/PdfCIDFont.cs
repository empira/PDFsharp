// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Filters;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a CIDFont dictionary.
    /// The subtype can be CIDFontType0 or CIDFontType2.
    /// PDFsharp only used CIDFontType2 which is a TrueType font program.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    class PdfCIDFont : PdfFont
    {
        public PdfCIDFont(PdfDocument document)
            : base(document)
        { }

        public PdfCIDFont(PdfDocument document, PdfFontDescriptor fontDescriptor /*, XFont font*/)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Font");
            Elements.SetName(Keys.Subtype, "/CIDFontType2");
            // Create CIDSystemInfo dictionary.
            PdfDictionary cid = new();
            cid.Elements.SetString("/Ordering", "Identity");
            cid.Elements.SetString("/Registry", "Adobe");
            cid.Elements.SetInteger("/Supplement", 0);
            Elements.SetValue(Keys.CIDSystemInfo, cid);
            // #PDF-UA: 'Identity' or a stream must obviously be set for CIDFonts to satisfy PDF/UA requirements.
            Elements.SetName(Keys.CIDToGIDMap, "Identity");

            FontDescriptor = fontDescriptor;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Owner.IrefTable.TryAdd(fontDescriptor);
            Elements[Keys.FontDescriptor] = fontDescriptor.Reference;

            //FontEncoding = font.PdfOptions.FontEncoding;
            FontEncoding = PdfFontEncoding.Unicode;
        }

        public PdfCIDFont(PdfDocument document, PdfFontDescriptor fontDescriptor, byte[] fontData)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Font");
            Elements.SetName(Keys.Subtype, "/CIDFontType2");
            PdfDictionary cid = new();
            cid.Elements.SetString("/Ordering", "Identity");
            cid.Elements.SetString("/Registry", "Adobe");
            cid.Elements.SetInteger("/Supplement", 0);
            Elements.SetValue(Keys.CIDSystemInfo, cid);
            // #PDF-UA: 'Identity' or a stream must obviously be set for CIDFonts to satisfy PDF/UA requirements.
            Elements.SetName(Keys.CIDToGIDMap, "Identity");

            FontDescriptor = fontDescriptor;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            //Owner.IrefTable.Add(fontDescriptor);
            Owner.IrefTable.TryAdd(fontDescriptor);
            Elements[Keys.FontDescriptor] = fontDescriptor.Reference;

            FontEncoding = PdfFontEncoding.Unicode;
        }

        public string BaseFont
        {
            get => Elements.GetName(Keys.BaseFont);
            set => Elements.SetName(Keys.BaseFont, value);
        }

        /// <summary>
        /// Prepares the object to get saved.
        /// </summary>
        internal override void PrepareForSave()
        {
            base.PrepareForSave();
#if DEBUG_
            if (FontDescriptor._descriptor.FontFace.loca == null)
                _ = typeof(int);
#endif
            FontDescriptor.PrepareForSave();
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new sealed class Keys : PdfFont.Keys
        {
            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes;
            /// must be Font for a CIDFont dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Font")]
            public new const string Type = "/Type";

            /// <summary>
            /// (Required) The type of CIDFont; CIDFontType0 or CIDFontType2.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public new const string Subtype = "/Subtype";

            /// <summary>
            /// (Required) The PostScript name of the CIDFont. For Type 0 CIDFonts, this
            /// is usually the value of the CIDFontName entry in the CIDFont program. For
            /// Type 2 CIDFonts, it is derived the same way as for a simple TrueType font;
            /// In either case, the name can have a subset prefix if appropriate.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public new const string BaseFont = "/BaseFont";

            /// <summary>
            /// (Required) A dictionary containing entries that define the character collection
            /// of the CIDFont.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required)]
            public const string CIDSystemInfo = "/CIDSystemInfo";

            /// <summary>
            /// (Required; must be an indirect reference) A font descriptor describing the
            /// CIDFont’s default metrics other than its glyph widths.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.MustBeIndirect, typeof(PdfFontDescriptor))]
            public new const string FontDescriptor = "/FontDescriptor";

            /// <summary>
            /// (Optional) The default width for glyphs in the CIDFont.
            /// Default value: 1000.
            /// </summary>
            [KeyInfo(KeyType.Integer)]
            public const string DW = "/DW";

            /// <summary>
            /// (Optional) A description of the widths for the glyphs in the CIDFont. The
            /// array’s elements have a variable format that can specify individual widths
            /// for consecutive CIDs or one width for a range of CIDs.
            /// Default value: none (the DW value is used for all glyphs).
            /// </summary>
            [KeyInfo(KeyType.Array, typeof(PdfArray))]
            public const string W = "/W";

            /// <summary>
            /// (Optional; applies only to CIDFonts used for vertical writing) An array of two
            /// numbers specifying the default metrics for vertical writing.
            /// Default value: [880 −1000].
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string DW2 = "/DW2";

            /// <summary>
            /// (Optional; applies only to CIDFonts used for vertical writing) A description
            /// of the metrics for vertical writing for the glyphs in the CIDFont.
            /// Default value: none (the DW2 value is used for all glyphs).
            /// </summary>
            [KeyInfo(KeyType.Array, typeof(PdfArray))]
            public const string W2 = "/W2";

            /// <summary>
            /// (Optional; Type 2 CIDFonts only) A specification of the mapping from CIDs
            /// to glyph indices. If the value is a stream, the bytes in the stream contain the
            /// mapping from CIDs to glyph indices: the glyph index for a particular CID
            /// value c is a 2-byte value stored in bytes 2 × c and 2 × c + 1, where the first
            /// byte is the high-order byte. If the value of CIDToGIDMap is a name, it must
            /// be Identity, indicating that the mapping between CIDs and glyph indices is
            /// the identity mapping.
            /// Default value: Identity.
            /// This entry may appear only in a Type 2 CIDFont whose associated True-Type font 
            /// program is embedded in the PDF file.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.StreamOrName)]
            public const string CIDToGIDMap = "/CIDToGIDMap";

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
