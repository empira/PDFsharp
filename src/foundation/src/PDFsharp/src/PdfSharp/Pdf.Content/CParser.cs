// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content.Objects;

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Provides the functionality to parse PDF content streams.
    /// </summary>
    /*public*/
    sealed class CParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CParser"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        public CParser(PdfPage page)
        {
            _page = page;
            PdfContent content = page.Contents.CreateSingleContent();
            var bytes = content?.Stream?.Value ?? NRT.ThrowOnNull<byte[]>();
            _lexer = new CLexer(bytes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CParser"/> class.
        /// </summary>
        /// <param name="content">The content bytes.</param>
        public CParser(byte[] content)
        {
            _lexer = new CLexer(content);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CParser"/> class.
        /// </summary>
        /// <param name="content">The content stream.</param>
        public CParser(MemoryStream content)
        {
            _lexer = new CLexer(content.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CParser"/> class.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        public CParser(CLexer lexer)
        {
            _lexer = lexer;
        }

        public CSymbol Symbol => _lexer.Symbol;

        public CSequence ReadContent()
        {
            CSequence sequence = new CSequence();
            ParseObject(sequence, CSymbol.Eof);

            return sequence;
        }

        /// <summary>
        /// Parses whatever comes until the specified stop symbol is reached.
        /// </summary>
        void ParseObject(CSequence sequence, CSymbol stop)
        {
            CSymbol symbol;
            while ((symbol = ScanNextToken()) != CSymbol.Eof)
            {
                if (symbol == stop)
                    return;

                CString s;
                COperator op;
                switch (symbol)
                {
                    case CSymbol.Comment:
                        // Ignore comments.
                        break;

                    case CSymbol.Integer:
                        CInteger n = new()
                        {
                            Value = _lexer.TokenToInteger
                        };
                        _operands.Add(n);
                        break;

                    case CSymbol.Real:
                        CReal r = new()
                        {
                            Value = _lexer.TokenToReal
                        };
                        _operands.Add(r);
                        break;

                    case CSymbol.String:
                    case CSymbol.HexString:
                    case CSymbol.UnicodeString:
                    case CSymbol.UnicodeHexString:
                        s = new()
                        {
                            Value = _lexer.Token,
                            CStringType = CStringType.String // Must set string type. So far, only CStringType.String is supported in CString.ToString().
                        };
                        _operands.Add(s);
                        break;

                    case CSymbol.Dictionary:
                        s = new()
                        {
                            Value = _lexer.Token,
                            CStringType = CStringType.Dictionary
                        };
                        _operands.Add(s);
                        op = CreateOperator(OpCodeName.Dictionary);
                        //_operands.Clear();
                        sequence.Add(op);
                        break;

                    case CSymbol.Name:
                        var name = new CName
                        {
                            Name = _lexer.Token
                        };
                        _operands.Add(name);
                        break;

                    case CSymbol.Operator:
                        op = CreateOperator();
                        //_operands.Clear();
                        sequence.Add(op);
                        break;

                    case CSymbol.BeginArray:
                        var array = new CArray();
                        if (_operands.Count != 0)
                            ContentReaderDiagnostics.ThrowContentReaderException("Array within array...");

                        ParseObject(array, CSymbol.EndArray);
                        array.Add(_operands);
                        _operands.Clear();
                        _operands.Add((CObject)array);
                        break;

                    case CSymbol.EndArray:
                        ContentReaderDiagnostics.HandleUnexpectedCharacter(']');
                        break;
#if DEBUG
                    default:
                        Debug.Assert(false);
                        break;
#endif
                }
            }
        }

        COperator CreateOperator()
        {
            string name = _lexer.Token;
            COperator op = OpCodes.OperatorFromName(name);
            return CreateOperator(op);
        }

        COperator CreateOperator(OpCodeName nameop)
        {
            string name = nameop.ToString();
            COperator op = OpCodes.OperatorFromName(name);
            return CreateOperator(op);
        }

        COperator CreateOperator(COperator op)
        {
            if (op.OpCode.OpCodeName == OpCodeName.BI)
            {
                _lexer.ScanInlineImage();
            }
#if DEBUG
            if (op.OpCode.Operands != -1 && op.OpCode.Operands != _operands.Count)
            {
                if (op.OpCode.OpCodeName != OpCodeName.ID)
                {
                    _ = typeof(int);
                    Debug.Assert(false, "Invalid number of operands.");
                }
            }
#endif
            //if (_operands.Count != 0)  // Do not create empty Operands sequence.
                op.Operands.Add(_operands);
            _operands.Clear();
            return op;
        }

        CSymbol ScanNextToken()
        {
            return _lexer.ScanNextToken();
        }

        CSymbol ScanNextToken(out string token)
        {
            CSymbol symbol = _lexer.ScanNextToken();
            token = _lexer.Token;
            return symbol;
        }

        /// <summary>
        /// Reads the next symbol that must be the specified one.
        /// </summary>
        CSymbol ReadSymbol(CSymbol symbol)
        {
            CSymbol current = _lexer.ScanNextToken();
            if (symbol != current)
                ContentReaderDiagnostics.ThrowContentReaderException(PsMsgs.UnexpectedToken(_lexer.Token));
            return current;
        }

        readonly CSequence _operands = new CSequence();
        PdfPage _page = default!;
        readonly CLexer _lexer;
    }
}
