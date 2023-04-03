// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Text;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Drawing;

namespace PdfSharp.Fonts.Internal
{
    /// <summary>
    /// Parameters that affect font selection.
    /// </summary>
    class FontResolvingOptions
    {
        public FontResolvingOptions(XFontStyleEx fontStyle)
        {
            FontStyle = fontStyle;
            Style = (fontStyle & XFontStyleEx.Italic) != 0 ? XFontStyles.Italic : XFontStyles.Normal;
            Weight = (fontStyle & XFontStyleEx.Bold) != 0 ? XFontWeights.Bold : XFontWeights.Normal;
            Stretch = XFontStretches.Normal;
        }

        public FontResolvingOptions(XFontStyleEx fontStyle, XStyleSimulations styleSimulations) : this(fontStyle)
        {
            OverrideStyleSimulations = true;
            StyleSimulations = styleSimulations;
        }

        public FontResolvingOptions(XFontStyle style, XFontWeight weight, XFontStretch stretch, XStyleSimulations? styleSimulations = null)
        {
            FontStyle = (style == XFontStyles.Italic || style == XFontStyles.Oblique ? XFontStyleEx.Italic : 0) | (weight >= XFontWeights.Bold ? XFontStyleEx.Bold : 0);
            Style = style;
            Weight = weight;
            Stretch = stretch;
            OverrideStyleSimulations = styleSimulations is not null;
            StyleSimulations = styleSimulations ?? XStyleSimulations.None;
        }

        public XFontStyle Style { get; }

        public XFontWeight Weight { get; }

        public XFontStretch Stretch { get; }

        //public bool IsBold => (FontStyle & XFontStyleEx.Bold) == XFontStyleEx.Bold;
        public bool IsBold => Weight >= XFontWeights.Bold;

        //public bool IsItalic => (FontStyle & XFontStyleEx.Italic) == XFontStyleEx.Italic;
        public bool IsItalic => Style == XFontStyles.Italic | Style == XFontStyles.Oblique;

        public bool IsBoldItalic => (FontStyle & XFontStyleEx.BoldItalic) == XFontStyleEx.BoldItalic;

        public bool MustSimulateBold => (StyleSimulations & XStyleSimulations.BoldSimulation) == XStyleSimulations.BoldSimulation;

        public bool MustSimulateItalic => (StyleSimulations & XStyleSimulations.ItalicSimulation) == XStyleSimulations.ItalicSimulation;

        public XFontStyleEx FontStyle { get; }

        public bool OverrideStyleSimulations { get; }

        public XStyleSimulations StyleSimulations { get; }
    }
}
