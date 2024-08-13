// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Provides functions for encoding and decoding of DDL text.
    /// </summary>
    public static class DdlEncoder
    {
        /// <summary>
        /// Converts a string into a text phrase.
        /// </summary>
        public static string? StringToText(string? str)
        {
            if (str == null)
                return null;

            int length = str.Length;
            var sb = new StringBuilder(length + (length >> 2));
            for (int index = 0; index < length; index++)
            {
                // Don’t convert characters into DDL.
                char ch = str[index];
                switch (ch)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '{':
                        sb.Append("\\{");
                        break;

                    case '}':
                        sb.Append("\\}");
                        break;

                    // Escape comments.
                    case '/':
                        if (index < length - 1 && str[index + 1] == '/')
                        {
                            sb.Append("\\//");
                            index++;
                        }
                        else
                            sb.Append("/");
                        break;

                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a string into a string literal (a quoted string).
        /// </summary>
        public static string StringToLiteral(string? str)
        {
            int length;
            if (str == null || (length = str.Length) == 0)
                return "\"\"";

            var sb = new StringBuilder(length + (length >> 2));
            sb.Append("\"");
            for (int index = 0; index < length; index++)
            {
                char ch = str[index];
                switch (ch)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '"':
                        sb.Append("\\\"");
                        break;

                    default:
                        sb.Append(ch);
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }

        /// <summary>
        /// Scans the given string for characters which are invalid for identifiers.
        /// Strings are limited to 64 characters.
        /// </summary>
        internal static bool IsDdeIdentifier(string? name)
        {
            if (String.IsNullOrEmpty(name))
                return false;

            int len = name.Length;
            if (len > 64)
                return false;

            for (int index = 0; index < len; index++)
            {
                char ch = name[index];
                if (ch == ' ')
                    return false;

                if (index == 0)
                {
                    if (!Char.IsLetter(ch) && ch != '_')
                        return false;
                }
                else
                {
                    if (!Char.IsLetterOrDigit(ch) && ch != '_')
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Quotes the given name if it contains characters which are invalid for identifiers.
        /// </summary>
        internal static string QuoteIfNameContainsBlanks(string name)
        {
            if (IsDdeIdentifier(name))
                return name;
            return "\"" + name + "\"";
        }
    }
}
