// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel;
using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Abstract base class for all classes that store rendering information.
    /// </summary>
    public abstract class RenderInfo
    {
        /// <summary>
        /// Gets the format information in a specific derived type. For a table, for example, this will be a TableFormatInfo with information about the first and last row showing on a page.
        /// </summary>
        public abstract FormatInfo FormatInfo { get; internal set; }

        /// <summary>
        /// Gets the layout information.
        /// </summary>
        public LayoutInfo LayoutInfo { get; } = new LayoutInfo();

        /// <summary>
        /// Gets the document object to which the layout information applies. Use the Tag property of DocumentObject to identify an object.
        /// </summary>
        public abstract DocumentObject DocumentObject { get; internal set; }

        internal virtual void RemoveEnding()
        {
            System.Diagnostics.Debug.Assert(false, "Unexpected call of RemoveEnding");
        }

        internal static XUnit GetTotalHeight(RenderInfo[]? renderInfos)
        {
            if (renderInfos == null || renderInfos.Length == 0)
                return 0;

            int lastIdx = renderInfos.Length - 1;
            var firstRenderInfo = renderInfos[0];
            var lastRenderInfo = renderInfos[lastIdx];
            var firstLayoutInfo = firstRenderInfo.LayoutInfo;
            var lastLayoutInfo = lastRenderInfo.LayoutInfo;
            XUnit top = firstLayoutInfo.ContentArea.Y - firstLayoutInfo.MarginTop;
            XUnit bottom = lastLayoutInfo.ContentArea.Y + lastLayoutInfo.ContentArea.Height;
            bottom += lastLayoutInfo.MarginBottom;
            return bottom - top;
        }
    }
}
