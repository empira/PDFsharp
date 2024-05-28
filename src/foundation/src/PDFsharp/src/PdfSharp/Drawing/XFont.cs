// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// #??? Clean up

using System.ComponentModel;
#if GDI
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
#endif
#if WPF
using System.Windows.Markup;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
#endif
#if UWP
using UwpFontFamily = Windows.UI.Xaml.Media.FontFamily;
#endif
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

// ReSharper disable ConvertToAutoProperty

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Defines an object used to draw text.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class XFont
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="emSize">The em size.</param>
        public XFont(string familyName, double emSize)
            : this(familyName, emSize, XFontStyleEx.Regular, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        public XFont(string familyName, double emSize, XFontStyleEx style)
            : this(familyName, emSize, style, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(string familyName, double emSize, XFontStyleEx style, XPdfFontOptions pdfOptions)
        {
            _familyName = familyName;
            _emSize = emSize;
            _style = style;
            _pdfOptions = pdfOptions;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class with enforced style simulation.
        /// Only for testing PDFsharp.
        /// </summary>
        internal XFont(string familyName, double emSize, XFontStyleEx style, XPdfFontOptions pdfOptions, XStyleSimulations styleSimulations)
        {
            _familyName = familyName;
            _emSize = emSize;
            _style = style;
            _pdfOptions = pdfOptions;
            OverrideStyleSimulations = true;
            StyleSimulations = styleSimulations;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// Not yet implemented.
        /// </summary>
        /// <param name="familyName">Name of the family.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The style.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="fontStretch">The font stretch.</param>
        /// <param name="pdfOptions">The PDF options.</param>
        /// <param name="styleSimulations">The style simulations.</param>
        /// <exception cref="System.NotImplementedException">XFont</exception>
        [Obsolete("Not yet implemented.")]
        public XFont(string familyName, double emSize, XFontStyle style, XFontWeight weight, XFontStretch fontStretch,
            XPdfFontOptions? pdfOptions = null, XStyleSimulations? styleSimulations = null)
        {
            throw new NotImplementedException(nameof(XFont));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// Not yet implemented.
        /// </summary>
        /// <param name="typeface">The typeface.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="pdfOptions">The PDF options.</param>
        /// <param name="styleSimulations">The style simulations.</param>
        /// <exception cref="System.NotImplementedException">XFont</exception>
        [Obsolete("Not yet implemented.")]
        public XFont(XTypeface typeface, double emSize, XPdfFontOptions? pdfOptions = null, XStyleSimulations? styleSimulations = null)
        {
            throw new NotImplementedException(nameof(XFont));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class.
        /// Not yet implemented.
        /// </summary>
        /// <param name="glyphTypeface">The typeface.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="pdfOptions">The PDF options.</param>
        /// <param name="styleSimulations">The style simulations.</param>
        public XFont(XGlyphTypeface glyphTypeface, double emSize, XPdfFontOptions? pdfOptions = null, XStyleSimulations? styleSimulations = null)
        {
            GlyphTypeface = glyphTypeface;
            _familyName = glyphTypeface.FamilyName;
            _emSize = emSize;
            _style = (glyphTypeface.IsBold ? XFontStyleEx.Bold : XFontStyleEx.Regular) |
                     (glyphTypeface.IsItalic ? XFontStyleEx.Italic : XFontStyleEx.Regular);
            _pdfOptions = pdfOptions ?? new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding);
            OverrideStyleSimulations = styleSimulations != null && styleSimulations != XStyleSimulations.None;
            StyleSimulations = styleSimulations ?? XStyleSimulations.None;
            Initialize();
        }
#if GDI
        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Drawing.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        public XFont(GdiFontFamily fontFamily, double emSize, XFontStyleEx style)
            : this(fontFamily, emSize, style, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Drawing.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(GdiFontFamily fontFamily, double emSize, XFontStyleEx style, XPdfFontOptions pdfOptions)
        {
            _familyName = fontFamily.Name;
            GdiFontFamily = fontFamily;
            _emSize = emSize;
            _style = style;
            _pdfOptions = pdfOptions;
            InitializeFromGdi();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.Font.
        /// </summary>
        /// <param name="font">The System.Drawing.Font.</param>
        public XFont(GdiFont font)
            : this(font, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.Font.
        /// </summary>
        /// <param name="font">The System.Drawing.Font.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(GdiFont font, XPdfFontOptions pdfOptions)
        {
            if (font.Unit != GraphicsUnit.World)
                throw new ArgumentException("Font must use GraphicsUnit.World.");
            GdiFont = font;
            Debug.Assert(font.Name == font.FontFamily.Name);
            _familyName = font.Name;
            _emSize = font.Size;
            _style = FontStyleFrom(font);
            _pdfOptions = pdfOptions;
            InitializeFromGdi();
        }
#endif

#if WPF
        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Windows.Media.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Windows.Media.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        public XFont(WpfFontFamily fontFamily, double emSize, XFontStyleEx style)
            : this(fontFamily, emSize, style, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Windows.Media.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(WpfFontFamily fontFamily, double emSize, XFontStyleEx style, XPdfFontOptions pdfOptions)
        {
            _familyName = fontFamily.FamilyNames[XmlLanguage.GetLanguage("en-US")];
            WpfFontFamily = fontFamily;
            _emSize = emSize;
            _style = style;
            _pdfOptions = pdfOptions;
            InitializeFromWpf();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont" /> class from a System.Windows.Media.Typeface.
        /// </summary>
        /// <param name="typeface">The System.Windows.Media.Typeface.</param>
        /// <param name="emSize">The em size.</param>
        public XFont(WpfTypeface typeface, double emSize)
            : this(typeface, emSize, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Windows.Media.Typeface.
        /// </summary>
        /// <param name="typeface">The System.Windows.Media.Typeface.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(WpfTypeface typeface, double emSize, XPdfFontOptions pdfOptions)
        {
            WpfTypeface = typeface;
            //Debug.Assert(font.Name == font.FontFamily.Name);
            //_familyName = font.Name;
            _emSize = emSize;
            _pdfOptions = pdfOptions;
            InitializeFromWpf();
        }
#endif

#if UWP_
        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Drawing.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        public XFont(UwpFontFamily fontFamily, double emSize, XFontStyleEx style)
            : this(fontFamily, emSize, style, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.FontFamily.
        /// </summary>
        /// <param name="fontFamily">The System.Drawing.FontFamily.</param>
        /// <param name="emSize">The em size.</param>
        /// <param name="style">The font style.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(UwpFontFamily fontFamily, double emSize, XFontStyleEx style, XPdfFontOptions pdfOptions)
        {
            _familyName = fontFamily.Source;
            _gdiFontFamily = fontFamily;
            _emSize = emSize;
            _style = style;
            _pdfOptions = pdfOptions;
            InitializeFromGdi();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.Font.
        /// </summary>
        /// <param name="font">The System.Drawing.Font.</param>
        public XFont(GdiFont font)
            : this(font, new XPdfFontOptions(GlobalFontSettings.DefaultFontEncoding))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFont"/> class from a System.Drawing.Font.
        /// </summary>
        /// <param name="font">The System.Drawing.Font.</param>
        /// <param name="pdfOptions">Additional PDF options.</param>
        public XFont(GdiFont font, XPdfFontOptions pdfOptions)
        {
            if (font.Unit != GraphicsUnit.World)
                throw new ArgumentException("Font must use GraphicsUnit.World.");
            _gdiFont = font;
            Debug.Assert(font.Name == font.FontFamily.Name);
            _familyName = font.Name;
            _emSize = font.Size;
            _style = FontStyleFrom(font);
            _pdfOptions = pdfOptions;
            InitializeFromGdi();
        }
#endif

        //// Methods
        //public Font(Font prototype, FontStyle newStyle);
        //public Font(FontFamily family, float emSize);
        //public Font(string familyName, float emSize);
        //public Font(FontFamily family, float emSize, FontStyle style);
        //public Font(FontFamily family, float emSize, GraphicsUnit unit);
        //public Font(string familyName, float emSize, FontStyle style);
        //public Font(string familyName, float emSize, GraphicsUnit unit);
        //public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit);
        //public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit);
        ////public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet);
        ////public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet);
        ////public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont);
        ////public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont);
        //public object Clone();
        //static FontFamily CreateFontFamilyWithFallback(string familyName);
        //private void Dispose(bool disposing);
        //public override bool Equals(object? obj);
        //protected override void Finalize();
        //public static Font FromHdc(IntPtr hdc);
        //public static Font FromHfont(IntPtr hfont);
        //public static Font FromLogFont(object lf);
        //public static Font FromLogFont(object lf, IntPtr hdc);
        //public override int GetHashCode();

        /// <summary>
        /// Initializes this instance by computing the glyph typeface, font family, font source and TrueType font face.
        /// (PDFsharp currently only deals with TrueType fonts.)
        /// </summary>
        void Initialize()
        {
#if DEBUG_
            if (_familyName == "Segoe UI Semilight" && (_style & XFontStyleEx.BoldItalic) == XFontStyleEx.Italic)
                _ = typeof(int);
#endif
            FontResolvingOptions fontResolvingOptions = OverrideStyleSimulations
                ? new FontResolvingOptions(_style, StyleSimulations)
                : new FontResolvingOptions(_style);

            if (GlyphTypeface == null!)
            {
                // In principle an XFont is an XGlyphTypeface plus an em-size.
                GlyphTypeface = XGlyphTypeface.GetOrCreateFrom(_familyName, fontResolvingOptions);

                GlyphTypeface.FontFace.SetFontEmbedding(_pdfOptions.FontEmbedding);
            }
#if GDI
            // Create font by using font family.
            var gdiFont = FontHelper.CreateFont(_familyName, (float)_emSize, (GdiFontStyle)(_style & XFontStyleEx.BoldItalic), out _);
            // Should not fail because GDI font is created by other GDI font.   
            // No, fails if CFR creates the font.
            //Debug.Assert(gdiFont!=null);
            GdiFont = gdiFont!;
#endif
#if WPF
            WpfFontFamily = GlyphTypeface.FontFamily.WpfFamily;
            WpfTypeface = GlyphTypeface.WpfTypeface;

            WpfFontFamily ??= new WpfFontFamily(Name);

            WpfTypeface ??= FontHelper.CreateTypeface(WpfFontFamily ?? NRT.ThrowOnNull<WpfFontFamily>(), _style);
#endif
            CreateDescriptorAndInitializeFontMetrics();
        }

#if GDI
        /// <summary>
        /// A GDI+ font object is used to setup the internal font objects.
        /// </summary>
        void InitializeFromGdi()
        {
            try
            {
                Lock.EnterFontFactory();
                if (GdiFontFamily != null!)
                {
                    // Create font based on its family.
                    GdiFont = new Font(GdiFontFamily, (float)_emSize, (GdiFontStyle)_style, GraphicsUnit.World);
                }

                if (GdiFont != null!)
                {
#if DEBUG_
                    string name1 = _gdiFont.Name;
                    string name2 = _gdiFont.OriginalFontName;
                    string name3 = _gdiFont.SystemFontName;
#endif
                    _familyName = GdiFont.FontFamily.Name;
                    // TODO: _glyphTypeface = XGlyphTypeface.GetOrCreateFrom(_gdiFont);
                }
                else
                {
                    Debug.Assert(false);
                }

                if (GlyphTypeface == null!)
                    GlyphTypeface = XGlyphTypeface.GetOrCreateFromGdi(GdiFont);

                CreateDescriptorAndInitializeFontMetrics();
            }
            finally { Lock.ExitFontFactory(); }
        }
#endif

#if WPF
        void InitializeFromWpf()
        {
            if (WpfFontFamily != null)
            {
                WpfTypeface = FontHelper.CreateTypeface(WpfFontFamily, _style);
            }

            if (WpfTypeface != null)
            {
                _familyName = WpfTypeface.FontFamily.FamilyNames[XmlLanguage.GetLanguage("en-US")];
                GlyphTypeface = XGlyphTypeface.GetOrCreateFromWpf(WpfTypeface) ?? NRT.ThrowOnNull<XGlyphTypeface>();
            }
            else
            {
                Debug.Assert(false);
            }

            if (GlyphTypeface == null!)
                GlyphTypeface = XGlyphTypeface.GetOrCreateFrom(_familyName, new FontResolvingOptions(_style));

            CreateDescriptorAndInitializeFontMetrics();
        }
#endif

        /// <summary>
        /// Code separated from Metric getter to make code easier to debug.
        /// (Setup properties in their getters caused side effects during debugging because Visual Studio calls a getter
        /// too early to show its value in a debugger window.)
        /// </summary>
        void CreateDescriptorAndInitializeFontMetrics()  // TODO: refactor
        {
            Debug.Assert(_fontMetrics == null, "InitializeFontMetrics() was already called.");
            OpenTypeDescriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptorFor(this); //_familyName, _style, _glyphTypeface.FontFace);
            _fontMetrics = new XFontMetrics(OpenTypeDescriptor.FontName2, OpenTypeDescriptor.UnitsPerEm, OpenTypeDescriptor.Ascender, OpenTypeDescriptor.Descender,
                OpenTypeDescriptor.Leading, OpenTypeDescriptor.LineSpacing, OpenTypeDescriptor.CapHeight, OpenTypeDescriptor.XHeight, OpenTypeDescriptor.StemV, 0, 0, 0,
                OpenTypeDescriptor.UnderlinePosition, OpenTypeDescriptor.UnderlineThickness, OpenTypeDescriptor.StrikeoutPosition, OpenTypeDescriptor.StrikeoutSize);

            XFontMetrics fm = Metrics;

            // Already done in CreateDescriptorAndInitializeFontMetrics.
            //if (_descriptor == null)
            //    _descriptor = (OpenTypeDescriptor)FontDescriptorStock.Global.CreateDescriptor(this);  //(Name, (XGdiFontStyle)Font.Style);

            UnitsPerEm = OpenTypeDescriptor.UnitsPerEm;
            CellAscent = OpenTypeDescriptor.Ascender;
            CellDescent = OpenTypeDescriptor.Descender;
            CellSpace = OpenTypeDescriptor.LineSpacing;

#if DEBUG_ && GDI
            int gdiValueUnitsPerEm = Font.FontFamily.GetEmHeight(Font.Style);
            Debug.Assert(gdiValueUnitsPerEm == UnitsPerEm);
            int gdiValueAscent = Font.FontFamily.GetCellAscent(Font.Style);
            Debug.Assert(gdiValueAscent == CellAscent);
            int gdiValueDescent = Font.FontFamily.GetCellDescent(Font.Style);
            Debug.Assert(gdiValueDescent == CellDescent);
            int gdiValueLineSpacing = Font.FontFamily.GetLineSpacing(Font.Style);
            Debug.Assert(gdiValueLineSpacing == CellSpace);
#endif
#if DEBUG_ && WPF
            int wpfValueLineSpacing = (int)Math.Round(Family.LineSpacing * _descriptor.UnitsPerEm);
            Debug.Assert(wpfValueLineSpacing == CellSpace);
#endif
            Debug.Assert(fm.UnitsPerEm == OpenTypeDescriptor.UnitsPerEm);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the XFontFamily object associated with this XFont object.
        /// </summary>
        [Browsable(false)]
        public XFontFamily FontFamily => GlyphTypeface.FontFamily;

        // TODO XFont.Name
        /// <summary>
        /// WRONG: Gets the face name of this Font object.
        /// Indeed, it returns the font family name.
        /// </summary>
        // [Obsolete("This function returns the font family name, not the face name. Use xxx.FontFamily.Name or xxx.FaceName")]
        public string Name => GlyphTypeface.FontFamily.Name;

        internal string FaceName => GlyphTypeface.FaceName;

        /// <summary>
        /// Gets the em-size of this font measured in the unit of this font object.
        /// </summary>
        public double Size => _emSize;

        readonly double _emSize;

        /// <summary>
        /// Gets style information for this Font object.
        /// </summary>
        [Browsable(false)]
        public XFontStyleEx Style => _style;

        readonly XFontStyleEx _style;

        /// <summary>
        /// Indicates whether this XFont object is bold.
        /// </summary>
        public bool Bold => (_style & XFontStyleEx.Bold) == XFontStyleEx.Bold;

        /// <summary>
        /// Indicates whether this XFont object is italic.
        /// </summary>
        public bool Italic => (_style & XFontStyleEx.Italic) == XFontStyleEx.Italic;

        /// <summary>
        /// Indicates whether this XFont object is stroke out.
        /// </summary>
        public bool Strikeout => (_style & XFontStyleEx.Strikeout) == XFontStyleEx.Strikeout;

        /// <summary>
        /// Indicates whether this XFont object is underlined.
        /// </summary>
        public bool Underline => (_style & XFontStyleEx.Underline) == XFontStyleEx.Underline;

        /// <summary>
        /// Temporary HACK for XPS to PDF converter.
        /// </summary>
        internal bool IsVertical
        {
            get => _isVertical;
            set => _isVertical = value;
        }
        bool _isVertical;

        /// <summary>
        /// Indicates whether this XFont object is a symbol font.
        /// </summary>
        public bool IsSymbolFont => OpenTypeDescriptor.IsSymbolFont;

        /// <summary>
        /// Gets the PDF options of the font.
        /// </summary>
        public XPdfFontOptions PdfOptions => _pdfOptions ??= new();

        XPdfFontOptions _pdfOptions;

        /// <summary>
        /// Indicates whether this XFont is encoded as Unicode.
        /// Gets a value indicating whether text drawn with this font uses Unicode / CID encoding in the PDF document.
        /// </summary>
        //internal bool Unicode => _pdfOptions != null && _pdfOptions.FontEncoding == PdfFontEncoding.Unicode;
        internal bool UnicodeEncoding => _pdfOptions is { FontEncoding: PdfFontEncoding.Unicode };

        /// <summary>
        /// Gets a value indicating whether text drawn with this font uses ANSI encoding in the PDF document.
        /// </summary>
        internal bool AnsiEncoding => _pdfOptions is { FontEncoding: PdfFontEncoding.WinAnsi };

        /// <summary>
        /// Gets a value indicating whether the font encoding is determined from the characters used in the text.
        /// </summary>
        internal bool AutoEncoding => _pdfOptions is { FontEncoding: PdfFontEncoding.Automatic };

        internal FontType FontTypeFromUnicodeFlag =>
            UnicodeEncoding ? FontType.Type0Unicode
            : AnsiEncoding ? FontType.TrueTypeWinAnsi
            : throw new InvalidOperationException("Font type must be Unicode or WinAnsi encoding.");

        /// <summary>
        /// Gets the cell space for the font. The CellSpace is the line spacing, the sum of CellAscent and CellDescent and optionally some extra space.
        /// </summary>
        public int CellSpace
        {
            get => _cellSpace;
            internal set => _cellSpace = value;
        }
        int _cellSpace;

        /// <summary>
        /// Gets the cell ascent, the area above the base line that is used by the font.
        /// </summary>
        public int CellAscent { get; internal set; }

        /// <summary>
        /// Gets the cell descent, the area below the base line that is used by the font.
        /// </summary>
        public int CellDescent { get; internal set; }

        /// <summary>
        /// Gets the font metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public XFontMetrics Metrics
        {
            get
            {
                // Code moved to InitializeFontMetrics().
                //if (_fontMetrics == null)
                //{
                //    FontDescriptor descriptor = FontDescriptorStock.Global.CreateDescriptor(this);
                //    _fontMetrics = new XFontMetrics(descriptor.FontName, descriptor.UnitsPerEm, descriptor.Ascender, descriptor.Descender,
                //        descriptor.Leading, descriptor.LineSpacing, descriptor.CapHeight, descriptor.XHeight, descriptor.StemV, 0, 0, 0);
                //}
                Debug.Assert(_fontMetrics != null, "InitializeFontMetrics() not yet called.");
                return _fontMetrics;
            }
        }

        XFontMetrics _fontMetrics = default!;

        /// <summary>
        /// Returns the line spacing, in pixels, of this font. The line spacing is the vertical distance
        /// between the base lines of two consecutive lines of text. Thus, the line spacing includes the
        /// blank space between lines along with the height of the character itself.
        /// </summary>
        public double GetHeight()
        {
            double value = CellSpace * _emSize / UnitsPerEm;
#if CORE || UWP
            return value;
#endif
#if GDI && !WPF
#if DEBUG_
            double gdiValue = Font.GetHeight();
            Debug.Assert(DoubleUtil.AreRoughlyEqual(gdiValue, value, 5));
#endif
            return value;
#endif
#if WPF && !GDI
            return value;
#endif
#if WPF && GDI  // Testing only
            return value;
#endif
        }

        /// <summary>
        /// Returns the line spacing, in the current unit of a specified Graphics object, of this font.
        /// The line spacing is the vertical distance between the base lines of two consecutive lines of
        /// text. Thus, the line spacing includes the blank space between lines along with the height of
        /// </summary>
        [Obsolete("Use GetHeight() without parameter.")]
        public double GetHeight(XGraphics graphics)
        {
#if true
            throw new InvalidOperationException("Honestly: Use GetHeight() without parameter!");
#else
#if CORE || UWP
            double value = CellSpace * _emSize / UnitsPerEm;
            return value;
#endif
#if GDI && !WPF
            if (graphics._gfx != null)  // #MediumTrust
            {
                double value = Font.GetHeight(graphics._gfx);
                Debug.Assert(value == Font.GetHeight(graphics._gfx.DpiY));
                double value2 = CellSpace * _emSize / UnitsPerEm;
                Debug.Assert(value - value2 < 1e-3, "??");
                return Font.GetHeight(graphics._gfx);
            }
            return CellSpace * _emSize / UnitsPerEm;
#endif
#if WPF && !GDI
            double value = CellSpace * _emSize / UnitsPerEm;
            return value;
#endif
#if GDI && WPF  // Testing only
            if (graphics.TargetContext == XGraphicTargetContext.GDI)
            {
#if DEBUG
                double value = Font.GetHeight(graphics._gfx);

                // 2355*(0.3/2048)*96 = 33.11719 
                double myValue = CellSpace * (_emSize / (96 * UnitsPerEm)) * 96;
                myValue = CellSpace * _emSize / UnitsPerEm;
                //Debug.Assert(value == myValue, "??");
                //Debug.Assert(value - myValue < 1e-3, "??");
#endif
                return Font.GetHeight(graphics._gfx);
            }

            if (graphics.TargetContext == XGraphicTargetContext.WPF)
            {
                double value = CellSpace * _emSize / UnitsPerEm;
                return value;
            }
            // ReSharper disable HeuristicUnreachableCode
            Debug.Fail("Either GDI or WPF.");
            return 0;
            // ReSharper restore HeuristicUnreachableCode
#endif
#endif
        }

        /// <summary>
        /// Gets the line spacing of this font.
        /// </summary>
        [Browsable(false)]
        public int Height => (int)Math.Ceiling(GetHeight());

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal XGlyphTypeface GlyphTypeface { get; private set; } = default!;

        internal OpenTypeDescriptor OpenTypeDescriptor { get; private set; } = default!;

        internal string FamilyName => _familyName;

        string _familyName = "";

        internal int UnitsPerEm { get; private set; }

        /// <summary>
        /// Override style simulations by using the value of StyleSimulations.
        /// </summary>
        internal bool OverrideStyleSimulations { get; set; }

        /// <summary>
        /// Used to enforce style simulations by renderer. For development purposes only.
        /// </summary>
        internal XStyleSimulations StyleSimulations { get; set; }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if GDI
        /// <summary>
        /// Gets the GDI family.
        /// </summary>
        /// <value>The GDI family.</value>
        public GdiFontFamily GdiFontFamily { get; } = default!;

        internal GdiFont GdiFont { get; private set; } = default!;

        internal static XFontStyleEx FontStyleFrom(GdiFont font)
        {
            return
              (font.Bold ? XFontStyleEx.Bold : 0) |
              (font.Italic ? XFontStyleEx.Italic : 0) |
              (font.Strikeout ? XFontStyleEx.Strikeout : 0) |
              (font.Underline ? XFontStyleEx.Underline : 0);
        }

        /// <summary>
        /// Implicit conversion form Font to XFont
        /// </summary>
        public static implicit operator XFont(GdiFont font) => new(font);
#endif

#if WPF
        /// <summary>
        /// Gets the WPF font family.
        /// Can be null.
        /// </summary>
        internal WpfFontFamily? WpfFontFamily { get; private set; }

        internal WpfTypeface? WpfTypeface { get; private set; }
#endif

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cache PdfFontTable.FontSelector to speed up finding the right PdfFont
        /// if this font is used more than once.
        /// </summary>
        internal string? PdfFontSelector { get; set; }

        internal void CheckVersion() => GlyphTypeface.CheckVersion();

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
        {
            get
            {
                return Invariant($"font=('{Name}' {Size:0.##}{(Bold ? " bold" : "")}{(Italic ? " italic" : "")} {GlyphTypeface.StyleSimulations})");
            }
        }
    }
}
