// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Class to write RTF output.
    /// </summary>
    class RtfWriter
    {
        /// <summary>
        /// Initializes a new instance of the RtfWriter class.
        /// </summary>
        public RtfWriter(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        /// <summary>
        /// Writes a left brace.
        /// </summary>
        public void StartContent()
        {
            _textWriter.Write("{");
            _lastWasControl = false;
        }

        /// <summary>
        /// Writes a right brace.
        /// </summary>
        public void EndContent()
        {
            _textWriter.Write("}");
            _lastWasControl = false;
        }

        /// <summary>
        /// Writes the given text, handling special characters before.
        /// </summary>
        public void WriteText(string text)
        {
            StringBuilder strBuilder = new StringBuilder(text.Length);
            if (_lastWasControl)
                strBuilder.Append(" ");

            int length = text.Length;
            for (int idx = 0; idx < length; idx++)
            {
                char ch = text[idx];
                switch (ch)
                {
                    case '\\':
                        strBuilder.Append(@"\\");
                        break;

                    case '{':
                        strBuilder.Append(@"\{");
                        break;

                    case '}':
                        strBuilder.Append(@"\}");
                        break;

                    case '\u00AD': //character 173, softhyphen
                        strBuilder.Append(@"\-");
                        break;

                    default:
                        if (IsCp1252Char(ch))
                            strBuilder.Append(ch);
                        else
                        {
                            strBuilder.Append(@"\u");
                            strBuilder.Append(((int)ch).ToString());
                            strBuilder.Append('?');
                        }
                        break;
                }
            }
            _textWriter.Write(strBuilder.ToString());
            _lastWasControl = false;
        }

        /// <summary>
        /// Indicates whether the specified Unicode character is available in the Ansi code page 1252.
        /// </summary>
        static bool IsCp1252Char(char ch)
        {
            if (ch < '\u00FF')
                return true;
            switch (ch)
            {
                case '\u20AC':
                case '\u0081':
                case '\u201A':
                case '\u0192':
                case '\u201E':
                case '\u2026':
                case '\u2020':
                case '\u2021':
                case '\u02C6':
                case '\u2030':
                case '\u0160':
                case '\u2039':
                case '\u0152':
                case '\u008D':
                case '\u017D':
                case '\u008F':
                case '\u0090':
                case '\u2018':
                case '\u2019':
                case '\u201C':
                case '\u201D':
                case '\u2022':
                case '\u2013':
                case '\u2014':
                case '\u02DC':
                case '\u2122':
                case '\u0161':
                case '\u203A':
                case '\u0153':
                case '\u009D':
                case '\u017E':
                case '\u0178':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Writes the number as hex value. Only numbers &lt;= 255 are allowed.
        /// </summary>
        public void WriteHex(uint hex)
        {
            if (hex > 0xFF)
                //TODO_OLD: Error message? Debug.Assert?
                return;

            _textWriter.Write(@"\'" + hex.ToString("x"));
            _lastWasControl = false;
            // No blank allowed after this code.
        }

        /// <summary>
        /// Writes a blank in paragraph text.
        /// </summary>
        public void WriteBlank()
        {
            _textWriter.Write(" ");
        }

        /// <summary>
        /// Writes a semicolon as separator e.g. in in font tables.
        /// </summary>
        public void WriteSeparator()
        {
            _textWriter.Write(";");
            _lastWasControl = false;
        }

        /// <summary>
        /// Writes the given string as control word optionally with a star.
        /// </summary>
        public void WriteControl(string ctrl, bool withStar)
        {
            if (!withStar)
                WriteControl(ctrl);
            else
                WriteControlWithStar(ctrl);
        }

        /// <summary>
        /// Writes the given string as control word with a star followed by a space.
        /// </summary>
        public void WriteControl(string ctrl, string value, bool withStar)
        {
            if (withStar)
                WriteControlWithStar(ctrl, value);
            else
                WriteControl(ctrl, value);
        }

        /// <summary>
        /// Writes the given string as control word with a star followed by a space.
        /// </summary>
        public void WriteControl(string ctrl, int value, bool withStar)
        {
            WriteControl(ctrl, value.ToString(), withStar);
        }

        /// <summary>
        /// Writes the given string as control word.
        /// </summary>
        public void WriteControl(string ctrl)
        {
            WriteControl(ctrl, "");
        }

        /// <summary>
        /// Writes the given string as control word.
        /// Many control words must be terminated by a space. In some cases (e.g. for the non-breaking space "\~") there must be no space.
        /// Set markAsControl to true for automatic padding of a blank, or false to suppress automatic padding.
        /// The default (for overloads without markAsControl) is true.
        /// </summary>
        public void WriteControl(bool markAsControl, string ctrl)
        {
            WriteControl(markAsControl, ctrl, "");
        }

        /// <summary>
        /// Writes the given string and integer as control word / value pair followed by a space.
        /// </summary>
        public void WriteControl(string ctrl, int value)
        {
            WriteControl(ctrl, value.ToString());
        }

        /// <summary>
        /// Writes the given strings as control word / value pair.
        /// </summary>
        public void WriteControl(string ctrl, string value)
        {
            _textWriter.Write("\\" + ctrl + value);
            _lastWasControl = true;
        }

        /// <summary>
        /// Writes the given strings as control word / value pair.
        /// Many control word / value pairs must be terminated by a space. In some cases (e.g. for the non-breaking space "\~") there must be no space.
        /// Set markAsControl to true for automatic padding of a blank, or false to suppress automatic padding.
        /// The default (for overloads without markAsControl) is true.
        /// </summary>
        public void WriteControl(bool markAsControl, string ctrl, string value)
        {
            _textWriter.Write("\\" + ctrl + value);
            _lastWasControl = markAsControl;
        }

        /// <summary>
        /// Writes the given string and integer as control word / value pair with a star.
        /// </summary>
        public void WriteControlWithStar(string ctrl)
        {
            WriteControlWithStar(ctrl, "");
        }

        /// <summary>
        /// Writes the given string and integer as control word / value pair followed by a space.
        /// </summary>
        public void WriteControlWithStar(string ctrl, int value)
        {
            WriteControlWithStar(ctrl, value.ToString());
        }

        /// <summary>
        /// Writes the given string and integer as control word / value pair with a star.
        /// </summary>
        public void WriteControlWithStar(string ctrl, string value)
        {
            _textWriter.Write("\\*\\" + ctrl + value);
            _lastWasControl = true;
        }

        bool _lastWasControl;
        readonly TextWriter _textWriter;
    }
}