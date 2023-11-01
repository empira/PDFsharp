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
                string realPath = Drawing.XPdfForm.ExtractPageNumber(path, out var pageNumber);
                if (File.Exists(realPath)) // Prevent unwanted exceptions during debugging.
                {
                    stream = new FileStream(realPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] bytes = new byte[1024];
                    stream.Read(bytes, 0, 1024);
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
                stream.Read(bytes, 0, 1024);
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
                if (document != null)
                {
                    document._fullPath = Path.GetFullPath(path);
                }
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
                stream.Read(header, 0, 1024);
                document._version = GetPdfFileVersion(header);
                if (document._version == 0)
                    throw new InvalidOperationException(PSSR.InvalidPdf);

                document.IrefTable.IsUnderConstruction = true;
                Parser parser = new Parser(document);
                // Read all trailers or cross-reference streams, but no objects.
                document.Trailer = parser.ReadTrailer();
                if (document.Trailer == null)
                    ParserDiagnostics.ThrowParserException("Invalid PDF file: no trailer found."); // TODO L10N using PSSR.

                Debug.Assert(document.IrefTable.IsUnderConstruction);
                document.IrefTable.IsUnderConstruction = false;

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
                            PdfPasswordProviderArgs args = new PdfPasswordProviderArgs();
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

                PdfReference[] irefs2 = document.IrefTable.AllReferences;
                int count2 = irefs2.Length;

                // The Pdf-Reference 1.7 states in chapter 7.5.6 (Incremental Updates):
                // "When a conforming reader reads the file,
                //  it shall build its cross-reference information in such a way that the
                //  most recent copy of each object shall be the one accessed from the file."

                // IrefTable.AllReferences is sorted by ObjectId which gives older objects preference
                // (as they typically have lower ObjectIds)
                // for xref-streams, we revert the order, so that the most current object is read first
                // this is because Parser.ReadObject does not overwrite an object, that was already collected

                // collect xref streams
                var xrefStreams = new List<PdfCrossReferenceStream>();
                for (var idx = 0; idx < count2; idx++)
                {
                    PdfReference iref = irefs2[idx];
                    if (iref.Value is PdfCrossReferenceStream xrefStream)
                        xrefStreams.Add(xrefStream);
                }
                // sort them so the last xref stream is read first
                // TODO: is this always sufficient ? (haven't found any issues so far testing with ~1300 pdfs...)
                xrefStreams.Sort((a, b) => (b.Reference?.Position ?? 0) - (a.Reference?.Position ?? 0));

                // 3rd: Create iRefs for all compressed objects.
                Dictionary<int, object?> objectStreams = new();
                foreach (var xrefStream in xrefStreams)
                {
                    for (int idx2 = 0; idx2 < xrefStream.Entries.Count; idx2++)
                    {
                        PdfCrossReferenceStream.CrossReferenceStreamEntry item = xrefStream.Entries[idx2];
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
                        }
                    }
                }

                // 4th: Read compressed objects.
                foreach (var xrefStream in xrefStreams)
                {
                    for (int idx2 = 0; idx2 < xrefStream.Entries.Count; idx2++)
                    {
                        PdfCrossReferenceStream.CrossReferenceStreamEntry item = xrefStream.Entries[idx2];
                        // Is type xref to compressed object?
                        if (item.Type == 2)
                        {
                            PdfReference irefNew = parser.ReadCompressedObject(new PdfObjectID((int)item.Field2),
                                (int)item.Field3);
                            Debug.Assert(document.IrefTable.Contains(xrefStream.ObjectID));
                            Debug.Assert(document.IrefTable.Contains(irefNew.ObjectID));
                        }
                    }
                }

                PdfReference[] irefs = document.IrefTable.AllReferences;
                int count = irefs.Length;

                // Read all indirect objects.
                for (int idx = 0; idx < count; idx++)
                {
                    PdfReference iref = irefs[idx];
                    if (iref.Value == null!)
                    {
#if DEBUG_
                        if (iref.ObjectNumber == 1074)
                            iref.GetType();
#endif
                        try
                        {
                            Debug.Assert(document.IrefTable.Contains(iref.ObjectID));
                            PdfObject pdfObject = parser.ReadObject(null, iref.ObjectID, false, false);
                            Debug.Assert(pdfObject.Reference == iref);
                            pdfObject.Reference = iref;
                            Debug.Assert(pdfObject.Reference.Value != null, "Something went wrong.");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            // 4STLA rethrow exception to notify caller.
                            throw;
                        }
                    }
                    else
                    {
                        Debug.Assert(document.IrefTable.Contains(iref.ObjectID));
                        //iref.GetType();
                    }
                    // Set maximum object number.
                    document.IrefTable.MaxObjectNumber = Math.Max(document.IrefTable.MaxObjectNumber,
                        iref.ObjectNumber);
                }

                // Decrypt all objects.
                if (effectiveSecurityHandler != null)
                {
                    effectiveSecurityHandler.DecryptDocument();

                    // Reset encryption so that it must be redefined to save the document encrypted.
                    effectiveSecurityHandler.SetEncryptionToNoneAndResetPasswords();
                }

                // Fix references of trailer values and then objects and irefs are consistent.
                document.Trailer.Finish();

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
                    PdfPages pages = document.Pages;
                    Debug.Assert(pages != null);

                    //bool b = document.irefTable.Contains(new PdfObjectID(1108));
                    //b.GetType();

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

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        public static PdfDocument Open(Stream stream)
        {
            return Open(stream, PdfDocumentOpenMode.Modify);
        }
    }
}
