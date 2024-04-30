// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a PDF trailer dictionary. Even though trailers are dictionaries they never have a cross
    /// reference entry in PdfReferenceTable.
    /// </summary>
    // Reference: 3.4.4  File Trailer / Page 96
    class PdfTrailer : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of PdfTrailer.
        /// </summary>
        public PdfTrailer(PdfDocument document)
            : base(document)
        {
            _document = document;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTrailer"/> class from a <see cref="PdfCrossReferenceStream"/>.
        /// </summary>
        public PdfTrailer(PdfCrossReferenceStream trailer)
            : base(trailer._document)
        {
            _document = trailer._document;
            SecurityHandlerInternal = trailer.SecurityHandlerInternal;

            // /ID [<09F877EBF282E9408ED1882A9A21D9F2><2A4938E896006F499AC1C2EA7BFB08E4>]
            // /Info 7 0 R
            // /Root 1 0 R
            // /Size 10

            var iref = trailer.Elements.GetReference(Keys.Info);
            if (iref != null)
                Elements.SetReference(Keys.Info, iref);

            Elements.SetReference(Keys.Root, trailer.Elements.GetReference(Keys.Root));

            Elements.SetInteger(Keys.Size, trailer.Elements.GetInteger(Keys.Size));

            var id = trailer.Elements.GetArray(Keys.ID);
            if (id != null)
                Elements.SetValue(Keys.ID, id);
        }

        public int Size
        {
            get => Elements.GetInteger(Keys.Size);
            set => Elements.SetInteger(Keys.Size, value);
        }

        //public int Prev needed when linearized..
        //{
        //  get {return Elements.GetInteger(Keys.Prev);}
        //}

        public PdfDocumentInformation Info => (PdfDocumentInformation)Elements.GetValue(Keys.Info, VCF.CreateIndirect)!; // Because of CreateIndirect.

        /// <summary>
        /// (Required; must be an indirect reference)
        /// The catalog dictionary for the PDF document contained in the file.
        /// </summary>
        public PdfCatalog Root => (PdfCatalog)Elements.GetValue(PdfTrailer.Keys.Root, VCF.CreateIndirect)!; // Because of CreateIndirect.

        /// <summary>
        /// Gets the first or second document identifier.
        /// </summary>
        public string GetDocumentID(int index)
        {
            if (index is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0 or 1.");

            var array = Elements[Keys.ID] as PdfArray;
            if (array == null || array.Elements.Count < 2)
                return "";
#if true
            var item = array.Elements[index] as PdfString;
            if (item != null)
            {
                // The DocumentID is just a hex string, never represents unicode content.
                // Add FEFF if it was truncated from the ID. Unlikely, but can happen.
                if ((item.Flags & PdfStringFlags.Unicode) != 0)
                {
                    return new string(new[] { '\u00FE', '\u00FF' })
                           + new string(item.Value.SelectMany(x => new[] { (char)(x / 256), (char)(x % 256) }).ToArray());
                }
                return item.Value;
            }
#else
            var item = array.Elements[index];
            if (item is PdfString pdfString)
                return pdfString.Value;
#endif
            return "";
        }

        /// <summary>
        /// Sets the first or second document identifier.
        /// </summary>
        public void SetDocumentID(int index, string value)
        {
            if (index is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be 0 or 1.");

            var array = Elements[Keys.ID] as PdfArray;
            if (array == null || array.Elements.Count < 2)
                array = CreateNewDocumentIDs();
            array.Elements[index] = new PdfString(value, PdfStringFlags.HexLiteral);
        }

        /// <summary>
        /// Creates and sets two identical new document IDs.
        /// </summary>
        internal PdfArray CreateNewDocumentIDs()
        {
            var array = new PdfArray(_document);
            byte[] docID = Guid.NewGuid().ToByteArray();
            string id = PdfEncoders.RawEncoding.GetString(docID, 0, docID.Length);
            array.Elements.Add(new PdfString(id, PdfStringFlags.HexLiteral));
            array.Elements.Add(new PdfString(id, PdfStringFlags.HexLiteral));
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
        public PdfStandardSecurityHandler? EffectiveSecurityHandler => (SecurityHandlerInternal ??= (PdfStandardSecurityHandler?)Elements.GetValue(Keys.Encrypt))?.GetIfEncryptionActive();

        /// <summary>
        /// Gets and sets the internally saved standard security handler.
        /// </summary>
        internal PdfStandardSecurityHandler? SecurityHandlerInternal;

        internal override void WriteObject(PdfWriter writer)
        {
            // Delete /XRefStm entry, if any.
            // HACK: 
            _elements?.Remove(Keys.XRefStm);

            // Don't encrypt myself.
            var securityHandler = writer.SecurityHandler;
            writer.SecurityHandler = null;
            base.WriteObject(writer);
            writer.SecurityHandler = securityHandler;
        }

        /// <summary>
        /// Replace temporary irefs by their correct counterparts from the iref table.
        /// </summary>
        internal void Finish()
        {
            PdfReference? iref;
            // /Root
            var currentTrailer = _document.Trailer;
            do
            {
                iref = currentTrailer.Elements[Keys.Root] as PdfReference;
                //if (iref != null && iref.Value == null)
                if (iref is { Value: null })
                {
                    iref = _document.IrefTable[iref.ObjectID];
                    Debug.Assert(iref is not null && iref.Value != null);
                    _document.Trailer.Elements[Keys.Root] = iref;
                }

                currentTrailer = currentTrailer.PreviousTrailer;
            } while (currentTrailer != null);

            // /Info
            iref = _document.Trailer.Elements[Keys.Info] as PdfReference;
            if (iref is { Value: null })
            {
                iref = _document.IrefTable[iref.ObjectID];
                Debug.Assert(iref is not null && iref.Value != null);
                _document.Trailer.Elements[Keys.Info] = iref;
            }

            // /Encrypt
            iref = _document.Trailer.Elements[Keys.Encrypt] as PdfReference;
            if (iref != null)
            {
                iref = _document.IrefTable[iref.ObjectID];
                Debug.Assert(iref?.Value != null);
                _document.Trailer.Elements[Keys.Encrypt] = iref;

                // The encryption dictionary (security handler) was read in before the XRefTable construction 
                // was completed. The next lines fix that state (it took several hours to find these bugs...).
                var securityHandler = _document.Trailer.SecurityHandlerInternal!;
                securityHandler.Reference = null; // Reference will be updated new when setting iref.Value.
                iref.Value = securityHandler;
            }

            Elements.Remove(Keys.Prev);

            Debug.Assert(_document.IrefTable.IsUnderConstruction == false);
            _document.IrefTable.IsUnderConstruction = false;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase  // Reference: TABLE 3.13  Entries in the file trailer dictionary / Page 97
        {
            /// <summary>
            /// (Required; must not be an indirect reference) The total number of entries in the file’s 
            /// cross-reference table, as defined by the combination of the original section and all
            /// update sections. Equivalently, this value is 1 greater than the highest object number
            /// used in the file.
            /// Note: Any object in a cross-reference section whose number is greater than this value is
            /// ignored and considered missing.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Size = "/Size";

            /// <summary>
            /// (Present only if the file has more than one cross-reference section; must not be an indirect
            /// reference) The byte offset from the beginning of the file to the beginning of the previous 
            /// cross-reference section.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Prev = "/Prev";

            /// <summary>
            /// (Required; must be an indirect reference) The catalog dictionary for the PDF document
            /// contained in the file.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required, typeof(PdfCatalog))]
            public const string Root = "/Root";

            /// <summary>
            /// (Required if document is encrypted; PDF 1.1) The document’s encryption dictionary.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfStandardSecurityHandler))]
            public const string Encrypt = "/Encrypt";

            /// <summary>
            /// (Optional; must be an indirect reference) The document’s information dictionary.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDocumentInformation))]
            public const string Info = "/Info";

            /// <summary>
            /// (Optional, but strongly recommended; PDF 1.1) An array of two strings constituting
            /// a file identifier for the file. Although this entry is optional, 
            /// its absence might prevent the file from functioning in some workflows
            /// that depend on files being uniquely identified.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string ID = "/ID";

            /// <summary>
            /// (Optional) The byte offset from the beginning of the file of a cross-reference stream.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string XRefStm = "/XRefStm";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            public static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
