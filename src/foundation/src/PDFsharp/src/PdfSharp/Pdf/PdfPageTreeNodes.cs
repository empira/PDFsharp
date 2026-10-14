// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO review

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents the children of a PdfPageTreeNode.
    /// </summary>
    //[DebuggerDisplay("(PageCount={" + nameof(Count) + "})")]
    public sealed class PdfPageTreeNodes : PdfArray
    {
        // Reference 2.0: 7.7.3.2  Page tree nodes / Page 102

        internal PdfPageTreeNodes()
        { }

        internal PdfPageTreeNodes(IEnumerable<PdfPage> pages)
        {
            foreach (var page in pages)
            {
                Elements.Add(page);
            }
        }

        internal PdfPageTreeNodes(PdfArray array)
            : base(array)
        { }
    }
}
