// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Represents an entry in the fonts table dictionary.
    /// </summary>
    class TableDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableDirectoryEntry"/> class.
        /// </summary>
        public TableDirectoryEntry()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableDirectoryEntry"/> class.
        /// </summary>
        public TableDirectoryEntry(string tag)
        {
            Debug.Assert(tag.Length == 4);
            Tag = tag;
        }

        /// <summary>
        /// 4 -byte identifier.
        /// </summary>
        public string Tag = null!;

        /// <summary>
        /// CheckSum for this table.
        /// </summary>
        public uint CheckSum;

        /// <summary>
        /// Offset from beginning of TrueType font file.
        /// </summary>
        public int Offset;

        /// <summary>
        /// Actual length of this table in bytes.
        /// </summary>
        public int Length;

        /// <summary>
        /// Gets the length rounded up to a multiple of four bytes.
        /// </summary>
        public int PaddedLength => (Length + 3) & ~3;

        /// <summary>
        /// Associated font table.
        /// </summary>
        public OpenTypeFontTable FontTable = null!;

        /// <summary>
        /// Creates and reads a TableDirectoryEntry from the font image.
        /// </summary>
        public static TableDirectoryEntry ReadFrom(OpenTypeFontface fontData)
        {
            var entry = new TableDirectoryEntry
            {
                Tag = fontData.ReadTag(),
                CheckSum = fontData.ReadULong(),
                Offset = fontData.ReadLong(),
                Length = (int)fontData.ReadULong()
            };
            return entry;
        }

        public void Read(OpenTypeFontface fontData)
        {
            Tag = fontData.ReadTag();
            CheckSum = fontData.ReadULong();
            Offset = fontData.ReadLong();
            Length = (int)fontData.ReadULong();
        }

        public void Write(OpenTypeFontWriter writer)
        {
            Debug.Assert(Tag.Length == 4);
            Debug.Assert(Offset != 0);
            Debug.Assert(Length != 0);
            writer.WriteTag(Tag);
            writer.WriteUInt(CheckSum);
            writer.WriteInt(Offset);
            writer.WriteUInt((uint)Length);
        }
    }
}
