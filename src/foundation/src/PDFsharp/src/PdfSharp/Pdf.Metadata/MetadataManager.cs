// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Events;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

// v7.0.0 Review

namespace PdfSharp.Pdf.Metadata
{
    /// <summary>
    /// The MetadataManager provides functionality for easier handling of PDF metadata.
    /// </summary>
    public class MetadataManager : ManagerBase
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        MetadataManager(PdfDocument document) : base(document)
        { }

        /// <summary>
        /// Gets the PDF metadata object from the catalog, or null,
        /// if no such object exists.
        /// </summary>
        public PdfMetadata? GetMetadata()
        {
            return Document.Catalog.GetMetadata();
        }

        /// <summary>
        /// Gets or sets the strategy how PDFsharp treads document metadata.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">DocumentMetadataStrategy was set more than once.</exception>
        public DocumentMetadataStrategy Strategy
        {
            get => _strategy ?? DocumentMetadataStrategy.NoMetadata;
            set
            {
                if (_strategy == value)
                    return;

                if (_strategy != null)
                {
                    // Update of strategy set by PdfAManager is allowed.
                    if (!(_strategy == DocumentMetadataStrategy.AutoGenerate && value == DocumentMetadataStrategy.UserGenerated))
                        throw new InvalidOperationException("DocumentMetadataStrategy can only be set once.");
                }
                _strategy = value;
            }
        }
        DocumentMetadataStrategy? _strategy;

        /// <summary>
        /// Adjusts the document metadata strategy for PDF/A generation if a PDF/A format is set.
        /// </summary>
        internal void AdjustStrategyForPdfA()
        {
            _strategy = _strategy switch
            {
                DocumentMetadataStrategy.KeepExisting => Fail(),
                DocumentMetadataStrategy.NoMetadata => Fail(),
                DocumentMetadataStrategy.AutoGenerate => DocumentMetadataStrategy.AutoGenerate,
                DocumentMetadataStrategy.UserGenerated => DocumentMetadataStrategy.UserGenerated,
                null or DocumentMetadataStrategy.NoMetadata => _strategy = DocumentMetadataStrategy.AutoGenerate,
                _ => throw new ArgumentOutOfRangeException(nameof(_strategy))
            };
            return;

            //[DoesNotReturn]
            DocumentMetadataStrategy Fail()
            {
                throw new InvalidOperationException(
                    $"With a document metadata strategy of '{_strategy}' the document cannot be a PDF/A document.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        public void SetMetadata(byte[] xml)
        {
            Document.Catalog.GetOrCreateMetadata().SetMetadata(xml);
        }

        /// <summary>
        /// Gets the metadata info PDFsharp has collected for the current document.
        /// This includes PdfDocumentInformation and the document IDs.
        /// </summary>
        public DocumentMetadataInfo GetMetadataInfo()
        {
            var md = new DocumentMetadataInfo(Document);
            return md;
        }

        internal void PrepareForSave()
        {
            var catalog = Document.Catalog;

            // If HasMetadata is true, but XML stream is empty, then remove the inconsistent Metadata.
            // We always remove empty Metadata here, regardless of the strategy.
            //if (catalog.HasMetadata && !(catalog.GetMetadata(false)!.Stream?.Value?.Length > 0))
            if (!(catalog.GetMetadata()?.Stream?.Value.Length > 0))
                catalog.Elements.Remove(PdfCatalog.Keys.Metadata);

            if (_strategy is null or DocumentMetadataStrategy.KeepExisting)
                return;

            if (_strategy == DocumentMetadataStrategy.NoMetadata)
            {
                // Remove Metadata.
                catalog.Elements.Remove(PdfCatalog.Keys.Metadata);
                return;
            }

            var metadata = catalog.GetOrCreateMetadata();
            if (_strategy == DocumentMetadataStrategy.AutoGenerate)
            {
                // Create default Metadata if it is currently empty.
                if (!(catalog.GetMetadata()?.Stream?.Value.Length > 0))  // TODO PdfDictionary.StreamLength would be helpful.
                {
                    var xml = metadata.CreateDefaultMetadata();
                    metadata.SetMetadata(xml);
                }
                return;
            }

            if (_strategy == DocumentMetadataStrategy.UserGenerated)
            {
                var metadataInfo = new DocumentMetadataInfo(Document);
                var metadataArgs = new DocumentMetadataEventArgs(Document)
                {
                    Metadata = metadata,
                    Info = metadataInfo
                };
                Document.Events.OnCreateDocumentMetadata(Document, metadataArgs);
            }
        }

        /// <summary>
        /// Converts a DateTimeOffset in an XMP metadata compatible string.
        /// </summary>
        internal static string ToXmpDateString(DateTimeOffset? dateTimeOffset)
        {
            var result = dateTimeOffset == null
                ? ""
                : Invariant($"{dateTimeOffset.Value:yyyy-MM-ddTHH:mm:ssK}");
            return result;
        }

        /// <summary>
        /// Gets or creates the MetadataManager for the specified document.
        /// </summary>
        public static MetadataManager ForDocument(PdfDocument document)
            => document.MetadataManager ??= new(document);
    }
}
