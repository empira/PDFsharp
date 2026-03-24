// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using System.IO;
//using System.Threading.Tasks;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;
//using PdfSharp.Quality;

// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming

namespace PdfSharp.Snippets.Fonts.Text
{
    public static class ShortTestTexts
    {
        public static readonly string GermanUmlautsAndEszett = "ÄÖÜäöüß";

        public static readonly string GoodMorning_English = "Good morning";
        public static readonly string GoodMorning_German = "Guten Morgen";
        public static readonly string GoodMorning_French = "Bonjour";
        public static readonly string GoodMorning_Italian = "Buongiorno";
        public static readonly string GoodMorning_Turkish = "Günaydın";

        // Non-western glyphs
        public static readonly string GoodMorning_Chinese = "早安";
        public static readonly string GoodMorning_SimplifiedChinese = "早上好";
        public static readonly string GoodMorning_Korean = "좋은 아침이에요";
        public static readonly string GoodMorning_Japanese = "おはよう";
        public static readonly string GoodMorning_Malayalam = "സുപ്രഭാതം";
        public static readonly string GoodMorning_Telugu = "శుభోదయం";
        public static readonly string GoodMorning_Thai = "สวัสดีตอนเช้า";

        // Right to left
        public static readonly string GoodMorning_Arabic = "صباح الخير";
    }
}
