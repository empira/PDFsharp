// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Indicates that this field can be accessed via SetValue and GetValue.
    /// </summary>
    [AttributeUsage(/*AttributeTargets.Field |*/ AttributeTargets.Property)]
    class DVAttribute : Attribute
    {
        ///// <summary>
        ///// Initializes a new instance of the DVAttribute class.
        ///// </summary>
        //public DVAttribute()
        //{ }

        ///// <summary>
        ///// Gets or sets the type of the reflected value. Must be specified by NEnum.
        ///// </summary>
        //public Type? Type { get; set; }

        /// <summary>
        /// Determines whether the property is reference-only and is excluded from recursive operations.
        /// </summary>
        public bool RefOnly { get; set; }

        /// <summary>
        /// Determines whether the property is read-only and cannot be set via SetValue.
        /// </summary>
        public bool ReadOnly { get; set; }
    }
}
