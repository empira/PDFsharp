// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Rendering information for charts.
    /// </summary>
    sealed class ChartRenderInfo : ShapeRenderInfo
    {
        internal ChartRenderInfo()
        { }

        public override FormatInfo FormatInfo
        {
            get => _formatInfo ??= new ChartFormatInfo();
            internal set => _formatInfo = (ChartFormatInfo)value;
        }

        ChartFormatInfo? _formatInfo;
    }
}
