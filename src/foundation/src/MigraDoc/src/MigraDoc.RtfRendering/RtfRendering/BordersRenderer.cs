// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to render a Borders object to RTF.
    /// </summary>
    class BordersRenderer : BorderRendererBase
    {
        internal BordersRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _borders = (Borders)domObj;
        }

        /// <summary>
        /// Renders a Borders object to RTF.
        /// </summary>
        internal override void Render()
        {
            _useEffectiveValue = true;

            var visible = GetValueAsIntended("Visible");
            var borderStyle = GetValueAsIntended("Style");
            var borderColor = GetValueAsIntended("Color");
            bool isCellBorders = (_parentCell != null);
            bool isFormatBorders = (_parentFormat != null);

            object? obj;
            if (LeaveAwayTop)
                _rtfWriter.WriteControl(GetBorderControl(BorderType.Top));
            else
            {
                obj = GetValueAsIntended("Top");
                if (obj != null)
                {
                    CreateBorderRenderer((Border)obj, BorderType.Top).Render();
                }
                else
                    RenderBorder(GetBorderControl(BorderType.Top));
            }
            if (!isCellBorders)
                Translate("DistanceFromTop", "brsp");
            //REVIEW: Andere Renderer verfahren glaube ich genauso, da ja sonst doppelt gemoppelt

            if (LeaveAwayLeft)
                _rtfWriter.WriteControl(GetBorderControl(BorderType.Top));
            else
            {
                obj = GetValueAsIntended("Left");
                if (obj != null)
                    CreateBorderRenderer((Border)obj, BorderType.Left).Render();
                else
                    RenderBorder(GetBorderControl(BorderType.Left));
            }
            if (!isCellBorders)
                Translate("DistanceFromLeft", "brsp");

            if (LeaveAwayRight)
                _rtfWriter.WriteControl(GetBorderControl(BorderType.Right));
            else
            {
                obj = GetValueAsIntended("Right");
                if (obj != null)
                    CreateBorderRenderer((Border)obj, BorderType.Right).Render();
                else
                    RenderBorder(GetBorderControl(BorderType.Right));
            }
            if (!isCellBorders)
                Translate("DistanceFromRight", "brsp");

            if (LeaveAwayBottom)
                _rtfWriter.WriteControl(GetBorderControl(BorderType.Bottom));
            else
            {
                obj = GetValueAsIntended("Bottom");
                if (obj != null)
                    CreateBorderRenderer((Border)obj, BorderType.Bottom).Render();
                else
                    RenderBorder(GetBorderControl(BorderType.Bottom));
            }
            // All renderers ignore DistanceFrom for table cells.
            // This would lead to problems with Word. (Padding is the substitute.)
            if (!isCellBorders)
                Translate("DistanceFromBottom", "brsp");

            if (isCellBorders)
            {
                obj = GetValueAsIntended("DiagonalDown");
                if (obj != null)
                    CreateBorderRenderer((Border)obj, BorderType.DiagonalDown).Render();

                obj = GetValueAsIntended("DiagonalUp");
                if (obj != null)
                    CreateBorderRenderer((Border)obj, BorderType.DiagonalUp).Render();
            }
        }

        BorderRenderer CreateBorderRenderer(Border border, BorderType borderType)
        {
            BorderRenderer brdrRndrr = new BorderRenderer(border, _docRenderer);
            brdrRndrr.ParentCell = _parentCell;
            brdrRndrr.ParentFormat = _parentFormat;
            brdrRndrr.BorderType = borderType;
            return brdrRndrr;
        }

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        Borders _borders;

        internal bool LeaveAwayTop;
        internal bool LeaveAwayLeft;
        internal bool LeaveAwayRight;
        internal bool LeaveAwayBottom;
    }
}
