// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Character table by name. Same as PdfSharp.Pdf.IO.Chars. Not yet clear if necessary.
    /// </summary>
    static class Chars
    {
        public const char EOF = (char)65535; //unchecked((char)(-1));
        public const char NUL = '\0';  // EOF
        public const char CR = '\x0D'; // ignored by lexer
        public const char LF = '\x0A'; // Line feed
        public const char BEL = '\a';  // Bell
        public const char BS = '\b';   // Backspace
        public const char FF = '\f';   // Form feed
        public const char HT = '\t';   // Horizontal tab
        public const char VT = '\v';   // Vertical tab
        public const char NonBreakableSpace = (char)160;  // char(160)

        // The following names come from "PDF Reference Third Edition"
        // Appendix D.1, Latin Character Set and Encoding
        public const char SP = ' ';
        public const char QuoteDbl = '"';
        public const char QuoteSingle = '\'';
        public const char ParenLeft = '(';
        public const char ParenRight = ')';
        public const char BraceLeft = '{';
        public const char BraceRight = '}';
        public const char BracketLeft = '[';
        public const char BracketRight = ']';
        public const char Less = '<';
        public const char Greater = '>';
        public const char Equal = '=';
        public const char Period = '.';
        public const char Semicolon = ';';
        public const char Colon = ':';
        public const char Slash = '/';
        public const char Bar = '|';
        public const char BackSlash = '\\';
        public const char Percent = '%';
        public const char Dollar = '$';
        public const char At = '@';
        public const char NumberSign = '#';
        public const char Asterisk = '*';
        public const char Question = '?';
        public const char Hyphen = '-';  // char(45)
        public const char SoftHyphen = '\u00AD';  // char(173)
        public const char Currency = '¤';
    }
}
