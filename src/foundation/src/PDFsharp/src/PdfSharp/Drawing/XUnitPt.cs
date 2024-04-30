// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a value with a unit of measure in point (1/72 inch). The structure converts implicitly from and to
    /// double.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public struct XUnitPt : IFormattable, IComparable<XUnitPt>, IComparable
    {
        /// <summary>
        /// Initializes a new instance of the XUnitPt class.
        /// </summary>
        public XUnitPt(double value) => Value = value;

        /// <summary>
        /// Gets or sets the raw value of the object, which is always measured in point for XUnitPt.
        /// </summary>

        public double Value;

        /// <summary>
        /// Gets or sets the value in point.
        /// </summary>
        public double Point
        {
            get => Value;
            set => Value = value;
        }

        /// <summary>
        /// Gets or sets the value in inch.
        /// </summary>
        public double Inch
        {
            get => Value / 72;
            set => Value = value * 72;
        }

        /// <summary>
        /// Gets or sets the value in millimeter.
        /// </summary>
        public double Millimeter
        {
            get => Value * (25.4 / 72);
            set => Value = value * (72 / 25.4);
        }

        /// <summary>
        /// Gets or sets the value in centimeter.
        /// </summary>
        public double Centimeter
        {
            get => Value * (2.54 / 72);
            set => Value = value * (72 / 2.54);
        }

        /// <summary>
        /// Gets or sets the value in presentation units (1/96 inch).
        /// </summary>
        public double Presentation
        {
            get => Value * (96d / 72);
            set => Value = value * (72d / 96);
        }

        /// <summary>
        /// Returns the object as string using the format information.
        /// The unit of measure is appended to the end of the string.
        /// </summary>
        public string ToString(IFormatProvider? formatProvider) => Value.ToString(formatProvider) + "pt";

        /// <summary>
        /// Returns the object as string using the specified format and format information.
        /// The unit of measure is appended to the end of the string.
        /// </summary>
        string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider) + "pt";

        /// <summary>
        /// Returns the object as string. The unit of measure is appended to the end of the string.
        /// </summary>
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture) + "pt";

        /// <summary>
        /// Returns an XUnitPt object.
        /// </summary>
        public static XUnitPt FromPoint(double value) => new(value);

        /// <summary>
        /// Returns an XUnitPt object. Converts the value to Point.
        /// </summary>
        public static XUnitPt FromInch(double value) => new(value * 72);

        /// <summary>
        /// Returns an XUnitPt object. Converts the value to Point.
        /// </summary>
        public static XUnitPt FromMillimeter(double value) => new(value * (72 / 25.4));

        /// <summary>
        /// Returns an XUnitPt object. Converts the value to Point.
        /// </summary>
        public static XUnitPt FromCentimeter(double value) => new(value * (72 / 2.54));

        /// <summary>
        /// Returns an XUnitPt object. Converts the value to Point.
        /// </summary>
        public static XUnitPt FromPresentation(double value) => new(value * (72d / 96));

        /// <summary>
        /// Converts a string to an XUnitPt object.
        /// If the string contains a suffix like 'cm' or 'in' the value will be converted to point.
        /// </summary>
        public static implicit operator XUnitPt(string value) => ((XUnit)value).Point;

        /// <summary>
        /// Converts an int to an XUnitPt object.
        /// </summary>
        public static implicit operator XUnitPt(int value) => new(value);

        /// <summary>
        /// Converts a double to an XUnitPt object.
        /// </summary>
        public static implicit operator XUnitPt(double value) => new(value);

        /// <summary>
        /// Converts an XUnitPt to a double value as point.
        /// </summary>
        public static implicit operator double(XUnitPt value) => value.Point;

        /// <summary>
        /// Converts an XUnit to an XUnitPt object.
        /// </summary>
        public static implicit operator XUnitPt(XUnit value) => value.Point;

        /// <summary>
        /// Converts an XUnitPt to an XUnit object.
        /// </summary>
        public static implicit operator XUnit(XUnitPt value) => XUnit.FromPoint(value);

        /// <summary>
        /// Memberwise comparison checking exact value.
        /// To compare by value tolerating rounding errors, use IsSameValue() or code like Math.Abs(a.Pt - b.Pt) &lt; 1e-5.
        /// </summary>
        // ReSharper disable CompareOfFloatsByEqualityOperator
        public static bool operator ==(XUnitPt value1, XUnitPt value2) => value1.Value == value2.Value;
        // ReSharper restore CompareOfFloatsByEqualityOperator

        /// <summary>
        /// Memberwise comparison checking exact value.
        /// To compare by value tolerating rounding errors, use IsSameValue() or code like Math.Abs(a.Pt - b.Pt) &lt; 1e-5.
        /// </summary>
        // ReSharper disable CompareOfFloatsByEqualityOperator
        public static bool operator !=(XUnitPt value1, XUnitPt value2) => value1.Value != value2.Value;
        // ReSharper restore CompareOfFloatsByEqualityOperator

        /// <summary>
        /// Compares two XUnitPt values.
        /// </summary>
        public static bool operator >(XUnitPt l, XUnitPt r) => l.CompareTo(r) > 0;

        /// <summary>
        /// Compares two XUnitPt values.
        /// </summary>
        public static bool operator >=(XUnitPt l, XUnitPt r) => l.CompareTo(r) >= 0;

        /// <summary>
        /// Compares two XUnitPt values.
        /// </summary>
        public static bool operator <(XUnitPt l, XUnitPt r) => l.CompareTo(r) < 0;

        /// <summary>
        /// Compares two XUnitPt values.
        /// </summary>
        public static bool operator <=(XUnitPt l, XUnitPt r) => l.CompareTo(r) <= 0;

        /// <summary>
        /// Returns the negative value of an XUnitPt.
        /// </summary>
        public static XUnitPt operator -(XUnitPt value) => new(-value.Point);

        /// <summary>
        /// Adds an XUnitPt to an XUnitPt.
        /// </summary>
        public static XUnitPt operator +(XUnitPt l, XUnitPt r) => new(l.Value + r.Value);

        /// <summary>
        /// Adds a string parsed as XUnitPt to an XUnitPt.
        /// </summary>
        public static XUnitPt operator +(XUnitPt l, string r) => new(l.Value + ((XUnitPt)r).Value);

        /// <summary>
        /// Subtracts an XUnitPt from an XUnitPt.
        /// </summary>
        public static XUnitPt operator -(XUnitPt l, XUnitPt r) => new(l.Value - r.Value);

        /// <summary>
        /// Subtracts a string parsed as UnitPt from an XUnitPt.
        /// </summary>
        public static XUnitPt operator -(XUnitPt l, string r) => new(l.Value - ((XUnitPt)r).Value);

        /// <summary>
        /// Multiplies an XUnitPt with a double.
        /// </summary>
        public static XUnitPt operator *(XUnitPt l, double r) => new(l.Value * r);

        /// <summary>
        /// Divides an XUnitPt by a double.
        /// </summary>
        public static XUnitPt operator /(XUnitPt l, double r) => new(l.Value / r);

        /// <summary>
        /// Compares this XUnitPt with another XUnitPt value.
        /// </summary>
        public int CompareTo(XUnitPt other) => Math.Sign(Value - other.Value);

        /// <summary>
        /// Compares this XUnitPt with another object.
        /// </summary>
        public int CompareTo(object? obj) => obj is XUnitPt unit ? CompareTo(unit) : 1;

        /// <summary>
        /// Compares the actual values of this XUnitPt and another XUnitPt value tolerating rounding errors.
        /// </summary>
        public bool IsSameValue(XUnitPt other) => Math.Abs(Value - other.Value) <= 1e-5;

        /// <summary>
        /// Calls base class Equals.
        /// </summary>
        public override bool Equals(Object? obj) => obj is XUnitPt unit && this == unit;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        // Re/Sharper disable NonReadonlyFieldInGetHashCode
        public override int GetHashCode() => Value.GetHashCode();
        // Re/Sharper restore NonReadonlyFieldInGetHashCode

        /// <summary>
        /// Represents a unit with all values zero.
        /// </summary>
        public static readonly XUnitPt Zero = new();

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        /// <value>The debugger display.</value>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay => Invariant($"{Value:0.######} pt");
        // ReSharper restore UnusedMember.Local
    }
}
