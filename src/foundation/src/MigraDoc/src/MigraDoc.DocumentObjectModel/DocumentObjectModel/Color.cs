// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Internals;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using FLOAT = System.Single;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// The Color class represents an ARGB color value.
    /// </summary>
    [DebuggerDisplay("(A={A}, R={R}, G={G}, B={B} C={C}, M={M}, Y={Y}, K={K})")]
    public struct Color : INullableValue
    {
        // Breaking change: Color.Empty was defined as RGB black with opacity of 0.
        // Sine v6.0 Color.Empty is a special value where _isCmyk is null and all other fields are 0.
        // Therefore it depends now what 'new Color(0)' means. Set Capabilities.BackwardCompatibility.TreatArgbZeroAsEmpty
        // to false and the constructor creates a an RGB black with opacity of 0 or set it
        // to true and it creates an Color.Empty as in versions prior to v6.0.

        /// <summary>
        /// Initializes a new instance of the Color class.
        /// </summary>
        public Color(uint argb)
        {
            if (argb == 0 && Capabilities.BackwardCompatibility.TreatArgbZeroAsEmptyColor)
            {
                LogHost.Logger.ArgbValueIsConsideredEmptyColor(LogLevel.Information);
                this = Color.Empty;
                return;
            }

            _isCmyk = false;
            _argb = argb;
            //_a = _c = _m = _y = _k = 0f; not needed anymore since C# 10.
            InitCmykFromRgb();
        }

        static Color()
        {
            // Recall that a static constructor is thread safe and we need no lock here.
            var colorNames = Enum.GetNames(typeof(ColorName));
            var colorValues = Enum.GetValues(typeof(ColorName));
            int count = colorNames.GetLength(0);
            for (int index = 0; index < count; index++)
            {
                var c = colorNames[index];
                var d = (uint)colorValues.GetValue(index)!;
                // Some colors are double named:
                // Aqua == Cyan
                // Fuchsia == Magenta
                //var key = new Style();  ??? 
                StandardColors.TryAdd(d, c);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Color class.
        /// </summary>
        public Color(byte r, byte g, byte b)
        {
            _isCmyk = false;
            _argb = 0xFF000000 | ((uint)r << 16) | ((uint)g << 8) | b;
            _a = _c = _m = _y = _k = 0f;
            InitCmykFromRgb();
        }

        /// <summary>
        /// Initializes a new instance of the Color class.
        /// </summary>
        public Color(byte a, byte r, byte g, byte b)
        {
            _isCmyk = false;
            _argb = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
            _a = _c = _m = _y = _k = 0f;
            InitCmykFromRgb();
        }

        /// <summary>
        /// Initializes a new instance of the Color class with a CMYK color.
        /// All values must be in the range 0 to 100 percent.
        /// </summary>
        public Color(double alpha, double cyan, double magenta, double yellow, double black)
        {
            _isCmyk = true;
            _a = (float)(alpha > 100 ? 100 : (alpha < 0 ? 0 : alpha));
            _c = (float)(cyan > 100 ? 100 : (cyan < 0 ? 0 : cyan));
            _m = (float)(magenta > 100 ? 100 : (magenta < 0 ? 0 : magenta));
            _y = (float)(yellow > 100 ? 100 : (yellow < 0 ? 0 : yellow));
            _k = (float)(black > 100 ? 100 : (black < 0 ? 0 : black));
            _argb = 0; // Compiler enforces this line of code
            InitRgbFromCmyk();
        }

        /// <summary>
        /// Initializes a new instance of the Color class with a CMYK color.
        /// All values must be in the range 0 to 100 percent.
        /// </summary>
        public Color(double cyan, double magenta, double yellow, double black)
            : this(100, cyan, magenta, yellow, black)
        { }

        void InitCmykFromRgb()
        {
            // Similar formula as in PDFsharp.
            _isCmyk = false;
            int c = 255 - (int)R;
            int m = 255 - (int)G;
            int y = 255 - (int)B;
            int k = Math.Min(c, Math.Min(m, y));
            if (k == 255)
                _c = _m = _y = 0;
            else
            {
                float black = 255f - k;
                _c = 100f * (c - k) / black;
                _m = 100f * (m - k) / black;
                _y = 100f * (y - k) / black;
            }
            _k = 100f * k / 255f;
            _a = A / 2.55f;
        }

        void InitRgbFromCmyk()
        {
            // Similar formula as in PDFsharp.
            _isCmyk = true;
            float black = _k * 2.55f + 0.5f;
            float factor = (255f - black) / 100f;
            byte a = (byte)(_a * 2.55f + 0.5);
            byte r = (byte)(255 - Math.Min(255f, _c * factor + black));
            byte g = (byte)(255 - Math.Min(255f, _m * factor + black));
            byte b = (byte)(255 - Math.Min(255f, _y * factor + black));
            _argb = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a CMYK color.
        /// </summary>
        public bool IsCmyk => _isCmyk ?? false;

        /// <summary>
        /// Determines whether this color is empty.
        /// </summary>
        public bool IsEmpty => _isCmyk == null;

        /// <summary>
        /// Returns the value.
        /// </summary>
        object INullableValue.GetValue() => this;

        /// <summary>
        /// Sets the given value.
        /// </summary>
        void INullableValue.SetValue(object? value)
        {
            if (value is uint u)
            {
                _isCmyk = false;
                _argb = u;
                InitCmykFromRgb();
            }
            else
                this = Parse(value?.ToString() ?? "");
        }

        /// <summary>
        /// Resets this instance, i.e. IsNull() will return true afterwards.
        /// </summary>
        void INullableValue.SetNull() => this = Empty;

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        bool INullableValue.IsNull => _isCmyk == null;

        /// <summary>
        /// Gets or sets the ARGB value.
        /// </summary>
        public uint Argb
        {
            // TODO Check performance
            //get => _isCmyk is not null ? _argb : throw new InvalidOperationException();  // oder LogHost.Warning.. 
            //[Inline]
            get => _argb;
            set
            {
                if (_isCmyk is true)
                    throw new InvalidOperationException("Cannot change a CMYK color.");
                _argb = value;
                InitCmykFromRgb();
            }
        }

        /// <summary>
        /// Gets or sets the RGB value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public uint RGB
        {
            get => _argb & 0xFFFFFF;
            set
            {
                if (_isCmyk is true)
                    throw new InvalidOperationException("Cannot change a CMYK color.");
                _argb = value;
                InitCmykFromRgb();
            }
        }

        /// <summary>
        /// Calls base class Equals.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is Color color)
            {
                if (_isCmyk == null)
                    return color._isCmyk == null;

                if (_isCmyk != color._isCmyk)
                    return false;

                if (_isCmyk.Value)
                {
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    return _a == color._a && _c == color._c && _m == color._m && _y == color._y && _k == color._k;
                    // ReSharper restore CompareOfFloatsByEqualityOperator
                }
                return _argb == color._argb;
            }
            return false;
        }

        /// <summary>
        /// Gets the ARGB value that this Color instance represents.
        /// </summary>
        public override int GetHashCode()
            => (int)_argb ^ _a.GetHashCode() ^ _c.GetHashCode() ^ _m.GetHashCode() ^ _y.GetHashCode() ^ _k.GetHashCode();

        /// <summary>
        /// Compares two color objects. True if both ARGB values are equal, false otherwise.
        /// </summary>
        public static bool operator ==(Color color1, Color color2)
        {
            if (color1._isCmyk == null)
                return color2._isCmyk == null;

            if (color2._isCmyk == null)
                return false;

            if (color1._isCmyk.Value)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return color1._a == color2._a && color1._c == color2._c && color1._m == color2._m &&
                       color1._y == color2._y && color1._k == color2._k;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
            return color1._argb == color2._argb;
        }

        /// <summary>
        /// Compares two color objects. True if both ARGB values are not equal, false otherwise.
        /// </summary>
        public static bool operator !=(Color color1, Color color2)
            => !(color1 == color2);

        /// <summary>
        /// Parses the string and returns a color object.
        /// Throws ArgumentException if color is invalid.
        /// Supports four different formats for hex colors.
        /// Format 1: uses prefix "0x", followed by as many hex digits as needed. Important: do not forget the opacity, so use 7 or 8 digits.
        /// Format 2: uses prefix "#", followed by exactly 8 digits including opacity.
        /// Format 3: uses prefix "#", followed by exactly 6 digits; opacity will be 0xff.
        /// Format 4: uses prefix "#", followed by exactly 3 digits; opacity will be 0xff; "#ccc" will be treated as "#ffcccccc", "#d24" will be treated as "#ffdd2244".
        /// </summary>
        /// <param name="color">integer, hex or color name.</param>
        public static Color Parse(string color)
        {
            if (color == null)
                throw new ArgumentNullException(nameof(color));

            #region Excursion on checking a string to be empty
            // Even after 20+ years the compiler still calls op_Equality for comparing with "" instead
            // of checking 'color.Length == 0'. Microsoft uses in .NET always the comparison with 0
            // because it is faster regarding to the documentation. I typically use String.IsNullOrEmpty,
            // but in case I already know the string is not null I want to write 's == ""' or 's != ""' and
            // expect that the compiler create the fastest code for me. I know the operators are redefined and
            // call Equals which calls EqualsHelper which works on the underlying span. But comparing to "" is
            // a special case.
            //
            // Over two decades I cannot see a reason why the substitution may not be save in any case.
            // So I asked ChatGPT today:
            // 
            //   Why does the C# compiler does not replace a string comparision with "" with a length comparison to 0?
            //   
            //
            //   The C# compiler does not automatically replace a string comparison with "" with a length comparison to 0
            //   because the two operations are not always equivalent.
            //   
            //   While comparing a string to "" may seem like it's checking for an empty string, it's actually checking
            //   for a specific value of an empty string.This can be a problem if the empty string is represented by
            //   something other than "" in the code, such as null or whitespace.
            //   
            //   On the other hand, comparing the length of a string to 0 is always checking for an empty string,
            //   regardless of how it's represented in the code. However, this operation can be less efficient than comparing
            //   a string to "", because it requires calculating the length of the string before doing the comparison.
            //   
            //   Therefore, the decision of whether to use a string comparison with "" or a length comparison to 0 should
            //   depend on the specific use case and the expected behavior of the code.In general, if you want to check if
            //   a string is empty regardless of how it's represented, a length comparison to 0 is a safer choice.
            //   But if you specifically want to check for an empty string represented by "", then a string comparison with ""
            //   may be more appropriate.
            //
            // Impressive answer, but still no explanation. There is no specific use case where a string with a length other
            // then 0 is considered to be the empty string. I think Microsoft has a reason. If you know it send me a mail.
            #endregion
            if (color == "")
                throw new ArgumentException(nameof(color));

            try
            {
                uint clr;
                // Must use Enum.Parse because Enum.IsDefined is case sensitive
                try
                {
                    var obj = Enum.Parse(typeof(ColorName), color, true);
                    clr = (uint)obj;
                    return new(clr);
                }
                catch
                {
                    // Ignore exception because it's not a ColorName.
                }

                var numberStyle = NumberStyles.Integer;
                string number = color.ToLower();
                if (number.StartsWith("0x", StringComparison.Ordinal))
                {
                    numberStyle = NumberStyles.HexNumber;
                    number = color[2..];
                }
                else if (number.StartsWith("#", StringComparison.Ordinal))
                {
                    numberStyle = NumberStyles.HexNumber;
                    switch (color.Length)
                    {
                        case 9: // Format "#aarrggbb".
                            number = color[1..];
                            break;
                        case 7: // Format "#rrggbb".
                            number = "ff" + color[1..];
                            break;
                        case 4:  // Format "#rgb".
                            var r = color[1..2];
                            var g = color[2..3];
                            var b = color[3..4];
                            number = "ff" + r + r + g + g + b + b;
                            break;
                        default:
                            throw new ArgumentException(DomSR.InvalidColorString(color), nameof(color));
                    }
                }
                clr = UInt32.Parse(number, numberStyle);
                return new(clr);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(DomSR.InvalidColorString(color), ex);
            }
        }

        /// <summary>
        /// Gets the alpha (transparency) part of the RGB Color.
        /// The value is in the range 0 to 255.
        /// </summary>
        public uint A => (_argb & 0xFF000000) >> 24;

        /// <summary>
        /// Gets the red part of the Color.
        /// The value is in the range 0 to 255.
        /// </summary>
        public uint R => (_argb & 0xFF0000) >> 16;

        /// <summary>
        /// Gets the green part of the Color.
        /// The value is in the range 0 to 255.
        /// </summary>
        public uint G => (_argb & 0x00FF00) >> 8;

        /// <summary>
        /// Gets the blue part of the Color.
        /// The value is in the range 0 to 255.
        /// </summary>
        public uint B => _argb & 0x0000FF;

        /// <summary>
        /// Gets the alpha (transparency) part of the CMYK Color.
        /// The value is in the range 0 (transparent) to 100 (opaque) percent.
        /// </summary>
        public FLOAT Alpha => _a;

        /// <summary>
        /// Gets the cyan part of the Color.
        /// The value is in the range 0 to 100 percent.
        /// </summary>
        public FLOAT C => _c;

        /// <summary>
        /// Gets the magenta part of the Color.
        /// The value is in the range 0 to 100 percent.
        /// </summary>
        public FLOAT M => _m;

        /// <summary>
        /// Gets the yellow part of the Color.
        /// The value is in the range 0 to 100 percent.
        /// </summary>
        public FLOAT Y => _y;

        /// <summary>
        /// Gets the key (black) part of the Color.
        /// The value is in the range 0 to 100 percent.
        /// </summary>
        public FLOAT K => _k;

        /// <summary>
        /// Gets a non-transparent color brightened in terms of transparency if any is given (A &lt; 255),
        /// otherwise this instance itself.
        /// </summary>
        public Color GetMixedTransparencyColor()
        {
            // Think of Empty as the neutral element.
            if (IsEmpty)
                return this;

            //if (_isCmyk is true)
            //{
            //    // Implement if really required.
            //}

            int alpha = (int)A;
            if (alpha == 0xFF)
                return this;

            int red = (int)R;
            int green = (int)G;
            int blue = (int)B;

            float whiteFactor = 1 - alpha / 255f;

            red = (int)(red + (255 - red) * whiteFactor);
            green = (int)(green + (255 - green) * whiteFactor);
            blue = (int)(blue + (255 - blue) * whiteFactor);
            return new((uint)(0xFF << 24 | (red << 16) | (green << 8) | blue));
        }

        /// <summary>
        /// Writes the Color object in its hexadecimal value.
        /// </summary>
        public override string ToString()
        {
            if (_isCmyk == null)
                return "";

            if (_isCmyk.Value)
            {
                string s;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                s = _a == 100f ?
                    String.Format(CultureInfo.InvariantCulture, "CMYK({0:0.##},{1:0.##},{2:0.##},{3:0.##})", C, M, Y, K) :
                    String.Format(CultureInfo.InvariantCulture, "CMYK({0:0.##},{1:0.##},{2:0.##},{3:0.##},{4:0.##})", Alpha, C, M, Y, K);
                return s;
            }
            else
            {
                if (StandardColors.ContainsKey(_argb))
                    return StandardColors[_argb];
                if ((_argb & 0xFF000000) == 0xFF000000)
                    return "RGB(" +
                           ((_argb & 0xFF0000) >> 16) + "," +
                           ((_argb & 0x00FF00) >> 8) + "," +
                           (_argb & 0x0000FF) + ")";
                return "0x" + _argb.ToString("X");
            }
        }

        static readonly Dictionary<uint, string> StandardColors = new(Enum.GetNames(typeof(ColorName)).Length);

        /// <summary>
        /// Creates an RGB color from an existing color with a new alpha (transparency) value.
        /// </summary>
        public static Color FromRgbColor(byte a, Color color)
            => new Color(a, (byte)color.R, (byte)color.G, (byte)color.B);

        /// <summary>
        /// Creates an RGB color from red, green, and blue.
        /// </summary>
        public static Color FromRgb(byte r, byte g, byte b)
            => new Color(255, r, g, b);

        /// <summary>
        /// Creates an RGB color from alpha, red, green, and blue.
        /// </summary>
        public static Color FromArgb(byte alpha, byte r, byte g, byte b)
            => new Color(alpha, r, g, b);

        /// <summary>
        /// Creates a Color structure from the specified CMYK values.
        /// All values must be in the range 0 to 100 percent.
        /// </summary>
        public static Color FromCmyk(double cyan, double magenta, double yellow, double black)
            => new Color(cyan, magenta, yellow, black);

        /// <summary>
        /// Creates a Color structure from the specified CMYK values.
        /// All values must be in the range 0 to 100 percent.
        /// </summary>
        public static Color FromCmyk(double alpha, double cyan, double magenta, double yellow, double black)
            => new Color(alpha, cyan, magenta, yellow, black);

        /// <summary>
        /// Creates a CMYK color from an existing color with a new alpha (transparency) value.
        /// </summary>
        public static Color FromCmykColor(double alpha, Color color)
            => new Color(alpha, color.C, color.M, color.Y, color.K);

        internal static Color? MakeNullIfEmpty(Color? color)
            => color == Empty ? null : color;

        uint _argb; // ARGB
        bool? _isCmyk; // Is null means color is undefined, which is the same as Empty.
        float _a; // \
        float _c; // |
        float _m; // |--- alpha + CMYK
        float _y; // |
        float _k; // /

        /// <summary>
        /// Represents the null color.
        /// Each property of the null color is zero.
        /// </summary>
        public static readonly Color Empty = new();
    }
}
