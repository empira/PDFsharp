// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using PdfSharp.Internal;
using PdfSharp.Logging;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Encapsulates the arguments of the PdfPasswordProvider delegate.
    /// </summary>
    public class PdfPasswordProviderArgs
    {
        /// <summary>
        /// Sets the password to open the document with.
        /// </summary>
        public string Password = "";

        /// <summary>
        /// When set to true the PdfReader.Open function returns null indicating that no PdfDocument was created.
        /// </summary>
        public bool Abort;
    }

    /// <summary>
    /// A delegate used by the PdfReader.Open function to retrieve a password if the document is protected.
    /// </summary>
    public delegate void PdfPasswordProvider(PdfPasswordProviderArgs args);

    /// <summary>
    /// Represents the functionality for reading PDF documents.
    /// </summary>
    public sealed class PdfReader
    {
        PdfReader(ILogger? logger, PdfReaderOptions? options)
        {
            _logger = logger ?? PdfSharpLogHost.PdfReadingLogger;
            _options = options ?? new();
        }

        /// <summary>
        /// Determines whether the file specified by its path is a PDF file by inspecting the first eight
        /// bytes of the data. If the file header has the form «%PDF-x.y» the function returns the version
        /// number as integer (e.g. 14 for PDF 1.4). If the file header is invalid or inaccessible
        /// for any reason, 0 is returned. The function never throws an exception. 
        /// </summary>
        public static int TestPdfFile(string path)
        {

            FileStream? stream = null;
            try
            {
                string realPath = Drawing.XPdfForm.ExtractPageNumber(path, out _ /*var pageNumber*/);
                if (File.Exists(realPath)) // Prevent unwanted exceptions during debugging.
                {
                    stream = new FileStream(realPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] bytes = new byte[1024];
                    var _ = stream.Read(bytes, 0, 1024);
                    return GetPdfFileVersion(bytes);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            finally
            {
                try
                {
                    stream?.Close();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }

            return 0;
        }

        /// <summary>
        /// Determines whether the specified stream is a PDF file by inspecting the first eight
        /// bytes of the data. If the data begins with «%PDF-x.y» the function returns the version
        /// number as integer (e.g. 14 for PDF 1.4). If the data is invalid or inaccessible
        /// for any reason, 0 is returned. The function never throws an exception.
        /// This method expects the stream position to point to the start of the file data to be checked.
        /// </summary>
        public static int TestPdfFile(Stream stream)
        {
            long pos = -1;
            try
            {
                pos = stream.Position;
                byte[] bytes = new byte[1024];
                var _ = stream.Read(bytes, 0, 1024);
                return GetPdfFileVersion(bytes);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            finally
            {
                try
                {
                    if (pos != -1)
                        stream.Position = pos;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }

            return 0;
        }

        /// <summary>
        /// Determines whether the specified data is a PDF file by inspecting the first eight
        /// bytes of the data. If the data begins with «%PDF-x.y» the function returns the version
        /// number as integer (e.g. 14 for PDF 1.4). If the data is invalid or inaccessible
        /// for any reason, 0 is returned. The function never throws an exception. 
        /// </summary>
        public static int TestPdfFile(byte[] data)
            => GetPdfFileVersion(data);

        /// <summary>
        /// Implements scanning the PDF file version.
        /// </summary>
        internal static int GetPdfFileVersion(byte[] bytes)
        {
            try
            {
                // Acrobat accepts headers like «%!PS-Adobe-N.n PDF-M.m»...
                var header =
                    PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length); // Encoding.ASCII.GetString(bytes);
                if (header[0] == '%' || header.Contains("%PDF"))
                {
                    var ich = header.IndexOf("PDF-", StringComparison.Ordinal);
                    if (ich > 0 && header[ich + 5] == '.')
                    {
                        var major = header[ich + 4];
                        var minor = header[ich + 6];
                        if (Char.IsDigit(major) && Char.IsDigit(minor))
                        {
                            var version = (major - '0') * 10 + (minor - '0');
                            if (version is >= 10 and <= 20)
                                return version;
                        }
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }
            return 0;
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, PdfDocumentOpenMode openMode, PdfReaderOptions? options = null)
            => Open(path, null, openMode, null, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, PdfDocumentOpenMode openMode, PdfPasswordProvider passwordProvider,
            PdfReaderOptions? options = null)
            => Open(path, null, openMode, passwordProvider, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, string password, PdfDocumentOpenMode openMode,
            PdfReaderOptions? options = null)
            => Open(path, password, openMode, null, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, string? password, PdfDocumentOpenMode openMode,
            PdfPasswordProvider? passwordProvider, PdfReaderOptions? options = null)
        {
            var reader = new PdfReader(null, options);
            var document = reader.OpenFromFile(path, password, openMode, passwordProvider);
            return document;
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, PdfReaderOptions? options = null)
            => Open(path, null, PdfDocumentOpenMode.Modify, null, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, string password, PdfReaderOptions? options = null)
            => Open(path, password, PdfDocumentOpenMode.Modify, null, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream)
            => Open(stream, PdfDocumentOpenMode.Modify);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, PdfDocumentOpenMode openMode, PdfReaderOptions? options = null)
            => Open(stream, null, openMode, null, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, PdfDocumentOpenMode openMode,
            PdfPasswordProvider passwordProvider, PdfReaderOptions? options = null)
            => Open(stream, null, openMode, passwordProvider, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, string? password, PdfDocumentOpenMode openMode,
            PdfReaderOptions? options = null)
            => Open(stream, password, openMode, null, options);

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, string? password, PdfDocumentOpenMode openMode,
            PdfPasswordProvider? passwordProvider, PdfReaderOptions? options = null)
        {
            var reader = new PdfReader(null, options);
            var document = reader.OpenFromStream(stream, password, openMode, passwordProvider);
            return document;
        }

        /// <summary>
        /// Opens a PDF document from a file.
        /// </summary>
        PdfDocument OpenFromFile(string path, string? password, PdfDocumentOpenMode openMode, PdfPasswordProvider? passwordProvider)
        {
            PdfDocument document;
            try
            {
                using Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                document = OpenFromStream(stream, password, openMode, passwordProvider);

                // There is a case when document is legally null here.
                // If the PDF document is protected and the password provider sets Abort to true
                // in PdfPasswordProviderArgs. This is useful to probe a PDF document to be protected.
                if (document != null!)
                    document.FullPath = Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                PdfSharpLogHost.Logger.LogError(ex, "Open a PDF document failed.");
                throw;
            }
            return document!;
        }

        /// <summary>
        /// Opens a PDF document from a stream.
        /// </summary>
        PdfDocument OpenFromStream(Stream stream, string? password, PdfDocumentOpenMode openMode,
            PdfPasswordProvider? passwordProvider, PdfReaderOptions? options = null)
        {
            try
            {
#if !USE_LONG_SIZE
                if (/*sizeof(SizeType) == 4 &&*/ stream.Length > Int32.MaxValue)
                    throw new PdfReaderException(
                        $"PDF document with size {stream.Length} cannot be opened with a 32-bit size type. " +
                        "Recompile PDFsharp with USE_LONG_SIZE set in Directory.Build.targets");
#endif
                var lexer = new Lexer(stream, _logger);
                _document = new PdfDocument(lexer);
                _document._state |= DocumentState.Imported;
                _document._openMode = openMode;
                _document._fileSize = stream.Length;

                // Get file version.
                byte[] header = new byte[1024];
                stream.Position = 0;
                var _ = stream.Read(header, 0, 1024);
                _document._version = GetPdfFileVersion(header);
                if (_document._version == 0)
                    throw new InvalidOperationException(PSSR.InvalidPdf);

                // Set IsUnderConstruction for IrefTable to true. This allows Parser.ParseObject() to insert placeholder references for objects not yet known.
                // This is necessary for documents with objects saved in objects streams, which are read and decoded after reading the file level PdfObjects.
                // After reading all objects, all documents placeholder references get replaced by references knowing their objects in FinishReferences(),
                // which finally sets IsUnderConstruction to false.
                _document.IrefTable.IsUnderConstruction = true;
                var parser = new Parser(_document, options ?? new PdfReaderOptions(), _logger);

                // 1. Read all trailers or cross-reference streams, but no objects.
                _document.Trailer = parser.ReadTrailer();
                if (_document.Trailer == null!)
                    ParserDiagnostics.ThrowParserException(
                        "Invalid PDF file: no trailer found."); // TODO L10N using PSSR.
                // References available by now: All references to file-level objects.
                // Reference.Values available by now: All trailers and cross-reference streams (which are not encrypted by definition). 

                // 2. Read the encryption dictionary, if existing.
                if (_document.Trailer!.Elements[PdfTrailer.Keys.Encrypt] is PdfReference xrefEncrypt)
                {
                    var encrypt = parser.ReadIndirectObject(xrefEncrypt, null, true);
                    encrypt.Reference = xrefEncrypt;
                    xrefEncrypt.Value = encrypt;

                    _document.SecurityHandler.PrepareForReading();
                }
                // References available by now: All references to file-level objects.
                // Reference.Values available by now: All trailers and cross-reference streams and the encryption dictionary.

                // 3. Check password, if required.
                var effectiveSecurityHandler = _document.EffectiveSecurityHandler;
                if (effectiveSecurityHandler != null)
                {
                TryAgain: // ... after the password provider provides a valid password.
                    // ReSharper disable RedundantIfElseBlock to keep code more readable.
                    PasswordValidity validity = effectiveSecurityHandler.ValidatePassword(password);
                    if (validity == PasswordValidity.Invalid)
                    {
                        if (passwordProvider != null)
                        {
                            var args = new PdfPasswordProviderArgs();
                            passwordProvider(args);
                            if (args.Abort)
                                return null!; // null means protected document not read.
                            password = args.Password;
                            goto TryAgain;
                        }
                        else
                        {
                            if (password == null)
                                throw new PdfReaderException(PSSR.PasswordRequired);
                            else
                                throw new PdfReaderException(PSSR.InvalidPassword);
                        }
                    }
                    else if (validity == PasswordValidity.UserPassword 
                        && (openMode == PdfDocumentOpenMode.Modify || openMode == PdfDocumentOpenMode.Append))
                    {
                        if (passwordProvider != null)
                        {
                            var args = new PdfPasswordProviderArgs();
                            passwordProvider(args);
                            if (args.Abort)
                                return null!; // Indicate the document was not opened.
                            password = args.Password;
                            goto TryAgain;
                        }
                        else
                            throw new PdfReaderException(PSSR.OwnerPasswordRequired);
                    }
                    // ReSharper restore RedundantIfElseBlock
                }
                else
                {
                    if (password != null)
                    {
                        PdfSharpLogHost.Logger.LogWarning("Password specified but document is not encrypted.");
                        // Ignore the password.
                    }
                }

                // 4. Read all Objects streams and the references to the objects saved in them.
                parser.ReadAllObjectStreamsAndTheirReferences();
                // References available by now: All references (to file-level objects and to objects residing in object streams).
                // Reference.Values available by now: All trailers and cross-reference streams, the encryption dictionary and all object streams.

                // 5. Read and decrypt all remaining objects.
                parser.ReadAllIndirectObjects();
                // References available by now: All references.
                // Reference.Values available by now: All objects.

                // 6. Reset encryption so that it must be redefined to save the document encrypted.
                effectiveSecurityHandler?.SetEncryptionToNoneAndResetPasswords();

                // 7. Replace all document’s placeholder references by references knowing their objects.
                // Placeholder references are used, when reading indirect objects referring objects stored in object streams before reading and decoding them.
                FinishReferences();

                RereadUnicodeStrings();

#if DEBUG_ // TODO: Delete or rewrite.
                // Some tests...
                PdfReference[] reachables = document.xrefTable.TransitiveClosure(document.trailer);
                _ = typeof(int);
                reachables = document.xrefTable.AllXRefs;
                document.xrefTable.CheckConsistence();
#endif
                if (openMode == PdfDocumentOpenMode.Modify)
                {
                    // Create new or change existing document IDs.
                    if (_document.Internals.SecondDocumentID == "")
                        _document.Trailer.CreateNewDocumentIDs();
                    else
                    {
                        byte[] agTemp = Guid.NewGuid().ToByteArray();
                        _document.Internals.SecondDocumentID =
                            PdfEncoders.RawEncoding.GetString(agTemp, 0, agTemp.Length);
                    }

                    // Change modification date.
                    _document.Info.ModificationDate = DateTime.Now;

                    // Remove all unreachable objects.
                    int removed = _document.IrefTable.Compact();
                    if (removed != 0)
                    {
                        //Debug.WriteLine("Number of deleted unreachable objects: " + removed);
                        PdfSharpLogHost.PdfReadingLogger.LogInformation("Number of deleted unreachable objects: " + removed);
                    }

                    // Force flattening of page tree.
                    var pages = _document.Pages;
                    Debug.Assert(pages != null);

                    _document.IrefTable.CheckConsistence();
                    _document.IrefTable.Renumber();
                    _document.IrefTable.CheckConsistence();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reading PDF document failed.");
                throw;
            }
            return _document;
        }

        void FinishReferences()
        {
            Debug.Assert(_document.IrefTable.IsUnderConstruction);

            var finishedObjects = new HashSet<PdfObject>();

            foreach (var iref in _document.IrefTable.AllReferences)
            {
                Debug.Assert(iref.Value != null,
                    "All references saved in IrefTable should have been created when their referred PdfObject has been accessible.");

                // Get and update object’s references.
                FinishItemReferences(iref.Value, iref, _document, finishedObjects);
            }

            // why setting it here AND in Trailer.Finish ??
            _document.IrefTable.IsUnderConstruction = false;

            // Fix references of trailer values and then objects and irefs are consistent.
            _document.Trailer.Finish();

            Debug.Assert(_document.IrefTable.ModifiedObjects.Count == 0, "There should be no modified objects");
        }

        void FinishItemReferences(PdfItem? pdfItem, PdfReference itemReference, PdfDocument document, HashSet<PdfObject> finishedObjects)
        {
            // Only PdfObjects may contain further PdfReferences.
            if (pdfItem is not PdfObject pdfObject)
                return;
#if true
            // Try to add object to finished objects.
            // Return, if this object was already processed.
            if (!finishedObjects.Add(pdfObject))
                return;
#else
            // Return, if this object is already processed.
            if (finishedObjects.Contains(pdfObject))
                return;

            // Mark object as processed.
            finishedObjects.Add(pdfObject);
#endif

#if true
            // For PdfDictionary and PdfArray, get and update child references.
            switch (pdfObject)
            {
                case PdfDictionary childDictionary:
                    FinishChildReferences(childDictionary, childDictionary.Reference ?? itemReference, finishedObjects);
                    childDictionary.SetModified(false);
                    break;
                case PdfArray childArray:
                    FinishChildReferences(childArray, childArray.Reference ?? itemReference, finishedObjects);
                    childArray.SetModified(false);
                    break;
            }
#else
            // For PdfDictionary and PdfArray, get and update child references.
            if (pdfObject is PdfDictionary childDictionary)
                FinishChildReferences(childDictionary, document, finishedObjects);
            if (pdfObject is PdfArray childArray)
                FinishChildReferences(childArray, document, finishedObjects);
#endif
        }

        void FinishChildReferences(PdfDictionary dictionary, PdfReference containingReference, HashSet<PdfObject> finishedObjects)
        {
            if (dictionary.ObjectNumber == 15)
                GetType();
            if (dictionary.Reference is null && dictionary.ContainingReference is null)
                dictionary.ContainingReference = containingReference;

            // Dictionary elements are modified inside loop. Avoid "Collection was modified; enumeration operation may not execute" error occuring in net 4.7.2.
            // There is no way to access KeyValuePairs via index natively to use a for loop with.
            // Instead, enumerate Keys and get value via Elements[key], which shall be O(1).
            foreach (var key in dictionary.Elements.Keys)
            {
                var item = dictionary.Elements[key];

                // For PdfReference: Update reference, if necessary, and continue with referred item.
                if (item is PdfReference iref)
                {
                    if (FinishReference(iref, out var newIref, out var value))
                        dictionary.Elements[key] = newIref;
                    item = value;
                }

                // Get and update item’s references.
                FinishItemReferences(item, containingReference, _document, finishedObjects);
            }
        }

        void FinishChildReferences(PdfArray array, PdfReference containingReference, HashSet<PdfObject> finishedObjects)
        {
            if (array.Reference is null && array.ContainingReference is null)
                array.ContainingReference = containingReference;

            var elements = array.Elements;
            for (var i = 0; i < elements.Count; i++)
            {
                var item = elements[i];

                // For PdfReference: Update reference, if necessary, and continue with referred item.
                if (item is PdfReference iref)
                {
                    if (FinishReference(iref, out var newIref, out var value))
                        elements[i] = newIref;
                    item = value;
                }

                // Get and update item’s references.
                FinishItemReferences(item, containingReference, _document, finishedObjects);
            }
        }

        bool FinishReference(PdfReference currentReference, out PdfItem actualReference, out PdfItem value)
        {
            var isChanged = false;
            PdfItem? reference = currentReference;

            // The value of the reference may be null.
            // If a file level PdfObject refers object stream level PdfObjects, that were not yet decompressed when reading it,
            // placeholder references are used. 
            if (reference is PdfReference { Value: null })
            {
                // Read the reference for the ObjectID of the placeholder reference from IrefTable, which should contain the value.
                var newIref = _document.IrefTable[currentReference.ObjectID];
                reference = newIref;
                isChanged = true;
            }

            // PDF Reference 2.0 section 7.3.10:
            // An indirect reference to an undefined object shall not be considered an error by a PDF processor;
            // it shall be treated as a reference to the null object.
            if (reference is PdfReference { Value: null })
            {
                reference = PdfNull.Value;
                isChanged = true;
            }

            if (reference == null)
            {
                reference = PdfNull.Value;
                isChanged = true;
            }

            actualReference = reference;

            if (!isChanged)
                value = currentReference.Value;
            else if (actualReference is PdfReference r)
                value = r.Value;
            else
                value = PdfNull.Value;

            return isChanged;
        }

        /// <summary>
        /// Checks all PdfStrings and PdfStringObjects for valid BOMs and rereads them with the specified Unicode encoding.
        /// </summary>
        void RereadUnicodeStrings()
        {
            foreach (var pdfReference in _document.IrefTable.ObjectTable.Values)
            {
                var pdfObject = pdfReference.Value;
                RereadUnicodeStrings(pdfObject);
            }
        }

        static void RereadUnicodeStrings(PdfItem pdfItem)
        {
            switch (pdfItem)
            {
                case PdfArray pdfArray:
                    {
                        foreach (var childItem in pdfArray.Elements)
                            RereadUnicodeStrings(childItem);
                        break;
                    }
                case PdfDictionary pdfDictionary:
                    {
                        foreach (var childItem in pdfDictionary.Elements.Select(x => x.Value))
                            RereadUnicodeStrings(childItem!);
                        break;
                    }
                case PdfString pdfString:
                    pdfString.TryRereadAsUnicode();
                    break;
                case PdfStringObject pdfStringObject:
                    pdfStringObject.TryRereadAsUnicode();
                    break;
            }
        }

        PdfReaderOptions _options;
        PdfDocument _document = default!;
        readonly ILogger _logger;
    }
}
