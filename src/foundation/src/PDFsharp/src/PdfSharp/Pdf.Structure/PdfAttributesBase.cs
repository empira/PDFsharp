// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Structure
{
    /// <summary>
    /// Base class for PDF attributes objects.
    /// </summary>
    public abstract class PdfAttributesBase : PdfDictionary
    {
        /// <summary>
        /// Constructor of the abstract <see cref="PdfSharp.Pdf.Structure.PdfAttributesBase"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        internal PdfAttributesBase(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Constructor of the abstract <see cref="PdfSharp.Pdf.Structure.PdfAttributesBase"/> class.
        /// </summary>
        protected PdfAttributesBase()
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference: TABLE 10.14  Entry common to all attribute object dictionaries / Page 873
            // Reference: TABLE 10.28  Standard attribute owners / Page 914

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) The name of the application or plug-in extension owning the attribute data.
            /// The name must conform to the guidelines described in Appendix E
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string O = "/O";

            // ReSharper restore InconsistentNaming
        }
    }
}
