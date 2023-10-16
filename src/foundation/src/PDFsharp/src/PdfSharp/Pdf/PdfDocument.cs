// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.InteropServices;
using PdfSharp.Events;
#if WPF
using System.IO;
#endif
//#if UWP
//using System.Threading.Tasks;
//#endif
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Security;
using PdfSharp.UniversalAccessibility;
// ReSharper disable InconsistentNaming

// ReSharper disable ConvertPropertyToExpressionBody

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF document.
    /// </summary>
    [DebuggerDisplay("(Name={" + nameof(Name) + "})")] // A name makes debugging easier
    public sealed class PdfDocument : PdfObject, IDisposable
    {
        internal DocumentState _state;
        internal PdfDocumentOpenMode _openMode;

#if DEBUG_
        static PdfDocument()
        {
            PSSR.TestResourceMessages();
            //string test = PSSR.ResMngr.GetString("SampleMessage1");
            //test.GetType();
        }
#endif

        /// <summary>
        /// Creates a new PDF document in memory.
        /// To open an existing PDF file, use the PdfReader class.
        /// </summary>
        public PdfDocument()
        {
            //PdfDocument.Gob.AttachDocument(Handle);

            _creation = DateTime.Now;
            _state = DocumentState.Created;
            _version = 14;
            Initialize();
            Info.CreationDate = _creation;
        }

        /// <summary>
        /// Creates a new PDF document with the specified file name. The file is immediately created and kept
        /// locked until the document is closed. At that time the document is saved automatically.
        /// Do not call Save() for documents created with this constructor, just call Close().
        /// To open an existing PDF file and import it, use the PdfReader class.
        /// </summary>
        public PdfDocument(string filename)
        {
            //PdfDocument.Gob.AttachDocument(Handle);

            _creation = DateTime.Now;
            _state = DocumentState.Created;
            _version = 14;
            Initialize();
            Info.CreationDate = _creation;

            OutStream = new FileStream(filename, FileMode.Create);
        }

        /// <summary>
        /// Creates a new PDF document using the specified stream.
        /// The stream won't be used until the document is closed. At that time the document is saved automatically.
        /// Do not call Save() for documents created with this constructor, just call Close().
        /// To open an existing PDF file, use the PdfReader class.
        /// </summary>
        public PdfDocument(Stream outputStream)
        {
            //PdfDocument.Gob.AttachDocument(Handle);

            _creation = DateTime.Now;
            _state = DocumentState.Created;
            _version = 14;
            Initialize();
            Info.CreationDate = _creation;

            OutStream = outputStream;
        }

        internal PdfDocument(Lexer lexer)
        {
            //PdfDocument.Gob.AttachDocument(Handle);

            _creation = DateTime.Now;
            _state = DocumentState.Imported;

            //_info = new PdfInfo(this);
            //_pages = new PdfPages(this);
            //_fontTable = new PdfFontTable();
            //_catalog = new PdfCatalog(this);
            ////_font = new PdfFont();
            //_objects = new PdfObjectTable(this);
            //_trailer = new PdfTrailer(this);
            IrefTable = new PdfCrossReferenceTable(this);
            _lexer = lexer;
        }

        void Initialize()
        {
            //_info = new PdfInfo(this);
            _fontTable = new PdfFontTable(this);
            _imageTable = new PdfImageTable(this);
            Trailer = new PdfTrailer(this);
            IrefTable = new PdfCrossReferenceTable(this);
            Trailer.CreateNewDocumentIDs();
        }

        //~PdfDocument()
        //{
        //  Dispose(false);
        //}

        /// <summary>
        /// Disposes all references to this document stored in other documents. This function should be called
        /// for documents you finished importing pages from. Calling Dispose is technically not necessary but
        /// useful for earlier reclaiming memory of documents you do not need anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_state != DocumentState.Disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (OutStream is not null)
                    {
                        ((IDisposable)OutStream).Dispose();
                    }
                }
                //PdfDocument.Gob.DetachDocument(Handle);
            }
            _state = DocumentState.Disposed;
        }

        /// <summary>
        /// Gets or sets a user defined object that contains arbitrary information associated with this document.
        /// The tag is not used by PDFsharp.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Encapsulates the document's events.
        /// </summary>
        public DocumentEvents Events => _events ??= new DocumentEvents();

        DocumentEvents? _events;

        /// <summary>
        /// Gets or sets a value used to distinguish PdfDocument objects.
        /// The name is not used by PDFsharp.
        /// </summary>
        string Name { get; set; } = NewName();

        /// <summary>
        /// Get a new default name for a new document.
        /// </summary>
        static string NewName()
        {
#if DEBUG_
            if (PdfDocument.nameCount == 57)
                PdfDocument.nameCount.GetType();
#endif
            return "Document " + _nameCount++;
        }
        static int _nameCount;

        //internal bool CanModify => true;
        internal bool CanModify => _openMode == PdfDocumentOpenMode.Modify;

        /// <summary>
        /// Closes this instance.
        /// Saves the document if the PdfDocument was created with a filename or a stream.
        /// </summary>
        public void Close()
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);

            if (OutStream != null)
            {
                // Get security handler if document gets encrypted.
                var effectiveSecurityHandler = SecuritySettings.EffectiveSecurityHandler;

                var writer = new PdfWriter(OutStream, effectiveSecurityHandler);
                try
                {
                    DoSave(writer);
                }
                finally
                {
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Saves the document to the specified path. If a file already exists, it will be overwritten.
        /// </summary>
        public void Save(string path)
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);

            using Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            Save(stream);
        }

