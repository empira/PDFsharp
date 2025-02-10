// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if false  // DELETE 2025-12-31
#if GDI
using System.Runtime.InteropServices;
using PdfSharp.Logging;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;
using GdiPrivateFontCollection = System.Drawing.Text.PrivateFontCollection;
#endif
#if WPF
using WpfFonts = System.Windows.Media.Fonts;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfTypeface = System.Windows.Media.Typeface;
using WpfGlyphTypeface = System.Windows.Media.GlyphTypeface;
#endif
using Microsoft.Extensions.Logging;
using PdfSharp.Fonts;
using PdfSharp.Fonts.Internal;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// This class is out of order. Use a font resolver instead.
    /// </summary>
    [Obsolete("XPrivateFontCollection is out of order now. Use a font resolver instead.")]
    public sealed class XPrivateFontCollection
    { }
}
#endif
