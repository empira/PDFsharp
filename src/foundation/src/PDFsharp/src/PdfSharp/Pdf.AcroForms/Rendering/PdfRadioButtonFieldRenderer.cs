﻿using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Renders a <see cref="PdfRadioButtonField"/><br></br>
    /// </summary>
    /// <remarks>
    /// Inheritors should override the methods
    /// <see cref="RenderCheckedState(PdfRadioButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/> and
    /// <see cref="RenderUncheckedState(PdfRadioButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/>
    /// </remarks>
    public class PdfRadioButtonFieldRenderer
    {
        /// <summary>
        /// Renders the ckecked state of the <see cref="PdfRadioButtonField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void RenderCheckedState(PdfRadioButtonField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            // draw border
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
            {
                var borderPen = new XPen(widget.BorderColor, widget.Border.Width);
                gfx.DrawEllipse(borderPen, 0, 0, rect.Width, rect.Height);
            }
            // draw a dot in the middle
            var dotRect = new XRect(rect.Location, rect.Size);
            dotRect.Inflate(-rect.Width * 0.25, -rect.Height * 0.25);
            gfx.DrawEllipse(new XSolidBrush(field.ForeColor), dotRect);
        }

        /// <summary>
        /// Renders the unckecked state of the <see cref="PdfRadioButtonField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void RenderUncheckedState(PdfRadioButtonField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            // draw border
            if (!widget.BorderColor.IsEmpty)
            {
                var borderPen = new XPen(widget.BorderColor);
                gfx.DrawEllipse(borderPen, 0, 0, rect.Width, rect.Height);
            }
        }
    }
}
