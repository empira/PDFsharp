// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF sound annotation.
    /// </summary>
    public sealed class PdfSoundAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.16  Sound annotations / Page 496

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSoundAnnotation"/> class.
        /// </summary>
        public PdfSoundAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfSoundAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Sound);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) A sound object defining the sound that shall be played when the annotation
            /// is activated (see 13.3, "Sounds").
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Required)]
            public const string Sound = "/Sound";

            /// <summary>
            /// (Optional) The name of an icon that shall be used in displaying the annotation.
            /// PDF writers should include this entry and PDF readers should provide predefined icon
            /// appearances for at least the standard names Speaker and Mic. Additional names may be
            /// supported as well. Default value: Speaker.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}