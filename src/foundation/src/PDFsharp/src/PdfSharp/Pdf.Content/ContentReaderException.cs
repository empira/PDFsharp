// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Exception thrown by ContentReader.
    /// </summary>
    public class ContentReaderException : PdfSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReaderException"/> class.
        /// </summary>
        public ContentReaderException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReaderException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ContentReaderException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReaderException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ContentReaderException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }
}
