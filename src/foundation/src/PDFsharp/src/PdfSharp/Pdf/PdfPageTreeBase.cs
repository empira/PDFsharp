// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

// v7.0.0 Ready

namespace PdfSharp.Pdf  
{
    /// <summary>
    /// Base class of all page tree entries.
    /// Allows PdfPageTreeNode as well as PdfPage objects to be elements in
    /// a PdfTreeNodes array.
    /// </summary>
    public abstract class PdfPageTreeBase : PdfDictionary
    {
        protected PdfPageTreeBase()
        { }

        internal PdfPageTreeBase(PdfDocument document)
            : base(document, true)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfPageTreeBase(PdfDictionary dictionary)
            : base(dictionary)
        { }
    }
}