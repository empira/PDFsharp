// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Represents a class that provides a series of Areas to render into.
    /// </summary>
    interface IAreaProvider
    {
        /// <summary>
        /// Gets the next area to render into.
        /// </summary>
        Area? GetNextArea();

        /// <summary>
        /// Probes the next area to render into like GetNextArea, but doesn't change the provider state. 
        /// </summary>
        /// <returns>The area for the next rendering act.</returns>
        Area? ProbeNextArea();

        FieldInfos AreaFieldInfos { get; }

        /// <summary>
        /// Determines whether the element requires an area break before.
        /// </summary>
        bool IsAreaBreakBefore(LayoutInfo layoutInfo);

        /// <summary>
        /// Positions the element vertically relatively to the current area.
        /// </summary>
        /// <param name="layoutInfo">The layout info of the element.</param>
        /// <returns>True, if the element was moved by the function.</returns>
        bool PositionVertically(LayoutInfo layoutInfo);

        /// <summary>
        /// Positions the element horizontally relatively to the current area.
        /// </summary>
        /// <param name="layoutInfo">The layout info of the element.</param>
        /// <returns>True, if the element was moved by the function.</returns>
        bool PositionHorizontally(LayoutInfo layoutInfo);

        /// <summary>
        /// Stores the RenderInfos of elements on the current area.
        /// </summary>
        /// <param name="renderInfos"></param>
        void StoreRenderInfos(List<RenderInfo> renderInfos);
    }
}
