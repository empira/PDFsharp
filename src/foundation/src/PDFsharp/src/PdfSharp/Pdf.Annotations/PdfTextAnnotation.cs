// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a text annotation.
    /// </summary>
    public sealed class PdfTextAnnotation : PdfAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTextAnnotation"/> class.
        /// </summary>
        public PdfTextAnnotation()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTextAnnotation"/> class.
        /// </summary>
        public PdfTextAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        void Initialize()
        {
            Elements.SetName(Keys.Subtype, "/Text");
            // By default make a yellow comment.
            Icon = PdfTextAnnotationIcon.Comment;
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
        public PdfTextAnnotationIcon Icon
        {
            get
            {
                string value = Elements.GetName(Keys.Name);
                if (value == "")
                    return PdfTextAnnotationIcon.NoIcon;
                value = value.Substring(1);
                if (!Enum.IsDefined(typeof(PdfTextAnnotationIcon), value))
                    return PdfTextAnnotationIcon.NoIcon;
                return (PdfTextAnnotationIcon)Enum.Parse(typeof(PdfTextAnnotationIcon), value, false);
            }
            set
            {
                if (Enum.IsDefined(typeof(PdfTextAnnotationIcon), value) &&
                  PdfTextAnnotationIcon.NoIcon != value)
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
            /// <summary>
            /// (Optional) A flag specifying whether the annotation should initially be displayed open.
            /// Default value: false (closed).
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string Open = "/Open";

            /// <summary>
            /// (Optional) The name of an icon to be used in displaying the annotation. Viewer
            /// applications should provide predefined icon appearances for at least the following
            /// standard names:
            ///   Comment 
            ///   Help 
            ///   Insert
            ///   Key 
            ///   NewParagraph 
            ///   Note
            ///   Paragraph
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            //State
            //StateModel

            public static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
