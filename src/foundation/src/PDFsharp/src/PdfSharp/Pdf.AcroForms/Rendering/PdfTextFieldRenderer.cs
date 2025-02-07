﻿using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Renders a <see cref="PdfTextField"/>
    /// </summary>
    /// <remarks>
    /// Inheritors should override the method <see cref="Render(PdfTextField, PdfWidgetAnnotation, XGraphics, XRect)"/>
    /// </remarks>
    public class PdfTextFieldRenderer
    {
        /// <summary>
        /// Renders the <see cref="PdfTextField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void Render(PdfTextField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            var text = field.Text;
            if (string.IsNullOrWhiteSpace(text) || field.Font == null)
                return;

            if (field.MaxLength > 0)
                text = text.Substring(0, Math.Min(text.Length, field.MaxLength));

            var format = field.TextAlign == PdfAcroFieldTextAlignment.Left
                ? XStringFormats.CenterLeft
                : field.TextAlign == PdfAcroFieldTextAlignment.Center
                    ? XStringFormats.Center
                    : XStringFormats.CenterRight;

            if (field.MultiLine)
                format.LineAlignment = XLineAlignment.Near;
            if (!widget.BackColor.IsEmpty)
                gfx.DrawRectangle(new XSolidBrush(widget.BackColor), rect);
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
            {
                gfx.DrawRectangle(new XPen(widget.BorderColor, widget.Border.Width), rect);
                rect.Inflate(-widget.Border.Width, -widget.Border.Width);
            }

            var renderFont = field.Font;
            if (field.AutomaticFontSize)
                renderFont = XFont.FromExisting(renderFont, field.DetermineFontSize(widget));

            if (field.Combined && field.MaxLength > 0)
            {
                var combWidth = rect.Width / field.MaxLength;
                var cBrush = new XSolidBrush(field.ForeColor);
                var count = Math.Min(text.Length, field.MaxLength);
                for (var ci = 0; ci < count; ci++)
                {
                    var cRect = new XRect(ci * combWidth, 0, combWidth, rect.Height);
                    gfx.DrawString(text[ci].ToString(), renderFont, cBrush, cRect, XStringFormats.Center);
                }
            }
            else
            {
                // for Multiline fields, we use XTextFormatter to handle line-breaks and a fixed TextFormat (only TopLeft is supported)
                if (field.MultiLine)
                {
                    var tf = new XTextFormatter(gfx);
                    tf.DrawString(text, renderFont, new XSolidBrush(field.ForeColor), rect, XStringFormats.TopLeft);
                }
                else
                    gfx.DrawString(text, renderFont, new XSolidBrush(field.ForeColor), rect, format);
            }
        }
    }
}
