// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

namespace PdfSharp.Internal.OpenType
{
    // ReSharper disable once InconsistentNaming
    public enum OpenTypeFontStyle
    {
        // Values must be in sync with WPF FontStyle and PDFsharp Graphics FontStyle.
        Normal = 0,
        Oblique = 1,
        Italic = 2
    }

    // ReSharper disable once InconsistentNaming
    public enum OpenTypeFontWeight
    {
        Thin = 100,
        UltraLight = 150,  // StL: added 24-03-04
        ExtraLight = 200,
        Light = 300,
        SemiLight = 350,  // StL: added 24-03-04
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900,
        ExtraBlack = 950,
    }

    // ReSharper disable once InconsistentNaming
    public enum OpenTypeFontStretch
    {
        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Normal = 5,
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9
    }

    /// <summary>
    /// The OpenType font descriptor.
    /// This class consolidates all relevant properties of a font face needed for glyph typeface, typeface,
    /// and font family.
    /// The calculation of some properties is identical to the calculation of these values in WPF.
    /// </summary>
    public sealed class OpenTypeFontDescriptor
    {
        // Invoked from cache.
        public OpenTypeFontDescriptor(string fontDescriptorKey, OpenTypeFontFace fontFace,
            string familyName, string faceName)
        {
            Key = fontDescriptorKey;
            OTFamilyName = FamilyName = familyName;
            OTSubfamilyName = FaceName = faceName;
            FontFace = fontFace;

            Initialize();
        }

        // Invoked from cache.
        public OpenTypeFontDescriptor(string fontDescriptorKey, OpenTypeFontFace fontFace,
            string familyName, string faceName, int dummy)
        {
            Key = fontDescriptorKey;
            OTFamilyName = FamilyName = familyName;
            OTSubfamilyName = FaceName = faceName;
            FontFace = fontFace;
            Initialize();
        }

        // Invoked from OpenTypeFontFace.
        public OpenTypeFontDescriptor(OpenTypeFontFace fontFace)
        {
            Key = "(yet undefined)";
            OTFamilyName = FamilyName = fontFace.name.OTFamilyName;
            OTSubfamilyName = FaceName = fontFace.name.OTSubfamilyName;
            FontFace = fontFace;
            Initialize();
            Key = KeyHelper.CalcFontFaceKey(FamilyName, FaceName, OTFontStyle, OTFontWeight, OTFontStretch);

        }

        public OpenTypeFontDescriptor Clone()
        {
            var clone = (OpenTypeFontDescriptor)MemberwiseClone();
            return clone;
        }

