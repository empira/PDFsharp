// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a file attachment annotation.
    /// </summary>
    public sealed class PdfFileAttachmentAnnotation : PdfAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFileAttachmentAnnotation"/> class.
        /// </summary>
        public PdfFileAttachmentAnnotation()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFileAttachmentAnnotation"/> class.
        /// </summary>
        public PdfFileAttachmentAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        void Initialize()
        {
            Elements.SetName(Keys.Subtype, "/FileAttachment");
            Icon = PdfFileAttachmentAnnotationIcon.PushPin;
            Flags = PdfAnnotationFlags.Locked;
        }

        /// <summary>
        /// Creates a file attachment annotation.
        /// </summary>
        public static PdfFileAttachmentAnnotation CreateFileAttachmentAnnotation(PdfRectangle rect, PdfFileSpecification fileSpecification)
        {
            var annot = new PdfFileAttachmentAnnotation
            {
                Rectangle = rect,
            };
            return annot;
        }

        /// <summary>
        /// Gets or sets an icon to be used in displaying the annotation.
        /// </summary>
        public PdfFileAttachmentAnnotationIcon Icon
        {
            get
            {
                string value = Elements.GetName(Keys.Name);
                if (value == "")
                    return PdfFileAttachmentAnnotationIcon.NoIcon;
                value = value.Substring(1);
                if (!Enum.IsDefined(typeof(PdfFileAttachmentAnnotationIcon), value))
                    return PdfFileAttachmentAnnotationIcon.NoIcon;
                return (PdfFileAttachmentAnnotationIcon)Enum.Parse(typeof(PdfFileAttachmentAnnotationIcon), value, false);
            }
            set
            {
                if (Enum.IsDefined(typeof(PdfFileAttachmentAnnotationIcon), value) &&
                  PdfFileAttachmentAnnotationIcon.NoIcon != value)
                {
                    Elements.SetName(Keys.Name, "/" + value.ToString());
                }
                else
                    Elements.Remove(Keys.Name);
            }
        }

        public IAnnotationAppearanceHandler CustomAppearanceHandler { get; set; }

        /// <summary>
        /// Creates the custom appearance form X object for this annotation
        /// </summary>
        public void RenderCustomAppearance()
        {
            var visible = !(Rectangle.X1 + Rectangle.X2 + Rectangle.Y1 + Rectangle.Y2 == 0);

            if (!visible)
                return;

            if (CustomAppearanceHandler == null)
                throw new Exception("AppearanceHandler is null");

            XForm form = new XForm(_document, Rectangle.Size);
            XGraphics gfx = XGraphics.FromForm(form);

            CustomAppearanceHandler.DrawAppearance(gfx, Rectangle.ToXRect());

            form.DrawingFinished();

            // Get existing or create new appearance dictionary
            if (Elements[PdfAnnotation.Keys.AP] is not PdfDictionary ap)
            {
                ap = new PdfDictionary(_document);
                Elements[PdfAnnotation.Keys.AP] = ap;
            }

            // Set XRef to normal state
            ap.Elements["/N"] = form.PdfForm.Reference;

            form.PdfRenderer.Close();

            Icon = PdfFileAttachmentAnnotationIcon.NoIcon;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal new class Keys : PdfAnnotation.Keys
        {
            /// <summary>
            /// (Required) The file associated with this annotation.
            /// </summary>
            [KeyInfo(KeyType.FileSpecification | KeyType.Required)]
            public const string FS = "/FS";

            /// <summary>
            /// (Optional) The name of an icon that shall be used in displaying the annotation. 
            /// Conforming readers shall provide predefined icon appearances for at least the following names:
            ///   Graph 
            ///   PushPin 
            ///   Paperclip
            ///   Tag
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            public static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
