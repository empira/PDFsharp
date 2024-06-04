// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Specifies the measure of a Unit object.
    /// </summary>
    public enum UnitType
    {
        /// <summary>
        /// Measure is in point. A point represents 1/72 of an inch.
        /// </summary>
        Point = 0,  // Default for new Unit() is Point

        /// <summary>
        /// Measure is in centimeter.
        /// </summary>
        Centimeter = 1,

        /// <summary>
        /// Measure is in inch.
        /// </summary>
        Inch = 2,

        /// <summary>
        /// Measure is in millimeter.
        /// </summary>
        Millimeter = 3,

        /// <summary>
        /// Measure is in pica. A pica represents 12 points, i.e. 6 pica are one inch.
        /// </summary>
        Pica = 4,
    }
}