        void Initialize()
        {
            GlyphCount = FontFace.maxp.numGlyphs;
            IsSymbolFont = FontFace.cmap.symbol;
            ItalicAngle = FontFace.post.italicAngle;

            Debug.Assert(FontFace.head != null);
            XMin = FontFace.head!.xMin;
            YMin = FontFace.head.yMin;
            XMax = FontFace.head.xMax;
            YMax = FontFace.head.yMax;

            UnderlinePosition = FontFace.post.underlinePosition;
            UnderlineThickness = FontFace.post.underlineThickness;

            StrikeoutPosition = FontFace.os2.yStrikeoutPosition;
            StrikeoutSize = FontFace.os2.yStrikeoutSize;

            // No documentation found how to get the set vertical stems width from the TrueType tables.
            // Acrobat sets StemV to 0.
            StemV = 0;

            UnitsPerEm = FontFace.head.unitsPerEm;

            // Calculate Ascent, Descent, Leading and LineSpacing like in WPF Source Code (see FontDriver.ReadBasicMetrics)

            // OS/2 is an optional table, but we can’t determine if it is existing in this font.
            bool os2SeemsToBeEmpty = FontFace.os2 is { sTypoAscender: 0, sTypoDescender: 0, sTypoLineGap: 0 };
            //Debug.Assert(!os2SeemsToBeEmpty); // Are there fonts without OS/2 table?

            // Check bit 8 of fsSelection (WWS).
            bool dontUseWinLineMetrics = (FontFace.os2.fsSelection & 128) != 0;
            if (!os2SeemsToBeEmpty && dontUseWinLineMetrics)
            {
                // Comment from WPF: The font specifies that the sTypoAscender, sTypoDescender, and sTypoLineGap fields are valid and
                // should be used instead of winAscent and winDescent.
                int typoAscender = FontFace.os2.sTypoAscender;
                int typoDescender = FontFace.os2.sTypoDescender;
                int typoLineGap = FontFace.os2.sTypoLineGap;

                // Comment from WPF: We include the line gap in the ascent so that white-space is distributed above the line. (Note that
                // the typo line gap is a different concept than "external leading".)
                Ascender = typoAscender + typoLineGap;
                // Comment from WPF: Typo descent is a signed value where the positive direction is up. It is therefore typically negative.
                // A signed typo descent would be quite unusual as it would indicate the descender was above the baseline
                Descender = -typoDescender;
                LineSpacing = typoAscender + typoLineGap - typoDescender;
                LineGap = typoLineGap;
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
                    //int winDescent = Math.Abs(FontFace.os2.usWinDescent);
                    int winDescent = FontFace.os2.usWinDescent;  // usWinDescent is already unsigned.

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
                    LineGap = lineGap;
                }
                else
                {
                    Ascender = ascender;
                    Descender = descender;
                    LineSpacing = ascender + descender + lineGap;
                    LineGap = lineGap;
                }
            }

            Debug.Assert(Descender >= 0);

            int cellHeight = Ascender + Descender;
            int internalLeading = cellHeight - UnitsPerEm; // Not used, only for debugging.
            int externalLeading = LineSpacing - cellHeight;
            Leading = externalLeading;
#if DEBUG
            CellHeight = cellHeight;
            InternalLeading = internalLeading;
            ExternalLeading = externalLeading;
#endif

            // sCapHeight and sxHeight are only valid if Version >= 2.
            if (FontFace.os2.version >= 2 && FontFace.os2.sCapHeight != 0)
                CapHeight = FontFace.os2.sCapHeight;
            else
                CapHeight = Ascender;

            if (FontFace.os2.version >= 2 && FontFace.os2.sxHeight != 0)
                XHeight = FontFace.os2.sxHeight;
            else
                XHeight = (int)(0.66 * Ascender);

            // Get values used for FontWeight, FontStretch, and FontStyle.
            OTFontWeight = (OpenTypeFontWeight)FontFace.os2.usWeightClass;
            OTFontStretch = (OpenTypeFontStretch)FontFace.os2.usWidthClass;
            var fs = FontFace.os2.fsSelection;
            OTFontStyle = (fs & OS2Table.FontSelectionFlags.Italic) != 0 ? OpenTypeFontStyle.Italic :
                       (fs & OS2Table.FontSelectionFlags.Oblique) != 0 ? OpenTypeFontStyle.Oblique :
                       OpenTypeFontStyle.Normal;

            IsBoldFace = FontFace.os2.IsBold;
            IsItalicFace = FontFace.os2.IsItalic;

            Height = Ascender + Descender;

            // WPF documentation of BaseLine:
            // The distance between the baseline and the character cell top.
            //
            // From the WPF 3.0 source code:
            // The ascent is from the top of the cell. Per [....], we want baseline to be relative to the 
            // top of a logical line (represented by lineSpacing) in which the cell is vertically centered.
            // Thus, we want half the external leading to be above the cell. The external leading is equal
            // to (lineSpacing - (ascent + descent)), giving us the following formula:
            // 
            // baseline = ascent + (lineSpacing - (ascent + descent)) * 0.5
            //          = ascent + lineSpacing * 0.5 - ascent * 0.5 - descent * 0.5 
            //          = ascent * 0.5 + lineSpacing * 0.5 - descent * 0.5 
            //          = (ascent + lineSpacing - descent) * 0.5
            // 
            Baseline = ((Ascender + LineSpacing - Descender) * .5f);

