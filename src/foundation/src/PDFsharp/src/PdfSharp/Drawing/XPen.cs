// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using PdfSharp.Internal;
using System.Drawing.Drawing2D;
using GdiPen = System.Drawing.Pen;
#endif
#if WPF
using System.Windows.Media;
using WpfPen = System.Windows.Media.Pen;
using WpfBrush = System.Windows.Media.Brush;
#endif

namespace PdfSharp.Drawing
{
    // TODO_OLD Free GDI objects (pens, brushes, ...) automatically without IDisposable.
    /// <summary>
    /// Defines an object used to draw lines and curves.
    /// </summary>
    public sealed class XPen
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XPen"/> class.
        /// </summary>
        public XPen(XColor color)
            : this(color, 1, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPen"/> class.
        /// </summary>
        public XPen(XColor color, double width)
            : this(color, width, false)
        { }

        internal XPen(XColor color, double width, bool immutable)
        {
            _color = color;
            _width = width;
            _lineJoin = XLineJoin.Miter;
            _lineCap = XLineCap.Flat;
            _dashStyle = XDashStyle.Solid;
            _dashOffset = 0f;
            _immutable = immutable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPen"/> class.
        /// </summary>
        public XPen(XPen pen)
        {
            _color = pen._color;
            _width = pen._width;
            _lineJoin = pen._lineJoin;
            _lineCap = pen._lineCap;
            _dashStyle = pen._dashStyle;
            _dashOffset = pen._dashOffset;
            _dashPattern = pen._dashPattern;
            if (_dashPattern != null)
                _dashPattern = (double[])_dashPattern.Clone();
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public XPen Clone()
            => new XPen(this);

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public XColor Color
        {
            get => _color;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                _dirty = _dirty || _color != value;
                _color = value;
            }
        }
        XColor _color;

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public double Width
        {
            get => _width;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                _dirty = _dirty || _width != value;
                _width = value;
            }
        }
        double _width;

        /// <summary>
        /// Gets or sets the line join.
        /// </summary>
        public XLineJoin LineJoin
        {
            get => _lineJoin;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                _dirty = _dirty || _lineJoin != value;
                _lineJoin = value;
            }
        }
        XLineJoin _lineJoin;

        /// <summary>
        /// Gets or sets the line cap.
        /// </summary>
        public XLineCap LineCap
        {
            get => _lineCap;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                _dirty = _dirty || _lineCap != value;
                _lineCap = value;
            }
        }
        XLineCap _lineCap;

        /// <summary>
        /// Gets or sets the miter limit.
        /// </summary>
        public double MiterLimit
        {
            get => _miterLimit;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                _dirty = _dirty || _miterLimit != value;
                _miterLimit = value;
            }
        }
        double _miterLimit;

        /// <summary>
        /// Gets or sets the dash style.
        /// </summary>
        public XDashStyle DashStyle
        {
            get => _dashStyle;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                _dirty = _dirty || _dashStyle != value;
                _dashStyle = value;
            }
        }
        XDashStyle _dashStyle;

        /// <summary>
        /// Gets or sets the dash offset.
        /// </summary>
        public double DashOffset
        {
            get => _dashOffset;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                _dirty = _dirty || _dashOffset != value;
                _dashOffset = value;
            }
        }
        double _dashOffset;

        /// <summary>
        /// Gets or sets the dash pattern.
        /// </summary>
        public double[] DashPattern
        {
            get
            {
                if (_dashPattern == null)
                    _dashPattern = [];
                return _dashPattern;
            }
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));

                int length = value.Length;
                //if (length == 0)
                //  throw new ArgumentException("Dash pattern array must not be empty.");

                for (int idx = 0; idx < length; idx++)
                {
                    if (value[idx] <= 0)
                        throw new ArgumentException("Dash pattern value must greater than zero.");
                }

                _dirty = true;
                _dashStyle = XDashStyle.Custom;
                _dashPattern = (double[])value.Clone();
            }
        }
        internal double[]? _dashPattern;

        /// <summary>
        /// Gets or sets a value indicating whether the pen enables overprint when used in a PDF document.
        /// Experimental, takes effect only on CMYK color mode.
        /// </summary>
        public bool Overprint
        {
            get => _overprint;
            set
            {
                if (_immutable)
                    throw new ArgumentException(PsMsgs.CannotChangeImmutableObject(nameof(XPen)));
                _overprint = value;
            }
        }
        bool _overprint;

