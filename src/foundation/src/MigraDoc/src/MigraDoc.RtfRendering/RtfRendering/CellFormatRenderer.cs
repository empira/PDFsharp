// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Internals;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Render the format information of a cell.
    /// </summary>
    class CellFormatRenderer : RendererBase
    {
        internal CellFormatRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _cell = (Cell)domObj;
        }

        /// <summary>
        /// Renders the cell's shading, borders and so on (used by the RowRenderer).
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;
            _coveringCell = _cellList.GetCoveringCell(_cell)!;
            var borders = _cellList.GetEffectiveBordersRtf(_coveringCell);
            if (_cell.Column!.Index != _coveringCell.Column!.Index) // The "!" are needed here as some properties may be null if DOM objects are not added to a document.
                return;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (borders != null)
            {
                BordersRenderer brdrsRenderer = new(borders, _docRenderer)
                {
                    LeaveAwayLeft = _cell.Column.Index != _coveringCell.Column.Index,
                    LeaveAwayTop = _cell.Row!.Index != _coveringCell.Row!.Index,
                    LeaveAwayBottom = _cell.Row.Index != _coveringCell.Row.Index + _coveringCell.MergeDown,
                    LeaveAwayRight = false,
                    ParentCell = _cell
                };
                brdrsRenderer.Render();
            }
            if (_cell == _coveringCell)
            {
                RenderLeftRightPadding();
                Translate("VerticalAlignment", "clvertal");
            }
            var obj = _coveringCell.Values.Shading;
            if (obj != null)
                new ShadingRenderer((DocumentObject)obj, _docRenderer).Render();

            //Note that vertical and horizontal merging are not symmetrical.
            //Horizontally merged cells are simply rendered as bigger cells.
            if (_cell.Row!.Index == _coveringCell.Row!.Index && _coveringCell.MergeDown > 0)
                _rtfWriter.WriteControl("clvmgf");

            if (_cell.Row.Index > _coveringCell.Row.Index)
                _rtfWriter.WriteControl("clvmrg");

            _rtfWriter.WriteControl("cellx", GetRightCellBoundary());
        }

        void RenderLeftRightPadding()
        {
            string clPadCtrl = "clpad";
            string cellPadUnit = "clpadf";
            var cellPdgVal = _cell.Column!.Values.LeftPadding;
            if (cellPdgVal == null)
                cellPdgVal = Unit.FromCentimeter(0.12);

            //Top and left padding are mixed up in word:
            _rtfWriter.WriteControl(clPadCtrl + "t", ToRtfUnit((Unit)cellPdgVal, RtfUnit.Twips));
            //Tells the RTF reader to take it as twips:
            _rtfWriter.WriteControl(cellPadUnit + "t", 3);
            cellPdgVal = _cell.Column.Values.RightPadding;
            if (cellPdgVal == null)
                cellPdgVal = Unit.FromCentimeter(0.12);

            _rtfWriter.WriteControl(clPadCtrl + "r", ToRtfUnit((Unit)cellPdgVal, RtfUnit.Twips));
            //Tells the RTF reader to take it as Twips:
            _rtfWriter.WriteControl(cellPadUnit + "r", 3);
        }

        /// <summary>
        /// Gets the right boundary of the cell which is currently rendered.
        /// </summary>
        int GetRightCellBoundary()
        {
            int rightClmIdx = _coveringCell.Column!.Index + _coveringCell.MergeRight;
            double width = RowsRenderer.CalculateLeftIndent(_cell.Table!.Rows).Point;
            for (int idx = 0; idx <= rightClmIdx; ++idx)
            {
                var obj = _cell.Table.Columns[idx].Values.Width;
                if (obj != null)
                    width += ((Unit)obj).Point;
                else
                    width += ((Unit)"2.5cm").Point;
            }
            return ToRtfUnit(new Unit((double)width), RtfUnit.Twips);
        }

        /// <summary>
        /// Sets the MergedCellList received from the DOM table. This property is set by the RowRenderer.
        /// </summary>
        internal MergedCellList CellList
        {
            set { _cellList = value; }
        }

        MergedCellList _cellList = null!;
        Cell _coveringCell = null!;
        readonly Cell _cell;
    }
}