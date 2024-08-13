// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.InteropServices;
using PdfSharp.Internal;
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a two-dimensional vector specified by x- and y-coordinates.
    /// It is a displacement in 2-D space.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct XVector : IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XVector"/> struct.
        /// </summary>
        /// <param name="x">The X-offset of the new Vector.</param>
        /// <param name="y">The Y-offset of the new Vector.</param>
        public XVector(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Compares two vectors for equality.
        /// </summary>
        /// <param name="vector1">The first vector to compare.</param>
        /// <param name="vector2">The second vector to compare.</param>
        public static bool operator ==(XVector vector1, XVector vector2)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return vector1._x == vector2._x && vector1._y == vector2._y;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Compares two vectors for inequality.
        /// </summary>
        /// <param name="vector1">The first vector to compare.</param>
        /// <param name="vector2">The second vector to compare.</param>
        public static bool operator !=(XVector vector1, XVector vector2)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return vector1._x != vector2._x || vector1._y != vector2._y;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Compares two vectors for equality.
        /// </summary>
        /// <param name="vector1">The first vector to compare.</param>
        /// <param name="vector2">The second vector to compare.</param>
        public static bool Equals(XVector vector1, XVector vector2)
        {
            if (vector1.X.Equals(vector2.X))
                return vector1.Y.Equals(vector2.Y);
            return false;
        }

        /// <summary>
        /// Determines whether the specified Object is a Vector structure and,
        /// if it is, whether it has the same X and Y values as this vector.
        /// </summary>
        /// <param name="o">The vector to compare.</param>
        public override bool Equals(object? o)
        {
            if (o is not XVector vector)
                return false;
            return Equals(this, vector);
        }

        /// <summary>
        /// Compares two vectors for equality.
        /// </summary>
        /// <param name="value">The vector to compare with this vector.</param>
        public bool Equals(XVector value)
        {
            return Equals(this, value);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return _x.GetHashCode() ^ _y.GetHashCode();
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        /// <summary>
        /// Converts a string representation of a vector into the equivalent Vector structure.
        /// </summary>
        /// <param name="source">The string representation of the vector.</param>
        public static XVector Parse(string source)
        {
            TokenizerHelper helper = new TokenizerHelper(source, CultureInfo.InvariantCulture);
            var str = helper.NextTokenRequired();
            var vector = new XVector(Convert.ToDouble(str, CultureInfo.InvariantCulture), Convert.ToDouble(helper.NextTokenRequired(), CultureInfo.InvariantCulture));
            helper.LastTokenRequired();
            return vector;
        }

        /// <summary>
        /// Gets or sets the X component of this vector.
        /// </summary>
        public double X
        {
            get => _x;
            set => _x = value;
        }
        double _x;

        /// <summary>
        /// Gets or sets the Y component of this vector.
        /// </summary>
        public double Y
        {
            get => _y;
            set => _y = value;
        }
        double _y;

        /// <summary>
        /// Returns the string representation of this Vector structure.
        /// </summary>
        public override string ToString()
        {
            return ConvertToString(null, null);
        }

        /// <summary>
        /// Returns the string representation of this Vector structure with the specified formatting information.
        /// </summary>
        /// <param name="provider">The culture-specific formatting information.</param>
        public string ToString(IFormatProvider provider) 
            => ConvertToString(null, provider);

        string IFormattable.ToString(string? format, IFormatProvider? provider) 
            => ConvertToString(format, provider);

        internal string ConvertToString(string? format, IFormatProvider? provider)
        {
            const char numericListSeparator = ',';
            provider = provider ?? CultureInfo.InvariantCulture;
            // ReSharper disable FormatStringProblem because it is too complex for ReSharper
            return String.Format(provider, "{1:" + format + "}{0}{2:" + format + "}",
                numericListSeparator, _x, _y);
            // ReSharper restore FormatStringProblem
        }

        /// <summary>
        /// Gets the length of this vector.
        /// </summary>
        public double Length
            => Math.Sqrt(_x * _x + _y * _y);

        /// <summary>
        /// Gets the square of the length of this vector.
        /// </summary>
        public double LengthSquared
            => _x * _x + _y * _y;

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        public void Normalize()
        {
            this = this / Math.Max(Math.Abs(_x), Math.Abs(_y));
            this = this / Length;
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector to evaluate.</param>
        /// <param name="vector2">The second vector to evaluate.</param>
        public static double CrossProduct(XVector vector1, XVector vector2)
            => vector1._x * vector2._y - vector1._y * vector2._x;

        /// <summary>
        /// Retrieves the angle, expressed in degrees, between the two specified vectors.
        /// </summary>
        /// <param name="vector1">The first vector to evaluate.</param>
        /// <param name="vector2">The second vector to evaluate.</param>
        public static double AngleBetween(XVector vector1, XVector vector2)
        {
            double y = vector1._x * vector2._y - vector2._x * vector1._y;
            double x = vector1._x * vector2._x + vector1._y * vector2._y;
            return Math.Atan2(y, x) * 57.295779513082323;
        }

        /// <summary>
        /// Negates the specified vector.
        /// </summary>
        /// <param name="vector">The vector to negate.</param>
        public static XVector operator -(XVector vector)
            => new XVector(-vector._x, -vector._y);

        /// <summary>
        /// Negates this vector. The vector has the same magnitude as before, but its direction is now opposite.
        /// </summary>
        public void Negate()
        {
            _x = -_x;
            _y = -_y;
        }

        /// <summary>
        /// Adds two vectors and returns the result as a vector.
        /// </summary>
        /// <param name="vector1">The first vector to add.</param>
        /// <param name="vector2">The second vector to add.</param>
        public static XVector operator +(XVector vector1, XVector vector2)
            => new(vector1._x + vector2._x, vector1._y + vector2._y);

        /// <summary>
        /// Adds two vectors and returns the result as a Vector structure.
        /// </summary>
        /// <param name="vector1">The first vector to add.</param>
        /// <param name="vector2">The second vector to add.</param>
        public static XVector Add(XVector vector1, XVector vector2)
            => new(vector1._x + vector2._x, vector1._y + vector2._y);

        /// <summary>
        /// Subtracts one specified vector from another.
        /// </summary>
        /// <param name="vector1">The vector from which vector2 is subtracted.</param>
        /// <param name="vector2">The vector to subtract from vector1.</param>
        public static XVector operator -(XVector vector1, XVector vector2) 
            => new(vector1._x - vector2._x, vector1._y - vector2._y);

        /// <summary>
        /// Subtracts the specified vector from another specified vector.
        /// </summary>
        /// <param name="vector1">The vector from which vector2 is subtracted.</param>
        /// <param name="vector2">The vector to subtract from vector1.</param>
        public static XVector Subtract(XVector vector1, XVector vector2)
            => new(vector1._x - vector2._x, vector1._y - vector2._y);

        /// <summary>
        /// Translates a point by the specified vector and returns the resulting point.
        /// </summary>
        /// <param name="vector">The vector used to translate point.</param>
        /// <param name="point">The point to translate.</param>
        public static XPoint operator +(XVector vector, XPoint point) 
            => new(point.X + vector._x, point.Y + vector._y);

        /// <summary>
        /// Translates a point by the specified vector and returns the resulting point.
        /// </summary>
        /// <param name="vector">The vector used to translate point.</param>
        /// <param name="point">The point to translate.</param>
        public static XPoint Add(XVector vector, XPoint point) 
            => new(point.X + vector._x, point.Y + vector._y);

        /// <summary>
        /// Multiplies the specified vector by the specified scalar and returns the resulting vector.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar to multiply.</param>
        public static XVector operator *(XVector vector, double scalar) 
            => new(vector._x * scalar, vector._y * scalar);

        /// <summary>
        /// Multiplies the specified vector by the specified scalar and returns the resulting vector.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar to multiply.</param>
        public static XVector Multiply(XVector vector, double scalar) 
            => new(vector._x * scalar, vector._y * scalar);

        /// <summary>
        /// Multiplies the specified scalar by the specified vector and returns the resulting vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="vector">The vector to multiply.</param>
        public static XVector operator *(double scalar, XVector vector) 
            => new(vector._x * scalar, vector._y * scalar);

        /// <summary>
        /// Multiplies the specified scalar by the specified vector and returns the resulting Vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="vector">The vector to multiply.</param>
        public static XVector Multiply(double scalar, XVector vector) 
            => new(vector._x * scalar, vector._y * scalar);

        /// <summary>
        /// Divides the specified vector by the specified scalar and returns the resulting vector.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The scalar by which vector will be divided.</param>
        public static XVector operator /(XVector vector, double scalar) 
            => vector * (1.0 / scalar);

        /// <summary>
        /// Divides the specified vector by the specified scalar and returns the result as a Vector.
        /// </summary>
        /// <param name="vector">The vector structure to divide.</param>
        /// <param name="scalar">The amount by which vector is divided.</param>
        public static XVector Divide(XVector vector, double scalar) 
            => vector * (1.0 / scalar);

        /// <summary>
        /// Transforms the coordinate space of the specified vector using the specified Matrix.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <param name="matrix">The transformation to apply to vector.</param>
        public static XVector operator *(XVector vector, XMatrix matrix) 
            => matrix.Transform(vector);

        /// <summary>
        /// Transforms the coordinate space of the specified vector using the specified Matrix.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <param name="matrix">The transformation to apply to vector.</param>
        public static XVector Multiply(XVector vector, XMatrix matrix) 
            => matrix.Transform(vector);

        /// <summary>
        /// Calculates the dot product of the two specified vector structures and returns the result as a Double.
        /// </summary>
        /// <param name="vector1">The first vector to multiply.</param>
        /// <param name="vector2">The second vector to multiply.</param>
        public static double operator *(XVector vector1, XVector vector2) 
            => vector1._x * vector2._x + vector1._y * vector2._y;

        /// <summary>
        /// Calculates the dot product of the two specified vectors and returns the result as a Double.
        /// </summary>
        /// <param name="vector1">The first vector to multiply.</param>
        /// <param name="vector2">The second vector structure to multiply.</param>
        public static double Multiply(XVector vector1, XVector vector2) 
            => vector1._x * vector2._x + vector1._y * vector2._y;

        /// <summary>
        /// Calculates the determinant of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector to evaluate.</param>
        /// <param name="vector2">The second vector to evaluate.</param>
        public static double Determinant(XVector vector1, XVector vector2) 
            => vector1._x * vector2._y - vector1._y * vector2._x;

        /// <summary>
        /// Creates a Size from the offsets of this vector.
        /// </summary>
        /// <param name="vector">The vector to convert.</param>
        public static explicit operator XSize(XVector vector) 
            => new(Math.Abs(vector._x), Math.Abs(vector._y));

        /// <summary>
        /// Creates a Point with the X and Y values of this vector.
        /// </summary>
        /// <param name="vector">The vector to convert.</param>
        public static explicit operator XPoint(XVector vector) 
            => new(vector._x, vector._y);

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
                const string format = Config.SignificantDecimalPlaces10;
                return String.Format(CultureInfo.InvariantCulture, "vector=({0:" + format + "}, {1:" + format + "})", _x, _y);
            }
        }
    }
}
