// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.ComponentModel;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a value and its unit of measure. The structure converts implicitly from and to
    /// double with a value measured in point.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public struct XUnit : IFormattable
    {
        internal const double PointFactor = 1;
        internal const double InchFactor = 72;
        internal const double MillimeterFactor = 72 / 25.4;
        internal const double CentimeterFactor = 72 / 2.54;
        internal const double PresentationFactor = 72 / 96d;

        internal const double PointFactorWpf = 96 / 72d;
        internal const double InchFactorWpf = 96;
        internal const double MillimeterFactorWpf = 96 / 25.4;
        internal const double CentimeterFactorWpf = 96 / 2.54;
        internal const double PresentationFactorWpf = 1;

        /// <summary>
        /// Initializes a new instance of the XUnit class with type set to point.
        /// </summary>
        public XUnit(double point)
        {
            PointValue = Value = point;
            Type = XGraphicsUnit.Point;
        }

        XUnit(double inch, double point)
        {
            PointValue = point;
            Value = inch;
            Type = XGraphicsUnit.Inch;
        }

        /// <summary>
        /// Initializes a new instance of the XUnit class.
        /// </summary>
        public XUnit(double value, XGraphicsUnit type)
        {
#if true
            if ((int)type is < (int)XGraphicsUnit.Point or > (int)XGraphicsUnit.Presentation)
#else
            // According to the profiler this code makes PDFsharp remarkably slow.
            if (!Enum.IsDefined(typeof(XGraphicsUnit), type))
#endif
                throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(XGraphicsUnit));

            Value = value;
            Type = type;
            PointValue = Type switch
            {
                XGraphicsUnit.Point => Value,
                XGraphicsUnit.Inch => Value * 72,
                XGraphicsUnit.Millimeter => Value * (72 / 25.4),
                XGraphicsUnit.Centimeter => Value * (72 / 2.54),
                XGraphicsUnit.Presentation => Value * (72d / 96),
                _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(XGraphicsUnit))
            };
        }

        /// <summary>
        /// Gets the raw value of the object without any conversion.
        /// To determine the XGraphicsUnit use property <code>Type</code>.
        /// To get the value in point use the implicit conversion to double.
        /// </summary>
        public double Value { get; private set; }

        /// <summary>
        /// Gets the current value in Points.
        /// Storing both Value and PointValue makes rendering more efficient.
        /// </summary>
        // Disadvantage: Conversion formulas are at four places.
        // ReSharper disable once ConvertToAutoProperty
        double PointValue
        {
            readonly get => _pointValue;
            set => _pointValue = value;
        }
        double _pointValue;

        /// <summary>
        /// Gets the unit of measure.
        /// </summary>
        public XGraphicsUnit Type { get; private set; }

        /// <summary>
        /// Gets or sets the value in point.
        /// </summary>
        public double Point
        {
            get => _pointValue;
            set
            {
                PointValue = Value = value;
                Type = XGraphicsUnit.Point;
            }
        }

        /// <summary>
        /// Gets or sets the value in inch.
        /// </summary>
        public double Inch
        {
            get
            {
                return Type switch
                {
                    XGraphicsUnit.Point => Value / 72,
                    XGraphicsUnit.Inch => Value,
                    XGraphicsUnit.Millimeter => Value / 25.4,
                    XGraphicsUnit.Centimeter => Value / 2.54,
                    XGraphicsUnit.Presentation => Value / 96,
                    _ => throw new InvalidCastException()
                };
            }
            set
            {
                Value = value;
                PointValue = value * 72;
                Type = XGraphicsUnit.Inch;
            }
        }

        /// <summary>
        /// Gets or sets the value in millimeter.
        /// </summary>
        public double Millimeter
        {
            get
            {
                return Type switch
                {
                    XGraphicsUnit.Point => Value * (25.4 / 72),
                    XGraphicsUnit.Inch => Value * 25.4,
                    XGraphicsUnit.Millimeter => Value,
                    XGraphicsUnit.Centimeter => Value * 10,
                    XGraphicsUnit.Presentation => Value * (25.4 / 96),
                    _ => throw new InvalidCastException()
                };
            }
            set
            {
                Value = value;
                PointValue = value * (72 / 25.4);
                Type = XGraphicsUnit.Millimeter;
            }
        }

        /// <summary>
        /// Gets or sets the value in centimeter.
        /// </summary>
        public double Centimeter
        {
            get
            {
                return Type switch
                {
                    XGraphicsUnit.Point => Value * (2.54 / 72),
                    XGraphicsUnit.Inch => Value * 2.54,
                    XGraphicsUnit.Millimeter => Value / 10,
                    XGraphicsUnit.Centimeter => Value,
                    XGraphicsUnit.Presentation => Value * (2.54 / 96),
                    _ => throw new InvalidCastException()
                };
            }
            set
            {
                Value = value;
                PointValue = value * (72 / 2.54);
                Type = XGraphicsUnit.Centimeter;
            }
        }

        /// <summary>
        /// Gets or sets the value in presentation units (1/96 inch).
        /// </summary>
        public double Presentation
        {
            get
            {
                return Type switch
                {
                    XGraphicsUnit.Point => Value * (96d / 72),
                    XGraphicsUnit.Inch => Value * 96,
                    XGraphicsUnit.Millimeter => Value * (96 / 25.4),
                    XGraphicsUnit.Centimeter => Value * (96 / 2.54),
                    XGraphicsUnit.Presentation => Value,
                    _ => throw new InvalidCastException()
                };
            }
            set
            {
                Value = value;
                PointValue = value * (72d / 96);
                Type = XGraphicsUnit.Presentation;
            }
        }

        /// <summary>
        /// Returns the object as string using the format information.
        /// The unit of measure is appended to the end of the string.
        /// </summary>
        public string ToString(IFormatProvider? formatProvider)
        {
            string value = Value.ToString(formatProvider) + GetSuffix();
            return value;
        }

        /// <summary>
        /// Returns the object as string using the specified format and format information.
        /// The unit of measure is appended to the end of the string.
        /// </summary>
        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        {
            string value = Value.ToString(format, formatProvider) + GetSuffix();
            return value;
        }

        /// <summary>
        /// Returns the object as string. The unit of measure is appended to the end of the string.
        /// </summary>
        public override string ToString()
        {
            string value = Value.ToString(CultureInfo.InvariantCulture) + GetSuffix();
            return value;
        }

        /// <summary>
        /// Returns the unit of measure of the object as a string like 'pt', 'cm', or 'in'.
        /// </summary>
        string GetSuffix()
        {
            return Type switch
            {
                XGraphicsUnit.Point => "pt",
                XGraphicsUnit.Inch => "in",
                XGraphicsUnit.Millimeter => "mm",
                XGraphicsUnit.Centimeter => "cm",
                XGraphicsUnit.Presentation => "pu",
                _ => throw new InvalidCastException()
            };
        }

        /// <summary>
        /// Returns an XUnit object. Sets type to point.
        /// </summary>
        public static XUnit FromPoint(double value)
        {
            // Create a Point XUnit without type check.
            XUnit unit = new(value/*, XGraphicsUnit.Point*/);
            return unit;
        }

        /// <summary>
        /// Returns an XUnit object. Sets type to inch.
        /// </summary>
        public static XUnit FromInch(double value)
        {
            // Create an Inch XUnit without type check.
            XUnit unit = new(value, value * 72);
            return unit;
        }

        /// <summary>
        /// Returns an XUnit object. Sets type to millimeters.
        /// </summary>
        public static XUnit FromMillimeter(double value)
        {
            XUnit unit = new(value, XGraphicsUnit.Millimeter);
            return unit;
        }

        /// <summary>
        /// Returns an XUnit object. Sets type to centimeters.
        /// </summary>
        public static XUnit FromCentimeter(double value)
        {
            XUnit unit = new(value, XGraphicsUnit.Centimeter);
            return unit;
        }

        /// <summary>
        /// Returns an XUnit object. Sets type to Presentation.
        /// </summary>
        public static XUnit FromPresentation(double value)
        {
            XUnit unit = new(value, XGraphicsUnit.Presentation);
            return unit;
        }

        /// <summary>
        /// Converts a string to an XUnit object.
        /// If the string contains a suffix like 'cm' or 'in' the object will be converted
        /// to the appropriate type, otherwise point is assumed.
        /// </summary>
        public static implicit operator XUnit(string value)
        {
            XUnit unit = default;
            value = value.Trim();

            // HACK for Germans...
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

            try
            {
                unit.Value = Double.Parse(value.Substring(0, valLen).Trim(), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                unit.Value = 1;
                string message = Invariant($"String '{value}' is not a valid value for structure 'XUnit'.");
                throw new ArgumentException(message, ex);
            }

            string typeStr = string.Join(string.Empty, value.Skip(valLen)).Trim().ToLower();
            unit.Type = XGraphicsUnit.Point;
            unit.Type = typeStr switch
            {
                "cm" => XGraphicsUnit.Centimeter,
                "in" => XGraphicsUnit.Inch,
                "mm" => XGraphicsUnit.Millimeter,
                "" or "pt" => XGraphicsUnit.Point,
                // presentation units
                "pu" => XGraphicsUnit.Presentation,
                _ => throw new ArgumentException("Unknown unit type: '" + typeStr + "'")
            };

            unit.PointValue = unit.Type switch
            {
                XGraphicsUnit.Point => unit.Value,
                XGraphicsUnit.Inch => unit.Value * 72,
                XGraphicsUnit.Millimeter => unit.Value * (72 / 25.4),
                XGraphicsUnit.Centimeter => unit.Value * (72 / 2.54),
                XGraphicsUnit.Presentation => unit.Value * (72d / 96),
                _ => throw new InvalidCastException()
            };
            return unit;
        }

        /// <summary>
        /// Converts an int to an XUnit object with type set to point.
        /// </summary>
        public static implicit operator XUnit(int value)
        {
            XUnit unit = new(value/*, XGraphicsUnit.Point*/);
            return unit;
        }

        /// <summary>
        /// Converts a double to an XUnit object with type set to point.
        /// </summary>
        public static implicit operator XUnit(double value)
        {
            XUnit unit = new(value/*, XGraphicsUnit.Point*/);
            return unit;
        }

        /// <summary>
        /// Returns a double value as point.
        /// </summary>
        public static implicit operator double(XUnit value)
        {
            return value.PointValue;
        }

        /// <summary>
        /// Memberwise comparison. To compare by value, 
        /// use code like Math.Abs(a.Pt - b.Pt) &lt; 1e-5.
        /// </summary>
        public static bool operator ==(XUnit value1, XUnit value2)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return value1.Type == value2.Type && value1.Value == value2.Value;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Memberwise comparison. To compare by value, 
        /// use code like Math.Abs(a.Pt - b.Pt) &lt; 1e-5.
        /// </summary>
        public static bool operator !=(XUnit value1, XUnit value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Calls base class Equals.
        /// </summary>
        public override bool Equals(Object? obj)
        {
            if (obj is XUnit unit)
                return this == unit;
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return Value.GetHashCode() ^ Type.GetHashCode();
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        /// <summary>
        /// This member is intended to be used by XmlDomainObjectReader only.
        /// </summary>
        public static XUnit Parse(string value)
        {
            XUnit unit = value;
            return unit;
        }

        /// <summary>
        /// Converts an existing object from one unit into another unit type.
        /// </summary>
        public void ConvertType(XGraphicsUnit type)
        {
            if (Type == type)
                return;

            switch (type)
            {
                case XGraphicsUnit.Point:
                    Value = PointValue;
                    Type = XGraphicsUnit.Point;
                    break;

                case XGraphicsUnit.Inch:
                    Value = Inch;
                    PointValue = Value * 72;
                    Type = XGraphicsUnit.Inch;
                    break;

                case XGraphicsUnit.Centimeter:
                    Value = Centimeter;
                    PointValue = Value * (72 / 2.54);
                    Type = XGraphicsUnit.Centimeter;
                    break;

                case XGraphicsUnit.Millimeter:
                    Value = Millimeter;
                    PointValue = Value * (72 / 25.4);
                    Type = XGraphicsUnit.Millimeter;
                    break;

                case XGraphicsUnit.Presentation:
                    Value = Presentation;
                    PointValue = Value * (72d / 96);
                    Type = XGraphicsUnit.Presentation;
                    break;

                default:
                    throw new ArgumentException($"Unknown unit type: '{type}'.");
            }
        }

        /// <summary>
        /// Represents a unit with all values zero.
        /// </summary>
        public static readonly XUnit Zero = new();

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        /// <value>The debugger display.</value>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
        {
            get
            {
                const string format = Config.SignificantFigures10;
                return String.Format(CultureInfo.InvariantCulture, "unit=({0:" + format + "} {1})", Value, GetSuffix());
            }
        }
    }
}
