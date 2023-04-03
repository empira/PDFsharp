// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Determines the type of the data label.
    /// </summary>
    public enum DataLabelType
    {
        /// <summary>
        /// No DataLabel.
        /// </summary>
        None,

        /// <summary>
        /// Percentage of the data. For pie charts only.
        /// </summary>
        Percent,

        /// <summary>
        /// Value of the data.
        /// </summary>
        Value
    }
}
