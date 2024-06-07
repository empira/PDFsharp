// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
#if CORE
#endif
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
#if true_
    /// <summary>
    /// Not used in this implementation.
    /// </summary>
    [Flags]
    public enum XStringFormatFlags
    {
        //DirectionRightToLeft  = 0x0001,
        //DirectionVertical     = 0x0002,
        //FitBlackBox           = 0x0004,
        //DisplayFormatControl  = 0x0020,
        //NoFontFallback        = 0x0400,
        /// <summary>
        /// The default value.
        /// </summary>
        MeasureTrailingSpaces = 0x0800,
        //NoWrap                = 0x1000,
        //LineLimit             = 0x2000,
        //NoClip                = 0x4000,
    }
#endif

    /// <summary>
    /// Represents the text layout information.
    /// </summary>
    public class XStringFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XStringFormat"/> class.
        /// </summary>
        public XStringFormat()
        { }

        //TODO public StringFormat(StringFormat format);
        //public StringFormat(StringFormatFlags options);
        //public StringFormat(StringFormatFlags options, int language);
        //public object Clone();
        //public void Dispose();
        //private void Dispose(bool disposing);
        //protected override void Finalize();
        //public float[] GetTabStops(out float firstTabOffset);
        //public void SetDigitSubstitution(int language, StringDigitSubstitute substitute);
        //public void SetMeasurableCharacterRanges(CharacterRange[] ranges);
        //public void SetTabStops(float firstTabOffset, float[] tabStops);
        //public override string ToString();

        /// <summary>
        /// Gets or sets horizontal text alignment information.
        /// </summary>
        public XStringAlignment Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
#if GDI
                // Update StringFormat if it exists.
                if (_stringFormat != null)
                {
                    _stringFormat.Alignment = (StringAlignment)value;
                }
#endif
            }
        }
        XStringAlignment _alignment;

        //public int DigitSubstitutionLanguage { get; }
        //public StringDigitSubstitute DigitSubstitutionMethod { get; }
        //public StringFormatFlags FormatFlags { get; set; }
        //public static StringFormat GenericDefault { get; }
        //public static StringFormat GenericTypographic { get; }
        //public HotkeyPrefix HotkeyPrefix { get; set; }

        /// <summary>
        /// Gets or sets the line alignment.
        /// </summary>
        public XLineAlignment LineAlignment
        {
            get { return _lineAlignment; }
            set
            {
                _lineAlignment = value;
#if GDI
                // Update StringFormat if it exists.
                if (_stringFormat != null)
                {
                    // BaseLine is specific to PDFsharp.
                    if (value == XLineAlignment.BaseLine)
                        _stringFormat.LineAlignment = StringAlignment.Near;
                    else
                        _stringFormat.LineAlignment = (StringAlignment)value;
                }
#endif
            }
        }
        XLineAlignment _lineAlignment;

        //public StringTrimming Trimming { get; set; }

        /// <summary>
        /// Gets a new XStringFormat object that aligns the text left on the base line.
        /// </summary>
        [Obsolete("Use XStringFormats.Default. (Note plural in class name.)")]
        public static XStringFormat Default => XStringFormats.Default;

        /// <summary>
        /// Gets a new XStringFormat object that aligns the text top left of the layout rectangle.
        /// </summary>
        [Obsolete("Use XStringFormats.Default. (Note plural in class name.)")]
        public static XStringFormat TopLeft => XStringFormats.TopLeft;

        /// <summary>
        /// Gets a new XStringFormat object that centers the text in the middle of the layout rectangle.
        /// </summary>
        [Obsolete("Use XStringFormats.Center. (Note plural in class name.)")]
        public static XStringFormat Center => XStringFormats.Center;

        /// <summary>
        /// Gets a new XStringFormat object that centers the text at the top of the layout rectangle.
        /// </summary>
        [Obsolete("Use XStringFormats.TopCenter. (Note plural in class name.)")]
        public static XStringFormat TopCenter => XStringFormats.TopCenter;

        /// <summary>
        /// Gets a new XStringFormat object that centers the text at the bottom of the layout rectangle.
        /// </summary>
        [Obsolete("Use XStringFormats.BottomCenter. (Note plural in class name.)")]
        public static XStringFormat BottomCenter => XStringFormats.BottomCenter;

#if GDI
        internal StringFormat RealizeGdiStringFormat()
        {
            if (_stringFormat == null)
            {
                // It seems that StringFormat.GenericTypographic is a global object and we need "Clone()" to avoid side effects.
                _stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                _stringFormat.Alignment = (StringAlignment)_alignment;

                // BaseLine is specific to PDFsharp.
                if (_lineAlignment == XLineAlignment.BaseLine)
                    _stringFormat.LineAlignment = StringAlignment.Near;
                else
                    _stringFormat.LineAlignment = (StringAlignment)_lineAlignment;

                //_stringFormat.FormatFlags = (StringFormatFlags)_formatFlags;

                // Bugfix: Set MeasureTrailingSpaces to get the correct width with Graphics.MeasureString().
                // Before, MeasureString() didn’t include blanks in width calculation, which could result in text overflowing table or page border before wrapping. $MaOs
                _stringFormat.FormatFlags = _stringFormat.FormatFlags | StringFormatFlags.MeasureTrailingSpaces;
            }
            return _stringFormat;
        }
        StringFormat? _stringFormat;
#endif
    }
}
