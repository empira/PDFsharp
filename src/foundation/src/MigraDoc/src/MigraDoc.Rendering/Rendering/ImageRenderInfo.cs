// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents rendering information for images.
    /// </summary>
    public sealed class ImageRenderInfo : ShapeRenderInfo
    {
        /// <summary>
        /// Gets the format information in a specific derived type. For a table, for example, this will be a TableFormatInfo with information about the first and last row showing on a page.
        /// </summary>
        public override FormatInfo FormatInfo
        {
            get => _formatInfo ??= new ImageFormatInfo();
            internal set => _formatInfo = (ImageFormatInfo)value;
        }

        ImageFormatInfo? _formatInfo;
    }
}
