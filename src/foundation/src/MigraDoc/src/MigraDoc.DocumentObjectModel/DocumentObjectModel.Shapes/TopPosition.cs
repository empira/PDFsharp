// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Represents the top position in a shape.
    /// </summary>
    public struct TopPosition : INullableValue
    {
        /// <summary>
        /// Initializes a new instance of TopPosition from Unit.
        /// </summary>
        TopPosition(Unit value)
        {
            ShapePosition = ShapePosition.Undefined;  // BUG_OLD? Why not Top?
            Position = value;
        }

        /// <summary>
        /// Initializes a new instance of TopPosition from ShapePosition.
        /// </summary>
        TopPosition(ShapePosition value)
        {
            if (!(IsValid(value) || value == ShapePosition.Undefined))
                throw new ArgumentException(MdDomMsgs.InvalidEnumForTopPosition.Message);

            ShapePosition = value;
            Position = Unit.Empty;
        }

        /// <summary>
        /// Sets shape position enum and resets position.
        /// </summary>
        void SetFromEnum(ShapePosition shapePosition)
        {
            if (!IsValid(shapePosition))
                throw new ArgumentException(MdDomMsgs.InvalidEnumForTopPosition.Message);

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
            else if (value is string s && Enum.IsDefined(typeof(ShapePosition), value))
                SetFromEnum((ShapePosition)Enum.Parse(typeof(ShapePosition), s, true));
            else
                SetFromUnit(value.ToString());
        }

        /// <summary>
        /// Gets the Position as Unit or ShapePosition.
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
            => this = new TopPosition();

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        bool INullableValue.IsNull => _unit is null;

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
        /// Indicates the given shapePosition is valid for TopPosition.
        /// </summary>
        static bool IsValid(ShapePosition shapePosition)
            => shapePosition is ShapePosition.Bottom or ShapePosition.Top or ShapePosition.Center;

        /// <summary>
        /// Converts a ShapePosition to a TopPosition.
        /// </summary>
        public static implicit operator TopPosition(ShapePosition value)
            => new TopPosition(value);

        /// <summary>
        /// Converts a Unit to a TopPosition.
        /// </summary>
        public static implicit operator TopPosition(Unit val)
            => new TopPosition(val);

        /// <summary>
        /// Converts a string to a TopPosition.
        /// The string is interpreted as a Unit.
        /// </summary>
        public static implicit operator TopPosition(string value)
            => new TopPosition(value);

        /// <summary>
        /// Converts a double to a TopPosition.
        /// The double is interpreted as a Unit in Point.
        /// </summary>
        public static implicit operator TopPosition(double value)
            => new TopPosition(value);

        /// <summary>
        /// Converts an integer to a TopPosition. 
        /// The integer is interpreted as a Unit in Point.
        /// </summary>
        public static implicit operator TopPosition(int value)
            => new TopPosition(value);

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        public static TopPosition Parse(string value)
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
        /// Converts TopPosition into DDL.
        /// </summary>  
        internal void Serialize(Serializer serializer)
        {
            if (ShapePosition == ShapePosition.Undefined)
                serializer.WriteSimpleAttribute("Top", Position);
            else
                serializer.WriteSimpleAttribute("Top", ShapePosition);
        }

        /// <summary>
        /// Represents the uninitialized TopPosition object.
        /// </summary>
        internal static readonly TopPosition NullValue = new();
    }
}
