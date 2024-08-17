﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Internal;
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
            _idxChar = 0;
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
                    // Eat comments, the parser doesn’t handle them.
                    ScanComment();
                    goto TryAgain;

                case '/':
                    return Symbol = ScanName();

                case '+':
                case '-':
                    // Cannot be an object reference if a sign was found.
                    return Symbol = ScanNumber(false);

                case '(':
                    return Symbol = ScanStringLiteral();

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
                    // just skip over unexpected character
                    ScanNextChar(true);
                    goto TryAgain;
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
                    PdfSharpLogHost.Logger.LogError("+/- not followed by a number.");
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
                && IsWhiteSpace(_currChar))
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
                Debug.Assert(IsWhiteSpace(_currChar));

                // A Reference has the form "nnn ggg R". The original implementation of the parser used a
                // reduce/shift algorithm in the first place. But this case is the only one we need to
                // look ahead 3 tokens.
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
                "R" => Symbol = Symbol.R,
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
                                        //_ = typeof(int);
                                        goto RetryAfterSkipIllegalCharacter;
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

        ///// <summary>
        ///// Tries to scan "\n", "\r" or "\r\n" and moves the Position to the next line.
        ///// </summary>
        //public bool TryScanEndOfLine() => TryScanEndOfLine(true, true, true);

        ///// <summary>
        ///// Tries to scan the accepted end-of-line markers and moves the Position to the next line.
        ///// </summary>
        //public bool TryScanEndOfLine(bool acceptCR, bool acceptLF, bool acceptCRLF)
        //{
        //    if (acceptCRLF && _currChar == Chars.CR && _nextChar == Chars.LF)
        //    {
        //        Position += 2;
        //        return true;
        //    }
        //    if (acceptCR && _currChar == Chars.CR || acceptLF && _currChar == Chars.LF)
        //    {
        //        Position += 1;
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// Return the exact position where the content of the stream starts.
        /// The logical position is also set to this value when the function returns.<br/>
        /// Reference:     3.2.7  Stream Objects / Page 60
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

            string message;
            if (skip > 0 || true)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    message = Invariant($"Skipped {skip} illegal blanks behind keyword 'stream' at position {currentPosition} in object {id}.");
                    _logger.LogWarning(message);
                }
            }

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
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    message = Invariant($"Keyword 'stream' followed by single CR is illegal at position {currentPosition} in object {id}.");
                    _logger.LogWarning(message);
                }
                Position += 1;
                return Position;
            }

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                message = Invariant($"Keyword 'stream' followed by illegal bytes at position {currentPosition} in object {id}.");
                _logger.LogWarning(message);
            }

            // Best we can do here is to define content starts immediately behind 'stream' or behind the last blank, respectively.
            return Position;
        }

        /// <summary>
        /// Reads the raw content of a stream.
        /// A stream longer than 2 GiB is not intended by design.
        /// </summary>
        public byte[] ScanStream(SizeType position, int length)
        {
            Debug.Assert(Position == position);
            // Set physical stream position because this is not the logical position.
            _pdfStream.Position = position;

            byte[] bytes = new byte[length];
            int read = _pdfStream.Read(bytes, 0, length);
            if (read != length)
            {
                throw new InvalidOperationException("Stream cannot be read. Please send us the PDF file so that we can fix this (issues (at) pdfsharp.net).");
            }

            // Note: Position += length cannot be used here.
            Position = position + length;
            return bytes;
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
#if DEBUG_
            if (start == 144848)
                _ = sizeof(int);
#endif
            var firstStart = start;
            while (start < _pdfLength)
            {
                var rawString = RandomReadRawString(start, Math.Min(searchLength, (int)(_pdfLength - start)));

                // When we come here, we have either an invalid or no \Length entry.
                // Best we can do is to consider all byte before 'endstream' are part of the stream content.
                // In case the stream is zipped, this is no problem. In case the stream is encrypted
                // it would be a serious problem. But we wait if this really happens.
                int idxEndStream = rawString.LastIndexOf("endstream", StringComparison.Ordinal);
                if (idxEndStream >= 0)
                {
                    // The spec says (7.3.8, Stream Objects):
                    // "There should be an end-of-line marker after the data and before endstream;
                    // this marker shall not be included in the stream length"

                    // check bytes before the keyword for possible CRLF or LF or CR
                    // (CR alone SHALL NOT be used but check it anyway)
                    // sanity check, should always pass since we SHOULD have read the "stream" keyword before we came here
                    if (start + idxEndStream >= 2)
                    {
                        _pdfStream.Position = start + idxEndStream - 2;
                        var b1 = _pdfStream.ReadByte();
                        var b2 = _pdfStream.ReadByte();
                        if (b2 == '\n' || b2 == '\r')   // possible CRLF or single LF or single CR
                        {
                            idxEndStream--;
                            if (b1 == '\r' && b2 != '\r')   // handle CRLF but not CRCR
                                idxEndStream--;
                        }
                    }
                    return (int)(start - firstStart + idxEndStream);
                }
                start += Math.Max(1, searchLength - "endstream".Length - 1);
            }
            SuppressExceptions.HandleError(suppressObjectOrderExceptions, () => throw TH.ObjectNotAvailableException_CannotRetrieveStreamLength());
            return -1;
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
            var bytes = new byte[length];
            var readBytes = _pdfStream.Read(bytes, 0, length);
            Debug.Assert(readBytes == length);
            return PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
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
