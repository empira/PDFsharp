// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using Fixed = System.Int32;
//using FWord = System.Int16;
//using UFWord = System.UInt16;

namespace PdfSharp.Fonts.OpenType
{
#if true_
    /// <summary>
    /// Generic font table. Not yet used
    /// </summary>
    internal class GenericFontTable : OpenTypeFontTable
    {
        public GenericFontTable(OpenTypeFontTable fontTable)
          : base(null, "xxxx")
        {
            DirectoryEntry.Tag = fontTable.DirectoryEntry.Tag;
            int length = fontTable.DirectoryEntry.Length;
            if (length > 0)
            {
                _table = new byte[length];
                Buffer.BlockCopy(fontTable.FontData.Data, fontTable.DirectoryEntry.Offset, _table, 0, length);
            }
        }

        public GenericFontTable(OpenTypeFontface fontData, string tag)
          : base(fontData, tag)
        {
            _fontData = fontData;
        }

        protected override OpenTypeFontTable DeepCopy()
        {
            GenericFontTable fontTable = (GenericFontTable)base.DeepCopy();
            fontTable._table = (byte[])_table.Clone();
            return fontTable;
        }

        byte[] _table;
    }
#endif
}
