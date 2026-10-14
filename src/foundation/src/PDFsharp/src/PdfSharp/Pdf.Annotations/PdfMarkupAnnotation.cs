// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Base class of all Markup annotations.
    /// </summary>
    public abstract class PdfMarkupAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.2  Markup annotations / Page 477
        [Obsolete("PDFsharp 6.4: Use a constructor with a PDF document parameter.")]
        protected PdfMarkupAnnotation()
            => throw new NotImplementedException("PDFsharp 6.4: Use a constructor with a PDF document parameter.");

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfMarkupAnnotation"/> class.
        /// </summary>
        protected PdfMarkupAnnotation(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSquareAnnotation"/> class.
        /// </summary>
        protected PdfMarkupAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the text label to be displayed in the title bar of the annotation’s
        /// pop-up window when open and active. By convention, this entry identifies
        /// the user who added the annotation.
        /// </summary>
        public string Title
        {
            get => Elements.GetString(Keys.T, true);
            set
            {
                Elements.SetString(Keys.T, value);
                Elements.SetDateTime(PdfAnnotation.Keys.M, DateTimeOffset.Now);
            }
        }

        /// <summary>
        /// Gets or sets text representing a short description of the subject being
        /// addressed by the annotation.
        /// </summary>
        public string Subject
        {
            get => Elements.GetString(Keys.Subj, true);
            set
            {
                Elements.SetString(Keys.Subj, value);
                Elements.SetDateTime(PdfAnnotation.Keys.M, DateTimeOffset.Now);
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 172 — Additional entries in an annotation dictionary specific to markup annotations / Page 479

            // ReSharper disable InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            /// <summary>
            /// (Optional; PDF 1.1) The text label that shall be displayed in the title bar of the
            /// annotation’s popup window when open and active. This entry shall identify the user
            /// who added the annotation.
            /// </summary>
            [KeyInfo("1.1", KeyType.TextString | KeyType.Optional)]
            public const string T = "/T";

            /// <summary>
            /// (Optional; PDF 1.3) An indirect reference to a pop-up annotation for entering or
            /// editing the text associated with this annotation.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
            public const string Popup = "/Popup";

            /// <summary>
            /// (Optional; PDF 1.5) A rich text string (see Adobe XML Architecture, XML Forms
            /// Architecture (XFA) Specification, version 3.3) that shall be displayed in the
            /// popup window when the annotation is opened.
            /// </summary>
            [KeyInfo("1.5", KeyType.TextStringOrTextStream | KeyType.Optional)]
            public const string RC = "/RC";

            /// <summary>
            /// (Optional; PDF 1.5) The date and time (7.9.4, "Dates") when the annotation
            /// was created.
            /// </summary>
            [KeyInfo("1.5", KeyType.Date | KeyType.Optional)]
            public const string CreationDate = "/CreationDate";

            /// <summary>
            /// (Required if an RT entry is present, otherwise optional; PDF 1.5) A reference to
            /// the annotation that this annotation is "in reply to." Both annotations shall be on
            /// the same page of the document. The relationship between the two annotations shall
            /// be specified by the RT entry.
            /// If this entry is present in an FDF file (see 12.7.8, "Forms data format"), its type
            /// shall not be a dictionary but a text string containing the contents of the NM entry
            /// of the annotation being replied to, to allow for a situation where the annotation
            /// being replied to is not in the same FDF file.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional)]
            public const string IRT = "/IRT";

            /// <summary>
            /// (Optional; PDF 1.5) Text representing a short description of the subject being
            /// addressed by the annotation.
            /// </summary>
            [KeyInfo("1.5", KeyType.TextString | KeyType.Optional)]
            public const string Subj = "/Subj";

            /// <summary>
            /// (Optional; meaningful only if IRT is present; PDF 1.6) A name specifying the
            /// relationship (the "reply type") between this annotation and one specified
            /// by IRT.
            /// Valid values are:
            /// R The annotation is considered a reply to the annotation specified by IRT.
            ///   Interactive PDF processors shall not display replies to an annotation
            ///   individually but together in the form of threaded comments.
            /// Group The annotation shall be grouped with the annotation specified by IRT;
            ///       see the discussion following this Table.
            /// Default value: R.
            /// </summary>
            [KeyInfo("1.6", KeyType.Name | KeyType.Optional)]
            public const string RT = "/RT";

            /// <summary>
            /// (Optional; PDF 1.6) A name describing the intent of the markup annotation.
            /// Intents allow interactive PDF processors to distinguish between different
            /// uses and behaviours of a single markup annotation type. If this entry is
            /// not present or its value is the same as the annotation type, the annotation
            /// shall have no explicit intent and should behave in a generic manner in an
            /// interactive PDF processor. Free text annotations ("Table 177 — Additional
            /// entries specific to a free text annotation"), line annotations
            /// ("Table 178 — Additional entries specific to a line annotation"),
            /// polygon annotations ("Table 181 — Additional entries specific to a polygon
            /// or polyline annotation"), (PDF 1.7) polyline annotations
            /// ("Table 181 — Additional entries specific to a polygon or polyline
            /// annotation") and stamp annotations (“Table 184 — Additional entries
            /// specific to a rubber stamp annotation") have defined intents, whose values
            /// are enumerated in the corresponding tables.
            /// </summary>
            [KeyInfo("1.6", KeyType.Name | KeyType.Optional)]
            public const string IT = "/IT";

            // ReSharper restore InconsistentNaming

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
