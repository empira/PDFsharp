// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Pdf.Internal;
using PdfSharp.Fonts;
#if !EDF_CORE
using PdfSharp.Drawing;
#endif

#pragma warning disable 0649

namespace PdfSharp.Fonts.OpenType
{
    // TODO: Needs to be refactored #???
    /// <summary>
    /// Base class for all font descriptors.
    /// Currently only OpenTypeDescriptor is derived from this base class.
    /// </summary>
    class FontDescriptor
    {
        protected FontDescriptor(string key)
        {
            _key = key;
        }

        public string Key => _key;

        readonly string _key;

        ///// <summary>
        ///// 
        ///// </summary>
        //public string FontFile
        //{
        //  get { return _fontFile; }
        //  private set { _fontFile = value; }  // BUG: never set
        //}
        //string _fontFile;

        ///// <summary>
        ///// 
        ///// </summary>
        //public string FontType
        //{
        //  get { return _fontType; }
        //  private set { _fontType = value; }  // BUG: never set
        //}
        //string _fontType;

        /// <summary>
        /// 
        /// </summary>
        public string FontName
        {
            get => _fontName;
            protected set => _fontName = value;
        }
        string _fontName = default!;

        ///// <summary>
        ///// 
        ///// </summary>
        //public string FullName
        //{
        //    get { return _fullName; }
        //    private set { _fullName = value; }  // BUG: never set
        //}
        //string _fullName;

        ///// <summary>
        ///// 
        ///// </summary>
        //public string FamilyName
        //{
        //    get { return _familyName; }
        //    private set { _familyName = value; }  // BUG: never set
        //}
        //string _familyName;

        /// <summary>
        /// 
        /// </summary>
        public string Weight
        {
            get;
            private set;
            // BUG: never set
        } = default!;

        /// <summary>
        /// Gets a value indicating whether this instance belongs to a bold font.
        /// </summary>
        public virtual bool IsBoldFace => false;

        /// <summary>
        /// 
        /// </summary>
        public float ItalicAngle { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance belongs to an italic font.
        /// </summary>
        public virtual bool IsItalicFace => false;

        /// <summary>
        /// 
        /// </summary>
        public int XMin { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int YMin { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int XMax { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int YMax { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsFixedPitch
        {
            get;
            private set;
            // BUG: never set
        }

        /// <summary>
        /// 
        /// </summary>
        public int UnderlinePosition { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int UnderlineThickness { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int StrikeoutPosition { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int StrikeoutSize { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public string Version
        {
            get;
            private set;
            // BUG: never set
        } = default!;

        ///// <summary>
        ///// 
        ///// </summary>
        //public string Notice
        //{
        //  get { return Notice; }
        //}
        //protected string notice;

        /// <summary>
        /// 
        /// </summary>
        public string EncodingScheme
        {
            get;
            private set;
            // BUG: never set
        } = default!;

        /// <summary>
        /// 
        /// </summary>
        public int UnitsPerEm { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int CapHeight { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int XHeight { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int Ascender { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int Descender { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int Leading { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int Flags
        {
            get;
            private set;
            // BUG: never set
        }

        /// <summary>
        /// 
        /// </summary>
        public int StemV { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int LineSpacing { get; protected set; }
        
        internal static string ComputeKey(XFont font)
        {
            return font.GlyphTypeface.Key;
            //return ComputeKey(font.GlyphTypeface.Fontface.FullFaceName, font.Style);
            //XGlyphTypeface glyphTypeface = font.GlyphTypeface;
            //string key = glyphTypeface.Fontface.FullFaceName.ToLowerInvariant() +
            //    (glyphTypeface.IsBold ? "/b" : "") + (glyphTypeface.IsItalic ? "/i" : "");
            //return key;
        }

        internal static string ComputeKey(string name, XFontStyleEx style)
        {
            return ComputeKey(name,
                (style & XFontStyleEx.Bold) == XFontStyleEx.Bold,
                (style & XFontStyleEx.Italic) == XFontStyleEx.Italic);
        }

        internal static string ComputeKey(string name, bool isBold, bool isItalic)
        {
#if true
            // Attempt to make it faster.
            if (!isBold && !isItalic)
                return name.ToLowerInvariant() + '/';
            else if (isBold && !isItalic)
                return name.ToLowerInvariant() + "/b";
            else if (!isBold && isItalic)
                return name.ToLowerInvariant() + "/i";
            else /*if (isBold && isItalic)*/
                return name.ToLowerInvariant() + "/bi";
#else
            // TODO StringBuilder?
            string key = name.ToLowerInvariant() + '/'
                + (isBold ? "b" : "") + (isItalic ? "i" : "");
            return key;
#endif
        }

        internal static string ComputeKey(string name)
        {
            string key = name.ToLowerInvariant();
            return key;
        }
    }
}
