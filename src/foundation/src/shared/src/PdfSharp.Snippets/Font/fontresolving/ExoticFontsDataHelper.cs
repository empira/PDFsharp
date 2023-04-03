// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if WPF
using System.Windows;
using System.Windows.Resources;
#endif
using PdfSharp.Quality;

// ReSharper disable InconsistentNaming

namespace PdfSharp.Snippets.Font
{
    public static class ExoticFontsDataHelper
    {
        public static byte[] Oblivious
        {
            get
            {
//#if NET/FX_CORE
//                // Embedded resource.
//                return FontHelper.LoadFontData("PdfSharp.Features.Fonts.oblivious.ttf", typeof(ExoticFontsDataHelper).GetTypeInfo().Assembly);
//#elif WPF
//                // Resource with Pack URI.
//                return FontHelper.LoadFontDataPack(new Uri("pack://application:,,,/Fonts/oblivious.ttf "));
//#else
//                // Embedded resource.
//                return FontHelper.LoadFontData("PdfSharp.Snippets.assets.fonts.oblivious.ttf", Assembly.GetExecutingAssembly());
//#endif
                return FontHelper.LoadFontData("PdfSharp.Snippets.assets.fonts.Oblivious.ttf", Assembly.GetExecutingAssembly());
            }
        }

        public static byte[] XFiles
        {
            get
            {
//#if NET/FX_CORE
//                // Embedded resource.
//                return FontHelper.LoadFontData("PdfSharp.Features.Fonts.xfiles.ttf", typeof(ExoticFontsDataHelper).GetTypeInfo().Assembly);
//#elif WPF
//                // Resource with Pack URI.
//                return FontHelper.LoadFontDataPack(new Uri("pack://application:,,,/Fonts/xfiles.ttf "));
//#else
//                // Embedded resource.
//                return FontHelper.LoadFontData("PdfSharp.Snippets.assets.fonts.xfiles.ttf", Assembly.GetExecutingAssembly());
//#endif
                return FontHelper.LoadFontData("PdfSharp.Snippets.assets.fonts.xfiles.ttf", Assembly.GetExecutingAssembly());

            }
        }
    }
}
