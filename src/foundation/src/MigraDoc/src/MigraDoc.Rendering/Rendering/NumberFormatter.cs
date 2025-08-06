// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using Microsoft.Extensions.Logging;
using MigraDoc.Logging;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Formats numbers roman or with letters.
    /// </summary>
    class NumberFormatter
    {
        internal static string Format(int number, string format)
        {
            return format switch
            {
                "ROMAN" => AsRoman(number, false),
                "roman" => AsRoman(number, true),
                "ALPHABETIC" => AsLetters(number, false),
                "alphabetic" => AsLetters(number, true),
                _ => number.ToString(CultureInfo.InvariantCulture)
            };
        }

        static string AsRoman(int number, bool lowercase)
        {
            if (Math.Abs(number) > 32768)
            {
                MigraDocLogHost.PdfRenderingLogger.LogWarning(MdPdfMsgs.NumberTooLargeForRoman(number).Message);
                //Debug.WriteLine(Messages2.NumberTooLargeForRoman(number), "warning");
                return number.ToString(CultureInfo.InvariantCulture);
            }
            if (number == 0)
                return "0";

            string res = "";
            if (number < 0)
                res += "-";

            number = Math.Abs(number);

            string[] roman;
            if (lowercase)
                roman = ["m", "cm", "d", "cd", "c", "xc", "l", "xl", "x", "ix", "v", "iv", "i"];
            else
                roman = ["M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I"];

            int[] numberValues = [1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1];

            for (int idx = 0; idx < numberValues.Length; idx++)
            {
                while (number >= numberValues[idx])
                {
                    res += roman[idx];
                    number -= numberValues[idx];
                }
            }
            return res;
        }

        static string AsLetters(int number, bool lowercase)
        {
            if (Math.Abs(number) > 32768)
            {
                MigraDocLogHost.PdfRenderingLogger.LogWarning(MdPdfMsgs.NumberTooLargeForLetters(number).Message);
                //Debug.WriteLine(Messages2.NumberTooLargeForLetters(number));
                return number.ToString();
            }

            if (number == 0)
                return "0";

            string str = "";
            if (number < 0)
                str += "-";

            number = Math.Abs(number);
            char cr;
            if (lowercase)
                cr = (char)('a' + (number - 1) % 26);
            else
                cr = (char)('A' + (number - 1) % 26);

            for (int n = 0; n <= (number - 1) / 26; ++n)
                str += cr;

            return str;
        }
    }
}
