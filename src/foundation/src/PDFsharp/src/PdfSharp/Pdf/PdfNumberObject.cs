// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Base class for indirect number values.
    /// </summary>
    public abstract class PdfNumberObject : PdfPrimitiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNumberObject"/> class.
        /// </summary>
        protected PdfNumberObject()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNumberObject"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        protected PdfNumberObject(PdfDocument document)
            : base(document, true)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNumberObject"/> class
        /// without making it indirect.
        /// Used in PDF parser only.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="createIndirect">If true creates an indirect object.</param>
        protected internal PdfNumberObject(PdfDocument document, bool createIndirect)
            : base(document, createIndirect)
        { }

        /// <summary>
        /// Gets a value indicating whether this instance is a 32-bit signed integer.
        /// </summary>
        public bool IsInteger { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a 64-bit signed integer.
        /// </summary>
        public bool IsLongInteger { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a floating point number.
        /// </summary>
        public bool IsReal { get; protected set; }
    }
}
