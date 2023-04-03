// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Summary description for TextFrameRenderInfo.
    /// </summary>
    public sealed class TextFrameRenderInfo : ShapeRenderInfo
    {
        /// <summary>
        /// Gets the format information in a specific derived type. For a table, for example, this will be a TableFormatInfo with information about the first and last row showing on a page.
        /// </summary>
        public override FormatInfo FormatInfo
        {
            get => _formatInfo ??= new TextFrameFormatInfo();
            internal set => _formatInfo = (TextFrameFormatInfo)value;
        }
        TextFrameFormatInfo _formatInfo = null!;
    }
}
