// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.InteropServices;
#if GDI
#endif
#if WPF
using SysPoint = System.Windows.Point;
using SysSize = System.Windows.Size;
#endif
#if WUI
using Windows.UI.Xaml.Media;
using SysPoint = Windows.Foundation.Point;
using SysSize = Windows.Foundation.Size;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a pair of floating-point numbers, typically the width and height of a
    /// graphical object.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    [Serializable, StructLayout(LayoutKind.Sequential)] //, ValueSerializer(typeof(SizeValueSerializer)), TypeConverter(typeof(SizeConverter))]
    public struct XSize : IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the XSize class with the specified values.
        /// </summary>
        public XSize(double width, double height)
        {
            if (width < 0 || height < 0)
                throw new ArgumentException("WidthAndHeightCannotBeNegative"); // TODO_OLD SR.Get(SRID.Size_WidthAndHeightCannotBeNegative, new object[0]));

            _width = width;
            _height = height;
        }

        /// <summary>
        /// Determines whether two size objects are equal.
        /// </summary>
        public static bool operator ==(XSize size1, XSize size2)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return size1.Width == size2.Width && size1.Height == size2.Height;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Determines whether two size objects are not equal.
        /// </summary>
        public static bool operator !=(XSize size1, XSize size2) => !(size1 == size2);

        /// <summary>
        /// Indicates whether these two instances are equal.
        /// </summary>
        public static bool Equals(XSize size1, XSize size2)
        {
            if (size1.IsEmpty)
                return size2.IsEmpty;
            return size1.Width.Equals(size2.Width) && size1.Height.Equals(size2.Height);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public override bool Equals(object? o)
        {
            if (o is not XSize size)
                return false;
            return Equals(this, size);
        }

        /// <summary>
        /// Indicates whether this instance and a specified size are equal.
        /// </summary>
        public bool Equals(XSize value) => Equals(this, value);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            if (IsEmpty)
                return 0;
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        /// <summary>
        /// Parses the size from a string.
        /// </summary>
        public static XSize Parse(string source)
        {
            XSize empty;
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            TokenizerHelper helper = new TokenizerHelper(source, cultureInfo);
            var str = helper.NextTokenRequired();
            if (str == "Empty")
                empty = Empty;
            else
                empty = new XSize(Convert.ToDouble(str, cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo));
            helper.LastTokenRequired();
            return empty;
        }

#if GDI
        /// <summary>
        /// Converts this XSize to a PointF.
        /// </summary>
        public PointF ToPointF()
        {
            return new PointF((float)_width, (float)_height);
        }
#endif

        /// <summary>
        /// Converts this XSize to an XPoint.
        /// </summary>
        public XPoint ToXPoint() => new(_width, _height);

        /// <summary>
        /// Converts this XSize to an XVector.
        /// </summary>
        public XVector ToXVector() => new(_width, _height);

#if GDI
        /// <summary>
        /// Converts this XSize to a SizeF.
        /// </summary>
        public SizeF ToSizeF() => new((float)_width, (float)_height);
#endif

#if WPF || WUI
        /// <summary>
        /// Converts this XSize to a System.Windows.Size.
        /// </summary>
        public SysSize ToSize() => new(_width, _height);
#endif

#if GDI
        /// <summary>
        /// Creates an XSize from a System.Drawing.Size.
        /// </summary>
        public static XSize FromSize(Size size) => new(size.Width, size.Height);

        /// <summary>
        /// Implicit conversion from XSize to System.Drawing.Size. The conversion must be implicit because the
        /// WinForms designer uses it.
        /// </summary>
        public static implicit operator XSize(Size size) => new(size.Width, size.Height);
#endif

#if WPF || WUI
        /// <summary>
        /// Creates an XSize from a System.Drawing.Size.
        /// </summary>
        public static XSize FromSize(SysSize size)
        {
            return new XSize(size.Width, size.Height);
        }
#endif

#if GDI
        /// <summary>
        /// Creates an XSize from a System.Drawing.Size.
        /// </summary>
        public static XSize FromSizeF(SizeF size) => new(size.Width, size.Height);
#endif

        /// <summary>
        /// Converts this XSize to a human-readable string.
        /// </summary>
        public override string ToString() => ConvertToString(null, null);

        /// <summary>
        /// Converts this XSize to a human-readable string.
        /// </summary>
        public string ToString(IFormatProvider provider) => ConvertToString(null, provider);

        /// <summary>
        /// Converts this XSize to a human-readable string.
        /// </summary>
        string IFormattable.ToString(string? format, IFormatProvider? provider) 
            => ConvertToString(format, provider);

        internal string ConvertToString(string? format, IFormatProvider? provider)
        {
            if (IsEmpty)
                return "Empty";

            char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
            provider = provider ?? CultureInfo.InvariantCulture;
            // ReSharper disable FormatStringProblem because it is too complex for ReSharper
            return String.Format(provider, "{1:" + format + "}{0}{2:" + format + "}",
                numericListSeparator, _width, _height);
            // ReSharper restore FormatStringProblem
        }

        /// <summary>
        /// Returns an empty size, i.e. a size with a width or height less than 0.
        /// </summary>
        public static XSize Empty => s_empty;

        static readonly XSize s_empty = new XSize
        {
            _width = Double.NegativeInfinity,
            _height = Double.NegativeInfinity
        };

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        public bool IsEmpty => _width < 0;

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public double Width
        {
            get => _width;
            set
            {
                if (IsEmpty)
                    throw new InvalidOperationException("CannotModifyEmptySize"); // TODO_OLD SR.Get(SRID.Size_CannotModifyEmptySize, new object[0]));
                if (value < 0)
                    throw new ArgumentException("WidthCannotBeNegative"); // TODO_OLD SR.Get(SRID.Size_WidthCannotBeNegative, new object[0]));
                _width = value;
            }
        }
        double _width;

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public double Height
        {
            get => _height;
            set
            {
                if (IsEmpty)
                    throw new InvalidOperationException("CannotModifyEmptySize"); // TODO_OLD SR.Get(SRID.Size_CannotModifyEmptySize, new object[0]));
                if (value < 0)
                    throw new ArgumentException("HeightCannotBeNegative"); // TODO_OLD SR.Get(SRID.Size_HeightCannotBeNegative, new object[0]));
                _height = value;
            }
        }
        double _height;

        /// <summary>
        /// Performs an explicit conversion from XSize to XVector.
        /// </summary>
        public static explicit operator XVector(XSize size) => new(size._width, size._height);

        /// <summary>
        /// Performs an explicit conversion from XSize to XPoint.
        /// </summary>
        public static explicit operator XPoint(XSize size) => new(size._width, size._height);

#if WPF || WUI
        /// <summary>
        /// Performs an explicit conversion from Size to XSize.
        /// </summary>
        public static explicit operator XSize(SysSize size) => new(size.Width, size.Height);
#endif

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
                return String.Format(CultureInfo.InvariantCulture,
                    "size=({2}{0:" + format + "}, {1:" + format + "})",
                    _width, _height, IsEmpty ? "Empty " : "");
            }
        }
    }
}
