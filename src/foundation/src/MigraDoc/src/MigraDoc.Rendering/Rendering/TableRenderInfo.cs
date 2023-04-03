// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Rendering information for tables.
    /// </summary>
    public class TableRenderInfo : RenderInfo
    {
        internal TableRenderInfo()
        { }

        /// <summary>
        /// Gets the format information in a specific derived type. For a table, for example, this will be a TableFormatInfo with information about the first and last row showing on a page.
        /// </summary>
        public override FormatInfo FormatInfo
        {
            get => _formatInfo;
            internal set => _formatInfo = (TableFormatInfo)value;
        }
        TableFormatInfo _formatInfo = new();

        /// <summary>
        /// Gets the document object to which the layout information applies. Use the Tag property of DocumentObject to identify an object.
        /// </summary>
        public override DocumentObject DocumentObject
        {
            get => _table ?? NRT.ThrowOnNull<Table>();
            internal set => _table = (Table)value;
        }
        Table? _table;
    }
}
