// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Internal;
using PdfSharp.Logging;
using PdfSharp.Pdf.Internal;
using PdfSharp.Internal.Logging;

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Lexical analyzer for PDF files. Technically a PDF file is a stream of bytes. Some chunks
    /// of bytes represent strings in several encodings. The actual encoding depends on the
    /// context where the string is used. Therefore, the bytes are 'raw encoded' into characters,
    /// i.e. a character or token read by the Lexer has always character values in the range from
    /// 0 to 255.
    /// </summary>
    class Lexer
    {
        /// <summary>
        /// Initializes a new instance of the Lexer class.
        /// </summary>
        public Lexer(Stream pdfInputStream, ILogger? logger)
        {
            _pdfStream = pdfInputStream;
            _pdfLength = (SizeType)_pdfStream.Length;
            _idxChar = 0;
            Position = 0;
            _logger = logger ?? LogHost.CreateLogger(LogCategory.PdfReading);
        }

        /// <summary>
        /// Gets or sets the position within the PDF stream.
        /// </summary>
        public SizeType Position
        {
            get
            {
                Debug.Assert(_pdfStream.Position == _idxChar + 2);
                return _idxChar;
            }
            set
            {
                Debug.Assert(value >= 0);
                _idxChar = value;
                _pdfStream.Position = value;

                // ReadByte return -1 (eof) at the end of the stream.
                _currChar = (char)_pdfStream.ReadByte();
                _nextChar = (char)_pdfStream.ReadByte();
                ClearToken();
            }
        }

        /// <summary>
        /// Reads the next token and returns its type. If the token starts with a digit, the parameter
        /// testForObjectReference specifies how to treat it. If it is false, the Lexer scans for a single integer.
        /// If it is true, the Lexer checks if the digit is the prefix of a reference. If it is a reference,
        /// the token is set to the object ID followed by the generation number separated by a blank
        /// (the 'R' is omitted from the token).
        /// </summary>
        // /// <param name="testReference">Indicates whether to test the next token if it is a reference.</param>
        public Symbol ScanNextToken(bool testForObjectReference)
        {
        TryAgain:
            ClearToken();

            char ch = MoveToNonWhiteSpace();
            switch (ch)
            {
                case '%':
                    // Eat comments, the parser doesn't handle them.
                    ScanComment();
                    goto TryAgain;

                case '/':
                    return Symbol = ScanName();

                case '+':
                case '-':
                    // Cannot be an object reference if a sign was found.
                    return Symbol = ScanNumber(false);

                case '(':
                    return Symbol = ScanLiteralString();

                case '[':
                    ScanNextChar(true);
                    return Symbol = Symbol.BeginArray;

                case ']':
                    ScanNextChar(true);
                    return Symbol = Symbol.EndArray;

                case '<':
                    if (_nextChar == '<')
                    {
                        ScanNextChar(true);
                        ScanNextChar(true);
                        return Symbol = Symbol.BeginDictionary;
                    }
                    return Symbol = ScanHexadecimalString();

                case '>':
                    if (_nextChar == '>')
                    {
                        ScanNextChar(true);
                        ScanNextChar(true);
                        return Symbol = Symbol.EndDictionary;
                    }
                    //ParserDiagnostics.HandleUnexpectedCharacter(_nextChar);
                    ch = _nextChar;
                    goto default;

                case >= '0' and <= '9':
                    Symbol = ScanNumber(testForObjectReference);
                    Debug.Assert(Symbol is Symbol.Integer or Symbol.LongInteger or Symbol.Real or Symbol.ObjRef);
                    return Symbol;

                case '.':
                    // Cannot be an object reference if a decimal point was found.
                    Symbol = ScanNumber(false);
                    Debug.Assert(Symbol == Symbol.Real);
                    return Symbol;

                case >= 'a' and <= 'z':
                    return Symbol = ScanKeyword();

                case 'R':
                    Debug.Assert(false, "'R' should not be parsed anymore.");
                    // Note: "case 'R':" is not scanned, because it is only used in an object reference.
                    // And object references are now parsed the 'compound symbol' ObjRef.
                    ScanNextChar(true);
                    // The next line only exists for the 'UseOldCode' case in PdfReader.
                    return Symbol = Symbol.R;

                case Chars.EOF:
                    return Symbol = Symbol.Eof;

                default:
                    Debug.Assert(!Char.IsLetter(ch), "I did something wrong. See code below.");
                    ParserDiagnostics.HandleUnexpectedCharacter(ch, DumpNeighborhoodOfPosition());
                    return Symbol = Symbol.None;
            }
        }

        /// <summary>
        /// Reads the raw content of a stream.
        /// A stream longer than 2 GiB is not implemented by design.
        /// </summary>
        public byte[] ReadStream(int length)
        {
            SizeType pos;

            // Skip illegal blanks behind «stream».
            int blanks = 0;
            while (_currChar == Chars.SP)
            {
                blanks++;
                ScanNextChar(true);
            }
            if (blanks > 0)
            {
                // #PRD
                //DiagnosticsHelper.xxx ("Skipped {blanks}x} superfluous blanks behind 'stream' at position xxxx", ...)
            }

            // Skip new line behind «stream».
            if (_currChar == Chars.CR)
            {
                if (_nextChar == Chars.LF)
                    pos = _idxChar + 2;
                else
                    pos = _idxChar + 1;
            }
            else
                pos = _idxChar + 1;

            _pdfStream.Position = pos;
            byte[] bytes = new byte[length];
            int read = _pdfStream.Read(bytes, 0, length);
            Debug.Assert(read == length);
            // With corrupted files, read could be different from length.
            if (bytes.Length != read)
            {
                Array.Resize(ref bytes, read);
            }

            // Synchronize idxChar etc.
            Position = pos + read;
            return bytes;
        }

        /// <summary>
        /// Reads a string in 'raw' encoding.
        /// </summary>
        public string ReadRawString(SizeType position, int length)
        {
            _pdfStream.Position = position;
            var bytes = new byte[length];
            var readBytes = _pdfStream.Read(bytes, 0, length);
            Debug.Assert(readBytes == length);
            return PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Scans a comment line.
        /// </summary>
        public Symbol ScanComment()
        {
            Debug.Assert(_currChar == Chars.Percent);

            ClearToken();
            while (true)
            {
                char ch = AppendAndScanNextChar();
                if (ch is Chars.LF or Chars.EOF)
                    break;
            }
            // TODO: not correct | StLa/24-01-23: Why?
            if (_token.ToString().StartsWith("%%EOF", StringComparison.Ordinal))
                return Symbol.Eof;
            return Symbol = Symbol.Comment;
        }

        /// <summary>
        /// Scans a name.
        /// </summary>
        public Symbol ScanName()
        {
            Debug.Assert(_currChar == Chars.Slash);

            ClearToken();
            while (true)
            {
                var ch = AppendAndScanNextChar();
                if (IsWhiteSpace(ch) || IsDelimiter(ch) || ch == Chars.EOF)
                {
                    break;

                    // DELETE
                    //// Name objects use UTF-8 encoding. We have to decode it here.
                    //var name = Token;

                    //for (int idx = 0; idx < name.Length; ++idx)
                    //{
                    //    // If MSB is set, we need UTF-8 decoding.
                    //    if (name[idx] > 127)
                    //    {
                    //        // Special characters in Name objects use UTF-8 encoding.
                    //        var bytes = new byte[name.Length];
                    //        for (int idx2 = 0; idx2 < name.Length; ++idx2)
                    //        {
                    //            bytes[idx2] = (byte)name[idx2];
                    //        }
                    //        var decodedName = Encoding.UTF8.GetString(bytes);
                    //        _token.Clear();
                    //        _token.Append(decodedName);
                    //        break;
                    //    }
                    //}
                    //return Symbol = Symbol.Name;
                }

                if (ch == '#')
                {
                    ScanNextChar(true);
                    var newChar = (_currChar switch
                    {
                        >= '0' and <= '9' => _currChar - '0',
                        >= 'A' and <= 'F' => _currChar - ('A' - 10),  // Not optimized in IL without parenthesis.
                        >= 'a' and <= 'f' => _currChar - ('a' - 10),
                        _ => LogError(_currChar)
                    } << 4) + _nextChar switch
                    {
                        >= '0' and <= '9' => _nextChar - '0',
                        >= 'A' and <= 'F' => _nextChar - ('A' - 10),
                        >= 'a' and <= 'f' => _nextChar - ('a' - 10),
                        _ => LogError(_nextChar)
                    };
                    ScanNextChar(true);
                    _currChar = (char)newChar;

                    static char LogError(char ch)
                    {
                        LogHost.Logger.LogError("Illegal character {char} in hex string.", ch);
                        return '\0';
                    }
                }
            }

            var name = Token;
            // Check for UTF-8 encoding.
            for (int idx = 0; idx < name.Length; ++idx)
            {
                // If the two top most significant bits are set this identifies a 2, 3, or 4
                // byte UTF-8 encoding sequence.
                if ((name[idx] & 0xC0) == 0xC0)
                {
                    // Special characters in Name objects use UTF-8 encoding.
                    var length = name.Length;
                    var bytes = new byte[length];
                    for (int idx2 = 0; idx2 < length; ++idx2)
                        bytes[idx2] = (byte)name[idx2];

                    var decodedName = Encoding.UTF8.GetString(bytes);
                    _token.Clear();
                    _token.Append(decodedName);
                    break;
                }
            }
            return Symbol = Symbol.Name;
        }

        /// <summary>
        /// Scans a number or an object reference.
        /// Returns one of the following symbols.
        /// Symbol.ObjRef if testForObjectReference is true and the pattern "nnn ggg R" can be found.
        /// Symbol.Real if a decimal point exists or the number of digits is too large for 64-bit integer.
        /// Symbol.Integer if the long value is in the range of 32-bit integer.
        /// Symbol.LongInteger otherwise.
        /// </summary>
        public Symbol ScanNumber(bool testForObjectReference)
        {
            // We found a PDF file created with Acrobat 7 with this entry 
            //   /Checksum 2996984786   # larger than 2.147.483.648 (2^31)
            //
            // Also got an AutoCAD PDF file that contains
            //   /C 264584027963392     # 15 digits
            //
            // So we introduced a LongInteger.

            // Note: This is a copy of CLexer.ScanNumber with minimal changes. Keep both versions in sync as far as possible.
            // Update StL: Function is revised for object reference look ahead.

            const int maxDigitsForObjectNumber = 7;      // max: 8_388_608 / 0x_7F_FF_FF
            const int maxDigitsForGenerationNumber = 5;  // max: 65_535    / 0x_FF_FF
            const int maxDigitsForLong = 18;
            const int maxDecimalDigits = 10;
            var value = 0L;
            var totalDigits = 0;
            var decimalDigits = 0;
            var period = false;
            var negative = false;
            var ch = _currChar;
            Debug.Assert(ch is '+' or '-' or '.' or (>= '0' and <= '9'));

            // If first char is not a digit, it cannot be an object reference.
            if (testForObjectReference && ch is not (>= '0' and <= '9'))
                testForObjectReference = false;
#if DEBUG_
            var pos = Position;
            var neighborhood = GetNeighborhoodOfCurrentPosition(Position);
            Console.WriteLine(neighborhood);
#endif
            ClearToken();
            if (ch is '+' or '-')
            {
                if (ch == '-')
                    negative = true;
                _token.Append(ch);
                ch = ScanNextChar(true);

                // Never saw this in any PDF file, but possible.
                if (ch is not ('.' or >= '0' and <= '9'))
                {
                    LogHost.Logger.LogError("+/- not followed by a number.");
                }
            }

            // Scan the number.
            while (true)
            {
                if (ch is >= '0' and <= '9')
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
                    // More than one period?
                    if (period)
                        ContentReaderDiagnostics.ThrowContentReaderException("More than one period in number.");

                    period = true;
                    _token.Append(ch);
                }
                else
                    break;
                ch = ScanNextChar(true);
            }

            // Can the scanned number be the first part of an object reference?
            if (testForObjectReference && period is false
                && totalDigits <= maxDigitsForObjectNumber
                && _currChar == Chars.SP)
            {
#if DEBUG
                LexerHelper.TryCheckReferenceCount++;
#endif
                int gen = TryReadReference();
                if (gen >= 0)
                {
#if DEBUG
                    LexerHelper.TryCheckReferenceSuccessCount++;
#endif
                    _tokenAsObjectID = ((int)value, gen);
                    return Symbol.ObjRef;
                }
            }

            if (totalDigits > maxDigitsForLong || decimalDigits > maxDecimalDigits)
            {
                // The number is too big for long or has too many decimal digits for our own code,
                // so we provide it as real only.
                // Number will be parsed by .NET.
                _tokenAsReal = Double.Parse(_token.ToString(), CultureInfo.InvariantCulture);
                return Symbol.Real;
            }

            if (negative)
                value = -value;

            if (period)
            {
                if (decimalDigits > 0)
                {
                    _tokenAsReal = value / PowersOf10[decimalDigits];
                }
                else
                {
                    _tokenAsReal = value;
                    _tokenAsLong = value;
                }
                return Symbol.Real;
            }
            _tokenAsLong = value;
            _tokenAsReal = Double.NaN;

            Debug.Assert(Int64.Parse(_token.ToString(), CultureInfo.InvariantCulture) == value);

            if (value is >= Int32.MinValue and < Int32.MaxValue)
                return Symbol.Integer;

            return Symbol.LongInteger;

            // Try to read generation number followed by an 'R'.
            // Returns -1 if not an object reference.
            int TryReadReference()
            {
                Debug.Assert(_currChar == Chars.SP);

                // A Reference has the form "nnn ggg R". The original implementation of the parser used a
                // reduce/shift algorithm in the first place. But this case is the only one we need to
                // look ahead 3 tokens.
                // This is a new implementation that checks whether a scanned integer is followed by
                // another integer and an 'R'. 

                // Save current position and token.
                SizeType position = Position;
                string token = _token.ToString();

                // Space expected.
                if (_currChar != Chars.SP)
                    goto NotAReference;

                // Skip spaces.
                while (_currChar == Chars.SP)
                    ScanNextChar(true);

                // First digit of generation expected.
                if (_currChar is not (>= '0' and <= '9'))
                    goto NotAReference;

                // Read generation number.
                var generationNumber = _currChar - '0';
                ScanNextChar(true);
                int digitCount = 1;
                while (_currChar is >= '0' and <= '9')
                {
                    if (++digitCount > maxDigitsForGenerationNumber)
                        goto NotAReference;
                    generationNumber = generationNumber * 10 + _currChar - '0';
                    ScanNextChar(true);
                }

                // Space expected.
                if (_currChar != Chars.SP)
                    goto NotAReference;

                // Skip spaces.
                while (_currChar == Chars.SP)
                    ScanNextChar(true);

                // "R" expected.
                // We can ignore _nextChar because there is no other valid token that starts with an uppercase letter 'R'.
                if (_currChar != 'R')
                    goto NotAReference;

                ScanNextChar(true);

                return generationNumber;

            NotAReference:
                // Restore stream position.
                Position = position;
                // Restore token because setting position clears it.
                _token.Append(token);
                return -1;
            }
        }
        static readonly double[] PowersOf10 = [1, 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000, 10_000_000_000];

        /// <summary>
        /// Scans a keyword.
        /// </summary>
        public Symbol ScanKeyword()
        {
            ClearToken();
            char ch = _currChar;
            // Scan token.
            while (true)
            {
                if (Char.IsLetter(ch))
                    _token.Append(ch);
                else
                    break;
                ch = ScanNextChar(false);
            }

            // Check known tokens.
            return _token.ToString() switch
            {
                "obj" => Symbol = Symbol.Obj,
                "endobj" => Symbol = Symbol.EndObj,
                "null" => Symbol = Symbol.Null,
                "true" => Symbol = Symbol.Boolean,
                "false" => Symbol = Symbol.Boolean,
                "R" => Symbol = Symbol.R,
                "stream" => Symbol = Symbol.BeginStream,
                "endstream" => Symbol = Symbol.EndStream,
                "xref" => Symbol = Symbol.XRef,
                "trailer" => Symbol = Symbol.Trailer,
                "startxref" => Symbol = Symbol.StartXRef,

                // Anything else is treated as a general keyword. Samples are f or n in iref.
                _ => Symbol = Symbol.Keyword
            };
        }

        /// <summary>
        /// Scans a literal string, contained between "(" and ")".
        /// </summary>
        public Symbol ScanLiteralString()
        {
            // Reference: 3.2.3  String Objects / Page 53
            // Reference: TABLE 3.32  String Types / Page 157

            Debug.Assert(_currChar == Chars.ParenLeft);
            ClearToken();
            int parenLevel = 0;
        RetryAfterSkipIllegalCharacter:
            char ch = ScanNextChar(true); // Inside of a string \r, \n and \r\n without preceding \\ shall be treated as \n.
            
            // Deal with escape characters.
            while (ch != Chars.EOF)
            {
                switch (ch)
                {
                    case '(':
                        parenLevel++;
                        break;

                    case ')':
                        if (parenLevel == 0)
                        {
                            ScanNextChar(false); // The string ended, so ignore \r, \n and \r\n again.
                            goto End;
                        }
                        parenLevel--;
                        break;

                    case '\\':
                        {
                            ch = ScanNextChar(true); // Inside of a string \r, \n and \r\n without preceding \\ shall be treated as \n.
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

                                // AutoCAD PDFs may contain such strings: (\ ) 
                                case ' ':
                                    // #PRD Notify about a string with an escaped blank.
                                    ch = ' ';
                                    break;

                                case Chars.CR:
                                case Chars.LF:
                                    ch = ScanNextChar(true); // Inside of a string \r, \n and \r\n without preceding \\ shall be treated as \n.
                                    continue;

                                default:
                                    // Try scan up to 3 octal digits.
                                    //if (Char.IsDigit(ch) && _nextChar is not '8' and not '9')  // First octal character.
                                    //if (Char.IsDigit(ch) && ch is not '8' and not '9')  // First octal character.
                                    if (ch is >= '0' and <= '7')  // First octal character.
                                    {
                                        //// Octal character code.
                                        //if (ch >= '8')
                                        //    ParserDiagnostics.HandleUnexpectedCharacter(ch);

                                        int n = ch - '0';
                                        //if (Char.IsDigit(_nextChar) && _nextChar is not '8' and not '9')  // Second octal character.
                                        if (_nextChar is >= '0' and <= '7')  // Second octal character.
                                        {
                                            ch = ScanNextChar(true); // Inside of a string \r, \n and \r\n without preceding \\ shall be treated as \n.
                                            //if (ch >= '8')
                                            //    ParserDiagnostics.HandleUnexpectedCharacter(ch);

                                            n = n * 8 + ch - '0';
                                            //if (Char.IsDigit(_nextChar) && _nextChar is not '8' and not '9')  // Third octal character.
                                            if (_nextChar is >= '0' and <= '7')  // Third octal character.
                                            {
                                                ch = ScanNextChar(true); // Inside of a string \r, \n and \r\n without preceding \\ shall be treated as \n.
                                                //if (ch >= '8')
                                                //    ParserDiagnostics.HandleUnexpectedCharacter(ch);

                                                n = n * 8 + ch - '0';
                                            }
                                        }
                                        // #PRD: 8^3 is 512. What if ch is in range [256..511]?
                                        if (n >= 256)
                                        {
                                            // -> Issue a warning??? 
                                        }
                                        ch = (char)n;
                                    }
                                    else
                                    {
                                        // PDF 32000: "If the character following the REVERSE SOLIDUS is not one of those shown in Table 3, the REVERSE SOLIDUS shall be ignored."
                                        // fyi: REVERSE SOLIDUS is a backslash
                                        // What does that mean: "abc\qxyz" is "abcxyz" oder "abcqxyz"?
                                        // #PRD Notify about unknown escape character.
                                        // Debug.As-sert(false, "Not implemented; unknown escape character.");
                                        // ParserDiagnostics.HandleUnexpectedCharacter(ch);
                                        //GetType();
                                        goto RetryAfterSkipIllegalCharacter;
                                    }
                                    break;
                            }
                            break;
                        }

                    default:
                        break;
                }

                _token.Append(ch);
                ch = ScanNextChar(true); // Inside of a string \r, \n and \r\n without preceding \\ shall be treated as \n.
            }


            End:
            return Symbol = Symbol.String;
        }

        /// <summary>
        /// Scans a hex encoded literal string, contained between "&lt;" and "&gt;".
        /// </summary>
        public Symbol ScanHexadecimalString()
        {
            Debug.Assert(_currChar == Chars.Less);

            ClearToken();
            ScanNextChar(true);
            while (true)
            {
                MoveToNonWhiteSpace();
                if (_currChar == '>')
                {
                    ScanNextChar(true);
                    break;
                }
#if true
                var hex = _currChar switch
                {
                    >= '0' and <= '9' => _currChar - '0',
                    >= 'A' and <= 'F' => _currChar - ('A' - 10),  // Not optimized in IL without parenthesis.
                    >= 'a' and <= 'f' => _currChar - ('a' - 10),
                    _ => LogError(_currChar)
                };

                ScanNextChar(true);
                if (_currChar == '>')
                {
                    // Second char is optional in PDF spec.
                    _token.Append((char)(hex << 4));
                    ScanNextChar(true);
                    break;
                }

                hex = (hex << 4) + _currChar switch
                {
                    >= '0' and <= '9' => _currChar - '0',
                    >= 'A' and <= 'F' => _currChar - ('A' - 10),
                    >= 'a' and <= 'f' => _currChar - ('a' - 10),
                    _ => LogError(_currChar)
                };
                _token.Append((char)hex);
                ScanNextChar(true);
#else  // DELETE
                if (Char.IsLetterOrDigit(_currChar))
                {
                    hex[0] = Char.ToUpper(_currChar);
                    // Second char is optional in PDF spec.
                    if (Char.IsLetterOrDigit(_nextChar))
                    {
                        hex[1] = Char.ToUpper(_nextChar);
                        ScanNextChar(true);
                    }
                    else
                    {
                        // We could check for ">" here and throw if we find anything else. The throw comes after the next iteration anyway.
                        hex[1] = '0';
                    }
                    ScanNextChar(true);

                    int ch = Int32.Parse(new string(hex), NumberStyles.AllowHexSpecifier);
                    _token.Append(Convert.ToChar(ch));
                }
                else
                    ParserDiagnostics.HandleUnexpectedCharacter(_currChar, DumpNeighborhoodOfPosition());
#endif
            }

            return Symbol = Symbol.HexString;

            static char LogError(char ch)
            {
                LogHost.Logger.LogError("Illegal character {char} in hex string.", ch);
                return '\0';
            }
        }

        /// <summary>
        /// Move current position one byte further in PDF stream and
        /// return it as a character with high byte always zero.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal char ScanNextChar(bool handleCRLF) // ScanNextByteAsChar
        {
            if (_idxChar >= _pdfLength)
            {
                _currChar = Chars.EOF;
                _nextChar = Chars.EOF;
            }
            else
            {
                _currChar = _nextChar;
                _nextChar = (char)_pdfStream.ReadByte();
                _idxChar++;
                if (handleCRLF && _currChar == Chars.CR)
                {
                    if (_nextChar == Chars.LF)
                    {
                        // Treat CR LF as LF.
                        _currChar = _nextChar;
                        _nextChar = (char)_pdfStream.ReadByte();
                        _idxChar++;
                    }
                    else
                    {
                        // Treat single CR as LF.
                        _currChar = Chars.LF;
                    }
                }
            }
            return _currChar;
        }

        /// <summary>
        /// Resets the current token to the empty string.
        /// </summary>
        void ClearToken() => _token.Clear();

        /// <summary>
        /// Appends current character to the token and
        /// reads next byte as a character.
        /// </summary>
        char AppendAndScanNextChar()
        {
            if (_currChar == Chars.EOF)
                ParserDiagnostics.ThrowParserException("Undetected EOF reached.");

            _token.Append(_currChar);
            return ScanNextChar(true);
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
                        ScanNextChar(true);
                        break;

                    //                    case (char)11:
                    //                    case (char)173:
                    //#warning "Must not be ignored"
                    //                        throw null;
                    //                        ScanNextChar(true);
                    //                        break;

                    default:
                        return _currChar;
                }
            }
            return _currChar;
        }

        /// <summary>
        /// Returns the neighborhood of the specified position as a string.
        /// It supports a human to find this position in a PDF file.
        /// The current position is tagged with a double quotation mark ('‼').
        /// </summary>
        /// <param name="position">The position to show. If it is -1 the current position is used.</param>
        /// <param name="hex">If set to <c>true</c> the string is a hex dump.</param>
        /// <param name="range">The number of bytes around the position to be dumped.</param>
        public string DumpNeighborhoodOfPosition(SizeType position = -1, bool hex = false, int range = 25)
        {
            // Do not use or change the Lexer Position property.
            SizeType originalPosition = (SizeType)_pdfStream.Position;

            // Test edge case calculation.
            //_pdfStream.Position =  5;  
            //_pdfStream.Position = _pdfLength - 5;

            // Note: The _pdfStream Position is mostly two bytes/chars behind the Lexer Position,
            // because the stream already has read the current and the next character.
            if (position < 0)
                position = Position;

            SizeType start = position - range;
            int tagIndex = range;
            int length = 2 * range;

            // Too close to the beginning of the stream?
            if (start < 0)
            {
                tagIndex += (int)start;
                length += (int)start;
                start = 0;
            }

            // Too close to the end of the stream?
            SizeType overhang = _pdfLength - (position + range);
            if (overhang < 0)
                length += (int)overhang;

            _pdfStream.Position = start;
            var bytes = new byte[length];
            _ = _pdfStream.Read(bytes, 0, (int)length);
            _pdfStream.Position = originalPosition;

            var result = new StringBuilder(10 * length);
            for (int idx = 0; idx < length; idx++)
            {
                if (hex)
                {
                    if (idx == tagIndex)
                        result.Append('‼');

                    result.Append(((int)bytes[idx]).ToString("X2"));
                    result.Append(' ');
                }
                else
                {
                    if (idx == tagIndex)
                        result.Append('‼');

                    // Make it more readable:
                    // ‼ tags current position
                    // ◌ for null
                    // ・ for blank
                    // ⟬xx⟭ hex for chars less than space (MATHEMATICAL WHITE TORTOISE SHELL BRACKET)
                    // 
                    // Some chars tried out. Not all chars are shown in Windows Terminal.
                    // ‹›⁌⁍ ‼·٠٠○◌●◙ «»
                    // 〈 RIGHT - POINTING ANGLE BRACKET(U+232A, Pe): 〉
                    // ❰ HEAVY RIGHT-POINTING ANGLE BRACKET ORNAMENT(U + 2771, Pe): ❱
                    var ch = (char)bytes[idx];
                    string s = ch switch
                    {
                        Chars.NUL => "◌",
                        Chars.CR => "〈CR〉",
                        Chars.LF => "〈LF〉",
                        Chars.HT => "〈HT〉",
                        Chars.VT => "〈VT〉",

                        Chars.SP => "·",
                        //Chars.SP => "・",  // U+30FB

                        //<= ' ' => $"⁌((int)ch).ToString(\"X2\")⁍",
                        <= ' ' => $"⟬((int)ch).ToString(\"X2\")⟭",

                        _ => ch.ToString()
                    };
                    result.Append(s);
                }
            }
            result.Append($"\r\nPosition marked with ‼ is {position}");
            return result.ToString();
        }

        /// <summary>
        /// Gets the current symbol.
        /// </summary>
        public Symbol Symbol { get; set; } = Symbol.None;

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public string Token => _token.ToString();

        /// <summary>
        /// Interprets current token as boolean literal.
        /// </summary>
        public bool TokenToBoolean
        {
            get
            {
                Debug.Assert(_token.ToString() == "true" || _token.ToString() == "false");
                return _token[0] == 't';
            }
        }

        /// <summary>
        /// Interprets current token as integer literal.
        /// </summary>
        public int TokenToInteger
        {
            get
            {
                Debug.Assert(_tokenAsLong == Int32.Parse(_token.ToString(), CultureInfo.InvariantCulture));
                return (int)_tokenAsLong;
            }
        }

        /// <summary>
        /// Interprets current token as 64-bit integer literal.
        /// </summary>
        public long TokenToLongInteger
        {
            get
            {
                Debug.Assert(_tokenAsLong == Int64.Parse(_token.ToString(), CultureInfo.InvariantCulture));
                return _tokenAsLong;
            }
        }

        /// <summary>
        /// Interprets current token as real or integer literal.
        /// </summary>
        public double TokenToReal
        {
            get
            {
                // Create double value only if requested.
                if (Double.IsNaN(_tokenAsReal))
                    _tokenAsReal = _tokenAsLong;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Debug.Assert(_tokenAsReal == Double.Parse(_token.ToString(), CultureInfo.InvariantCulture));
                return _tokenAsReal;
            }
        }

        /// <summary>
        /// Interprets current token as a pair of objectNumber and generation
        /// </summary>
        public (int objectNumber, int generationNumber) TokenToObjectID
        {
            get
            {
                Debug.Assert(Symbol == Symbol.ObjRef);
                return _tokenAsObjectID;
            }
        }

        /// <summary>
        /// Indicates whether the specified character is a PDF white-space character.
        /// </summary>
        internal static bool IsWhiteSpace(char ch)
        {
            return ch switch
            {
                Chars.NUL => true, // 0 Null
                Chars.HT => true,  // 9 Horizontal Tab
                Chars.LF => true,  // 10 Line Feed
                Chars.FF => true,  // 12 Form Feed
                Chars.CR => true,  // 13 Carriage Return
                Chars.SP => true,  // 32 Space
                _ => false
            };
        }

        /// <summary>
        /// Indicates whether the specified character is a PDF delimiter character.
        /// </summary>
        internal static bool IsDelimiter(char ch)
        {
            return ch switch
            {
                '(' => true,
                ')' => true,
                '<' => true,
                '>' => true,
                '[' => true,
                ']' => true,
                '{' => true,
                '}' => true,
                '/' => true,
                '%' => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets the length of the PDF output.  
        /// </summary>
        public SizeType PdfLength => _pdfLength;
        readonly SizeType _pdfLength;
        SizeType _idxChar;
        char _currChar;
        char _nextChar;
        readonly StringBuilder _token = new();
        long _tokenAsLong;
        double _tokenAsReal;
        (int, int) _tokenAsObjectID;
        readonly Stream _pdfStream;
        ILogger _logger;

    }

    //#pragma warning disable CS1591 // DELETE
#if DEBUG
    public class LexerHelper
    {
        // Give me an idea of the try/success ratio.
        // To be DELETED

        public static int TryCheckReferenceCount;
        public static int TryCheckReferenceSuccessCount;
    }
#endif
}
