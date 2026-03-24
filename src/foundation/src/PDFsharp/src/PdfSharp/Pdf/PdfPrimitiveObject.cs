// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Common base class for PDF objects that are neither PdfArray nor PdfDictionary.
    /// ‘Primitive objects’ are PdfStringObject or PdfNumberObject, which is base class of PdfIntegerObject,
    /// PdfLongIntegerObject, and PdfRealObject.
    /// A none-compound object must not be used as a direct object.
    /// For technical use only, e.g. as marker class. There is no counterpart of
    /// this class in the PDF specification.
    /// </summary>
    public abstract class PdfPrimitiveObject : PdfObject
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        protected PdfPrimitiveObject()
        { }

        /// <summary>
        /// Initializes a new instance of this class
        /// without making it indirect.
        /// </summary>
        protected internal PdfPrimitiveObject(PdfDocument document, bool createIndirect)
            : base(document, createIndirect)
        {
            // The PDFsharp parser internally needs a way to create primitive objects initially as direct object.
            // Developers must not do that.
        }

#if PRESERVE_PARSED_VALUES
        /// <summary>
        /// Gets or sets the string that was originally read from the lexer.
        /// </summary>
        internal string? ParsedValue { get; set; }
#endif
    }
}
