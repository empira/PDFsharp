// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.Metadata;
using PdfSharp.Pdf.Structure;

// v7.0.0 TODO review and cleanup

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the PDF catalog dictionary.
    /// </summary>
    public sealed class PdfCatalog : PdfDictionary
    {
        // Reference 2.0:  / Page 

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCatalog"/> class.
        /// </summary>
        public PdfCatalog(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Catalog");
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfCatalog(PdfDictionary dictionary)
            : base(dictionary)
        { }

        /// <summary>
        /// Get or sets the version of the PDF specification to which the document conforms.
        /// </summary>
        public string Version
        {
            get => _version;
            set
            {
                // TODO Create: public enum PdfVersion { Pdf10, Pdf11, ..., Pdf20}
                switch (value)
                {
                    case "1.0":
                    case "1.1":
                    case "1.2":
                        throw new InvalidOperationException("Unsupported PDF version.");

                    case "1.3":
                    case "1.4":
                        _version = value;
                        break;

                    case "1.5":
                    case "1.6":
                        throw new InvalidOperationException("Unsupported PDF version.");

                    case "1.7":
                    case "2.0":
                        _version = value;
                        break;

                    default:
                        throw new ArgumentException("Invalid version.");
                }
            }
        }
        string _version = "1.7";

        /// <summary>
        /// Gets the page tree root of this document.  // TODO flat or not
        /// </summary>
        public PdfPages Pages
            => _pages ??= Elements.GetRequiredDictionary<PdfPages>(Keys.Pages, VCF.CreateIndirect);
        PdfPages? _pages;

        /// <summary>
        /// Implementation of PdfDocument.PageLayout.
        /// </summary>
        internal PdfPageLayout PageLayout
        {
            get => (PdfPageLayout)Elements.GetEnumFromName(Keys.PageLayout, PdfPageLayout.SinglePage);
            set => Elements.SetEnumAsName(Keys.PageLayout, value);
        }

        /// <summary>
        /// Implementation of PdfDocument.PageMode.
        /// </summary>
        internal PdfPageMode PageMode
        {
            get => (PdfPageMode)Elements.GetEnumFromName(Keys.PageMode, PdfPageMode.UseNone);
            set => Elements.SetEnumAsName(Keys.PageMode, value);
        }

        /// <summary>
        /// Implementation of PdfDocument.ViewerPreferences.
        /// </summary>
        internal PdfViewerPreferences ViewerPreferences
            => Elements.GetRequiredDictionary<PdfViewerPreferences>(Keys.ViewerPreferences, VCF.CreateIndirect);

        internal bool HasOutline
            => Elements.GetDictionary(Keys.Outlines, VCF.NoTransform) != null;

        internal PdfOutline Outline
            => Elements.GetRequiredDictionary<PdfOutline>(Keys.Outlines, VCF.CreateIndirect);

        /// <summary>
        /// Implementation of PdfDocument.Outlines.
        /// </summary>
        internal PdfOutlineCollection Outlines
        {
            get
            {
                var outline = Elements.GetRequiredDictionary<PdfOutline>(Keys.Outlines, VCF.CreateIndirect);
                return outline.Outlines;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document has a /Metadata dictionary.
        /// </summary>
        public bool HasMetadata => Elements.GetDictionary<PdfMetadata>(Keys.Metadata) != null;

        /// <summary>
        /// Gets the metadata dictionary of this document.
        /// An empty dictionary is created if it does not exist.
        /// </summary>
        [Obsolete("Use GetMetadata or GetRequiredMetadata.")]
        public PdfMetadata Metadata
            => Elements.GetRequiredDictionary<PdfMetadata>(Keys.Metadata, VCF.CreateIndirect);

        /// <summary>
        /// Gets the metadata dictionary of this document.
        /// Returns null if it does not exist.
        /// </summary>
        public PdfMetadata? GetMetadata() 
            => Elements.GetDictionary<PdfMetadata>(Keys.Metadata);

        /// <summary>
        /// Gets the metadata dictionary of this document.
        /// It is created if it does not exist.
        /// </summary>
        public PdfMetadata GetOrCreateMetadata() 
            => Elements.GetRequiredDictionary<PdfMetadata>(Keys.Metadata, VCF.CreateIndirect);

        /// <summary>
        /// Gets a value indicating whether the document has a /Names dictionary.
        /// </summary>
        public bool HasNames => Elements.GetDictionary<PdfNameDictionary>(Keys.Names) != null;

        /// <summary>
        /// Gets the /Names dictionary of this document.
        /// An empty dictionary is created if it does not exist.
        /// </summary>
        public PdfNameDictionary Names
            => Elements.GetRequiredDictionary<PdfNameDictionary>(Keys.Names, VCF.CreateIndirect);

        /// <summary>
        /// Gets the /Names dictionary of this document.
        /// Returns null if it does not exist.
        /// </summary>
        public PdfNameDictionary? GetNames() 
            => Elements.GetDictionary<PdfNameDictionary>(Keys.Metadata);

        /// <summary>
        /// Gets the /Names dictionary of this document.
        /// It is created if it does not exist.
        /// </summary>
        public PdfNameDictionary GetOrCreateNames() 
            => Elements.GetRequiredDictionary<PdfNameDictionary>(Keys.Metadata, VCF.CreateIndirect);

        /// <summary>
        /// Gets a value indicating whether the document has a /Dests dictionary.
        /// </summary>
        public bool HasDestinations
            => Elements.GetDictionary<PdfNamedDestinations>(Keys.Dests) != null;

        /// <summary>
        /// Gets the named destinations defined in the Catalog.
        /// </summary>
        [Obsolete("Use GetDestinations or GetRequiredDestinations.")]
        public PdfNamedDestinations Destinations
            => Elements.GetRequiredDictionary<PdfNamedDestinations>(Keys.Dests, VCF.CreateIndirect);

        /// <summary>
        /// Gets the named destinations dictionary of this document.
        /// Returns null if it does not exist.
        /// </summary>
        public PdfNamedDestinations? GetDestinations() 
            => Elements.GetDictionary<PdfNamedDestinations>(Keys.Dests);

        /// <summary>
        /// Gets the named destinations dictionary of this document.
        /// It is created if it does not exist.
        /// </summary>
        public PdfNamedDestinations GetRequiredDestinations() 
            => Elements.GetRequiredDictionary<PdfNamedDestinations>(Keys.Dests, VCF.CreateIndirect);

        /// <summary>
        /// Gets a value indicating whether the document has a /AcroForm dictionary.
        /// </summary>
        public bool HasAcroForm => Elements.GetDictionary<PdfForm>(Keys.AcroForm) != null;

        /// <summary>
        /// Gets the interactive form (AcroForm) dictionary of this document.
        /// </summary>
        [Obsolete("Use GetAcroForm or GetOrCreateAcroForm.")]
        public PdfForm AcroForm
            => Elements.GetRequiredDictionary<PdfForm>(Keys.AcroForm, VCF.CreateIndirect);

        /// <summary>
        /// Gets the acro form dictionary of this document.
        /// Returns null if it does not exist.
        /// </summary>
        public PdfForm? GetAcroForm() 
            => Elements.GetDictionary<PdfForm>(Keys.AcroForm);

        /// <summary>
        /// Gets the acro form dictionary of this document.
        /// It is created if it does not exist.
        /// </summary>
        public PdfForm GetOrCreateAcroForm() 
            => Elements.GetRequiredDictionary<PdfForm>(Keys.AcroForm, VCF.CreateIndirect);

        /// <summary>
        /// Gets or sets the language identifier specifying the natural language for all text in the document.
        /// Sample values are 'en-US' for 'English United States' or 'de-DE' for 'Deutsch Deutschland' (i.e. 'German Germany').
        /// </summary>
        public string Language
        {
            get => Elements.GetString(Keys.Lang);
            set
            {
                if (value == null!)
                    Elements.Remove(Keys.Lang);
                else
                    Elements.SetString(Keys.Lang, value);
            }
        }

        /// <summary>
        /// Dispatches PrepareForSave to the objects that need it.
        /// </summary>
        internal override void PrepareForSave()
        {
            // Prepare pages.
            _pages?.PrepareForSave();

            // Prepare metadata.
            var metadataManager = Document.GetMetadataManager();
            metadataManager.PrepareForSave();

            var xxx = metadataManager.ToString();

            // Prepare AcroForm objects.
            var acroForm = GetAcroForm();
            var fields = acroForm?.Elements.GetArray(PdfForm.Keys.Fields);
            if (fields != null)
            {
                foreach (var formRef in fields.Elements)
                {
                    var form = (formRef as PdfReference)?.Value;
                    if (form is PdfFormField field)
                    {
                        field.PrepareForSave();
                    }
                }
            }

            // Create outline objects.
            if (HasOutline && Outline is { Outlines.Count: > 0 } outline)
            {
                if (!Elements.HasValue(Keys.PageMode)) // #US373 GetValue() not needed, just checking existence.
                    PageMode = PdfPageMode.UseOutlines;
                outline.PrepareForSave();
            }

            // Clean up structure tree root.
            if (Elements.GetObject(Keys.StructTreeRoot) is PdfStructureTreeRoot structTree)
                structTree.PrepareForSave();
        }

        internal override void WriteObject(PdfWriter writer)
        {
            var state = Document.State;
            //if (_outline != null && _outline.Outlines.Count > 0)
            if (HasOutline && Outline is { Outlines.Count: > 0 })
            {
                if (!Elements.HasValue(Keys.PageMode)) // #US373 GetValue() not needed, just checking existence.
                    PageMode = PdfPageMode.UseOutlines;
            }
            base.WriteObject(writer);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : KeysBase
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes; 
            /// must be Catalog for the catalog dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Catalog")]
            public const string Type = "/Type";

            /// <summary>
            /// (Optional; PDF 1.4) The version of the PDF specification to which the document
            /// conforms (for example, 1.4) if later than the version specified in the file’s header.
            /// If the header specifies a later version, or if this entry is absent, the document 
            /// conforms to the version specified in the header. This entry enables a PDF producer 
            /// application to update the version using an incremental update.
            /// </summary>
            [KeyInfo("1.4", KeyType.Name | KeyType.Optional)]
            public const string Version = "/Version";

            /// <summary>
            /// (Required; must be an indirect reference) The page tree node that is the root of 
            /// the document’s page tree.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required | KeyType.MustBeIndirect, typeof(PdfPages))]
            public const string Pages = "/Pages";

            /// <summary>
            /// (Optional; PDF 1.3) A number tree defining the page labeling for the document. 
            /// The keys in this tree are page indices; the corresponding values are page label dictionaries.
            /// Each page index denotes the first page in a labeling range to which the specified page 
            /// label dictionary applies. The tree must include a value for pageindex 0.
            /// </summary>
            [KeyInfo("1.3", KeyType.NumberTree | KeyType.Optional)]
            public const string PageLabels = "/PageLabels";

            /// <summary>
            /// (Optional; PDF 1.2) The document’s name dictionary.
            /// (PDF 2.0) For unencrypted wrapper documents for an encrypted payload document
            /// the Names dictionary is required and shall contain the EmbeddedFiles name tree.
            /// </summary>
            [KeyInfo("1.2", KeyType.Dictionary | KeyType.Optional)]
            public const string Names = "/Names";

            /// <summary>
            /// (Optional; PDF 1.1; must be an indirect reference) A dictionary of names and 
            /// corresponding destinations.
            /// </summary>
            [KeyInfo("1.1", KeyType.Dictionary | KeyType.Optional)]
            public const string Dests = "/Dests";

            /// <summary>
            /// (Optional; PDF 1.2) A viewer preferences dictionary specifying the way the document 
            /// is to be displayed on the screen. If this entry is absent, applications should use
            /// their own current user preference settings.
            /// </summary>
            [KeyInfo("1.2", KeyType.Dictionary | KeyType.Optional, typeof(PdfViewerPreferences))]
            public const string ViewerPreferences = "/ViewerPreferences";

            /// <summary>
            /// (Optional) A name object specifying the page layout to be used when the document is 
            /// opened:
            ///     SinglePage - Display one page at a time.
            ///     OneColumn - Display the pages in one column.
            ///     TwoColumnLeft - Display the pages in two columns, with odd-numbered pages on the left.
            ///     TwoColumnRight - Display the pages in two columns, with odd-numbered pages on the right.
            ///     TwoPageLeft - (PDF 1.5) Display the pages two at a time, with odd-numbered pages on the left
            ///     TwoPageRight - (PDF 1.5) Display the pages two at a time, with odd-numbered pages on the right.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string PageLayout = "/PageLayout";

            /// <summary>
            /// (Optional) A name object specifying how the document should be displayed when opened:
            ///     UseNone - Neither document outline nor thumbnail images visible.
            ///     UseOutlines - Document outline visible.
            ///     UseThumbs - Thumbnail images visible.
            ///     FullScreen - Full-screen mode, with no menu bar, window controls, or any other window visible.
            ///     UseOC - (PDF 1.5) Optional content group panel visible.
            ///     UseAttachments (PDF 1.6) Attachments panel visible.
            /// Default value: UseNone.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string PageMode = "/PageMode";

            /// <summary>
            /// (Optional; must be an indirect reference) The outline dictionary that is the root 
            /// of the document’s outline hierarchy.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfOutline))]
            public const string Outlines = "/Outlines";

            /// <summary>
            /// (Optional; PDF 1.1; must be an indirect reference) An array of thread dictionaries 
            /// representing the document’s article threads.
            /// </summary>
            [KeyInfo("1.1", KeyType.Array | KeyType.Optional)]
            public const string Threads = "/Threads";

            /// <summary>
            /// (Optional; PDF 1.1) A value specifying a destination to be displayed or an action to be 
            /// performed when the document is opened. The value is either an array defining a destination 
            /// or an action dictionary representing an action. If this entry is absent, the document
            /// should be opened to the top of the first page at the default magnification factor.
            /// </summary>
            [KeyInfo("1.1", KeyType.ArrayOrDictionary | KeyType.Optional)]
            public const string OpenAction = "/OpenAction";

            /// <summary>
            /// (Optional; PDF 1.4) An additional-actions dictionary defining the actions to be taken 
            /// in response to various trigger events affecting the document as a whole.
            /// </summary>
            [KeyInfo("1.4", KeyType.Dictionary | KeyType.Optional)]
            public const string AA = "/AA";

            /// <summary>
            /// (Optional; PDF 1.1) A URI dictionary containing document-level information for URI 
            /// (uniform resource identifier) actions.
            /// </summary>
            [KeyInfo("1.1", KeyType.Dictionary | KeyType.Optional)]
            public const string URI = "/URI";

            /// <summary>
            /// (Optional; PDF 1.2) The document’s interactive form (AcroForm) dictionary.
            /// </summary>
            [KeyInfo("1.2", KeyType.Dictionary | KeyType.Optional, typeof(PdfForm))]
            public const string AcroForm = "/AcroForm";

            /// <summary>
            /// (Optional; PDF 1.4; must be an indirect reference) A metadata stream 
            /// containing metadata for the document.
            /// </summary>
            [KeyInfo("1.4", KeyType.Dictionary | KeyType.Optional | KeyType.MustBeIndirect)]
            public const string Metadata = "/Metadata";

            /// <summary>
            /// (Optional; PDF 1.3) The document’s structure tree root dictionary.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
            public const string StructTreeRoot = "/StructTreeRoot";

            /// <summary>
            /// (Optional; PDF 1.4) A mark information dictionary containing information
            /// about the document’s usage of Tagged PDF conventions.
            /// </summary>
            [KeyInfo("1.4", KeyType.Dictionary | KeyType.Optional)]
            public const string MarkInfo = "/MarkInfo";

            /// <summary>
            /// (Optional; PDF 1.4) A language identifier specifying the natural language for all 
            /// text in the document except where overridden by language specifications for structure 
            /// elements or marked content. If this entry is absent, the language is considered unknown.
            /// </summary>
            [KeyInfo("1.4", KeyType.String | KeyType.Optional)]
            public const string Lang = "/Lang";

            /// <summary>
            /// (Optional; PDF 1.3) A Web Capture information dictionary containing state information
            /// used by the Acrobat Web Capture (AcroSpider) plugin extension.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
            public const string SpiderInfo = "/SpiderInfo";

            /// <summary>
            /// (Optional; PDF 1.4) An array of output intent dictionaries describing the color 
            /// characteristics of output devices on which the document might be rendered.
            /// </summary>
            [KeyInfo("1.4", KeyType.Array | KeyType.Optional)]
            public const string OutputIntents = "/OutputIntents";

            /// <summary>
            /// (Optional; PDF 1.4) A page-piece dictionary associated with the document.
            /// </summary>
            [KeyInfo("1.4", KeyType.Dictionary | KeyType.Optional)]
            public const string PieceInfo = "/PieceInfo";

            /// <summary>
            /// (Optional; PDF 1.5; required if a document contains optional content) The document’s 
            /// optional content properties dictionary.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional)]
            public const string OCProperties = "/OCProperties";

            /// <summary>
            /// (Optional; PDF 1.5) A permissions dictionary that specifies user access permissions 
            /// for the document.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional)]
            public const string Perms = "/Perms";

            /// <summary>
            /// (Optional; PDF 1.5) A dictionary containing attestations regarding the content of a 
            /// PDF document, as it relates to the legality of digital signatures.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional)]
            public const string Legal = "/Legal";

            /// <summary>
            /// (Optional; PDF 1.7) An array of requirement dictionaries representing
            /// requirements for the document.
            /// </summary>
            [KeyInfo("1.7", KeyType.Array | KeyType.Optional)]
            public const string Requirements = "/Requirements";

            /// <summary>
            /// (Optional; PDF 1.7) A collection dictionary that a PDF consumer uses to enhance
            /// the presentation of file attachments stored in the PDF document.
            /// </summary>
            [KeyInfo("1.7", KeyType.Dictionary | KeyType.Optional)]
            public const string Collection = "/Collection";

            /// <summary>
            /// Optional; deprecated in PDF 2.0) A flag used to expedite the display of PDF documents containing XFA forms.
            /// It specifies whether the document must be regenerated when the document is first opened.
            /// If true, the viewer application treats the document as a shell and regenerates the content
            /// when the document is opened, regardless of any dynamic forms settings that appear in the XFA
            /// stream itself. This setting is used to expedite the display of documents whose layout varies
            /// depending on the content of the XFA streams. 
            /// If false, the viewer application does not regenerate the content when the document is opened.
            /// See the XML Forms Architecture (XFA) Specification (Bibliography).
            /// Default value: false.
            /// </summary>
            [KeyInfo("1.7", KeyType.Boolean | KeyType.Optional | KeyType.DeprecatedIn20)]
            public const string NeedsRendering = "/NeedsRendering";

            // ----- New PDF 2.0 entries -----

            // DSS - nyi

            /// <summary>
            /// (Optional; PDF 2.0) An array of one or more file specification dictionaries which denote the associated
            /// files for this PDF document. 
            /// For unencrypted wrapper documents for an encrypted payload document the AF key is required and shall
            /// include a reference to the file specification dictionary for the encrypted payload document.
            /// </summary>
            [KeyInfo("2.0", KeyType.ArrayOfDictionaries | KeyType.Optional, ObjectType = typeof(PdfArrayOfDictionaries))]
            public const string AF = "/AF";

            // DPartRoot - nyi

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
