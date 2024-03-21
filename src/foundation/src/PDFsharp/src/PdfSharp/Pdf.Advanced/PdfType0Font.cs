// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a composite PDF font. Used for Unicode glyph encoding.
    /// </summary>
    sealed class PdfType0Font : PdfFont
    {
        public PdfType0Font(PdfDocument document)
            : base(document)
        { }
       
        public PdfType0Font(PdfDocument document, XGlyphTypeface glyphTypeface, bool vertical)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Font");
            Elements.SetName(Keys.Subtype, "/Type0");
            Elements.SetName(Keys.Encoding, vertical ? "/Identity-V" : "/Identity-H");

            var otDescriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptorFor(glyphTypeface);
#if SHARED_FONTDESCRIPTOR
            FontDescriptor = document.PdfFontDescriptorCache.GetOrCreatePdfDescriptorFor(otDescriptor, glyphTypeface.GetBaseName());
#else
            FontDescriptor = new PdfFontDescriptor(document, otDescriptor);
#endif
            //FontOptions = font.PdfOptions;
            //Debug.Assert(FontOptions != null);

            _cmapInfo = new CMapInfo(otDescriptor);
            _descendantFont = new PdfCIDFont(document, FontDescriptor /*, font*/)
            {
                // Base font uses the same cmap info.
                CMapInfo = _cmapInfo
            };

            // Create ToUnicode map
            _toUnicodeMap = new PdfToUnicodeMap(document, _cmapInfo);
            document.Internals.AddObject(_toUnicodeMap);
            Elements.Add(Keys.ToUnicode, _toUnicodeMap);

#if SHARED_FONTDESCRIPTOR
            BaseFont = glyphTypeface.GetBaseName();
            // CID fonts are always embedded
            BaseFont = FontDescriptor.FontName2;
            _descendantFont.BaseFont = BaseFont;
#else
            BaseFont = glyphTypeface.GetBaseName();
            // CID fonts are always embedded
            BaseFont = FontDescriptor.CreateEmbeddedFontSubsetName(BaseFont);

            FontDescriptor.FontName2 = BaseFont;
            _descendantFont.BaseFont = BaseFont;
#endif
            PdfArray descendantFonts = new PdfArray(document);
            Owner.IrefTable.Add(_descendantFont);
            descendantFonts.Elements.Add(_descendantFont.Reference!); // Reference is set in Add(_descendantFont).
            Elements[Keys.DescendantFonts] = descendantFonts;
        }

#if true_  // May be superfluous.
        public PdfType0Font(PdfDocument document, string idName, byte[] fontData, bool vertical)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Font");
            Elements.SetName(Keys.Subtype, "/Type0");
            Elements.SetName(Keys.Encoding, vertical ? "/Identity-V" : "/Identity-H");

            OpenTypeDescriptor otDescriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptor(idName, fontData);
            //FontDescriptor = new PdfFontDescriptor(document, ttDescriptor);
            FontDescriptor = document.PdfFontDescriptorCache.GetOrCreatePdfDescriptorFor(otDescriptor);

            FontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
            Debug.Assert(FontOptions != null);

            _cmapInfo0 = new CMapInfo(otDescriptor);
            //FontDescriptor._cmapInfo2 = new(otDescriptor);
            _descendantFont = new PdfCIDFont(document, FontDescriptor, fontData)
            {
                // Base font uses the same cmap info.
                CMapInfo = _cmapInfo0
            };
            //_descendantFont.FontDescriptor._cmapInfo2 = new(otDescriptor);


            // Create ToUnicode map
            _toUnicodeMap = new PdfToUnicodeMap(document, _cmapInfo0);
            document.Internals.AddObject(_toUnicodeMap);
            Elements.Add(Keys.ToUnicode, _toUnicodeMap);

            //BaseFont = ttDescriptor.FontName.Replace(" ", "");
            BaseFont = otDescriptor.FontName;

            // CID fonts are always embedded
            if (!BaseFont.Contains("+"))  // HACK in PdfType0Font
                BaseFont = FontDescriptor.CreateEmbeddedFontSubsetName(BaseFont);

            FontDescriptor.FontName = BaseFont;
            _descendantFont.BaseFont = BaseFont;

            PdfArray descendantFonts = new PdfArray(document);
            Owner.IrefTable.Add(_descendantFont);
            descendantFonts.Elements.Add(_descendantFont.Reference!);
            Elements[Keys.DescendantFonts] = descendantFonts;
        }
#endif

        XPdfFontOptions FontOptions { get; } = default!;

        public string BaseFont
        {
            get => Elements.GetName(Keys.BaseFont);
            set => Elements.SetName(Keys.BaseFont, value);
        }

        internal PdfCIDFont DescendantFont => _descendantFont;

        readonly PdfCIDFont _descendantFont = default!;

        internal override void PrepareForSave()
        {
            base.PrepareForSave();

            // Use GetGlyphIndices to create the widths array.
            var descriptor = FontDescriptor.Descriptor;
            var w = new StringBuilder("[");
            if (_cmapInfo != null!)
            {
                int[] glyphIndices = _cmapInfo.GetGlyphIndices();
                int count = glyphIndices.Length;
                int[] glyphWidths = new int[count];

                for (int idx = 0; idx < count; idx++)
                    glyphWidths[idx] = descriptor.GlyphIndexToPdfWidth(glyphIndices[idx]);

                //TODO: optimize order of indices

                for (int idx = 0; idx < count; idx++)
                    w.AppendFormat("{0}[{1}]", glyphIndices[idx], glyphWidths[idx]);
                w.Append("]");
                _descendantFont.Elements.SetValue(PdfCIDFont.Keys.W, new PdfLiteral(w.ToString()));
            }
            _descendantFont.PrepareForSave();
            ToUnicodeMap.PrepareForSave();
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
            /// (Required) The type of font; must be Type0 for a Type 0 font.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public new const string Subtype = "/Subtype";

            /// <summary>
            /// (Required) The PostScript name of the font. In principle, this is an arbitrary
            /// name, since there is no font program associated directly with a Type 0 font
            /// dictionary. The conventions described here ensure maximum compatibility
            /// with existing Acrobat products.
            /// If the descendant is a Type 0 CIDFont, this name should be the concatenation
            /// of the CIDFont’s BaseFont name, a hyphen, and the CMap name given in the
            /// Encoding entry (or the CMapName entry in the CMap). If the descendant is a
            /// Type 2 CIDFont, this name should be the same as the CIDFont’s BaseFont name.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public new const string BaseFont = "/BaseFont";

            /// <summary>
            /// (Required) The name of a predefined CMap, or a stream containing a CMap
            /// that maps character codes to font numbers and CIDs. If the descendant is a
            /// Type 2 CIDFont whose associated TrueType font program is not embedded
            /// in the PDF file, the Encoding entry must be a predefined CMap name.
            /// </summary>
            [KeyInfo(KeyType.StreamOrName | KeyType.Required)]
            public const string Encoding = "/Encoding";

            /// <summary>
            /// (Required) A one-element array specifying the CIDFont dictionary that is the
            /// descendant of this Type 0 font.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string DescendantFonts = "/DescendantFonts";

            /// <summary>
            /// ((Optional) A stream containing a CMap file that maps character codes to
            /// Unicode values.
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
                    if (Keys._meta == null)
                        Keys._meta = CreateMeta(typeof(Keys));
                    return Keys._meta;
                }
            }

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
