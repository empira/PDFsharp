// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a cell to RTF.
    /// </summary>
    class CellRenderer : StyleAndFormatRenderer
    {
        internal CellRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _cell = (Cell)domObj;
        }

        internal override void Render()
        {
            _useEffectiveValue = true;
            Debug.Assert(_cell != null, nameof(_cell) + " != null");
            var cvrgCell = _cellList.GetCoveringCell(_cell);
            Debug.Assert(cvrgCell != null, nameof(cvrgCell) + " != null");
            if (_cell!.Column!.Index != cvrgCell!.Column!.Index)
                return;

            bool writtenAnyContent = false;
            if (_cell.Values.Elements is not null && !_cell.Values.Elements.IsNull())
            {
                if (_cell == cvrgCell)
                {
                    foreach (var docObj in _cell.Elements)
                    {
                        RendererBase rndrr = RendererFactory.CreateRenderer(docObj!, _docRenderer);
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                        if (rndrr != null)
                        {
                            rndrr.Render();
                            writtenAnyContent = true;
                        }
                    }
                }
            }
            if (!writtenAnyContent)
            {
                //Format attributes need to be set here to satisfy Word 2000.
                _rtfWriter.WriteControl("pard");
                RenderStyleAndFormat();
                _rtfWriter.WriteControl("intbl");
                EndStyleAndFormatAfterContent();
            }
            _rtfWriter.WriteControl("cell");
        }

        internal MergedCellList CellList
        {
            set
            {
                _cellList = value;
            }
        }
        MergedCellList _cellList = null!;

        readonly Cell _cell;
    }
}
