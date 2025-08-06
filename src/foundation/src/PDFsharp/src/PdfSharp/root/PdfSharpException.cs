// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp
{
    /// <summary>
    /// Base class of all exceptions in the PDFsharp library.
    /// </summary>
    public class PdfSharpException : Exception
    {
        // The class is not yet used

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSharpException"/> class.
        /// </summary>
        public PdfSharpException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSharpException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public PdfSharpException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSharpException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PdfSharpException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }
}
