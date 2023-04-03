// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Base class for FontTable, ImageTable, FormXObjectTable etc.
    /// </summary>
    public class PdfResourceTable
    {
        /// <summary>
        /// Base class for document wide resource tables.
        /// </summary>
        public PdfResourceTable(PdfDocument owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            _owner = owner;
        }

        /// <summary>
        /// Gets the owning document of this resource table.
        /// </summary>
        protected PdfDocument Owner => _owner;

        readonly PdfDocument _owner;
    }
}
