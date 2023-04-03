// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents rendering information for a paragraph.
    /// </summary>
    public sealed class ParagraphRenderInfo : RenderInfo
    {
        internal ParagraphRenderInfo()
        { }

        /// <summary>
        /// Gets the format information in a specific derived type. For a table, for example, this will be a TableFormatInfo with information about the first and last row showing on a page.
        /// </summary>
        public override FormatInfo FormatInfo
        {
            get => _formatInfo;
            internal set => _formatInfo = (ParagraphFormatInfo)value;
        }
        ParagraphFormatInfo _formatInfo = new();

        /// <summary>
        /// Gets the document object to which the layout information applies. Use the Tag property of DocumentObject to identify an object.
        /// </summary>
        public override DocumentObject DocumentObject
        {
            get => _paragraph;
            internal set => _paragraph = (Paragraph)value;
        }
        Paragraph _paragraph = null!;

        internal override void RemoveEnding()
        {
            ParagraphFormatInfo pfInfo = (ParagraphFormatInfo)FormatInfo;
            pfInfo.RemoveEnding();
            Area contentArea = LayoutInfo.ContentArea;
            contentArea.Height -= LayoutInfo.TrailingHeight;
        }
    }
}
