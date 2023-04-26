// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Character table by name.
    /// </summary>
    public sealed class Chars
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// The null or EOF character.
        /// </summary>
        public const char Null = '\0';   // EOF
        /// <summary>
        /// The carriage-return character.
        /// </summary>
        public const char CR = '\x0D'; // ignored by scanner
        /// <summary>
        /// The line-feed character.
        /// </summary>
        public const char LF = '\x0A';
        /// <summary>
        /// The bell character.
        /// </summary>
        public const char BEL = '\a';   // Bell
        /// <summary>
        /// The backspace character.
        /// </summary>
        public const char BS = '\b';   // Backspace
        /// <summary>
        /// The form-feed character.
        /// </summary>
        public const char FF = '\f';   // Form feed
        /// <summary>
        /// The horizontal-tab character.
        /// </summary>
        public const char HT = '\t';   // Horizontal tab
        /// <summary>
        /// The vertical-tab character.
        /// </summary>
        public const char VT = '\v';   // Vertical tab
        /// <summary>
        /// The no-break space.
        /// </summary>
        public const char NonBreakableSpace = (char)160;  // char(160)
        // ReSharper restore InconsistentNaming

        // ===== The following names come from "PDF Reference Third Edition" =====
        // ===== Appendix D.1, Latin Character Set and Encoding =====
        /// <summary>
        /// A regular blank.
        /// </summary>
        public const char Space = ' ';
        /// <summary>
        /// A double quote '"'.
        /// </summary>
        public const char QuoteDbl = '"';
        /// <summary>
        /// A single quote "'".
        /// </summary>
        public const char QuoteSingle = '\'';
        /// <summary>
        /// A left parenthesis '('.
        /// </summary>
        public const char ParenLeft = '(';
        /// <summary>
        /// A right parenthesis ')'.
        /// </summary>
        public const char ParenRight = ')';
        /// <summary>
        /// A left brace '{'.
        /// </summary>
        public const char BraceLeft = '{';
        /// <summary>
        /// A right brace '}'.
        /// </summary>
        public const char BraceRight = '}';
        /// <summary>
        /// A left bracket '['.
        /// </summary>
        public const char BracketLeft = '[';
        /// <summary>
        /// A right bracket ']'.
        /// </summary>
        public const char BracketRight = ']';
        /// <summary>
        /// A less sign '&lt;'.
        /// </summary>
        public const char Less = '<';
        /// <summary>
        /// A greater sign '&gt;'.
        /// </summary>
        public const char Greater = '>';
        /// <summary>
        /// An equal sign '='.
        /// </summary>
        public const char Equal = '=';
        /// <summary>
        /// A period '.'.
        /// </summary>
        public const char Period = '.';
        /// <summary>
        /// A semicolon ';'.
        /// </summary>
        public const char Semicolon = ';';
        /// <summary>
        /// A colon ':'.
        /// </summary>
        public const char Colon = ':';
        /// <summary>
        /// A slash '/'.
        /// </summary>
        public const char Slash = '/';
        /// <summary>
        /// A bar '|'.
        /// </summary>
        public const char Bar = '|';
        /// <summary>
        /// A back-slash '\'.
        /// </summary>
        public const char BackSlash = '\\';
        /// <summary>
        /// A percent sign '%'.
        /// </summary>
        public const char Percent = '%';
        /// <summary>
        /// A dollar sign '$'.
        /// </summary>
        public const char Dollar = '$';
        /// <summary>
        /// An at sign '@'.
        /// </summary>
        public const char At = '@';
        /// <summary>
        /// A number sign '#'.
        /// </summary>
        public const char NumberSign = '#';
        /// <summary>
        /// A question mark '?'.
        /// </summary>
        public const char Question = '?';
        /// <summary>
        /// A hyphen.
        /// </summary>
        public const char Hyphen = '-';  // char(45)
        /// <summary>
        /// A soft-hyphen.
        /// </summary>
        public const char SoftHyphen = '\u00AD';  // char(173)
        /// <summary>
        /// A currency sign '¤'.
        /// </summary>
        public const char Currency = '¤';
    }
}
