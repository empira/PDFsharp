// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF file attachment annotation.
    /// </summary>
    public sealed class PdfFileAttachmentAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.15  File attachment annotations / Page 496

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFileAttachmentAnnotation"/> class.
        /// </summary>
        public PdfFileAttachmentAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFileAttachmentAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.FileAttachment);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            /// <summary>
            /// (Required) The file associated with this annotation.
            /// </summary>
            [KeyInfo(KeyType.FileSpecification | KeyType.Required)]
            public const string FS = "/FS";

            /// <summary>
            /// (Optional) The name of an icon that shall be used in displaying the annotation.
            /// PDF writers should include this entry and PDF readers should provide predefined
            /// icon appearances for at least the following standard names:
            /// Graph, PushPin, Paperclip, Tag
            /// Additional names may be supported as well.Default value: PushPin.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}