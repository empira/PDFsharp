// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

// v7.0.0 Ready

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a PDF trailer dictionary. Even though trailers are dictionaries they never have a
    /// cross-reference entry in PdfReferenceTable.
    /// </summary>
    // Reference: 3.4.4  File Trailer / Page 96
    public class PdfTrailer : PdfDictionary
    {
        // Reference 2.0: 7.5.5  File trailer / Page 58
        // - and -
        // Reference 2.0: 12.7.8.2.4  FDF trailer / Page 558

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTrailer"/> class from a PdfDocument.
        /// </summary>
        public PdfTrailer(PdfDocument document)
            : base(document)
        {
            Debug.Assert(ReferenceEquals(_document2, Document));
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfTrailer(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTrailer"/> class from a <see cref="PdfCrossReferenceStream"/>.
        /// </summary>
        public PdfTrailer(PdfCrossReferenceStream trailer)
            : base(trailer.Document)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Debug.Assert(ReferenceEquals(_document2, trailer.Document));
            Debug.Assert(ReferenceEquals(Document, trailer._document2));
            SecurityHandlerInternal = trailer.SecurityHandlerInternal;

            // /ID [<09F877EBF282E9408ED1882A9A21D9F2><2A4938E896006F499AC1C2EA7BFB08E4>]
            // /Info 7 0 R
            // /Root 1 0 R
            // /Size 10

            var iref = trailer.Elements.GetReference(Keys.Info);
            if (iref != null)
                Elements.SetReference(Keys.Info, iref);

            Elements.SetReference(Keys.Root, trailer.Elements.GetReference(Keys.Root) ?? throw TH.InvalidOperationException_ReferenceMustNotBeNull());

            Elements.SetInteger(Keys.Size, trailer.Elements.GetInteger(Keys.Size));

            var id = trailer.Elements.GetArray(Keys.ID);
            if (id != null)
            {
                // TODO: Check what happened here: Why found we this bug so late?
                id = id.Clone();
                Elements.SetValue(Keys.ID, id);
            }
        }

        public int Size
        {
            get => Elements.GetInteger(Keys.Size);
            set => Elements.SetInteger(Keys.Size, value);
        }

        public PdfDocumentInformation Info => Elements.GetRequiredValue<PdfDocumentInformation>(Keys.Info, VCF.CreateIndirect);

        /// <summary>
        /// (Required; must be an indirect reference)
        /// The catalog dictionary for the PDF document contained in the file.
        /// </summary>
        public PdfCatalog Root => Elements.GetRequiredValue<PdfCatalog>(PdfTrailer.Keys.Root, VCF.CreateIndirect);

        /// <summary>
        /// Gets the first or second document identifier.
        /// The index must be 0 or 1.
        /// </summary>
        public string GetDocumentID(int index)
        {
            if (index is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0 or 1.");

            //var array = Elements[Keys.ID] as PdfArray;
            //if (array == null || array.Elements.Count < 2)

            //if (Elements[Keys.ID] is not PdfArray array || array.Elements.Count < 2)
            if (Elements.GetArray(Keys.ID) is not { } array || array.Elements.Count < 2) // #US373
                return "";

            //var item = array.Elements[index] as PdfString;
            //if (item != null)
            if (array.Elements[index] is PdfString item)
            {
                // The DocumentID is just a hex string, never represents Unicode content.
                // Add FEFF if it was truncated from the ID. Unlikely, but can happen.
                if ((item.Flags & PdfStringFlags.Unicode) != 0)
                {
                    return new string(new[] { '\u00FE', '\u00FF' })
                           + new string(item.Value.SelectMany(x => new[] { (char)(x / 256), (char)(x % 256) }).ToArray());
                }
                return item.Value;
            }
            return "";
        }

        /// <summary>
        /// Sets the first or second document identifier.
        /// </summary>
        public void SetDocumentID(int index, string value)
        {
            if (index is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0 or 1.");

            //var array = Elements[Keys.ID] as PdfArray;
            //if (array == null || array.Elements.Count < 2)

            //if (Elements[Keys.ID] is not PdfArray array || array.Elements.Count < 2)
            if (Elements.GetArray(Keys.ID) is not { } array || array.Elements.Count < 2) // #US373
                array = CreateNewDocumentIDs();
            array.Elements[index] = new PdfString(value, PdfStringFlags.HexLiteral);
        }

        /// <summary>
        /// Creates and sets two identical new document IDs.
        /// </summary>
        internal PdfArray CreateNewDocumentIDs()
        {
            Debug.Assert(ReferenceEquals(_document2, Document));
            var array = new PdfArray(Document);
            byte[] docID = Guid.NewGuid().ToByteArray();
            string id = PdfEncoders.RawEncoding.GetString(docID, 0, docID.Length);
            var str = new PdfString(id, PdfStringFlags.HexLiteral);
            array.Elements.Add(str);
            array.Elements.Add(str);
            Elements[Keys.ID] = array;
            return array;
        }

        /// <summary>
        /// Gets or sets the PdfTrailer of the previous version in a PDF with incremental updates.
        /// </summary>
        public PdfTrailer? PreviousTrailer { get; set; }

        /// <summary>
        /// Gets the standard security handler and creates it, if not existing.
        /// </summary>
        public PdfStandardSecurityHandler SecurityHandler
            => SecurityHandlerInternal ??= (PdfStandardSecurityHandler?)Elements.GetValue(Keys.Encrypt, VCF.CreateIndirect)!;

        /// <summary>
        /// Gets the standard security handler, if existing and encryption is active.
        /// </summary>
        public PdfStandardSecurityHandler? EffectiveSecurityHandler => SecurityHandlerInternal?.GetIfEncryptionIsActive();

        /// <summary>
        /// Gets and sets the internally saved standard security handler.
        /// </summary>
        internal PdfStandardSecurityHandler? SecurityHandlerInternal;

        internal override void WriteObject(PdfWriter writer)
        {
            // Delete /XRefStm entry, if any.
            // HACK_OLD: TODO When can this happen??
            Elements.Remove(Keys.XRefStm);

            // Don’t encrypt myself.
            var effectiveSecurityHandler = writer.EffectiveSecurityHandler;
            writer.EffectiveSecurityHandler = null;
            base.WriteObject(writer);
            writer.NewLine();
            writer.EffectiveSecurityHandler = effectiveSecurityHandler;
        }

        /// <summary>
        /// Replace temporary irefs by their correct counterparts from the iref table.
        /// </summary>
        internal void Finish()
        {
            PdfReference? iref;

            // /Root
            var currentTrailer = Document.Trailer;
            Debug.Assert(ReferenceEquals(_document2, Document));
            do
            {
                //iref = currentTrailer.Elements[Keys.Root] as PdfReference;
                iref = currentTrailer.Elements.GetReference(Keys.Root); // TODO #US373
                //if (iref != null && iref.Value == null)
                if (iref is { Value: null })
                {
                    iref = Document.IrefTable[iref.ObjectID];
                    Debug.Assert(iref is not null && iref.Value != null);
                    currentTrailer.Elements[Keys.Root] = iref;
                }

                currentTrailer = currentTrailer.PreviousTrailer;
            } while (currentTrailer != null);

            // /Info
            //iref = Document.Trailer.Elements[Keys.Info] as PdfReference;
            iref = Document.Trailer.Elements.GetReference(Keys.Info); // TODO #US373
            if (iref is { Value: null })
            {
                iref = Document.IrefTable[iref.ObjectID];
                Debug.Assert(iref?.Value != null);
                Document.Trailer.Elements[Keys.Info] = iref;
            }

            // /Encrypt
            //iref = Document.Trailer.Elements[Keys.Encrypt] as PdfReference;
            iref = Document.Trailer.Elements.GetReference(Keys.Encrypt); // TODO #US373
            if (iref != null)
            {
                if (iref.Value == null!)
                {
                    iref = Document.IrefTable[iref.ObjectID];
                    Debug.Assert(iref?.Value != null);
                    Document.Trailer.Elements[Keys.Encrypt] = iref;
                }

                // The encryption dictionary (security handler) was read in before the XRefTable construction 
                // was completed. The next lines fix that state (it took several hours to find these bugs...).
                var securityHandler = Document.Trailer.SecurityHandlerInternal!;
                securityHandler.Reference = null; // Reference will be updated new when setting iref.Value.
                iref.Value = securityHandler;
            }

            Elements.Remove(Keys.Prev);

            Debug.Assert(!Document.IrefTable.IsUnderConstruction);
            Document.IrefTable.IsUnderConstruction = false;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference 2.0: Table 15 — Entries in the file trailer dictionary / Page 58
            // - and -
            // Reference 2.0: Table 19 — Additional entries in a hybrid-reference file’s trailer dictionary / Page 58
            // - and -
            // Reference 2.0: Table 244 — Entry in the FDF trailer dictionary / Page 558

            /// <summary>
            /// (Required; shall not be an indirect reference) The total number of entries in the PDF
            /// file’s cross-reference table, as defined by the combination of the original section
            /// and all update sections. Equivalently, this value shall be 1 greater than the highest
            /// object number defined in the PDF file.<br/>
            /// Any object in a cross-reference section whose number is greater than this value shall
            /// be ignored and defined to be missing by a PDF reader.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Size = "/Size";

            /// <summary>
            ///(Optional, present only if the file has more than one cross-reference section; shall
            /// be a direct object) The byte offset from the beginning of the PDF file to the beginning
            /// of the previous cross-reference stream.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Prev = "/Prev";

            /// <summary>
            /// (Required; shall be an indirect reference) The catalog dictionary object for this FDF file.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required, typeof(PdfCatalog))]
            public const string Root = "/Root";

            /// <summary>
            /// (Required if document is encrypted; PDF 1.1) The document’s encryption dictionary.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfStandardSecurityHandler))]
            public const string Encrypt = "/Encrypt";

            /// <summary>
            /// (Optional; shall be an indirect reference) The PDF file’s information dictionary.
            /// As described in "Document information dictionary", this method for specifying document
            /// metadata has been deprecated in PDF 2.0 and should therefore only be used to encode
            /// information that is stated as required elsewhere in this document.<br/>
            /// NOTE 1<br/>
            /// The ModDate key within the Info dictionary is required if Page-Piece dictionaries(see 14.5, "Page-piece dictionaries") are used.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDocumentInformation))]
            public const string Info = "/Info";

            /// <summary>
            /// (Required in PDF 2.0 and later, or if an Encrypt entry is present; optional otherwise;
            /// PDF 1.1) An array of two byte-strings constituting a PDF file identifier for the PDF file.
            /// Each PDF file identifier byte-string shall have a minimum length of 16 bytes. If there
            /// is an Encrypt entry, this array and the two byte-strings shall be direct objects and
            /// shall be unencrypted.<br/>
            /// NOTE 2<br/>
            /// Because the ID entries are not encrypted, the ID key can be checked to assure that the
            /// correct PDF file is being accessed without decrypting the PDF file. The restrictions
            /// that the objects all be direct objects and not be encrypted ensure this.<br/>
            /// NOTE 3<br/>
            /// Although this entry is optional prior to PDF 2.0, its absence can prevent the PDF file
            /// from functioning in some workflows that depend on PDF files being uniquely identified.<br/>
            /// NOTE 4<br/>
            /// The values of the ID strings are used as input to the encryption algorithm.If these
            /// strings were indirect, or if the ID array were indirect, these strings would be encrypted
            /// when written.This would result in a circular condition for a PDF reader: the ID strings
            /// need be decrypted in order to use them to decrypt strings, including the ID strings
            /// themselves.The preceding restriction prevents this circular condition.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string ID = "/ID";

            /// <summary>
            /// (Optional) The byte offset in the decoded stream from the beginning of the PDF file of a cross-reference stream.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string XRefStm = "/XRefStm";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
