// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    // Taken from WPF source code to ensure compatibility.
    class TokenizerHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizerHelper"/> class.
        /// </summary>
        public TokenizerHelper(string? str, IFormatProvider formatProvider)
        {
            char numericListSeparator = GetNumericListSeparator(formatProvider);
            Initialize(str, '\'', numericListSeparator);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizerHelper"/> class.
        /// </summary>
        public TokenizerHelper(string? str, char quoteChar, char separator)
        {
            Initialize(str, quoteChar, separator);
        }

        void Initialize(string? str, char quoteChar, char separator)
        {
            _str = str;
            _strLen = str?.Length ?? 0;
            _currentTokenIndex = -1;
            _quoteChar = quoteChar;
            _argSeparator = separator;

            // Skip any whitespace.
            while (_charIndex < _strLen)
            {
                if (!Char.IsWhiteSpace(_str!, _charIndex))
                    return;
                _charIndex++;
            }
        }

        public string? NextTokenRequired()
        {
            if (!NextToken(false))
                throw new InvalidOperationException("PrematureStringTermination"); //SR.Get(SRID.TokenizerHelperPrematureStringTermination, new object[0]));
            return GetCurrentToken();
        }

        public string? NextTokenRequired(bool allowQuotedToken)
        {
            if (!NextToken(allowQuotedToken))
                throw new InvalidOperationException("PrematureStringTermination");  //SR.Get(SRID.TokenizerHelperPrematureStringTermination, new object[0]));
            return GetCurrentToken();
        }

        public string? GetCurrentToken()
        {
            if (_currentTokenIndex < 0)
                return null;
            return _str!.Substring(_currentTokenIndex, _currentTokenLength);
        }

        public void LastTokenRequired()
        {
            if (_charIndex != _strLen)
                throw new InvalidOperationException("Extra data encountered"); //SR.Get(SRID.TokenizerHelperExtraDataEncountered, new object[0]));
        }

        /// <summary>
        /// Move to next token.
        /// </summary>
        public bool NextToken()
        {
            return NextToken(false);
        }

        /// <summary>
        /// Move to next token.
        /// </summary>
        public bool NextToken(bool allowQuotedToken)
        {
            return NextToken(allowQuotedToken, _argSeparator);
        }

        public bool NextToken(bool allowQuotedToken, char separator)
        {
            // Reset index.
            _currentTokenIndex = -1;
            _foundSeparator = false;

            // Already at the end of the string?
            if (_charIndex >= _strLen)
                return false;

            char currentChar = _str![_charIndex];

            // Setup the quoteCount .
            int quoteCount = 0;

            // If we are allowing a quoted token and this token begins with a quote, 
            // set up the quote count and skip the initial quote
            if (allowQuotedToken &&
                currentChar == _quoteChar)
            {
                quoteCount++;
                _charIndex++;
            }

            int newTokenIndex = _charIndex;
            int newTokenLength = 0;

            // Loop until hit end of string or hit a separator or whitespace.
            while (_charIndex < _strLen)
            {
                currentChar = _str[_charIndex];

                // If have a quoteCount and this is a quote  decrement the quoteCount.
                if (quoteCount > 0)
                {
                    // If anything but a quoteChar we move on.
                    if (currentChar == _quoteChar)
                    {
                        quoteCount--;

                        // If at zero which it always should for now break out of the loop.
                        if (quoteCount == 0)
                        {
                            ++_charIndex;
                            break;
                        }
                    }
                }
                else if ((Char.IsWhiteSpace(currentChar)) || (currentChar == separator))
                {
                    if (currentChar == separator)
                        _foundSeparator = true;
                    break;
                }

                _charIndex++;
                newTokenLength++;
            }

            // If quoteCount isn't zero we hit the end of the string before the ending quote.
            if (quoteCount > 0)
                throw new InvalidOperationException("Missing end quote"); //SR.Get(SRID.TokenizerHelperMissingEndQuote, new object[0]));

            // Move at the start of the nextToken.
            ScanToNextToken(separator); 

            // Update the _currentToken values.
            _currentTokenIndex = newTokenIndex;
            _currentTokenLength = newTokenLength;

            if (_currentTokenLength < 1)
                throw new InvalidOperationException("Empty token"); // SR.Get(SRID.TokenizerHelperEmptyToken, new object[0]));

            return true;
        }

        void ScanToNextToken(char separator)
        {
            // Do nothing if already at end of the string.
            if (_charIndex < _strLen)
            {
                char currentChar = _str![_charIndex];

                // Ensure that currentChar is a white space or separator.
                if (currentChar != separator && !Char.IsWhiteSpace(currentChar))
                    throw new InvalidOperationException("ExtraDataEncountered"); //SR.Get(SRID.TokenizerHelperExtraDataEncountered, new object[0]));

                // Loop until a character that isn't the separator or white space.
                int argSepCount = 0;
                while (_charIndex < _strLen)
                {
                    currentChar = _str[_charIndex];
                    if (currentChar == separator)
                    {
                        _foundSeparator = true;
                        argSepCount++;
                        _charIndex++;

                        if (argSepCount > 1)
                            throw new InvalidOperationException("EmptyToken"); //SR.Get(SRID.TokenizerHelperEmptyToken, new object[0]));
                    }
                    else if (Char.IsWhiteSpace(currentChar))
                    {
                        // Skip white space.
                        ++_charIndex;
                    }
                    else
                        break;
                }

                // If there was a separatorChar then we shouldn't be at the end of string or means there was a separator but there isn't an arg.
                if (argSepCount > 0 && _charIndex >= _strLen)
                    throw new InvalidOperationException("EmptyToken"); // SR.Get(SRID.TokenizerHelperEmptyToken, new object[0]));
            }
        }

        public static char GetNumericListSeparator(IFormatProvider? provider)
        {
            char numericSeparator = ',';
            NumberFormatInfo numberFormat = NumberFormatInfo.GetInstance(provider);
            if (numberFormat.NumberDecimalSeparator.Length > 0 && numericSeparator == numberFormat.NumberDecimalSeparator[0])
                numericSeparator = ';';
            return numericSeparator;
        }

        public bool FoundSeparator => _foundSeparator;

        bool _foundSeparator;

        char _argSeparator;
        int _charIndex;
        int _currentTokenIndex;
        int _currentTokenLength;
        char _quoteChar;
        string? _str;
        int _strLen;
    }
}
