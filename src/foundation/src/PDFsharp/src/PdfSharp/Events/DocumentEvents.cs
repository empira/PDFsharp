// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfSharp.Events
{
    /// <summary>
    /// Base class for EventArgs in PDFsharp.
    /// </summary>
    public abstract class PdfSharpEventArgs : EventArgs
    {
        /// <summary>
        /// The source of the event.
        /// </summary>
        public PdfObject Source { get; set; } = null!;
    }

    /// <summary>
    /// The event type of a PageEvent.
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
    public class PageEventArgs : PdfSharpEventArgs
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

    /// <summary>
    /// The action type of a PageGraphicsEvent.
    /// </summary>
    public enum PageGraphicsActionType
    {
        /// <summary>
        /// The XGraphics object for the page was created.
        /// </summary>
        GraphicsCreated = 1,
        /// <summary>
        /// DrawString() was called on the page's XGraphics object.
        /// </summary>
        DrawString,
        /// <summary>
        /// Another method drawing content was called on the page's XGraphics object.
        /// </summary>
        Draw
    }

    /// <summary>
    /// EventArgs for actions on a page's XGraphics object.
    /// </summary>
    public class PageGraphicsEventArgs : PdfSharpEventArgs
    {
        /// <summary>
        /// Gets the page xxxxx.
        /// </summary>
        public PdfPage Page { get; internal set; } = null!;

        /// <summary>
        /// Gets the created XGraphics object.
        /// </summary>
        public XGraphics Graphics { get; internal set; } = null!;

        /// <summary>
        /// The action type of PageGraphicsEvent.
        /// </summary>
        public PageGraphicsActionType ActionType { get; internal set; }
    }

    /// <summary>
    /// EventHandler for OnPageGraphicsAction.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The PageGraphicsEventArgs of the event.</param>
    public delegate void PageGraphicsEventHandler(object sender, PageGraphicsEventArgs e);

    /// <summary>
    /// A class encapsulating all events of a PdfDocument.
    /// </summary>
    public class DocumentEvents
    {
        /// <summary>
        /// An event raised if a page was added.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The PageEventArgs of the event.</param>
        public void OnPageAdded(object sender, PageEventArgs args)
        {
            if (PageAdded != null)
                PageAdded(sender, args);
        }

        /// <summary>
        /// EventHandler for OnPageAdded.
        /// </summary>
        public event PageAddedOrRemovedEventHandler? PageAdded;

        /// <summary>
        /// An event raised if a page was removes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The PageEventArgs of the event.</param>
        public void OnPageRemoved(object sender, PageEventArgs args)
        {
            PageRemoved?.Invoke(sender, args);
        }

        /// <summary>
        /// EventHandler for OnPageRemoved.
        /// </summary>
        public event PageAddedOrRemovedEventHandler? PageRemoved;

        /// <summary>
        /// An event raised if the XGraphics object of a page is created.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The PageGraphicsEventArgs of the event.</param>
        public void OnPageGraphicsCreated(object sender, PageGraphicsEventArgs args)
        {
            PageGraphicsCreated?.Invoke(sender, args);
        }

        /// <summary>
        /// EventHandler for OnPageGraphicsCreated.
        /// </summary>
        public event PageGraphicsEventHandler? PageGraphicsCreated;

        /// <summary>
        /// An event raised if something is drawn on a page's XGraphics object.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The PageGraphicsEventArgs of the event.</param>
        public void OnPageGraphicsAction(object sender, PageGraphicsEventArgs args)
        {
            PageGraphicsAction?.Invoke(sender, args);
        }

        /// <summary>
        /// EventHandler for OnPageGraphicsAction.
        /// </summary>
        public event PageGraphicsEventHandler? PageGraphicsAction;
    }
}