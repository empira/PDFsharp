// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Metadata;

// TODO REMOVE
#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Events
{
    /// <summary>  // TODO
    /// EventArgs for changes in the PdfPages of a document.
    /// </summary>
    public class DocumentMetadataEventArgs(PdfDocument source) : PdfSharpEventArgs(source)
    {
        public required PdfMetadata Metadata { get; init; }

        public required DocumentMetadataInfo Info { get; init; }
    }

    /// <summary>  // TODO
    /// 
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The XmlMetadataEventArgs of the event.</param>
    public delegate void DocumentMetadataEventHandler(object sender, DocumentMetadataEventArgs e);
}

namespace PdfSharp.Events // MaOs4StLa Review.
{
    /// <summary>
    /// EventArgs for a document.
    /// </summary>
    public class DocumentEventArgs(PdfDocument source) : PdfSharpEventArgs(source)
    { }

    public delegate void DocumentEventHandler(object sender, DocumentEventArgs e);
}

namespace PdfSharp.Events
{
    /// <summary>
    /// A class encapsulating all events of a PdfDocument.
    /// </summary>
    public class DocumentEvents
    {
        /// <summary>
        /// An event raised if a document gets disposed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The DocumentEventArgs of the event.</param>
        public void OnDisposed(object sender, DocumentEventArgs args) // MaOs4StLa Review.
        {
            Disposed?.Invoke(sender, args);
        }

        /// <summary>
        /// EventHandler for OnDisposed.
        /// </summary>
        public event DocumentEventHandler? Disposed; // MaOs4StLa Review.

        /// <summary>
        /// An event raised if a page was added.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The PageEventArgs of the event.</param>
        public void OnPageAdded(object sender, PageEventArgs args)
        {
            PageAdded?.Invoke(sender, args);
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
        /// An event raised if something is drawn on a page’s XGraphics object.
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
        /// <summary>
        /// An event raised if something is drawn on a page’s XGraphics object. TODO
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The PageGraphicsEventArgs of the event.</param>
        public void OnCreateDocumentMetadata(object sender, DocumentMetadataEventArgs args)
        {
            if (CreateDocumentMetadata == null)
                throw new InvalidOperationException("The document event CreateDocumentMetadata is not set and cannot be invoked. "+
                                                    "You must provide a delegate that handle the CreateDocumentMetadata event.");

            CreateDocumentMetadata.Invoke(sender, args);
        }

        /// <summary>
        /// EventHandler for OnCreateDocumentMetadata.
        /// </summary>
        public event DocumentMetadataEventHandler? CreateDocumentMetadata;
    }
}