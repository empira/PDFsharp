// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//#define VERBOSE

//using System.Runtime.InteropServices;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
#endif
#if WPF
using System.IO;
//using System.Windows;
//using System.Windows.Documents;
//using System.Windows.Media;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
#endif
//using PdfSharp.Fonts;
#if !EDF_CORE
using PdfSharp.Drawing;
//using PdfSharp.Internal;
#endif

using Fixed = System.Int32;
using FWord = System.Int16;
using UFWord = System.UInt16;

#pragma warning disable 0649

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Represents an OpenType fontface in memory.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    sealed class OpenTypeFontface
    {
        // Implementation Notes
        // OpenTypeFontface represents a 'decompiled' font file in memory.
        //
        // * An OpenTypeFontface can belong to more than one 
        //   XGlyphTypeface because of StyleSimulations.
        //
        // * Currently there is a one to one relationship to XFontSource.
        // 
        // * Consider OpenTypeFontface as an decompiled XFontSource.
        //
        // http://www.microsoft.com/typography/otspec/

        /// <summary>
        /// Shallow copy for font subset.
        /// </summary>
        OpenTypeFontface(OpenTypeFontface fontface)
        {
            _offsetTable = fontface._offsetTable;
            _fullFaceName = fontface._fullFaceName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenTypeFontface"/> class.
        /// </summary>
        public OpenTypeFontface(byte[] data, string faceName)
        {
            _fullFaceName = faceName;
            // Always save a copy of the font bytes.
            int length = data.Length;
            //FontSource = new XFontSource(faceName, new byte[length]);
            Array.Copy(data, FontSource.Bytes, length);
            Read();
        }

        public OpenTypeFontface(XFontSource fontSource)
        {
            FontSource = fontSource;
            Read();
            _fullFaceName = name.FullFontName;
        }

        public static OpenTypeFontface CetOrCreateFrom(XFontSource fontSource)
        {
            if (OpenTypeFontfaceCache.TryGetFontface(fontSource.Key, out var fontface))
                return fontface;

            //  Each font source already contains its OpenTypeFontface.
            Debug.Assert(fontSource.Fontface != null);
            fontface = OpenTypeFontfaceCache.AddFontface(fontSource.Fontface);
            Debug.Assert(ReferenceEquals(fontSource.Fontface, fontface));
            return fontface;
        }

        /// <summary>
        /// Gets the full face name from the name table.
        /// Name is also used as the key.
        /// </summary>
        public string FullFaceName => _fullFaceName;

        readonly string _fullFaceName;

        public ulong CheckSum
        {
            get
            {
                if (_checkSum == 0)
                    _checkSum = FontHelper.CalcChecksum(FontSource.Bytes);
                return _checkSum;
            }
        }
        ulong _checkSum;

        /// <summary>
        /// Gets the bytes that represents the font data.
        /// </summary>
        public XFontSource FontSource
        {
            get => _fontSource ?? NRT.ThrowOnNull<XFontSource>();
            private set =>
                // Stop working if font was not found.
                _fontSource = value ?? throw new InvalidOperationException("Font cannot be resolved.");
        }
        XFontSource? _fontSource;

        internal FontTechnology? _fontTechnology;

        internal OffsetTable _offsetTable;

        /// <summary>
        /// The dictionary of all font tables.
        /// </summary>
        internal Dictionary<string, TableDirectoryEntry> TableDictionary = new();

        // Keep names identical to OpenType spec.
        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        internal CMapTable cmap = default!; // NRT TODO Change programming model so that it fits NRTs.
        internal ColrTable? colr = default;
        internal CpalTable? cpal = default;
        internal ControlValueTable cvt = default!; // NRT
        internal FontProgram fpgm = default!; // NRT
        internal MaximumProfileTable maxp = default!; // NRT
        internal NameTable name = default!; // NRT
        internal ControlValueProgram? prep;
        internal FontHeaderTable? head;
        internal HorizontalHeaderTable hhea = default!; // NRT
        internal HorizontalMetricsTable hmtx = default!; // NRT
        internal OS2Table os2 = default!; // NRT
        internal PostScriptTable post = default!; // NRT
        internal GlyphDataTable glyf = default!; // NRT
        internal IndexToLocationTable loca = default!; // NRT
        internal GlyphSubstitutionTable gsub = default!; // NRT
        internal VerticalHeaderTable? vhea; // TODO
        internal VerticalMetricsTable? vmtx; // TODO
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming

        public bool CanRead => _fontSource != null;

        public bool CanWrite => _fontSource == null;

        /// <summary>
        /// Adds the specified table to this font image.
        /// </summary>
        public void AddTable(OpenTypeFontTable fontTable)
        {
            if (!CanWrite)
                throw new InvalidOperationException("Font image cannot be modified.");

            if (fontTable == null)
                throw new ArgumentNullException(nameof(fontTable));

            if (fontTable._fontData == null)
            {
                fontTable._fontData = this;
            }
            else
            {
                Debug.Assert(fontTable._fontData.CanRead);
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

                    // BUG BUG BUG BUG
                    //Debug.Assert(fpgm != null);
                    break;

                case TableTagNames.MaxP:
                    maxp = (fontTable as MaximumProfileTable)!; // ?? NRT.ThrowOnNull<MaximumProfileTable>();
                    if (maxp != null!)
                    {
                        // It is never not null :-(
                        ((Action)(() => { }))();
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
                    Debug-Break.Break();
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
                Debug.Assert(_pos == 12);
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

                // PDFlib checks this, but it is not part of the OpenType spec anymore
                if (TableDictionary.ContainsKey("bhed"))
                    throw new NotSupportedException("Bitmap fonts are not supported by PDFsharp.");

                // Read required tables
                if (Seek(CMapTable.Tag) != -1)
                    cmap = new CMapTable(this);

                if (Seek(ColrTable.Tag) != -1)
                    colr = new ColrTable(this);

                if (Seek(CpalTable.Tag) != -1)
                    cpal = new CpalTable(this);

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
            }
            catch (Exception)
            {
                ((Action)(() => { }))();
                throw;
            }
        }

        /// <summary>
        /// Creates a new font image that is a subset of this font image containing only the specified glyphs.
        /// </summary>
        public OpenTypeFontface CreateFontSubSet(Dictionary<uint, object> glyphs, bool cidFont)
        {
            // Create new font image
            var fontData = new OpenTypeFontface(this);

            // Create new loca and glyf table
            var locaNew = new IndexToLocationTable();
            locaNew.ShortIndex = loca.ShortIndex;
            var glyfNew = new GlyphDataTable();

            // Add all required tables
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
            Debug.Assert(glyphs != null);
            glyf.CompleteGlyphClosure(glyphs!);

            // Create a sorted array of all used glyphs.
            int glyphCount = glyphs.Count;
            uint[] glyphArray = new uint[glyphCount];
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
            for (uint idx = 0; idx < numGlyphs; idx++)
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
#if DEBUG
                if (entry.Tag == "glyf" || entry.Tag == "loca")
                    ((Action)(() => { }))();
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
            FontSource = XFontSource.CreateCompiledFont(stream.ToArray());
        }
        // 2^entrySelector[n] <= n
        static readonly int[] _entrySelectors = { 0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

        public int Position
        {
            get => _pos;
            set => _pos = value;
        }
        int _pos;

        public int Seek(string tag)
        {
            if (TableDictionary.ContainsKey(tag))
            {
                _pos = TableDictionary[tag].Offset;
                return _pos;
            }
            return -1;
        }

        public int SeekOffset(int offset)
        {
            _pos += offset;
            return _pos;
        }

        /// <summary>
        /// Reads a System.Byte.
        /// </summary>
        public byte ReadByte()
        {
            return FontSource.Bytes[_pos++];
        }

        /// <summary>
        /// Reads a System.Int16.
        /// </summary>
        public short ReadShort()
        {
            int pos = _pos;
            _pos += 2;
            return (short)((FontSource.Bytes[pos] << 8) | (FontSource.Bytes[pos + 1]));
        }

        /// <summary>
        /// Reads a System.UInt16.
        /// </summary>
        public ushort ReadUShort()
        {
            int pos = _pos;
            _pos += 2;
            return (ushort)((FontSource.Bytes[pos] << 8) | (FontSource.Bytes[pos + 1]));
        }

        /// <summary>
        /// Reads a System.Int32.
        /// </summary>
        public int ReadLong()
        {
            int pos = _pos;
            _pos += 4;
            return (FontSource.Bytes[pos] << 24) | (FontSource.Bytes[pos + 1] << 16) | (FontSource.Bytes[pos + 2] << 8) | FontSource.Bytes[pos + 3];
        }

        /// <summary>
        /// Reads a System.UInt32.
        /// </summary>
        public uint ReadULong()
        {
            int pos = _pos;
            _pos += 4;
            return (uint)((FontSource.Bytes[pos] << 24) | (FontSource.Bytes[pos + 1] << 16) | (FontSource.Bytes[pos + 2] << 8) | FontSource.Bytes[pos + 3]);
        }

        /// <summary>
        /// Reads a System.Int32.
        /// </summary>
        public Fixed ReadFixed()
        {
            int pos = _pos;
            _pos += 4;
            return (FontSource.Bytes[pos] << 24) | (FontSource.Bytes[pos + 1] << 16) | (FontSource.Bytes[pos + 2] << 8) | FontSource.Bytes[pos + 3];
        }

        /// <summary>
        /// Reads a System.Int16.
        /// </summary>
        public short ReadFWord()
        {
            int pos = _pos;
            _pos += 2;
            return (short)((FontSource.Bytes[pos] << 8) | FontSource.Bytes[pos + 1]);
        }

        /// <summary>
        /// Reads a System.UInt16.
        /// </summary>
        public ushort ReadUFWord()
        {
            int pos = _pos;
            _pos += 2;
            return (ushort)((FontSource.Bytes[pos] << 8) | FontSource.Bytes[pos + 1]);
        }

        /// <summary>
        /// Reads a System.Int64.
        /// </summary>
        public long ReadLongDate()
        {
            int pos = _pos;
            _pos += 8;
            byte[] bytes = FontSource.Bytes;
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
                chars[idx] = (char)FontSource.Bytes[_pos++];
            return new string(chars);
        }

        /// <summary>
        /// Reads a System.Byte[] with the specified size.
        /// </summary>
        public byte[] ReadBytes(int size)
        {
            byte[] bytes = new byte[size];
            for (int idx = 0; idx < size; idx++)
                bytes[idx] = FontSource.Bytes[_pos++];
            return bytes;
        }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        public void Read(byte[] buffer)
        {
            Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        public void Read(byte[] buffer, int offset, int length)
        {
            Buffer.BlockCopy(FontSource.Bytes, _pos, buffer, offset, length);
            _pos += length;
        }

        /// <summary>
        /// Reads a System.Char[4] as System.String.
        /// </summary>
        public string ReadTag()
        {
            return ReadString(4);
        }

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
