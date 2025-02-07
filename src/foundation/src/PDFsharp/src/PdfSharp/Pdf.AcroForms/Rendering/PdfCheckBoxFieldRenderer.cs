﻿using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Renders a <see cref="PdfCheckBoxField"/><br></br>
    /// </summary>
    /// <remarks>
    /// Inheritors should override the methods <see cref="RenderCheckedState(PdfCheckBoxField, PdfWidgetAnnotation, XGraphics, XRect)"/>
    /// and <see cref="RenderUncheckedState(PdfCheckBoxField, PdfWidgetAnnotation, XGraphics, XRect)"/>
    /// </remarks>
    public class PdfCheckBoxFieldRenderer
    {
        /// <summary>
        /// Renders the ckecked state of the <see cref="PdfCheckBoxField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void RenderCheckedState(PdfCheckBoxField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            // draw border
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
            {
                var borderPen = new XPen(widget.BorderColor, widget.Border.Width);
                gfx.DrawRectangle(borderPen, 0, 0, rect.Width, rect.Height);
            }
            // draw an X-shape
            var pen = new XPen(field.ForeColor, rect.Width * 0.125)
            {
                LineCap = XLineCap.Round
            };
            var pad = widget.Border.Width + 1;
            gfx.DrawLine(pen, 0 + pad, pad, rect.Width - pad, rect.Height - pad);
            gfx.DrawLine(pen, 0 + pad, rect.Height - pad, rect.Width - pad, pad);
        }

        /// <summary>
        /// Renders the unckecked state of the <see cref="PdfCheckBoxField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void RenderUncheckedState(PdfCheckBoxField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            // draw border
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
            {
                var borderPen = new XPen(widget.BorderColor, widget.Border.Width);
                gfx.DrawRectangle(borderPen, 0, 0, rect.Width, rect.Height);
            }
        }
    }
}
