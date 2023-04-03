// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Specifies with type of chart will be drawn.
    /// </summary>
    public enum ChartType
    {
        /// <summary>
        /// A line chart.
        /// </summary>
        Line,

        /// <summary>
        /// A clustered 2d column chart.
        /// </summary>
        Column2D,

        /// <summary>
        /// A stacked 2d column chart.
        /// </summary>
        ColumnStacked2D,

        /// <summary>
        /// A 2d area chart.
        /// </summary>
        Area2D,

        /// <summary>
        /// A clustered 2d bar chart.
        /// </summary>
        Bar2D,

        /// <summary>
        /// A stacked 2d bar chart.
        /// </summary>
        BarStacked2D,

        /// <summary>
        /// A 2d pie chart.
        /// </summary>
        Pie2D,

        /// <summary>
        /// An exploded 2d pie chart.
        /// </summary>
        PieExploded2D,
    }
}
