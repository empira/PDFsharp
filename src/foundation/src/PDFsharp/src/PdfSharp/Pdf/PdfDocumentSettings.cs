// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Holds PDF specific information of the document.
    /// </summary>
    public sealed class PdfDocumentSettings
    {
        internal PdfDocumentSettings(PdfDocument document)
        { }

        /// <summary>
        /// Gets or sets the default trim margins.
        /// </summary>
        public TrimMargins TrimMargins
        {
            get
            {
                if (_trimMargins == null)
                    _trimMargins = new TrimMargins();
                return _trimMargins;
            }
            set
            {
                if (_trimMargins == null)
                    _trimMargins = new TrimMargins();
                if (value != null)
                {
                    _trimMargins.Left = value.Left;
                    _trimMargins.Right = value.Right;
                    _trimMargins.Top = value.Top;
                    _trimMargins.Bottom = value.Bottom;
                }
                else
                    _trimMargins.All = 0;
            }
        }

        TrimMargins _trimMargins = new TrimMargins();
    }
}