// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

// v7.0.0 TODO review creator functions

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF rubber stamp annotation.
    /// </summary>
    public sealed class PdfStampAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.12  Rubber stamp annotations / Page 494

        [Obsolete("PDFsharp 6.4: Use a constructor with a PDF document parameter.")]
        public PdfStampAnnotation()
            => throw new NotImplementedException("PDFsharp 6.4: Use a constructor with a PDF document parameter.");

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfStampAnnotation"/> class.
        /// </summary>
        public PdfStampAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfStampAnnotation(PdfDictionary dict)
            : base(dict)
        {
            Initialize();
        }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, "/Stamp");
            Color = XColors.Yellow;
        }

        /// <summary>
        /// Gets or sets an icon to be used in displaying the annotation.
        /// </summary>
        public PdfStampAnnotationIcons Icon
        {
            get
            {
                string value = Elements.GetName(Keys.Name);
                if (value == "")
                    return PdfStampAnnotationIcons.NoIcon;
                value = value.Substring(1);
                if (!Enum.IsDefined(typeof(PdfStampAnnotationIcons), value))
                    return PdfStampAnnotationIcons.NoIcon;
                return (PdfStampAnnotationIcons)Enum.Parse(typeof(PdfStampAnnotationIcons), value, false);
            }
            set
            {
                if (Enum.IsDefined(typeof(PdfStampAnnotationIcons), value) &&
                  PdfStampAnnotationIcons.NoIcon != value)
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
            // Reference 2.0: Table 184 — Additional entries specific to a rubber stamp annotation / Page 494

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The name of an icon that shall be used in displaying the annotation.
            /// PDF writers should include this entry and PDF readers should provide predefined icon
            /// appearances for at least the following standard names:
            ///   Approved, Experimental, NotApproved, AsIs, Expired, NotForPublicRelease, Confidential,
            ///   Final, Sold, Departmental, ForComment, TopSecret, Draft, ForPublicRelease
            /// Additional names may be supported as well.
            /// Default value: Draft.
            /// If the IT key is present and its value is not Stamp, this Name key shall not be present.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Name = "/Name";

            /// <summary>
            /// (Optional; PDF 2.0) A name that shall describe the intent of the stamp. The following
            /// values shall be valid:<br/>
            ///   StampSnapshot The appearance of this annotation has been taken from preexisting PDF content.<br/>
            ///   StampImage    The appearance of this annotation is an Image.<br/>
            ///   Stamp         The appearance of this annotation is a rubber stamp.<br/>
            /// Default value: Stamp
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string IT = "/IT";

            // ReSharper restore InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }

    /// <summary>
    /// Use PdfStampAnnotation.
    /// </summary>
    [Obsolete("Use PdfStampAnnotation.")]
    public class PdfRubberStampAnnotation
    { }
}
