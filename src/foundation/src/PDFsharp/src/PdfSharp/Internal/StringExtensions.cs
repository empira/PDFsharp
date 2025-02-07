namespace PdfSharp
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Add a number-suffix to the provided string.<br></br>
        /// If the string already has a number-suffix, the suffix is incremented by one.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AddIncrementalSuffix(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var numberSuffix = 1;   // will be incremented to 2 if text does not have a suffix
            var idx = text.Length - 1;
            // check if value ends with a number. if it does, increment it
            while (idx >= 0 && char.IsDigit(text[idx]))
                idx--;
            idx++;
            if (idx < text.Length)
#if NET6_0_OR_GREATER
                numberSuffix = int.Parse(text[idx..]);
#else
                numberSuffix = int.Parse(text.Substring(idx));
#endif
            numberSuffix++;
#if NET6_0_OR_GREATER
            var namePrefix = text[..idx];
#else
            var namePrefix = text.Substring(0, idx);
#endif
            return namePrefix + numberSuffix.ToString(CultureInfo.InvariantCulture);
        }
    }
}
