// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfSharp.Events
{
    /// <summary>
    /// The action type of PageGraphicsEvent.
    /// </summary>
    public enum PageGraphicsActionType
    {
        /// <summary>
        /// The XGraphics object for the page was created.
        /// </summary>
        GraphicsCreated = 1,

        /// <summary>
        /// DrawString() was called on the page’s XGraphics object.
        /// </summary>
        DrawString,

        /// <summary>
        /// Another method drawing content was called on the page’s XGraphics object.
        /// </summary>
        Draw
    }

    /// <summary>
    /// EventArgs for actions on a page’s XGraphics object.
    /// </summary>
    public class PageGraphicsEventArgs(PdfDocument source) : PdfSharpEventArgs(source)
    {
        /// <summary>
        /// Gets the page that causes the event.
        /// </summary>
        public PdfPage Page { get; internal init; } = null!;

        /// <summary>
        /// Gets the created XGraphics object.
        /// </summary>
        public XGraphics Graphics { get; internal init; } = null!;

        /// <summary>
        /// The action type of PageGraphicsEvent.
        /// </summary>
        public PageGraphicsActionType ActionType { get; internal init; }
    }

    /// <summary>
    /// EventHandler for OnPageGraphicsAction.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The PageGraphicsEventArgs of the event.</param>
    public delegate void PageGraphicsEventHandler(object sender, PageGraphicsEventArgs e);
}