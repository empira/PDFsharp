// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    partial class PdfDocument
    {
        /// <summary>
        /// Gets or sets the bytes of the file the document was read from.
        /// Only set if the document was opened with <see cref="PdfDocumentOpenMode.ModifyIncremental"/>,
        /// because only an incremental update needs them.
        /// </summary>
        /// <remarks>
        /// Known limitation: the whole file is kept in memory, because the stream it was read from is not
        /// necessarily open anymore when the document is saved. Opening a large file with
        /// <see cref="PdfDocumentOpenMode.ModifyIncremental"/> therefore needs the size of the file in
        /// addition to the memory the document needs anyway.
        /// </remarks>
        internal byte[]? OriginalBytes { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of all objects the document was read from the file with.
        /// Only set if the document was opened with <see cref="PdfDocumentOpenMode.ModifyIncremental"/>.
        /// Every object not contained here is a new object and therefore part of an incremental update.
        /// </summary>
        internal HashSet<PdfObjectID>? OriginalObjectIDs { get; set; }

        /// <summary>
        /// Declares an object as modified, so that it is written again by the next incremental update.
        /// </summary>
        /// <remarks>
        /// An incremental update writes only new and modified objects. PDFsharp cannot detect the modification
        /// of an object, therefore every modified object of the original file must be declared with this function.
        /// New objects are detected automatically and need not be declared.
        /// Calling this function has no effect if the document was not opened with
        /// <see cref="PdfDocumentOpenMode.ModifyIncremental"/>.
        /// </remarks>
        /// <param name="obj">The modified object. Direct objects are ignored, because they are written
        /// as part of the indirect object that contains them. Declare that object instead.</param>
        public void MarkAsModified(PdfObject obj)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            if (OpenMode != PdfDocumentOpenMode.ModifyIncremental)
                return;

            if (obj.Reference is null)
                return;

            _modifiedObjectIDs.Add(obj.Reference.ObjectID);
        }

        /// <summary>
        /// Saves the document as an incremental update to the specified path.
        /// If a file already exists, it will be overwritten.
        /// </summary>
        /// <param name="path">The path of the file to create.</param>
        public void SaveIncremental(string path)
        {
            // Safely call the async version on the current thread.
            SaveIncrementalAsync(path).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Saves the document async as an incremental update to the specified path.
        /// If a file already exists, it will be overwritten.
        /// The async version of save is useful if you want to create a signed PDF file with a time stamp.
        /// A time stamp server should be accessed asynchronously, and therefore we introduced this function.
        /// </summary>
        /// <param name="path">The path of the file to create.</param>
        public async Task SaveIncrementalAsync(string path)
        {
            EnsureCanSaveIncremental();

            // We need ReadWrite when adding a signature. Write is sufficient if not adding a signature.
            var fileAccess = DigitalSignatureHandler == null ? FileAccess.Write : FileAccess.ReadWrite;

            // ReSharper disable once UseAwaitUsing because we need no DisposeAsync for a simple FileStream.
            using var stream = new FileStream(path, FileMode.Create, fileAccess, FileShare.None);
            await SaveIncrementalAsync(stream).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves the document as an incremental update to the specified stream.
        /// </summary>
        /// <param name="stream">The stream the document is written to. It must be empty and positioned at its
        /// beginning. If the document is signed, the stream must also be readable and seekable.</param>
        /// <param name="closeStream">If set to true the stream is closed after saving.</param>
        public void SaveIncremental(Stream stream, bool closeStream = false)
        {
            // Safely call the async version on the current thread.
            SaveIncrementalAsync(stream, closeStream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Saves the document async as an incremental update to the specified stream.
        /// The async version of save is useful if you want to create a signed PDF file with a time stamp.
        /// A time stamp server should be accessed asynchronously, and therefore we introduced this function.
        /// </summary>
        /// <remarks>
        /// An incremental update writes the bytes of the original file unchanged, followed by the new and the
        /// modified objects and a cross-reference section that is chained to the cross-reference section of the
        /// original file by its /Prev entry. Because no byte of the original file is touched, a digital signature
        /// the original file may contain stays valid and further signatures can be added one by one.
        /// The document must be opened with <see cref="PdfDocumentOpenMode.ModifyIncremental"/>, because the
        /// object numbers of the original file must be preserved.
        /// Every modified object of the original file must be declared with <see cref="MarkAsModified"/>.
        /// Known limitation: the bytes of the original file are kept in memory from opening the document
        /// until saving it.
        /// </remarks>
        /// <param name="stream">The stream the document is written to. It must be empty and positioned at its
        /// beginning. If the document is signed, the stream must also be readable and seekable.</param>
        /// <param name="closeStream">If set to true the stream is closed after saving.</param>
        public async Task SaveIncrementalAsync(Stream stream, bool closeStream = false)
        {
            EnsureCanSaveIncremental();

            if (!stream.CanWrite)
                throw new InvalidOperationException(PsMsgs.StreamMustBeWritable);

            // The positions of the cross-reference section and of the signature refer to the beginning of
            // the stream, therefore the document must be written to an empty stream.
            if (stream.CanSeek && stream.Position != 0)
            {
                throw new InvalidOperationException(
                    "An incremental update must be written to an empty stream positioned at its beginning.");
            }

            var originalBytes = OriginalBytes ?? throw new InvalidOperationException(
                "The bytes of the original file are not available. " +
                "Open the document with PdfDocumentOpenMode.ModifyIncremental to save it as an incremental update.");

            var previousStartxref = FindLastStartxref(originalBytes);

            PdfWriter? writer = null;
            try
            {
                writer = new PdfWriter(stream, this, null);

                // Prepare for signing. New objects created here are part of the incremental update.
                if (DigitalSignatureHandler != null)
                    await DigitalSignatureHandler.AddSignatureComponentsAsync().ConfigureAwait(false);

                PrepareForSaveIncremental();

                // The set is never empty: reading the document changed its modification date.
                var changedReferences = GetChangedReferences();

                // 1. Write the original file unchanged.
                stream.Write(originalBytes, 0, originalBytes.Length);
                var lastByte = originalBytes[originalBytes.Length - 1];
                if (lastByte != '\n' && lastByte != '\r')
                    writer.WriteRaw("\n");

                // 2. Write the new and the modified objects.
                foreach (var iref in changedReferences)
                {
                    iref.Position = writer.Position;
                    iref.Value.WriteObject(writer);
                }

                // 3. Write the cross-reference section of this update.
                var startxref = writer.Position;
                WriteIncrementalXRefSection(writer, changedReferences);

                // 4. Write the trailer. It keeps /ID, /Root and /Info of the original file and refers to the
                //    cross-reference section of the previous revision.
                Trailer.Elements.SetInteger(PdfTrailer.Keys.Size, IrefTable.MaxObjectNumber + 1);
                Trailer.Elements[PdfTrailer.Keys.Prev] = new PdfLongInteger(previousStartxref);
                writer.WriteRaw("trailer\n");
                Trailer.WriteObject(writer);

                writer.WriteRaw("startxref\n");
                writer.WriteRaw(startxref.ToString(CultureInfo.InvariantCulture));
                writer.WriteRaw("\n%%EOF\n");

                // 5. Compute /ByteRange and /Contents of the signature. The ranges cover the whole file except
                //    the hole of the /Contents entry, therefore the original revision is signed too.
                if (DigitalSignatureHandler != null)
                    await DigitalSignatureHandler.ComputeSignatureAndRange(writer).ConfigureAwait(false);
            }
            finally
            {
                State |= DocumentState.Saved;

                if (stream != null!)
                {
                    stream.Flush();
                    if (!closeStream && stream is { CanRead: true, CanSeek: true })
                        stream.Position = 0; // Reset the stream position if the stream is kept open.
                }

                writer?.Close(closeStream);
            }
        }

        /// <summary>
        /// Checks whether this document can be saved as an incremental update.
        /// </summary>
        void EnsureCanSaveIncremental()
        {
            EnsureNotYetSaved();

            if (OpenMode != PdfDocumentOpenMode.ModifyIncremental)
            {
                throw new InvalidOperationException(
                    "Only a document opened with PdfDocumentOpenMode.ModifyIncremental can be saved as an " +
                    "incremental update, because the object numbers of the original file must be preserved.");
            }

            // An incremental update appends unencrypted objects to the original file. Encrypting them would
            // require the encryption key of the original file, which PDFsharp resets when reading the document.
            if (Trailer.Elements.ContainsKey(PdfTrailer.Keys.Encrypt))
            {
                throw new NotSupportedException(
                    "An encrypted document cannot be saved as an incremental update.");
            }

            if (SecuritySettings.EffectiveSecurityHandler != null)
            {
                throw new NotSupportedException(
                    "A document cannot be encrypted when it is saved as an incremental update.");
            }

            // The cross-reference section written by an incremental update is a cross-reference table.
            // Chaining it to a cross-reference stream of the original file is not valid PDF.
            if (Trailer is PdfCrossReferenceStream)
            {
                throw new NotSupportedException(
                    "A document with a cross-reference stream cannot be saved as an incremental update, " +
                    "because PDFsharp writes a cross-reference table. Save the document with Save instead.");
            }
        }

        /// <summary>
        /// Dispatches PrepareForSave to the objects that need it.
        /// In contrast to the version used by Save, neither unreachable objects are removed nor are the objects
        /// renumbered, because an incremental update must preserve the object numbers of the original file.
        /// </summary>
        void PrepareForSaveIncremental()
        {
            // Keep the original producer. This is “PDF created by” in Adobe Reader.
            if (Info.Producer.Length == 0)
                Info.Elements.SetString(PdfDocumentInformation.Keys.Producer, DefaultProducer);

            // Prepare used fonts.
            _fontTable?.PrepareForSave();

            // Let catalog do the rest. It may modify itself, e.g. by adding metadata, so it is written again.
            MarkAsModified(Catalog);
            Catalog.PrepareForSave();
        }

        /// <summary>
        /// Gets the references of all objects that must be written by the incremental update, in ascending
        /// order by their object number. These are all objects that are not contained in the original file
        /// plus all objects declared as modified.
        /// </summary>
        List<PdfReference> GetChangedReferences()
        {
            var originalObjectIDs = OriginalObjectIDs;
            var changedReferences = new List<PdfReference>();

            // Removing an object would require a free entry in the cross-reference section of the update.
            // PDFsharp does not write free entries, so the object would still be reachable.
            if (originalObjectIDs != null)
            {
                foreach (var objectID in originalObjectIDs)
                {
                    if (!IrefTable.Contains(objectID))
                    {
                        throw new NotSupportedException(
                            Invariant($"Object {objectID} was removed from the document. Removing an object is ") +
                            "not supported by an incremental update.");
                    }
                }
            }

            // AllReferences is sorted by object identifier, so the result is sorted too.
            foreach (var iref in IrefTable.AllReferences)
            {
                if (iref.ObjectNumber <= 0)
                    continue;

                var isNewObject = originalObjectIDs is null || !originalObjectIDs.Contains(iref.ObjectID);
                if (isNewObject || _modifiedObjectIDs.Contains(iref.ObjectID))
                    changedReferences.Add(iref);
            }
            return changedReferences;
        }

        /// <summary>
        /// Writes the cross-reference table of an incremental update. It contains only the objects written by
        /// this update, grouped into subsections of consecutive object numbers.
        /// </summary>
        static void WriteIncrementalXRefSection(PdfWriter writer, List<PdfReference> changedReferences)
        {
            writer.WriteRaw("xref\n");

            int index = 0;
            int count = changedReferences.Count;
            while (index < count)
            {
                // Find the end of the subsection of consecutive object numbers.
                int endOfSubsection = index + 1;
                while (endOfSubsection < count &&
                       changedReferences[endOfSubsection].ObjectNumber == changedReferences[endOfSubsection - 1].ObjectNumber + 1)
                {
                    endOfSubsection++;
                }

                writer.WriteRaw(Invariant($"{changedReferences[index].ObjectNumber} {endOfSubsection - index}\n"));
                for (int idx = index; idx < endOfSubsection; idx++)
                {
                    var iref = changedReferences[idx];

                    // Acrobat is very pedantic; it must be exactly 20 bytes per line.
                    writer.WriteRaw(Invariant($"{iref.Position:0000000000} {iref.GenerationNumber:00000} n \n"));
                }
                index = endOfSubsection;
            }
        }

        /// <summary>
        /// Gets the position of the cross-reference section of the last revision of the original file,
        /// i.e. the value of its last startxref entry.
        /// </summary>
        static SizeType FindLastStartxref(byte[] originalBytes)
        {
            // "startxref" is followed by the position of the cross-reference section and by "%%EOF".
            var position = LastIndexOf(originalBytes, "startxref");
            if (position < 0)
            {
                throw new InvalidOperationException(
                    "The original file contains no startxref entry and therefore cannot be updated incrementally.");
            }

            int index = position + "startxref".Length;
            while (index < originalBytes.Length && Lexer.IsWhiteSpace((char)originalBytes[index]))
                index++;

            SizeType startxref = 0;
            var digits = 0;
            while (index < originalBytes.Length && originalBytes[index] is >= (byte)'0' and <= (byte)'9')
            {
                startxref = startxref * 10 + (originalBytes[index] - '0');
                digits++;
                index++;
            }

            if (digits == 0 || startxref >= originalBytes.Length)
            {
                throw new InvalidOperationException(
                    "The startxref entry of the original file does not contain a valid position.");
            }
            return startxref;
        }

        /// <summary>
        /// Gets the position of the last occurrence of the specified ASCII text in the specified bytes,
        /// or -1 if the text does not occur.
        /// </summary>
        static int LastIndexOf(byte[] bytes, string text)
        {
            for (int start = bytes.Length - text.Length; start >= 0; start--)
            {
                var found = true;
                for (int idx = 0; idx < text.Length; idx++)
                {
                    if (bytes[start + idx] != (byte)text[idx])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return start;
            }
            return -1;
        }

        readonly HashSet<PdfObjectID> _modifiedObjectIDs = [];
    }
}
