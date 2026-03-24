// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Internal.OpenType
{
    /// <summary>
    /// Base class for all OpenType tables used in PDFsharp.
    /// </summary>
    public class OpenTypeFontTable : ICloneable
    {
        protected OpenTypeFontTable(OpenTypeFontFace? fontData, string tag)
        {
            FontData = fontData;
            if (fontData != null && fontData.TableDictionary.ContainsKey(tag))
                DirectoryEntry = fontData.TableDictionary[tag];
            else
                DirectoryEntry = new TableDirectoryEntry(tag);
            DirectoryEntry.FontTable = this;
        }

        /// <summary>
        /// Creates a deep copy of the current instance.
        /// </summary>
        public object Clone() => DeepCopy();

        protected virtual OpenTypeFontTable DeepCopy()
        {
            var fontTable = (OpenTypeFontTable)MemberwiseClone();
            fontTable.DirectoryEntry.Offset = 0;
            fontTable.DirectoryEntry.FontTable = fontTable;
            return fontTable;
        }

        /// <summary>
        /// Gets the font image the table belongs to.
        /// </summary>
        public OpenTypeFontFace? FontData
#if DEBUG__
        { get; set; }
#endif
            = null; //  => FontData;

        //internal OpenTypeFontFace? FontData = null!;

        public TableDirectoryEntry DirectoryEntry;

        /// <summary>
        /// When overridden in a derived class, prepares the font table to be compiled into its binary representation.
        /// </summary>
        public virtual void PrepareForCompilation()
        { }

        /// <summary>
        /// When overridden in a derived class, converts the font into its binary representation.
        /// </summary>
        public virtual void Write(OpenTypeFontWriter writer)
        { }

        /// <summary>
        /// Calculates the checksum of a table represented by its bytes.
        /// </summary>
        protected static uint CalcChecksum(byte[] bytes)
        {
            Debug.Assert((bytes.Length & 3) == 0);
            // Cannot use Buffer.BlockCopy because 32-bit values are Big-endian in fonts.
            uint byte3 = 0, byte2 = 0, byte1 = 0, byte0 = 0;
            int length = bytes.Length;
            for (int idx = 0; idx < length;)
            {
                byte3 += bytes[idx++];
                byte2 += bytes[idx++];
                byte1 += bytes[idx++];
                byte0 += bytes[idx++];
            }
            return (byte3 << 24) + (byte2 << 16) + (byte1 << 8) + byte0;
        }
    }
}
