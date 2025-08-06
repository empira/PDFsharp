// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using Fixed = System.Int32;
//using FWord = System.Int16;
//using UFWord = System.UInt16;

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Represents an indirect reference to an existing font table in a font image.
    /// Used to create binary copies of an existing font table that is not modified.
    /// </summary>
    // ReSharper disable once InconsistentNaming because "I" stands for "indirect", not "interface".
    sealed class IRefFontTable : OpenTypeFontTable
    {
        public IRefFontTable(OpenTypeFontFace fontData, OpenTypeFontTable fontTable)
            : base(null, fontTable.DirectoryEntry.Tag)
        {
            _fontData = fontData;
            _irefDirectoryEntry = fontTable.DirectoryEntry;
        }

        readonly TableDirectoryEntry _irefDirectoryEntry;

        /// <summary>
        /// Prepares the font table to be compiled into its binary representation.
        /// </summary>
        public override void PrepareForCompilation()
        {
            base.PrepareForCompilation();
            DirectoryEntry.Length = _irefDirectoryEntry.Length;
            DirectoryEntry.CheckSum = _irefDirectoryEntry.CheckSum;
#if DEBUG
            // Check the checksum algorithm.
            if (DirectoryEntry.Tag != TableTagNames.Head)
            {
                byte[] bytes = new byte[DirectoryEntry.PaddedLength];
                Buffer.BlockCopy(_irefDirectoryEntry.FontTable._fontData!.FontSource.Bytes, _irefDirectoryEntry.Offset, bytes, 0, DirectoryEntry.PaddedLength);
                uint checkSum1 = DirectoryEntry.CheckSum;
                uint checkSum2 = CalcChecksum(bytes);
                // TODO_OLD: Sometimes this Assert fails,
                //Debug.Assert(checkSum1 == checkSum2, "Bug in checksum algorithm.");
            }
#endif
        }

        /// <summary>
        /// Converts the font into its binary representation.
        /// </summary>
        public override void Write(OpenTypeFontWriter writer)
        {
            writer.Write(_irefDirectoryEntry.FontTable._fontData!.FontSource.Bytes, _irefDirectoryEntry.Offset, _irefDirectoryEntry.PaddedLength);
        }
    }
}
