// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace MigraDoc.DocumentObjectModel.Internals
{
    [Flags]
    enum VDFlags
    {
        None = 0,

        /// <summary>
        /// The property is an associated object that is not an aggregation of the objects it is contained it.
        /// RefOnly properties are e.g. not evaluated for IsNull.
        /// </summary>
        RefOnly = 0x0001,

        /// <summary>
        /// The property must only be read by GetValue even it has technically a setter.
        /// </summary>
        ReadOnly = 0x0002
    }
}
