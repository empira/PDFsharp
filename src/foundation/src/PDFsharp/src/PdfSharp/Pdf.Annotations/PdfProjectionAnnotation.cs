// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF projection annotation.
    /// </summary>
    public sealed class PdfProjectionAnnotation : PdfMarkupAnnotation
    {
        // Reference 2.0: 12.5.6.24  Projection annotations / Page 505

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfProjectionAnnotation"/> class.
        /// </summary>
        public PdfProjectionAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfProjectionAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Projection);
        }

        // This annotation has no special keys of its own.
    }
}
