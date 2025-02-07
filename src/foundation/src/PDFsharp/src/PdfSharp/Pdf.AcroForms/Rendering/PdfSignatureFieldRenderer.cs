﻿using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Renders a <see cref="PdfSignatureField"/><br></br>
    /// </summary>
    /// <remarks>
    /// Inheritors should override the method <see cref="Render(PdfSignatureField, PdfWidgetAnnotation, XGraphics, XRect)"/>
    /// </remarks>
    public class PdfSignatureFieldRenderer
    {
        /// <summary>
        /// Renders the <see cref="PdfSignatureField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void Render(PdfSignatureField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            const double pad = 4.0;

            if (!widget.BackColor.IsEmpty)
                gfx.DrawRectangle(new XSolidBrush(widget.BackColor), rect);
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
            {
                gfx.DrawRectangle(new XPen(widget.BorderColor, widget.Border.Width), rect);
                rect.Inflate(-widget.Border.Width, -widget.Border.Width);
            }

            // render a horizontal line where a (handwritten) signature may be placed on
            var linePen = new XPen(XColors.Black, 1.0);
            gfx.DrawLine(linePen, rect.Left + pad, rect.Bottom - pad, rect.Right - pad, rect.Bottom - pad);
        }
    }
}
