// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Table to RTF.
    /// </summary>
    class TableRenderer : RendererBase
    {
        internal TableRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _table = (Table)domObj;
            Debug.Assert(_table != null);
        }

        /// <summary>
        /// Renders a Table to RTF.
        /// </summary>
        internal override void Render()
        {
            var elms = DocumentRelations.GetParent(_table) as DocumentElements;
            MergedCellList mrgdCellList = new MergedCellList(_table);

            foreach (var row in _table.Rows)
            {
                Debug.Assert(row != null, nameof(row) + " != null");
                RowRenderer rowRenderer = new(row, _docRenderer)
                {
                    CellList = mrgdCellList
                };
                rowRenderer.Render();
            }
        }

        readonly Table _table;
    }
}
