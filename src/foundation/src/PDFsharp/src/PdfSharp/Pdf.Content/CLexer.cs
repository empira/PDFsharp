// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
#if WPF
using System.IO;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Lexical analyzer for PDF content files. Adobe specifies no grammar, but it seems that it
    /// is a simple post-fix notation.
    /// </summary>
    public class CLexer
    {
        /// <summary>
        /// Initializes a new instance of the Lexer class.
        /// </summary>
        public CLexer(byte[] content)
        {
            _content = content;
            _charIndex = 0;
        }

        /// <summary>
        /// Initializes a new instance of the Lexer class.
        /// </summary>
        public CLexer(MemoryStream content)
        {
            _content = content.ToArray();
            _charIndex = 0;
        }

        /// <summary>
        /// Reads the next token and returns its type.
        /// </summary>
        public CSymbol ScanNextToken()
        {
        Again:
            ClearToken();
            char ch = MoveToNonWhiteSpace();
            switch (ch)
            {
                case '%':
                    // Eat comments, the parser doesn't handle them.
                    //return symbol = ScanComment();
                    ScanComment();
                    goto Again;

                case '/':
                    return _symbol = ScanName();

                //case 'R':
                //  if (Lexer.IsWhiteSpace(nextChar))
                //  {
                //    ScanNextChar();
                //    return Symbol.R;
                //  }
                //  break;

                case '+':
                case '-':
                    return _symbol = ScanNumber();

                case '[':
                    ScanNextChar();
                    return _symbol = CSymbol.BeginArray;

                case ']':
                    ScanNextChar();
                    return _symbol = CSymbol.EndArray;

                case '(':
                    return _symbol = ScanLiteralString();

                case '<':
                    if (_nextChar == '<')
                        return _symbol = ScanDictionary();
                    return _symbol = ScanHexadecimalString();

                case '.':
                    return _symbol = ScanNumber();

                case '"':
                case '\'':
                    return _symbol = ScanOperator();
            }
            if (Char.IsDigit(ch))
                return _symbol = ScanNumber();

            if (Char.IsLetter(ch))
                return _symbol = ScanOperator();

            if (ch == Chars.EOF)
                return _symbol = CSymbol.Eof;

            ContentReaderDiagnostics.HandleUnexpectedCharacter(ch);
            return _symbol = CSymbol.None;
        }

        /// <summary>
        /// Scans a comment line. (Not yet used, comments are skipped by lexer.)
        /// </summary>
        public CSymbol ScanComment()
        {
            Debug.Assert(_currChar == Chars.Percent);

            ClearToken();
            char ch;
            while ((ch = AppendAndScanNextChar()) != Chars.LF && ch != Chars.EOF) { }
            return _symbol = CSymbol.Comment;
        }

        /// <summary>
        /// Scans the bytes of an inline image.
        /// NYI: Just scans over it.
        /// </summary>
        public CSymbol ScanInlineImage()
        {
            // TODO: Implement inline images.
            // Skip this:
            // BI
            // … Key-value pairs …
            // ID
            // … Image data …
            // EI

            bool ascii85 = false;
            do
            {
                ScanNextToken();
                // HACK: Is image ASCII85 decoded?
                if (!ascii85 && _symbol == CSymbol.Name && Token is "/ASCII85Decode" or "/A85")
                    ascii85 = true;
            } while (_symbol != CSymbol.Operator || Token != "ID");

            if (ascii85)
            {
                // Look for '~>' because 'EI' may be part of the encoded image.
                while (_currChar != Chars.EOF && (_currChar != '~' || _nextChar != '>'))
                    ScanNextChar();
                if (_currChar == Chars.EOF)
                    ContentReaderDiagnostics.HandleUnexpectedCharacter(_currChar);
            }

            // Look for '<ws>EI<ws>', as 'EI' may be part of the binary image data here too.
            while (_currChar != Chars.EOF)
            {
                if (IsWhiteSpace(_currChar))
                {
                    if (ScanNextChar() == 'E')
                        if (ScanNextChar() == 'I')
                            if (IsWhiteSpace(ScanNextChar()))
                                break;
                }
                else
                    ScanNextChar();
            }
            if (_currChar == Chars.EOF)
                ContentReaderDiagnostics.HandleUnexpectedCharacter(_currChar);

            // We currently do nothing with inline images.
            return CSymbol.None;
        }

        /// <summary>
        /// Scans a name.
        /// </summary>
        public CSymbol ScanName()
        {
            Debug.Assert(_currChar == Chars.Slash);

            ClearToken();
            while (true)
            {
                char ch = AppendAndScanNextChar();
                if (IsWhiteSpace(ch) || IsDelimiter(ch))
                    return _symbol = CSymbol.Name;

                if (ch == '#')
                {
                    ScanNextChar();
                    char[] hex = new char[2];
                    hex[0] = _currChar;
                    hex[1] = _nextChar;
                    ScanNextChar();
                    // TODO Check syntax
                    ch = (char)(ushort)Int32.Parse(new string(hex), NumberStyles.AllowHexSpecifier);
                    _currChar = ch;
                }
            }
        }

        /// <summary>
        /// Scans the dictionary.
        /// </summary>
        protected CSymbol ScanDictionary()
        {
            // TODO Do an actual recursive parse instead of this simple scan.

            ClearToken();
            _token.Append(_currChar);      // '<'
            _token.Append(ScanNextChar()); // '<'

            bool inString = false, inHexString = false;
            int nestedDict = 0, nestedStringParen = 0;
            while (true)
            {
                char ch;
                _token.Append(ch = ScanNextChar());
                if (ch == '<')
                {
                    if (_nextChar == '<')
                    {
                        _token.Append(ScanNextChar());
                        ++nestedDict;
                    }
                    else
                        inHexString = true;
                }
                else if (!inHexString && ch == '(')
                {
                    if (inString)
                        ++nestedStringParen;
                    else
                    {
                        inString = true;
                        nestedStringParen = 0;
                    }
                }
                else if (inString && ch == ')')
                {
                    if (nestedStringParen > 0)
                        --nestedStringParen;
                    else
                        inString = false;
                }
                else if (inString && ch == '\\')
                    _token.Append(ScanNextChar());
                else if (ch == '>')
                {
                    if (inHexString)
                        inHexString = false;
                    else if (_nextChar == '>')
                    {
                        _token.Append(ScanNextChar());
                        if (nestedDict > 0)
                            --nestedDict;
                        else
                        {
                            ScanNextChar();
#if true
                            return CSymbol.Dictionary;
#else
                            return CSymbol.String;
#endif
                        }
                    }
                }
                else if (ch == Chars.EOF)
                    ContentReaderDiagnostics.HandleUnexpectedCharacter(ch);
            }
        }

        /// <summary>
        /// Scans an integer or real number.
        /// </summary>
        public CSymbol ScanNumber()
        {
            // Note: This is a copy of Lexer.ScanNumber with minimal changes. Keep both versions in sync as far as possible.
            const int maxDigitsForLong = 18;
            const int maxDecimalDigits = 10;
            long value = 0;
            int totalDigits = 0;
            int decimalDigits = 0;
            bool period = false;
            bool negative = false;

            ClearToken();
            char ch = _currChar;
            if (ch is '+' or '-')
            {
                if (ch == '-')
                    negative = true;
                _token.Append(ch);
                ch = ScanNextChar();
            }
            while (true)
            {
                if (Char.IsDigit(ch))
                {
                    _token.Append(ch);
                    ++totalDigits;
                    if (decimalDigits < maxDecimalDigits)
                    {
                        // Calculate the value if it still fits into long.
                        if (totalDigits <= maxDigitsForLong)
                            value = 10 * value + ch - '0';
                    }
                    if (period)
                        ++decimalDigits;
                }
                else if (ch == '.')
                {
                    if (period)
                        ContentReaderDiagnostics.ThrowContentReaderException("More than one period in number.");

                    period = true;
                    _token.Append(ch);
                }
                else
                    break;
                ch = ScanNextChar();
            }

            if (totalDigits > maxDigitsForLong ||
                decimalDigits > maxDecimalDigits)
            {
                // The number is too big for long or has too many decimal digits for our own code, so we provide it as real only.
                // Number will be parsed here.
                _tokenAsReal = Double.Parse(_token.ToString(), CultureInfo.InvariantCulture);
                return CSymbol.Real;
            }

            if (negative)
                value = -value;

            if (period)
            {
                if (decimalDigits > 0)
                {
                    _tokenAsReal = value / PowersOf10[decimalDigits];
                    //_tokenAsLong = value / PowersOf10[decimalDigits];
                }
                else
                {
                    _tokenAsReal = value;
                    _tokenAsLong = value;
                }
                return CSymbol.Real;
            }
            _tokenAsLong = value;
            _tokenAsReal = Convert.ToDouble(value);

            Debug.Assert(Int64.Parse(_token.ToString(), CultureInfo.InvariantCulture) == value);

            if (value is >= Int32.MinValue and < Int32.MaxValue)
                return CSymbol.Integer;

            return CSymbol.Real;
        }

        static readonly double[] PowersOf10 = { 1, 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000, 10_000_000_000 };

        /// <summary>
        /// Scans an operator.
        /// </summary>
        public CSymbol ScanOperator()
        {
            ClearToken();
            char ch = _currChar;
            // Scan token.
            while (IsOperatorChar(ch))
                ch = AppendAndScanNextChar();

            return _symbol = CSymbol.Operator;
        }

        // TODO        
        /// <summary>
        /// Scans a literal string.
        /// </summary>
        public CSymbol ScanLiteralString()
        {
            Debug.Assert(_currChar == Chars.ParenLeft);

            ClearToken();
            int parenLevel = 0;
            char ch = ScanNextChar();
            // Test UNICODE string
            if (ch == '\xFE' && _nextChar == '\xFF')
            {
                // I'm not sure if the code is correct in any case.
                // ? Can a UNICODE character not start with ')' as hibyte
                // ? What about \# escape sequences
                ScanNextChar();
                char chHi = ScanNextChar();
                if (chHi == ')')
                {
                    // The empty Unicode string...
                    ScanNextChar();
                    return _symbol = CSymbol.String;
                }
                char chLo = ScanNextChar();
                ch = (char)(chHi * 256 + chLo);
                while (true)
                {
                SkipChar:
                    switch (ch)
                    {
                        case '(':
                            parenLevel++;
                            break;

                        case ')':
                            if (parenLevel == 0)
                            {
                                ScanNextChar();
                                return _symbol = CSymbol.String;
                            }
                            parenLevel--;
                            break;

                        case '\\':
                            {
                                // TODO: not sure that this is correct...
                                ch = ScanNextChar();
                                switch (ch)
                                {
                                    case 'n':
                                        ch = Chars.LF;
                                        break;

                                    case 'r':
                                        ch = Chars.CR;
                                        break;

                                    case 't':
                                        ch = Chars.HT;
                                        break;

                                    case 'b':
                                        ch = Chars.BS;
                                        break;

                                    case 'f':
                                        ch = Chars.FF;
                                        break;

                                    case '(':
                                        ch = Chars.ParenLeft;
                                        break;

                                    case ')':
                                        ch = Chars.ParenRight;
                                        break;

                                    case '\\':
                                        ch = Chars.BackSlash;
                                        break;

                                    case Chars.LF:
                                        ch = ScanNextChar();
                                        goto SkipChar;

                                    default:
                                        if (Char.IsDigit(ch))
                                        {
                                            // Octal character code
                                            int n = ch - '0';
                                            if (Char.IsDigit(_nextChar))
                                            {
                                                n = n * 8 + ScanNextChar() - '0';
                                                if (Char.IsDigit(_nextChar))
                                                    n = n * 8 + ScanNextChar() - '0';
                                            }
                                            ch = (char)n;
                                        }
                                        break;
                                }
                                break;
                            }

                        //case '#':
                        //    ContentReaderDiagnostics.HandleUnexpectedCharacter('#');
                        //    break;

                        default:
                            // Every other char is appended to the token.
                            break;
                    }
                    _token.Append(ch);
                    chHi = ScanNextChar();
                    if (chHi == ')')
                    {
                        ScanNextChar();
                        return _symbol = CSymbol.String;
                    }
                    chLo = ScanNextChar();
                    ch = (char)(chHi * 256 + chLo);
                }
            }
            else
            {
                // 8-bit characters
                while (true)
                {
                SkipChar:
                    switch (ch)
                    {
                        case '(':
                            parenLevel++;
                            break;

                        case ')':
                            if (parenLevel == 0)
                            {
                                ScanNextChar();
                                return _symbol = CSymbol.String;
                            }
                            parenLevel--;
                            break;

                        case '\\':
                            {
                                ch = ScanNextChar();
                                switch (ch)
                                {
                                    case 'n':
                                        ch = Chars.LF;
                                        break;

                                    case 'r':
                                        ch = Chars.CR;
                                        break;

                                    case 't':
                                        ch = Chars.HT;
                                        break;

                                    case 'b':
                                        ch = Chars.BS;
                                        break;

                                    case 'f':
                                        ch = Chars.FF;
                                        break;

                                    case '(':
                                        ch = Chars.ParenLeft;
                                        break;

                                    case ')':
                                        ch = Chars.ParenRight;
                                        break;

                                    case '\\':
                                        ch = Chars.BackSlash;
                                        break;

                                    case Chars.LF:
                                        ch = ScanNextChar();
                                        goto SkipChar;

                                    default:
                                        if (Char.IsDigit(ch))
                                        {
                                            // Octal character code.
                                            int n = ch - '0';
                                            if (Char.IsDigit(_nextChar))
                                            {
                                                n = n * 8 + ScanNextChar() - '0';
                                                if (Char.IsDigit(_nextChar))
                                                    n = n * 8 + ScanNextChar() - '0';
                                            }
                                            ch = (char)n;
                                        }
                                        break;
                                }
                                break;
                            }

                        //case '#':
                        //    ContentReaderDiagnostics.HandleUnexpectedCharacter('#');
                        //    break;

                        default:
                            // Every other char is appended to the token.
                            break;
                    }
                    _token.Append(ch);
                    //token.Append(Encoding.GetEncoding(1252).GetString(new byte[] { (byte)ch }));
                    ch = ScanNextChar();
                }
            }
        }

        /// <summary>
        /// Scans a hexadecimal string.
        /// </summary>
        public CSymbol ScanHexadecimalString()
        {
            Debug.Assert(_currChar == Chars.Less);

            ClearToken();
            char[] hex = new char[2];
            ScanNextChar();
            while (true)
            {
                MoveToNonWhiteSpace();
                if (_currChar == '>')
                {
                    ScanNextChar();
                    break;
                }
                if (Char.IsLetterOrDigit(_currChar))
                {
                    hex[0] = Char.ToUpper(_currChar);
                    hex[1] = Char.ToUpper(_nextChar);
                    int ch = Int32.Parse(new string(hex), NumberStyles.AllowHexSpecifier);
                    _token.Append(Convert.ToChar(ch));
                    ScanNextChar();
                    ScanNextChar();
                }
            }
            string chars = _token.ToString();
            int count = chars.Length;
            if (count > 2 && chars[0] == (char)0xFE && chars[1] == (char)0xFF)
            {
                Debug.Assert(count % 2 == 0);
                _token.Length = 0;
                for (int idx = 2; idx < count; idx += 2)
                    _token.Append((char)(chars[idx] * 256 + chars[idx + 1]));
            }
            return _symbol = CSymbol.HexString;
        }

        /// <summary>
        /// Move current position one character further in content stream.
        /// </summary>
        internal char ScanNextChar()
        {
            if (ContLength <= _charIndex)
            {
                _currChar = Chars.EOF;
                if (IsOperatorChar(_nextChar))
                    _token.Append(_nextChar);
                _nextChar = Chars.EOF;
            }
            else
            {
                _currChar = _nextChar;
                _nextChar = (char)_content[_charIndex++];
                if (_currChar == Chars.CR)
                {
                    if (_nextChar == Chars.LF)
                    {
                        // Treat CR LF as LF
                        _currChar = _nextChar;
                        if (ContLength <= _charIndex)
                            _nextChar = Chars.EOF;
                        else
                            _nextChar = (char)_content[_charIndex++];
                    }
                    else
                    {
                        // Treat single CR as LF
                        _currChar = Chars.LF;
                    }
                }
            }
            return _currChar;
        }

        /// <summary>
        /// Resets the current token to the empty string.
        /// </summary>
        void ClearToken()
        {
            _token.Length = 0;
            _tokenAsLong = 0;
            _tokenAsReal = 0;
        }

        /// <summary>
        /// Appends current character to the token and reads next one.
        /// </summary>
        internal char AppendAndScanNextChar()
        {
            _token.Append(_currChar);
            return ScanNextChar();
        }

        /// <summary>
        /// If the current character is not a white space, the function immediately returns it.
        /// Otherwise, the PDF cursor is moved forward to the first non-white space or EOF.
        /// White spaces are NUL, HT, LF, FF, CR, and SP.
        /// </summary>
        public char MoveToNonWhiteSpace()
        {
            while (_currChar != Chars.EOF)
            {
                switch (_currChar)
                {
                    case Chars.NUL:
                    case Chars.HT:
                    case Chars.LF:
                    case Chars.FF:
                    case Chars.CR:
                    case Chars.SP:
                        ScanNextChar();
                        break;

                    default:
                        return _currChar;
                }
            }
            return _currChar;
        }

        /// <summary>
        /// Gets or sets the current symbol.
        /// </summary>
        public CSymbol Symbol
        {
            get => _symbol;
            set => _symbol = value;
        }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public string Token => _token.ToString();

        /// <summary>
        /// Interprets current token as integer literal.
        /// </summary>
        internal int TokenToInteger
        {
            get
            {
                Debug.Assert(_tokenAsLong == Int32.Parse(_token.ToString(), CultureInfo.InvariantCulture));
                return (int)_tokenAsLong;
            }
        }

        /// <summary>
        /// Interpret current token as real or integer literal.
        /// </summary>
        internal double TokenToReal
        {
            get
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Debug.Assert(_tokenAsReal == double.Parse(_token.ToString(), CultureInfo.InvariantCulture));
                return _tokenAsReal;
            }
        }

        /// <summary>
        /// Indicates whether the specified character is a content stream white-space character.
        /// </summary>
        internal static bool IsWhiteSpace(char ch)
        {
            switch (ch)
            {
                case Chars.NUL:  // 0 Null
                case Chars.HT:   // 9 Tab
                case Chars.LF:   // 10 Line feed
                case Chars.FF:   // 12 Form feed
                case Chars.CR:   // 13 Carriage return
                case Chars.SP:   // 32 Space
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Indicates whether the specified character is an content operator character.
        /// </summary>
        internal static bool IsOperatorChar(char ch)
        {
            if (Char.IsLetter(ch))
                return true;
            switch (ch)
            {
                case Chars.Asterisk:    // *
                case Chars.QuoteSingle: // '
                case Chars.QuoteDouble:    // "
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Indicates whether the specified character is a PDF delimiter character.
        /// </summary>
        internal static bool IsDelimiter(char ch)
        {
            switch (ch)
            {
                case '(':
                case ')':
                case '<':
                case '>':
                case '[':
                case ']':
                case '/':
                case '%':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the length of the content.
        /// </summary>
        public int ContLength => _content.Length;

        /// <summary>
        /// Gets or sets the position in the content.
        /// </summary>
        public int Position
        {
            get => _charIndex;
            set
            {
                _charIndex = value;
                _currChar = (char)_content[_charIndex - 1];
                _nextChar = (char)_content[_charIndex - 1];
            }
        }

        readonly byte[] _content;
        int _charIndex;
        char _currChar;
        char _nextChar;

        readonly StringBuilder _token = new();
        long _tokenAsLong;
        double _tokenAsReal;
        CSymbol _symbol = CSymbol.None;
    }
}
