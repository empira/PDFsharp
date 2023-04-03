// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Extension methods introduced for nullable reference type migration.
    /// </summary>
    public static class NrtExtensions
    {
        /// <summary>
        /// Determines whether a String null or empty.
        /// </summary>
        public static bool IsValueNullOrEmpty(this string? s)
            => String.IsNullOrEmpty(s);

        /// <summary>
        /// Determines whether a Color null or empty.
        /// </summary>
        public static bool IsValueNullOrEmpty(this Color? clr) 
            => clr is null || ((INullableValue)clr).IsNull;

        /// <summary>
        /// Determines whether a Unit is null or empty.
        /// </summary>
        public static bool IsValueNullOrEmpty(this Unit? unit) 
            => unit is null || ((INullableValue)unit).IsNull;

        /// <summary>
        /// Determines whether a TopPosition is null or empty.
        /// </summary>
        public static bool IsValueNullOrEmpty(this TopPosition? pos) 
            => pos is null || ((INullableValue)pos).IsNull;

        /// <summary>
        /// Determines whether a LeftPosition is null or empty.
        /// </summary>
        public static bool IsValueNullOrEmpty(this LeftPosition? pos) 
            => pos is null || ((INullableValue)pos).IsNull;

        /// <summary>
        /// Determines whether a TopPosition is null or empty.
        /// </summary>
        public static bool IsValueNullOrEmpty(this DocumentObject? doc) 
            => doc is null || doc.IsNull();
    }
}
