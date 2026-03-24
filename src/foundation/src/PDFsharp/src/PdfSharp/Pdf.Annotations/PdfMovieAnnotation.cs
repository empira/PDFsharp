// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF movie annotation.
    /// </summary>
    public sealed class PdfMovieAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.17  Movie annotations / Page 497

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMovieAnnotation"/> class.
        /// </summary>
        public PdfMovieAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfMovieAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Movie);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The title of the movie annotation. Movie actions (12.6.4.10, "Movie actions")
            /// may use this title to reference the movie annotation.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string T = "/T";

            /// <summary>
            /// (Required) A movie dictionary that shall describe the movie’s static characteristics
            /// (see 13.4, "Movies").
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required)]
            public const string Movie = "/Movie";

            /// <summary>
            /// (Optional) A flag or dictionary specifying whether and how to play the movie when the
            /// annotation is activated. If this value is a dictionary, it shall be a movie activation
            /// dictionary (see 13.4, "Movies") specifying how to play the movie. If the value is the
            /// boolean true, the movie shall be played using default activation parameters. If the
            /// value is false, the movie shall not be played. Default value: true.
            /// </summary>
            [KeyInfo(KeyType.BooleanOrDictionary | KeyType.Optional)]
            public const string A = "/A";

            // ReSharper restore InconsistentNaming
            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}