            Encoding ansi = AnsiEncoding.Encoder;
            Encoding unicode = Encoding.Unicode;
            byte[] bytes = new byte[256];

            bool isSymbolFont = IsSymbolFont;
            Widths = new int[256];
            for (int idx = 0; idx < 256; idx++)
            {
                bytes[idx] = (byte)idx;

                char ch = (char)idx;
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
                var glyphIndex = BmpCodepointToGlyphIndex(ch);
                Widths[idx] = GlyphIndexToPdfWidth(glyphIndex);
            }
        }

        // ========== Properties ==========

        public string Key { get; private set; }

        /// <summary>
        /// Name ID 1 (Font Family name) from the names table in en-US.
        /// </summary>
        public string OTFamilyName { get; private set; }

        /// <summary>
        /// Name ID 2 (Font Subfamily name) from the names table in en-US.
        /// </summary>
        public string OTSubfamilyName { get; private set; }

        /// <summary>
        /// Initially same as OTFamilyName.
        /// The value can be overridden during font registration.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Initially same as OTSubfamilyName.
        /// The value can be overridden during font registration.
        /// </summary>
        public string FaceName { get; set; }

        /// <summary>
        /// The number of glyphs in the font.
        /// </summary>
        public int GlyphCount { get; private set; }

        /// <summary>
        /// Value depending on bit 0, 6, and 9 of fsSelection (font selection flag) from OS/2 table.
        /// Value is 0, 1, or 2: Normal, Oblique, or Italic.
        /// The value can be overridden during font registration.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public OpenTypeFontStyle OTFontStyle { get; set; }

        /// <summary>
        /// Value of usWeightClass from OS/2 table.
        /// The value can be overridden during font registration.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public OpenTypeFontWeight OTFontWeight { get; set; }

