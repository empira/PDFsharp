// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

// v7.0.0 REVIEW

namespace PdfSharp.Pdf.Actions
{
    /// <summary>
    /// Represents a PDF Goto action.
    /// </summary>
    public sealed class PdfGoToAction : PdfAction
    {
        // Reference 2.0: 12.6.4.2 Go-To actions / Page 511

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGoToAction"/> class.
        /// </summary>
        public PdfGoToAction()
        {
            Inititalize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGoToAction"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        public PdfGoToAction(PdfDocument document)
            : base(document)
        {
            Inititalize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfGoToAction(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Creates a link within the current document.
        /// </summary>
        /// <param name="destinationName">The Named Destination’s name in the target document.</param>
        public static PdfGoToAction CreateGoToAction(string destinationName)
        {
            var action = new PdfGoToAction
            {
                _destinationName = destinationName
            };
            return action;
        }
        string _destinationName = default!;

        void Inititalize()
        {
            Elements.SetName(PdfAction.Keys.S, PdfNamedActionTypes.GoTo);
        }

        internal override void WriteObject(PdfWriter writer)
        {
            Elements.SetString(PdfRemoteGoToAction.Keys.D, _destinationName);

            base.WriteObject(writer);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal new class Keys : PdfAction.Keys
        {
            // Reference 2.0: Table 202 — Additional entries specific to a go-to action / Page 511

            ///// <summary>
            ///// (Required) The type of action that this dictionary describes;
            ///// shall be GoTo for a go-to action.
            ///// </summary>
            //[KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "GoTo")]
            //public const string S = "/S";

            /// <summary>
            /// (Required) The destination to jump to (see Section 8.2.1, “Destinations”).
            /// </summary>
            //[KeyInfo(KeyType.Name | KeyType.ByteString | KeyType.Array | KeyType.Required)] // #US373 Cannot "|" types.
            [KeyInfo(KeyType.NameOrByteStringOrArray | KeyType.Required)]
            public const string D = "/D";

            /// <summary>
            /// (Required) The destination to jump to (see Section 8.2.1, “Destinations”).
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string SD = "/SD";
        }
    }
}
