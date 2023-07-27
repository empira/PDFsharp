// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

using BorderStyle = MigraDoc.DocumentObjectModel.BorderStyle;
using Color = MigraDoc.DocumentObjectModel.Color;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Base class for BorderRenderer and BordersRenderer 
    /// Useful because the BordersRenderer needs to draw single borders too.
    /// </summary>
    abstract class BorderRendererBase : RendererBase
    {
        internal BorderRendererBase(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        { }

        /// <summary>
        /// Renders a single border that might have a parent of type Borders or Border.
        /// </summary>
        protected void RenderBorder(string borderControl)
        {
            var visible = GetValueAsIntended("Visible");
            var borderStyle = GetValueAsIntended("Style");
            var borderColor = GetValueAsIntended("Color");
            var borderWidth = GetValueAsIntended("Width");
            // REM: Necessary for invisible borders?
            _rtfWriter.WriteControl(borderControl);

            if (visible == null && borderStyle == null && borderColor == null && borderWidth == null)
                return;

            // A border of width 0 would be displayed in Word:
            if (borderWidth != null && ((Unit)borderWidth) == 0)
                return;

            if ((visible != null && !(bool)visible) ||
              (borderStyle != null && ((BorderStyle)borderStyle) == BorderStyle.None))
                return;

            //Caution: Write width AFTER style to satisfy word.
            Translate("Style", "brdr", RtfUnit.Undefined, "s", false);
            Translate("Width", "brdrw", RtfUnit.Twips, "10", false);
            Translate("Color", "brdrcf", RtfUnit.Undefined, _docRenderer.GetColorIndex(GetDefaultColor()).ToString(), false);
        }

        /// <summary>
        /// Gets the default color of the Border.
        /// Paragraph Borders use the font color by default.
        /// </summary>
        Color GetDefaultColor()
        {
            if (_parentFormat != null)
            {
                var clr = _parentFormat.Values.Font?.Values.Color;
                if (clr != null)
                    return (Color)clr;
            }
            return Colors.Black;
        }

        /// <summary>
        /// Gets the RTF border control for the given border type.
        /// </summary>
        protected string GetBorderControl(BorderType type)
        {
            string borderCtrl;
            bool isCellBorder = _parentCell != null;
            if (isCellBorder)
                borderCtrl = "clbrdr";
            else
                borderCtrl = "brdr";
            switch (type)
            {
                case BorderType.Top:
                    borderCtrl += "t";
                    break;

                case BorderType.Bottom:
                    borderCtrl += "b";
                    break;

                case BorderType.Left:
                    borderCtrl += "l";
                    break;

                case BorderType.Right:
                    borderCtrl += "r";
                    break;

                case BorderType.DiagonalDown:
                    Debug.Assert(isCellBorder);
                    borderCtrl = "cldglu";
                    break;

                case BorderType.DiagonalUp:
                    Debug.Assert(isCellBorder);
                    borderCtrl = "cldgll";
                    break;
            }
            return borderCtrl;
        }

        /// <summary>
        /// Sets the paragraph format the Border is part of.
        /// This property is set by the ParagraphFormatRenderer
        /// </summary>
        internal ParagraphFormat? ParentFormat
        {
            set
            {
                _parentFormat = value;
                if (value != null)
                    _parentCell = null;
            }
        }
        protected ParagraphFormat? _parentFormat;

        /// <summary>
        /// Sets the cell the border is part of.
        /// This property is set by the CellFormatRenderer
        /// </summary>
        internal Cell? ParentCell
        {
            set
            {
                _parentCell = value;
                if (value != null)
                    _parentFormat = null;
            }
        }
        protected Cell? _parentCell;
    }
}
