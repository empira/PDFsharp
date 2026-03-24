// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 Ready

namespace PdfSharp.Pdf.Actions
{
    /// <summary>
    /// Represents the base class for all PDF actions.
    /// </summary>
    public abstract class PdfAction : PdfDictionary
    {
        // Reference 2.0: 12.6  Actions / Page 506

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
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfAction(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal class Keys : KeysBase
        {
            // Reference 2.0: Table 196 — Entries common to all action dictionaries / Page 506

            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes;
            /// if present, shall be Action for an action dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Action")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of action that this dictionary describes;
            /// see PdfActionTypes for specific values.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string S = "/S";

            /// <summary>
            /// (Optional; PDF 1.2) The next action or sequence of actions that shall be performed after
            /// the action represented by this dictionary. The value is either a single action dictionary
            /// or an array of action dictionaries that shall be performed in order; see Note 1 for
            /// further discussion.
            /// </summary>
            [KeyInfo(KeyType.ArrayOrDictionary | KeyType.Optional)]
            public const string Next = "/Next";
        }
    }
}
