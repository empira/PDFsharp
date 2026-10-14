// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Text;
using Fixed = System.Int32;
using FWord = System.Int16;
using UFWord = System.UInt16;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace PdfSharp.Internal.OpenType
{
    enum PlatformId
    {
        Apple, Mac, Iso, Win
    }

    /// <summary>
    /// Only Symbol and Unicode are used by PDFsharp.
    /// </summary>
    public enum WinEncodingId
    {
        Symbol = 0,
        UnicodeUSC_2 = 1,
        //ShiftJIS = 2,
        //PRC = 3,
        //Big5 = 4,
        //Wansung = 5,
        //Johab = 6,
        UnicodeUSC_4 = 10
    }

    /// <summary>
    /// Name IDs of strings in name table.
    /// </summary>
    public static class NameId
    {
        public const ushort CopyrightNotice = 0;
        public const ushort FamilyName = 1;
        public const ushort SubfamilyName = 2;
        public const ushort UniqueIdentifier = 3;
        public const ushort FullName = 4;
        public const ushort Version = 5;
        public const ushort PostScriptName = 0;
        public const ushort Trademark = 7;
        public const ushort Manufacturer = 8;
        public const ushort Designer = 9;
        public const ushort Description = 10;
        public const ushort VendorUrl = 11;
        public const ushort DesignerUrl = 12;
        public const ushort License = 13;
        public const ushort LicenseInfoUrl = 14;
        public const ushort Reserved = 15;
        public const ushort TypographicFamily = 16;
        public const ushort TypographicSubfamily = 17;
        public const ushort CompatibleFull = 18;
        public const ushort SampleText = 19;
        public const ushort PostScriptCID = 20;
        public const ushort WwsFamily = 21;
        public const ushort WwsSubfamily = 22;
        public const ushort DarkBackgroundPalette = 24;
        public const ushort VariationsPostScriptNamePrefix = 25;

        public const int MaxValue = 25;
    }

    /// <summary>
    /// CMap format 4: Segment mapping to delta values.
    /// The Windows standard format.
    /// </summary>
    public class CMap4 : OpenTypeFontTable
    {
        public WinEncodingId encodingId; // Windows encoding ID.
        public ushort format; // Format number is set to 4.
        public ushort length; // This is the length in bytes of the sub-table. 
        public ushort language; // This field must be set to zero for all cmap sub-tables whose platform IDs are other than Macintosh (platform ID 1). 
        public ushort segCountX2; // 2 x segCount.
        public ushort searchRange; // 2 x (2**floor(log2(segCount)))
        public ushort entrySelector; // log2(searchRange/2)
        public ushort rangeShift;
        public ushort[] endCount = null!;      // [segCount] / End characterCode for each segment, last=0xFFFF.
        public ushort[] startCount = null!;    // [segCount] / Start character code for each segment.
        public short[] idDelta = null!;        // [segCount] / Delta for all character codes in segment.
        public ushort[] idRangeOffs = null!;   // [segCount] / Offsets into glyphIdArray or 0
        public int glyphCount;                 // = (length - (16 + 4 * 2 * segCount)) / 2;
        public ushort[] glyphIdArray = null!;  // Glyph index array (arbitrary length)

        public CMap4(OpenTypeFontFace fontData, WinEncodingId encodingId)
            : base(fontData, "----")
        {
            this.encodingId = encodingId;
            Read();
        }

        internal void Read()
        {
            try
            {
                // m_EncodingID = encID;
                format = FontData!.ReadUShort(); // NRT
                Debug.Assert(format == 4, "Only format 4 expected.");
                length = FontData.ReadUShort();
                language = FontData.ReadUShort();  // Always null in Windows
                segCountX2 = FontData.ReadUShort();
                searchRange = FontData.ReadUShort();
                entrySelector = FontData.ReadUShort();
                rangeShift = FontData.ReadUShort();

                int segCount = segCountX2 / 2;
                glyphCount = (length - (16 + 8 * segCount)) / 2;

                //ASSERT_CONDITION(0 <= m_NumGlyphIds && m_NumGlyphIds < m_Length, "Invalid Index");

                endCount = new ushort[segCount];
                startCount = new ushort[segCount];
                idDelta = new short[segCount];
                idRangeOffs = new ushort[segCount];

                glyphIdArray = new ushort[glyphCount];

                for (int idx = 0; idx < segCount; idx++)
                    endCount[idx] = FontData.ReadUShort();

                // Read reserved pad.
                FontData.ReadUShort();

                for (int idx = 0; idx < segCount; idx++)
                    startCount[idx] = FontData.ReadUShort();

                for (int idx = 0; idx < segCount; idx++)
                    idDelta[idx] = FontData.ReadShort();

                for (int idx = 0; idx < segCount; idx++)
                    idRangeOffs[idx] = FontData.ReadUShort();

                for (int idx = 0; idx < glyphCount; idx++)
                    glyphIdArray[idx] = FontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// CMap format 12: Segmented coverage.
    /// The Windows standard format.
    /// </summary>
    public class CMap12 : OpenTypeFontTable
    {
        public struct SequentialMapGroup
        {
            public UInt32 startCharCode;// First character code in this group.
            public UInt32 endCharCode; // Last character code in this group.
            public UInt32 startGlyphIndex; // Glyph index corresponding to the starting character code.
        }

        public WinEncodingId encodingId; // Windows encoding ID.
        public UInt16 format; // Subtable format; set to 12.
        public UInt32 length; // Byte length of this subtable (including the header).
        public UInt32 language; // This field must be set to zero for all cmap subtables whose platform IDs are other than Macintosh (platform ID 1).
        public UInt32 numGroups; // Number of groupings which follow.

        public SequentialMapGroup[] groups = null!;

        public CMap12(OpenTypeFontFace fontData, WinEncodingId encodingId)
            : base(fontData, "----")
        {
            this.encodingId = encodingId;
            Read();
        }

        internal void Read()
        {
            try
            {
                // m_EncodingID = encID;
                format = FontData!.ReadUShort(); // NRT
                Debug.Assert(format == 12, "Only format 12 expected.");
                FontData.ReadUShort(); // Reserved.
                length = FontData.ReadULong();
                language = FontData.ReadULong(); // Always null in Windows.
                numGroups = FontData.ReadULong();

                groups = new SequentialMapGroup[numGroups];

                for (int i = 0; i < groups.Length; i++)
                {
                    ref var group = ref groups[i];
                    group.startCharCode = FontData.ReadULong();
                    group.endCharCode = FontData.ReadULong();
                    group.startGlyphIndex = FontData.ReadULong();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table defines the mapping of character codes to the glyph index values used in the font.
    /// It may contain more than one subtable, in order to support more than one character encoding scheme.
    /// </summary>
    public class CMapTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.CMap;

        public ushort version;
        public ushort numTables;

        /// <summary>
        /// Is true for symbol font encoding.
        /// </summary>
        public bool symbol;

        public CMap4 cmap4 = null!;
        public CMap12 cmap12 = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="CMapTable"/> class.
        /// </summary>
        public CMapTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        internal void Read()
        {
            try
            {
                int tableOffset = FontData!.Position;

                version = FontData.ReadUShort();
                numTables = FontData.ReadUShort();

                bool success = false;
                for (int idx = 0; idx < numTables; idx++)
                {
                    PlatformId platformId = (PlatformId)FontData.ReadUShort();
                    WinEncodingId encodingId = (WinEncodingId)FontData.ReadUShort();
                    int offset = FontData.ReadLong();

                    int currentPosition = FontData.Position;

                    // Just read Windows stuff.
                    if (platformId == PlatformId.Win &&
                        encodingId is WinEncodingId.Symbol or WinEncodingId.UnicodeUSC_2 or WinEncodingId.UnicodeUSC_4)
                    {
                        symbol = encodingId == WinEncodingId.Symbol;

                        FontData.Position = tableOffset + offset;

                        var format = FontData.ReadUShort();
                        FontData.Position = tableOffset + offset;

                        if (format == 4)
                        {
                            cmap4 = new(FontData, encodingId);
                        }
                        else if (format == 12)
                        {
                            cmap12 = new(FontData, encodingId);
                        }

                        FontData.Position = currentPosition;

                        // We have found what we are looking for, but we do not break as there may be another hit.
                        success = true;
                        // break;
                    }
                }
                if (!success)
                    throw new InvalidOperationException("Font has no usable platform or encoding ID. It cannot be used with PDFsharp.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table adds support for multi-colored glyphs in a manner that integrates with the rasterizers
    /// of existing text engines and that is designed to be easy to support with current OpenType font files.
    /// </summary>
    public class ColorTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.COLR;

        public struct GlyphRecord
        {
            public ushort glyphId;
            public ushort firstLayerIndex;
            public ushort numLayers;
        }

        public struct LayerRecord
        {
            public ushort glyphId;
            public ushort paletteIndex;
        }

        public ushort version;
        // version 0 tables start
        public ushort numBaseGlyphRecords;  // Number of BaseGlyph records.
        public uint baseGlyphRecordsOffset;  // Offset to baseGlyphRecords array, from beginning of COLR table.
        public uint layerRecordsOffset;  // Offset to layerRecords array, from beginning of COLR table.
        public ushort numLayerRecords;  // Number of Layer records.
        // version 0 tables end

        public GlyphRecord[] baseGlyphRecords = [];
        public LayerRecord[] layerRecords = [];

        // Helper array that contains just the glyphIds for the baseGlyphRecords.
        private int[] glyphRecordsHelperArray = [];

        public ColorTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read(fontData);
        }

        public GlyphRecord? GetLayers(int glyphId)
        {
            var index = Array.BinarySearch(glyphRecordsHelperArray, glyphId);
            if (index >= 0)
                return baseGlyphRecords[index];
            return null;
        }

        void Read(OpenTypeFontFace fontData)
        {
            try
            {
                var tableStart = fontData.Position;

                version = fontData.ReadUShort();
                Debug.Assert(version == 0 || version == 1, "Version 0 or 1 of COLR table is expected");
                numBaseGlyphRecords = fontData.ReadUShort();
                baseGlyphRecordsOffset = fontData.ReadULong();
                layerRecordsOffset = fontData.ReadULong();
                numLayerRecords = fontData.ReadUShort();

                baseGlyphRecords = new GlyphRecord[numBaseGlyphRecords];
                glyphRecordsHelperArray = new int[numBaseGlyphRecords];
                layerRecords = new LayerRecord[numLayerRecords];

                fontData.Position = tableStart + (int)baseGlyphRecordsOffset;
                for (var i = 0; i < numBaseGlyphRecords; i++)
                {
                    var glyphId = fontData.ReadUShort();
                    baseGlyphRecords[i] = new()
                    {
                        glyphId = glyphId,
                        firstLayerIndex = fontData.ReadUShort(),
                        numLayers = fontData.ReadUShort()
                    };
                    glyphRecordsHelperArray[i] = glyphId;
                }
                fontData.Position = tableStart + (int)layerRecordsOffset;
                for (var i = 0; i < numLayerRecords; i++)
                {
                    layerRecords[i] = new()
                    {
                        glyphId = fontData.ReadUShort(),
                        paletteIndex = fontData.ReadUShort()
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table is a set of one or more palettes, each containing a predefined number of color records.
    /// It may also contain 'name' table IDs describing the palettes and their entries.
    /// </summary>
    public class ColorPalletTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.CPAL;

        public ushort version;
        public ushort numPaletteEntries;  // Number of palette entries in each palette.
        public ushort numPalettes;  // Number of palettes in the table.
        public ushort numColorRecords;  // Total number of color records, combined for all palettes.
        public uint colorRecordsArrayOffset;  // Offset from the beginning of CPAL table to the first ColorRecord.
        public ushort[] colorRecordIndices = [];  // Index of each palette’s first color record in the combined color record array.
        public OTColor[] colorRecords = [];

        public ColorPalletTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read(fontData);
        }

        void Read(OpenTypeFontFace fontData)
        {
            try
            {
                var tableStart = fontData.Position;

                version = fontData.ReadUShort();
                numPaletteEntries = fontData.ReadUShort();
                numPalettes = fontData.ReadUShort();
                numColorRecords = fontData.ReadUShort();
                colorRecordsArrayOffset = fontData.ReadULong();

                colorRecordIndices = new ushort[numPalettes];
                for (int idx = 0; idx < numPalettes; idx++)
                {
                    colorRecordIndices[idx] = fontData.ReadUShort();
                }
                colorRecords = new OTColor[numColorRecords];
                for (int idx = 0; idx < numColorRecords; idx++)
                {
                    var blue = fontData.ReadByte();
                    var green = fontData.ReadByte();
                    var red = fontData.ReadByte();
                    var alpha = fontData.ReadByte();
                    colorRecords[idx] = OTColor.FromArgb(alpha, red, green, blue);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table gives global information about the font. The bounding box values should be computed using 
    /// only glyphs that have contours. Glyphs with no contours should be ignored for the purposes of these calculations.
    /// </summary>
    public class FontHeaderTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Head;

        public Fixed version; // 0x00010000 for Version 1.0.
        public Fixed fontRevision;
        public uint checkSumAdjustment;
        public uint magicNumber; // Set to 0x5F0F3CF5
        public ushort flags;
        public ushort unitsPerEm; // Valid range is from 16 to 16384. This value should be a power of 2 for fonts that have TrueType outlines.
        public long created;
        public long modified;
        public short xMin, yMin; // For all glyph bounding boxes.
        public short xMax, yMax; // For all glyph bounding boxes.
        public ushort macStyle;
        public ushort lowestRecPPEM;
        public short fontDirectionHint;
        public short indexToLocFormat; // 0 for short offsets, 1 for long
        public short glyphDataFormat; // 0 for current format

        public FontHeaderTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                version = FontData!.ReadFixed();
                fontRevision = FontData.ReadFixed();
                checkSumAdjustment = FontData.ReadULong();
                magicNumber = FontData.ReadULong();
                flags = FontData.ReadUShort();
                unitsPerEm = FontData.ReadUShort();
                created = FontData.ReadLongDate();
                modified = FontData.ReadLongDate();
                xMin = FontData.ReadShort();
                yMin = FontData.ReadShort();
                xMax = FontData.ReadShort();
                yMax = FontData.ReadShort();
                macStyle = FontData.ReadUShort();
                lowestRecPPEM = FontData.ReadUShort();
                fontDirectionHint = FontData.ReadShort();
                indexToLocFormat = FontData.ReadShort();
                glyphDataFormat = FontData.ReadShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table contains information for horizontal layout. The values in the minRightSideBearing, 
    /// MinLeftSideBearing and xMaxExtent should be computed using only glyphs that have contours.
    /// Glyphs with no contours should be ignored for the purposes of these calculations.
    /// All reserved areas must be set to 0. 
    /// </summary>
    public class HorizontalHeaderTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.HHea;

        public Fixed version; // 0x00010000 for Version 1.0.
        public FWord ascender; // Typographic ascent. (Distance from baseline of highest Ascender) 
        public FWord descender; // Typographic descent. (Distance from baseline of lowest Descender) 
        public FWord lineGap; // Typographic line gap. Negative LineGap values are treated as zero in Windows 3.1, System 6, and System 7.
        public UFWord advanceWidthMax;
        public FWord minLeftSideBearing;
        public FWord minRightSideBearing;
        public FWord xMaxExtent;
        public short caretSlopeRise;
        public short caretSlopeRun;
        public short reserved1;
        public short reserved2;
        public short reserved3;
        public short reserved4;
        public short reserved5;
        public short metricDataFormat;
        public ushort numberOfHMetrics;

        public HorizontalHeaderTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                version = FontData!.ReadFixed();
                ascender = FontData.ReadFWord();
                descender = FontData.ReadFWord();
                lineGap = FontData.ReadFWord();
                advanceWidthMax = FontData.ReadUFWord();
                minLeftSideBearing = FontData.ReadFWord();
                minRightSideBearing = FontData.ReadFWord();
                xMaxExtent = FontData.ReadFWord();
                caretSlopeRise = FontData.ReadShort();
                caretSlopeRun = FontData.ReadShort();
                reserved1 = FontData.ReadShort();
                reserved2 = FontData.ReadShort();
                reserved3 = FontData.ReadShort();
                reserved4 = FontData.ReadShort();
                reserved5 = FontData.ReadShort();
                metricDataFormat = FontData.ReadShort();
                numberOfHMetrics = FontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    public class HorizontalMetrics : OpenTypeFontTable
    {
        public const string Tag = "----";

        public ushort advanceWidth;
        public short leftSideBearing;

        public HorizontalMetrics(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                advanceWidth = FontData!.ReadUFWord();
                leftSideBearing = FontData.ReadFWord();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// The type longHorMetric is defined as an array where each element has two parts:
    /// the advance width, which is of type USHORT, and the left side bearing, which is of type SHORT.
    /// These fields are in font design units.
    /// </summary>
    public class HorizontalMetricsTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.HMtx;

        public HorizontalMetrics[] HorzMetrics = null!;
        public FWord[] LeftSideBearing = null!;

        public HorizontalMetricsTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public (int lsb, int advw) GetLsbAndAdvanceWidth(ushort glyphIndex)
        {
            int lsb = 0;
            int advw = 0;
            if (glyphIndex < _numMetrics)
            {
                var hm = HorzMetrics[glyphIndex];
                lsb = hm.leftSideBearing;
                advw = hm.advanceWidth;
            }
            else
            {
                lsb = LeftSideBearing[glyphIndex - _numMetrics];
                advw = HorzMetrics[_numMetrics - 1].advanceWidth;
            }
            return (lsb, advw);
        }

        public void Read()
        {
            try
            {
                var hhea = FontData!.hhea;
                var maxp = FontData.maxp;
                if (hhea != null! && maxp != null!)
                {
                    _numMetrics = hhea.numberOfHMetrics;
                    _numLsbs = maxp.numGlyphs - _numMetrics;

                    Debug.Assert(_numMetrics != 0);
                    Debug.Assert(_numLsbs >= 0);

                    HorzMetrics = new HorizontalMetrics[_numMetrics];
                    for (int idx = 0; idx < _numMetrics; idx++)
                        HorzMetrics[idx] = new HorizontalMetrics(FontData);

                    if (_numLsbs > 0)
                    {
                        LeftSideBearing = new FWord[_numLsbs];
                        for (int idx = 0; idx < _numLsbs; idx++)
                            LeftSideBearing[idx] = FontData.ReadFWord();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }

        int _numMetrics;
        int _numLsbs;
    }

    /// <summary>
    /// The vertical header table contains information needed for vertical layout of Chinese, Japanese,
    /// Korean (CJK) and other ideographic scripts. In vertical layout, these scripts are written either
    /// top to bottom or bottom to top. This table contains information that is general to the font as
    /// a whole. Information that pertains to specific glyphs is given in the vertical metrics ('vmtx')
    /// table. The formats of these tables are similar to those for horizontal metrics, 'hhea' and 'hmtx'.
    /// https://learn.microsoft.com/en-us/typography/opentype/spec/vhea
    /// </summary>
    public class VerticalHeaderTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.VHea;

        public Fixed version; // 0x00010000 for Version 1.0.
        public FWord vertTypoAscender; // Typographic ascent. (Distance from baseline of highest Ascender) 
        public FWord vertTypoDescender; // Typographic descent. (Distance from baseline of lowest Descender) 
        public FWord vertTypoLineGap; // Typographic line gap. Negative LineGap values are treated as zero in Windows 3.1, System 6, and System 7.
        public UFWord advanceHeightMax;
        public FWord minTopSideBearing;
        public FWord minBottomSideBearing;
        public FWord yMaxExtent;
        public short caretSlopeRise;
        public short caretSlopeRun;
        public short caretOffset;
        public short reserved1;
        public short reserved2;
        public short reserved3;
        public short reserved4;
        public short metricDataFormat;
        public ushort numOfLongVerMetrics;

        public VerticalHeaderTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                if (FontData == null)
                    return;

                version = FontData.ReadFixed();
                vertTypoAscender = FontData.ReadFWord();
                vertTypoDescender = FontData.ReadFWord();
                vertTypoLineGap = FontData.ReadFWord();
                advanceHeightMax = FontData.ReadUFWord();
                minTopSideBearing = FontData.ReadFWord();
                minBottomSideBearing = FontData.ReadFWord();
                yMaxExtent = FontData.ReadFWord();
                caretSlopeRise = FontData.ReadShort();
                caretSlopeRun = FontData.ReadShort();
                caretOffset = FontData.ReadShort();
                reserved1 = FontData.ReadShort();
                reserved2 = FontData.ReadShort();
                reserved3 = FontData.ReadShort();
                reserved4 = FontData.ReadShort();
                metricDataFormat = FontData.ReadShort();
                numOfLongVerMetrics = FontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    public class VerticalMetrics : OpenTypeFontTable
    {
        public const string Tag = "----";

        public ushort advanceHeight;
        public short topSideBearing;

        public VerticalMetrics(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                advanceHeight = FontData!.ReadUFWord();
                topSideBearing = FontData.ReadFWord();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// The vertical metrics table allows you to specify the vertical spacing for each glyph in a
    /// vertical font. This table consists of either one or two arrays that contain metric information
    /// (the advance heights and top sidebearings) for the vertical layout of each of the glyphs in
    /// the font.
    /// https://learn.microsoft.com/en-us/typography/opentype/spec/vmtx
    /// </summary>
    public class VerticalMetricsTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.VMtx;

        public VerticalMetrics[] VertMetrics = null!;
        public FWord[] TopSideBearing = null!;

        public VerticalMetricsTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public (int tsb, int advh) GetTsbAndAdvanceHeight(ushort glyphIndex)
        {
            int tsb = 0;
            int advh = 0;
            if (glyphIndex < _numMetrics)
            {
                var vm = VertMetrics[glyphIndex];
                tsb = vm.topSideBearing;
                advh = vm.advanceHeight;
            }
            else
            {
                tsb = TopSideBearing[glyphIndex - _numMetrics];
                advh = VertMetrics[_numMetrics - 1].advanceHeight;
            }
            return (tsb, advh);
        }

        public void Read()
        {
            try
            {
                var vhea = FontData!.vhea;
                var maxp = FontData.maxp;
                if (vhea != null! && maxp != null!)
                {
                    _numMetrics = vhea.numOfLongVerMetrics;
                    _numTsbs = maxp.numGlyphs - _numMetrics;

                    Debug.Assert(_numMetrics != 0);
                    Debug.Assert(_numTsbs >= 0);

                    VertMetrics = new VerticalMetrics[_numMetrics];
                    for (int idx = 0; idx < _numMetrics; idx++)
                        VertMetrics[idx] = new(FontData);

                    if (_numTsbs > 0)
                    {
                        TopSideBearing = new FWord[_numTsbs];
                        for (int idx = 0; idx < _numTsbs; idx++)
                            TopSideBearing[idx] = FontData.ReadFWord();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }

        int _numMetrics;
        int _numTsbs;

    }

    /// <summary>
    /// This table establishes the memory requirements for this font.
    /// Fonts with CFF data must use Version 0.5 of this table, specifying only the numGlyphs field.
    /// Fonts with TrueType outlines must use Version 1.0 of this table, where all data is required.
    /// Both formats of OpenType require a 'maxp' table because a number of applications call the 
    /// Windows GetFontData() API on the 'maxp' table to determine the number of glyphs in the font.
    /// </summary>
    public class MaximumProfileTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.MaxP;

        public Fixed version;
        public ushort numGlyphs;
        public ushort maxPoints;
        public ushort maxContours;
        public ushort maxCompositePoints;
        public ushort maxCompositeContours;
        public ushort maxZones;
        public ushort maxTwilightPoints;
        public ushort maxStorage;
        public ushort maxFunctionDefs;
        public ushort maxInstructionDefs;
        public ushort maxStackElements;
        public ushort maxSizeOfInstructions;
        public ushort maxComponentElements;
        public ushort maxComponentDepth;

        public MaximumProfileTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                version = FontData!.ReadFixed();
                numGlyphs = FontData.ReadUShort();
                maxPoints = FontData.ReadUShort();
                maxContours = FontData.ReadUShort();
                maxCompositePoints = FontData.ReadUShort();
                maxCompositeContours = FontData.ReadUShort();
                maxZones = FontData.ReadUShort();
                maxTwilightPoints = FontData.ReadUShort();
                maxStorage = FontData.ReadUShort();
                maxFunctionDefs = FontData.ReadUShort();
                maxInstructionDefs = FontData.ReadUShort();
                maxStackElements = FontData.ReadUShort();
                maxSizeOfInstructions = FontData.ReadUShort();
                maxComponentElements = FontData.ReadUShort();
                maxComponentDepth = FontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// The naming table allows multilingual strings to be associated with the OpenType font file.
    /// These strings can represent copyright notices, font names, family names, style names, and so on.
    /// To keep this table short, the font manufacturer may wish to make a limited set of entries in some
    /// small set of languages; later, the font can be "localized" and the strings translated or added.
    /// Other parts of the OpenType font file that require these strings can then refer to them simply by
    /// their index number. Clients that need a particular string can look it up by its platform ID, character
    /// encoding ID, language ID and name ID. Note that some platforms may require single byte character
    /// strings, while others may require double byte strings. 
    ///
    /// For historical reasons, some applications which install fonts perform Version control using Macintosh
    /// platform (platform ID 1) strings from the 'name' table. Because of this, we strongly recommend that
    /// the 'name' table of all fonts include Macintosh platform strings and that the syntax of the Version
    /// number (name ID 5) follows the guidelines given in this document.
    /// </summary>
    public class NameTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Name;
        const int NameArraySize = (int)NameId.MaxValue + 1;
        const ushort EnglishUS = 0x0409;

        /// <summary>
        /// Get the OpenType family name.
        /// This is the FamilyName in FontFace.
        /// </summary>
        public string OTFamilyName = "";

        /// <summary>
        /// Get the OpenType subfamily name.
        /// This is the FaceName in FontFace.
        /// </summary>
        public string OTSubfamilyName = "";

        /// <summary>
        /// Get the OpenType full font name.
        /// </summary>
        public string OTFullFontName = "";

        /// <summary>
        /// All en-US names of the name table.
        /// </summary>
        public string[] NamesEnUs = new string[NameArraySize];

        public ushort format;
        public ushort count;
        public ushort stringOffset;

        byte[] bytes = null!;

        public NameTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                Debug.Assert(FontData != null);
#if DEBUG || true  // TODO_OLD
                if (FontData!.Position != DirectoryEntry.Offset)
                    throw new InvalidOperationException("FontData!.Position != DirectoryEntry.Offset");

                FontData.Position = DirectoryEntry.Offset;
#endif
                bytes = new byte[DirectoryEntry.PaddedLength];
                Buffer.BlockCopy(FontData.OTFontSource.Bytes, DirectoryEntry.Offset, bytes, 0, DirectoryEntry.Length);

                format = FontData.ReadUShort();
                count = FontData.ReadUShort();
                stringOffset = FontData.ReadUShort();

                if (format == 1)
                {
                    Debug.Assert(false, "Check this case.");
                }

                for (int idx = 0; idx < count; idx++)
                {
                    var nrec = ReadNameRecord();
                    byte[] value = new byte[nrec.length];
                    Buffer.BlockCopy(FontData.OTFontSource.Bytes, DirectoryEntry.Offset + stringOffset + nrec.offset, value, 0, nrec.length);

                    //De/bug.WriteLine(nrec.platformID.ToString());

                    // Read font name and style in US English.
                    if (nrec.platformID is 0 or 3)
                    {
                        if (nrec is { languageID: EnglishUS, nameID: < NameId.MaxValue })
                        {
                            NamesEnUs[nrec.nameID] = Encoding.BigEndianUnicode.GetString(value, 0, value.Length);
                        }

                        // Font Family name. Up to four fonts can share the Font Family name, 
                        // forming a font style linking group (regular, italic, bold, bold italic - 
                        // as defined by OS/2.fsSelection bit settings).
                        if (nrec.nameID == NameId.FamilyName && nrec.languageID == EnglishUS)
                        {
                            if (String.IsNullOrEmpty(OTFamilyName))
                                OTFamilyName = Encoding.BigEndianUnicode.GetString(value, 0, value.Length);
                        }

                        // Font Subfamily name. The Font Subfamily name distinguishes the font in a 
                        // group with the same Font Family name (name ID 1). This is assumed to
                        // address style (italic, oblique) and weight (light, bold, black, etc.).
                        // A font with no particular differences in weight or style (e.g. medium weight,
                        // not italic and fsSelection bit 6 set) should have the string “Regular” stored in 
                        // this position.
                        if (nrec.nameID == NameId.SubfamilyName && nrec.languageID == EnglishUS)
                        {
                            if (String.IsNullOrEmpty(OTSubfamilyName))
                                OTSubfamilyName = Encoding.BigEndianUnicode.GetString(value, 0, value.Length);
                        }

                        // Full font name; a combination of strings 1 and 2, or a similar human-readable
                        // variant. If string 2 is "Regular", it is sometimes omitted from name ID 4.
                        if (nrec.nameID == NameId.FullName && nrec.languageID == EnglishUS)
                        {
                            if (String.IsNullOrEmpty(OTFullFontName))
                                OTFullFontName = Encoding.BigEndianUnicode.GetString(value, 0, value.Length);
                        }
                    }
                }
                Debug.Assert(!String.IsNullOrEmpty(OTFamilyName));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }

        NameRecord ReadNameRecord()
        {
            var nrec = new NameRecord
            {
                platformID = FontData!.ReadUShort(),
                encodingID = FontData.ReadUShort(),
                languageID = FontData.ReadUShort(),
                nameID = FontData.ReadUShort(),
                length = FontData.ReadUShort(),
                offset = FontData.ReadUShort()
            };
            return nrec;
        }

        class NameRecord
        {
            public ushort platformID;
            public ushort encodingID;
            public ushort languageID;
            public ushort nameID;
            public ushort length;
            public ushort offset;
        }
    }

    /// <summary>
    /// The OS/2 table consists of a set of Metrics that are required in OpenType fonts. 
    /// </summary>
    public class OS2Table : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.OS2;

        public class FontSelectionFlags
        {
            public const ushort Italic = 1 << 0;
            public const ushort Bold = 1 << 5;
            public const ushort Regular = 1 << 6;
            public const ushort Oblique = 1 << 9;
        }

        public ushort version;
        public short xAvgCharWidth;
        public ushort usWeightClass;
        public ushort usWidthClass;
        public ushort fsType;
        public short ySubscriptXSize;
        public short ySubscriptYSize;
        public short ySubscriptXOffset;
        public short ySubscriptYOffset;
        public short ySuperscriptXSize;
        public short ySuperscriptYSize;
        public short ySuperscriptXOffset;
        public short ySuperscriptYOffset;
        public short yStrikeoutSize;
        public short yStrikeoutPosition;
        public short sFamilyClass;
        public byte[] panose = default!; // = new byte[10]; // NRT
        public uint ulUnicodeRange1; // Bits 0-31
        public uint ulUnicodeRange2; // Bits 32-63
        public uint ulUnicodeRange3; // Bits 64-95
        public uint ulUnicodeRange4; // Bits 96-127
        public string achVendID = default!; // = ""; // NRT
        public ushort fsSelection;
        public ushort usFirstCharIndex;
        public ushort usLastCharIndex;
        public short sTypoAscender;
        public short sTypoDescender;
        public short sTypoLineGap;
        public ushort usWinAscent;
        public ushort usWinDescent;
        // Version >= 1
        public uint ulCodePageRange1; // Bits 0-31
        public uint ulCodePageRange2; // Bits 32-63
        // Version >= 2
        public short sxHeight;
        public short sCapHeight;
        public ushort usDefaultChar;
        public ushort usBreakChar;
        public ushort usMaxContext;

        public OS2Table(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                version = FontData!.ReadUShort(); // NRT
                xAvgCharWidth = FontData.ReadShort();
                usWeightClass = FontData.ReadUShort();
                usWidthClass = FontData.ReadUShort();
                fsType = FontData.ReadUShort();
                ySubscriptXSize = FontData.ReadShort();
                ySubscriptYSize = FontData.ReadShort();
                ySubscriptXOffset = FontData.ReadShort();
                ySubscriptYOffset = FontData.ReadShort();
                ySuperscriptXSize = FontData.ReadShort();
                ySuperscriptYSize = FontData.ReadShort();
                ySuperscriptXOffset = FontData.ReadShort();
                ySuperscriptYOffset = FontData.ReadShort();
                yStrikeoutSize = FontData.ReadShort();
                yStrikeoutPosition = FontData.ReadShort();
                sFamilyClass = FontData.ReadShort();
                panose = FontData.ReadBytes(10);
                ulUnicodeRange1 = FontData.ReadULong();
                ulUnicodeRange2 = FontData.ReadULong();
                ulUnicodeRange3 = FontData.ReadULong();
                ulUnicodeRange4 = FontData.ReadULong();
                achVendID = FontData.ReadString(4);
                fsSelection = FontData.ReadUShort();
                usFirstCharIndex = FontData.ReadUShort();
                usLastCharIndex = FontData.ReadUShort();
                sTypoAscender = FontData.ReadShort();
                sTypoDescender = FontData.ReadShort();
                sTypoLineGap = FontData.ReadShort();
                usWinAscent = FontData.ReadUShort();
                usWinDescent = FontData.ReadUShort();

                if (version >= 1)
                {
                    ulCodePageRange1 = FontData.ReadULong();
                    ulCodePageRange2 = FontData.ReadULong();

                    if (version >= 2)
                    {
                        sxHeight = FontData.ReadShort();
                        sCapHeight = FontData.ReadShort();
                        usDefaultChar = FontData.ReadUShort();
                        usBreakChar = FontData.ReadUShort();
                        usMaxContext = FontData.ReadUShort();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }

        public bool IsBold => (fsSelection & FontSelectionFlags.Bold) != 0;

        public bool IsItalic => (fsSelection & FontSelectionFlags.Italic) != 0;
    }

    /// <summary>
    /// This table contains additional information needed to use TrueType or OpenType fonts
    /// on PostScript printers. 
    /// </summary>
    public class PostScriptTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Post;

        public Fixed formatType;
        public float italicAngle;
        public FWord underlinePosition;
        public FWord underlineThickness;
        public ulong isFixedPitch;
        public ulong minMemType42;
        public ulong maxMemType42;
        public ulong minMemType1;
        public ulong maxMemType1;

        public PostScriptTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                formatType = FontData!.ReadFixed(); // NRT
                italicAngle = FontData.ReadFixed() / 65536f;
                underlinePosition = FontData.ReadFWord();
                underlineThickness = FontData.ReadFWord();
                isFixedPitch = FontData.ReadULong();
                minMemType42 = FontData.ReadULong();
                maxMemType42 = FontData.ReadULong();
                minMemType1 = FontData.ReadULong();
                maxMemType1 = FontData.ReadULong();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table contains a list of values that can be referenced by instructions.
    /// They can be used, among other things, to control characteristics for different glyphs.
    /// The length of the table must be an integral number of FWORD units.
    /// </summary>
    public class ControlValueTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Cvt;

        FWord[] array = default!; // List of n values referenceable by instructions. n is the number of FWORD items that fit in the size of the table.

        public ControlValueTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            DirectoryEntry.Tag = TableTagNames.Cvt;
            DirectoryEntry = fontData.TableDictionary[TableTagNames.Cvt];
            Read();
        }

        public void Read()
        {
            try
            {
                int length = DirectoryEntry.Length / 2;
                array = new FWord[length];
                for (int idx = 0; idx < length; idx++)
                    array[idx] = FontData!.ReadFWord(); // NRT
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table is similar to the CVT Program, except that it is only run once, when the font is first used.
    /// It is used only for FDEFs and IDEFs. Thus, the CVT Program need not contain function definitions.
    /// However, the CVT Program may redefine existing FDEFs or IDEFs.
    /// </summary>
    public class FontProgram : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Fpgm;

        byte[] bytes = null!; // Instructions. n is the number of BYTE items that fit in the size of the table.

        public FontProgram(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            DirectoryEntry.Tag = TableTagNames.Fpgm;
            DirectoryEntry = fontData.TableDictionary[TableTagNames.Fpgm];
            Read();
        }

        public void Read()
        {
            try
            {
                int length = DirectoryEntry.Length;
                bytes = new byte[length];
                for (int idx = 0; idx < length; idx++)
                    bytes[idx] = FontData!.ReadByte();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// The Control Value Program consists of a set of TrueType instructions that will be executed whenever the font or
    /// point size or transformation matrix change and before each glyph is interpreted. Any instruction is legal in the
    /// CVT Program but since no glyph is associated with it, instructions intended to move points within a particular
    /// glyph outline cannot be used in the CVT Program. The name 'prep' is anachronistic.
    /// </summary>
    public class ControlValueProgram : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Prep;

        byte[] bytes = null!; // Set of instructions executed whenever point size or font or transformation change. n is the number of BYTE items that fit in the size of the table.

        public ControlValueProgram(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            DirectoryEntry.Tag = TableTagNames.Prep;
            DirectoryEntry = fontData.TableDictionary[TableTagNames.Prep];
            Read();
        }

        public void Read()
        {
            try
            {
                int length = DirectoryEntry.Length;
                bytes = new byte[length];
                for (int idx = 0; idx < length; idx++)
                    bytes[idx] = FontData!.ReadByte(); // NRT
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table contains information that describes the glyphs in the font in the TrueType outline format.
    /// Information regarding the rasterizer (scaler) refers to the TrueType rasterizer.
    /// </summary>
    public class GlyphSubstitutionTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.GSUB;

        public GlyphSubstitutionTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            DirectoryEntry.Tag = TableTagNames.GSUB;
            DirectoryEntry = fontData.TableDictionary[TableTagNames.GSUB];
            Read();
        }

        public void Read()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(OtMsgs.ErrorReadingFontData, ex);
            }
        }
    }
}
