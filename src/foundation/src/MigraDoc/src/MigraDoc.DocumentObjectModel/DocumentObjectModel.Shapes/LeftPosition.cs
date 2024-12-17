// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Represents the left position in a shape.
    /// </summary>
    public struct LeftPosition : INullableValue
    {
        /// <summary>
        /// Initializes a new instance of the LeftPosition class from Unit.
        /// </summary>
        LeftPosition(Unit value)
        {
            ShapePosition = ShapePosition.Undefined;  // BUG_OLD
            Position = value;
        }

        /// <summary>
        /// Initializes a new instance of the LeftPosition class from ShapePosition.
        /// </summary>
        LeftPosition(ShapePosition value)
        {
            if (!(value == ShapePosition.Undefined || IsValid(value)))
                throw new ArgumentException(MdDomMsgs.InvalidEnumForLeftPosition.Message);

            ShapePosition = value;
            Position = Unit.Empty;
        }

        /// <summary>
        /// Sets shape position enum and resets position.
        /// </summary>
        void SetFromEnum(ShapePosition shapePosition)
        {
            if (!IsValid(shapePosition))
                throw new ArgumentException(MdDomMsgs.InvalidEnumForLeftPosition.Message);

            ShapePosition = shapePosition;
            Position = Unit.Empty;
        }

        /// <summary>
        /// Sets the Position from a Unit.
        /// </summary>
        void SetFromUnit(Unit unit)
        {
            ShapePosition = ShapePosition.Undefined;
            Position = unit;
        }

        /// <summary>
        /// Sets the Position from an object.
        /// </summary>
        void INullableValue.SetValue(object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is ShapePosition position)
                SetFromEnum(position);
            else if (value is string s && Enum.IsDefined(typeof(ShapePosition), s))
                SetFromEnum((ShapePosition)Enum.Parse(typeof(ShapePosition), s, true));
            else
                SetFromUnit(value.ToString());
        }

        /// <summary>
        /// Gets the value of the position.
        /// </summary>
        object INullableValue.GetValue()
        {
            if (ShapePosition == ShapePosition.Undefined)
                return Position;

            return ShapePosition;
        }

        /// <summary>
        /// Resets this instance, i.e. IsNull() will return true afterwards.
        /// </summary>
        void INullableValue.SetNull()
            => this = new LeftPosition();

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        bool INullableValue.IsNull => _unit == null;

        /// <summary>
        /// Gets the value of the position in unit.
        /// </summary>
        public Unit Position
        {
            get => _unit ?? new Unit();
            internal set => _unit = value;
        }
        Unit? _unit = null;

        /// <summary>
        /// Gets the value of the position.
        /// </summary>
        public ShapePosition ShapePosition { get; internal set; }

        /// <summary>
        /// Indicates the given shapePosition is valid for LeftPosition.
        /// </summary>
        static bool IsValid(ShapePosition shapePosition)
            => shapePosition is ShapePosition.Left or ShapePosition.Center or ShapePosition.Right or ShapePosition.Inside or ShapePosition.Outside;

        /// <summary>
        /// Converts a ShapePosition to a LeftPosition.
        /// </summary>
        public static implicit operator LeftPosition(ShapePosition value)
            => new LeftPosition(value);

        /// <summary>
        /// Converts a Unit to a LeftPosition.
        /// </summary>
        public static implicit operator LeftPosition(Unit value)
            => new LeftPosition(value);

        /// <summary>
        /// Converts a string to a LeftPosition.
        /// The string is interpreted as a Unit.
        /// </summary>
        public static implicit operator LeftPosition(string value)
            => new LeftPosition(value);

        /// <summary>
        /// Converts a double to a LeftPosition.
        /// The double is interpreted as a Unit in Point.
        /// </summary>
        public static implicit operator LeftPosition(double value)
            => new LeftPosition(value);

        /// <summary>
        /// Converts an integer to a LeftPosition. 
        /// The integer is interpreted as a Unit in Point.
        /// </summary>
        public static implicit operator LeftPosition(int value)
            => new LeftPosition(value);

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        public static LeftPosition Parse(string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            value = value.Trim();
            char ch = value[0];
            if (ch is '+' or '-' || Char.IsNumber(ch))
                return Unit.Parse(value);
            return (ShapePosition)Enum.Parse(typeof(ShapePosition), value, true);
        }

        /// <summary>
        /// Converts LeftPosition into DDL.
        /// </summary>  
        internal void Serialize(Serializer serializer)
        {
            if (ShapePosition == ShapePosition.Undefined)
                serializer.WriteSimpleAttribute("Left", Position);
            else
                serializer.WriteSimpleAttribute("Left", ShapePosition);
        }

        /// <summary>
        /// Returns the uninitialized LeftPosition object.
        /// </summary>
        internal static readonly LeftPosition NullValue = new();
    }
}
