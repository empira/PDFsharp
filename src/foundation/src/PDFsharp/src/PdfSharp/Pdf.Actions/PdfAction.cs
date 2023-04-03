// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Actions
{
    /// <summary>
    /// Represents the base class for all PDF actions.
    /// </summary>
    public abstract class PdfAction : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAction"/> class.
        /// </summary>
        protected PdfAction()
        {
            Elements.SetName(Keys.Type, "/Action");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAction"/> class.
        /// </summary>
        /// <param name="document">The document that owns this object.</param>
        protected PdfAction(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Action");
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes;
            /// if present, must be Action for an action dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Action")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of action that this dictionary describes.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string S = "/S";

            /// <summary>
            /// (Optional; PDF 1.2) The next action or sequence of actions to be performed
            /// after the action represented by this dictionary. The value is either a
            /// single action dictionary or an array of action dictionaries to be performed
            /// in order; see below for further discussion.
            /// </summary>
            [KeyInfo(KeyType.ArrayOrDictionary | KeyType.Optional)]
            public const string Next = "/Next";
        }
    }
}
