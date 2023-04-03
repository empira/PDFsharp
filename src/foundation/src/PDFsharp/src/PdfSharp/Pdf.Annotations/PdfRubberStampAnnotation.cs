// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a rubber stamp annotation.
    /// </summary>
    public sealed class PdfRubberStampAnnotation : PdfAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRubberStampAnnotation"/> class.
        /// </summary>
        public PdfRubberStampAnnotation()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRubberStampAnnotation"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public PdfRubberStampAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        void Initialize()
        {
            Elements.SetName(Keys.Subtype, "/Stamp");
            Color = XColors.Yellow;
        }

        /// <summary>
        /// Gets or sets an icon to be used in displaying the annotation.
        /// </summary>
        public PdfRubberStampAnnotationIcon Icon
        {
            get
            {
                string value = Elements.GetName(Keys.Name);
                if (value == "")
                    return PdfRubberStampAnnotationIcon.NoIcon;
                value = value.Substring(1);
                if (!Enum.IsDefined(typeof(PdfRubberStampAnnotationIcon), value))
                    return PdfRubberStampAnnotationIcon.NoIcon;
                return (PdfRubberStampAnnotationIcon)Enum.Parse(typeof(PdfRubberStampAnnotationIcon), value, false);
            }
            set
            {
                if (Enum.IsDefined(typeof(PdfRubberStampAnnotationIcon), value) &&
                  PdfRubberStampAnnotationIcon.NoIcon != value)
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
            /// (Optional) The name of an icon to be used in displaying the annotation. Viewer
            /// applications should provide predefined icon appearances for at least the following
            /// standard names:
            ///   Approved
            ///   AsIs
            ///   Confidential
            ///   Departmental
            ///   Draft
            ///   Experimental
            ///   Expired
            ///   Final
            ///   ForComment
            ///   ForPublicRelease
            ///   NotApproved
            ///   NotForPublicRelease
            ///   Sold
            ///   TopSecret
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
