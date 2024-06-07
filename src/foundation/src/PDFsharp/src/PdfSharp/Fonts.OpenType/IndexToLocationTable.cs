// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#define VERBOSE_

//using Fixed = System.Int32;
//using FWord = System.Int16;
//using UFWord = System.UInt16;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// The indexToLoc table stores the offsets to the locations of the glyphs in the font,
    /// relative to the beginning of the glyphData table. In order to compute the length of
    /// the last glyph element, there is an extra entry after the last valid index. 
    /// </summary>
    class IndexToLocationTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Loca;

        internal int[] LocaTable = null!;

        public IndexToLocationTable()
            : base(null, Tag)
        {
            DirectoryEntry.Tag = TableTagNames.Loca;
        }

        public IndexToLocationTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            DirectoryEntry = _fontData!.TableDictionary[TableTagNames.Loca];
            Read();
        }

        public bool ShortIndex;

        /// <summary>
        /// Converts the bytes in a handy representation.
        /// </summary>
        public void Read()
        {
            try
            {
                ShortIndex = _fontData!.head!.indexToLocFormat == 0;
                _fontData.Position = DirectoryEntry.Offset;
                if (ShortIndex)
                {
                    int entries = DirectoryEntry.Length / 2;
                    Debug.Assert(_fontData.maxp.numGlyphs + 1 == entries,
                        "For your information only: Number of glyphs mismatch in font. You can ignore this assertion.");
                    LocaTable = new int[entries];
                    for (int idx = 0; idx < entries; idx++)
                        LocaTable[idx] = 2 * _fontData.ReadUFWord();
                }
                else
                {
                    int entries = DirectoryEntry.Length / 4;
                    Debug.Assert(_fontData.maxp.numGlyphs + 1 == entries,
                        "For your information only: Number of glyphs mismatch in font. You can ignore this assertion.");
                    LocaTable = new int[entries];
                    for (int idx = 0; idx < entries; idx++)
                        LocaTable[idx] = _fontData.ReadLong();
                }
            }
            catch (Exception)
            {
                _ = typeof(int);
                throw;
            }
        }

        /// <summary>
        /// Prepares the font table to be compiled into its binary representation.
        /// </summary>
        public override void PrepareForCompilation()
        {
            DirectoryEntry.Offset = 0;
            if (ShortIndex)
                DirectoryEntry.Length = LocaTable.Length * 2;
            else
                DirectoryEntry.Length = LocaTable.Length * 4;

            _bytes = new byte[DirectoryEntry.PaddedLength];
            int length = LocaTable.Length;
            int byteIdx = 0;
            if (ShortIndex)
            {
                for (int idx = 0; idx < length; idx++)
                {
                    int value = LocaTable[idx] / 2;
                    _bytes[byteIdx++] = (byte)(value >> 8);
                    _bytes[byteIdx++] = (byte)(value);
                }
            }
            else
            {
                for (int idx = 0; idx < length; idx++)
                {
                    int value = LocaTable[idx];
                    _bytes[byteIdx++] = (byte)(value >> 24);
                    _bytes[byteIdx++] = (byte)(value >> 16);
                    _bytes[byteIdx++] = (byte)(value >> 8);
                    _bytes[byteIdx++] = (byte)value;
                }
            }
            DirectoryEntry.CheckSum = CalcChecksum(_bytes);
        }

        byte[] _bytes = default!;

        /// <summary>
        /// Converts the font into its binary representation.
        /// </summary>
        public override void Write(OpenTypeFontWriter writer)
        {
            writer.Write(_bytes, 0, DirectoryEntry.PaddedLength);
        }
    }
}
