// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member because it is for internal use only.

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Defines the action to be taken by PDFsharp if a problem occurs during reading
    /// a PDF file.
    /// </summary>
    public enum PdfReaderProblemBehavior // PdfReaderProblemBehavior
    {
        /// <summary>
        /// Silently ignore the parser error.
        /// </summary>
        SilentlyIgnore = 0,

        /// <summary>
        /// Log an information.
        /// </summary>
        LogInformation = 1,

        /// <summary>
        /// Log a  warning.
        /// </summary>
        LogWarning = 2,

        /// <summary>
        /// Log an  error.
        /// </summary>
        LogError = 3,

        /// <summary>
        /// Throw a parser exception.
        /// </summary>
        ThrowException = 4,
    }

    public enum PdfReaderProblemType
    {
        /// <summary>
        /// A reference to a not existing object occurs.
        /// PDF reference states that such a reference is considered to
        /// be a reference to the Null-Object, but it is worth to be reported.
        /// </summary>
        InvalidObjectReference = 1,

        /// <summary>
        /// The specified length of a stream is invalid.
        /// </summary>
        InvalidStreamLength = 2,

        /// <summary>
        /// The specified length is an indirect reference to an object in
        /// an Object stream that is not yet decrypted.
        /// </summary>
        UnreachableStreamLength = 3,

        /// <summary>
        /// The ID of an object occurs more than once.
        /// </summary>
        MultipleObjectId = 4,

        // ... there is more...
    }
    public class PdfReaderProblemDetails
    {
        public PdfReaderProblemType Type { get; set; }

        /// <summary>
        /// Gets or sets a human-readable title for this problem.
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Gets or sets a human-readable more detailed description for this problem.
        /// </summary>
        public string Description { get; set; } = default!;

        /// <summary>
        /// Gets or sets a human-readable description of the action taken by PDFsharp for this problem.
        /// </summary>
        public string Behavior { get; set; } = default!;
    }

    public delegate void ReaderProblemDelegate(PdfReaderProblemDetails details);

    /// <summary>
    /// UNDER CONSTRUCTION
    /// </summary>
    public class PdfReaderOptions
    {
        public PdfReaderProblemBehavior InvalidStreamLength { get; set; } = PdfReaderProblemBehavior.SilentlyIgnore;

        public PdfReaderProblemBehavior ReferenceToUndefinedObject { get; set; } = PdfReaderProblemBehavior.SilentlyIgnore;

        public PdfReaderProblemBehavior IssuesWithDecryption { get; set; } = PdfReaderProblemBehavior.SilentlyIgnore;

        public ReaderProblemDelegate? ReaderProblemCallback { get; set; }

        // Testing only

        //public bool UseOldCode { get; set; } = false;
    }
}
