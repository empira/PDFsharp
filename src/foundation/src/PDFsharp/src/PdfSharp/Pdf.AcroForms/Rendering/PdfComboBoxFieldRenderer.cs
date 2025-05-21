﻿using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Renders a <see cref="PdfComboBoxField"/><br></br>
    /// </summary>
    /// <remarks>
    /// Inheritors should override the method <see cref="Render(PdfComboBoxField, PdfWidgetAnnotation, XGraphics, XRect)"/>
    /// </remarks>
    public class PdfComboBoxFieldRenderer
    {
        /// <summary>
        /// Renders the <see cref="PdfComboBoxField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void Render(PdfComboBoxField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            if (field.Font == null)
                return;

            if (widget.BackColor != XColor.Empty)
                gfx.DrawRectangle(new XSolidBrush(widget.BackColor), rect);
            // Draw Border
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
            {
                gfx.DrawRectangle(new XPen(widget.BorderColor, widget.Border.Width), rect);
                rect.Inflate(-widget.Border.Width, -widget.Border.Width);
            }

            var index = field.SelectedIndex;
            if (index > 0)
            {
                var text = field.ValueInOptArray(index, false);
                if (!String.IsNullOrEmpty(text))
                {
                    var format = field.TextAlign == PdfAcroFieldTextAlignment.Left
                        ? XStringFormats.CenterLeft
                        : field.TextAlign == PdfAcroFieldTextAlignment.Center
                        ? XStringFormats.Center
                        : XStringFormats.CenterRight;
                    gfx.DrawString(text, field.Font, new XSolidBrush(field.ForeColor), rect, format);
                }
            }
        }
    }
}
