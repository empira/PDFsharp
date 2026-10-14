// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Globalization;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using Fixed = System.Int32;

// v7.0 TODO review

#pragma warning disable 0649

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Represents an OpenType font face in memory.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed partial class OpenTypeFontFace  // In English, it’s spelled 'typeface', but 'font face'.
    {
        // http://www.microsoft.com/typography/otspec/

        /// <summary>
        /// Shallow copy for font subset.
        /// </summary>
        OpenTypeFontFace(OpenTypeFontFace fontFace)
        {
            _offsetTable = fontFace._offsetTable;
            _fullFaceName = fontFace._fullFaceName;
            OTDescriptor = fontFace.OTDescriptor;
        }

        OpenTypeFontFace(OpenTypeFontSource fontSource)
        {
            Debug.Assert(fontSource.OTFontFace == null);
            OTFontSource = fontSource;
            Read();  // This unimpressive line reads all the font tables we need.
            OTDescriptor = new OpenTypeFontDescriptor(this);
            _fullFaceName = name.OTFullFontName;
            Debug.Assert(fontSource.FontFaceKey == null);
            fontSource.OTFontFace = this;
            var key = OTDescriptor.Key;
        }

        public readonly OpenTypeFontDescriptor OTDescriptor;

        public static OpenTypeFontFace GetOrCreateFrom(OpenTypeFontSource otFontSource)
        {
            Debug.Assert(otFontSource.OTFontFace == null);

            var openTypeFontFaceCache = OtGlobals.Global.OTFonts.OpenTypeFontFaceCache;
            if (openTypeFontFaceCache.TryGetFontFace(otFontSource.ChecksumKey, out var otFontFace))
            {
                // We should not come here if the font source is in the cache but has no font face.
                Debug.Assert(otFontSource.OTFontFace != null, "Ooops, something went wrong.");
                return otFontFace;
            }

            Debug.Assert(otFontSource.FontFaceKey == null);
            otFontFace = new(otFontSource);
            Debug.Assert(otFontSource.FontFaceKey != null);
            otFontSource.OTFontFace = otFontFace;
            otFontFace = openTypeFontFaceCache.AddFontFace(otFontFace);
            Debug.Assert(ReferenceEquals(otFontSource.OTFontFace, otFontFace));
            return otFontFace;
        }

        public int GlyphCount => OTDescriptor.GlyphCount;

        /// <summary>
        /// Gets the full face name from the name table.
        /// Name is also used as the key.
        /// </summary>
        public string FullFaceName => _fullFaceName;

        readonly string _fullFaceName;

        /// <summary>
        /// Gets the 64-bit check sum to identify the font face by its font source.
        /// </summary>
        public ulong CheckSum
        {
            get
            {
                if (_checkSum == 0)
                    _checkSum = ChecksumHelper.CalcChecksum(OTFontSource.Bytes);
                return _checkSum;
            }
        }
        ulong _checkSum;

        public void SetFontEmbedding(OpenTypeFontEmbedding fontEmbedding)
        {
            Debug.Assert(fontEmbedding is OpenTypeFontEmbedding.TryComputeSubset or OpenTypeFontEmbedding.EmbedCompleteFontFile);

            if (_fontEmbedding == (OpenTypeFontEmbedding)(-1))
            {
                _fontEmbedding = fontEmbedding;
                return;
            }

            if (fontEmbedding == _fontEmbedding)
                return;

            if (fontEmbedding == OpenTypeFontEmbedding.TryComputeSubset)
            {
                // Case: _fontEmbedding is already set to EmbedCompleteFontFile.
                LogHost.Logger.LogError("Font embedding option was already set to EmbedCompleteFontFile. Setting to TryComputeSubset is ignored.");
            }
            else
            {
                // Case: _fontEmbedding is already set to TryComputeSubset.
                LogHost.Logger.LogError("Font embedding option was already set to TryComputeSubset. Now it is reset to EmbedCompleteFontFile.");
                _fontEmbedding = fontEmbedding;
            }
        }
        OpenTypeFontEmbedding _fontEmbedding = (OpenTypeFontEmbedding)(-1);

        public OpenTypeGlyphMetrics GetOpenTypeGlyphMetrics(ushort glyphIndex)
        {
            // Create glyph metrics only on demand for the glyphs needed.

            _otMetrics ??= new OpenTypeGlyphMetrics?[GlyphCount];
            var otMetric = _otMetrics[glyphIndex] ??= CreateGlyphMetrics(glyphIndex);

            return otMetric;
        }
        OpenTypeGlyphMetrics?[]? _otMetrics;  // _metricses???

        public RenderingGlyphMetrics GetRenderingGlyphMetrics(ushort glyphIndex)
        {
            // Create glyph metrics only on demand for the glyphs needed.

            _rMetrics ??= new RenderingGlyphMetrics?[GlyphCount];
            var rMetrics = _rMetrics[glyphIndex] ??= CreateGlyphMetrics(GetOpenTypeGlyphMetrics(glyphIndex));

            return rMetrics;
        }
        RenderingGlyphMetrics?[]? _rMetrics;

        /// <summary>
        /// Gets the bytes that represents the font data.
        /// </summary>
        public OpenTypeFontSource OTFontSource
        {
            get => _otFontSource ?? NRT.ThrowOnNull<OpenTypeFontSource>();
            private set =>
                // Stop working if font was not found.
                _otFontSource = value ?? throw new InvalidOperationException("Font cannot be resolved.");
        }
        OpenTypeFontSource? _otFontSource;

#pragma warning disable CS0414 // Field is assigned but its value is never used
        /*internal*/
        FontTechnology? _fontTechnology;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        /*internal*/
        OffsetTable _offsetTable;

        /// <summary>
        /// The dictionary of all font tables.
        /// </summary>
        internal Dictionary<string, TableDirectoryEntry> TableDictionary = new();

        // Keep names identical to OpenType spec.
        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        public CMapTable cmap = null!; // NRT TODO_OLD Change programming model so that it fits NRTs.
        public ColorTable? colr;
        public ColorPalletTable? cpal;
        public ControlValueTable cvt = null!; // NRT
        public FontProgram fpgm = null!; // NRT
        public MaximumProfileTable maxp = null!; // NRT
        public NameTable name = null!; // NRT
        public ControlValueProgram? prep;
        public FontHeaderTable? head;
        public HorizontalHeaderTable hhea = null!; // NRT
        public HorizontalMetricsTable hmtx = null!; // NRT
        public OS2Table os2 = null!; // NRT
        public PostScriptTable post = null!; // NRT
        public GlyphDataTable glyf = null!; // NRT
        public IndexToLocationTable loca = null!; // NRT
        public GlyphSubstitutionTable gsub = null!; // NRT
        public VerticalHeaderTable? vhea;
        public VerticalMetricsTable? vmtx;
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming

        public bool CanRead => _otFontSource != null;

        public bool CanWrite => _otFontSource == null;

        /// <summary>
        /// Adds the specified table to this font image.
        /// </summary>
        public void AddTable(OpenTypeFontTable fontTable)
        {
            if (!CanWrite)
                throw new InvalidOperationException("Font image cannot be modified.");

            if (fontTable == null)
                throw new ArgumentNullException(nameof(fontTable));

            if (fontTable.FontData == null)
            {
                fontTable.FontData = this;
            }
            else
            {
                Debug.Assert(fontTable.FontData.CanRead);
                // Create a reference to this font table.
                fontTable = new IRefFontTable(this, fontTable);
            }

            //Debug.Assert(fontTable.FontData == null);
            //fontTable.fontData = this;

            TableDictionary[fontTable.DirectoryEntry.Tag] = fontTable.DirectoryEntry;
            switch (fontTable.DirectoryEntry.Tag)
            {
                case TableTagNames.CMap:
                    cmap = (fontTable as CMapTable)!; //?? NRT.ThrowOnNull<CMapTable>();
                    break;

                case TableTagNames.Cvt:
                    cvt = (fontTable as ControlValueTable)!; // ?? NRT.ThrowOnNull<ControlValueTable>();
                    break;

                case TableTagNames.Fpgm:
                    fpgm = (fontTable as FontProgram)!; // ?? NRT.ThrowOnNull<FontProgram>();

                    // BUG_OLD BUG_OLD BUG_OLD BUG_OLD
                    //Debug.Assert(fpgm != null);
                    break;

                case TableTagNames.MaxP:
                    maxp = (fontTable as MaximumProfileTable)!; // ?? NRT.ThrowOnNull<MaximumProfileTable>();
                    if (maxp != null!)
                    {
                        // It is never not null :-(
                        _ = typeof(int);
                    }
                    break;

                case TableTagNames.Name:
                    name = fontTable as NameTable ?? NRT.ThrowOnNull<NameTable>();
                    break;

                case TableTagNames.Head:
                    head = fontTable as FontHeaderTable; // ?? NRT.ThrowOnNull<FontHeaderTable>(); Can be null.
                    break;

                case TableTagNames.HHea:
                    hhea = (fontTable as HorizontalHeaderTable)!; // ?? NRT.ThrowOnNull<HorizontalHeaderTable>();
                    break;

                case TableTagNames.HMtx:
                    hmtx = (fontTable as HorizontalMetricsTable)!; // ?? NRT.ThrowOnNull<HorizontalMetricsTable>();
                    break;

                case TableTagNames.OS2:
                    os2 = fontTable as OS2Table ?? NRT.ThrowOnNull<OS2Table>();
                    break;

                case TableTagNames.Post:
                    post = fontTable as PostScriptTable ?? NRT.ThrowOnNull<PostScriptTable>();
                    break;

                case TableTagNames.Glyf:
                    glyf = fontTable as GlyphDataTable ?? NRT.ThrowOnNull<GlyphDataTable>();
                    break;

                case TableTagNames.Loca:
                    loca = fontTable as IndexToLocationTable ?? NRT.ThrowOnNull<IndexToLocationTable>();
                    break;

                case TableTagNames.GSUB:
                    gsub = fontTable as GlyphSubstitutionTable ?? NRT.ThrowOnNull<GlyphSubstitutionTable>();
                    break;

                case TableTagNames.Prep:
                    prep = (fontTable as ControlValueProgram); // ?? NRT.ThrowOnNull<ControlValueProgram>();
                    break;

                default:
                    _ = typeof(int);
                    break;
            }
        }

        /// <summary>
        /// Reads all required tables from the font data.
        /// </summary>
        internal void Read()
        {
            // Determine font technology
            // ReSharper disable InconsistentNaming
            const uint OTTO = 0x4f54544f;  // Adobe OpenType CFF data, tag: 'OTTO'
            const uint TTCF = 0x74746366;  // TrueType Collection tag: 'ttcf'  
            // ReSharper restore InconsistentNaming
            try
            {
#if DEBUG_
                if (Name == "Cambria")
                    _ = typeof(int);
#endif
                // Check if data is a TrueType collection font.
                uint startTag = ReadULong();
                if (startTag == TTCF)
                {
                    _fontTechnology = FontTechnology.TrueTypeCollection;
                    throw new InvalidOperationException("TrueType collection fonts are not yet supported by PDFsharp.");
                }

                // Read offset table
                _offsetTable.Version = startTag;
                _offsetTable.TableCount = ReadUShort();
                _offsetTable.SearchRange = ReadUShort();
                _offsetTable.EntrySelector = ReadUShort();
                _offsetTable.RangeShift = ReadUShort();

                // Move to table dictionary at position 12
                Debug.Assert(Position == 12);
                //tableDictionary = (offsetTable.TableCount);

                if (_offsetTable.Version == OTTO)
                    _fontTechnology = FontTechnology.PostscriptOutlines;
                else
                    _fontTechnology = FontTechnology.TrueTypeOutlines;

                for (int idx = 0; idx < _offsetTable.TableCount; idx++)
                {
                    TableDirectoryEntry entry = TableDirectoryEntry.ReadFrom(this);
                    TableDictionary.Add(entry.Tag, entry);
#if VERBOSE
                    Debug.WriteLine(String.Format("Font table: {0}", entry.Tag));
#endif
                }

                //// PDFlib checks this, but it is not part of the OpenType spec anymore
                //if (TableDictionary.ContainsKey("bhed"))
                //    throw new NotSupportedException("Bitmap fonts are not supported by PDFsharp.");

                // Read required tables.
                if (Seek(CMapTable.Tag) != -1)
                    cmap = new CMapTable(this);

                if (Seek(ColorTable.Tag) != -1)
                    colr = new ColorTable(this);

                if (Seek(ColorPalletTable.Tag) != -1)
                    cpal = new ColorPalletTable(this);

                if (Seek(ControlValueTable.Tag) != -1)
                    cvt = new ControlValueTable(this);

                if (Seek(FontProgram.Tag) != -1)
                    fpgm = new FontProgram(this);

                if (Seek(MaximumProfileTable.Tag) != -1)
                    maxp = new MaximumProfileTable(this);

                if (Seek(NameTable.Tag) != -1)
                    name = new NameTable(this);

                if (Seek(FontHeaderTable.Tag) != -1)
                    head = new FontHeaderTable(this);

                if (Seek(HorizontalHeaderTable.Tag) != -1)
                    hhea = new HorizontalHeaderTable(this);

                if (Seek(HorizontalMetricsTable.Tag) != -1)
                    hmtx = new HorizontalMetricsTable(this);

                if (Seek(OS2Table.Tag) != -1)
                    os2 = new OS2Table(this);

                if (Seek(PostScriptTable.Tag) != -1)
                    post = new PostScriptTable(this);

                if (Seek(GlyphDataTable.Tag) != -1)
                    glyf = new GlyphDataTable(this);

                if (Seek(IndexToLocationTable.Tag) != -1)
                    loca = new IndexToLocationTable(this);

                if (Seek(GlyphSubstitutionTable.Tag) != -1)
                    gsub = new GlyphSubstitutionTable(this);

                if (Seek(ControlValueProgram.Tag) != -1)
                    prep = new ControlValueProgram(this);

                if (Seek(VerticalHeaderTable.Tag) != -1)
                    vhea = new VerticalHeaderTable(this);

                if (Seek(VerticalMetricsTable.Tag) != -1)
                    vmtx = new VerticalMetricsTable(this);
            }
            catch (Exception ex)
            {
                LogHost.Logger./*FontManagementLogger.*/LogCritical($"Error while reading OpenType font face: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new font image that is a subset of this font image containing only the specified glyphs.
        /// </summary>
        public OpenTypeFontFace CreateFontSubset(Dictionary<ushort, object?> glyphs, bool cidFont)
        {
            // Do not create a subset?
            // No loca table means font has postscript outline.
            if (_fontEmbedding == OpenTypeFontEmbedding.EmbedCompleteFontFile || loca == null!)
                return this;

            // Create new font image.
            var fontData = new OpenTypeFontFace(this);

            // Create new loca and glyf table.
            var locaNew = new IndexToLocationTable();
            locaNew.ShortIndex = loca.ShortIndex;
            var glyfNew = new GlyphDataTable();

            // Add all required tables.
            //fontData.AddTable(os2);
            if (!cidFont)
                fontData.AddTable(cmap);
            if (cvt != null!)
                fontData.AddTable(cvt);
            if (fpgm != null!)
                fontData.AddTable(fpgm);
            fontData.AddTable(glyfNew);
            fontData.AddTable(head!);
            fontData.AddTable(hhea);
            fontData.AddTable(hmtx);
            fontData.AddTable(locaNew);
            if (maxp != null!)
                fontData.AddTable(maxp);
            //fontData.AddTable(name);
            if (prep != null)
                fontData.AddTable(prep);

            // Get closure of used glyphs.
            //Debug.Assert(glyphs != null);
            glyf.CompleteGlyphClosure(glyphs);

            // Create a sorted array of all used glyphs.
            int glyphCount = glyphs.Count;
            ushort[] glyphArray = new ushort[glyphCount];
            glyphs.Keys.CopyTo(glyphArray, 0);
            Array.Sort(glyphArray);

            // Calculate new size of glyph table.
            int size = 0;
            for (int idx = 0; idx < glyphCount; idx++)
                size += glyf.GetGlyphSize(glyphArray[idx]);
            glyfNew.DirectoryEntry.Length = size;

            // Create new loca table.
            int numGlyphs = maxp!.numGlyphs;
            locaNew.LocaTable = new int[numGlyphs + 1];

            // Create new glyf table.
            glyfNew.GlyphTable = new byte[glyfNew.DirectoryEntry.PaddedLength];

            // Fill new glyf and loca table.
            int glyphOffset = 0;
            int glyphIndex = 0;
            for (int idx = 0; idx < numGlyphs; idx++)
            {
                locaNew.LocaTable[idx] = glyphOffset;
                if (glyphIndex < glyphCount && glyphArray[glyphIndex] == idx)
                {
                    glyphIndex++;
                    byte[] bytes = glyf.GetGlyphData(idx);
                    int length = bytes.Length;
                    if (length > 0)
                    {
                        Buffer.BlockCopy(bytes, 0, glyfNew.GlyphTable, glyphOffset, length);
                        glyphOffset += length;
                    }
                }
            }
            locaNew.LocaTable[numGlyphs] = glyphOffset;

            // Compile font tables into byte array.
            fontData.Compile();

            return fontData;
        }

        /// <summary>
        /// Compiles the font to its binary representation.
        /// </summary>
        void Compile()
        {
            var stream = new MemoryStream();
            var writer = new OpenTypeFontWriter(stream);

            int tableCount = TableDictionary.Count;
            int selector = _entrySelectors[tableCount];

            _offsetTable.Version = 0x00010000;
            _offsetTable.TableCount = tableCount;
            _offsetTable.SearchRange = (ushort)((1 << selector) * 16);
            _offsetTable.EntrySelector = (ushort)selector;
            _offsetTable.RangeShift = (ushort)((tableCount - (1 << selector)) * 16);
            _offsetTable.Write(writer);

            // Sort tables by tag name
            string[] tags = new string[tableCount];
            TableDictionary.Keys.CopyTo(tags, 0);
            Array.Sort(tags, StringComparer.Ordinal);

#if VERBOSE
            Debug.WriteLine("Start Compile");
#endif
            // Write tables in alphabetical order
            int tablePosition = 12 + 16 * tableCount;
            for (int idx = 0; idx < tableCount; idx++)
            {
                var entry = TableDictionary[tags[idx]];
#if DEBUG_
                if (entry.Tag is "glyf" or "loca")
                    _ = typeof(int);
#endif
                entry.FontTable.PrepareForCompilation();
                entry.Offset = tablePosition;
                writer.Position = tablePosition;
                entry.FontTable.Write(writer);
                int endPosition = writer.Position;
                tablePosition = endPosition;
                writer.Position = 12 + 16 * idx;
                entry.Write(writer);
#if VERBOSE
                Debug.WriteLine(String.Format("  Write Table '{0}', offset={1}, length={2}, checksum={3}, ", entry.Tag, entry.Offset, entry.Length, entry.CheckSum));
#endif
            }
#if VERBOSE
            Debug.WriteLine("End Compile");
#endif
            writer.Stream.Flush();
            int l = (int)writer.Stream.Length;
            OTFontSource = OpenTypeFontSource.CreateCompiledFont(stream.ToArray());
        }
        // 2^entrySelector[n] <= n
        static readonly int[] _entrySelectors = [0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4];

        public int Position { get; set; }

        public int Seek(string tag)
        {
            if (TableDictionary.TryGetValue(tag, out var entry))
            {
                Position = entry.Offset;
                return Position;
            }
            return -1;
        }

        public int SeekOffset(int offset) => (Position += offset);

        /// <summary>
        /// Reads a System.Byte.
        /// </summary>
        public byte ReadByte() => OTFontSource.Bytes[Position++];

        /// <summary>
        /// Reads a System.Int16.
        /// </summary>
        public short ReadShort()
        {
            return (short)((OTFontSource.Bytes[Position++] << 8) | (OTFontSource.Bytes[Position++]));
        }

        /// <summary>
        /// Reads a System.UInt16.
        /// </summary>
        public ushort ReadUShort()
        {
            return (ushort)((OTFontSource.Bytes[Position++] << 8) | (OTFontSource.Bytes[Position++]));
        }

        /// <summary>
        /// Reads a System.Int32.
        /// </summary>
        public int ReadLong()
        {
            return (OTFontSource.Bytes[Position++] << 24) | (OTFontSource.Bytes[Position++] << 16) | (OTFontSource.Bytes[Position++] << 8) | OTFontSource.Bytes[Position++];
        }

        /// <summary>
        /// Reads a System.UInt32.
        /// </summary>
        public uint ReadULong()
        {
            return (uint)((OTFontSource.Bytes[Position++] << 24) | (OTFontSource.Bytes[Position++] << 16) | (OTFontSource.Bytes[Position++] << 8) | OTFontSource.Bytes[Position++]);
        }

        /// <summary>
        /// Reads a System.Int32.
        /// </summary>
        public Fixed ReadFixed()
        {
            return (OTFontSource.Bytes[Position++] << 24) | (OTFontSource.Bytes[Position++] << 16) | (OTFontSource.Bytes[Position++] << 8) | OTFontSource.Bytes[Position++];
        }

        /// <summary>
        /// Reads a System.Int16.
        /// </summary>
        public short ReadFWord()
        {
            return (short)((OTFontSource.Bytes[Position++] << 8) | OTFontSource.Bytes[Position++]);
        }

        /// <summary>
        /// Reads a System.UInt16.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public ushort ReadUFWord()
        {
            return (ushort)((OTFontSource.Bytes[Position++] << 8) | OTFontSource.Bytes[Position++]);
        }

        /// <summary>
        /// Reads a System.Int64.
        /// </summary>
        public long ReadLongDate()
        {
            int pos = Position;
            Position += 8;
            byte[] bytes = OTFontSource.Bytes;
            return (((long)bytes[pos]) << 56) | (((long)bytes[pos + 1]) << 48) | (((long)bytes[pos + 2]) << 40) | (((long)bytes[pos + 3]) << 32) |
                   (((long)bytes[pos + 4]) << 24) | (((long)bytes[pos + 5]) << 16) | (((long)bytes[pos + 6]) << 8) | bytes[pos + 7];
        }

        /// <summary>
        /// Reads a System.String with the specified size.
        /// </summary>
        public string ReadString(int size)
        {
            char[] chars = new char[size];
            for (int idx = 0; idx < size; idx++)
                chars[idx] = (char)OTFontSource.Bytes[Position++];
            return new string(chars);
        }

        /// <summary>
        /// Reads a System.Byte[] with the specified size.
        /// </summary>
        public byte[] ReadBytes(int size)
        {
            byte[] bytes = new byte[size];
            for (int idx = 0; idx < size; idx++)
                bytes[idx] = OTFontSource.Bytes[Position++];
            return bytes;
        }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        public void Read(byte[] buffer) => Read(buffer, 0, buffer.Length);

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        public void Read(byte[] buffer, int offset, int length)
        {
            Buffer.BlockCopy(OTFontSource.Bytes, Position, buffer, offset, length);
            Position += length;
        }

        /// <summary>
        /// Reads a System.Char[4] as System.String.
        /// </summary>
        public string ReadTag() => ReadString(4);

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        internal string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
            => String.Format(CultureInfo.InvariantCulture, "OpenType font faces: {0}", _fullFaceName);

        /// <summary>
        /// Represents the font offset table.
        /// </summary>
        internal struct OffsetTable
        {
            /// <summary>
            /// 0x00010000 for Version 1.0.
            /// </summary>
            public uint Version;

            /// <summary>
            /// Number of tables.
            /// </summary>
            public int TableCount;

            /// <summary>
            /// (Maximum power of 2 ≤ numTables) x 16.
            /// </summary>
            public ushort SearchRange;

            /// <summary>
            /// Log2(maximum power of 2 ≤ numTables).
            /// </summary>
            public ushort EntrySelector;

            /// <summary>
            /// NumTables x 16-searchRange.
            /// </summary>
            public ushort RangeShift;

            /// <summary>
            /// Writes the offset table.
            /// </summary>
            public void Write(OpenTypeFontWriter writer)
            {
                writer.WriteUInt(Version);
                writer.WriteShort(TableCount);
                writer.WriteUShort(SearchRange);
                writer.WriteUShort(EntrySelector);
                writer.WriteUShort(RangeShift);
            }
        }
    }
}
