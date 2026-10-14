// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO review and sync with metadata, DateTimeOffset review

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents the PDF document information dictionary.
    /// </summary>
    public sealed class PdfDocumentInformation : PdfDictionary
    {
        // Reference 2.0: 14.3.3  Document information dictionary / Page 716

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDocumentInformation"/> class.
        /// </summary>
        public PdfDocumentInformation(PdfDocument document)
            : base(document)
        {
            Producer = document.DefaultProducer;
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfDocumentInformation(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the document’s title.
        /// </summary>
        public string Title
        {
            get => Elements.GetString(Keys.Title);
            set => Elements.SetString(Keys.Title, value);
        }

        /// <summary>
        /// Gets or sets the name of the person who created the document.
        /// </summary>
        public string Author
        {
            // Field is "Creator" in XMP. "Author" from document info will be used if XMP metadata
            // is not used. If XMP metadata is used, the field Author can be empty if Creator cannot
            // be found in XMP; Author from document info will then be ignored.
            get => Elements.GetString(Keys.Author);
            set => Elements.SetString(Keys.Author, value);
        }

        /// <summary>
        /// Gets or sets the subject of the document.
        /// </summary>
        public string Subject
        {
            get => Elements.GetString(Keys.Subject);
            set => Elements.SetString(Keys.Subject, value);
        }

        /// <summary>
        /// Gets or sets keywords associated with the document.
        /// </summary>
        public string Keywords
        {
            get => Elements.GetString(Keys.Keywords);
            set => Elements.SetString(Keys.Keywords, value);
        }

        /// <summary>
        /// Gets or sets the name of the application (for example, MigraDoc) that created the document.
        /// </summary>
        public string Creator
        {
            get => Elements.GetString(Keys.Creator);
            set => Elements.SetString(Keys.Creator, value);
        }

        /// <summary>
        /// Gets the producer application (for example, PDFsharp).
        /// </summary>
        public string Producer
        {
            get => Elements.GetString(Keys.Producer);
#if PDFSHARP_SET_PRODUCER
            set => Elements.SetString(Keys.Producer, value);
#else
            internal init => Elements.SetString(Keys.Producer, value);
#endif
        }

        /// <summary>
        /// Gets or sets the creation date of the document or null, if no entry is set.
        /// </summary>
        public DateTimeOffset? CreationDate
        {
            get => Elements.GetDateTime(Keys.CreationDate, null);
            set
            {
                if (value == null)
                    Elements.Remove(Keys.CreationDate);
                else
                    Elements.SetDateTime(Keys.CreationDate, value.Value);
            }
        }

        /// <summary>
        /// Gets or sets the modification date of the document or null, if no entry is set.
        /// </summary>
        public DateTimeOffset? ModificationDate
        {
            get => Elements.GetDateTime(Keys.ModDate, null);
            set
            {
                if (value == null)
                    Elements.Remove(Keys.ModDate);
                else
                    Elements.SetDateTime(Keys.ModDate, value.Value);
            }
        }

        // TODO CustomProperties and metadata

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : KeysBase
        {
            // Reference 2.0: Table 349 — Entries in the document information dictionary / Page 716

            /// <summary>
            /// (Optional; PDF 1.1) The document’s title.<br/>
            /// NOTE 1<br/>
            /// The dc:title entry in the document’s metadata stream can be used to represent the
            /// document’s title.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Title = "/Title";

            /// <summary>
            /// (Optional) The name of the person who created the document.<br/>
            /// NOTE 2<br/>
            /// The dc:creator entry in the document’s metadata stream can be used to represent the
            /// person or persons who created the document. This note was corrected in this
            /// document (2020).
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Author = "/Author";

            /// <summary>
            /// (Optional; PDF 1.1) The subject of the document.<br/>
            /// NOTE 3<br/>
            /// The dc:description entry in the document’s metadata stream can be used to represent
            /// the subject the document.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Subject = "/Subject";

            /// <summary>
            /// (Optional; PDF 1.1) Keywords associated with the document.<br/>
            /// NOTE 4<br/>
            /// The pdf:Keywords entry in the document’s metadata stream can be used to represent the
            /// keywords for the document.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Keywords = "/Keywords";

            /// <summary>
            /// (Optional) If the document was converted to PDF from another format, the name of the
            /// application (for example, Adobe FrameMaker®) that created the original document from
            /// which it was converted.<br/>
            /// NOTE 5<br/>
            /// The xmp:CreatorTool entry in the document’s metadata stream can be used to represent
            /// the creation tool of the document.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Creator = "/Creator";

            /// <summary>
            /// (Optional) If the document was converted to PDF from another format,
            /// the name of the application (for example, this library) that converted it to PDF.<br/>
            /// NOTE 6<br/>
            /// The pdf:Producer entry in the document’s metadata stream can be used to represent the
            /// tool that saved the document as a PDF.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Producer = "/Producer";

            /// <summary>
            /// (Optional) The date and time the document was created, in human-readable form.<br/>
            /// NOTE 7<br/>
            /// The xmp:CreateDate entry in the document’s metadata stream can be used to represent
            /// document’s creation date and time.
            /// </summary>
            [KeyInfo(KeyType.Date | KeyType.Optional)]
            public const string CreationDate = "/CreationDate";

            /// <summary>
            /// (Required if PieceInfo is present in the document catalog; otherwise optional; PDF 1.1)
            /// The date and time the document was most recently modified, in human-readable form.<br/>
            /// NOTE 8<br/>
            /// The xmp:ModifyDate entry in the document’s metadata stream can be used to represent the
            /// date and time the document was most recently modified.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string ModDate = "/ModDate";

            /// <summary>
            /// (Optional; PDF 1.3; deprecated in PDF 2.0) A name object indicating whether the document
            /// has been modified to include trapping information:<br/>
            /// True<br/>
            /// The document has been fully trapped; no further trapping is needed. (This is the name True,
            /// not the boolean value true.)<br/>
            /// False<br/>
            /// The document has not yet been trapped; any desired trapping must still be done.
            /// (This is the name False, not the boolean value false.)<br/>
            /// Unknown<br/>
            /// Either it is unknown whether the document has been trapped or it has been partly but not
            /// yet fully trapped; some additional trapping may still be needed.<br/>
            /// Default value: Unknown.<br/>
            /// NOTE 9<br/>
            /// The value of this entry can be set automatically by the software creating the document’s
            /// trapping information, or it can be known only to a human operator and entered manually.<br/>
            /// NOTE 10<br/>
            /// The pdf:Trapped entry in the document’s metadata stream can be used to represent the
            /// trapping information for the document.
            /// </summary>
            [KeyInfo("1.3", KeyType.Name | KeyType.Optional)]
            public const string Trapped = "/Trapped";

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
