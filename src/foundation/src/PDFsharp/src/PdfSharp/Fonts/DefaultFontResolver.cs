// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
//using System.Drawing;
//using System.Drawing.Drawing2D;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
#endif
#if WPF
//using System.Windows;
//using System.Windows.Documents;
//using System.Windows.Media;
using System.IO;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
using WpfStyleSimulations = System.Windows.Media.StyleSimulations;
#endif
using PdfSharp.Drawing;
using PdfSharp.Fonts.Internal;
using System.Runtime.InteropServices;

namespace PdfSharp.Fonts
{
    //public class DefaultFontResolver : IFontResolver
    //{
    //    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    //    {
    //        var info = PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    //        return info;
    //    }

    //    public byte[] GetFont(string faceName)
    //    {
    //        var source = FontFactory.GetFontSourceByTypefaceKey(faceName);
    //        return source.Bytes;
    //    }
    //}
}
