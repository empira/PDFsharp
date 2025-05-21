﻿using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Annotations.enums;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.AcroForms.Rendering
{
    /// <summary>
    /// Renders a <see cref="PdfPushButtonField"/><br></br>
    /// The current implementation does not render the <b>Rollover-</b> and <b>Down-</b> states 
    /// so the button will always appear in <b>Normal</b> state.<br></br>
    /// </summary>
    /// <remarks>
    /// Inheritors should override the methods <see cref="RenderNormalState(PdfPushButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/>, 
    /// <see cref="RenderRolloverState(PdfPushButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/> and 
    /// <see cref="RenderDownState(PdfPushButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/><br></br><br></br>
    /// It is allowed to throw a <see cref="NotImplementedException"/> in the methods
    /// <see cref="RenderRolloverState(PdfPushButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/> and 
    /// <see cref="RenderDownState(PdfPushButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/>,
    /// only the implementation of <see cref="RenderNormalState(PdfPushButtonField, PdfWidgetAnnotation, XGraphics, XRect)"/> 
    /// is mandatory.
    /// </remarks>
    public class PdfPushButtonFieldRenderer
    {
        static XColor ShadeLight = XColors.White;
        static XColor ShadeDark = XColors.DimGray;
        const double shadeWidth = 2;

        /// <summary>
        /// Renders the normal state of the <see cref="PdfPushButtonField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void RenderNormalState(PdfPushButtonField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            var text = field.Caption;
            if (string.IsNullOrWhiteSpace(text) || field.Font == null)
                return;

            var format = XStringFormats.Center;
            if (widget.NormalIcon != null)
            {
                format = widget.CaptionPlacement == PdfButtonCaptionPosition.LeftOfIcon
                ? XStringFormats.CenterLeft
                : widget.CaptionPlacement == PdfButtonCaptionPosition.RightOfIcon
                    ? XStringFormats.CenterRight
                    : widget.CaptionPlacement == PdfButtonCaptionPosition.AboveIcon
                        ? XStringFormats.TopCenter
                        : XStringFormats.BottomCenter;
            }

            if (!widget.BackColor.IsEmpty)
                gfx.DrawRectangle(new XSolidBrush(widget.BackColor), rect);
            if (!widget.BorderColor.IsEmpty && widget.Border.Width > 0)
            {
                gfx.DrawRectangle(new XPen(widget.BorderColor, widget.Border.Width), rect);
                //var prevRect = new XRect(rect.X, rect.Y, rect.Width, rect.Height);
                rect.Inflate(-widget.Border.Width, -widget.Border.Width);
                //gfx.TranslateTransform(widget.Border.Width, widget.Border.Width);
                //gfx.ScaleAtTransform(rect.Width / prevRect.Width, rect.Height / prevRect.Height, rect.Width / 2, rect.Height / 2);
            }
            var tlColor = XColor.Empty;
            var brColor = XColor.Empty;
            if (widget.Border.BorderStyle == Annotations.enums.PdfAnnotationBorderStyle.Beveled)
            {
                tlColor = ShadeLight;
                brColor = ShadeDark;
            }
            else if (widget.Border.BorderStyle == Annotations.enums.PdfAnnotationBorderStyle.Inset)
            {
                tlColor = ShadeDark;
                brColor = ShadeLight;
            }
            if (!tlColor.IsEmpty)
            {
                // draw additional beveled or inset border
                var tlPen = new XPen(tlColor, shadeWidth)
                {
                    LineCap = XLineCap.Square,
                    LineJoin = XLineJoin.Miter
                }; var brPen = new XPen(brColor, shadeWidth)
                {
                    LineCap = XLineCap.Square,
                    LineJoin = XLineJoin.Miter
                };
                // top
                gfx.DrawLine(tlPen, rect.X, rect.Top, rect.Right, rect.Top);
                // left
                gfx.DrawLine(tlPen, rect.X, rect.Y, rect.X, rect.Bottom);
                // bottom
                gfx.DrawLine(brPen, rect.X + shadeWidth, rect.Bottom, rect.Right, rect.Bottom);
                // right
                gfx.DrawLine(brPen, rect.Right, rect.Y, rect.Right, rect.Bottom);
                rect.Inflate(-brPen.Width, -brPen.Width);
            }
            // TODO: offset icon based on CaptionPosition
            if (widget.NormalIcon != null && widget.CaptionPlacement != PdfButtonCaptionPosition.CaptionOnly)
                gfx.AppendToContentStream(PdfEncoders.RawEncoding.GetString(widget.NormalIcon.Stream.UnfilteredValue));

            if (widget.CaptionPlacement != PdfButtonCaptionPosition.IconOnly)
                gfx.DrawString(text, field.Font, new XSolidBrush(field.ForeColor), rect, format);
        }

        /// <summary>
        /// Renders the rollover state of the <see cref="PdfPushButtonField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void RenderRolloverState(PdfPushButtonField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders the down state of the <see cref="PdfPushButtonField"/>
        /// </summary>
        /// <param name="field">The field being rendered</param>
        /// <param name="widget">The <see cref="PdfWidgetAnnotation"/> of the field that is being rendered</param>
        /// <param name="gfx">The <see cref="XGraphics"/> used to perform drawing operations</param>
        /// <param name="rect">The <see cref="XRect"/> spcifying the position and dimensions of the field</param>
        public virtual void RenderDownState(PdfPushButtonField field, PdfWidgetAnnotation widget, XGraphics gfx, XRect rect)
        {
            throw new NotImplementedException();
        }
    }
}
