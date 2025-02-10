// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.Internal;

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
            // ReSharper disable once RedundantCast because SizeType can be 32 bit depending on build.
            _pdfLength = (SizeType)_pdfStream.Length;
            _charIndex = 0;
            Position = 0;
            _logger = logger ?? PdfSharpLogHost.PdfReadingLogger;
        }

        /// <summary>
        /// Gets or sets the logical current position within the PDF stream.<br/>
        /// When got, the logical position of the stream pointer is returned.
        /// The actual position in the .NET Stream is two bytes more, because the
        /// reader has a look-ahead of two bytes (_currChar and _nextChar).<br/>
        /// When set, the logical position is set and 2 bytes of look-ahead are red
        /// into _currChar and _nextChar.<br/>
        /// This ensures that immediately getting and setting or setting and getting
        /// is idempotent.
        /// </summary>
        public SizeType Position
        {
            get
            {
                Debug.Assert(_pdfStream.Position == _charIndex + 2);
                return _charIndex;
            }
            set
            {
                Debug.Assert(value >= 0);
                _charIndex = value;
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
                    // Eat comments, the parser doesn’t handle them.
                    ScanComment();
                    goto TryAgain;

                case '/':
                    return ScanName();

                case '+':
                case '-':
                    // Cannot be an object reference if a sign was found.
                    return ScanNumber(false);

                case '(':
                    return ScanStringLiteral();

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
                    return ScanHexadecimalString();

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
                    ScanNumber(testForObjectReference);
                    Debug.Assert(Symbol is Symbol.Integer or Symbol.LongInteger or Symbol.Real or Symbol.ObjRef);
                    return Symbol;

                case '.':
                    // Cannot be an object reference if a decimal point was found.
                    ScanNumber(false);
                    Debug.Assert(Symbol == Symbol.Real);
                    return Symbol;

                case >= 'a' and <= 'z':
                    return ScanKeyword();

#if DEBUG
                case 'R':
                    Debug.Assert(false, "'R' should not be parsed anymore.");
                    // Note: "case 'R':" is not scanned, because it is only used in an object reference.
                    // And object references are now parsed the 'compound symbol' ObjRef.
                    ScanNextChar(true);
                    // The next line only exists for the 'UseOldCode' case in PdfReader.
                    return Symbol = Symbol.R;
#endif

                case Chars.EOF:
                    return Symbol = Symbol.Eof;

                default:
                    Debug.Assert(!Char.IsLetter(ch), "PDFsharp did something wrong. See code below.");
                    ParserDiagnostics.HandleUnexpectedCharacter(ch, DumpNeighborhoodOfPosition());
                    return Symbol = Symbol.None;
            }
        }

        /// <summary>
        /// Reads a string in 'raw' encoding without changing the state of the lexer.
        /// </summary>
        public string RandomReadRawString(SizeType position, int length)
        {
            var oldPosition = _pdfStream.Position;
            var str = ScanRawString(position, length);
            _pdfStream.Position = oldPosition;
            return str;
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
            // If someone writes '%%EOF' somewhere in the document this must not be
            // interpreted as the end of the PDF file.
            if (_token.ToString().StartsWith("%%EOF", StringComparison.Ordinal))
            {
                //return Symbol.Eof; // This is wrong.
                _logger.LogError("Unexpected '%%EOF' read.");
            }
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
                    break;

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
                        PdfSharpLogHost.Logger.LogError("Illegal character {char} in hex string.", ch);
                        return '\0';
                    }
                }
            }

            var name = Token;
            // Check for UTF-8 encoding.
            for (int idx = 0; idx < name.Length; idx++)
            {
                // If the two top most significant bits are set this identifies a 2, 3, or 4
                // byte UTF-8 encoding sequence.
                if ((name[idx] & 0xC0) == 0xC0)
                {
                    // Special characters in Name objects use UTF-8 encoding.
                    var length = name.Length;
                    var bytes = new byte[length];
                    for (int idx2 = 0; idx2 < length; idx2++)
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
        /// Symbol.Real if a decimal point exists or the number is too large for 64-bit integer.
        /// Symbol.Integer if the long value is in the range of 32-bit integer.
        /// Symbol.LongInteger otherwise.
        /// </summary>
        /*public */
        internal Symbol ScanNumber(bool testForObjectReference)
        {
            // We found a PDF file created with Acrobat 7 with this entry 
            //   /Checksum 2996984786   # larger than 2,147,483,648 (2^31)
            //
            // Also got an AutoCAD PDF file that contains
            //   /C 264584027963392     # 15 digits
            //
            // So we introduced a LongInteger.

            // Note: This is a copy of CLexer.ScanNumber with minimal changes. Keep both versions in sync as far as possible.
            // Update StL: Function is revised for object reference look ahead.

            // Parsing Strategy:
            // Most real life numbers in PDF files have less than 19 digits. So we try to parse all digits as 64-bit integers
            // in the first place. All leading zeros are skipped and not counted.
            // If we found a decimal point we later divide the result by the appropriate power of 10 and covert is to Double.
            // For edge cases, which are numbers with 19 digits, the token is parsed again with 'Int64.TryParse()' and
            // if this fails with 'Double.TryParse'.
            // If 'testForObjectReference' is 'true' and the value has up to 7 digits, no decimal point and no sign,
            // we look ahead whether it is an object reference having the form '{object-number} {generation-number} R'.
            const int maxDigitsForObjectNumber = 7;      // max: 8_388_608 / 0x_7F_FF_FF
            const int maxDigitsForGenerationNumber = 5;  // max: 65_535    / 0x_FF_FF
            const int maxDigitsForLong = 18;             // Up to 18 digits values can safely fit into 64-bit integer.
            const int maxDecimalDigits = 10;             // Maximum number of decimal digits we process.
            var value = 0L;                              // The 64-bit value we have parsed.
            var canBeLeadingZero = true;                 // True if a '0' is a leading zero and gets skipped.
            var leadingZeros = 0;                        // The number of scanned leading zeros. Used for optional warning.
            var totalDigits = 0;                         // The total number of digits scanned. E.g. is 7 for '123.4567'.
            var decimalDigits = 0;                       // The total number of decimal digits scanned. E.g. is 4 for '123.4567'.
            //var allDecimalDigitsAreZero = true;        // Not used anymore, because '123.000' is always treated as double, never as integer.
            var period = false;                          // The decimal point '.' was scanned.
            var negative = false;                        // The value is negative and the scanned value is negated.

            var ch = _currChar;
            Debug.Assert(ch is '+' or '-' or '.' or (>= '0' and <= '9'));

            // If the first char is not a digit, it cannot be by definition an object reference.
            if (testForObjectReference && ch is not (>= '0' and <= '9'))
                testForObjectReference = false;

            ClearToken();
            if (ch is '+' or '-')
            {
                // 'testForObjectReference == false' is already ensured here.

                if (ch == '-')
                    negative = true;
                _token.Append(ch);
                ch = ScanNextChar(true);

                // Never saw this in any PDF file.
                if (ch is not ('.' or >= '0' and <= '9'))
                {
                    PdfSharpLogHost.Logger.LogError("+/- not followed by a number or decimal point.");
                }
            }

            // Scan the number.
            while (true)
            {
                if (ch is >= '0' and <= '9')
                {
                    _token.Append(ch);

                    if (canBeLeadingZero)
                    {
                        if (ch == '0')
                        {
                            leadingZeros++;
                        }
                        else
                        {
                            canBeLeadingZero = false;
                            ++totalDigits;
                        }
                    }
                    else
                    {
                        ++totalDigits;
                    }

                    if (decimalDigits < maxDecimalDigits)
                    {
                        // Calculate the value if it still fits into long.
                        // The value gets later parsed again by .NET if the total number of digits exceeds 18 digits.
                        if (totalDigits <= maxDigitsForLong)
                            value = 10 * value + ch - '0';
                    }

                    if (period)
                    {
                        ++decimalDigits;
                        // A number with a decimal point is always considered as Symbol.Real,
                        // even if it fits in Symbol.Integer or Symbol.LongInteger.
                        // KEEP for documental purposes.
                        //// E.g. '123.0000' is real, but fits in an integer.
                        //if (ch is not '0')
                        //    allDecimalDigitsAreZero = false;
                    }
                }
                else if (ch == '.')
                {
                    _token.Append(ch);

                    // More than one period?
                    if (period)
                        ContentReaderDiagnostics.ThrowContentReaderException("More than one decimal point in number.");

                    period = true;
                    canBeLeadingZero = false;
                }
                else
                    break;
                ch = ScanNextChar(true);
            }

#if true_  // KEEP Maybe we warn in the future about leading zeros outside xref.
            // xref has lots of leading zeros.
            // Maybe we add a parameter 'warnAboutLeadingZeros', but not yet.
            if (leadingZeros > 1)
#pragma warning disable CA2254 // This is a rare case outside xref. Therefor we use string interpolation.
                PdfSharpLogHost.PdfReadingLogger.LogWarning($"Token '{_token}' has more than one leading zero.");
#pragma warning restore CA2254
#endif
            // Can the scanned number be the first part of an object reference?
            if (testForObjectReference && period is false
                && totalDigits <= maxDigitsForObjectNumber  // Values in range [8_388_609..9_999_999] are checked in PdfObjectID.
                && IsWhiteSpace(_currChar))
            {
#if DEBUG_
                LexerHelper.TryCheckReferenceCount++;
#endif
                int gen = TryReadReference();
                if (gen >= 0)
                {
#if DEBUG_
                    LexerHelper.TryCheckReferenceSuccessCount++;
#endif
                    _tokenAsObjectID = ((int)value, gen);
                    return Symbol = Symbol.ObjRef;
                }
            }

            if (totalDigits > maxDigitsForLong || decimalDigits > maxDecimalDigits)
            {
                // Case: It is not guarantied that the number fits in a 64-bit integer.

                // It is not integer if there is a period.
                if (period is false && totalDigits == maxDigitsForLong + 1)
                {
                    // Case: We have exactly 19 digits and no decimal point, which might fit in a 64-bit integer,
                    // depending on the value.
                    // If the 19-digit numbers is
                    // in the range [1,000,000,000,000,000,000 .. 9,223,372,036,854,775,807]
                    // or 
                    // in the range [-9,223,372,036,854,775,808 .. -1,000,000,000,000,000,000]
                    // it is a 64-bit integer. Otherwise, it is not.
                    // Because this is a super rare case we make life easy and parse the scanned token again by .NET.
                    if (Int64.TryParse(_token.ToString(), out var result))
                    {
                        _tokenAsLong = result;
                        _tokenAsReal = result;
                        return Symbol = Symbol.LongInteger;
                    }
                }

                // Case: The number is too big for long or has too many decimal digits for our own code,
                // so we provide it as real only.
                // Number will be parsed by .NET.
                _tokenAsReal = Double.Parse(_token.ToString(), CultureInfo.InvariantCulture);

                return Symbol = Symbol.Real;
            }

            // Case: The number is in range [0 .. 999,999,999,999,999,999] and fits into a 64-bit integer.
            if (negative)
                value = -value;  // Flipping 64-bit integer sign is safe here.

            if (period)
            {
                // Case: A number with a period is always considered to be real value.
                if (decimalDigits > 0)
                {
                    _tokenAsReal = value / PowersOf10[decimalDigits];
                    // KEEP for documental purposes.
                    //// It is not integer if there is a period.
                    //if (allDecimalDigitsAreZero)
                    //    _tokenAsLong = (long)_tokenAsReal;
                }
                else
                {
                    _tokenAsReal = value;
                    // KEEP for documental purposes.
                    //// It is not integer if there is a period.
                    // _tokenAsLong = value;
                }
                return Symbol = Symbol.Real;
            }
            _tokenAsLong = value;
            _tokenAsReal = Double.NaN;

            Debug.Assert(Int64.Parse(_token.ToString(), CultureInfo.InvariantCulture) == value);


            if (value is >= Int32.MinValue and <= Int32.MaxValue)
            {
                // Case: Fits in the range of a 32-bit integer.
                return Symbol = Symbol.Integer;
            }

            return Symbol = Symbol.LongInteger;

            // Try to read generation number followed by an 'R'.
            // Returns -1 if not an object reference.
            int TryReadReference()
            {
                Debug.Assert(IsWhiteSpace(_currChar));

                // A Reference has the form "nnn ggg R". The original implementation of the parser used a
                // reduce/shift algorithm in the first place. But this case is the only one we need to
                // look ahead 2 tokens.
                // This is a new implementation that checks whether a scanned integer is followed by
                // another integer and an 'R'. 

                // Save current position and token.
                SizeType position = Position;
                string token = _token.ToString();

                // White-space expected.
                if (!IsWhiteSpace(_currChar))
                    goto NotAReference;

                // Skip white-spaces.
                while (IsWhiteSpace(_currChar))
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

                // White-space expected.
                if (!IsWhiteSpace(_currChar))
                    goto NotAReference;

                // Skip white-spaces.
                while (IsWhiteSpace(_currChar))
                    ScanNextChar(true);

                // "R" expected.
                // We can ignore _nextChar because there is no other valid token that starts with an uppercase letter 'R'.
                if (_currChar != 'R')
                    goto NotAReference;

                // Eat the 'R'.
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
                // ReSharper disable StringLiteralTypo
                "obj" => Symbol = Symbol.Obj,
                "endobj" => Symbol = Symbol.EndObj,
                "null" => Symbol = Symbol.Null,
                "true" => Symbol = Symbol.Boolean,
                "false" => Symbol = Symbol.Boolean,
                "R" => Symbol = Symbol.R,  // Not scanned anymore because it is handled in ScanNumber.
                "stream" => Symbol = Symbol.BeginStream,
                "endstream" => Symbol = Symbol.EndStream,
                "xref" => Symbol = Symbol.XRef,
                "trailer" => Symbol = Symbol.Trailer,
                "startxref" => Symbol = Symbol.StartXRef,
                // ReSharper restore StringLiteralTypo

                // Anything else is treated as a general keyword. Samples are f or n in iref.
                _ => Symbol = Symbol.Keyword
            };
        }

        /// <summary>
        /// Scans a string literal, contained between "(" and ")".
        /// </summary>
        public Symbol ScanStringLiteral()
        {
            // Reference: 3.2.3  String Objects / Page 53
            // Reference: TABLE 3.32  String Types / Page 157

            Debug.Assert(_currChar == Chars.ParenLeft);
            ClearToken();
            int parenLevel = 0;
            //RetryAfterSkipIllegalCharacter:
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
                                        // Adobe Reader ignores '\', but keeps 'q'. We do the same.
                                        // #PRD Notify about unknown escape character.
                                        // Debug.As-sert(false, "Not implemented; unknown escape character.");
                                        // ParserDiagnostics.HandleUnexpectedCharacter(ch);
                                        PdfSharpLogHost.PdfReadingLogger.LogWarning($"Illegal escape sequence '\\{ch}' found. Reverse solidus (backslash) is ignored.");
                                    }
                                    break;
                            }
                            break;
                        }
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
            }

            return Symbol = Symbol.HexString;

            static char LogError(char ch)
            {
                PdfSharpLogHost.Logger.LogError("Illegal character {char} in hex string.", ch);
                return '\0';
            }
        }

        /// <summary>
        /// Tries to scan the specified literal from the current stream position.
        /// </summary>
        public bool TryScanLiterally(string literal)
        {
            var initialPosition = Position;

            foreach (var expectedChar in literal)
            {
                if (_currChar != expectedChar)
                {
                    // Restore initial position, if no success.
                    Position = initialPosition;
                    return false;
                }
                ScanNextChar(false);
            }
            return true;
        }

        /// <summary>
        /// Return the exact position where the content of the stream starts.
        /// The logical position is also set to this value when the function returns.<br/>
        /// Reference:     3.2.7  Stream Objects / Page 60<br/>
        /// Reference 2.0: 7.3.8  Stream objects / Page 31
        /// </summary>
        public SizeType FindStreamStartPosition(PdfObjectID id)
        {
            // Quote from Reference 2.0:
            // The keyword stream that follows the stream dictionary shall be followed by an
            // end-of-line marker consisting of either a CARRIAGE RETURN and a LINE FEED
            // or just a LINE FEED, and not by a CARRIAGE RETURN alone.

            // The byte behind 'stream'.
            SizeType currentPosition = Position;

            // Most PDF files are well-formatted, so check this first.

            // Check first correct case.
            if (_currChar == Chars.LF)
            {
                Position += 1;
                return Position;
            }

            // Check second correct case.
            if (_currChar == Chars.CR && _nextChar == Chars.LF)
            {
                Position += 2;
                return Position;
            }

            // OK, stream is ill-formatted.
            // We saw PDF files with blanks behind 'stream'.
            int skip = 0;
            while (_currChar == Chars.SP)
            {
                skip++;
                ScanNextChar(true);
            }

            if (skip > 0 || true)
                _logger.SkippedIllegalBlanksAfterStreamKeyword(skip, currentPosition, id);

            // Single LF.
            if (_currChar == Chars.LF)
            {
                Position += 1;
                return Position;
            }

            // CRLF.
            if (_currChar == Chars.CR && _nextChar == Chars.LF)
            {
                Position += 2;
                return Position;
            }

            // Single CR is illegal according to spec.
            if (_currChar == Chars.CR)
            {
                _logger.StreamKeywordFollowedBySingleCR(currentPosition, id);
                Position += 1;
                return Position;
            }

            _logger.StreamKeywordFollowedByIllegalBytes(currentPosition, id);

            // Best we can do here is to define content starts immediately behind 'stream' or behind the last blank, respectively.
            return Position;
        }

        /// <summary>
        /// Reads the raw content of a stream.
        /// A stream longer than 2 GiB is not intended by design.
        /// May return fewer bytes than requested if EOF is reached while reading.
        /// </summary>
        public byte[] ScanStream(SizeType position, int length, out int bytesRead)
        {
            Debug.Assert(Position == position);
            // Set physical stream position because this is not the logical position.
            _pdfStream.Position = position;

            if (!ReadWholeStreamSequence(_pdfStream, length, out var bytes, out bytesRead))
            {
                // EOF reached, so return what was read.
                _logger.EndOfStreamReached(length, position, bytesRead);

                Position = position + bytesRead;
                return bytes;
            }

            // Note: Position += length cannot be used here.
            Position = position + length;
            return bytes;
        }

        /// <summary>
        /// Reads the whole given sequence of bytes of the current stream and advances the position within the stream by the number of bytes read.
        /// Stream.Read is executed multiple times, if necessary.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length of the sequence to be read.</param>
        /// <param name="content">The array holding the stream data read.</param>
        /// <param name="bytesRead">The number of bytes actually read.</param>
        /// <exception cref="EndOfStreamException">
        /// Thrown when the sequence could not be read completely although the end of the stream has not been reached.
        /// </exception>
        static bool ReadWholeStreamSequence(Stream stream, int length, out byte[] content, out int bytesRead)
        {
            content = new Byte[length];

            var bytesAhead = length;
            bytesRead = 0;
            int currentBytesRead;
            var round = 1;

            // Stream.Read may not read all requested bytes, if the end of the stream is reached, the internal buffer of the stream is too small, or for other reasons.
            // We explicitly don’t want to handle all kinds of possible stream errors. We expect the user to use streams that are fully available and seekable and to
            // use a MemoryStream to cache the original stream if necessary.
            // However, without this workaround the file would be regarded as corrupt instead of the stream not being compatible.
            do
            {
                // Log error only for retries.
                ConditionalLogOnRetry(bytesRead, "Stream.Read did not return the whole requested sequence. As a workaround, reading the missing bytes is tried again.");

                currentBytesRead = stream.Read(content, bytesRead, bytesAhead);
                bytesRead += currentBytesRead;
                bytesAhead -= currentBytesRead;

                // Return true, if the whole sequence could be read.
                if (bytesRead == length)
                {
                    // Log error only if retries were done.
                    ConditionalLogOnRetry(bytesRead, "The workaround was able to load the whole sequence.");
                    return true;
                }

                // Return false, if the end of the stream was reached.
                if (stream.Position >= stream.Length)
                {
                    // Log error only if retries were done.
                    ConditionalLogOnRetry(bytesRead, "The workaround could not load the whole sequence. The end of the stream was reached before the end of the sequence.");
                    return false;
                }

                round++;
            } while (bytesAhead > 0 && currentBytesRead > 0);


            // The sequence could not be read completely although the end of the stream has not been reached: Throw exception.
            throw TH.EndOfStreamException_CouldNotReadToStreamEnd();

            void ConditionalLogOnRetry(int bytesRead, string status)
            {
                if (round > 1)
                    PdfSharpLogHost.Logger.StreamIssue(status, bytesRead, length);
            }
        }

        /// <summary>
        /// Gets the effective length of a stream on the basis of the position of 'endstream'.
        /// Call this function if 'endstream' was not found at the end of a stream content after
        /// it is parsed.
        /// </summary>
        /// <param name="start">The position behind 'stream' symbol in dictionary.</param>
        /// <param name="searchLength">The range to search for 'endstream'.</param>
        /// <param name="suppressObjectOrderExceptions">Suppresses exceptions that may be caused by not yet available objects.</param>
        /// <returns>The real length of the stream when 'endstream' was found.</returns>
        public int DetermineStreamLength(SizeType start, int searchLength, SuppressExceptions? suppressObjectOrderExceptions = null)
        {
            var rawString = RandomReadRawString(start, searchLength);

            // When we come here, we have either an invalid or no \Length entry.
            // Best we can do is to consider all byte before 'endstream' are part of the stream content.
            // In case the stream is zipped, this is no problem. In case the stream is encrypted
            // it would be a serious problem. But we wait if this really happens.
            int idxEndStream = rawString.LastIndexOf("endstream", StringComparison.Ordinal);
            if (idxEndStream == -1)
            {
                SuppressExceptions.HandleError(suppressObjectOrderExceptions, () => throw TH.ObjectNotAvailableException_CannotRetrieveStreamLength());
                return -1;
            }

            return idxEndStream;
        }

        /// <summary>
        /// Tries to scan 'endstream' after reading the stream content with a logical position
        /// on the first byte behind the read stream content.
        /// Returns true if success. The logical position is then immediately behind 'endstream'.
        /// In case of false the logical position is not well-defined.
        /// </summary>
        public bool TryScanEndStreamSymbol()
        {
            return _currChar switch
            {
                // This case is not recommended by specs, but valid PDF.
                'e' when _nextChar == 'n' => TryScanLiterally("endstream"),
                // These are the valid by specs cases.
                Chars.CR when _nextChar == 'e' => TryScanLiterally("\rendstream"),
                Chars.LF when _nextChar == 'e' => TryScanLiterally("\nendstream"),
                Chars.CR when _nextChar == Chars.LF => TryScanLiterally("\r\nendstream"),
                _ => false
            };
        }

        /// <summary>
        /// Reads a string in 'raw' encoding.
        /// </summary>
        public string ScanRawString(SizeType position, int length)
        {
            _pdfStream.Position = position;
            var result = ReadWholeStreamSequence(_pdfStream, length, out var bytes, out _);
            Debug.Assert(result);
            return PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Move current position one byte further in PDF stream and
        /// return it as a character with high byte always zero.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal char ScanNextChar(bool handleCRLF)
        {
            if (_charIndex >= _pdfLength)
            {
                _currChar = _nextChar; // The last character we are now dealing with.
                _nextChar = Chars.EOF; // Next character is EOF.
            }
            else
            {
                _currChar = _nextChar;
                _nextChar = (char)_pdfStream.ReadByte();
                _charIndex++;
                if (handleCRLF && _currChar == Chars.CR)
                {
                    if (_nextChar == Chars.LF)
                    {
                        // Treat CR LF as LF.
                        _currChar = _nextChar;
                        _nextChar = (char)_pdfStream.ReadByte();
                        _charIndex++;
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
            // ReSharper disable once RedundantCast because SizeType can be 32 bit depending on build.
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
                        <= ' ' => $"""⟬{(((int)ch).ToString("X2"))}⟭""",

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
                return Symbol switch
                {
                    Symbol.Integer => (int)_tokenAsLong,

                    // Should always fail, because if token fits into integer the symbol type would not be LongInteger.
                    Symbol.LongInteger => _tokenAsLong is >= Int32.MinValue and <= Int32.MaxValue ?
                        (int)_tokenAsLong : Throw(),

                    // All other types fail.
                    //Symbol.Real => 42,
                    //Symbol.ObjRef => 42,
                    _ => throw new InvalidOperationException("Symbol type is not 'Integer'.")
                };

                static int Throw() => throw new InvalidOperationException("64-bit value too large for 32-bit value.");
            }
        }

        /// <summary>
        /// Interprets current token as 64-bit integer literal.
        /// </summary>
        public long TokenToLongInteger
        {
            get => Symbol switch
            {
                Symbol.Integer => _tokenAsLong,
                Symbol.LongInteger => _tokenAsLong,
                _ => throw new InvalidOperationException("Symbol type is not 'Integer' or 'LongInteger'.")
            };
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
                // Reference 2.0: 7.1  Table 1 — White-space characters / Page 22
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
            // Reference 2.0: 7.1  Table 2 — Delimiter characters / Page 23
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
        SizeType _charIndex;
        char _currChar;
        char _nextChar;
        readonly StringBuilder _token = new();
        long _tokenAsLong;
        double _tokenAsReal;
        (int, int) _tokenAsObjectID;
        readonly Stream _pdfStream;
        readonly ILogger _logger;
    }

#if DEBUG_
    public class LexerHelper
    {
        // Give me an idea of the try/success ratio.
        // To be DELETED

        public static int TryCheckReferenceCount;
        public static int TryCheckReferenceSuccessCount;
    }
#endif
}