        /// <summary>
        /// Value of usWidthClass from OS/2 table.
        /// The value can be overridden during font registration.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public OpenTypeFontStretch OTFontStretch { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance belongs to a bold font.
        /// </summary>
        public bool IsBoldFace { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance belongs to an italic font.
        /// </summary>
        public bool IsItalicFace { get; private set; }

        /// <summary>
        /// head unitsPerEm.
        /// </summary>
        public int UnitsPerEm { get; private set; }

        /// <summary>
        /// Same as Ascender + Descender.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// (Ascender + LineSpacing - Descender) * .5
        /// </summary>
        public float Baseline { get; private set; }

        /// <summary>
        /// OS/2 sCapHeight.
        /// </summary>
        public int CapHeight { get; private set; }

        /// <summary>
        /// OS/2 sxHeight.
        /// </summary>
        public int XHeight { get; private set; }

        /// <summary>
        /// post underlinePosition.
        /// </summary>
        public int UnderlinePosition { get; private set; }

        /// <summary>
        /// post underlineThickness.
        /// </summary>
        public int UnderlineThickness { get; private set; }

        /// <summary>
        /// OS/2 yStrikeoutPosition.
        /// </summary>
        public int StrikeoutPosition { get; private set; }

        /// <summary>
        /// OS/2 yStrikeoutSize.
        /// </summary>
        public int StrikeoutSize { get; private set; }

        /// <summary>
        /// OS/2 usWinDescent.
        /// </summary>
        public int Ascender { get; private set; }

        /// <summary>
        /// OS/2 usWinDescent.
        /// </summary>
        public int Descender { get; private set; }

        /// <summary>
        /// LineSpacing - (Ascender + Descender);.
        /// </summary>
        public int Leading { get; private set; }

        /// <summary>
        /// hhea lineGap.
        /// </summary>
        public int LineGap { get; private set; }

        /// <summary>
        /// max(lineGap + ascender + descender, winAscent + winDescent).
        /// </summary>
        public int LineSpacing { get; private set; }

        /// <summary>
        /// (Not used).
        /// </summary>
        public float ItalicAngle { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int XMin { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int YMin { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int XMax { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int YMax { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int StemV { get; private set; }

        internal readonly int GlobalVersion = OtGlobals.Global.Version;

        // ========== Utilities ==========

        public int DesignUnitsToPdf(double value)
            => (int)Math.Round(value * 1000.0 / UnitsPerEm);

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
            // Cannot happen anymore with glyphIndex as of type ushort:  Debug.Assert((glyphIndex & 0xFFFF0000) == 0, "Glyph index larger than 65535.");
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
                LogHost.Logger./*FontManagementLogger.*/LogWarning("For code points from the BMP call BmpCharacterToGlyphID directly.");
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

                // glyphIndex >= numberOfHMetrics means the font is monospaced and all glyphs have the same width.
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = (ushort)(numberOfHMetrics - 1);

                int width = FontFace.hmtx.HorzMetrics[glyphIndex].advanceWidth;

                // Sometimes the unitsPerEm is 1000, sometimes a power of 2.
                if (UnitsPerEm == 1000)
                    return width;
                return width * 1000 / UnitsPerEm; // normalize
            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception)
            {
                LogHost.Logger./*FontManagementLogger.*/LogError("Invalid glyph index hmtx: 0x{Glyph:X4}", glyphIndex);
                throw;
            }
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// </summary>
        public double GlyphIndexToEmWidth(ushort glyphIndex, double emSize)
        {
            try
            {
                uint numberOfHMetrics = FontFace.hhea.numberOfHMetrics;

                // glyphIndex >= numberOfHMetrics means the font is monospaced and all glyphs have the same width.
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = (ushort)(numberOfHMetrics - 1);

                int width = FontFace.hmtx.HorzMetrics[glyphIndex].advanceWidth;
                return width * emSize / UnitsPerEm;
            }
            catch (Exception ex)
            {
                LogHost.Logger.LogError(ex, "Error calculating em-size for glyph 0x{Glyph:X4}.", glyphIndex);
                throw;
            }
        }

        /// <summary>
        /// Converts the width of a glyph identified by its index to PDF design units.
        /// </summary>
        public int GlyphIndexToWidth(int glyphIndex)
        {
            try
            {
                int numberOfHMetrics = FontFace.hhea.numberOfHMetrics;

                // glyphIndex >= numberOfHMetrics means the font is monospaced and all glyphs have the same width.
                if (glyphIndex >= numberOfHMetrics)
                    glyphIndex = numberOfHMetrics - 1;

                int width = FontFace.hmtx.HorzMetrics[glyphIndex].advanceWidth;
                return width;
            }
            catch (Exception ex)
            {
                LogHost.Logger.LogError(ex, "Error find advance width for glyph 0x{Glyph:X4}.", glyphIndex);
                throw;
            }
        }

        /// <summary>
        /// Converts the code units of a UTF-16 string into the glyph identifier of this font.
        /// If useAnsiCharactersOnly is true, only valid ANSI code units a taken into account.
        /// All non-ANSI characters are skipped and not part of the result.
        /// </summary>
        public bool IsSymbolFont { get; private set; }

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
                        LogHost.Logger./*FontManagementLogger.*/LogDebug("Unexpected character found for symbol font: 0x{Char:X2}", ch);
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

            // Check if ch is either a byte or in range [0xf000..0xf0ff], which is both valid for a
            // code unit of a symbol font.
            // Second expression is clever code from WPF source meaning:
            // 'ch >= 0xf000 && ch <= 0xf0ff' done with one test and branch.
            if (ch > 255 && !((uint)(ch - 0xf000) <= 0xff))
            {
                var value = ((int)ch).ToString("x4");
                LogHost.Logger./*FontManagementLogger.*/LogError("Character 0u{char} of a symbol font is not in valid range.", value);
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

        public readonly OpenTypeFontFace FontFace;

        public int[] Widths = null!;

#if DEBUG
        public int CellHeight;
        public int InternalLeading;
        public int ExternalLeading;
#endif
    }
}
