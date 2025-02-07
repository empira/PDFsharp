﻿using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Renders a <see cref="PdfListBoxField"/><br></br>
    /// </summary>
    /// <remarks>
    /// Inheritors should override the method <see cref="Render(PdfListBoxField, PdfWidgetAnnotation, XGraphics, XRect)"/>
    /// </remarks>
    public class PdfListBoxFieldRenderer
    {
        /// <summary>
        /// Renders the <see cref="PdfListBoxField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void Render(PdfListBoxField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            if (field.Font == null)
                return;

            var format = field.TextAlign == PdfAcroFieldTextAlignment.Left
                ? XStringFormats.CenterLeft
                : field.TextAlign == PdfAcroFieldTextAlignment.Center
                    ? XStringFormats.Center
                    : XStringFormats.CenterRight;
            if (widget.BackColor != XColor.Empty)
                gfx.DrawRectangle(new XSolidBrush(widget.BackColor), rect);
            // Draw Border
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
                gfx.DrawRectangle(new XPen(widget.BorderColor, widget.Border.Width), rect);

            var lineHeight = field.Font.Height;
            var y = 0.0;
            var startIndex = Math.Max(0, Math.Min(field.TopIndex, field.Options.Count - (int)(rect.Height / lineHeight)));
            for (var i = startIndex; i < field.Options.Count; i++)
            {
                var text = field.Options.ElementAt(i);
                // offset and shrink a bit to not draw on top of the outer border
                var lineRect = new XRect(widget.Border.Width, y + widget.Border.Width,
                    rect.Width - 2 * widget.Border.Width, lineHeight - 1);
                var selected = false;
                if (text.Length > 0)
                {
                    if (field.SelectedIndices.Contains(i))
                    {
                        gfx.DrawRectangle(new XSolidBrush(field.HighlightColor), lineRect);
                        selected = true;
                    }
                    lineRect.Inflate(-2, 0);
                    gfx.DrawString(text, field.Font,
                        new XSolidBrush(selected ? field.HighlightTextColor : field.ForeColor), lineRect, format);
                    y += lineHeight;
                }
            }
        }
    }
}
