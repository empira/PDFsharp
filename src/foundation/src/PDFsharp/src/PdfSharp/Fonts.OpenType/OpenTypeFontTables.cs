// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Drawing;

using Fixed = System.Int32;
using FWord = System.Int16;
using UFWord = System.UInt16;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace PdfSharp.Fonts.OpenType
{
    enum PlatformId
    {
        Apple, Mac, Iso, Win
    }

    /// <summary>
    /// Only Symbol and Unicode are used by PDFsharp.
    /// </summary>
    enum WinEncodingId
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
    /// CMap format 4: Segment mapping to delta values.
    /// The Windows standard format.
    /// </summary>
    class CMap4 : OpenTypeFontTable
    {
        public WinEncodingId encodingId; // Windows encoding ID.
        public ushort format; // Format number is set to 4.
        public ushort length; // This is the length in bytes of the sub-table. 
        public ushort language; // This field must be set to zero for all cmap sub-tables whose platform IDs are other than Macintosh (platform ID 1). 
        public ushort segCountX2; // 2 x segCount.
        public ushort searchRange; // 2 x (2**floor(log2(segCount)))
        public ushort entrySelector; // log2(searchRange/2)
        public ushort rangeShift;
        public ushort[] endCount = null!; // [segCount] / End characterCode for each segment, last=0xFFFF.
        public ushort[] startCount = null!; // [segCount] / Start character code for each segment.
        public short[] idDelta = null!; // [segCount] / Delta for all character codes in segment.
        public ushort[] idRangeOffs = null!; // [segCount] / Offsets into glyphIdArray or 0
        public int glyphCount; // = (length - (16 + 4 * 2 * segCount)) / 2;
        public ushort[] glyphIdArray = null!;     // Glyph index array (arbitrary length)

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
                format = _fontData!.ReadUShort(); // NRT
                Debug.Assert(format == 4, "Only format 4 expected.");
                length = _fontData.ReadUShort();
                language = _fontData.ReadUShort();  // Always null in Windows
                segCountX2 = _fontData.ReadUShort();
                searchRange = _fontData.ReadUShort();
                entrySelector = _fontData.ReadUShort();
                rangeShift = _fontData.ReadUShort();

                int segCount = segCountX2 / 2;
                glyphCount = (length - (16 + 8 * segCount)) / 2;

                //ASSERT_CONDITION(0 <= m_NumGlyphIds && m_NumGlyphIds < m_Length, "Invalid Index");

                endCount = new ushort[segCount];
                startCount = new ushort[segCount];
                idDelta = new short[segCount];
                idRangeOffs = new ushort[segCount];

                glyphIdArray = new ushort[glyphCount];

                for (int idx = 0; idx < segCount; idx++)
                    endCount[idx] = _fontData.ReadUShort();

                // Read reserved pad.
                _fontData.ReadUShort();

                for (int idx = 0; idx < segCount; idx++)
                    startCount[idx] = _fontData.ReadUShort();

                for (int idx = 0; idx < segCount; idx++)
                    idDelta[idx] = _fontData.ReadShort();

                for (int idx = 0; idx < segCount; idx++)
                    idRangeOffs[idx] = _fontData.ReadUShort();

                for (int idx = 0; idx < glyphCount; idx++)
                    glyphIdArray[idx] = _fontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// CMap format 12: Segmented coverage.
    /// The Windows standard format.
    /// </summary>
    internal class CMap12 : OpenTypeFontTable
    {
        internal struct SequentialMapGroup
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
                format = _fontData!.ReadUShort(); // NRT
                Debug.Assert(format == 12, "Only format 12 expected.");
                _fontData.ReadUShort(); // Reserved.
                length = _fontData.ReadULong();
                language = _fontData.ReadULong(); // Always null in Windows.
                numGroups = _fontData.ReadULong();

                groups = new SequentialMapGroup[numGroups];

                for (int i = 0; i < groups.Length; i++)
                {
                    ref var group = ref groups[i];
                    group.startCharCode = _fontData.ReadULong();
                    group.endCharCode = _fontData.ReadULong();
                    group.startGlyphIndex = _fontData.ReadULong();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table defines the mapping of character codes to the glyph index values used in the font.
    /// It may contain more than one subtable, in order to support more than one character encoding scheme.
    /// </summary>
    class CMapTable : OpenTypeFontTable
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
                int tableOffset = _fontData!.Position;

                version = _fontData.ReadUShort();
                numTables = _fontData.ReadUShort();

                bool success = false;
                for (int idx = 0; idx < numTables; idx++)
                {
                    PlatformId platformId = (PlatformId)_fontData.ReadUShort();
                    WinEncodingId encodingId = (WinEncodingId)_fontData.ReadUShort();
                    int offset = _fontData.ReadLong();

                    int currentPosition = _fontData.Position;

                    // Just read Windows stuff.
                    if (platformId == PlatformId.Win &&
                        encodingId is WinEncodingId.Symbol or WinEncodingId.UnicodeUSC_2 or WinEncodingId.UnicodeUSC_4)
                    {
                        symbol = encodingId == WinEncodingId.Symbol;

                        _fontData.Position = tableOffset + offset;

                        var format = _fontData.ReadUShort();
                        _fontData.Position = tableOffset + offset;

                        if (format == 4)
                        {
                            cmap4 = new(_fontData, encodingId);
                        }
                        else if (format == 12)
                        {
                            cmap12 = new(_fontData, encodingId);
                        }

                        _fontData.Position = currentPosition;

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
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table adds support for multi-colored glyphs in a manner that integrates with the rasterizers
    /// of existing text engines and that is designed to be easy to support with current OpenType font files.
    /// </summary>
    class ColorTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.COLR;

        internal struct GlyphRecord
        {
            public ushort glyphId;
            public ushort firstLayerIndex;
            public ushort numLayers;
        }

        internal struct LayerRecord
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
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table is a set of one or more palettes, each containing a predefined number of color records.
    /// It may also contain 'name' table IDs describing the palettes and their entries.
    /// </summary>
    class ColorPalletTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.CPAL;

        public ushort version;
        public ushort numPaletteEntries;  // Number of palette entries in each palette.
        public ushort numPalettes;  // Number of palettes in the table.
        public ushort numColorRecords;  // Total number of color records, combined for all palettes.
        public uint colorRecordsArrayOffset;  // Offset from the beginning of CPAL table to the first ColorRecord.
        public ushort[] colorRecordIndices = [];  // Index of each palette’s first color record in the combined color record array.
        public XColor[] colorRecords = [];

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
                colorRecords = new XColor[numColorRecords];
                for (int idx = 0; idx < numColorRecords; idx++)
                {
                    var blue = fontData.ReadByte();
                    var green = fontData.ReadByte();
                    var red = fontData.ReadByte();
                    var alpha = fontData.ReadByte();
                    colorRecords[idx] = XColor.FromArgb(alpha, red, green, blue);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table gives global information about the font. The bounding box values should be computed using 
    /// only glyphs that have contours. Glyphs with no contours should be ignored for the purposes of these calculations.
    /// </summary>
    class FontHeaderTable : OpenTypeFontTable
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
                version = _fontData!.ReadFixed();
                fontRevision = _fontData.ReadFixed();
                checkSumAdjustment = _fontData.ReadULong();
                magicNumber = _fontData.ReadULong();
                flags = _fontData.ReadUShort();
                unitsPerEm = _fontData.ReadUShort();
                created = _fontData.ReadLongDate();
                modified = _fontData.ReadLongDate();
                xMin = _fontData.ReadShort();
                yMin = _fontData.ReadShort();
                xMax = _fontData.ReadShort();
                yMax = _fontData.ReadShort();
                macStyle = _fontData.ReadUShort();
                lowestRecPPEM = _fontData.ReadUShort();
                fontDirectionHint = _fontData.ReadShort();
                indexToLocFormat = _fontData.ReadShort();
                glyphDataFormat = _fontData.ReadShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table contains information for horizontal layout. The values in the minRightSideBearing, 
    /// MinLeftSideBearing and xMaxExtent should be computed using only glyphs that have contours.
    /// Glyphs with no contours should be ignored for the purposes of these calculations.
    /// All reserved areas must be set to 0. 
    /// </summary>
    class HorizontalHeaderTable : OpenTypeFontTable
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
                version = _fontData!.ReadFixed();
                ascender = _fontData.ReadFWord();
                descender = _fontData.ReadFWord();
                lineGap = _fontData.ReadFWord();
                advanceWidthMax = _fontData.ReadUFWord();
                minLeftSideBearing = _fontData.ReadFWord();
                minRightSideBearing = _fontData.ReadFWord();
                xMaxExtent = _fontData.ReadFWord();
                caretSlopeRise = _fontData.ReadShort();
                caretSlopeRun = _fontData.ReadShort();
                reserved1 = _fontData.ReadShort();
                reserved2 = _fontData.ReadShort();
                reserved3 = _fontData.ReadShort();
                reserved4 = _fontData.ReadShort();
                reserved5 = _fontData.ReadShort();
                metricDataFormat = _fontData.ReadShort();
                numberOfHMetrics = _fontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    class HorizontalMetrics : OpenTypeFontTable
    {
        public const string Tag = "----";

        public ushort advanceWidth;
        public short lsb;

        public HorizontalMetrics(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                advanceWidth = _fontData!.ReadUFWord();
                lsb = _fontData.ReadFWord();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// The type longHorMetric is defined as an array where each element has two parts:
    /// the advance width, which is of type USHORT, and the left side bearing, which is of type SHORT.
    /// These fields are in font design units.
    /// </summary>
    class HorizontalMetricsTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.HMtx;

        public HorizontalMetrics[] Metrics = default!;
        public FWord[] LeftSideBearing = default!;

        public HorizontalMetricsTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                var hhea = _fontData!.hhea;
                var maxp = _fontData.maxp;
                if (hhea != null! && maxp != null!)
                {
                    int numMetrics = hhea.numberOfHMetrics; //->NumberOfHMetrics();
                    int numLsbs = maxp.numGlyphs - numMetrics;

                    Debug.Assert(numMetrics != 0);
                    Debug.Assert(numLsbs >= 0);

                    Metrics = new HorizontalMetrics[numMetrics];
                    for (int idx = 0; idx < numMetrics; idx++)
                        Metrics[idx] = new HorizontalMetrics(_fontData);

                    if (numLsbs > 0)
                    {
                        LeftSideBearing = new FWord[numLsbs];
                        for (int idx = 0; idx < numLsbs; idx++)
                            LeftSideBearing[idx] = _fontData.ReadFWord();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    // Not used in PDFsharp.
    class VerticalHeaderTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.VHea;

        // Code comes from HorizontalHeaderTable.
        public Fixed Version; // 0x00010000 for Version 1.0.
        public FWord Ascender; // Typographic ascent. (Distance from baseline of highest Ascender) 
        public FWord Descender; // Typographic descent. (Distance from baseline of lowest Descender) 
        public FWord LineGap; // Typographic line gap. Negative LineGap values are treated as zero in Windows 3.1, System 6, and System 7.
        public UFWord AdvanceWidthMax;
        public FWord MinLeftSideBearing;
        public FWord MinRightSideBearing;
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

        public VerticalHeaderTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                Version = _fontData!.ReadFixed();
                Ascender = _fontData.ReadFWord();
                Descender = _fontData.ReadFWord();
                LineGap = _fontData.ReadFWord();
                AdvanceWidthMax = _fontData.ReadUFWord();
                MinLeftSideBearing = _fontData.ReadFWord();
                MinRightSideBearing = _fontData.ReadFWord();
                xMaxExtent = _fontData.ReadFWord();
                caretSlopeRise = _fontData.ReadShort();
                caretSlopeRun = _fontData.ReadShort();
                reserved1 = _fontData.ReadShort();
                reserved2 = _fontData.ReadShort();
                reserved3 = _fontData.ReadShort();
                reserved4 = _fontData.ReadShort();
                reserved5 = _fontData.ReadShort();
                metricDataFormat = _fontData.ReadShort();
                numberOfHMetrics = _fontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    class VerticalMetrics : OpenTypeFontTable
    {
        public const string Tag = "----";

        // code comes from HorizontalMetrics
        public ushort advanceWidth;
        public short lsb;

        public VerticalMetrics(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                advanceWidth = _fontData!.ReadUFWord();
                lsb = _fontData.ReadFWord();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// The vertical Metrics table allows you to specify the vertical spacing for each glyph in a
    /// vertical font. This table consists of either one or two arrays that contain metric
    /// information (the advance heights and top sidebearings) for the vertical layout of each
    /// of the glyphs in the font.
    /// </summary>
    // Not used in PDFsharp.
    class VerticalMetricsTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.VMtx;

        // Code comes from HorizontalMetricsTable.
        public HorizontalMetrics[] metrics;
        public FWord[] leftSideBearing;

        public VerticalMetricsTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
            throw new NotImplementedException("VerticalMetricsTable");
        }

        public void Read()
        {
            try
            {
                var hhea = _fontData!.hhea;
                var maxp = _fontData.maxp;
                if (hhea != null! && maxp != null!)
                {
                    int numMetrics = hhea.numberOfHMetrics; //->NumberOfHMetrics();
                    int numLsbs = maxp.numGlyphs - numMetrics;

                    Debug.Assert(numMetrics != 0);
                    Debug.Assert(numLsbs >= 0);

                    metrics = new HorizontalMetrics[numMetrics];
                    for (int idx = 0; idx < numMetrics; idx++)
                        metrics[idx] = new(_fontData);

                    if (numLsbs > 0)
                    {
                        leftSideBearing = new FWord[numLsbs];
                        for (int idx = 0; idx < numLsbs; idx++)
                            leftSideBearing[idx] = _fontData.ReadFWord();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table establishes the memory requirements for this font.
    /// Fonts with CFF data must use Version 0.5 of this table, specifying only the numGlyphs field.
    /// Fonts with TrueType outlines must use Version 1.0 of this table, where all data is required.
    /// Both formats of OpenType require a 'maxp' table because a number of applications call the 
    /// Windows GetFontData() API on the 'maxp' table to determine the number of glyphs in the font.
    /// </summary>
    class MaximumProfileTable : OpenTypeFontTable
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
                version = _fontData!.ReadFixed();
                numGlyphs = _fontData.ReadUShort();
                maxPoints = _fontData.ReadUShort();
                maxContours = _fontData.ReadUShort();
                maxCompositePoints = _fontData.ReadUShort();
                maxCompositeContours = _fontData.ReadUShort();
                maxZones = _fontData.ReadUShort();
                maxTwilightPoints = _fontData.ReadUShort();
                maxStorage = _fontData.ReadUShort();
                maxFunctionDefs = _fontData.ReadUShort();
                maxInstructionDefs = _fontData.ReadUShort();
                maxStackElements = _fontData.ReadUShort();
                maxSizeOfInstructions = _fontData.ReadUShort();
                maxComponentElements = _fontData.ReadUShort();
                maxComponentDepth = _fontData.ReadUShort();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
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
    class NameTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Name;

        /// <summary>
        /// Get the font family name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Get the font subfamily name.
        /// </summary>
        public string Style = "";

        /// <summary>
        /// Get the full font name.
        /// </summary>
        public string FullFontName = "";

        public ushort format;
        public ushort count;
        public ushort stringOffset;

        byte[] bytes = default!;

        public NameTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            Read();
        }

        public void Read()
        {
            try
            {
                Debug.Assert(_fontData != null);
#if DEBUG || true  // TODO_OLD
                if (_fontData.Position != DirectoryEntry.Offset)
                    throw new InvalidOperationException("_fontData!.Position != DirectoryEntry.Offset");

                _fontData.Position = DirectoryEntry.Offset;
#endif
                bytes = new byte[DirectoryEntry.PaddedLength];
                Buffer.BlockCopy(_fontData.FontSource.Bytes, DirectoryEntry.Offset, bytes, 0, DirectoryEntry.Length);

                format = _fontData.ReadUShort();
                count = _fontData.ReadUShort();
                stringOffset = _fontData.ReadUShort();

                for (int idx = 0; idx < count; idx++)
                {
                    var nrec = ReadNameRecord();
                    byte[] value = new byte[nrec.length];
                    Buffer.BlockCopy(_fontData.FontSource.Bytes, DirectoryEntry.Offset + stringOffset + nrec.offset, value, 0, nrec.length);

                    //De/bug.WriteLine(nrec.platformID.ToString());

                    // Read font name and style in US English.
                    if (nrec.platformID is 0 or 3)
                    {
                        // Font Family name. Up to four fonts can share the Font Family name, 
                        // forming a font style linking group (regular, italic, bold, bold italic - 
                        // as defined by OS/2.fsSelection bit settings).
                        if (nrec.nameID == 1 && nrec.languageID == 0x0409)
                        {
                            if (String.IsNullOrEmpty(Name))
                                Name = Encoding.BigEndianUnicode.GetString(value, 0, value.Length);
                        }

                        // Font Subfamily name. The Font Subfamily name distinguishes the font in a 
                        // group with the same Font Family name (name ID 1). This is assumed to
                        // address style (italic, oblique) and weight (light, bold, black, etc.).
                        // A font with no particular differences in weight or style (e.g. medium weight,
                        // not italic and fsSelection bit 6 set) should have the string “Regular” stored in 
                        // this position.
                        if (nrec.nameID == 2 && nrec.languageID == 0x0409)
                        {
                            if (String.IsNullOrEmpty(Style))
                                Style = Encoding.BigEndianUnicode.GetString(value, 0, value.Length);
                        }

                        // Full font name; a combination of strings 1 and 2, or a similar human-readable
                        // variant. If string 2 is "Regular", it is sometimes omitted from name ID 4.
                        if (nrec.nameID == 4 && nrec.languageID == 0x0409)
                        {
                            if (String.IsNullOrEmpty(FullFontName))
                                FullFontName = Encoding.BigEndianUnicode.GetString(value, 0, value.Length);
                        }
                    }
                }
                Debug.Assert(!String.IsNullOrEmpty(Name));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }

        NameRecord ReadNameRecord()
        {
            var nrec = new NameRecord
            {
                platformID = _fontData!.ReadUShort(),
                encodingID = _fontData.ReadUShort(),
                languageID = _fontData.ReadUShort(),
                nameID = _fontData.ReadUShort(),
                length = _fontData.ReadUShort(),
                offset = _fontData.ReadUShort()
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
    class OS2Table : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.OS2;

        [Flags]
        public enum FontSelectionFlags : ushort
        {
            Italic = 1 << 0,
            Bold = 1 << 5,
            Regular = 1 << 6,
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
                version = _fontData!.ReadUShort(); // NRT
                xAvgCharWidth = _fontData.ReadShort();
                usWeightClass = _fontData.ReadUShort();
                usWidthClass = _fontData.ReadUShort();
                fsType = _fontData.ReadUShort();
                ySubscriptXSize = _fontData.ReadShort();
                ySubscriptYSize = _fontData.ReadShort();
                ySubscriptXOffset = _fontData.ReadShort();
                ySubscriptYOffset = _fontData.ReadShort();
                ySuperscriptXSize = _fontData.ReadShort();
                ySuperscriptYSize = _fontData.ReadShort();
                ySuperscriptXOffset = _fontData.ReadShort();
                ySuperscriptYOffset = _fontData.ReadShort();
                yStrikeoutSize = _fontData.ReadShort();
                yStrikeoutPosition = _fontData.ReadShort();
                sFamilyClass = _fontData.ReadShort();
                panose = _fontData.ReadBytes(10);
                ulUnicodeRange1 = _fontData.ReadULong();
                ulUnicodeRange2 = _fontData.ReadULong();
                ulUnicodeRange3 = _fontData.ReadULong();
                ulUnicodeRange4 = _fontData.ReadULong();
                achVendID = _fontData.ReadString(4);
                fsSelection = _fontData.ReadUShort();
                usFirstCharIndex = _fontData.ReadUShort();
                usLastCharIndex = _fontData.ReadUShort();
                sTypoAscender = _fontData.ReadShort();
                sTypoDescender = _fontData.ReadShort();
                sTypoLineGap = _fontData.ReadShort();
                usWinAscent = _fontData.ReadUShort();
                usWinDescent = _fontData.ReadUShort();

                if (version >= 1)
                {
                    ulCodePageRange1 = _fontData.ReadULong();
                    ulCodePageRange2 = _fontData.ReadULong();

                    if (version >= 2)
                    {
                        sxHeight = _fontData.ReadShort();
                        sCapHeight = _fontData.ReadShort();
                        usDefaultChar = _fontData.ReadUShort();
                        usBreakChar = _fontData.ReadUShort();
                        usMaxContext = _fontData.ReadUShort();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }

        public bool IsBold => (fsSelection & (ushort)FontSelectionFlags.Bold) != 0;

        public bool IsItalic => (fsSelection & (ushort)FontSelectionFlags.Italic) != 0;
    }

    /// <summary>
    /// This table contains additional information needed to use TrueType or OpenType fonts
    /// on PostScript printers. 
    /// </summary>
    class PostScriptTable : OpenTypeFontTable
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
                formatType = _fontData!.ReadFixed(); // NRT
                italicAngle = _fontData.ReadFixed() / 65536f;
                underlinePosition = _fontData.ReadFWord();
                underlineThickness = _fontData.ReadFWord();
                isFixedPitch = _fontData.ReadULong();
                minMemType42 = _fontData.ReadULong();
                maxMemType42 = _fontData.ReadULong();
                minMemType1 = _fontData.ReadULong();
                maxMemType1 = _fontData.ReadULong();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table contains a list of values that can be referenced by instructions.
    /// They can be used, among other things, to control characteristics for different glyphs.
    /// The length of the table must be an integral number of FWORD units.
    /// </summary>
    class ControlValueTable : OpenTypeFontTable
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
                    array[idx] = _fontData!.ReadFWord(); // NRT
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table is similar to the CVT Program, except that it is only run once, when the font is first used.
    /// It is used only for FDEFs and IDEFs. Thus, the CVT Program need not contain function definitions.
    /// However, the CVT Program may redefine existing FDEFs or IDEFs.
    /// </summary>
    class FontProgram : OpenTypeFontTable
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
                    bytes[idx] = _fontData!.ReadByte();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// The Control Value Program consists of a set of TrueType instructions that will be executed whenever the font or
    /// point size or transformation matrix change and before each glyph is interpreted. Any instruction is legal in the
    /// CVT Program but since no glyph is associated with it, instructions intended to move points within a particular
    /// glyph outline cannot be used in the CVT Program. The name 'prep' is anachronistic.
    /// </summary>
    class ControlValueProgram : OpenTypeFontTable
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
                    bytes[idx] = _fontData!.ReadByte(); // NRT
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }

    /// <summary>
    /// This table contains information that describes the glyphs in the font in the TrueType outline format.
    /// Information regarding the rasterizer (scaler) refers to the TrueType rasterizer.
    /// </summary>
    class GlyphSubstitutionTable : OpenTypeFontTable
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
                throw new InvalidOperationException(PsMsgs.ErrorReadingFontData, ex);
            }
        }
    }
}