#if UWP
        /// <summary>
        /// Saves the document to the specified path. If a file already exists, it will be overwritten.
        /// </summary>
        public async Task SaveAsync(string path, bool closeStream)
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);

            // Just march through...

            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("My1st.pdf", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var stream = await file.OpenStreamForWriteAsync();
            using (var writer = new StreamWriter(stream))
            {
                Save(stream, false);
            }

            //var ms = new MemoryStream();
            //Save(ms, false);
            //byte[] pdf = ms.ToArray();
            //ms.Close();
            //await stream.WriteAsync(pdf, 0, pdf.Length);
            //stream.Close();
        }
#endif

        /// <summary>
        /// Saves the document to the specified stream.
        /// </summary>
        public void Save(Stream stream, bool closeStream)
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);

            // TODO: more diagnostic checks
            string message = "";
            if (!CanSave(ref message))
                throw new PdfSharpException(message);

            // Get security handler if document gets encrypted.
            var effectiveSecurityHandler = SecuritySettings.EffectiveSecurityHandler;

            PdfWriter? writer = null;
            try
            {
                writer = new PdfWriter(stream, effectiveSecurityHandler);
                DoSave(writer);
            }
            finally
            {
                if (stream != null)
                {
                    if (closeStream)
#if UWP
                        stream.Dispose();
#else
                        stream.Close();
#endif
                    else
                    {
                        if (stream.CanRead && stream.CanSeek)
                            stream.Position = 0; // Reset the stream position if the stream is kept open.
                    }
                }
                writer?.Close(closeStream);
            }
        }

        /// <summary>
        /// Saves the document to the specified stream.
        /// The stream is not closed by this function.
        /// (Older versions of PDFsharp closes the stream. That was not very useful.)
        /// </summary>
        public void Save(Stream stream)
            => Save(stream, false);

        /// <summary>
        /// Implements saving a PDF file.
        /// </summary>
        void DoSave(PdfWriter writer)
        {
            if (_pages == null || _pages.Count == 0)
            {
                if (OutStream != null)
                {
                    // Give feedback if the wrong constructor was used.
                    throw new InvalidOperationException("Cannot save a PDF document with no pages. Do not use \"public PdfDocument(string filename)\" or \"public PdfDocument(Stream outputStream)\" if you want to open an existing PDF document from a file or stream; use PdfReader.Open() for that purpose.");
                }
                throw new InvalidOperationException("Cannot save a PDF document with no pages.");
            }

            try
            {
                // HACK: Remove XRefTrailer
                if (Trailer is PdfCrossReferenceStream)
                {
                    // HACK^2: Preserve the SecurityHandler.
                    var securityHandler = Trailer.SecurityHandlerInternal;
                    Trailer = new PdfTrailer((PdfCrossReferenceStream)Trailer);
                    Trailer.SecurityHandlerInternal = securityHandler;
                }

                var effectiveSecurityHandler = _securitySettings?.EffectiveSecurityHandler;
                if (effectiveSecurityHandler != null)
                {
                    if (effectiveSecurityHandler.Reference == null)
                        IrefTable.Add(effectiveSecurityHandler);
                    else
                        Debug.Assert(IrefTable.Contains(effectiveSecurityHandler.ObjectID));
                    Trailer.Elements[PdfTrailer.Keys.Encrypt] = _securitySettings!.SecurityHandler.Reference;
                }
                else
                    Trailer.Elements.Remove(PdfTrailer.Keys.Encrypt);

                PrepareForSave();

                effectiveSecurityHandler?.PrepareForWriting();

                writer.WriteFileHeader(this);
                var irefs = IrefTable.AllReferences;
                int count = irefs.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    PdfReference iref = irefs[idx];
#if DEBUG_
                    if (iref.ObjectNumber == 378)
                        GetType();
#endif
                    iref.Position = writer.Position;
                    iref.Value.WriteObject(writer);
                }
                int startxref = writer.Position;
                IrefTable.WriteObject(writer);
                writer.WriteRaw("trailer\n");
                Trailer.Elements.SetInteger("/Size", count + 1);
                Trailer.WriteObject(writer);
                writer.WriteEof(this, startxref);

                //if (encrypt)
                //{
                //  state &= ~DocumentState.SavingEncrypted;
                //  //_securitySettings.SecurityHandler.EncryptDocument();
                //}
            }
            finally
            {
                if (writer != null)
                {
                    writer.Stream.Flush();
                    // DO NOT CLOSE WRITER HERE
                    //writer.Close();
                }
            }
        }

        /// <summary>
        /// Dispatches PrepareForSave to the objects that need it.
        /// </summary>
        internal override void PrepareForSave()
        {
            PdfDocumentInformation info = Info;

            // DELETE
            //// Add patch level to producer if it is not '0'.
            //string pdfSharpProducer = VersionInfo.Producer;
            //if (!PdfSharpProductVersionInformation.VersionPatch.Equals("0"))
            //    pdfSharpProducer = ProductVersionInfo.Producer;

            // The Creator is called 'Application' in Acrobat.
            // The Producer is call "Created by" in Acrobat.

            // Set Creator if value is undefined. This is the 'application' in Adobe Reader.
            if (info.Elements[PdfDocumentInformation.Keys.Creator] is null)
                info.Creator = PdfSharpProductVersionInformation.Producer;

            // We set Producer if it is not yet set.

            var pdfProducer = $"{PdfSharpProductVersionInformation.Creator} under {RuntimeInformation.OSDescription}";

            //pdfProducer = $"{GitVersionInformation.SemVer} under {RuntimeInformation.OSDescription}";

            // Keep original producer if file was imported. This is 'PDF created by' in Adobe Reader.
            string producer = info.Producer;
            if (producer.Length == 0)
                producer = pdfProducer;
            else
            {
                // Prevent endless concatenation if file is edited with PDFsharp more than once.
                if (!producer.StartsWith(PdfSharpProductVersionInformation.Title, StringComparison.Ordinal))
                    producer = $"{pdfProducer} (Original: {producer})";
            }
            info.Elements.SetString(PdfDocumentInformation.Keys.Producer, producer);

            // Prepare used fonts.
            _fontTable?.PrepareForSave();

            // Let catalog do the rest.
            Catalog.PrepareForSave();

#if true
            // Remove all unreachable objects (e.g. from deleted pages).
            int removed = IrefTable.Compact();
            if (removed != 0)
                Debug.WriteLine("PrepareForSave: Number of deleted unreachable objects: " + removed);
            IrefTable.Renumber();
#endif

            // @PDF/UA
            // Create PdfMetadata now to include the final document information in XMP generation.
            Catalog.Elements.SetReference(PdfCatalog.Keys.Metadata, new PdfMetadata(this));
        }

        /// <summary>
        /// Determines whether the document can be saved.
        /// </summary>
        public bool CanSave(ref string message)
        {
            if (!SecuritySettings.CanSave(ref message))
                return false;

            return true;
        }

        internal bool HasVersion(string version)
        {
            return String.CompareOrdinal(Catalog.Version, version) >= 0;
        }

        /// <summary>
        /// Gets the document options used for saving the document.
        /// </summary>
        public PdfDocumentOptions Options
            => _options ??= new PdfDocumentOptions(this);

        PdfDocumentOptions? _options;

        /// <summary>
        /// Gets PDF specific document settings.
        /// </summary>
        public PdfDocumentSettings Settings
            => _settings ??= new PdfDocumentSettings(this);

        PdfDocumentSettings? _settings;

        /// <summary>
        /// NYI Indicates whether large objects are written immediately to the output stream to reduce
        /// memory consumption.
        /// </summary>
        internal bool EarlyWrite => false;

        /// <summary>
        /// Gets or sets the PDF version number. Return value 14 e.g. means PDF 1.4 / Acrobat 5 etc.
        /// </summary>
        public int Version
        {
            get => _version;
            set
            {
                if (!CanModify)
                    throw new InvalidOperationException(PSSR.CannotModify);
                if (value is < 12 or > 20) // TODO not really implemented
                    throw new ArgumentException(PSSR.InvalidVersionNumber, nameof(value));
                _version = value;
            }
        }
        internal int _version;

        /// <summary>
        /// Adjusts the version if the current version is lower than the required version.
        /// </summary>
        /// <param name="requiredVersion">The minimum version number to set version to.</param>
        /// <returns>True, if Version was modified.</returns>
        public bool SetRequiredVersion(int requiredVersion)
        {
            if (requiredVersion > Version)
            {
                Version = requiredVersion;
                return true;
            }

            return false;

        }

        /// <summary>
        /// Gets the number of pages in the document.
        /// </summary>
        public int PageCount
        {
            get
            {
                if (CanModify)
                    return Pages.Count;
                // PdfOpenMode is InformationOnly.
                var pageTreeRoot = (PdfDictionary?)Catalog.Elements.GetObject(PdfCatalog.Keys.Pages);
                return pageTreeRoot?.Elements.GetInteger(PdfPages.Keys.Count) ?? 0;
            }
        }

        /// <summary>
        /// Gets the file size of the document.
        /// </summary>
        public long FileSize => _fileSize;

        internal long _fileSize; // TODO: make private

        /// <summary>
        /// Gets the full qualified file name if the document was read form a file, or an empty string otherwise.
        /// </summary>
        public string FullPath => _fullPath;

        internal string _fullPath = String.Empty; // TODO: make private

        /// <summary>
        /// Gets a Guid that uniquely identifies this instance of PdfDocument.
        /// </summary>
        public Guid Guid => _guid;

        readonly Guid _guid = Guid.NewGuid();

        internal DocumentHandle Handle
            => _handle ??= new DocumentHandle(this);

        DocumentHandle? _handle;

        /// <summary>
        /// Returns a value indicating whether the document was newly created or opened from an existing document.
        /// Returns true if the document was opened with the PdfReader.Open function, false otherwise.
        /// </summary>
        public bool IsImported => (_state & DocumentState.Imported) != 0;

        /// <summary>
        /// Returns a value indicating whether the document is read only or can be modified.
        /// </summary>
        public bool IsReadOnly => (_openMode != PdfDocumentOpenMode.Modify);

        internal Exception DocumentNotImported()
        {
            return new InvalidOperationException("Document not imported.");
        }

        /// <summary>
        /// Gets information about the document.
        /// </summary>
        public PdfDocumentInformation Info
            => _info ??= Trailer.Info;

        PdfDocumentInformation? _info;  // Never changes if once created.

        /// <summary>
        /// This function is intended to be undocumented.
        /// </summary>
        public PdfCustomValues? CustomValues
        {
            get => _customValues ??= PdfCustomValues.Get(Catalog.Elements);
            set
            {
                if (value != null)
                    throw new ArgumentException("Only null is allowed to clear all custom values.");
                PdfCustomValues.Remove(Catalog.Elements);
                _customValues = null;
            }
        }
        PdfCustomValues? _customValues;

        /// <summary>
        /// Get the pages dictionary.
        /// </summary>
        public PdfPages Pages
            => _pages ??= Catalog.Pages;

        PdfPages? _pages;  // Never changes if once created.

        /// <summary>
        /// Gets or sets a value specifying the page layout to be used when the document is opened.
        /// </summary>
        public PdfPageLayout PageLayout
        {
            get => Catalog.PageLayout;
            set
            {
                if (!CanModify)
                    throw new InvalidOperationException(PSSR.CannotModify);
                Catalog.PageLayout = value;
            }
        }

        /// <summary>
        /// Gets or sets a value specifying how the document should be displayed when opened.
        /// </summary>
        public PdfPageMode PageMode
        {
            get => Catalog.PageMode;
            set
            {
                if (!CanModify)
                    throw new InvalidOperationException(PSSR.CannotModify);
                Catalog.PageMode = value;
            }
        }

        /// <summary>
        /// Gets the viewer preferences of this document.
        /// </summary>
        public PdfViewerPreferences ViewerPreferences => Catalog.ViewerPreferences;

        /// <summary>
        /// Gets the root of the outline (or bookmark) tree.
        /// </summary>
        public PdfOutlineCollection Outlines => Catalog.Outlines;

        /// <summary>
        /// Get the AcroForm dictionary.
        /// </summary>
        public PdfAcroForm AcroForm => Catalog.AcroForm;

        /// <summary>
        /// Gets or sets the default language of the document.
        /// </summary>
        public string Language
        {
            get => Catalog.Language;
            set => Catalog.Language = value;
        }

        /// <summary>
        /// Gets the security settings of this document.
        /// </summary>
        public PdfSecuritySettings SecuritySettings
            => _securitySettings ??= new PdfSecuritySettings(this);

        internal PdfSecuritySettings? _securitySettings;

        /// <summary>
        /// Gets the document font table that holds all fonts used in the current document.
        /// </summary>
        internal PdfFontTable FontTable
            => _fontTable ??= new PdfFontTable(this);

        PdfFontTable? _fontTable;

        /// <summary>
        /// Gets the document image table that holds all images used in the current document.
        /// </summary>
        internal PdfImageTable ImageTable
            => _imageTable ??= new PdfImageTable(this);

        PdfImageTable? _imageTable;

        /// <summary>
        /// Gets the document form table that holds all form external objects used in the current document.
        /// </summary>
        internal PdfFormXObjectTable FormTable  // TODO: Rename to ExternalDocumentTable.
            => _formTable ??= new PdfFormXObjectTable(this);

        PdfFormXObjectTable? _formTable;

        /// <summary>
        /// Gets the document ExtGState table that holds all form state objects used in the current document.
        /// </summary>
        internal PdfExtGStateTable ExtGStateTable
            => _extGStateTable ??= new PdfExtGStateTable(this);

        PdfExtGStateTable? _extGStateTable;

        /// <summary>
        /// Gets the PdfCatalog of the current document.
        /// </summary>
        internal PdfCatalog Catalog
            => _catalog ??= Trailer.Root;

        PdfCatalog? _catalog;  // never changes if once created

        /// <summary>
        /// Gets the PdfInternals object of this document, that grants access to some internal structures
        /// which are not part of the public interface of PdfDocument.
        /// </summary>
        public new PdfInternals Internals
            => _internals ??= new PdfInternals(this);

        PdfInternals? _internals;

        /// <summary>
        /// Creates a new page and adds it to this document.
        /// Depending on the IsMetric property of the current region the page size is set to 
        /// A4 or Letter respectively. If this size is not appropriate it should be changed before
        /// any drawing operations are performed on the page.
        /// </summary>
        public PdfPage AddPage()
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);
            return Catalog.Pages.Add();
        }

        /// <summary>
        /// Adds the specified page to this document. If the page is from an external document,
        /// it is imported to this document. In this case the returned page is not the same
        /// object as the specified one.
        /// </summary>
        public PdfPage AddPage(PdfPage page)
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);
            return Catalog.Pages.Add(page);
        }

        /// <summary>
        /// Creates a new page and inserts it in this document at the specified position.
        /// </summary>
        public PdfPage InsertPage(int index)
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);
            return Catalog.Pages.Insert(index);
        }

        /// <summary>
        /// Inserts the specified page in this document. If the page is from an external document,
        /// it is imported to this document. In this case the returned page is not the same
        /// object as the specified one.
        /// </summary>
        public PdfPage InsertPage(int index, PdfPage page)
        {
            if (!CanModify)
                throw new InvalidOperationException(PSSR.CannotModify);
            return Catalog.Pages.Insert(index, page);
        }

        /// <summary>
        /// Adds a named destination to the document.
        /// </summary>
        /// <param name="destinationName">The Named Destination's name.</param>
        /// <param name="destinationPage">The page to navigate to.</param>
        /// <param name="parameters">The PdfNamedDestinationParameters defining the named destination's parameters.</param>
        public void AddNamedDestination(string destinationName, int destinationPage, PdfNamedDestinationParameters parameters)
            => Internals.Catalog.Names.AddNamedDestination(destinationName, destinationPage, parameters);

        /// <summary>
        /// Adds an embedded file to the document.
        /// </summary>
        /// <param name="name">The name used to refer and to entitle the embedded file.</param>
        /// <param name="path">The path of the file to embed.</param>
        /// <param name="checksum">A 16-byte string which is a MD5 checksum of the bytes of the file</param>
        public void AddEmbeddedFile(string name, string path, string? checksum = null)
        {
            var stream = new FileStream(path, FileMode.Open);
            AddEmbeddedFile(name, stream, checksum);
        }

        /// <summary>
        /// Adds an embedded file to the document.
        /// </summary>
        /// <param name="name">The name used to refer and to entitle the embedded file.</param>
        /// <param name="stream">The stream containing the file to embed.</param>
        /// <param name="checksum">A 16-byte string which is a MD5 checksum of the bytes of the file</param>
        public void AddEmbeddedFile(string name, Stream stream, string? checksum = null)
            => Internals.Catalog.Names.AddEmbeddedFile(name, stream, checksum);

        /// <summary>
        /// Flattens a document (make the fields non-editable).
        /// </summary>
        public void Flatten()
        {
            for (int idx = 0; idx < AcroForm.Fields.Count; idx++)
            {
                AcroForm.Fields[idx].ReadOnly = true;
            }
        }

        /// <summary>
        /// Gets the standard security handler and creates it, if not existing.
        /// </summary>
        public PdfStandardSecurityHandler SecurityHandler => Trailer.SecurityHandler;

        /// <summary>
        /// Gets the standard security handler, if existing and encryption is active.
        /// </summary>
        internal PdfStandardSecurityHandler? EffectiveSecurityHandler => Trailer.EffectiveSecurityHandler;

        internal PdfTrailer Trailer { get; set; } = default!;

        internal PdfCrossReferenceTable IrefTable { get; set; } = default!;

        internal Stream? OutStream { get; set; }

        // Imported Document.
        internal Lexer? _lexer;

        internal DateTime _creation;

        /// <summary>
        /// Occurs when the specified document is not used anymore for importing content.
        /// </summary>
        internal void OnExternalDocumentFinalized(PdfDocument.DocumentHandle handle)
        {
            //PdfDocument[] documents = tls.Documents;
            tls?.DetachDocument(handle);

            _formTable?.DetachDocument(handle);
        }

        //internal static GlobalObjectTable Gob = new GlobalObjectTable();

        /// <summary>
        /// Gets the ThreadLocalStorage object. It is used for caching objects that should be created
        /// only once.
        /// </summary>
        internal static ThreadLocalStorage Tls => tls ??= new ThreadLocalStorage();

        [ThreadStatic] static ThreadLocalStorage? tls;

        [DebuggerDisplay("(ID={ID}, alive={IsAlive})")]
        internal class DocumentHandle
        {
            public DocumentHandle(PdfDocument document)
            {
                _weakRef = new WeakReference(document);
                ID = document._guid.ToString("B").ToUpper();
            }

            public bool IsAlive => _weakRef.IsAlive;

            public PdfDocument? Target => _weakRef.Target as PdfDocument;

            readonly WeakReference _weakRef;

            public string ID;

            public override bool Equals(object? obj)
            {
                if (obj is DocumentHandle handle)
                    return ID == handle.ID;
                return false;
            }

            public override int GetHashCode()
                => ID.GetHashCode();

            public static bool operator ==(DocumentHandle? left, DocumentHandle? right)
            {
                if (left is null)
                    return right is null;
                return left.Equals(right);
            }

            public static bool operator !=(DocumentHandle? left, DocumentHandle? right)
                => !(left == right);
        }

#pragma warning disable CS0649
        internal UAManager? _uaManager;
    }
}
