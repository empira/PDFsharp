// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;

/*
  ddl = <document> | <empty>
  
  table-element:
    \table «attributes»opt { «columns-element» «rows-element» }

  table-element:
    \table «attributes»opt { «columns-element» «rows-element» }
*/

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// DdlScanner
    /// </summary>
    sealed class DdlScanner
    {
        /// <summary>
        /// Initializes a new instance of the DdlScanner class.
        /// </summary>
        internal DdlScanner(string documentFileName, string ddl, DdlReaderErrors errors)
        {
            //_errors = errors; Not (yet?) used.

            DocumentPath = documentFileName;
            _strDocument = ddl;
            _ddlLength = _strDocument.Length;
            _idx = 0;
            _idxLine = 1;
            _idxLinePos = 0;

            DocumentFileName = documentFileName;

            //_curDocumentIndex = _idx; Not (yet?) used.
            CurrentLine = _idxLine;
            CurrentLinePos = _idxLinePos;

            ScanNextChar();
        }

        /// <summary>
        /// Initializes a new instance of the DdlScanner class.
        /// </summary>
        internal DdlScanner(string ddl, DdlReaderErrors errors)
            : this("", ddl, errors)
        { }

        /// <summary>
        /// Reads to the next DDL token. Comments are ignored.
        /// </summary>
        /// <returns>
        /// Returns the current symbol.
        /// It is Symbol.Eof if the end of the DDL string is reached.
        /// </returns>
        internal Symbol ReadCode()
        {
        Again:
            Symbol = Symbol.None;
            TokenType = TokenType.None;
            Token = "";

            MoveToNonWhiteSpace();
            SaveCurDocumentPos();

            if (_currChar == Chars.Null)
            {
                Symbol = Symbol.Eof;
                return Symbol.Eof;
            }

            if (IsIdentifierChar(_currChar, true))
            {
                // Token is identifier.
                Symbol = ScanIdentifier();
                TokenType = TokenType.Identifier;
                // Some keywords do not start with a backslash:
                // true, false, and null.
                var sym = KeyWords.SymbolFromName(Token);
                if (sym != Symbol.None)
                {
                    Symbol = sym;
                    TokenType = TokenType.KeyWord;
                }
            }
            else if (_currChar == '"')
            {
                // Token is string literal.
                Token += ScanStringLiteral();
                Symbol = Symbol.StringLiteral;
                TokenType = TokenType.StringLiteral;
            }
            //NYI: else if (IsNumber())
            //      symbol = ScanNumber(false);
            else if (IsDigit(_currChar) ||
                     _currChar is '-' or '+' && IsDigit(_nextChar))
            {
                // Token is number literal.
                Symbol = ScanNumber(false);
                TokenType = Symbol == Symbol.RealLiteral ? TokenType.RealLiteral : TokenType.IntegerLiteral;
            }
            else if (_currChar == '.' && IsDigit(_nextChar))
            {
                // Token is real literal.
                Symbol = ScanNumber(true);
                TokenType = TokenType.RealLiteral;
            }
            else if (_currChar == '\\')
            {
                // Token is keyword or escaped character.
                Token = "\\";
                Symbol = ScanKeyword();
                TokenType = Symbol != Symbol.None ? TokenType.KeyWord : TokenType.None;
            }
            else if (_currChar == '/' && _nextChar == '/')
            {
                // Token is comment. In code comments are ignored.
                ScanSingleLineComment();
                goto Again;
            }
            else if (_currChar == '@' && _nextChar == '"')
            {
                // Token is verbatim string literal.
                ScanNextChar();
                Token += ScanVerbatimStringLiteral();
                Symbol = Symbol.StringLiteral;
                TokenType = Symbol != Symbol.None ? TokenType.StringLiteral : TokenType.None;
            }
            else
            {
                // Punctuator or syntax error.
                Symbol = ScanPunctuator();
            }
            return Symbol;
        }

        /// <summary>
        /// Gets the next keyword at the current position without touching the DDL cursor.
        /// </summary>
        internal Symbol PeekKeyword()
        {
            Debug.Assert(_currChar == Chars.BackSlash);

            return PeekKeyword(_idx);
        }

        /// <summary>
        /// Gets the next keyword without touching the DDL cursor.
        /// </summary>
        internal Symbol PeekKeyword(int index)
        {
            // Check special keywords.
            switch (_strDocument[index])
            {
                case '{':
                case '}':
                case '\\':
                case '-':
                case '(':
                    return Symbol.Character;
            }

            string token = "\\";
            int idx = index;
            int length = _ddlLength - idx;
            while (length > 0)
            {
                char ch = _strDocument[idx++];
                if (IsLetter(ch))
                {
                    token += ch;
                    length--;
                }
                else
                    break;
            }
            return KeyWords.SymbolFromName(token);
        }

        /// <summary>
        /// Gets the next punctuation terminal symbol without touching the DDL cursor.
        /// </summary>
        Symbol PeekPunctuation(int index)
        {
            Symbol sym = Symbol.None;
            char ch = _strDocument[index];
            switch (ch)
            {
                case '{':
                    sym = Symbol.BraceLeft;
                    break;

                case '}':
                    sym = Symbol.BraceRight;
                    break;

                case '[':
                    sym = Symbol.BracketLeft;
                    break;

                case ']':
                    sym = Symbol.BracketRight;
                    break;

                case '(':
                    sym = Symbol.ParenLeft;
                    break;

                case ')':
                    sym = Symbol.ParenRight;
                    break;

                case ':':
                    sym = Symbol.Colon;
                    break;

                case ';':
                    sym = Symbol.Semicolon;
                    break;

                case '.':
                    sym = Symbol.Dot;
                    break;

                case ',':
                    sym = Symbol.Comma;
                    break;

                case '%':
                    sym = Symbol.Percent;
                    break;

                case '$':
                    sym = Symbol.Dollar;
                    break;

                case '@':
                    sym = Symbol.At;
                    break;

                case '#':
                    sym = Symbol.Hash;
                    break;

                //case '?':
                //  sym = Symbol.Question;
                //  break;

                case '¤':
                    sym = Symbol.Currency; //??? used in DDL?
                    break;

                //case '|':
                //  sym = Symbol.Bar;
                //  break;

                case '=':
                    sym = Symbol.Assign;
                    break;

                case '/':
                    sym = Symbol.Slash;
                    break;

                case '\\':
                    sym = Symbol.BackSlash;
                    break;

                case '+':
                    if (_ddlLength >= index + 1 && _strDocument[index + 1] == '=')
                        sym = Symbol.PlusAssign;
                    else
                        sym = Symbol.Plus;
                    break;

                case '-':
                    if (_ddlLength >= index + 1 && _strDocument[index + 1] == '=')
                        sym = Symbol.MinusAssign;
                    else
                        sym = Symbol.Minus;
                    break;

                case Chars.CR:
                    sym = Symbol.CR;
                    break;

                case Chars.LF:
                    sym = Symbol.LF;
                    break;

                case Chars.Space:
                    sym = Symbol.Blank;
                    break;

                case Chars.Null:
                    sym = Symbol.Eof;
                    break;
            }
            return sym;
        }

        /// <summary>
        /// Gets the next symbol without touching the DDL cursor.
        /// </summary>
        internal Symbol PeekSymbol()
        {
            int idx = _idx - 1;
            int length = _ddlLength - idx;

            // Move to first non whitespace
            char ch = Char.MinValue;
            while (length > 0)
            {
                ch = _strDocument[idx++];
                if (!IsWhiteSpace(ch))
                    break;
                length--;
            }

            if (IsLetter(ch))
                return Symbol.Text;
            if (ch == '\\')
                return PeekKeyword(idx);
            return PeekPunctuation(idx - 1);
        }

        /// <summary>
        /// Reads either text or \keyword from current position.
        /// </summary>
        internal Symbol ReadText(bool rootLevel)
        {
            // Previous call encountered an empty line.
            if (_emptyLine)
            {
                _emptyLine = false;
                Symbol = Symbol.EmptyLine;
                TokenType = TokenType.None;
                Token = "";
                return Symbol.EmptyLine;
            }

            // Init for scanning.
            _prevSymbol = Symbol;
            Symbol = Symbol.None;
            TokenType = TokenType.None;
            Token = "";

            // Save where we are
            SaveCurDocumentPos();

            // Check for EOF.
            if (_currChar == Chars.Null)
            {
                Symbol = Symbol.Eof;
                return Symbol.Eof;
            }

            // Check for keyword or escaped character.
            if (_currChar == '\\')
            {
                switch (_nextChar)
                {
                    case '\\':
                    case '{':
                    case '}':
                    case '/':
                    case '-':
                        return ReadPlainText(rootLevel);
                }
                // Either key word or syntax error.
                Token = "\\";
                return ScanKeyword();
            }

            // Check for reserved terminal symbols in text.
            switch (_currChar)
            {
                case '{':
                    AppendAndScanNextChar();
                    Symbol = Symbol.BraceLeft;
                    TokenType = TokenType.OperatorOrPunctuator;
                    return Symbol.BraceLeft;  // Syntax error in any case.

                case '}':
                    AppendAndScanNextChar();
                    Symbol = Symbol.BraceRight;
                    TokenType = TokenType.OperatorOrPunctuator;
                    return Symbol.BraceRight;
            }

            // Check for end of line.
            if (_currChar == Chars.LF)
            {
                // The line ends here. See if the paragraph continues in the next line.
                if (MoveToNextParagraphContentLine(rootLevel))
                {
                    // Paragraph continues in next line. Simulate the read of a blank to separate words.
                    Token = " ";
                    if (IgnoreLineBreak())
                        Token = "";
                    Symbol = Symbol.Text;
                    return Symbol.Text;
                }
                else
                {
                    // Paragraph ends here. Return NewLine or BraceRight.
                    if (_currChar != Chars.BraceRight)
                    {
                        Symbol = Symbol.EmptyLine;
                        TokenType = TokenType.None; //???
                        return Symbol.EmptyLine;
                    }
                    else
                    {
                        AppendAndScanNextChar();
                        Symbol = Symbol.BraceRight;
                        TokenType = TokenType.OperatorOrPunctuator;
                        return Symbol.BraceRight;
                    }
                }
            }
            return ReadPlainText(rootLevel);
        }

        /// <summary>
        /// Returns whether the linebreak should be ignored because the previous symbol is already a whitespace.
        /// </summary>
        bool IgnoreLineBreak()
        {
            return _prevSymbol switch
            {
                Symbol.LineBreak or Symbol.Space or Symbol.Tab => true,
                _ => false
            };
        }

        /// <summary>
        /// Read text from current position until block ends or \keyword occurs.
        /// </summary>
        Symbol ReadPlainText(bool rootLevel)
        {
            bool foundSpace = false;
            bool loop = true;
            while (loop && _currChar != Chars.Null)
            {
                // Check for escaped character or keyword.
                if (_currChar == '\\')
                {
                    switch (_nextChar)
                    {
                        case '\\':
                        case '{':
                        case '}':
                        case '/':
                            ScanNextChar();
                            AppendAndScanNextChar();
                            break;

                        case '-':
                            // Treat \- as soft hyphen.
                            ScanNextChar();
                            // Fake soft hyphen and go on as usual.
                            _currChar = Chars.SoftHyphen;
                            break;

                        // Keyword
                        default:
                            loop = false;
                            break;
                    }

                    continue;
                }

                // Check for reserved terminal symbols in text
                switch (_currChar)
                {
                    case '{':
                        // Syntax error any way
                        loop = false;
                        continue;

                    case '}':
                        // Block end
                        loop = false;
                        continue;

                    case '/':
                        if (_nextChar != '/')
                            goto ValidCharacter;
                        ScanToEol();
                        break;
                }

                // Check for end of line.
                if (_currChar == Chars.LF)
                {
                    // The line ends here. See if the paragraph continues in the next line.
                    if (MoveToNextParagraphContentLine(rootLevel))
                    {
                        // Paragraph continues in next line. Add a blank to separate words.
                        if (!Token.EndsWith(" ", StringComparison.Ordinal))
                            Token += ' ';
                        continue;
                    }
                    else
                    {
                        // Paragraph ends here. Remember that for next call except the reason
                        // for end is '}'
                        _emptyLine = _currChar != Chars.BraceRight;
                        break;
                    }
                }

            ValidCharacter:
                // Compress multiple blanks to one
                if (_currChar == ' ')
                {
                    if (foundSpace)
                    {
                        ScanNextChar();
                        continue;
                    }

                    foundSpace = true;
                }
                else
                    foundSpace = false;

                AppendAndScanNextChar();
            }

            Symbol = Symbol.Text;
            TokenType = TokenType.Text;
            return Symbol.Text;
        }

        /// <summary>
        /// Moves to the next DDL token if Symbol is not set to a valid position.
        /// </summary>
        internal Symbol MoveToCode()
        {
            if (Symbol is Symbol.None or Symbol.CR /*|| this .symbol == Symbol.comment*/)
                ReadCode();
            return Symbol;
        }

        /// <summary>
        /// Moves to the first character the content of a paragraph starts with. Empty lines
        /// and comments are skipped. Returns true if such a character exists, and false if the
        /// paragraph ends without content.
        /// </summary>
        internal bool MoveToParagraphContent()
        {
        Again:
            MoveToNonWhiteSpace();
            if (_currChar == Chars.Slash && _nextChar == Chars.Slash)
            {
                MoveBeyondEol();
                goto Again;
            }
            return _currChar != Chars.BraceRight;
        }

        /// <summary>
        /// Moves to the first character of the content of a paragraph beyond an EOL. 
        /// Returns true if such a character exists and belongs to the current paragraph.
        /// Returns false if a new line (at root level) or '}' occurs. If a new line caused
        /// the end of the paragraph, the DDL cursor is moved to the next valid content
        /// character or '}' respectively.
        /// </summary>
        internal bool MoveToNextParagraphContentLine(bool rootLevel)
        {
            Debug.Assert(_currChar == Chars.LF);
            bool loop = true;
            ScanNextChar();
            while (loop)
            {
                // Scan to next EOL and ignore any white space.
                MoveToNonWhiteSpaceOrEol();
                switch (_currChar)
                {
                    case Chars.Null:
                        loop = false;
                        break;

                    case Chars.LF:
                        ScanNextChar(); // Read beyond EOL
                        if (rootLevel)
                        {
                            // At nesting level 0 (root level) a new line ends the paragraph content.
                            // Move to next content block or '}' respectively.
                            MoveToParagraphContent();
                            return false;
                        }
                        else
                        {
                            // Skip new lines at the end of the paragraph.
                            if (PeekSymbol() == Symbol.BraceRight)
                            {
                                MoveToNonWhiteSpace();
                                return false;
                            }

                            //TODO_OLD NiSc NYI
                            //Check.NotImplemented("empty line at non-root level");
                        }
                        break;

                    case Chars.Slash:
                        if (_nextChar == Chars.Slash)
                        {
                            // A line with comment is not treated as empty.
                            // Skip this line.
                            MoveBeyondEol();
                        }
                        else
                        {
                            // Current character is a slash.
                            return true;
                        }
                        break;

                    case Chars.BraceRight:
                        return false;

                    default:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the current character is not a white space, the function immediately returns it.
        /// Otherwise, the DDL cursor is moved forward to the first non-white space or EOF.
        /// White spaces are SPACE, HT, VT, CR, and LF.???
        /// </summary>
        internal char MoveToNonWhiteSpaceOrEol()
        {
            while (_currChar != Chars.Null)
            {
                switch (_currChar)
                {
                    case Chars.Space:
                    case Chars.HT:
                    case Chars.VT:
                        ScanNextChar();
                        break;

                    default:
                        return _currChar;
                }
            }
            return _currChar;
        }

        /// <summary>
        /// If the current character is not a white space, the function immediately returns it.
        /// Otherwise, the DDL cursor is moved forward to the first non-white space or EOF.
        /// White spaces are SPACE, HT, VT, CR, and LF.
        /// </summary>
        internal char MoveToNonWhiteSpace()
        {
            while (_currChar != Chars.Null)
            {
                switch (_currChar)
                {
                    case Chars.Space:
                    case Chars.HT:
                    case Chars.VT:
                    case Chars.CR:
                    case Chars.LF:
                        ScanNextChar();
                        break;

                    default:
                        return _currChar;
                }
            }
            return _currChar;
        }

        /// <summary>
        /// Moves to the first character beyond the next EOL. 
        /// </summary>
        internal void MoveBeyondEol()
        {
            // Similar to ScanSingleLineComment but do not scan the token.
            ScanNextChar();
            while (_currChar != Chars.Null && _currChar != Chars.LF)
                ScanNextChar();
            ScanNextChar(); // read beyond EOL
        }

        /// <summary>
        /// Reads a single line comment.
        /// </summary>
        internal Symbol ScanSingleLineComment()
        {
            char ch = ScanNextChar();
            while (ch != Chars.Null && ch != Chars.LF)
            {
                Token += _currChar;
                ch = ScanNextChar();
            }
            ScanNextChar(); // read beyond EOL
            return Symbol.Comment;
        }

        /// <summary>
        /// Gets the current symbol.
        /// </summary>
        internal Symbol Symbol { get; private set; } = Symbol.None;

        /// <summary>
        /// Gets the current token type.
        /// </summary>
        internal TokenType TokenType { get; private set; } = TokenType.None;

        /// <summary>
        /// Gets the current token.
        /// </summary>
        internal string Token { get; private set; } = "";

        /// <summary>
        /// Interpret current token as integer literal.
        /// </summary>
        internal int GetTokenValueAsInt()
        {
            if (Symbol == Symbol.IntegerLiteral)
                return Int32.Parse(Token, CultureInfo.InvariantCulture);

            if (Symbol == Symbol.HexIntegerLiteral)
            {
                string number = Token.Substring(2);
                return Int32.Parse(number, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
            Debug.Assert(false, "Should not come here.");
            return 0;
        }

        /// <summary>
        /// Interpret current token as unsigned integer literal.
        /// </summary>
        internal uint GetTokenValueAsUInt()
        {
            if (Symbol == Symbol.IntegerLiteral)
                return UInt32.Parse(Token, CultureInfo.InvariantCulture);

            if (Symbol == Symbol.HexIntegerLiteral)
            {
                string number = Token.Substring(2);
                return UInt32.Parse(number, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
            Debug.Assert(false, "Should not come here.");
            return 0;
        }

        /// <summary>
        /// Interpret current token as real literal.
        /// </summary>
        internal double GetTokenValueAsReal() 
            => Double.Parse(Token, CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the current character or EOF.
        /// </summary>
        internal char Char => _currChar;

        /// <summary>
        /// Gets the character after the current character or EOF.
        /// </summary>
        internal char NextChar => _nextChar;

        /// <summary>
        /// Move DDL cursor one character further.
        /// </summary>
        internal char ScanNextChar()
        {
            if (_ddlLength <= _idx)
            {
                _currChar = Chars.Null;
                _nextChar = Chars.Null;
            }
            else
            {
            SkipChar:
                _currChar = _strDocument[_idx++];
                _nextChar = _ddlLength <= _idx ? Chars.Null : _strDocument[_idx];

                ++_idxLinePos;
                switch (_currChar)
                {
                    case Chars.Null:  //???
                        ++_idxLine;
                        _idxLinePos = 0;
                        break;

                    // Ignore CR
                    case Chars.CR:
                        if (_nextChar == Chars.LF)
                        {
                            goto SkipChar;
                        }
                        //else
                        //{
                        //    //TOxDO NiSc NYI
                        //    //NYI: MacOS uses CR only
                        //    //Check.NotImplemented();
                        //}
                        break;

                    case Chars.LF:
                        //NYI: Unix uses LF only
                        _idxLine++;
                        _idxLinePos = 0;
                        break;
                }
            }
            return _currChar;
        }

        /// <summary>
        /// Move DDL cursor to the next EOL (or EOF).
        /// </summary>
        internal void ScanToEol()
        {
            while (!IsEof(_currChar) && _currChar != Chars.LF)
                ScanNextChar();
        }

        /// <summary>
        /// Appends current character to the token and reads next character.
        /// </summary>
        internal char AppendAndScanNextChar()
        {
            Token += _currChar;
            return ScanNextChar();
        }

        /// <summary>
        /// Appends all next characters to current token until end of line or end of file is reached.
        /// CR/LF or EOF is not part of the token.
        /// </summary>
        internal void AppendAndScanToEol()
        {
            char ch = ScanNextChar();
            while (ch != Chars.Null && ch != Chars.CR && ch != Chars.LF)
            {
                Token += _currChar;
                ch = ScanNextChar();
            }
        }

        /// <summary>
        /// Is character in '0' ... '9'.
        /// </summary>
        internal static bool IsDigit(char ch) 
            => Char.IsDigit(ch);

        /// <summary>
        /// Is character a hexadecimal digit.
        /// </summary>
        internal static bool IsHexDigit(char ch) 
            => Char.IsDigit(ch) || ch is >= 'A' and <= 'F' || ch is >= 'a' and <= 'f';

        /// <summary>
        /// Is character an octal digit.
        /// </summary>
        internal static bool IsOctDigit(char ch)
            => Char.IsDigit(ch) && ch < '8';

        /// <summary>
        /// Is character an alphabetic letter.
        /// </summary>
        internal static bool IsLetter(char ch) => Char.IsLetter(ch);

        /// <summary>
        /// Is character a white space.
        /// </summary>
        internal static bool IsWhiteSpace(char ch) => Char.IsWhiteSpace(ch);

        /// <summary>
        /// Is character an identifier character. First character can be letter or underscore, following
        /// letters, digits, or underscores.
        /// </summary>
        internal static bool IsIdentifierChar(char ch, bool firstChar) //IsId..Char
        {
            if (firstChar)
                return Char.IsLetter(ch) | ch == '_';

            return Char.IsLetterOrDigit(ch) | ch == '_';
        }

        /// <summary>
        /// Is character the end of file character.
        /// </summary>
        internal static bool IsEof(char ch) => ch == Chars.Null;

        //internal bool IsNumber();
        //internal bool IsFormat();
        //internal bool IsParagraphFormat(Symbol* _docSym /*= null*/);
        //internal bool IsField();
        //internal bool IsFieldSpecifier();
        //internal bool IsSymbol();
        ////bool IsSymbolSpecifier();
        //internal bool IsFootnote();
        //internal bool IsComment();
        //internal bool IsInlineShape();
        //
        //internal bool IsValueSymbol();
        //internal bool IsScriptSymbol(Symbol _docSym);
        //internal bool IsParagraphToken();
        //internal bool IsExtendedParagraphToken();
        //internal bool IsParagraphElement();
        //internal bool IsHardHyphen();
        //internal bool IsNewLine();
        //internal bool IsWhiteSpace(Symbol _docSym);

        /// <summary>
        /// Determines whether the given symbol is a valid keyword for a document element.
        /// </summary>
        static bool IsDocumentElement(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.Paragraph:
                case Symbol.Table:
                case Symbol.Image:
                case Symbol.TextFrame:
                case Symbol.Chart:
                case Symbol.PageBreak:
                case Symbol.Barcode:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the given symbol is a valid keyword for a section element.
        /// </summary>
        internal static bool IsSectionElement(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.Paragraph:
                case Symbol.Table:
                case Symbol.Image:
                case Symbol.TextFrame:
                case Symbol.Chart:
                case Symbol.PageBreak:
                case Symbol.Barcode:
                case Symbol.Header:
                case Symbol.PrimaryHeader:
                case Symbol.FirstPageHeader:
                case Symbol.EvenPageHeader:
                case Symbol.Footer:
                case Symbol.PrimaryFooter:
                case Symbol.FirstPageFooter:
                case Symbol.EvenPageFooter:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the given symbol is a valid keyword for a paragraph element.
        /// </summary>
        internal static bool IsParagraphElement(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.Blank:
                case Symbol.Bold:
                case Symbol.Italic:
                case Symbol.Underline:
                case Symbol.Font:
                case Symbol.FontColor:
                case Symbol.FontSize:
                case Symbol.Field:
                case Symbol.Hyperlink:
                case Symbol.Footnote:
                case Symbol.Image:
                case Symbol.Tab:
                case Symbol.SoftHyphen:
                case Symbol.Space:
                case Symbol.Symbol:
                case Symbol.Chr:
                case Symbol.LineBreak:
                case Symbol.Text:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the given symbol is a valid keyword for a header or footer element.
        /// </summary>
        internal static bool IsHeaderFooterElement(Symbol symbol)
        {
            // All paragraph elements.
            if (IsParagraphElement(symbol))
                return true;

            // All document elements except page break.
            if (IsDocumentElement(symbol))
            {
                if (symbol == Symbol.PageBreak)
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the given symbol is a valid keyword for a footnote element.
        /// </summary>
        internal static bool IsFootnoteElement(Symbol symbol)
        {
            // All paragraph elements except footnote.
            if (IsParagraphElement(symbol))
            {
                if (symbol == Symbol.Footnote)
                    return false; // Nested Footnotes are invalid.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the current filename of the document.
        /// </summary>
        internal string DocumentFileName { get; }

        /// <summary>
        /// Gets the current path of the document.
        /// </summary>
        internal string DocumentPath { get; }

        /// <summary>
        /// Gets the current scanner line in the document.
        /// </summary>
        internal int CurrentLine { get; private set; }

        /// <summary>
        /// Gets the current scanner column in the document.
        /// </summary>
        internal int CurrentLinePos { get; private set; }

        /// <summary>
        /// Scans an identifier.
        /// </summary>
        Symbol ScanIdentifier()
        {
            char ch = AppendAndScanNextChar();
            while (IsIdentifierChar(ch, false))
                ch = AppendAndScanNextChar();

            return Symbol.Identifier;
        }

        /// <summary>
        /// Scans an integer or real literal.
        /// </summary>
        Symbol ScanNumber(bool mantissa)
        {
            char ch = _currChar;
            Token += _currChar;

            ScanNextChar();
            if (!mantissa && ch == '0' && (_currChar == 'x' || _currChar == 'X'))
                return ReadHexNumber();

            while (_currChar != Chars.Null)
            {
                if (IsDigit(_currChar))
                    AppendAndScanNextChar();
                else if (!mantissa && _currChar == Chars.Period)
                {
                    //token += currChar;
                    return ScanNumber(true);
                }
                else //if (!IsIdentifierChar(currChar))
                    break;
                //else
                //  THROW_COMPILER_ERROR (COMPERR_LEX_NUMBER);
            }
            return mantissa ? Symbol.RealLiteral : Symbol.IntegerLiteral;
        }

        /// <summary>
        /// Scans a hexadecimal literal.
        /// </summary>
        Symbol ReadHexNumber()
        {
            Token = "0x";
            ScanNextChar();
            while (_currChar != Chars.Null)
            {
                if (IsHexDigit(_currChar))
                    AppendAndScanNextChar();
                else if (!IsIdentifierChar(_currChar, false)) //???
                    break;
                else
                    //THROW_COMPILER_ERROR (COMPERR_LEX_NUMBER);
                    AppendAndScanNextChar();
            }
            return Symbol.HexIntegerLiteral;
        }

        /// <summary>
        /// Scans a DDL keyword that starts with a backslash.
        /// </summary>
        Symbol ScanKeyword()
        {
            char ch = ScanNextChar();

            // \- is a soft hyphen == char(173).
            if (ch == '-')
            {
                Token += "-";
                ScanNextChar();
                return Symbol.SoftHyphen;
            }

            // \( is a shortcut for symbol.
            if (ch == '(')
            {
                Token += "(";
                Symbol = Symbol.Chr;
                return Symbol.Chr; // Shortcut for \chr(
            }

#if false
            // \/ is an escaped slash.
            if (ch == '/')
            {
                _token += "/";
                _symbol = Symbol.Chr;
                return Symbol.Chr;
            }
#endif
            while (!IsEof(ch) && IsIdentifierChar(ch, false))
                ch = AppendAndScanNextChar();

            Symbol = KeyWords.SymbolFromName(Token);
            return Symbol;
        }

        /// <summary>
        /// Scans punctuator terminal symbols.
        /// </summary>
        Symbol ScanPunctuator()
        {
            var sym = Symbol.None;
            switch (_currChar)
            {
                case '{':
                    sym = Symbol.BraceLeft;
                    break;

                case '}':
                    sym = Symbol.BraceRight;
                    break;

                case '[':
                    sym = Symbol.BracketLeft;
                    break;

                case ']':
                    sym = Symbol.BracketRight;
                    break;

                case '(':
                    sym = Symbol.ParenLeft;
                    break;

                case ')':
                    sym = Symbol.ParenRight;
                    break;

                case ':':
                    sym = Symbol.Colon;
                    break;

                case ';':
                    sym = Symbol.Semicolon;
                    break;

                case '.':
                    sym = Symbol.Dot;
                    break;

                case ',':
                    sym = Symbol.Comma;
                    break;

                case '%':
                    sym = Symbol.Percent;
                    break;

                case '$':
                    sym = Symbol.Dollar;
                    break;

                case '@':
                    sym = Symbol.At;
                    break;

                case '#':
                    sym = Symbol.Hash;
                    break;

                //case '?':
                //  sym = Symbol.Question;
                //  break;

                case '¤':
                    sym = Symbol.Currency; //??? used in DDL?
                    break;

                //case '|':
                //  sym = Symbol.Bar;
                //  break;

                case '=':
                    sym = Symbol.Assign;
                    break;

                case '/':
                    sym = Symbol.Slash;
                    break;

                case '\\':
                    sym = Symbol.BackSlash;
                    break;

                case '+':
                    if (_nextChar == '=')
                    {
                        Token += _currChar;
                        ScanNextChar();
                        sym = Symbol.PlusAssign;
                    }
                    else
                        sym = Symbol.Plus;
                    break;

                case '-':
                    if (_nextChar == '=')
                    {
                        Token += _currChar;
                        ScanNextChar();
                        sym = Symbol.MinusAssign;
                    }
                    else
                        sym = Symbol.Minus;
                    break;

                case Chars.CR:
                    sym = Symbol.CR;
                    break;

                case Chars.LF:
                    sym = Symbol.LF;
                    break;

                case Chars.Space:
                    sym = Symbol.Blank;
                    break;

                case Chars.Null:
                    sym = Symbol.Eof;
                    return sym;
            }
            Token += _currChar;
            ScanNextChar();
            return sym;
        }

        //    protected Symbol ReadValueIdentifier();
        ///// <summary>
        ///// Scans string literals used as identifiers.
        ///// </summary>
        //protected string ReadRawString()  //ScanStringLiteralIdentifier
        //{
        //  string str = "";
        //  char ch = ScanNextChar();
        //  while (!IsEof(ch))
        //  {
        //    if (ch == Chars.QuoteDbl)
        //    {
        //      if (nextChar == Chars.QuoteDbl)
        //      {
        //        str += ch;
        //        ch = ScanNextChar();
        //      }
        //      else
        //        break;
        //    }
        //
        //    str += ch;
        //    ch = ScanNextChar();
        //  }
        //
        //  ScanNextChar();
        //  return str;
        //}

        /// <summary>
        /// Scans verbatim strings like «@"String with ""quoted"" text"».
        /// </summary>
        string ScanVerbatimStringLiteral()
        {
            string str = "";
            char ch = ScanNextChar();
            while (!IsEof(ch))
            {
                if (ch == Chars.QuoteDbl)
                {
                    if (_nextChar == Chars.QuoteDbl)
                        ch = ScanNextChar();
                    else
                        break;
                }

                str += ch;
                ch = ScanNextChar();
            }
            ScanNextChar();
            return str;
        }

        /// <summary>
        /// Scans regular string literals like «"String with \"escaped\" text"».
        /// </summary>
        string ScanStringLiteral()
        {
            Debug.Assert(Char == '\"');
            var str = new StringBuilder();
            ScanNextChar();
            while (_currChar != Chars.QuoteDbl && !IsEof(_currChar))
            {
                if (_currChar == '\\')
                {
                    ScanNextChar(); // read escaped characters
                    switch (_currChar)
                    {
                        case 'a':
                            str.Append('\a');
                            break;

                        case 'b':
                            str.Append('\b');
                            break;

                        case 'f':
                            str.Append('\f');
                            break;

                        case 'n':
                            str.Append('\n');
                            break;

                        case 'r':
                            str.Append('\r');
                            break;

                        case 't':
                            str.Append('\t');
                            break;

                        case 'v':
                            str.Append('\v');
                            break;

                        case '\'':
                            str.Append('\'');
                            break;

                        case '\"':
                            str.Append('\"');
                            break;

                        case '\\':
                            str.Append('\\');
                            break;

                        case 'x':
                            {
                                ScanNextChar();
                                int hexNrCount = 0;
                                //string hexString = "0x";
                                while (IsHexDigit(_currChar))
                                {
                                    ++hexNrCount;
                                    //hexString += _currChar;
                                    ScanNextChar();
                                }

                                if (hexNrCount <= 2)
                                    str.Append("?????"); //(char)AscULongFromHexString(hexString);
                                else
                                    throw TH.DdlParserException_EscapeSequenceNotAllowed(str.ToString());
                            }
                            break;

                        //NYI: octal numbers
                        //case '0':
                        //{
                        //  ScanNextChar();
                        //  int hexNrCount = 0;
                        //  string hexString = "0x";
                        //  while (IsOctDigit(currChar))
                        //  {
                        //    ++hexNrCount;
                        //    hexString += currChar;
                        //    ScanNextChar();
                        //  }
                        //  if (hexNrCount <=2)
                        //    str += "?????"; //(char)AscULongFromHexString(hexString);
                        //  else
                        //    throw new DdlParserException(DdlErrorLevel.Error, "DdlScanner",DomMsgID.EscapeSequenceNotAllowed, null);
                        //}
                        //  break;

                        default:
                            throw TH.DdlParserException_EscapeSequenceNotAllowed(str.ToString());
                    }
                }
                else if (_currChar is Chars.Null or Chars.CR or Chars.LF)
                    throw TH.DdlParserException_NewlineInString();
                else
                    str.Append(_currChar);

                ScanNextChar();
            }
            ScanNextChar();  // read '"'
            return str.ToString();
        }

        /// <summary>
        /// Save the current scanner location in the document for error handling.
        /// </summary>
        void SaveCurDocumentPos()
        {
            //_curDocumentIndex = _idx - 1; Not (yet?) used.
            CurrentLine = _idxLine;
            CurrentLinePos = _idxLinePos;
        }

        //int _curDocumentIndex; Not(yet?) used.

        readonly string _strDocument;
        readonly int _ddlLength;
        int _idx;
        int _idxLine;
        int _idxLinePos;

        char _currChar;
        char _nextChar;
        Symbol _prevSymbol = Symbol.None;
        bool _emptyLine;

        //DdlReaderErrors _errors; Not (yet?) used.
    }
}
