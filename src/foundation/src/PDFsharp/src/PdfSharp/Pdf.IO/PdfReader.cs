// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using PdfSharp.Internal;
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
    public static class PdfReader
    {
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
            catch { }
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
            catch { }
            finally
            {
                try
                {
                    if (pos != -1)
                        stream.Position = pos;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }
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
        {
            return GetPdfFileVersion(data);
        }

        /// <summary>
        /// Implements scanning the PDF file version.
        /// </summary>
        internal static int GetPdfFileVersion(byte[] bytes)
        {
            try
            {
                // Acrobat accepts headers like «%!PS-Adobe-N.n PDF-M.m»...
                var header = PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);  // Encoding.ASCII.GetString(bytes);
                if (header[0] == '%' || header.IndexOf("%PDF", StringComparison.Ordinal) >= 0)
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
            catch { }
            return 0;
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, PdfDocumentOpenMode openMode)
        {
            return Open(path, null, openMode, null);
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, PdfDocumentOpenMode openMode, PdfPasswordProvider provider)
        {
            return Open(path, null, openMode, provider);
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, string password, PdfDocumentOpenMode openMode)
        {
            return Open(path, password, openMode, null);
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, string? password, PdfDocumentOpenMode openMode, PdfPasswordProvider? provider)
        {
            PdfDocument document;
            Stream? stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                document = Open(stream, password, openMode, provider);
                if (document != null!)
                    document._fullPath = Path.GetFullPath(path);
            }
            finally
            {
                stream?.Close();
            }
            return document;

        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path)
        {
            return Open(path, null, PdfDocumentOpenMode.Modify, null);
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(string path, string password)
        {
            return Open(path, password, PdfDocumentOpenMode.Modify, null);
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, PdfDocumentOpenMode openMode)
        {
            return Open(stream, null, openMode);
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, PdfDocumentOpenMode openMode, PdfPasswordProvider passwordProvider)
        {
            return Open(stream, null, openMode, passwordProvider);
        }
        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, string? password, PdfDocumentOpenMode openMode)
        {
            return Open(stream, password, openMode, null);
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream, string? password, PdfDocumentOpenMode openMode, PdfPasswordProvider? passwordProvider)
        {
            PdfDocument document;
            try
            {
                var lexer = new Lexer(stream);
                document = new PdfDocument(lexer);
                document._state |= DocumentState.Imported;
                document._openMode = openMode;
                document._fileSize = stream.Length;

                // Get file version.
                byte[] header = new byte[1024];
                stream.Position = 0;
                var _ = stream.Read(header, 0, 1024);
                document._version = GetPdfFileVersion(header);
                if (document._version == 0)
                    throw new InvalidOperationException(PSSR.InvalidPdf);

                // Set IsUnderConstruction for IrefTable to true. This allows Parser.ParseObject() to insert placeholder references for objects not yet known.
                // This is necessary for documents with objects saved in objects streams, which are read and decoded after reading the file level PdfObjects.
                // After reading all objects, all documents placeholder references get replaced by references knowing their objects in FinishReferences(),
                // which finally sets IsUnderConstruction to false.
                document.IrefTable.IsUnderConstruction = true;
                var parser = new Parser(document);
                // Read all trailers or cross-reference streams, but no objects.
                document.Trailer = parser.ReadTrailer();
                if (document.Trailer == null!)
                    ParserDiagnostics.ThrowParserException("Invalid PDF file: no trailer found."); // TODO L10N using PSSR.

                // Is document encrypted?
                if (document.Trailer!.Elements[PdfTrailer.Keys.Encrypt] is PdfReference xrefEncrypt)
                {
                    //xrefEncrypt.Value = parser.ReadObject(null, xrefEncrypt.ObjectID, false);
                    var encrypt = parser.ReadObject(null, xrefEncrypt.ObjectID, false, false);
                    encrypt.Reference = xrefEncrypt;
                    xrefEncrypt.Value = encrypt;

                    document.SecurityHandler.PrepareForReading();
                }

                var effectiveSecurityHandler = document.EffectiveSecurityHandler;
                if (effectiveSecurityHandler != null)
                {
                    TryAgain:
                    PasswordValidity validity = effectiveSecurityHandler.ValidatePassword(password);
                    if (validity == PasswordValidity.Invalid)
                    {
                        if (passwordProvider != null)
                        {
                            var args = new PdfPasswordProviderArgs();
                            passwordProvider(args);
                            if (args.Abort)
                                return null!; // NRT THROW
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
                    else if (validity == PasswordValidity.UserPassword && openMode == PdfDocumentOpenMode.Modify)
                    {
                        if (passwordProvider != null)
                        {
                            var args = new PdfPasswordProviderArgs();
                            passwordProvider(args);
                            if (args.Abort)
                                return null!; // NRT THROW
                            password = args.Password;
                            goto TryAgain;
                        }
                        else
                            throw new PdfReaderException(PSSR.OwnerPasswordRequired);
                    }
                }
                else
                {
                    if (password != null)
                    {
                        // Password specified but document is not encrypted.
                        // ignore
                    }
                }

                // Read all file level indirect objects and decrypt them.
                // Objects stored in object streams are not yet included and must not be decrypted as they are not encrypted.
                ReadIndirectObjectsFromIrefTable(document, parser, true);

                // Read all indirect objects stored in object streams.
                ReadCompressedObjects(document, parser);

                // Read all not yet known indirect objects (the ones that were stored in object streams) and don't decrypt them, as they are not encrypted.
                ReadIndirectObjectsFromIrefTable(document, parser, false);

                // Reset encryption so that it must be redefined to save the document encrypted.
                effectiveSecurityHandler?.SetEncryptionToNoneAndResetPasswords();

                // Replace all document's placeholder references by references knowing their objects.
                // Placeholder references are used, when reading indirect objects referring objects stored in object streams before reading and decoding them.
                FinishReferences(document);
#if DEBUG_
    // Some tests...
                PdfReference[] reachables = document.xrefTable.TransitiveClosure(document.trailer);
                reachables.GetType();
                reachables = document.xrefTable.AllXRefs;
                document.xrefTable.CheckConsistence();
#endif
                if (openMode == PdfDocumentOpenMode.Modify)
                {
                    // Create new or change existing document IDs.
                    if (document.Internals.SecondDocumentID == "")
                        document.Trailer.CreateNewDocumentIDs();
                    else
                    {
                        byte[] agTemp = Guid.NewGuid().ToByteArray();
                        document.Internals.SecondDocumentID = PdfEncoders.RawEncoding.GetString(agTemp, 0, agTemp.Length);
                    }

                    // Change modification date.
                    document.Info.ModificationDate = DateTime.Now;

                    // Remove all unreachable objects.
                    int removed = document.IrefTable.Compact();
                    if (removed != 0)
                        Debug.WriteLine("Number of deleted unreachable objects: " + removed);

                    // Force flattening of page tree.
                    var pages = document.Pages;
                    Debug.Assert(pages != null);

                    document.IrefTable.CheckConsistence();
                    document.IrefTable.Renumber();
                    document.IrefTable.CheckConsistence();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            return document;
        }

        static void ReadIndirectObjectsFromIrefTable(PdfDocument document, Parser parser, bool decrypt)
        {
            var effectiveSecurityHandler = document.EffectiveSecurityHandler;

            foreach (var iref in document.IrefTable.AllReferences)
            {
                if (iref.Value == null!)
                {
                    try
                    {
                        Debug.Assert(document.IrefTable.Contains(iref.ObjectID));
                        var pdfObject = parser.ReadObject(null, iref.ObjectID, false, false);
                        Debug.Assert(pdfObject.Reference == iref);
                        pdfObject.Reference = iref;
                        Debug.Assert(pdfObject.Reference.Value != null, "Something went wrong.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        // Rethrow exception to notify caller.
                        throw;
                    }
                }
                else
                {
                    Debug.Assert(document.IrefTable.Contains(iref.ObjectID));
                }

                // Decrypt object, if needed.
                if (decrypt && iref.Value is { } pdfObject2)
                    effectiveSecurityHandler?.DecryptObject(pdfObject2);

                // Set maximum object number.
                document.IrefTable.MaxObjectNumber = Math.Max(document.IrefTable.MaxObjectNumber, iref.ObjectNumber);
            }
        }

        static void ReadCompressedObjects(PdfDocument document, Parser parser)
        {
            // The PDF Reference 1.7 states in chapter 7.5.6 (Incremental Updates):
            // "When a conforming reader reads the file,
            //  it shall build its cross-reference information in such a way that the
            //  most recent copy of each object shall be the one accessed from the file."

            // IrefTable.AllReferences is sorted by ObjectId which gives older objects preference
            // (as they typically have lower ObjectIds).
            // For xref-streams, we revert the order, so that the most current object is read first.
            // This is because Parser.ReadObject does not overwrite an object that was already collected.

            // Collect xref streams.
            var xrefStreams = new List<PdfCrossReferenceStream>();
            foreach (var iref in document.IrefTable.AllReferences)
            {
                if (iref.Value is PdfCrossReferenceStream xrefStream)
                    xrefStreams.Add(xrefStream);
            }
            // Sort them so the last xref stream is read first.
            // TODO: Is this always sufficient? (haven't found any issues so far testing with ~1300 PDFs...)
            xrefStreams.Sort((a, b) => (b.Reference?.Position ?? 0) - (a.Reference?.Position ?? 0));



            Dictionary<int, object?> objectStreams = new();
            foreach (var xrefStream in xrefStreams)
            {
                foreach (var item in xrefStream.Entries)
                {
                    // Is type xref to compressed object?
                    if (item.Type == 2)
                    {
                        int objectNumber = (int)item.Field2;

                        if (!objectStreams.ContainsKey(objectNumber))
                        {
                            objectStreams.Add(objectNumber, null);
                            var objectID = new PdfObjectID((int)item.Field2);
                            parser.ReadIRefsFromCompressedObject(objectID);
                        }

                        PdfReference irefNew = parser.ReadCompressedObject(new PdfObjectID((int)item.Field2),
                            (int)item.Field3);
                        Debug.Assert(document.IrefTable.Contains(xrefStream.ObjectID));
                        Debug.Assert(document.IrefTable.Contains(irefNew.ObjectID));
                    }
                }
            }
        }

        static void FinishReferences(PdfDocument document)
        {
            Debug.Assert(document.IrefTable.IsUnderConstruction);

            var finishedObjects = new HashSet<PdfObject>();

            foreach (var iref in document.IrefTable.AllReferences)
            {
                Debug.Assert(iref.Value != null, "All references saved in IrefTable should have been created when their referred PdfObject has been accessible.");

                // Get and update object's references.
                FinishItemReferences(iref.Value, document, finishedObjects);
            }

            document.IrefTable.IsUnderConstruction = false;

            // Fix references of trailer values and then objects and irefs are consistent.
            document.Trailer.Finish();
        }

        static void FinishItemReferences(PdfItem? pdfItem, PdfDocument document, HashSet<PdfObject> finishedObjects)
        {
            // Only PdfObjects may contain further PdfReferences.
            if (pdfItem is not PdfObject pdfObject)
                return;

            // Return, if this object is already processed.
            if (finishedObjects.Contains(pdfObject))
                return;

            // Mark object as processed.
            finishedObjects.Add(pdfObject);

            // For PdfDictionary and PdfArray, get and update child references.
            if (pdfObject is PdfDictionary childDictionary)
                FinishChildReferences(childDictionary, document, finishedObjects);
            if (pdfObject is PdfArray childArray)
                FinishChildReferences(childArray, document, finishedObjects);
        }

        static void FinishChildReferences(PdfDictionary dictionary, PdfDocument document, HashSet<PdfObject> finishedObjects)
        {
            foreach (var element in dictionary.Elements)
            {
                var item = element.Value;

                // For PdfReference: Update reference, if necessary, and continue with referred item.
                if (item is PdfReference iref)
                {
                    if (FinishReference(iref, document, out var newIref, out var value))
                        dictionary.Elements[element.Key] = newIref;
                    item = value;
                }

                // Get and update item's references.
                FinishItemReferences(item, document, finishedObjects);
            }
        }

        static void FinishChildReferences(PdfArray array, PdfDocument document, HashSet<PdfObject> finishedObjects)
        {
            var elements = array.Elements;
            for (var i = 0; i < elements.Count; i++)
            {
                var item = elements[i];

                // For PdfReference: Update reference, if necessary, and continue with referred item.
                if (item is PdfReference iref)
                {
                    if (FinishReference(iref, document, out var newIref, out var value))
                        elements[i] = newIref;
                    item = value;
                }

                // Get and update item's references.
                FinishItemReferences(item, document, finishedObjects);
            }
        }

        static bool FinishReference(PdfReference currentReference, PdfDocument document, out PdfItem actualReference, out PdfItem value)
        {
            var isChanged = false;
            PdfItem? reference = currentReference;

            // The value of the reference may be null.
            // If a file level PdfObject refers object stream level PdfObjects, that were not yet decompressed when reading it,
            // placeholder references are used. 
            if (reference is PdfReference { Value: null })
            {
                // Read the reference for the ObjectID of the placeholder reference from IrefTable, which should contain the value.
                var newIref = document.IrefTable[currentReference.ObjectID];
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
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream)
        {
            return Open(stream, PdfDocumentOpenMode.Modify);
        }
    }
}
