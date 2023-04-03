// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Charting;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Formatting information for a chart.
    /// </summary>
    sealed class ChartFormatInfo : ShapeFormatInfo
    {
        internal ChartFrame? ChartFrame;
        internal FormattedTextArea? FormattedHeader;
        internal FormattedTextArea? FormattedLeft;
        internal FormattedTextArea? FormattedTop;
        internal FormattedTextArea? FormattedBottom;
        internal FormattedTextArea? FormattedRight;
        internal FormattedTextArea? FormattedFooter;
    }
}
