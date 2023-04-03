// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Structure
{
    /// <summary>
    /// Represents a marked-content reference.
    /// </summary>
    public sealed class PdfObjectReference : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObjectReference"/> class.
        /// </summary>
        public PdfObjectReference()
        {
            Elements.SetName(Keys.Type, "/OBJR");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObjectReference"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        public PdfObjectReference(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/OBJR");
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            // Reference: TABLE 10.12  Entries in an object reference dictionary / Page 868

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes;
            /// must be OBJR for an object reference.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "OBJR")]
            public const string Type = "/Type";

            /// <summary>
            /// (Optional; must be an indirect reference) The page object representing the page
            /// on which the object is rendered. This entry overrides any Pg entry in the
            /// structure element containing the object reference;
            /// it is required if the structure element has no such entry.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Pg = "/Pg";

            /// <summary>
            /// (Required; must be an indirect reference) The referenced object.
            /// </summary>
            [KeyInfo(KeyType.Required)]
            public const string Obj = "/Obj";

            // ReSharper restore InconsistentNaming
        }
    }
}
