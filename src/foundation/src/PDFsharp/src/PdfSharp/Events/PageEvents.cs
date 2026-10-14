// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;

namespace PdfSharp.Events  // #FILE PageEvents.cs
{
    /// <summary>
    /// The event type of PageEvent.
    /// </summary>
    public enum PageEventType
    {
        /// <summary>
        /// A new page was created.
        /// </summary>
        Created,

        /// <summary>
        /// A page was moved.
        /// </summary>
        Moved,

        /// <summary>
        /// A page was imported from another document.
        /// </summary>
        Imported,

        /// <summary>
        /// A page was removed.
        /// </summary>
        Removed
    }

    /// <summary>
    /// EventArgs for changes in the PdfPages of a document.
    /// </summary>
    //public class PageEventArgs(PdfObject source) : PdfSharpEventArgs(source)
    public class PageEventArgs(PdfDocument source) : PdfSharpEventArgs(source)
    {
        /// <summary>
        /// Gets or sets the affected page.
        /// </summary>
        public PdfPage Page { get; set; } = null!;

        /// <summary>
        /// Gets or sets the page index of the affected page.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// The event type of PageEvent.
        /// </summary>
        public PageEventType EventType { get; internal set; }
    }

    /// <summary>
    /// EventHandler for OnPageAdded and OnPageRemoved.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The PageEventArgs of the event.</param>
    public delegate void PageAddedOrRemovedEventHandler(object sender, PageEventArgs e);
}