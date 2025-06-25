// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Events;
using PdfSharp.Fonts.Internal;
using PdfSharp.Logging;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Signatures;
using PdfSharp.Pdf.Structure;
using PdfSharp.UniversalAccessibility;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertPropertyToExpressionBody

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF document.
    /// </summary>
    [DebuggerDisplay("(Name={" + nameof(Name) + "})")]  // A name makes debugging easier.
    public sealed class PdfDocument : PdfObject, IDisposable
    {
        /// <summary>
        /// Creates a new PDF document in memory.
        /// To open an existing PDF file, use the PdfReader class.
        /// </summary>
        public PdfDocument()
        {
            PdfSharpLogHost.Logger.PdfDocumentCreated(Name);
            //PdfDocument.Gob.AttachDocument(Handle);
            _document = this;
            _creation = DateTime.Now;
            _state = DocumentState.Created;
            _version = 17;
            Initialize();
            Info.CreationDate = _creation;
        }

        /// <summary>
        /// Creates a new PDF document with the specified file name. The file is immediately created and kept
        /// locked until the document is closed. At that time the document is saved automatically.
        /// Do not call Save for documents created with this constructor, just call Close.
        /// To open an existing PDF file and import it, use the PdfReader class.
        /// </summary>
        public PdfDocument(string outputFilename) : this()
        {
            OutStream = new FileStream(outputFilename, FileMode.Create);
        }

        /// <summary>
        /// Creates a new PDF document using the specified stream.
        /// The stream won’t be used until the document is closed. At that time the document is saved automatically.
        /// Do not call Save for documents created with this constructor, just call Close.
        /// To open an existing PDF file, use the PdfReader class.
        /// </summary>
        public PdfDocument(Stream outputStream)
        {
            _document = this;
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

            _document = this;
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

        /// <summary>
        /// Why we need XML documentation here?
        /// </summary>
        ~PdfDocument()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes all references to this document stored in other documents. This function should be called
        /// for documents you finished importing pages from. Calling Dispose is technically not necessary but
        /// useful for earlier reclaiming memory of documents you do not need anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            _state = DocumentState.Disposed | DocumentState.Saved;
        }

        /// <summary>
        /// Gets or sets a user-defined object that contains arbitrary information associated with this document.
        /// The tag is not used by PDFsharp.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Temporary hack to set a value that tells PDFsharp to create a PDF/A conform document.
        /// </summary>
        public void SetPdfA()  // HACK_OLD
        {
            _isPdfA = true;
            _ = UAManager.ForDocument(this);
        }

        /// <summary>
        /// Gets a value indicating that you create a PDF/A conform document.
        /// This function is temporary and will change in the future.
        /// </summary>
        public bool IsPdfA => _isPdfA;  // HACK_OLD
        bool _isPdfA;

        /// <summary>
        /// Encapsulates the document’s events.
        /// </summary>
        public DocumentEvents Events => _documentEvents ??= new();
        DocumentEvents? _documentEvents;

        /// <summary>
        /// Encapsulates the document’s render events.
        /// </summary>
        public RenderEvents RenderEvents => _renderEvents ??= new();
        RenderEvents? _renderEvents;

        /// <summary>
        /// Gets or sets a value used to distinguish PdfDocument objects.
        /// The name is not used by PDFsharp.
        /// </summary>
        internal string Name { get; set; } = NewName();

        /// <summary>
        /// Get a new default name for a new document.
        /// </summary>
        static string NewName()
        {
#if DEBUG_
            if (PdfDocument.nameCount == 57)
                PdfDocument.nameCount.GetType();
#endif
            return "Document #" + ++_nameCount;
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
            if (OutStream != null)
            {
                if (!CanModify)
                    throw new InvalidOperationException(PsMsgs.CannotModify);

                EnsureNotYetSaved();

                // Get security handler if document gets encrypted.
                var effectiveSecurityHandler = SecuritySettings.EffectiveSecurityHandler;

                var writer = new PdfWriter(OutStream, _document, effectiveSecurityHandler);
                try
                {
                    DoSaveAsync(writer).GetAwaiter().GetResult();
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
            // Safely call the async version on the current thread.
            SaveAsync(path).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Saves the document async to the specified path. If a file already exists, it will be overwritten.
        /// The async version of save is useful if you want to create a signed PDF file with a time stamp.
        /// A time stamp server should be accessed asynchronously, and therefore we introduced this function.
        /// </summary>
        public async Task SaveAsync(string path)
        {
            EnsureNotYetSaved();

            if (!CanModify)
                throw new InvalidOperationException(PsMsgs.CannotModify);

            // We need ReadWrite when adding a signature. Write is sufficient if not adding a signature.
            var fileAccess = _digitalSignatureHandler == null ? FileAccess.Write : FileAccess.ReadWrite;

            // ReSharper disable once UseAwaitUsing because we need no DisposeAsync for a simple FileStream.
            using var stream = new FileStream(path, FileMode.Create, fileAccess, FileShare.None);
            await SaveAsync(stream).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves the document to the specified stream.
        /// </summary>
        public void Save(Stream stream, bool closeStream = false)
        {
            // Safely call the async version on the current thread.
            SaveAsync(stream, closeStream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Saves the document to the specified stream.
        /// The async version of save is useful if you want to create a signed PDF file with a time stamp.
        /// A time stamp server should be accessed asynchronously, and therefore we introduced this function.
        /// </summary>
        public async Task SaveAsync(Stream stream, bool closeStream = false)
        {
            EnsureNotYetSaved();

            if (!stream.CanWrite)
                throw new InvalidOperationException(PsMsgs.StreamMustBeWritable);

            if (!CanModify)
                throw new InvalidOperationException(PsMsgs.CannotModify);

            // #PDF-A
            if (IsPdfA)
                PrepareForPdfA();

            // TODO_OLD: more diagnostic checks
            string message = "";
            if (!CanSave(ref message))
                throw new PdfSharpException(message);

            // Get security handler if document gets encrypted.
            var effectiveSecurityHandler = SecuritySettings.EffectiveSecurityHandler;

            PdfWriter? writer = null;
            try
            {
                Debug.Assert(ReferenceEquals(_document, this));
                writer = new(stream, _document, effectiveSecurityHandler);
                await DoSaveAsync(writer).ConfigureAwait(false);
            }
            finally
            {
                if (stream != null!)
                {
                    if (closeStream)
                    {
                        stream.Close();
                    }
                    else
                    {
                        if (stream is { CanRead: true, CanSeek: true })
                            stream.Position = 0; // Reset the stream position if the stream is kept open.
                    }
                }
                writer?.Close(closeStream);
            }
        }

        /// <summary>
        /// Implements saving a PDF file.
        /// </summary>
        async Task DoSaveAsync(PdfWriter writer)
        {
            PdfSharpLogHost.Logger.PdfDocumentSaved(Name);

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
                // Prepare for signing.
                if (_digitalSignatureHandler != null)
                    await _digitalSignatureHandler.AddSignatureComponentsAsync().ConfigureAwait(false);

                // Remove XRefTrailer
                if (Trailer is PdfCrossReferenceStream crossReferenceStream)
                {
                    Trailer = new PdfTrailer(crossReferenceStream);
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
                foreach(var iref in irefs.Where(x=>x.Value is not PdfPages))
                {
                    iref.Position = writer.Position;
                    iref.Value.WriteObject(writer);
                }
                foreach (var iref in irefs.Where(x => x.Value is PdfPages))
                {
                    iref.Position = writer.Position;
                    iref.Value.WriteObject(writer);
                }
                // ReSharper disable once RedundantCast. Redundant only if 64 bit.
                var startXRef = (SizeType)writer.Position;
                IrefTable.WriteObject(writer);
                writer.WriteRaw("trailer");
                writer.WriteRaw(Options.LineEnding);
                Trailer.Elements.SetInteger("/Size", count + 1);
                Trailer.WriteObject(writer);
                writer.WriteEof(this, startXRef);

                // #Signature: What about encryption + signing ??
                // Prepare for signing.
                if (_digitalSignatureHandler != null)
                    await _digitalSignatureHandler.ComputeSignatureAndRange(writer).ConfigureAwait(false);

                //if (encrypt)
                //{
                //  state &= ~DocumentState.SavingEncrypted;
                //  //_securitySettings.SecurityHandler.EncryptDocument();
                //}
            }
            finally
            {
                //await writer.Stream.FlushAsync().ConfigureAwait(false);
                writer.Stream.Flush();
                // Do not close the stream writer here.
                _state |= DocumentState.Saved;
            }
        }

        void PrepareForPdfA()  // Just a first hack.
        {
            var internals = Internals;

            Debug.Assert(_uaManager != null);
            // UAManager sets MarkInformation.
            if (_uaManager == null)
            {
                // Marked must be true in MarkInfo.
                var markInfo = new PdfMarkInformation();
                //internals.AddObject(markInfo);

                markInfo.Elements.SetBoolean(PdfMarkInformation.Keys.Marked, true);
                //internals.Catalog.Elements.SetReference(PdfCatalog.Keys.MarkInfo, markInfo);
                internals.Catalog.Elements.Add(PdfCatalog.Keys.MarkInfo, markInfo);
            }

            var outputIntentsArray = new PdfArray(this);
            //internals.AddObject(outputIntentsArray);
            var outputIntents = new PdfDictionary(this);
            outputIntentsArray.Elements.Add(outputIntents);

            outputIntents.Elements.Add("/Type", new PdfName("/OutputIntent"));
            outputIntents.Elements.Add("/S", new PdfName("/GTS_PDFA1"));
            outputIntents.Elements.Add("/OutputConditionIdentifier", new PdfString("sRGB"));
            outputIntents.Elements.Add("/RegistryName", new PdfString("http://www.color.org"));
            outputIntents.Elements.Add("/Info", new PdfString("Creator: ColorOrg     Manufacturer:IEC    Model:sRGB"));

            var profileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfSharp.Resources.sRGB2014.icc")
                ?? throw new InvalidOperationException("Embedded color profile was not found.");

            var profile = new byte[profileStream.Length];
            var read = profileStream.Read(profile, 0, (int)profileStream.Length);
            if (read != profileStream.Length)
                throw new InvalidOperationException("Embedded color profile was not read.");

            var fd = new FlateDecode();
            byte[] profileCompressed = fd.Encode(profile, Options.FlateEncodeMode);

            var profileObject = new PdfDictionary(this);
            IrefTable.Add(profileObject);
            profileObject.Stream = new PdfDictionary.PdfStream(profileCompressed, profileObject);
            profileObject.Elements["/N"] = new PdfInteger(3);
            profileObject.Elements["/Length"] = new PdfInteger(profileCompressed.Length);
            profileObject.Elements["/Filter"] = new PdfName("/FlateDecode");

            outputIntents.Elements.Add("/DestOutputProfile", profileObject.Reference);
            //internals.Catalog.Elements.SetReference(PdfCatalog.Keys.OutputIntents, outputIntentsArray);
            internals.Catalog.Elements.Add(PdfCatalog.Keys.OutputIntents, outputIntentsArray);
        }

        /// <summary>
        /// Dispatches PrepareForSave to the objects that need it.
        /// </summary>
        internal override void PrepareForSave()
        {
            PdfDocumentInformation info = Info;

            // The Creator is called 'Application' in Acrobat.
            // The Producer is call "Created by" in Acrobat.

            // Set Creator if value is undefined. This is the 'application' in Adobe Reader.
            if (info.Elements[PdfDocumentInformation.Keys.Creator] is null)
                info.Creator = PdfSharpProductVersionInformation.Producer;

            // We set Producer if it is not yet set.
            var pdfProducer = PdfSharpProductVersionInformation.Creator;
#if DEBUG
            // Add OS suffix only in DEBUG build.
            pdfProducer += $" under {RuntimeInformation.OSDescription}";
#endif
            // Keep original producer if file was imported. This is 'PDF created by' in Adobe Reader.
            string producer = info.Producer;
            if (producer.Length == 0)
            {
                producer = pdfProducer;
            }
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
            if (removed != 0 && PdfSharpLogHost.Logger.IsEnabled(LogLevel.Information))
            {
                PdfSharpLogHost.Logger.LogInformation($"PrepareForSave: Number of deleted unreachable objects: {removed}");
            }
            IrefTable.Renumber();
#endif

            // #PDF-UA
            // Create PdfMetadata now to include the final document information in XMP generation.
            if (Options.EnableImplicitMetadata)
                Catalog.Elements.SetReference(PdfCatalog.Keys.Metadata, new PdfMetadata(this));
        }

        /// <summary>
        /// Determines whether the document can be saved.
        /// </summary>
        public bool CanSave(ref string message)
        {
            if (!SecuritySettings.CanSave(ref message))
                return false;

            if ((_state & DocumentState.Saved) != 0)
                return false;

            return true;
        }

        internal bool HasVersion(string version)
            => String.CompareOrdinal(Catalog.Version, version) >= 0;

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

        ///// <summary>
        ///// NYI Indicates whether large objects are written immediately to the output stream to reduce
        ///// memory consumption.
        ///// </summary>
        //internal bool EarlyWrite => false;

        /// <summary>
        /// Gets or sets the PDF version number as integer. Return value 14 e.g. means PDF 1.4 / Acrobat 5 etc.
        /// </summary>
        public int Version
        {
            get => _version;
            set
            {
                EnsureNotYetSaved();

                if (!CanModify)
                    throw new InvalidOperationException(PsMsgs.CannotModify);
                if (value is < 12 or > 20) // TODO_OLD not really implemented
                    throw new ArgumentException(PsMsgs.InvalidVersionNumber(value), nameof(value));
                _version = value;
            }
        }
        internal int _version;

        /// <summary>
        /// Adjusts the version if the current version is lower than the required version.
        /// Version is not adjusted for inconsistent files in ReadOnly mode.
        /// </summary>
        /// <param name="requiredVersion">The minimum version number to set version to.</param>
        /// <returns>True, if Version was modified.</returns>
        public bool SetRequiredVersion(int requiredVersion)
        {
            EnsureNotYetSaved();

            if (requiredVersion > Version && CanModify)
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
                EnsureNotYetSaved();

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
        public long FileSize { get; internal set; }

        /// <summary>
        /// Gets the full qualified file name if the document was read form a file, or an empty string otherwise.
        /// </summary>
        public string FullPath { get; internal set; } = "";

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

        //internal Exception DocumentNotImported()
        //{
        //    return new InvalidOperationException("Document not imported.");
        //}

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
                EnsureNotYetSaved();
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
                    throw new InvalidOperationException(PsMsgs.CannotModify);
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
                    throw new InvalidOperationException(PsMsgs.CannotModify);
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
            => _securitySettings ??= new(this);
        internal PdfSecuritySettings? _securitySettings;

        /// <summary>
        /// Adds characters whose glyphs have to be embedded in the PDF file.
        /// By default, PDFsharp only embeds glyphs of a font that are used for drawing text
        /// on a page. With this function actually unused glyphs can be added. This is useful
        /// for PDF that can be modified or has text fields. So all characters that can be
        /// potentially used are available in the PDF document.
        /// </summary>
        /// <param name="font">The font whose glyph should be added.</param>
        /// <param name="chars">A string with all unicode characters that should be added.</param>
        public void AddCharacters(XFont font, string chars)
        {
            // Get or create PDF font with glyph encoding.
            var pdfFont = FontTable.GetOrCreateFont(font.GlyphTypeface, FontType.Type0Unicode);
            var codePoints = UnicodeHelper.Utf32FromString(chars);
            var otDescriptor = font.OpenTypeDescriptor;
            var codePointsWithGlyphIndices = otDescriptor.GlyphIndicesFromCodePoints(codePoints);
            pdfFont.AddChars(codePointsWithGlyphIndices);
        }

        /// <summary>
        /// Gets the document font table that holds all fonts used in the current document.
        /// </summary>
        internal PdfFontTable FontTable
            => _fontTable ??= new(this);
        PdfFontTable? _fontTable;

        /// <summary>
        /// Gets the document image table that holds all images used in the current document.
        /// </summary>
        internal PdfImageTable ImageTable
            => _imageTable ??= new(this);
        PdfImageTable? _imageTable;

        /// <summary>
        /// Gets the document form table that holds all form external objects used in the current document.
        /// </summary>
        internal PdfFormXObjectTable FormTable  // TODO_OLD: Rename to ExternalDocumentTable.
            => _formTable ??= new(this);
        PdfFormXObjectTable? _formTable;

        /// <summary>
        /// Gets the document ExtGState table that holds all form state objects used in the current document.
        /// </summary>
        internal PdfExtGStateTable ExtGStateTable
            => _extGStateTable ??= new(this);
        PdfExtGStateTable? _extGStateTable;

        /// <summary>
        /// Gets the document PdfFontDescriptorCache that holds all PdfFontDescriptor objects used in the current document.
        /// </summary>
        internal PdfFontDescriptorCache PdfFontDescriptorCache
            => _pdfFontDescriptorCache ??= new(this);
        PdfFontDescriptorCache? _pdfFontDescriptorCache;

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
            EnsureNotYetSaved();

            if (!CanModify)
                throw new InvalidOperationException(PsMsgs.CannotModify);
            return Catalog.Pages.Add();
        }

        /// <summary>
        /// Adds the specified page to this document. If the page is from an external document,
        /// it is imported to this document. In this case the returned page is not the same
        /// object as the specified one.
        /// </summary>
        public PdfPage AddPage(PdfPage page)
        {
            EnsureNotYetSaved();

            if (!CanModify)
                throw new InvalidOperationException(PsMsgs.CannotModify);
            return Catalog.Pages.Add(page);
        }

        /// <summary>
        /// Creates a new page and inserts it in this document at the specified position.
        /// </summary>
        public PdfPage InsertPage(int index)
        {
            EnsureNotYetSaved();

            if (!CanModify)
                throw new InvalidOperationException(PsMsgs.CannotModify);
            return Catalog.Pages.Insert(index);
        }

        /// <summary>
        /// Inserts the specified page in this document. If the page is from an external document,
        /// it is imported to this document. In this case the returned page is not the same
        /// object as the specified one.
        /// </summary>
        public PdfPage InsertPage(int index, PdfPage page)
        {
            EnsureNotYetSaved();

            if (!CanModify)
                throw new InvalidOperationException(PsMsgs.CannotModify);
            return Catalog.Pages.Insert(index, page);
        }

        /// <summary>
        /// Adds a named destination to the document.
        /// </summary>
        /// <param name="destinationName">The Named Destination’s name.</param>
        /// <param name="destinationPage">The page to navigate to.</param>
        /// <param name="parameters">The PdfNamedDestinationParameters defining the named destination’s parameters.</param>
        public void AddNamedDestination(string destinationName, int destinationPage, PdfNamedDestinationParameters parameters)
            => Internals.Catalog.Names.AddNamedDestination(destinationName, destinationPage, parameters);

        /// <summary>
        /// Adds an embedded file to the document.
        /// </summary>
        /// <param name="name">The name used to refer and to entitle the embedded file.</param>
        /// <param name="path">The path of the file to embed.</param>
        public void AddEmbeddedFile(string name, string path)
        {
            var stream = new FileStream(path, FileMode.Open);
            AddEmbeddedFile(name, stream);
        }

        /// <summary>
        /// Adds an embedded file to the document.
        /// </summary>
        /// <param name="name">The name used to refer and to entitle the embedded file.</param>
        /// <param name="stream">The stream containing the file to embed.</param>
        public void AddEmbeddedFile(string name, Stream stream)
            => Internals.Catalog.Names.AddEmbeddedFile(name, stream);

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

#if true
        /// <summary>
        /// Gets the ThreadLocalStorage object. It is used for caching objects that should be created
        /// only once.
        /// </summary>
        internal static ThreadLocalStorage Tls => tls ??= new ThreadLocalStorage();

        [ThreadStatic] static ThreadLocalStorage? tls;
#endif

        [DebuggerDisplay("(ID={ID}, alive={IsAlive})")]
        internal class DocumentHandle(PdfDocument document)
        {
            public bool IsAlive => _weakRef.IsAlive;

            public PdfDocument? Target => _weakRef.Target as PdfDocument;

            readonly WeakReference _weakRef = new(document);

            public readonly string ID = document._guid.ToString("B").ToUpper();




            public override bool Equals(object? obj)
            {
                if (obj is DocumentHandle handle)
                    return ID == handle.ID;
                return false;
            }

            public override int GetHashCode() => ID.GetHashCode();

            public static bool operator ==(DocumentHandle? left, DocumentHandle? right)
            {
                if (left is null)
                    return right is null;
                return left.Equals(right);
            }

            public static bool operator !=(DocumentHandle? left, DocumentHandle? right)
                => !(left == right);
        }

        internal void EnsureNotYetSaved()
        {
            if ((_state & DocumentState.Saved) == 0)
                return;

            var message = "The document was already saved and cannot be modified anymore. " +
                          "Saving a document converts its in-memory representation into a PDF file or stream. " +
                          "This can only be done once. " +
                          "After that process the in-memory representation is outdated and protected against further modification.";
            throw new InvalidOperationException(message);
        }

        internal DocumentState _state;
        internal PdfDocumentOpenMode _openMode;
        internal UAManager? _uaManager;
        internal DigitalSignatureHandler? _digitalSignatureHandler;
    }

#if true_
    // UNDER_CONSTRUCTION
    static class PDFA_
    {
        public static bool IsPdfA => true;
    }
#endif
}
