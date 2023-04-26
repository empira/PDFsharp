// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a PDF page object.
    /// </summary>
    class PdfPageInheritableObjects : PdfDictionary
    {
        public PdfPageInheritableObjects()
        { }

        // TODO Inheritable Resources not yet supported

        /// <summary>
        /// 
        /// </summary>
        public PdfRectangle MediaBox
        {
            get => _mediaBox;
            set => _mediaBox = value;
        }
        PdfRectangle _mediaBox = default!;

        public PdfRectangle CropBox
        {
            get => _cropBox;
            set => _cropBox = value;
        }
        PdfRectangle _cropBox = default!;

        public int Rotate
        {
            get => _rotate;
            set
            {
                if (value % 90 != 0)
                    throw new ArgumentException("The value must be a multiple of 90.", nameof(value));
                _rotate = value;
            }
        }
        int _rotate;
    }
}
