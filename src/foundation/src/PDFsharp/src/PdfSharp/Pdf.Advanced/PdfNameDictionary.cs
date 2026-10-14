// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Pdf.Attachments;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the name dictionary of the catalog.
    /// </summary>
    public sealed class PdfNameDictionary : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNameDictionary"/> class.
        /// </summary>
        public PdfNameDictionary(PdfDocument document)
            : base(document, true)
        {
            // Is always an indirect object.
            //document.Internals.AddObject(this);
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfNameDictionary(PdfDictionary dictionary)
            : base(dictionary)
        {
            var dests = Elements.GetDictionary(Keys.Dests);
            if (dests != null)
            {
                if (dests is not PdfNameTreeNode dests2)
                    dests2 = new PdfNameTreeNode(dests);
                _dests = dests2;
            }
        }

        /// <summary>
        /// Gets the named destinations
        /// </summary>
        public PdfNameTreeNode? NameTree => _dests;

        internal void AddNamedDestination(string destinationName, int destinationPage, PdfNamedDestinationParameters parameters)
        {
            if (_dests == null)
            {
                _dests = new PdfNameTreeNode();
                Owner.Internals.AddObject(_dests);
                Elements.SetReference(Keys.Dests, _dests.Reference ?? throw TH.InvalidOperationException_ReferenceMustNotBeNull());
            }

            // destIndex > Owner.PageCount can happen when rendering pages using PDFsharp directly.
            int destIndex = destinationPage;
            if (destIndex > Owner.PageCount)
                destIndex = Owner.PageCount;
            destIndex--;
            PdfPage dest = Owner.Pages[destIndex];

#if true
            PdfArray destination = new PdfArray(Owner,
                new PdfLiteral("{0} 0 R {1}", dest.ObjectNumber, parameters));
            _dests.AddName(destinationName, destination);
#else
// Insert reference to destination dictionary instead of inserting the destination array directly.
            PdfArray destination = new PdfArray(Owner, new PdfLiteral("{0} 0 R {1}", dest.ObjectNumber, parameters));
            PdfDictionary destinationDict = new PdfDictionary(Owner);
            destinationDict.Elements.SetObject("/D", destination);
            Owner.Internals.AddObject(destinationDict);
            _dests.AddName(destinationName, destinationDict.Reference);
#endif
        }
        PdfNameTreeNode? _dests;

        //// TODO: Remove
        //internal void AddEmbeddedFile(string name, Stream stream, out PdfFileSpecification fileSpecification, string? subType = null)
        //{
        //    var embeddedFiles = GetEmbeddedFiles();
        //    if (embeddedFiles == null)
        //    {
        //        // Create a direct object.
        //        embeddedFiles = new PdfEmbeddedFiles();
        //        Elements.Add(Keys.EmbeddedFiles, embeddedFiles);
        //    }

        //    var embeddedFileStream = new PdfEmbeddedFileStream(Owner, stream, subType);
        //    fileSpecification = new PdfFileSpecification(Owner, embeddedFileStream, name);
        //    Owner.Internals.AddObject(fileSpecification);

        //    embeddedFiles.AddName(name, fileSpecification.RequiredReference);
        //}

        internal bool HasEmbeddedFiles 
            => Elements.TryGetValue(Keys.EmbeddedFiles, out _);

        /// <summary>
        /// Get or created... TODO
        /// </summary>
        [return: NotNullIfNotNull(nameof(create))]
        internal PdfEmbeddedFiles? GetEmbeddedFiles(bool create = false)  // TODO: null or not null? => Use NotNullIfNotNullAttribute
        {
            var ef = Elements.GetRequiredDictionary<PdfEmbeddedFiles>(Keys.EmbeddedFiles,
                create ? VCF.Create : VCF.None);
            return ef;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : KeysBase
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional; PDF 1.2) A name tree mapping name strings to destinations (see “Named Destinations” on page 583).
            /// </summary>
            [KeyInfo("1.2", KeyType.NameTree | KeyType.Optional)]
            public const string Dests = "/Dests";

            ///// <summary>
            ///// (Optional; PDF 1.3) A name tree mapping name strings to annotation appearance streams
            ///// (see Section 8.4.4, “Appearance Streams”).
            ///// </summary>
            //[KeyInfo("1.3", KeyType.NameTree | KeyType.Optional)]
            //public const string AP = "/AP";

            ///// <summary>
            ///// (Optional; PDF 1.3) A name tree mapping name strings to document-level JavaScript actions
            ///// (see “JavaScript Actions” on page 709).
            ///// </summary>
            //[KeyInfo("1.3", KeyType.NameTree | KeyType.Optional)]
            //public const string JavaScript = "/JavaScript";

            ///// <summary>
            ///// (Optional; PDF 1.3) A name tree mapping name strings to visible pages for use in interactive forms
            ///// (see Section 8.6.5, “Named Pages”).
            ///// </summary>
            //[KeyInfo("1.3", KeyType.NameTree | KeyType.Optional)]
            //public const string Pages = "/Pages";

            ///// <summary>
            ///// (Optional; PDF 1.3) A name tree mapping name strings to invisible (template) pages for use in
            ///// interactive forms (see Section 8.6.5, “Named Pages”).
            ///// </summary>
            //[KeyInfo("1.3", KeyType.NameTree | KeyType.Optional)]
            //public const string Templates = "/Templates";

            ///// <summary>
            ///// (Optional; PDF 1.3) A name tree mapping digital identifiers to Web Capture content sets
            ///// (see Section 10.9.3, “Content Sets”).
            ///// </summary>
            //[KeyInfo("1.3", KeyType.NameTree | KeyType.Optional)]
            //public const string IDS = "/IDS";

            ///// <summary>
            ///// (Optional; PDF 1.3) A name tree mapping uniform resource locators (URLs) to Web Capture content sets
            ///// (see Section 10.9.3, “Content Sets”).
            ///// </summary>
            //[KeyInfo("1.3", KeyType.NameTree | KeyType.Optional)]
            //public const string URLS = "/URLS";

            /// <summary>
            /// (Optional; PDF 1.4) A name tree mapping name strings to file specifications for embedded file streams
            /// (see Section 3.10.3, “Embedded File Streams”).
            /// </summary>
            [KeyInfo("1.4", KeyType.NameTree | KeyType.Optional, typeof(PdfEmbeddedFiles))]
            public const string EmbeddedFiles = "/EmbeddedFiles";

            ///// <summary>
            ///// (Optional; PDF 1.4) A name tree mapping name strings to alternate presentations
            ///// (see Section 9.4, “Alternate Presentations”).
            ///// </summary>
            //[KeyInfo("1.4", KeyType.NameTree | KeyType.Optional)]
            //public const string AlternatePresentations = "/AlternatePresentations";

            ///// <summary>
            ///// (Optional; PDF 1.5) A name tree mapping name strings (which must have Unicode encoding) to
            ///// rendition objects (see Section 9.1.2, “Renditions”).
            ///// </summary>
            //[KeyInfo("1.5", KeyType.NameTree | KeyType.Optional)]
            //public const string Renditions = "/Renditions";

            // ReSharper restore InconsistentNaming

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
