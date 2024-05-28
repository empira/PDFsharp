// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Actions
{
    /// <summary>
    /// Represents a PDF Goto action.
    /// </summary>
    public sealed class PdfGoToAction : PdfAction
    {
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
            Elements.SetName(PdfAction.Keys.Type, "/Action");
            Elements.SetName(PdfAction.Keys.S, "/GoTo");
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
            ///// <summary>
            ///// (Required) The type of action that this dictionary describes;
            ///// must be GoTo for a go-to action.
            ///// </summary>
            //[KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "GoTo")]
            //public const string S = "/S";

            /// <summary>
            /// (Required) The destination to jump to (see Section 8.2.1, “Destinations”).
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.ByteString | KeyType.Array | KeyType.Required)]
            public const string D = "/D";
        }
    }
}
