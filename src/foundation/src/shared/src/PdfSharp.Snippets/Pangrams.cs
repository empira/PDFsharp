// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Quality;

namespace PdfSharp.Snippets
{
    public class Pangrams
    {
        public static string QuickBrownFox = "The quick brown fox jumps over the lazy dog. 1234567890 +-*/";
        public static string SphinxOfQuarz = "Sphinx of black quartz, judge my vow. 1234567890 +-*/";
        public static string TwelveBoxer = "Zwölf Boxkämpfer jagen Viktor quer über den großen Sylter Deich. 1234567890 +-*/";
    }

    public class xxxxxxxxxx
    {
        public static string Arabic = "نص حكيم له سر قاطع وذو شأن عظيم مكتوب على ثوب أخضر ومغلف بجلد أزرق";
        public static string Arabic2 = "يقفز الثعلب البني السريع فوق الكسول. 1234567890 +-*/";
    }
}