#if GDI
        /// <summary>
        /// Implicit conversion from Pen to XPen
        /// </summary>
        public static implicit operator XPen(Pen pen)
        {
            // ReSharper disable once IdentifierTypo
            XPen xpen;
            try
            {
                Lock.EnterGdiPlus();
                xpen = pen.PenType switch
                {
                    PenType.SolidColor => new(pen.Color, pen.Width)
                    {
                        LineJoin = (XLineJoin)pen.LineJoin,
                        DashStyle = (XDashStyle)pen.DashStyle,
                        _miterLimit = pen.MiterLimit
                    },
                    _ => throw new NotImplementedException("Pen type not supported by PDFsharp.")
                };
                // Custom dash style, fix by drice2@ageone.de.
                if (pen.DashStyle == System.Drawing.Drawing2D.DashStyle.Custom)
                {
                    int length = pen.DashPattern.Length;
                    double[] pattern = new double[length];
                    for (int idx = 0; idx < length; idx++)
                        pattern[idx] = pen.DashPattern[idx];
                    xpen.DashPattern = pattern;
                    xpen._dashOffset = pen.DashOffset;
                }
            }
            finally { Lock.ExitGdiPlus(); }
            return xpen;
        }

        internal Pen RealizeGdiPen()
        {
            if (_dirty)
            {
                if (_gdiPen == null!)
                    _gdiPen = new System.Drawing.Pen(_color.ToGdiColor(), (float)_width);
                else
                {
                    _gdiPen.Color = _color.ToGdiColor();
                    _gdiPen.Width = (float)_width;
                }
                var lineCap = XConvert.ToLineCap(_lineCap);
                _gdiPen.StartCap = lineCap;
                _gdiPen.EndCap = lineCap;
                _gdiPen.LineJoin = XConvert.ToLineJoin(_lineJoin);
                _gdiPen.DashOffset = (float)_dashOffset;
                if (_dashStyle == XDashStyle.Custom && _dashPattern != null)
                {
                    int len = /*_dashPattern == null! ? 0 :*/ _dashPattern.Length;
                    float[] pattern = new float[len];
                    for (int idx = 0; idx < len; idx++)
                        pattern[idx] = (float)_dashPattern[idx];
                    _gdiPen.DashPattern = pattern;
                }
                else
                    _gdiPen.DashStyle = (DashStyle)_dashStyle;
            }
            return _gdiPen;
        }
#endif

#if WPF
        internal WpfPen RealizeWpfPen()
        {
#if true
            // XPen is frozen by design, WPF Pen can change.
            // We realize a new pen independent of dirty flag.
            if (_dirty || !_dirty)
            {
                var lineCap = XConvert.ToPenLineCap(_lineCap);

                // Always create a new WpfPen.
                _wpfPen = new WpfPen(new SolidColorBrush(_color.ToWpfColor()), _width)
                {
                    StartLineCap = lineCap,
                    EndLineCap = lineCap,
                    LineJoin = XConvert.ToPenLineJoin(_lineJoin)
                };

                if (_dashStyle == XDashStyle.Custom)
                {
                    // TODOWPF: does not work in all cases
                    _wpfPen.DashStyle = new System.Windows.Media.DashStyle(_dashPattern, _dashOffset);
                }
                else
                {
                    switch (_dashStyle)
                    {
                        case XDashStyle.Solid:
                            _wpfPen.DashStyle = DashStyles.Solid;
                            break;

                        case XDashStyle.Dash:
                            //_wpfPen.DashStyle = DashStyles.Dash;
                            _wpfPen.DashStyle = new System.Windows.Media.DashStyle(new double[] { 2, 2 }, 0);
                            break;

                        case XDashStyle.Dot:
                            //_wpfPen.DashStyle = DashStyles.Dot;
                            _wpfPen.DashStyle = new System.Windows.Media.DashStyle(new double[] { 0, 2 }, 1.5);
                            break;

                        case XDashStyle.DashDot:
                            //_wpfPen.DashStyle = DashStyles.DashDot;
                            _wpfPen.DashStyle = new System.Windows.Media.DashStyle(new double[] { 2, 2, 0, 2 }, 0);
                            break;

                        case XDashStyle.DashDotDot:
                            //_wpfPen.DashStyle = DashStyles.DashDotDot;
                            _wpfPen.DashStyle = new System.Windows.Media.DashStyle(new double[] { 2, 2, 0, 2, 0, 2 }, 0);
                            break;
                    }
                }
            }
#else
            _wpfPen = new System.Windows.Media.Pen();
            _wpfPen.Brush = new SolidColorBrush(_color.ToWpfColor());
            _wpfPen.Thickness = _width;
#endif
            return _wpfPen;
        }
#endif

        bool _dirty = true;
        readonly bool _immutable;
#if GDI
        GdiPen _gdiPen = null!;
#endif
#if WPF
        WpfPen _wpfPen = null!;
#endif
    }
}
