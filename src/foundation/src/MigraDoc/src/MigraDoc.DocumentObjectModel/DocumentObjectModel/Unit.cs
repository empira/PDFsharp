// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A Unit consists of a numerical value and a UnitType like Centimeter, Millimeter, or Inch.
    /// Several conversions between different measures are supported.
    /// </summary>
    public struct Unit : IFormattable, INullableValue, IComparable<Unit?>, IComparable
    {
        /// <summary>
        /// Initializes a new instance of the Unit class with type set to point.
        /// </summary>
        public Unit(double point)
        {
            _value = (float)point;
            _type = UnitType.Point;
        }

        /// <summary>
        /// Initializes a new instance of the Unit class.
        /// Throws System.ArgumentException if <code>type</code> is invalid.
        /// </summary>
        public Unit(double value, UnitType type)
        {
            if (!Enum.IsDefined(typeof(UnitType), type))
                throw new /*InvalidEnum*/ArgumentException(MdDomMsgs.InvalidEnumValue(type).Message, nameof(type));

            _value = (float)value;
            _type = type;
            //_initialized = true;
        }

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        public bool IsEmpty => IsNull;

        /// <summary>
        /// Gets the value of the unit.
        /// </summary>
        object INullableValue.GetValue() => this;

        /// <summary>
        /// Sets the unit to the given value.
        /// </summary>
        void INullableValue.SetValue(object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));  // BUG? float = 0; uint=null

            if (value is Unit unit)
                this = unit;
            else
                this = value.ToString();
        }

        /// <summary>
        /// Resets this instance,
        /// i.e. IsNull() will return true afterwards.
        /// </summary>
        void INullableValue.SetNull()
        {
            _value = 0;
            _type = UnitType.Point;
        }

        // Explicit interface implementations cannot contain access specifiers, i.e. they are accessible by a
        // cast operator only, e.g. ((IDomValue)obj).IsNull.
        // Therefore, the second IsNull-Property is used as a handy shortcut.
        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        bool INullableValue.IsNull => IsNull;

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        internal bool IsNull => _type == null;

        /// <summary>
        /// Gets or sets the raw value of the object without any conversion.
        /// To determine the UnitType use property <code>Type</code>.
        /// </summary>
        public double Value
        {
            get => IsNull ? 0 : _value;
            set => _value = (float)value;
        }

        /// <summary>
        /// Gets the UnitType of the object.
        /// </summary>
        public UnitType Type
        {
            get => _type ?? UnitType.Point;
            set => _type = value;
        }

        /// <summary>
        /// Gets or sets the value in point.
        /// </summary>
        public double Point
        {
            get
            {
                if (IsNull)
                    return 0;

                switch (_type)
                {
                    case UnitType.Centimeter:
                        return _value * 72 / 2.54;

                    case UnitType.Inch:
                        return _value * 72;

                    case UnitType.Millimeter:
                        return _value * 72 / 25.4;

                    case UnitType.Pica:
                        return _value * 12;

                    case UnitType.Point:
                        return _value;

                    default:
                        Debug.Assert(false, "Missing unit type.");
                        return 0;
                }
            }
            set
            {
                _value = (float)value;
                _type = UnitType.Point;
            }
        }

        /// <summary>
        /// Gets or sets the value in centimeter.
        /// </summary>
        public double Centimeter
        {
            get
            {
                if (IsNull)
                    return 0;

                switch (_type)
                {
                    case UnitType.Centimeter:
                        return _value;

                    case UnitType.Inch:
                        return _value * 2.54;

                    case UnitType.Millimeter:
                        return _value / 10;

                    case UnitType.Pica:
                        return _value * 12 * 2.54 / 72;

                    case UnitType.Point:
                        return _value * 2.54 / 72;

                    default:
                        Debug.Assert(false, "Missing unit type");
                        return 0;
                }
            }
            set
            {
                _value = (float)value;
                _type = UnitType.Centimeter;
            }
        }

        /// <summary>
        /// Gets or sets the value in inch.
        /// </summary>
        public double Inch
        {
            get
            {
                if (IsNull)
                    return 0;

                switch (_type)
                {
                    case UnitType.Centimeter:
                        return _value / 2.54;

                    case UnitType.Inch:
                        return _value;

                    case UnitType.Millimeter:
                        return _value / 25.4;

                    case UnitType.Pica:
                        return _value * 12 / 72;

                    case UnitType.Point:
                        return _value / 72;

                    default:
                        Debug.Assert(false, "Missing unit type.");
                        return 0;
                }
            }
            set
            {
                _value = (float)value;
                _type = UnitType.Inch;
            }
        }

        /// <summary>
        /// Gets or sets the value in millimeter.
        /// </summary>
        public double Millimeter
        {
            get
            {
                if (IsNull)
                    return 0;

                switch (_type)
                {
                    case UnitType.Centimeter:
                        return _value * 10;

                    case UnitType.Inch:
                        return _value * 25.4;

                    case UnitType.Millimeter:
                        return _value;

                    case UnitType.Pica:
                        return _value * 12 * 25.4 / 72;

                    case UnitType.Point:
                        return _value * 25.4 / 72;

                    default:
                        Debug.Assert(false, "Missing unit type.");
                        return 0;
                }
            }
            set
            {
                _value = (float)value;
                _type = UnitType.Millimeter;
            }
        }

        /// <summary>
        /// Gets or sets the value in pica.
        /// </summary>
        public double Pica
        {
            get
            {
                if (IsNull)
                    return 0;

                switch (_type)
                {
                    case UnitType.Centimeter:
                        return _value * 72 / 2.54 / 12;

                    case UnitType.Inch:
                        return _value * 72 / 12;

                    case UnitType.Millimeter:
                        return _value * 72 / 25.4 / 12;

                    case UnitType.Pica:
                        return _value;

                    case UnitType.Point:
                        return _value / 12;

                    default:
                        Debug.Assert(false, "Missing unit type.");
                        return 0;
                }
            }
            set
            {
                _value = (float)value;
                _type = UnitType.Pica;
            }
        }

        /// <summary>
        /// Returns the object as string using the format information.
        /// Measure will be added to the end of the string.
        /// </summary>
        public string ToString(IFormatProvider formatProvider)
        {
            if (IsNull)
                return 0.ToString(formatProvider);

            var value = _value.ToString(formatProvider) + GetSuffix();
            return value;
        }

        /// <summary>
        /// Returns the object as string using the format.
        /// Measure will be added to the end of the string.
        /// </summary>
        public string ToString(string format)
        {
            if (IsNull)
                return 0.ToString(format);

            var value = _value.ToString(format) + GetSuffix();
            return value;
        }

        /// <summary>
        /// Returns the object as string using the specified format and format information.
        /// Measure will be added to the end of the string.
        /// </summary>
        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        {
            if (IsNull)
                return 0.ToString(format, formatProvider);

            var value = _value.ToString(format, formatProvider) + GetSuffix();
            return value;
        }

        /// <summary>
        /// Returns the object as string. Measure will be added to the end of the string.
        /// </summary>
        public override string ToString()
        {
            if (IsNull)
                return 0.ToString(CultureInfo.InvariantCulture);

            var value = _value.ToString(CultureInfo.InvariantCulture) + GetSuffix();
            return value;
        }

        /// <summary>
        /// Returns the type of the object as a string like 'pc', 'cm', or 'in'. Empty string is equal to 'pt'.
        /// </summary>
        string GetSuffix()
        {
            switch (_type)
            {
                case UnitType.Centimeter:
                    return "cm";

                case UnitType.Inch:
                    return "in";

                case UnitType.Millimeter:
                    return "mm";

                case UnitType.Pica:
                    return "pc";

                case UnitType.Point:
                case null:
                    // Point is default, so leave this blank.
                    return "";

                default:
                    Debug.Assert(false, "Missing unit type.");
                    return "";
            }
        }

        /// <summary>
        /// Returns a Unit object. Sets type to centimeter.
        /// </summary>
        public static Unit FromCentimeter(double value)
        {
            var unit = Zero;
            unit._value = (float)value;
            unit._type = UnitType.Centimeter;
            return unit;
        }

        /// <summary>
        /// Returns a Unit object. Sets type to millimeter.
        /// </summary>
        public static Unit FromMillimeter(double value)
        {
            var unit = Zero;
            unit._value = (float)value;
            unit._type = UnitType.Millimeter;
            return unit;
        }

        /// <summary>
        /// Returns a Unit object. Sets type to point.
        /// </summary>
        public static Unit FromPoint(double value)
        {
            var unit = Zero;
            unit._value = (float)value;
            unit._type = UnitType.Point;
            return unit;
        }

        /// <summary>
        /// Returns a Unit object. Sets type to inch.
        /// </summary>
        public static Unit FromInch(double value)
        {
            var unit = Zero;
            unit._value = (float)value;
            unit._type = UnitType.Inch;
            return unit;
        }

        /// <summary>
        /// Returns a Unit object. Sets type to pica.
        /// </summary>
        public static Unit FromPica(double value)
        {
            var unit = Zero;
            unit._value = (float)value;
            unit._type = UnitType.Pica;
            return unit;
        }

        /// <summary>
        /// Converts a string to a Unit object.
        /// If the string contains a suffix like 'cm' or 'in' the object will be converted
        /// to the appropriate type, otherwise point is assumed.
        /// </summary>
        public static implicit operator Unit(string? value)
        {
            if (value is null)
                //NRT.ThrowOnNull("string parameter was null"); // BUG Throwing on null.
                return Zero;

            var unit = Zero;
            value = value.Trim();

            // For Germans...
            value = value.Replace(',', '.');

            int count = value.Length;
            int valLen = 0;
            for (; valLen < count;)
            {
                char ch = value[valLen];
                if (ch is '.' or '-' or '+' || Char.IsNumber(ch))
                    valLen++;
                else
                    break;
            }

            unit._value = 1;
            try
            {
                unit._value = float.Parse(value[..valLen].Trim(), CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(MdDomMsgs.InvalidUnitValue(value).Message, ex);
            }

            var typeStr = value[valLen..].Trim().ToLower();
            unit._type = UnitType.Point;
            switch (typeStr)
            {
                case "cm":
                    unit._type = UnitType.Centimeter;
                    break;

                case "in":
                    unit._type = UnitType.Inch;
                    break;

                case "mm":
                    unit._type = UnitType.Millimeter;
                    break;

                case "pc":
                    unit._type = UnitType.Pica;
                    break;

                case "":
                case "pt":
                    unit._type = UnitType.Point;
                    break;

                default:
                    throw new ArgumentException(MdDomMsgs.InvalidUnitType(typeStr).Message);
            }

            return unit;
        }

        /// <summary>
        /// Converts an int to a Unit object with type set to point.
        /// </summary>
        public static implicit operator Unit(int value)
        {
            var unit = Zero;
            unit._value = value;
            unit._type = UnitType.Point;
            return unit;
        }

        /// <summary>
        /// Converts a float to a Unit object with type set to point.
        /// </summary>
        public static implicit operator Unit(float value)
        {
            var unit = Zero;
            unit._value = value;
            unit._type = UnitType.Point;
            return unit;
        }

        /// <summary>
        /// Converts a double to a Unit object with type set to point.
        /// </summary>
        public static implicit operator Unit(double value)
        {
            var unit = Zero;
            unit._value = (float)value;
            unit._type = UnitType.Point;
            return unit;
        }

        /// <summary>
        /// Returns a double value as point.
        /// </summary>
        [Obsolete("Implicit conversion to double is obsolete, because it led to misunderstandings and unexpected behavior. Use e. g. the Point property instead.")]
        public static implicit operator double(Unit value)
            => value.Point;

        /// <summary>
        /// Returns a float value as point.
        /// </summary>
        [Obsolete("Implicit conversion to float is obsolete, because it led to misunderstandings and unexpected behavior. Use e. g. the Point property instead.")]
        public static implicit operator float(Unit value)
            => (float)value.Point;

        /// <summary>
        /// Memberwise comparison checking exact value and unit.
        /// To compare by value tolerating rounding errors, use IsSameValue() or code like Math.Abs(a.Point - b.Point) &lt; 1e-5.
        /// </summary>
        public static bool operator ==(Unit l, Unit r)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            // BUG _type may be null
            return l._type == r._type && l._value == r._value;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Memberwise comparison checking exact value and unit.
        /// To compare by value tolerating rounding errors, use IsSameValue() or code like Math.Abs(a.Point - b.Point) &lt; 1e-5.
        /// </summary>
        public static bool operator ==(Unit? l, Unit? r)
        {
            if (!l.HasValue && !r.HasValue)
                return true;

            if (!l.HasValue || !r.HasValue)
                return false;

            return l.Value == r.Value;
        }

        /// <summary>
        /// Memberwise comparison checking exact value and unit.
        /// To compare by value tolerating rounding errors, use code like Math.Abs(a.Point - b.Point) &lt; 1e-5.
        /// </summary>
        public static bool operator !=(Unit l, Unit r)
            => !(l == r);

        /// <summary>
        /// Memberwise comparison checking exact value and unit.
        /// To compare by value tolerating rounding errors, use code like Math.Abs(a.Point - b.Point) &lt; 1e-5.
        /// </summary>
        public static bool operator !=(Unit? l, Unit? r)
            => !(l == r);

        /// <summary>
        /// Compares two Unit values.
        /// </summary>
        public static bool operator >(Unit l, Unit r) => Compare(l, r) > 0;

        /// <summary>
        /// Compares two Unit? values.
        /// </summary>
        public static bool operator >(Unit? l, Unit? r) => Compare(l, r) > 0;

        /// <summary>
        /// Compares two Unit values.
        /// </summary>
        public static bool operator >=(Unit l, Unit r) => Compare(l, r) >= 0;

        /// <summary>
        /// Compares two Unit? values.
        /// </summary>
        public static bool operator >=(Unit? l, Unit? r) => Compare(l, r) >= 0;

        /// <summary>
        /// Compares two Unit values.
        /// </summary>
        public static bool operator <(Unit l, Unit r) => Compare(l, r) < 0;

        /// <summary>
        /// Compares two Unit? values.
        /// </summary>
        public static bool operator <(Unit? l, Unit? r) => Compare(l, r) < 0;

        /// <summary>
        /// Compares two Unit values.
        /// </summary>
        public static bool operator <=(Unit l, Unit r) => Compare(l, r) <= 0;

        /// <summary>
        /// Compares two Unit? values.
        /// </summary>
        public static bool operator <=(Unit? l, Unit? r) => Compare(l, r) <= 0;

        /// <summary>
        /// Returns the negative value of a Unit.
        /// </summary>
        public static Unit operator -(Unit value) => new(-value.Value, value.Type);

        /// <summary>
        /// Adds a Unit to a Unit.
        /// </summary>
        public static Unit operator +(Unit l, Unit r)
        {
            if (l.Type != r.Type)
                r.ConvertType(l.Type);

            return new(l.Value + r.Value, l.Type);
        }

        /// <summary>
        /// Adds a Unit? to a Unit?.
        /// </summary>
        public static Unit? operator +(Unit? l, Unit? r) => !l.HasValue || !r.HasValue ? null : l.Value + r.Value;

        /// <summary>
        /// Adds a string parsed as Unit to a Unit.
        /// </summary>
        public static Unit operator +(Unit l, string r)
        {
            var u = (Unit)r;
            return l + u;
        }

        /// <summary>
        /// Adds a string parsed as Unit to a Unit?.
        /// </summary>
        public static Unit? operator +(Unit? l, string r) => !l.HasValue ? null : l.Value + r;

        /// <summary>
        /// Subtracts a Unit from a Unit.
        /// </summary>
        public static Unit operator -(Unit l, Unit r)
        {
            if (l.Type != r.Type)
                r.ConvertType(l.Type);

            return new(l.Value - r.Value, l.Type);
        }

        /// <summary>
        /// Subtracts a Unit? from a Unit?.
        /// </summary>
        public static Unit? operator -(Unit? l, Unit? r) => !l.HasValue || !r.HasValue ? null : l.Value - r.Value;

        /// <summary>
        /// Subtracts a string parsed as Unit from a Unit.
        /// </summary>
        public static Unit operator -(Unit l, string r)
        {
            var u = (Unit)r;
            return l - u;
        }

        /// <summary>
        /// Subtracts a string parsed as Unit from a Unit?.
        /// </summary>
        public static Unit? operator -(Unit? l, string r) => !l.HasValue ? null : l.Value - r;

        /// <summary>
        /// Multiplies a Unit with a double.
        /// </summary>
        public static Unit operator *(Unit l, double r)
        {
            return new(l.Value * r, l.Type);
        }

        /// <summary>
        /// Multiplies a Unit? with a double.
        /// </summary>
        // ReSharper disable once MergeConditionalExpression
        public static Unit? operator *(Unit? l, double r) => !l.HasValue ? null : l.Value * r;

        /// <summary>
        /// Divides a Unit by a double.
        /// </summary>
        public static Unit operator /(Unit l, double r)
        {
            return new(l.Value / r, l.Type);
        }

        /// <summary>
        /// Divides a Unit? by a double.
        /// </summary>
        // ReSharper disable once MergeConditionalExpression
        public static Unit? operator /(Unit? l, double r) => !l.HasValue ? null : l.Value / r;

        static int Compare(Unit l, Unit r)
        {
            return l.Point.CompareTo(r.Point);
        }

        static int Compare(Unit? l, Unit? r)
        {
            if (!l.HasValue && !r.HasValue)
                return 0;

            if (!l.HasValue)
                return -1;

            if (!r.HasValue)
                return 1;

            return Compare(l.Value, r.Value);
        }

        /// <summary>
        /// Compares this Unit with another Unit value.
        /// </summary>
        public int CompareTo(Unit other) => Math.Sign(Point - other.Point);

        /// <summary>
        /// Compares this Unit with another Unit? value.
        /// </summary>
        public int CompareTo(Unit? other) => other.HasValue ? CompareTo(other.Value) : 1;

        /// <summary>
        /// Compares this Unit with another object.
        /// </summary>
        public int CompareTo(object? obj) => obj is Unit unit ? CompareTo(unit) : 1;

        /// <summary>
        /// Compares the actual values of this Unit and another Unit value tolerating rounding errors.
        /// </summary>
        public bool IsSameValue(Unit other)
        {
            return Math.Abs(Point - other.Point) <= 1e-5;
        }

        /// <summary>
        /// Compares the actual values of this Unit and another Unit? value tolerating rounding errors.
        /// </summary>
        public bool IsSameValue(Unit? other) => other.HasValue && IsSameValue(other.Value);

        /// <summary>
        /// Calls base class Equals.
        /// </summary>
        // ReSharper disable once RedundantOverriddenMember because when removing this function
        // code analysis complains its missing because operator == is overridden.
        public override bool Equals(Object? obj)
            => base.Equals(obj);

        /// <summary>
        /// Calls base class GetHashCode.
        /// </summary>
        // ReSharper disable once RedundantOverriddenMember because when removing this function
        // code analysis complains its missing because operator == is overridden.
        public override int GetHashCode()
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode because we need must override this function.
            => base.GetHashCode();

        /// <summary>
        /// This member is intended to be used by XmlDomainObjectReader only.
        /// StL: A fun fact is that this project does not even contain an XmlDomainObjectReader
        /// </summary>
        public static Unit Parse(string value)
        {
            Unit unit = value;
            return unit;
        }

        /// <summary>
        /// Converts an existing object from one unit into another unit type.
        /// </summary>
        public void ConvertType(UnitType type)
        {
            if (_type == type)
                return;

            switch (type)
            {
                case UnitType.Centimeter:
                    _value = (float)Centimeter;
                    _type = UnitType.Centimeter;
                    break;

                case UnitType.Inch:
                    _value = (float)Inch;
                    _type = UnitType.Inch;
                    break;

                case UnitType.Millimeter:
                    _value = (float)Millimeter;
                    _type = UnitType.Millimeter;
                    break;

                case UnitType.Pica:
                    _value = (float)Pica;
                    _type = UnitType.Pica;
                    break;

                case UnitType.Point:
                    _value = (float)Point;
                    _type = UnitType.Point;
                    break;

                default:
                    if (!Enum.IsDefined(typeof(UnitType), type))
                        throw new ArgumentException(MdDomMsgs.InvalidUnitType(type.ToString()).Message);

                    // Remember missing unit type.
                    Debug.Assert(false, "Missing unit type.");
                    break;
            }
        }

        /// <summary>
        /// Represents the uninitialized Unit object.
        /// </summary>
        public static readonly Unit Empty = new();

        /// <summary>
        /// Represents an initialized Unit object with value 0 and unit type point.
        /// </summary>
        public static readonly Unit Zero = new(0);

        internal static Unit? MakeNullIfEmpty(Unit? unit)
            => unit == Empty ? null : unit;

        float _value;
        UnitType? _type;
    }
}
