// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Internals;

using BorderStyle = MigraDoc.DocumentObjectModel.BorderStyle;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Row to RTF.
    /// </summary>
    class RowsRenderer : RendererBase
    {
        internal RowsRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _rows = (Rows)domObj;
        }

        internal override void Render()
        {
            Translate("Alignment", "trq");
            _rtfWriter.WriteControl("trleft", ToTwips(CalculateLeftIndent(_rows)));
        }

        internal static Unit CalculateLeftIndent(Rows rows)
        {
            var leftInd = rows.Values.LeftIndent;
            if (leftInd == null)
            {
                leftInd = rows.Table!.Columns[0].Values.LeftPadding; // Re "!": Table can be null for standalone objects, but not for objects in documents.
                if (leftInd == null)
                    leftInd = Unit.FromCentimeter(-0.12);
                else
                    leftInd = Unit.FromPoint(-((Unit)leftInd));

                Cell cell = rows[0].Cells[0];

                var visible = cell.Values.Borders?.Values.Left?.Values.Visible;
                var lineWidth = cell.Values.Borders?.Values.Left?.Values.Width;

                var style = cell.Values.Borders?.Values.Left?.Values.Style;
                var color = cell.Values.Borders?.Values.Left?.Values.Color;

                if (visible == null || (bool)visible)
                {
                    if (lineWidth != null || style != null || color != null)
                    {
                        if (style != null && (BorderStyle)style != BorderStyle.None)
                        {
                            if (lineWidth != null)
                                leftInd = Unit.FromPoint(((Unit)leftInd).Point - ((Unit)lineWidth).Point);
                            else
                                leftInd = Unit.FromPoint(((Unit)leftInd).Point - 0.5);
                        }
                    }
                }
            }
            return (Unit)leftInd;
        }

        readonly Rows _rows;
    }
}
