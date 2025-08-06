// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
//using System.Windows.Media;
#endif
using PdfSharp.Pdf.Internal;
using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
using PdfSharp.Logging;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// The OpenType font descriptor.
    /// Currently, the only font type PDFsharp supports.
    /// </summary>
    sealed class OpenTypeDescriptor : FontDescriptor
    {
        ///// <summary>
        ///// New...
        ///// </summary>
        //public OpenTypeDescriptor(string fontDescriptorKey, string name, XFontStyleEx style, OpenTypeFontFace fontFace, XPdfFontOptions options)
        //    : base(fontDescriptorKey)
        //{
        //    FontFace = fontFace;
        //    FontName = name;
        //    Initialize();
        //}

        public OpenTypeDescriptor(string fontDescriptorKey, XFont font)
            : base(fontDescriptorKey)
        {
            try
            {
                FontFace = font.GlyphTypeface.FontFace;
                FontName3 = font.Name2;
                Initialize();
            }
            catch
            {
                _ = typeof(int);
                throw;
            }
        }

        public OpenTypeDescriptor(string fontDescriptorKey, XGlyphTypeface glyphTypeface)
            : base(fontDescriptorKey)
        {
            try
            {
                FontFace = glyphTypeface.FontFace;
                FontName3 = glyphTypeface.FaceName;
                Initialize();
            }
            catch
            {
                _ = typeof(int);
                throw;
            }
        }

        internal OpenTypeDescriptor(string fontDescriptorKey, string idName, byte[] fontData)
            : base(fontDescriptorKey)
        {
            try
            {
                FontFace = new OpenTypeFontFace(fontData, idName);
                // Try to get real name from name table
                if (idName.Contains("XPS-Font-") && FontFace.name != null! && FontFace.name.Name.Length != 0)
                {
                    string tag = "";
                    if (idName.IndexOf("+", StringComparison.Ordinal) == 6)
                        tag = idName.Substring(0, 6);
                    idName = tag + "+" + FontFace.name.Name;
                    if (FontFace.name.Style.Length != 0)
                        idName += "," + FontFace.name.Style;
                    //idName = idName.Replace(" ", "");
                }
                FontName3 = idName;
                Initialize();
            }
            catch (Exception)
            {
#if DEBUG
                _ = typeof(int);
#endif
                throw;
            }
        }

        internal OpenTypeFontFace FontFace;

        void Initialize()
        {
            // TODO_OLD: Respect embedding restrictions.
            //bool embeddingRestricted = fontData.os2.fsType == 0x0002;

            //fontName = image.n
            ItalicAngle = FontFace.post.italicAngle;

            XMin = FontFace.head!.xMin;
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
            // /StemV to 0 always. I think the value doesn’t matter.
            //float weight = (float)(image.os2.usWeightClass / 65.0f);
            //stemV = (int)(50 + weight * weight);  // MAGIC
            StemV = 0;

            UnitsPerEm = FontFace.head.unitsPerEm;

            // Calculate Ascent, Descent, Leading and LineSpacing like in WPF Source Code (see FontDriver.ReadBasicMetrics)

            // OS/2 is an optional table, but we can’t determine if it is existing in this font.
            bool os2SeemsToBeEmpty = FontFace.os2 is { sTypoAscender: 0, sTypoDescender: 0, sTypoLineGap: 0 };
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
                // Comment from WPF: get the lineGap field and make sure it’s >= 0 
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
                    // Comment from WPF:
                    // The following calculation for designLineSpacing is per [....]. The default line spacing 
                    // should be the sum of the Mac ascender, descender, and lineGap unless the resulting value would
                    // be less than the cell height (winAscent + winDescent) in which case we use the cell height.
                    // See also http://www.microsoft.com/typography/otspec/recom.htm.
                    // Note that in theory it’s valid for the baseline-to-baseline distance to be less than the cell
                    // height. However, Windows has never allowed this for TrueType fonts, and fonts built for Windows
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

            Encoding ansi = PdfEncoders.WinAnsiEncoding;
            Encoding unicode = Encoding.Unicode;
            byte[] bytes = new byte[256];

            bool isSymbolFont = IsSymbolFont;
            Widths = new int[256];
            for (int idx = 0; idx < 256; idx++)
            {
                bytes[idx] = (byte)idx;
                // PDFlib handles some font flaws here...
                // We wait for bug reports.

                char ch = (char)idx;
#if true
                if (isSymbolFont)
                {
                    ch = RemapSymbolChar(ch);
                }
                else
                {
                    string s = ansi.GetString(bytes, idx, 1);
                    if (s.Length != 0)
                        ch = s[0];
                }
#else
                string s = ansi.GetString(bytes, idx, 1);
                if (s.Length != 0)
                {
                    //if (s[0] != ch)
                    ch = s[0];
                }

                // Remap ch for symbol fonts.
                if (isSymbolFont)
                    ch = RemapSymbolChar(ch);
#endif
                var glyphIndex = BmpCodepointToGlyphIndex(ch);
                Widths[idx] = GlyphIndexToPdfWidth(glyphIndex);
            }
        }
        public int[] Widths = default!;

        /// <summary>
        /// Gets a value indicating whether this instance belongs to a bold font.
        /// </summary>
        public override bool IsBoldFace => FontFace.os2.IsBold;

        /// <summary>
        /// Gets a value indicating whether this instance belongs to an italic font.
        /// </summary>
        public override bool IsItalicFace => FontFace.os2.IsItalic;

        internal int DesignUnitsToPdf(double value)
            => (int)Math.Round(value * 1000.0 / FontFace.head!.unitsPerEm);

        /// <summary>
        /// Maps a Unicode code point from the BMP to the index of the corresponding glyph.
        /// Returns 0 if no glyph exists for the specified character.
        /// See OpenType spec "cmap - Character To Glyph Index Mapping Table /
        /// Format 4: Segment mapping to delta values"
        /// for details about this a little bit strange looking algorithm.
        /// </summary>
        public ushort BmpCodepointToGlyphIndex(char ch)
        {
            // ReSharper disable once IdentifierTypo
            var cmap = FontFace.cmap.cmap4;
            int segCount = cmap.segCountX2 / 2;
            int seg;

            for (seg = 0; seg < segCount; seg++)
            {
                if (ch <= cmap.endCount[seg])
                    break;
            }

            if (seg == segCount)
                return 0;

            if (ch < cmap.startCount[seg])
                return 0;

            ushort glyphIndex;

            if (cmap.idRangeOffs[seg] == 0)
            {
                glyphIndex = (ushort)((ch + (uint)cmap.idDelta[seg]) & 0xFFFF);
                // Cannot happen anymore with glyphIndex as of type ushort:  Debug.Assert((glyphIndex & 0xFFFF0000) == 0, "Glyph index larger than 65535.");
                return glyphIndex;
            }

            int idx = cmap.idRangeOffs[seg] / 2 + (ch - cmap.startCount[seg]) - (segCount - seg);
            Debug.Assert(idx >= 0 && idx < cmap.glyphCount);

            glyphIndex = cmap.glyphIdArray[idx];
            if (glyphIndex == 0)
                return 0;

            glyphIndex = (ushort)((glyphIndex + (uint)cmap.idDelta[seg]) & 0xFFFF);
            Debug.Assert((glyphIndex & 0xFFFF0000) == 0, "Glyph index larger than 65535.");
            return glyphIndex;
        }

        /// <summary>
        /// Maps a Unicode character from outside the BMP to the index of the corresponding glyph.
        /// Returns 0 if no glyph exists for the specified code point.
        /// See OpenType spec "cmap - Character To Glyph Index Mapping Table /
        /// Format 12: Segmented coverage"
        /// for details about this a little bit strange looking algorithm.
        /// </summary>
        public ushort SurrogatePairToGlyphIndex(char highSurrogate, char lowSurrogate)
        {
            // ReSharper disable once IdentifierTypo
            var cmap = FontFace.cmap.cmap12;
            // cmap can be null here if the font does not support surrogate pairs.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (cmap is null)
                return 0;

            var codePoint = Char.ConvertToUtf32(highSurrogate, lowSurrogate);
            return CodepointToGlyphIndex(codePoint);
        }

        /// <summary>
        /// Maps a Unicode code point to the index of the corresponding glyph.
        /// Returns 0 if no glyph exists for the specified character.
        /// Should only be called for code points that are not from BMP.
        /// See OpenType spec "cmap - Character To Glyph Index Mapping Table /
        /// Format 4: Segment mapping to delta values"
        /// for details about this a little bit strange looking algorithm.
        /// </summary>
        public ushort CodepointToGlyphIndex(int codePoint)
        {
            if (codePoint <= 0xFFFF)
            {
                PdfSharpLogHost.FontManagementLogger.LogWarning("For code points from the BMP call BmpCharacterToGlyphID directly.");
                return BmpCodepointToGlyphIndex((char)codePoint);
            }

            // ReSharper disable once IdentifierTypo
            var cmap = FontFace.cmap.cmap12;
            // cmap can be null here if the font does not support surrogate pairs.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (cmap is null)
                return 0;

            int seg;
            for (seg = 0; seg < cmap.groups.Length; seg++)
            {
                if (codePoint <= cmap.groups[seg].endCharCode)
                    break;
            }

            if (seg == cmap.groups.Length)
                return 0;

            if (codePoint < cmap.groups[seg].startCharCode)
                return 0;

            var glyphIndex = (ushort)(cmap.groups[seg].startGlyphIndex + codePoint - cmap.groups[seg].startCharCode);
            // Debug.Assert((glyphIndex & 0xFFFF0000) == 0, "Glyph index larger than 65535.");
            return glyphIndex;
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// Index 0 also returns a valid font specific width for the non-existing glyph.
        /// </summary>
        public int GlyphIndexToPdfWidth(ushort glyphIndex)
        {
            try
            {
                var numberOfHMetrics = FontFace.hhea.numberOfHMetrics;
                var unitsPerEm = FontFace.head!.unitsPerEm;

                // glyphIndex >= numberOfHMetrics means the font is monospaced and all glyphs have the same width.
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = (ushort)(numberOfHMetrics - 1);

                int width = FontFace.hmtx.Metrics[glyphIndex].advanceWidth;

                // Sometimes the unitsPerEm is 1000, sometimes a power of 2.
                if (unitsPerEm == 1000)
                    return width;
                return width * 1000 / unitsPerEm; // normalize
            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception)
            {
                PdfSharpLogHost.FontManagementLogger.LogError("Invalid glyph index hmtx: 0x{Glyph:X4}", glyphIndex);
                throw;
            }
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// </summary>
        public double GlyphIndexToEmWidth(uint glyphIndex, double emSize)
        {
            try
            {
                uint numberOfHMetrics = FontFace.hhea.numberOfHMetrics;
                int unitsPerEm = FontFace.head!.unitsPerEm;

                // glyphIndex >= numberOfHMetrics means the font is monospaced and all glyphs have the same width.
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = numberOfHMetrics - 1;

                int width = FontFace.hmtx.Metrics[glyphIndex].advanceWidth;
                return width * emSize / unitsPerEm;
            }
            catch (Exception ex)
            {
                PdfSharpLogHost.Logger.LogError(ex, "Error calculating em-size for glyph 0x{Glyph:X4}.", glyphIndex);
                throw;
            }
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// </summary>
        public int GlyphIndexToWidth(int glyphIndex)
        {
            //if (glyphIndex == 0)
            //    return 0;
            try
            {
                int numberOfHMetrics = FontFace.hhea.numberOfHMetrics;

                // glyphIndex >= numberOfHMetrics means the font is monospaced and all glyphs have the same width.
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = numberOfHMetrics - 1;

                int width = FontFace.hmtx.Metrics[glyphIndex].advanceWidth;
                return width;
            }
            catch (Exception ex)
            {
                PdfSharpLogHost.Logger.LogError(ex, "Error find advance width for glyph 0x{Glyph:X4}.", glyphIndex);
                throw;
            }
        }

        /// <summary>
        /// Converts the code units of a UTF-16 string into the glyph identifier of this font.
        /// If useAnsiCharactersOnly is true, only valid ANSI code units a taken into account.
        /// All non-ANSI characters are skipped and not part of the result
        /// </summary>
        public bool IsSymbolFont => FontFace.cmap.symbol;

        public CodePointGlyphIndexPair[] GlyphIndicesFromString(string s, bool useAnsiCharactersOnly = false)
        {
            if (String.IsNullOrEmpty(s))
                return [];

            var codePoints = UnicodeHelper.Utf32FromString(s);
            return GlyphIndicesFromCodePoints(codePoints);
        }

        public CodePointGlyphIndexPair[] GlyphIndicesFromCodePoints(int[] codePoints, bool useAnsiCharactersOnly = false)
        {
            if (codePoints == null!)
                return [];

            int count = codePoints.Length;
            if (count == 0)
                return [];

            var result = new CodePointGlyphIndexPair[count];
            int iRes = 0;

            // Is the font a symbol font?
            if (IsSymbolFont)
            {
                for (int idx = 0; idx < count; idx++)
                {
                    ref var item = ref result[iRes++];
                    int codePoint = codePoints[idx];

                    char ch = (char)codePoint;
                    // ch must be fit in one byte.
                    if ((ch & 0xFF00) != 0)
                    {
                        // Just log a hint but do not skip the character.
                        PdfSharpLogHost.FontManagementLogger.LogDebug("Unexpected character found for symbol font: 0x{Char:X2}", ch);
                    }

                    // Remap ch for symbol fonts.
                    item.CodePoint = codePoint;
                    item.GlyphIndex = BmpCodepointToGlyphIndex(ch);
                }
            }
            else
            {
                // It is not a symbol font, i.e. it is a regular open type font.
                for (int idx = 0; idx < count; idx++)
                {
                    ref var item = ref result[iRes++];
                    item.CodePoint = codePoints[idx];
                    item.GlyphIndex = item.CodePoint < UnicodeHelper.UnicodePlane01Start
                        ? BmpCodepointToGlyphIndex((char)item.CodePoint)
                        : CodepointToGlyphIndex(item.CodePoint);
                }
            }
            return result;
        }

        public ColorTable.GlyphRecord?[] GlyphColorRecordsFromGlyphIndices(CodePointGlyphIndexPair[] pairs)
        {
            if (pairs == null!)
                return [];

            int count = pairs.Length;
            if (count == 0)
                return [];

            var glyphRecords = new ColorTable.GlyphRecord?[count];

            for (int idx = 0; idx < count; idx++)
            {
                glyphRecords[idx] = GetColorRecord(pairs[idx].GlyphIndex);
            }

            return glyphRecords;
        }

        /// <summary>
        /// Remaps a character of a symbol font.
        /// Required to get the correct glyph identifier
        /// from the cmap type 4 table.
        /// </summary>
        public char RemapSymbolChar(char ch)
        {
            Debug.Assert(IsSymbolFont);

            // Check if ch is either a byte or in range [0xf000..0xf0ff], wich is both valid for a
            // code unit of a symbol font.
            // Second expression is clever code from WPF source meaning:
            // 'ch >= 0xf000 && ch <= 0xf0ff' done with one test and branch.
            if (ch > 255 && !((uint)(ch - 0xf000) <= 0xff))
            {
                var value = ((int)ch).ToString("x4");
                PdfSharpLogHost.FontManagementLogger.LogError("Character 0u{char} of a symbol font is not in valid range.", value);
            }

            // Used | instead of + because of: http://pdfsharp.codeplex.com/workitem/15954
            return (char)(ch | (FontFace.os2.usFirstCharIndex & 0xFF00));
        }

        /// <summary>
        /// Gets the color-record of the glyph with the specified index.
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <returns>The color-record for the specified glyph or null, if the specified glyph has no color record.</returns>
        public ColorTable.GlyphRecord? GetColorRecord(int glyphIndex)
        {
            // Both tables COLR and CPAL are required according to the spec.
            // ref: https://learn.microsoft.com/en-us/typography/opentype/spec/colr
            if (FontFace is { cpal: not null, colr: not null })
            {
                return FontFace.colr.GetLayers(glyphIndex);
            }
            return null;
        }
    }
}
