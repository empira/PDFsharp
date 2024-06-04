// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Exception thrown by PdfReader, indicating that the object can not (yet) be read.
    /// </summary>
    public class ObjectNotAvailableException : PdfReaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNotAvailableException"/> class.
        /// </summary>
        public ObjectNotAvailableException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNotAvailableException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ObjectNotAvailableException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNotAvailableException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ObjectNotAvailableException(string message, Exception innerException)
            :
            base(message, innerException)
        { }
    }
}
