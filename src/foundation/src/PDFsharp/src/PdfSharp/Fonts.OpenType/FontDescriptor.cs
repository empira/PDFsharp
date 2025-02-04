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
using PdfSharp.Drawing;
using PdfSharp.Internal;

#pragma warning disable 0649

namespace PdfSharp.Fonts.OpenType
{
    /// <summary>
    /// Base class for all font descriptors.
    /// Currently only OpenTypeDescriptor is derived from this base class.
    /// </summary>
    class FontDescriptor
    {
        protected FontDescriptor(string key)
        {
            Key = key;
        }

        public string Key { get; }

        /// <summary>
        /// 
        /// </summary>
        public string FontName3 { get; init; } = default!; // #NFM check format of this name. It is the name of the XFont or the name of XGlyphTypeface.

        /// <summary>
        /// 
        /// </summary>
        public string Weight
        {
            get;
            private set;
            // BUG_OLD: never set
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
            init; // BUG_OLD: never set
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
            // BUG_OLD: never set
        } = default!;

        /// <summary>
        /// 
        /// </summary>
        public string EncodingScheme
        {
            get;
            private set;
            // BUG_OLD: never set
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
            // BUG_OLD: never set
        }

        /// <summary>
        /// 
        /// </summary>
        public int StemV { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int LineSpacing { get; protected set; }

        internal static string ComputeFdKey(string name, XFontStyleEx style)
            => ComputeFdKey(name,
                (style & XFontStyleEx.Bold) != 0,
                (style & XFontStyleEx.Italic) != 0);

        internal static string ComputeFdKey(string name, bool isBold, bool isItalic)
        {
            name = name.ToLowerInvariant();
            var key = isBold switch
            {
                false when !isItalic => name + '/',
                true when !isItalic => name + "/b",
                false when isItalic => name + "/i",
                _ => name + "/bi"
            };
            return key;
        }

        internal readonly int GlobalVersion = Globals.Global.Version;
    }
}
