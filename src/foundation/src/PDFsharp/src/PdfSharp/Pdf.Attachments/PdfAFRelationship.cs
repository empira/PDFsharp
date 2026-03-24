// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Attachments
{
    /// <summary>
    /// Specifies the associated files relationship.
    /// (Optional; PDF 2.0) A name value that represents the relationship between the component of this PDF document
    /// that refers to this file specification and the associated file denoted by this file specification dictionary.
    /// These values represent the following relationships:
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class PdfAFRelationship // TODO: clean up   #US322
    {
        /// <summary>
        /// shall be used when the relationship is not known or cannot be described using one of the other values.
        /// Unspecified is to be used only when no other value correctly reflects the relationship.
        /// NOTE 3 The value of AFRelationship does not explicitly provide any processing instructions for a PDF processor.It is provided for information and semantic purposes for those processors that are able to use such additional information.
        /// </summary>
        public const string Unspecified = nameof(Unspecified);

        /// <summary>
        /// shall be used if this file specification is the original source material for the associated content.
        /// </summary>
        public const string Source = nameof(Source);

        /// <summary>
        /// shall be used if this file specification represents information used to derive a visual presentation – such as for a table or a graph.
        /// </summary>
        public const string Data = nameof(Data);

        /// <summary>
        /// shall be used if this file specification is an alternative representation of content, for example audio.
        /// </summary>
        public const string Alternative = nameof(Alternative);

        /// <summary>
        /// shall be used if this file specification represents a supplemental representation of the original source or data that may be more easily consumable (e.g., A MathML version of an equation).
        /// </summary>
        public const string Supplement = nameof(Supplement);

        /// <summary>
        /// shall be used if this file specification is an encrypted payload document that should be displayed to the user if the PDF processor has the cryptographic filter needed to decrypt the document.
        /// </summary>
        public const string EncryptedPayload = nameof(EncryptedPayload);

        /// <summary>
        /// shall be used if this file specification is the data associated with the AcroForm (see 12.7.3, "Interactive form dictionary") of this PDF.
        /// </summary>
        public const string FormData = nameof(FormData);

        /// <summary>
        /// shall be used if this file specification is a schema definition for the associated object (e.g. an XML schema associated with a metadata stream).
        /// </summary>
        public const string Schema = nameof(Schema);
    }
}

