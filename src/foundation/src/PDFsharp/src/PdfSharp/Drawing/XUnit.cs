// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a value and its unit of measure.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public struct XUnit : IFormattable, IComparable<XUnit>, IComparable
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
        /// To get the value in point use property <code>Point</code>.
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
            => Value.ToString(formatProvider) + GetSuffix();

        /// <summary>
        /// Returns the object as string using the specified format and format information.
        /// The unit of measure is appended to the end of the string.
        /// </summary>
        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
            => Value.ToString(format, formatProvider) + GetSuffix();

        /// <summary>
        /// Returns the object as string. The unit of measure is appended to the end of the string.
        /// </summary>
        public override string ToString()
            => Value.ToString(CultureInfo.InvariantCulture) + GetSuffix();

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
        public static XUnit FromPoint(double value) => new(value/*, XGraphicsUnit.Point*/);

        /// <summary>
        /// Returns an XUnit object. Sets type to inch.
        /// </summary>
        public static XUnit FromInch(double value) => new(value, value * 72);

        /// <summary>
        /// Returns an XUnit object. Sets type to millimeters.
        /// </summary>
        public static XUnit FromMillimeter(double value) => new(value, XGraphicsUnit.Millimeter);

        /// <summary>
        /// Returns an XUnit object. Sets type to centimeters.
        /// </summary>
        public static XUnit FromCentimeter(double value) => new(value, XGraphicsUnit.Centimeter);

        /// <summary>
        /// Returns an XUnit object. Sets type to Presentation.
        /// </summary>
        public static XUnit FromPresentation(double value) => new(value, XGraphicsUnit.Presentation);

        /// <summary>
        /// Converts a string to an XUnit object.
        /// If the string contains a suffix like 'cm' or 'in' the object will be converted
        /// to the appropriate type, otherwise point is assumed.
        /// </summary>
        public static implicit operator XUnit(string value)
        {
            XUnit unit = default;
            value = value.Trim();

            // No commas allowed anymore. ',' as decimal separator was a special hack for German numbers.
            if (value.Contains(','))
                throw new FormatException($"value '{value}' must not contain a comma as decimal or thousands separator.");

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

            string typeStr = value.Substring(valLen).Trim().ToLower();
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
        [Obsolete("In PDFsharp 6.1 implicit conversion from int is marked obsolete, because it led to misunderstandings and unexpected behavior. " +
                  "Provide the unit by XUnit.FromPoint() or use the new class XUnitPt instead.")]
        public static implicit operator XUnit(int value) => new(value/*, XGraphicsUnit.Point*/);

        /// <summary>
        /// Converts a double to an XUnit object with type set to point.
        /// </summary>
        [Obsolete("In PDFsharp 6.1 implicit conversion from double is marked obsolete, because it led to misunderstandings and unexpected behavior. " +
                  "Provide the unit by XUnit.FromPoint() or use the new class XUnitPt instead.")]
        public static implicit operator XUnit(double value) => new(value/*, XGraphicsUnit.Point*/);

        /// <summary>
        /// Converts an XUnit object to a double value as point.
        /// </summary>
        [Obsolete("In PDFsharp 6.1 implicit conversion to double is marked obsolete, because it led to misunderstandings and unexpected behavior. Use the XUnit.Point property instead.")]
        public static implicit operator double(XUnit value) => value.PointValue;

        /// <summary>
        /// Memberwise comparison checking the exact value and unit.
        /// To compare by value tolerating rounding errors, use IsSameValue() or code like Math.Abs(a.Pt - b.Pt) &lt; 1e-5.
        /// </summary>
        // ReSharper disable CompareOfFloatsByEqualityOperator
        public static bool operator ==(XUnit l, XUnit r) => l.Type == r.Type && l.Value == r.Value;
        // ReSharper restore CompareOfFloatsByEqualityOperator

        /// <summary>
        /// Memberwise comparison checking exact value and unit.
        /// To compare by value tolerating rounding errors, use IsSameValue() or code like Math.Abs(a.Pt - b.Pt) &lt; 1e-5.
        /// </summary>
        public static bool operator !=(XUnit l, XUnit r) => !(l == r);

        /// <summary>
        /// Compares two XUnit values.
        /// </summary>
        public static bool operator >(XUnit l, XUnit r) => l.CompareTo(r) > 0;

        /// <summary>
        /// Compares two XUnit values.
        /// </summary>
        public static bool operator >=(XUnit l, XUnit r) => l.CompareTo(r) >= 0;

        /// <summary>
        /// Compares two XUnit values.
        /// </summary>
        public static bool operator <(XUnit l, XUnit r) => l.CompareTo(r) < 0;

        /// <summary>
        /// Compares two XUnit values.
        /// </summary>
        public static bool operator <=(XUnit l, XUnit r) => l.CompareTo(r) <= 0;

        /// <summary>
        /// Returns the negative value of an XUnit.
        /// </summary>
        public static XUnit operator -(XUnit value) => new(-value.Value, value.Type);

        /// <summary>
        /// Adds an XUnit to an XUnit.
        /// </summary>
        public static XUnit operator +(XUnit l, XUnit r)
        {
            if (l.Type != r.Type)
                r.ConvertType(l.Type);

            return new(l.Value + r.Value, l.Type);
        }

        /// <summary>
        /// Adds a string parsed as XUnit to an XUnit.
        /// </summary>
        public static XUnit operator +(XUnit l, string r)
        {
            var u = (XUnit)r;
            return l + u;
        }

        /// <summary>
        /// Subtracts an XUnit from an XUnit.
        /// </summary>
        public static XUnit operator -(XUnit l, XUnit r)
        {
            if (l.Type != r.Type)
                r.ConvertType(l.Type);

            return new(l.Value - r.Value, l.Type);
        }

        /// <summary>
        /// Subtracts a string parsed as Unit from an XUnit.
        /// </summary>
        public static XUnit operator -(XUnit l, string r)
        {
            var u = (XUnit)r;
            return l - u;
        }

        /// <summary>
        /// Multiplies an XUnit with a double.
        /// </summary>
        public static XUnit operator *(XUnit l, double r)
        {
            return new(l.Value * r, l.Type);
        }

        /// <summary>
        /// Divides an XUnit by a double.
        /// </summary>
        public static XUnit operator /(XUnit l, double r)
        {
            return new(l.Value / r, l.Type);
        }

        /// <summary>
        /// Compares this XUnit with another XUnit value.
        /// </summary>
        public int CompareTo(XUnit other) => Math.Sign(PointValue - other.PointValue);

        /// <summary>
        /// Compares this XUnit with another object.
        /// </summary>
        public int CompareTo(object? obj) => obj is XUnit unit ? CompareTo(unit) : 1;

        /// <summary>
        /// Compares the actual values of this XUnit and another XUnit value tolerating rounding errors.
        /// </summary>
        public bool IsSameValue(XUnit other) => Math.Abs(Point - other.Point) <= 1e-5;

        /// <summary>
        /// Calls base class Equals.
        /// </summary>
        public override bool Equals(Object? obj) => obj is XUnit unit && this == unit;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        // ReSharper disable NonReadonlyFieldInGetHashCode
        public override int GetHashCode() => Value.GetHashCode() ^ Type.GetHashCode();
        // ReSharper restore NonReadonlyFieldInGetHashCode

        /// <summary>
        /// This member is intended to be used by XmlDomainObjectReader only.
        /// </summary>
        public static XUnit Parse(string value) => value;

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
        string DebuggerDisplay => Invariant($"{Value:0.######} {GetSuffix()}");
        // ReSharper restore UnusedMember.Local
    }
}
