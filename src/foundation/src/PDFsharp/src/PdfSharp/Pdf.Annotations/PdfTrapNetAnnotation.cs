// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF trap network annotation.
    /// </summary>
    public sealed class PdfTrapNetAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.21  Trap network annotations / Page 501

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLineAnnotation"/> class.
        /// </summary>
        public PdfTrapNetAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfTrapNetAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.TrapNet);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required if Version and AnnotStates are absent; shall be absent if Version and AnnotStates
            /// are present; PDF 1.4) The date and time (see 7.9.4, "Dates") when the trap network was
            /// most recently modified.
            /// </summary>
            [KeyInfo(KeyType.Date | KeyType.Required)]
            public const string LastModified = "/LastModified";

            /// <summary>
            /// (Required if AnnotStates is present; shall be absent if LastModified is present) An unordered
            /// array of all objects present in the page description at the time the trap networks were
            /// generated and that, if changed, could affect the appearance of the page. If present,
            /// the array shall include the following objects:
            /// • All content streams identified in the page object’s Contents entry(see 7.7.3.3,
            ///   "Page objects")
            /// • All resource objects(other than procedure sets) in the page’s resource dictionary
            ///   (see 7.8.3, "Resource dictionaries")
            /// • All resource objects(other than procedure sets) in the resource dictionaries of any
            ///   form XObjects on the page(see 8.10, "Form XObjects")
            /// • All OPI dictionaries associated with XObjects on the page(see 14.11.7, "Open prepress
            ///   interface (OPI)"). This entry is deprecated in PDF 2.0.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string Version = "/Version";

            /// <summary>
            /// (Required if Version is present; shall be absent if LastModified is present) An array
            /// of name objects representing the appearance states (value of the AS entry) for annotations
            /// associated with the page. The appearance states shall be listed in the same order as
            /// the annotations in the page’s Annots array (see 7.7.3.3, "Page objects"). For an annotation
            /// with no AS entry, the corresponding array element should be null. No appearance state
            /// shall be included for the trap network annotation itself.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string AnnotStates = "/AnnotStates";

            /// <summary>
            /// (Optional) An array of font dictionaries representing fonts that were fauxed (replaced
            /// by substitute fonts) during the generation of trap networks for the page.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string FontFauxing = "/FontFauxing";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}