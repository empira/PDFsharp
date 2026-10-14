// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;

// v7.0.0 TODO review

namespace PdfSharp.Pdf.Forms
{
    /// <summary>
    /// TODO: Needed?
    /// Represents an Acro field that has no type entry (/FT) nor has a widget annotation part.
    /// </summary>
    public class PdfFormFieldNode : PdfFormField
    {
        // Reference 2.0: 12.7.4  Field dictionaries / Page 530

        internal PdfFormFieldNode(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormFieldNode" /> class.
        /// </summary>
        protected PdfFormFieldNode(PdfDictionary dict)
            : base(dict)
        { }
        
        /// <summary>
        /// Gets the actual type of the field with inheritance considered.
        /// </summary>
        public override Type FieldType
        {
            get
            {
                // PdfFormFieldNode doesn’t define the field type, so we ask the parent.
                var parent = Parent;
                if (parent == null)
                    throw new Exception("PdfFormFieldNode must have a parent, which defines the type of the field, or it should not be asked, as a child of the field defines the type.");
                return parent.FieldType;
            }
        }
    }
}
