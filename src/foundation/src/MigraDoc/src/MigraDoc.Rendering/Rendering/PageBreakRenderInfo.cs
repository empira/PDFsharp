// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Rendering information for page breaks.
    /// </summary>
    public sealed class PageBreakRenderInfo : RenderInfo
    {
        internal PageBreakRenderInfo()
        { }

        /// <summary>
        /// Gets the format information in a specific derived type. For a table, for example, this will be a TableFormatInfo with information about the first and last row showing on a page.
        /// </summary>
        public override FormatInfo FormatInfo
        {
            get => _pageBreakFormatInfo;
            internal set => _pageBreakFormatInfo = (PageBreakFormatInfo)value;
        }
        PageBreakFormatInfo _pageBreakFormatInfo = null!; // BUG NRT

        /// <summary>
        /// Gets the document object to which the layout information applies. Use the Tag property of DocumentObject to identify an object.
        /// </summary>
        public override DocumentObject DocumentObject
        {
            get => _pageBreak;
            internal set => _pageBreak = (PageBreak)value;
        }
        PageBreak _pageBreak = null!; // BUG NRT
    }
}
