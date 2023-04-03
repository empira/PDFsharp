// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Interface for simple nullable values.
    /// Originally used for  like NInt, NString etc.
    /// Currently only used for Color, Unit, TopPosition, and LeftPosition.
    /// </summary>
    interface INullableValue
    {
        object GetValue();

        void SetValue(object? value);

        void SetNull();

        bool IsNull { get; }
    }
}
