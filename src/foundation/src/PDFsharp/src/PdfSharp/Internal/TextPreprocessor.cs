using System.Text;
using System.Collections.Generic;

namespace PdfSharp.Internal
{
    public static class TextPreprocessor
    {
        // Check if a char is Hebrew
        private static bool IsHebrew(char c)
        {
            return c >= 0x0590 && c <= 0x05FF;
        }

        // Reverse each Hebrew run and reverse the order of runs
        public static string ReverseHebrewRunsAndOrder(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var runs = new List<string>();
            var run = new StringBuilder();
            bool runIsHebrew = IsHebrew(text[0]);

            foreach (var c in text)
            {
                if (IsHebrew(c) == runIsHebrew)
                {
                    run.Append(c);
                }
                else
                {
                    // Flush previous run
                    if (runIsHebrew)
                        runs.Add(ReverseString(run.ToString()));
                    else
                        runs.Add(run.ToString());

                    run.Clear();
                    run.Append(c);
                    runIsHebrew = IsHebrew(c);
                }
            }

            // Flush last run
            if (runIsHebrew)
                runs.Add(ReverseString(run.ToString()));
            else
                runs.Add(run.ToString());

            // Reverse the order of runs for proper RTL layout
            runs.Reverse();

            var sb = new StringBuilder();
            foreach (var r in runs)
            {
                sb.Append(r);
            }

            return sb.ToString();
        }

        private static string ReverseString(string s)
        {
            var arr = s.ToCharArray();
            System.Array.Reverse(arr);
            return new string(arr);
        }
    }
}
