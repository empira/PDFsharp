// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Advanced
{
    enum FontType
    {
        /// <summary>
        /// TrueType with WinAnsi encoding.
        /// </summary>
        TrueType = 1,

        /// <summary>
        /// TrueType with Identity-H or Identity-V encoding (Unicode).
        /// </summary>
        Type0 = 2,
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
        { }

        /// <summary>
        /// Gets a PdfFont from an XFont. If no PdfFont already exists, a new one is created.
        /// </summary>
        public PdfFont GetFont(XFont font)
        {
            var selector = font.Selector;
            if (selector == null)
            {
                selector = ComputeKey(font); //new FontSelector(font);
                font.Selector = selector;
            }

            if (!_fonts.TryGetValue(selector, out var pdfFont))
            {
                if (font.Unicode)
                    pdfFont = new PdfType0Font(Owner, font, font.IsVertical);
                else
                    pdfFont = new PdfTrueTypeFont(Owner, font);
                //pdfFont.Document = _document;
                Debug.Assert(pdfFont.Owner == Owner);
                _fonts[selector] = pdfFont;
            }
            return pdfFont;
        }

#if true
        /// <summary>
        /// Gets a PdfFont from a font program. If no PdfFont already exists, a new one is created.
        /// </summary>
        public PdfFont GetFont(string idName, byte[] fontData)
        {
            Debug.Assert(false);
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
        }
#endif

        /// <summary>
        /// Tries to get a PdfFont from the font dictionary.
        /// Returns null if no such PdfFont exists.
        /// </summary>
        public PdfFont? TryGetFont(string idName)
        {
            Debug.Assert(false);
            //FontSelector selector = new FontSelector(idName);
            string selector = null;
            _fonts.TryGetValue(selector, out var pdfFont);
            return pdfFont;
        }

        internal static string ComputeKey(XFont font)
        {
            // TODO Check if StringBuilder is more efficient here.
            var glyphTypeface = font.GlyphTypeface;
#if true
            // Attempt to make it more efficient.
            var bold = glyphTypeface.IsBold;
            var italic = glyphTypeface.IsItalic;
            if (!bold && !italic)
                return glyphTypeface.Fontface.FullFaceName.ToLowerInvariant() + font.Unicode;
            else if (bold && !italic)
                return glyphTypeface.Fontface.FullFaceName.ToLowerInvariant() + "/b" + font.Unicode;
            else if (!bold && italic)
                return glyphTypeface.Fontface.FullFaceName.ToLowerInvariant() + "/i" + font.Unicode;
            // else if (bold && italic)
            return glyphTypeface.Fontface.FullFaceName.ToLowerInvariant() + "/b/i" + font.Unicode;
#else
            string key = glyphTypeface.Fontface.FullFaceName.ToLowerInvariant() +
                (glyphTypeface.IsBold ? "/b" : "") + (glyphTypeface.IsItalic ? "/i" : "") + font.Unicode;
            return key;
#endif
        }

        /// <summary>
        /// Map from PdfFontSelector to PdfFont.
        /// </summary>
        readonly Dictionary<string, PdfFont> _fonts = new Dictionary<string, PdfFont>();

        public void PrepareForSave()
        {
            foreach (var font in _fonts.Values)
                font.PrepareForSave();
        }
    }
}
