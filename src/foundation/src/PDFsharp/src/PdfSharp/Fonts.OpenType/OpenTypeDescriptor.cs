// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Pdf.Internal;
#if !EDF_CORE
using PdfSharp.Drawing;
#endif

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// The OpenType font descriptor.
    /// Currently the only font type PDFsharp supports.
    /// </summary>
    sealed class OpenTypeDescriptor : FontDescriptor
    {
        /// <summary>
        /// New...
        /// </summary>
        public OpenTypeDescriptor(string fontDescriptorKey, string name, XFontStyleEx stlye, OpenTypeFontface fontface, XPdfFontOptions options)
            : base(fontDescriptorKey)
        {
            FontFace = fontface;
            FontName = name;
            Initialize();
        }

        public OpenTypeDescriptor(string fontDescriptorKey, XFont font)
            : base(fontDescriptorKey)
        {
            try
            {
                FontFace = font.GlyphTypeface.Fontface;
                FontName = font.Name;
                Initialize();
            }
            catch
            {
                GetType();
                throw;
            }
        }

        internal OpenTypeDescriptor(string fontDescriptorKey, string idName, byte[] fontData)
            : base(fontDescriptorKey)
        {
            try
            {
                FontFace = new OpenTypeFontface(fontData, idName);
                // Try to get real name from name table
                if (idName.Contains("XPS-Font-") && FontFace.name != null && FontFace.name.Name.Length != 0)
                {
                    string tag = String.Empty;
                    if (idName.IndexOf("+", StringComparison.Ordinal) == 6)
                        tag = idName.Substring(0, 6);
                    idName = tag + "+" + FontFace.name.Name;
                    if (FontFace.name.Style.Length != 0)
                        idName += "," + FontFace.name.Style;
                    //idName = idName.Replace(" ", "");
                }
                FontName = idName;
                Initialize();
            }
            catch (Exception)
            {
                GetType();
                throw;
            }
        }

        internal OpenTypeFontface FontFace;

        void Initialize()
        {
            // TODO: Respect embedding restrictions.
            //bool embeddingRestricted = fontData.os2.fsType == 0x0002;

            //fontName = image.n
            ItalicAngle = FontFace.post.italicAngle;

            XMin = FontFace!.head!.xMin;
            YMin = FontFace.head.yMin;
            XMax = FontFace.head.xMax;
            YMax = FontFace.head.yMax;

            UnderlinePosition = FontFace.post.underlinePosition;
            UnderlineThickness = FontFace.post.underlineThickness;

            // PDFlib states that some Apple fonts miss the OS/2 table.
            Debug.Assert(FontFace.os2 != null, "TrueType font has no OS/2 table.");

            StrikeoutPosition = FontFace.os2.yStrikeoutPosition;
            StrikeoutSize = FontFace.os2.yStrikeoutSize;

            // No documentation found how to get the set vertical stems width from the
            // TrueType tables.
            // The following formula comes from PDFlib Lite source code. Acrobat 5.0 sets
            // /StemV to 0 always. I think the value doesn't matter.
            //float weight = (float)(image.os2.usWeightClass / 65.0f);
            //stemV = (int)(50 + weight * weight);  // MAGIC
            StemV = 0;

            UnitsPerEm = FontFace.head.unitsPerEm;

            // Calculate Ascent, Descent, Leading and LineSpacing like in WPF Source Code (see FontDriver.ReadBasicMetrics)

            // OS/2 is an optional table, but we can't determine if it is existing in this font.
            bool os2SeemsToBeEmpty = FontFace.os2.sTypoAscender == 0 && FontFace.os2.sTypoDescender == 0 && FontFace.os2.sTypoLineGap == 0;
            //Debug.Assert(!os2SeemsToBeEmpty); // Are there fonts without OS/2 table?

            bool dontUseWinLineMetrics = (FontFace.os2.fsSelection & 128) != 0;
            if (!os2SeemsToBeEmpty && dontUseWinLineMetrics)
            {
                // Comment from WPF: The font specifies that the sTypoAscender, sTypoDescender, and sTypoLineGap fields are valid and
                // should be used instead of winAscent and winDescent.
                int typoAscender = FontFace.os2.sTypoAscender;
                int typoDescender = FontFace.os2.sTypoDescender;
                int typoLineGap = FontFace.os2.sTypoLineGap;

                // Comment from WPF: We include the line gap in the ascent so that white space is distributed above the line. (Note that
                // the typo line gap is a different concept than "external leading".)
                Ascender = typoAscender + typoLineGap;
                // Comment from WPF: Typo descent is a signed value where the positive direction is up. It is therefore typically negative.
                // A signed typo descent would be quite unusual as it would indicate the descender was above the baseline
                Descender = -typoDescender;
                LineSpacing = typoAscender + typoLineGap - typoDescender;
            }
            else
            {
                // Comment from WPF: get the ascender field
                int ascender = FontFace.hhea.ascender;
                // Comment from WPF: get the descender field; this is measured in the same direction as ascender and is therefore 
                // normally negative whereas we want a positive value; however some fonts get the sign wrong
                // so instead of just negating we take the absolute value.
                int descender = Math.Abs(FontFace.hhea.descender);
                // Comment from WPF: get the lineGap field and make sure it's >= 0 
                int lineGap = Math.Max((short)0, FontFace.hhea.lineGap);

                if (!os2SeemsToBeEmpty)
                {
                    // Comment from WPF: we could use sTypoAscender, sTypoDescender, and sTypoLineGap which are supposed to represent
                    // optimal typographic values not constrained by backwards compatibility; however, many fonts get 
                    // these fields wrong or get them right only for Latin text; therefore we use the more reliable 
                    // platform-specific Windows values. We take the absolute value of the win32descent in case some
                    // fonts get the sign wrong. 
                    int winAscent = FontFace.os2.usWinAscent;
                    int winDescent = Math.Abs(FontFace.os2.usWinDescent);

                    Ascender = winAscent;
                    Descender = winDescent;
                    // Comment from WPF: The following calculation for designLineSpacing is per [....]. The default line spacing 
                    // should be the sum of the Mac ascender, descender, and lineGap unless the resulting value would
                    // be less than the cell height (winAscent + winDescent) in which case we use the cell height.
                    // See also http://www.microsoft.com/typography/otspec/recom.htm.
                    // Note that in theory it's valid for the baseline-to-baseline distance to be less than the cell
                    // height. However, Windows has never allowed this for Truetype fonts, and fonts built for Windows
                    // sometimes rely on this behavior and get the hha values wrong or set them all to zero.
                    LineSpacing = Math.Max(lineGap + ascender + descender, winAscent + winDescent);
                }
                else
                {
                    Ascender = ascender;
                    Descender = descender;
                    LineSpacing = ascender + descender + lineGap;
                }
            }

            Debug.Assert(Descender >= 0);

            int cellHeight = Ascender + Descender;
            int internalLeading = cellHeight - UnitsPerEm; // Not used, only for debugging.
            int externalLeading = LineSpacing - cellHeight;
            Leading = externalLeading;

            // sCapHeight and sxHeight are only valid if Version >= 2
            if (FontFace.os2.version >= 2 && FontFace.os2.sCapHeight != 0)
                CapHeight = FontFace.os2.sCapHeight;
            else
                CapHeight = Ascender;

            if (FontFace.os2.version >= 2 && FontFace.os2.sxHeight != 0)
                XHeight = FontFace.os2.sxHeight;
            else
                XHeight = (int)(0.66 * Ascender);

            //flags = image.

#if !EDF_CORE
            Encoding ansi = PdfEncoders.WinAnsiEncoding; // System.Text.Encoding.Default;
#else
            Encoding ansi = null; //$$$ PdfEncoders.WinAnsiEncoding; // System.Text.Encoding.Default;
#endif

            Encoding unicode = Encoding.Unicode;
            byte[] bytes = new byte[256];

            bool symbol = FontFace.cmap.symbol;
            Widths = new int[256];
            for (int idx = 0; idx < 256; idx++)
            {
                bytes[idx] = (byte)idx;
                // PDFlib handles some font flaws here...
                // We wait for bug reports.

                char ch = (char)idx;
                string s = ansi.GetString(bytes, idx, 1);
                if (s.Length != 0)
                {
                    if (s[0] != ch)
                        ch = s[0];
                }

                //Debug.Assert(ch == idx);

                //int glyphIndex;
                //if (symbol)
                //{
                //    glyphIndex = idx + (FontFace.os2. usFirstCharIndex & 0xFF00);
                //    glyphIndex = CharCodeToGlyphIndex((char)glyphIndex);
                //}
                //else
                //{
                //    //Debug.Assert(idx + (fontData.os2.usFirstCharIndex & 0xFF00) == idx);
                //    //glyphIndex = CharCodeToGlyphIndex((char)idx);
                //    glyphIndex = CharCodeToGlyphIndex(ch);
                //}

                if (symbol)
                {
                    // Remap ch for symbol fonts.
                    ch = (char)(ch | (FontFace.os2.usFirstCharIndex & 0xFF00));  // @@@ refactor
                }
                var glyphIndex = CharCodeToGlyphIndex(ch);
                Widths[idx] = GlyphIndexToPdfWidth(glyphIndex);
            }
        }
        public int[] Widths = null!;

        /// <summary>
        /// Gets a value indicating whether this instance belongs to a bold font.
        /// </summary>
        public override bool IsBoldFace => FontFace.os2.IsBold;

        /// <summary>
        /// Gets a value indicating whether this instance belongs to an italic font.
        /// </summary>
        public override bool IsItalicFace => FontFace.os2.IsItalic;

        internal int DesignUnitsToPdf(double value)
        {
            return (int)Math.Round(value * 1000.0 / FontFace!.head!.unitsPerEm);
        }

        /// <summary>
        /// Maps a Unicode to the index of the corresponding glyph.
        /// See OpenType spec "cmap - Character To Glyph Index Mapping Table / Format 4: Segment mapping to delta values"
        /// for details about this a little bit strange looking algorithm.
        /// </summary>
        public uint CharCodeToGlyphIndex(char value)
        {
            //try
            //{

            CMap4 cmap = FontFace.cmap.cmap4;
            int segCount = cmap.segCountX2 / 2;
            int seg;
            for (seg = 0; seg < segCount; seg++)
            {
                if (value <= cmap.endCount[seg])
                    break;
            }
            Debug.Assert(seg < segCount);

            if (value < cmap.startCount[seg])
                return 0;

            if (cmap.idRangeOffs[seg] == 0)
                return (value + (uint)cmap.idDelta[seg]) & 0xFFFF;

            int idx = cmap.idRangeOffs[seg] / 2 + (value - cmap.startCount[seg]) - (segCount - seg);
            Debug.Assert(idx >= 0 && idx < cmap.glyphCount);

            if (cmap.glyphIdArray[idx] == 0)
                return 0;

            return (cmap.glyphIdArray[idx] + (uint)cmap.idDelta[seg]) & 0xFFFF;

            //}
            //catch
            //{
            //    GetType();
            //    throw;
            //}
        }

        /// <summary>
        /// Maps a Unicode to the index of the corresponding glyph.
        /// See OpenType spec "cmap - Character To Glyph Index Mapping Table / Format 4: Segment mapping to delta values"
        /// for details about this a little bit strange looking algorithm.
        /// </summary>
        public uint CharCodeToGlyphIndex(char highSurrogate, char lowSurrogate)
        {
            try
            {
                var value = char.ConvertToUtf32(highSurrogate, lowSurrogate);

                var converted = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
                var cmap = FontFace.cmap.cmap12;

                int seg;
                for (seg = 0; seg < cmap.groups.Length; seg++)
                {
                    if (value <= cmap.groups[seg].endCharCode)
                        break;
                }
                Debug.Assert(seg < cmap.groups.Length);

                if (value < cmap.groups[seg].startCharCode)
                    return 0;

                return cmap.groups[seg].startGlyphID + converted - cmap.groups[seg].startCharCode;
            }
            catch
            {
                GetType();
                throw;
            }
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// </summary>
        public int GlyphIndexToPdfWidth(uint glyphIndex)
        {
            try
            {
                var numberOfHMetrics = FontFace.hhea.numberOfHMetrics;
                var unitsPerEm = FontFace!.head!.unitsPerEm;

                // glyphIndex >= numberOfHMetrics means the font is mono-spaced and all glyphs have the same width
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = numberOfHMetrics - (uint)1;

                int width = FontFace.hmtx.Metrics[glyphIndex].advanceWidth;

                // Sometimes the unitsPerEm is 1000, sometimes a power of 2.
                if (unitsPerEm == 1000)
                    return width;
                return width * 1000 / unitsPerEm; // normalize
            }
            catch (Exception)
            {
                GetType();
                throw;
            }
        }

        public int PdfWidthFromCharCode(char ch)
        {
            var idx = CharCodeToGlyphIndex(ch);
            int width = GlyphIndexToPdfWidth(idx);
            return width;
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// </summary>
        public double GlyphIndexToEmfWidth(uint glyphIndex, double emSize)
        {
            try
            {
                uint numberOfHMetrics = FontFace.hhea.numberOfHMetrics;
                int unitsPerEm = FontFace!.head!.unitsPerEm;

                // glyphIndex >= numberOfHMetrics means the font is mono-spaced and all glyphs have the same width
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = numberOfHMetrics - 1;

                int width = FontFace.hmtx.Metrics[glyphIndex].advanceWidth;

                return width * emSize / unitsPerEm; // normalize
            }
            catch (Exception)
            {
                GetType();
                throw;
            }
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// </summary>
        public int GlyphIndexToWidth(uint glyphIndex)
        {
            try
            {
                uint numberOfHMetrics = FontFace.hhea.numberOfHMetrics;

                // glyphIndex >= numberOfHMetrics means the font is mono-spaced and all glyphs have the same width
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = numberOfHMetrics - 1;

                int width = FontFace.hmtx.Metrics[glyphIndex].advanceWidth;
                return width;
            }
            catch (Exception)
            {
                GetType();
                throw;
            }
        }
    }
}
