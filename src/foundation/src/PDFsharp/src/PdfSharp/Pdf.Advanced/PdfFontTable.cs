// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Fonts.StandardFonts;
using PdfSharp.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Pdf.Advanced
{
    enum FontType
    {
        Undefined = 0,

        /// <summary>
        /// TrueType with WinAnsi encoding.
        /// </summary>
        TrueTypeWinAnsi = 1,  // #RENAME better name

        /// <summary>
        /// TrueType with Identity-H or Identity-V encoding (Unicode).
        /// </summary>
        Type0Unicode = 2,  // #RENAME better name
    }

    /// <summary>
    /// Contains all used fonts of a document.
    /// </summary>
    sealed class PdfFontTable : PdfResourceTable
    {
        /// <summary>
        /// Initializes a new instance of this class, which is a singleton for each document.
        /// </summary>
        public PdfFontTable(PdfDocument document)
            : base(document)
        {
        }

        /// <summary>
        /// Gets a PdfFont from an XFont. If no PdfFont already exists, a new one is created.
        /// </summary>
        public PdfFont GetOrCreateFont(XGlyphTypeface glyphTypeface, FontType fontType)
        {
            // TODO_OLD: The selector should be the glyph typeface key plus the font type key.
            var selector = ComputePdfFontKey(glyphTypeface, fontType);
            if (!_fonts.TryGetValue(selector, out var pdfFont))
            {
                if (fontType == FontType.Type0Unicode)
                    pdfFont = new PdfType0Font(Owner, glyphTypeface, false);
                else if (fontType == FontType.TrueTypeWinAnsi)
                    pdfFont = new PdfTrueTypeFont(Owner, glyphTypeface);
                else
                    throw new InvalidOperationException($"Invalid font type '{fontType}'.");
                Debug.Assert(pdfFont.Owner == Owner);
                _fonts[selector] = pdfFont;
            }
            return pdfFont;
        }

        /// <summary>
        /// Caches a font from an existing document.<br></br>
        /// Used to prevent adding new fonts when filling existing AcroForms.
        /// </summary>
        /// <param name="fontDict"></param>
        /// <param name="glyphTypeface"></param>
        /// <param name="fontType"></param>
        internal void CacheExistingFont(PdfDictionary fontDict, XGlyphTypeface glyphTypeface, FontType fontType)
        {
            var selector = ComputePdfFontKey(glyphTypeface, fontType);
            if (!_fonts.ContainsKey(selector))
            {
                var otDescriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptorFor(glyphTypeface);
                var descriptor = Owner.PdfFontDescriptorCache.GetOrCreatePdfDescriptorFor(otDescriptor, glyphTypeface.GetBaseName());

                var font = new PdfFont(fontDict, descriptor, PdfFontEncoding.Automatic);
                _fonts[selector] = font;
            }
        }

#if true
        /// <summary>
        /// Gets a PdfFont from a font program. If no PdfFont already exists, a new one is created.
        /// </summary>
        public PdfFont GetFont(string idName, byte[] fontData)
        {
            Debug.Assert(false, "Should not come here anymore.");
            return null!;
#if true_
            //FontSelector selector = new FontSelector(idName);
            string selector = null; // ComputeKey(font); //new FontSelector(font);
            if (!_fonts.TryGetValue(selector, out var pdfFont))
            {
                //if (font.Unicode)
                pdfFont = new PdfType0Font(Owner, idName, fontData, false);
                //else
                //  pdfFont = new PdfTrueTypeFont(_owner, font);
                //pdfFont.Document = _document;
                Debug.Assert(pdfFont.Owner == Owner);
                _fonts[selector] = pdfFont;
            }
            return pdfFont;
#endif
        }
#endif

        /// <summary>
        /// Tries to get a PdfFont from the font dictionary.
        /// Returns null if no such PdfFont exists.
        /// </summary>
        public PdfFont? TryGetFont(string idName)
        {
            Debug.Assert(false, "Should not come here anymore.");
            return null;

#if true_
            //FontSelector selector = new FontSelector(idName);
            string selector = null;
            _fonts.TryGetValue(selector, out var pdfFont);
            return pdfFont;
#endif
        }

        internal static string ComputePdfFontKey(XGlyphTypeface glyphTypeface, FontType fontType)
        {
            // fontType must be defined to compute the key.
            Debug.Assert(fontType is FontType.TrueTypeWinAnsi or FontType.Type0Unicode);

            // TODO_OLD Check if StringBuilder is more efficient here.
            //var glyphTypeface = font.GlyphTypeface;

            // #NFM Use gtk here. But the gtk without simulation flags.

            var faceName = glyphTypeface.FontFace.FullFaceName.ToLowerInvariant();
            var bold = glyphTypeface.IsBold;
            var italic = glyphTypeface.IsItalic;
            var type = fontType == FontType.TrueTypeWinAnsi ? "+A" : "+U";
            var key = bold switch
            {
                false when !italic => faceName + type,
                true when !italic => faceName + "/b" + type,
                false when italic => faceName + "/i" + type,
                _ => faceName + "/bi" + type
            };
            return key;
        }

        /// <summary>
        /// Map from PdfFont selector to PdfFont.
        /// </summary>
        readonly Dictionary<string, PdfFont> _fonts = [];

        public void PrepareForSave()
        {
            foreach (var font in _fonts.Values)
                font.PrepareForSave();
        }
    }
}
