// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections;
using System.ComponentModel;
#if GDI
using System.Drawing;
using System.Windows.Forms;
#endif
#if Wpf
using System.Windows.Media;
#endif

#if !GDI
#error This file must only be included in GDI build.
#endif

namespace PdfSharp.Forms
{
    /// <summary>
    /// Implements the control that previews the page.
    /// </summary>
    class PagePreviewCanvas : System.Windows.Forms.Control
    {
        public PagePreviewCanvas(PagePreview preview)
        {
            _preview = preview;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        }
        PagePreview _preview;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_preview._showPage)
                return;

            Graphics gfx = e.Graphics;
            bool zoomChanged;
            _preview.CalculatePreviewDimension(out zoomChanged);
            _preview.RenderPage(gfx);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!_preview._showPage)
            {
                e.Graphics.Clear(_preview._desktopColor);
                return;
            }
            bool zoomChanged;
            _preview.CalculatePreviewDimension(out zoomChanged);
            _preview.PaintBackground(e.Graphics);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Invalidate();
        }
    }
}
