// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Attachments
{
    /// <summary>
    /// Provides functionality to add and retrieve embedded files from a PDF document.
    /// </summary>
    public class EmbeddedFilesManager : ManagerBase
    {
        EmbeddedFilesManager(PdfDocument document) : base(document)
        {
            _document = document;

            switch (_document.State)
            {
                case DocumentState.Created:
                    InitializeNewDocument();
                    break;

                case DocumentState.Imported:
                    InitializeImportedDocument();
                    break;

                case DocumentState.Disposed:
                case DocumentState.Saved:
                default:
                    throw new InvalidOperationException($"Document is in state '{document.State}' and cannot be modified anymore.");
            }
        }

        void InitializeNewDocument()
        { }

        void InitializeImportedDocument()
        {
            var catalog = _document.Catalog;
            var x = catalog.Names;
        }

        /// <summary>
        /// Gets the number of embedded files of this document.
        /// </summary>
        public int FileCount => EmbeddedFiles?.FileCount ?? 0; // ChatGPT suggests FileCount over FilesCount.

        /// <summary>
        /// Gets the keys of all embedded files from the /Names array.
        /// </summary>
        public string[] NamesKeys // Here we use NamesKey because of the /Names array.
        {
            get
            {
                var embeddedFiles = _document.Catalog.Names.GetEmbeddedFiles(true);
                return embeddedFiles.Names?.NamesKeys ?? [];
            }
        }

        /// <summary>
        /// Embeds the specified file in the PDF document.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="compressStream"></param>
        public void AddFile(EmbeddedFileInfo fileInfo, bool compressStream = true)
        {
            //// TODO: Should return non-nullable object.
            //var embeddedFiles = _document.Catalog.Names.GetEmbeddedFiles(true)
            //    ?? throw new InvalidOperationException(SyMsgs.UnexpectedNullValueRetrieved(
            //        $"Function: {nameof(EmbeddedFilesManager)}.{nameof(AddFile)}.").Message);

            var embeddedFiles = _document.Catalog.Names.GetEmbeddedFiles(true);

            //var embeddedFileStream = new PdfEmbeddedFileStream(_document, fileInfo.Data, fileInfo.FileType,
            //      fileInfo.ModificationTime);  // TODO compress
            //var fileSpecification = new PdfFileSpecification(_document, embeddedFileStream, fileInfo.FileName);

            var fileSpecification = new PdfFileSpecification(_document, fileInfo);
            embeddedFiles.AddFileSpecification(fileInfo.NamesKey, fileSpecification);
        }

        /// <summary>
        /// Gets an EmbeddedFileInfo for the embedded file with the specified index.
        /// </summary>
        /// <param name="index">Index of the embedded file in range [0..FileCount].</param>
        public EmbeddedFileInfo GetEmbeddedFileInfo(int index)
        {
            var ef = EmbeddedFiles;
            if (ef == null)
                throw new InvalidOperationException("Document has no embedded files.");

            return ef.GetFileSpecification(index).GetFileInfo();
        }

        /// <summary>
        /// Gets an EmbeddedFileInfo for the embedded file with the specified /Names key.
        /// </summary>
        /// <param name="namesKey">The key of the embedded file in the /Names array.</param>
        /// <returns></returns>
        public EmbeddedFileInfo? GetEmbeddedFileInfo(string namesKey)
        {
            var ef = EmbeddedFiles;
            if (ef == null)
                return null;

            return ef.GetFileSpecification(namesKey)?.GetFileInfo() ?? null;
        }

        /// <summary>
        /// Get the /EmbeddedFiles name tree dictionary from the catalogs /Names entry, or null,
        /// if no such entry exists.
        /// Use this property to get direct access to the PDF objects that describes the embedded files.
        /// </summary>
        PdfEmbeddedFiles? EmbeddedFiles
        {
            get
            {
                var catalog = _document.Catalog;
                if (catalog.HasNames)
                {
                    var names = catalog.Names;
                    if (names.HasEmbeddedFiles)
                    {
                        var embeddedFiles = names.GetEmbeddedFiles();
                        return embeddedFiles;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets or creates the EmbeddedFilesManager for the specified document.
        /// </summary>
        public static EmbeddedFilesManager ForDocument(PdfDocument document)
            => document.EmbeddedFilesManager ??= new(document);

        readonly PdfDocument _document;
    }
}
