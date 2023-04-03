// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Abstract base class to serve as a layoutable unit.
    /// </summary>
    public class LayoutInfo
    {
        internal LayoutInfo()
        { }

        /// <summary>
        /// Gets or sets the height necessary to start the document object.
        /// </summary>
        internal XUnit StartingHeight { get; set; }

        /// <summary>
        /// Gets or sets the height necessary to end the document object.
        /// </summary>
        internal XUnit TrailingHeight { get; set; }

        /// <summary>
        /// Indicates whether the document object shall be kept on one page
        /// with its successor.
        /// </summary>
        internal bool KeepWithNext { get; set; }

        /// <summary>
        /// Indicates whether the document object shall be kept together on one page.
        /// </summary>
        internal bool KeepTogether { get; set; }

        /// <summary>
        /// The space that shall be kept free above the element's content.
        /// </summary>
        internal virtual XUnit MarginTop { get; set; }

        /// <summary>
        /// The space that shall be kept free right to the element's content.
        /// </summary>
        internal XUnit MarginRight { get; set; }

        /// <summary>
        /// The space that shall be kept free below the element's content.
        /// </summary>
        internal XUnit MarginBottom { get; set; }

        /// <summary>
        /// The space that shall be kept free left to the element's content.
        /// </summary>
        internal XUnit MarginLeft { get; set; }

        /// <summary>
        /// Gets or sets the Area needed by the content (including padding and borders for e.g. paragraphs).
        /// </summary>
        public Area ContentArea { get; set; } = null!;  //BUG

        /// <summary>
        /// Gets or sets the value indicating whether the element shall appear on a new page.
        /// </summary>
        internal bool PageBreakBefore { get; set; }

        /// <summary>
        /// Gets or sets the reference point for horizontal positioning.
        /// </summary>
        /// <remarks>Default value is AreaBoundary.</remarks>
        internal HorizontalReference HorizontalReference { get; set; }

        /// <summary>
        /// Gets or sets the reference point for vertical positioning.
        /// </summary>
        /// <remarks>Default value is PreviousElement.</remarks>
        internal VerticalReference VerticalReference { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the element.
        /// </summary>
        /// <remarks>Default value is Near.</remarks>
        internal ElementAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment of the element.
        /// </summary>
        /// <remarks>Default value is Near.</remarks>
        internal ElementAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the floating behavior of surrounding elements.
        /// </summary>
        /// <remarks>Default value is TopBottom.</remarks>
        internal Floating Floating { get; set; }

        /// <summary>
        /// Gets or sets the top position of the element.
        /// </summary>
        internal XUnit Top { get; set; }

        /// <summary>
        /// Gets or sets the left position of the element.
        /// </summary>
        internal XUnit Left { get; set; }

        /// <summary>
        /// Gets or sets the minimum width of the element.
        /// </summary>
        internal XUnit MinWidth { get; set; }
    }
}
