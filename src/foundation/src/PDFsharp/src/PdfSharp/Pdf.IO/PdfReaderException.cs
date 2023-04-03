// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Exception thrown by PdfReader.
    /// </summary>
    public class PdfReaderException : PdfSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReaderException"/> class.
        /// </summary>
        public PdfReaderException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReaderException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PdfReaderException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfReaderException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PdfReaderException(string message, Exception innerException)
            :
            base(message, innerException)
        { }
    }
}
