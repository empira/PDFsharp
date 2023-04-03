// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Character table by name.
    /// </summary>
    public static class Chars
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The EOF marker.
        /// </summary>
        public const char EOF = (char)65535; //unchecked((char)(-1));
        /// <summary>
        /// The null byte.
        /// </summary>
        public const char NUL = '\0';   // EOF
        /// <summary>
        /// The carriage return character (ignored by lexer).
        /// </summary>
        public const char CR = '\x0D'; // ignored by lexer
        /// <summary>
        /// The line feed character.
        /// </summary>
        public const char LF = '\x0A'; // Line feed
        /// <summary>
        /// The bell character.
        /// </summary>
        public const char BEL = '\a';   // Bell
        /// <summary>
        /// The backspace character.
        /// </summary>
        public const char BS = '\b';   // Backspace
        /// <summary>
        /// The form feed character.
        /// </summary>
        public const char FF = '\f';   // Form feed
        /// <summary>
        /// The horizontal tab character.
        /// </summary>
        public const char HT = '\t';   // Horizontal tab
        /// <summary>
        /// The vertical tab character.
        /// </summary>
        public const char VT = '\v';   // Vertical tab
        /// <summary>
        /// The non-breakable space character (aka no-break space or non-breaking space).
        /// </summary>
        public const char NonBreakableSpace = (char)160;  // char(160)

        // The following names come from "PDF Reference Third Edition"
        // Appendix D.1, Latin Character Set and Encoding
        /// <summary>
        /// The space character.
        /// </summary>
        public const char SP = ' ';
        /// <summary>
        /// The double quote character.
        /// </summary>
        public const char QuoteDbl = '"';
        /// <summary>
        /// The single quote character.
        /// </summary>
        public const char QuoteSingle = '\'';
        /// <summary>
        /// The left parenthesis.
        /// </summary>
        public const char ParenLeft = '(';
        /// <summary>
        /// The right parenthesis.
        /// </summary>
        public const char ParenRight = ')';
        /// <summary>
        /// The left brace.
        /// </summary>
        public const char BraceLeft = '{';
        /// <summary>
        /// The right brace.
        /// </summary>
        public const char BraceRight = '}';
        /// <summary>
        /// The left bracket.
        /// </summary>
        public const char BracketLeft = '[';
        /// <summary>
        /// The right bracket.
        /// </summary>
        public const char BracketRight = ']';
        /// <summary>
        /// The less-than sign.
        /// </summary>
        public const char Less = '<';
        /// <summary>
        /// The greater-than sign.
        /// </summary>
        public const char Greater = '>';
        /// <summary>
        /// The equal sign.
        /// </summary>
        public const char Equal = '=';
        /// <summary>
        /// The period.
        /// </summary>
        public const char Period = '.';
        /// <summary>
        /// The semicolon.
        /// </summary>
        public const char Semicolon = ';';
        /// <summary>
        /// The colon.
        /// </summary>
        public const char Colon = ':';
        /// <summary>
        /// The slash.
        /// </summary>
        public const char Slash = '/';
        /// <summary>
        /// The bar character.
        /// </summary>
        public const char Bar = '|';
        /// <summary>
        /// The back slash.
        /// </summary>
        public const char BackSlash = '\\';
        /// <summary>
        /// The percent sign.
        /// </summary>
        public const char Percent = '%';
        /// <summary>
        /// The dollar sign.
        /// </summary>
        public const char Dollar = '$';
        /// <summary>
        /// The at sign.
        /// </summary>
        public const char At = '@';
        /// <summary>
        /// The number sign.
        /// </summary>
        public const char NumberSign = '#';
        /// <summary>
        /// The question mark.
        /// </summary>
        public const char Question = '?';
        /// <summary>
        /// The hyphen.
        /// </summary>
        public const char Hyphen = '-';  // char(45)
        /// <summary>
        /// The soft hyphen.
        /// </summary>
        public const char SoftHyphen = '\u00AD';  // char(173)
        /// <summary>
        /// The currency sign.
        /// </summary>
        public const char Currency = '¤';

        // ReSharper restore InconsistentNaming
    }
}
