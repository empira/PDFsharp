// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO review

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF text annotation.
    /// </summary>
    public sealed class PdfTextAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.4  Text annotations / Page 482

        [Obsolete("PDFsharp 6.4: Use a constructor with a PDF document parameter.")]
        public PdfTextAnnotation() 
            => throw new NotImplementedException("PDFsharp 6.4: Use a constructor with a PDF document parameter.");

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTextAnnotation"/> class.
        /// </summary>
        public PdfTextAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfTextAnnotation(PdfDictionary dict)
            : base(dict)
        {
            Initialize();
        }

        void Initialize()
        {
            Elements.SetName(Keys.Subtype, "/Text");
            // By default make a yellow comment.
            Icon = PdfTextAnnotationIcons.Comment;
            //Color = XColors.Yellow;
        }

        //    public static PdfTextAnnotation CreateDocumentLink(PdfRectangle rect, int destinationPage)
        //    {
        //      PdfTextAnnotation link = new PdfTextAnnotation();
        //      //link.linkType = PdfTextAnnotation.LinkType.Document;
        //      //link.Rectangle = rect;
        //      //link.destPage = destinationPage;
        //      return link;
        //    }

        /// <summary>
        /// Gets or sets a flag indicating whether the annotation should initially be displayed open.
        /// </summary>
        public bool Open
        {
            get => Elements.GetBoolean(Keys.Open);
            set => Elements.SetBoolean(Keys.Open, value);
        }

        /// <summary>
        /// Gets or sets an icon to be used in displaying the annotation.
        /// </summary>
        public PdfTextAnnotationIcons Icon
        {
            get
            {
                string value = Elements.GetName(Keys.Name);
                if (value == "" || value == "/")
                    return PdfTextAnnotationIcons.NoIcon;
                value = value[1..];
                if (!Enum.IsDefined(typeof(PdfTextAnnotationIcons), value))
                    return PdfTextAnnotationIcons.NoIcon;
                return (PdfTextAnnotationIcons)Enum.Parse(typeof(PdfTextAnnotationIcons), value, false);
            }
            set
            {
                if (Enum.IsDefined(typeof(PdfTextAnnotationIcons), value) &&
                  PdfTextAnnotationIcons.NoIcon != value)
                {
                    Elements.SetName(Keys.Name, "/" + value.ToString());
                }
                else
                    Elements.Remove(Keys.Name);
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 175 — Additional entries specific to a text annotation / Page 482

            /// <summary>
            /// (Optional) A flag specifying whether the annotation shall initially be displayed open.
            /// Default value: false (closed).
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Open = "/Open";

            /// <summary>
            /// (Optional) The name of an icon that shall be used in displaying the annotation.
            /// Interactive PDF processors shall provide predefined icon appearances for at least
            /// the following standard names:
            ///   Comment, Key, Note, Help, NewParagraph, Paragraph, Insert
            /// Additional names may be supported as well.
            /// Default value: Note.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            /// <summary>
            /// (Optional; PDF 1.5) The state to which the original annotation shall be set.
            /// Default: Unmarked if StateModel is Marked; None if StateModel is Review.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string State = "/State";

            /// <summary>
            /// (Required if State is present, otherwise optional; PDF 1.5)
            /// The state model corresponding to State.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string StateModel = "/StateModel";

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
