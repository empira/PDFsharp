// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Row to RTF.
    /// </summary>
    class RowRenderer : RendererBase
    {
        internal RowRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _row = (Row)domObj;
        }

        /// <summary>
        /// Render a Row to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;
            _rtfWriter.WriteControl("trowd");
            new RowsRenderer((DocumentRelations.GetParent(_row) as Rows)!, _docRenderer).Render();
            RenderRowHeight();
            //MigraDoc always keeps together table rows.
            _rtfWriter.WriteControl("trkeep");
            Translate("HeadingFormat", "trhdr");

            // trkeepfollow is intended to keep table rows together.
            // Unfortunately, this does not work in word.
            int thisRowIdx = _row.Index;
            for (int rowIdx = 0; rowIdx <= _row.Index; ++rowIdx)
            {
                var keepWith = _row.Table!.Rows[rowIdx].Values.KeepWith;
                if (keepWith != null && (int)keepWith + rowIdx > thisRowIdx)
                    _rtfWriter.WriteControl("trkeepfollow");
            }
            RenderTopBottomPadding();

            //Cell borders etc. are written before the contents.
            for (int idx = 0; idx < _row.Table!.Columns.Count; ++idx)
            {
                Cell cell = _row.Cells[idx];
                CellFormatRenderer cellFrmtRenderer =
                new CellFormatRenderer(cell, _docRenderer);
                cellFrmtRenderer.CellList = _cellList;
                cellFrmtRenderer.Render();
            }
            foreach (var cell in _row.Cells)
            {
                Debug.Assert(cell != null, nameof(cell) + " != null");
                CellRenderer cellRndrr = new(cell, _docRenderer);
                cellRndrr.CellList = _cellList;
                cellRndrr.Render();
            }

            _rtfWriter.WriteControl("row");
        }

        void RenderTopBottomPadding()
        {
            string rwPadCtrl = "trpadd";
            string rwPadUnit = "trpaddf";
            var rwPdgVal = _row.Values.TopPadding;
            if (rwPdgVal == null)
                rwPdgVal = Unit.FromCentimeter(0);

            //Word bug: Top and leftpadding are being confused in word.
            _rtfWriter.WriteControl(rwPadCtrl + "t", ToRtfUnit((Unit)rwPdgVal, RtfUnit.Twips));
            // Tells the RTF reader to take it as Twips.
            _rtfWriter.WriteControl(rwPadUnit + "t", 3);
            rwPdgVal = _row.Values.BottomPadding ?? Unit.FromCentimeter(0);

            _rtfWriter.WriteControl(rwPadCtrl + "b", ToRtfUnit((Unit)rwPdgVal, RtfUnit.Twips));
            _rtfWriter.WriteControl(rwPadUnit + "b", 3);
        }

        void RenderRowHeight()
        {
            var heightObj = GetValueAsIntended("Height");
            var heightRlObj = GetValueAsIntended("HeightRule");
            if (heightRlObj != null)
            {
                switch ((RowHeightRule)heightRlObj)
                {
                    case RowHeightRule.AtLeast:
                        Translate("Height", "trrh", RtfUnit.Twips, "0", false);
                        break;
                    case RowHeightRule.Auto:
                        _rtfWriter.WriteControl("trrh", 0);
                        break;

                    case RowHeightRule.Exactly:
                        if (heightObj != null)
                            RenderUnit("trrh", -((Unit)heightObj).Point);
                        break;
                }
            }
            else
                Translate("Height", "trrh", RtfUnit.Twips, "0", false); //treat it like "AtLeast".
        }

        /// <summary>
        /// Sets the merged cell list. This property is set by the table renderer.
        /// </summary>
        internal MergedCellList CellList
        {
            set => _cellList = value;
        }
        MergedCellList _cellList = null!;

        readonly Row _row;
    }
}
