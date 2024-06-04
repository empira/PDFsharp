// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Printing;
using PdfSharp;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.IO;

namespace MigraDoc.Rendering.Printing
{
    /// <summary>
    /// Represents a specialized System.Drawing.Printing.PrintDocument for MigraDoc documents.
    /// This component knows about MigraDoc and simplifies printing of MigraDoc documents.
    /// </summary>
    public class MigraDocPrintDocument : PrintDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MigraDoc.Rendering.Printing.MigraDocPrintDocument"/> class. 
        /// </summary>
        public MigraDocPrintDocument()
        {
            DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            OriginAtMargins = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MigraDoc.Rendering.Printing.MigraDocPrintDocument"/> class
        /// with the specified <see cref="T:MigraDoc.Rendering.DocumentRenderer"/> object.
        /// </summary>
        public MigraDocPrintDocument(DocumentRenderer renderer)
        {
            _renderer = renderer;
            DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            OriginAtMargins = false;
        }

        /// <summary>
        /// Gets or sets the DocumentRenderer that prints the pages of the document.
        /// </summary>
        public DocumentRenderer Renderer
        {
            get { return _renderer; }
            set { _renderer = value; }
        }
        DocumentRenderer _renderer;

        /// <summary>
        /// Gets or sets the page number that identifies the selected page. It it used on printing when 
        /// PrintRange.Selection is set.
        /// </summary>
        public int SelectedPage
        {
            get { return _selectedPage; }
            set { _selectedPage = value; }
        }
        int _selectedPage;

        /// <summary>
        /// Raises the <see cref="E:System.Drawing.Printing.PrintDocument.BeginPrint"/> event. It is called after the <see cref="M:System.Drawing.Printing.PrintDocument.Print"/> method is called and before the first page of the document prints.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs"/> that contains the event data.</param>
        protected override void OnBeginPrint(PrintEventArgs e)
        {
            Debug.Assert(_renderer != null, "Cannot print without a MigraDoc.Rendering.DocumentRenderer.");

            base.OnBeginPrint(e);
            if (!e.Cancel)
            {
                switch (PrinterSettings.PrintRange)
                {
                    case PrintRange.AllPages:
                        _pageNumber = 1;
                        _pageCount = _renderer.FormattedDocument.PageCount;
                        break;

                    case PrintRange.SomePages:
                        _pageNumber = PrinterSettings.FromPage;
                        _pageCount = PrinterSettings.ToPage - PrinterSettings.FromPage + 1;
                        break;

                    case PrintRange.Selection:
                        _pageNumber = _selectedPage;
                        _pageCount = 1;
                        break;

                    default:
                        Debug.Assert(false, "Invalid PrinterRange.");
                        e.Cancel = true;
                        break;
                }
            }
        }
        int _pageNumber = -1;
        int _pageCount;

        /// <summary>
        /// Raises the <see cref="E:System.Drawing.Printing.PrintDocument.QueryPageSettings"/> event. It is called immediately before each <see cref="E:System.Drawing.Printing.PrintDocument.PrintPage"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Drawing.Printing.QueryPageSettingsEventArgs"/> that contains the event data.</param>
        protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            base.OnQueryPageSettings(e);
            if (!e.Cancel)
            {
                PageSettings settings = e.PageSettings;
                PageInfo pageInfo = _renderer.FormattedDocument.GetPageInfo(_pageNumber);

                // set portrait/landscape
                settings.Landscape = pageInfo.Orientation == PageOrientation.Landscape;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Drawing.Printing.PrintDocument.PrintPage"/> event. It is called before a page prints.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Drawing.Printing.PrintPageEventArgs"/> that contains the event data.</param>
        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);
            if (!e.Cancel)
            {
                PageSettings settings = e.PageSettings;
                try
                {
                    Graphics graphics = e.Graphics;
                    IntPtr hdc = graphics.GetHdc();
                    int xOffset = GetDeviceCaps(hdc, PHYSICALOFFSETX);
                    int yOffset = GetDeviceCaps(hdc, PHYSICALOFFSETY);
                    graphics.ReleaseHdc(hdc);
                    graphics.TranslateTransform(-xOffset * 100 / graphics.DpiX, -yOffset * 100 / graphics.DpiY);
                    // Recall: Width and Height are exchanged when settings.Landscape is true.
                    XSize size = new XSize(e.PageSettings.Bounds.Width / 100.0 * 72, e.PageSettings.Bounds.Height / 100.0 * 72);
                    const float scale = 100f / 72f;
                    graphics.ScaleTransform(scale, scale);
                    // draw line A4 portrait
                    //graphics.DrawLine(Pens.Red, 0, 0, 21f / 2.54f * 72, 29.7f / 2.54f * 72);
#if WPF
//#warning TODO WPFPDF
// TODO WPFPDF
#else
                    XGraphics gfx = XGraphics.FromGraphics(graphics, size);
                    _renderer.RenderPage(gfx, _pageNumber);
#endif
                }
                catch
                {
                    e.Cancel = true;
                }
                _pageNumber++;
                _pageCount--;
                e.HasMorePages = _pageCount > 0;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Drawing.Printing.PrintDocument.EndPrint"/> event. It is called when the last page of the document has printed.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs"/> that contains the event data.</param>
        protected override void OnEndPrint(PrintEventArgs e)
        {
            base.OnEndPrint(e);
            _pageNumber = -1;
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int capability);
        const int PHYSICALOFFSETX = 112; // Physical Printable Area x margin
        const int PHYSICALOFFSETY = 113; // Physical Printable Area y margin
    }
}
