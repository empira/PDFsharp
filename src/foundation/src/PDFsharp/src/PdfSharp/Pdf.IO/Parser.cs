// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using PdfSharp.Internal;
using PdfSharp.Logging;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using System.Text;
using Microsoft.Extensions.Logging;
using static PdfSharp.Pdf.Advanced.PdfCrossReferenceStream;
using static PdfSharp.Pdf.PdfDictionary;

namespace PdfSharp.Pdf.IO
{
    /*
       Direct and indirect objects

       * If a simple object (boolean, integer, number, date, string, rectangle etc.) is referenced indirect,
         the parser reads this object immediately and consumes the indirection.

       * If a composite object (dictionary, array etc.) is referenced indirect, a PdfReference object
         is returned.

       * If a composite object is a direct object, no PdfReference is created and the object is
         parsed immediately.

       * A reference to a non-existing object is specified as legal, therefore null is returned.
    */

    /// <summary>
    /// Provides the functionality to parse PDF documents.
    /// </summary>
    sealed class Parser
    {
        public Parser(PdfDocument? document, Stream pdf)
        {
            _document = document!; // NRT HACK
            _lexer = new Lexer(pdf);
            _stack = new ShiftStack();
        }

        public Parser(PdfDocument document)
        {
            _document = document;
            _lexer = document?._lexer ?? throw new ArgumentNullException(nameof(document), "Lexer not defined.");
            _stack = new ShiftStack();
        }

        /// <summary>
        /// Sets PDF input stream position to the specified object.
        /// </summary>
        public int MoveToObject(PdfObjectID objectID)
        {
            int position = _document.IrefTable[objectID]?.Position ?? throw new AggregateException("Invalid object ID.");
            if (position < 0)
            {
                throw new AggregateException($"Invalid position {position} for object ID {objectID}.");
            }
            return _lexer.Position = position;
        }

        public Symbol Symbol => _lexer.Symbol;

        public PdfObjectID ReadObjectNumber(int position)
        {
            _lexer.Position = position;
            int objectNumber = ReadInteger();
            int generationNumber = ReadInteger();
            return new(objectNumber, generationNumber);
        }

        /// <summary>
        /// Reads PDF object from input stream.
        /// </summary>
        /// <param name="pdfObject">Either the instance of a derived type or null. If it is null
        /// an appropriate object is created.</param>
        /// <param name="objectID">The address of the object.</param>
        /// <param name="includeReferences">If true, specifies that all indirect objects
        /// are included recursively.</param>
        /// <param name="fromObjectStream">If true, the object is parsed from an object stream.</param>
        public PdfObject ReadObject(PdfObject? pdfObject, PdfObjectID objectID, bool includeReferences, bool fromObjectStream)
        {
#if DEBUG_
            Debug.WriteLine("ReadObject: " + objectID);
            if (objectID.ObjectNumber == 20)
                GetType();
#endif

            int objectNumber = objectID.ObjectNumber;
            int generationNumber = objectID.GenerationNumber;
            if (!fromObjectStream)
            {
                MoveToObject(objectID);
                objectNumber = ReadInteger();
                generationNumber = ReadInteger();
            }
            else
            {
                // Reading from ObjectStream.
                var iref = _document.IrefTable[objectID];
                if (iref != null)
                {
                    // Attempt to read an object that was already registered. Keep the former object.
                    // This only happens with corrupt PDF files that have duplicate IDs.
                    if (iref.Value != null!)
                    {
                        LogHost.Logger.LogWarning("Another instance of object {iref} was found. Using previously encountered object instead.", iref);
                        // Attempt to read an object that was already read. Keep the former object.
                        return iref.Value;
                    }

                    if (iref.Position >= 0)
                    {
                        LogHost.Logger.LogWarning("Another instance of object {iref} was found. Keeping reference to previously encountered object.", iref);
                        // The object ID was already found, but the object was not read yet.
                        // We ignore the object in the object stream and return a dummy object.
                        // Better: Do not call this method in the first place.
                        var dummy = new PdfArray
                        {
                            Reference = iref
                        };
                        return dummy; // Return a dummy object. The object is not used, but Reference must not be empty.
                    }
                }
            }

#if DEBUG
            // The following assertion sometime failed (see below)
            //Debug.Assert(objectID == new PdfObjectID(objectNumber, generationNumber));
            if (!fromObjectStream && objectID != new PdfObjectID(objectNumber, generationNumber))
            {
                // A special kind of bug? Or is this an undocumented PDF feature?
                // PDF4NET 2.6 provides a sample called 'Unicode', which produces a file 'unicode.pdf'
                // The iref table of this file contains the following entries:
                //    iref
                //    0 148
                //    0000000000 65535 f 
                //    0000000015 00000 n 
                //    0000000346 00000 n 
                //    ....
                //    0000083236 00000 n 
                //    0000083045 00000 n 
                //    0000083045 00000 n 
                //    0000083045 00000 n 
                //    0000083045 00000 n 
                //    0000080334 00000 n 
                //    ....
                // Object 84, 85, 86, and 87 maps to the same dictionary, but all PDF readers I tested
                // ignores this mismatch! The following assertion failed about 50 times with this file.
#if true_
                string message = String.Format("xref entry {0} {1} maps to object {2} {3}.",
                    objectID.ObjectNumber, objectID.GenerationNumber, objectNumber, generationNumber);
                Debug.Assert(false, message);
#endif
            }
#endif
            // Always use object ID from iref table (see above).
            objectNumber = objectID.ObjectNumber;
            generationNumber = objectID.GenerationNumber;
#if true_
            Debug.WriteLine(String.Format("obj: {0} {1}", objectNumber, generationNumber));
#endif
            if (!fromObjectStream)
                ReadSymbol(Symbol.Obj);

            bool checkForStream = false;
            Symbol symbol = ScanNextToken();
            switch (symbol)
            {
                case Symbol.BeginArray:
                    PdfArray array;
                    if (pdfObject == null)
                        array = new PdfArray(_document);
                    else
                        array = (PdfArray)pdfObject;
                    //PdfObject.RegisterObject(array, objectID, generation);
                    pdfObject = ReadArray(array, includeReferences);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    break;

                case Symbol.BeginDictionary:
                    PdfDictionary dict;
                    if (pdfObject == null)
                        dict = new PdfDictionary(_document);
                    else
                        dict = (PdfDictionary)pdfObject;
                    //PdfObject.RegisterObject(dict, objectID, generation);
                    checkForStream = true;
                    pdfObject = ReadDictionary(dict, includeReferences);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    break;

                // Acrobat 6 Professional proudly presents: The Null object!
                // Even with a one-digit object number an indirect reference «x 0 R» to this object is
                // one character larger than the direct use of «null». Probable this is the reason why
                // it is true that Acrobat Web Capture 6.0 creates this object, but obviously never 
                // creates a reference to it!
                case Symbol.Null:
                    pdfObject = new PdfNullObject(_document);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    if (!fromObjectStream)
                        ReadSymbol(Symbol.EndObj);
                    return pdfObject;

                // Empty object. Invalid PDF, but we need to handle it. Treat as null object.
                case Symbol.EndObj:
                    pdfObject = new PdfNullObject(_document);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    return pdfObject;

                case Symbol.Boolean:
                    pdfObject = new PdfBooleanObject(_document, String.Compare(_lexer.Token, Boolean.TrueString, StringComparison.OrdinalIgnoreCase) == 0);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    if (!fromObjectStream)
                        ReadSymbol(Symbol.EndObj);
                    return pdfObject;

                case Symbol.Integer:
                    pdfObject = new PdfIntegerObject(_document, _lexer.TokenToInteger);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    if (!fromObjectStream)
                        ReadSymbol(Symbol.EndObj);
                    return pdfObject;

                case Symbol.UInteger:
                    pdfObject = new PdfUIntegerObject(_document, _lexer.TokenToUInteger);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    if (!fromObjectStream)
                        ReadSymbol(Symbol.EndObj);
                    return pdfObject;

                case Symbol.Real:
                    pdfObject = new PdfRealObject(_document, _lexer.TokenToReal);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    if (!fromObjectStream)
                        ReadSymbol(Symbol.EndObj);
                    return pdfObject;

                case Symbol.String:
                case Symbol.UnicodeString:
                case Symbol.HexString:
                case Symbol.UnicodeHexString:
                    pdfObject = new PdfStringObject(_document, _lexer.Token);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    if (!fromObjectStream)
                        ReadSymbol(Symbol.EndObj);
                    return pdfObject;

                case Symbol.Name:
                    pdfObject = new PdfNameObject(_document, _lexer.Token);
                    pdfObject.SetObjectID(objectNumber, generationNumber);
                    if (!fromObjectStream)
                        ReadSymbol(Symbol.EndObj);
                    return pdfObject;

                case Symbol.Keyword:
                    // Should not come here anymore.
                    ParserDiagnostics.HandleUnexpectedToken(_lexer.Token);
                    break;

                default:
                    // Should not come here anymore.
                    ParserDiagnostics.HandleUnexpectedToken(_lexer.Token);
                    break;
            }
            symbol = ScanNextToken();
            if (symbol == Symbol.BeginStream)
            {
                var dict = (PdfDictionary?)pdfObject;
                Debug.Assert(checkForStream, "Unexpected stream...");
#if true_
                ReadStream(dict);
#else
                int length = GetStreamLength(dict!); // NRT HACK
                byte[] bytes = _lexer.ReadStream(length);
#if true_
                if (dict.Elements.GetString("/Filter") == "/FlateDecode")
                {
                    if (dict.Elements["/Subtype"] == null)
                    {
                        try
                        {
                            byte[] decoded = Filtering.FlateDecode.Decode(bytes);
                            if (decoded.Length == 0)
                                goto End;
                            string pageContent = Filtering.FlateDecode.DecodeToString(bytes);
                            if (pageContent.Length > 100)
                                pageContent = pageContent.Substring(pageContent.Length - 100);
                            pageContent.GetType();
                            bytes = decoded;
                            dict.Elements.Remove("/Filter");
                            dict.Elements.SetInteger("/Length", bytes.Length);
                        }
                        catch
                        {
                        }
                    }
                End: ;
                }
#endif
                var stream = new PdfDictionary.PdfStream(bytes, dict ?? NRT.ThrowOnNull<PdfDictionary>());
                dict.Stream = stream;
                ReadSymbol(Symbol.EndStream);
                symbol = ScanNextToken();
#endif
            }
            if (!fromObjectStream && symbol != Symbol.EndObj)
                ParserDiagnostics.ThrowParserException(PSSR.UnexpectedToken(_lexer.Token));
            return pdfObject ?? NRT.ThrowOnNull<PdfObject>();
        }

        //public PdfObject ReadObject(PdfObject obj, bool includeReferences)

        /// <summary>
        /// Reads the stream of a dictionary.
        /// </summary>
        void ReadStream(PdfDictionary dict)
        {
            Symbol symbol = _lexer.Symbol;
            Debug.Assert(symbol == Symbol.BeginStream);
            int length = GetStreamLength(dict);
            byte[] bytes = _lexer.ReadStream(length);
            PdfDictionary.PdfStream stream = new PdfDictionary.PdfStream(bytes, dict);
            Debug.Assert(dict.Stream == null, "Dictionary already has a stream.");
            dict.Stream = stream;
            ReadSymbol(Symbol.EndStream);
            ScanNextToken();
        }

        // HACK: Solve problem more general.
        int GetStreamLength(PdfDictionary dict)
        {
            if (dict.Elements["/F"] != null)
                throw new NotImplementedException("File streams are not yet implemented.");

            var value = dict.Elements["/Length"];
            if (value is PdfInteger)
                return Convert.ToInt32(value);

            if (value is PdfReference reference)
            {
#if true
                object length;
                if (reference.Position == -1 && reference.Value != null!)
                {
                    if (reference.Value is not PdfIntegerObject integer)
                        throw new InvalidOperationException("Cannot retrieve stream length from stream object.");

                    length = integer;
                }
                else
                {
                    ParserState state = SaveState();
                    length = ReadObject(null, reference.ObjectID, false, false);
                    RestoreState(state);
                }
#else
                ParserState state = SaveState();
                object length = ReadObject(null, reference.ObjectID, false, false);
                RestoreState(state);
#endif
                int len = ((PdfIntegerObject)length).Value;
                dict.Elements["/Length"] = new PdfInteger(len);
                return len;
            }
            throw new InvalidOperationException("Cannot retrieve stream length.");
        }

        public PdfArray ReadArray(PdfArray array, bool includeReferences)
        {
            Debug.Assert(Symbol == Symbol.BeginArray);

            if (array == null!)
                array = new PdfArray(_document);

            int sp = _stack.SP;
            ParseObject(Symbol.EndArray);
            int count = _stack.SP - sp;
            PdfItem[] items = _stack.ToArray(sp, count);
            _stack.Reduce(count);
            for (int idx = 0; idx < count; idx++)
            {
                PdfItem val = items[idx];
                if (includeReferences && val is PdfReference)
                    val = ReadReference((PdfReference)val, true);
                array.Elements.Add(val);
            }
            return array;
        }

#if DEBUG_
        static int ReadDictionaryCounter;
#endif

        internal PdfDictionary ReadDictionary(PdfDictionary? dict, bool includeReferences)
        {
            Debug.Assert(Symbol == Symbol.BeginDictionary);

#if DEBUG_
            ReadDictionaryCounter++;
            Debug.WriteLine(ReadDictionaryCounter.ToString());
            if (ReadDictionaryCounter == 101)
                GetType();
#endif

            if (dict == null)
                dict = new PdfDictionary(_document);
            DictionaryMeta meta = dict.Meta;

            int sp = _stack.SP;
            ParseObject(Symbol.EndDictionary);
            int count = _stack.SP - sp;
            Debug.Assert(count % 2 == 0);
            PdfItem[] items = _stack.ToArray(sp, count);
            _stack.Reduce(count);
            for (int idx = 0; idx < count; idx += 2)
            {
                var val = items[idx];
                if (!(val is PdfName))
                    ParserDiagnostics.ThrowParserException("Name expected."); // TODO L10N using PSSR.

                string key = val.ToString() ?? NRT.ThrowOnNull<string>();
                val = items[idx + 1];
                if (includeReferences && val is PdfReference reference)
                    val = ReadReference(reference, true);
                dict.Elements[key] = val;
            }
            return dict;
        }

#if DEBUG_
        static int ParseObjectCounter;
#endif

        /// <summary>
        /// Parses whatever comes until the specified stop symbol is reached.
        /// </summary>
        void ParseObject(Symbol stop)
        {
#if DEBUG_
            ParseObjectCounter++;
            Debug.WriteLine(ParseObjectCounter.ToString());
            if (ParseObjectCounter == 178)
                GetType();
#endif
            Symbol symbol;
            while ((symbol = ScanNextToken()) != Symbol.Eof)
            {
                if (symbol == stop)
                    return;

                switch (symbol)
                {
                    case Symbol.Comment:
                        // Ignore comments.
                        break;

                    case Symbol.Null:
                        _stack.Shift(PdfNull.Value);
                        break;

                    case Symbol.Boolean:
                        _stack.Shift(new PdfBoolean(_lexer.TokenToBoolean));
                        break;

                    case Symbol.Integer:
                        _stack.Shift(new PdfInteger(_lexer.TokenToInteger));
                        break;

                    case Symbol.UInteger:
                        _stack.Shift(new PdfUInteger(_lexer.TokenToUInteger));
                        break;

                    case Symbol.Real:
                        _stack.Shift(new PdfReal(_lexer.TokenToReal));
                        break;

                    case Symbol.String:
                        //stack.Shift(new PdfString(lexer.Token, PdfStringFlags.PDFDocEncoding));
                        _stack.Shift(new PdfString(_lexer.Token, PdfStringFlags.RawEncoding));
                        break;

                    case Symbol.UnicodeString:
                        _stack.Shift(new PdfString(_lexer.Token, PdfStringFlags.Unicode));
                        break;

                    case Symbol.HexString:
                        _stack.Shift(new PdfString(_lexer.Token, PdfStringFlags.HexLiteral));
                        break;

                    case Symbol.UnicodeHexString:
                        _stack.Shift(new PdfString(_lexer.Token, PdfStringFlags.Unicode | PdfStringFlags.HexLiteral));
                        break;

                    case Symbol.Name:
                        _stack.Shift(new PdfName(_lexer.Token));
                        break;

                    case Symbol.R:
                        {
                            Debug.Assert(_stack.GetItem(-1) is PdfInteger && _stack.GetItem(-2) is PdfInteger);
                            PdfObjectID objectID = new PdfObjectID(_stack.GetInteger(-2), _stack.GetInteger(-1));

                            var iref = _document.IrefTable[objectID];
                            if (iref == null)
                            {
                                // If a document has more than one PdfXRefTable it is possible that the first trailer has
                                // indirect references to objects whose iref entry is not yet read in.
                                if (_document.IrefTable.IsUnderConstruction)
                                {
                                    // XRefTable not complete when trailer is read. Create temporary irefs that are
                                    // removed later in PdfTrailer.Finish().
                                    iref = new PdfReference(objectID, 0);
                                    _stack.Reduce(iref, 2);
                                    break;
                                }
                                // PDF Reference 2.0 section 7.3.10:
                                // An indirect reference to an undefined object shall not be considered an error by a PDF processor;
                                // it shall be treated as a reference to the null object.
                                _stack.Reduce(PdfNull.Value, 2);
                                // Let's see what null objects are good for...
                                //Debug.Assert(false, "Null object detected!");
                                //stack.Reduce(PdfNull.Value, 2);
                            }
                            else
                                _stack.Reduce(iref, 2);
                            break;
                        }

                    case Symbol.BeginArray:
                        PdfArray array = new PdfArray(_document);
                        ReadArray(array, false);
                        _stack.Shift(array);
                        break;

                    case Symbol.BeginDictionary:
                        PdfDictionary dict = new PdfDictionary(_document);
                        ReadDictionary(dict, false);
                        _stack.Shift(dict);
                        break;

                    case Symbol.BeginStream:
                        throw new NotImplementedException();

                    // Not expected here:
                    //case Symbol.None:
                    //case Symbol.Keyword:
                    //case Symbol.EndStream:
                    //case Symbol.EndArray:
                    //case Symbol.EndDictionary:
                    //case Symbol.Obj:
                    //case Symbol.EndObj:
                    //case Symbol.XRef:
                    //case Symbol.Trailer:
                    //case Symbol.StartXRef:
                    //case Symbol.Eof:
                    default:
                        ParserDiagnostics.HandleUnexpectedToken(_lexer.Token);
                        SkipCharsUntil(stop);
                        return;
                }
            }
            ParserDiagnostics.ThrowParserException("Unexpected end of file."); // TODO L10N using PSSR.
        }

        Symbol ScanNextToken()
        {
            return _lexer.ScanNextToken();
        }

        Symbol ScanNextToken(out string token)
        {
            Symbol symbol = _lexer.ScanNextToken();
            token = _lexer.Token;
            return symbol;
        }

        Symbol SkipCharsUntil(Symbol stop)
        {
            switch (stop)
            {
                case Symbol.EndDictionary:
                    return SkipCharsUntil(">>", stop);

                default:
                    Symbol symbol;
                    do
                    {
                        symbol = ScanNextToken();
                    } while (symbol != stop && symbol != Symbol.Eof);
                    return symbol;
            }
        }

        Symbol SkipCharsUntil(string text, Symbol stop)
        {
            int length = text.Length;
            int idx = 0;
            char ch;
            while ((ch = _lexer.ScanNextChar(true)) != Chars.EOF)
            {
                if (ch == text[idx])
                {
                    if (idx + 1 == length)
                    {
                        _lexer.ScanNextChar(true);
                        return stop;
                    }
                    idx++;
                }
                else
                    idx = 0;
            }
            return Symbol.Eof;
        }

        //protected Symbol ScanNextToken(out string token, bool testReference)
        //{
        //  Symbol symbol = lexer.ScanNextToken(testReference);
        //  token = lexer.Token;
        //  return symbol;
        //}

        //    internal object ReadObject(int position)
        //    {
        //      lexer.Position = position;
        //      return ReadObject(false);
        //    }
        //
        //    internal virtual object ReadObject(bool directObject)
        //    {
        //      throw new InvalidOperationException("PdfParser.ReadObject() base class called");
        //    }

        /// <summary>
        /// Reads the object ID and the generation and sets it into the specified object.
        /// </summary>
        void ReadObjectID(PdfObject? obj)
        {
            int objectNumber = ReadInteger();
            int generationNumber = ReadInteger();
            ReadSymbol(Symbol.Obj);
            if (obj != null)
                obj.SetObjectID(objectNumber, generationNumber);
        }

        PdfItem ReadReference(PdfReference iref, bool includeReferences)
        {
            throw new NotImplementedException("ReadReference");
        }

        /// <summary>
        /// Reads the next symbol that must be the specified one.
        /// </summary>
        Symbol ReadSymbol(Symbol symbol)
        {
            if (symbol == Symbol.EndStream)
            {
            Skip:
                char ch = _lexer.MoveToNonWhiteSpace();

                if (ch == Chars.EOF)
                    ParserDiagnostics.HandleUnexpectedCharacter(ch);

                if (ch != 'e')
                {
                    _lexer.ScanNextChar(false);
                    goto Skip;
                }
            }
            Symbol current = _lexer.ScanNextToken();
            if (symbol != current)
                ParserDiagnostics.HandleUnexpectedToken(_lexer.Token);
            return current;
        }

        /// <summary>
        /// Reads the next token that must be the specified one.
        /// </summary>
        Symbol ReadToken(string token)
        {
            Symbol current = _lexer.ScanNextToken();
            if (token != _lexer.Token)
                ParserDiagnostics.HandleUnexpectedToken(_lexer.Token);
            return current;
        }

        /// <summary>
        /// Reads a name from the PDF data stream. The preceding slash is part of the result string.
        /// </summary>
        string ReadName()
        {
            string name;
            Symbol symbol = ScanNextToken(out name);
            if (symbol != Symbol.Name)
                ParserDiagnostics.HandleUnexpectedToken(name);
            return name;
        }

        /*
            /// <summary>
            /// Reads a string immediately or (optionally) indirectly from the PDF data stream.
            /// </summary>
            protected string ReadString(bool canBeIndirect)
            {
              Symbol symbol = Symbol.None; //lexer.ScanNextToken(canBeIndirect);
              if (symbol == Symbol.String || symbol == Symbol.UnicodeString || symbol == Symbol.HexString || symbol == Symbol.UnicodeHexString)
                return lexer.Token;
              else if (symbol == Symbol.R)
              {
                int position = lexer.Position;
                MoveToObject(lexer.Token);
                ReadObjectID(null);
                string s = ReadString();
                ReadSymbol(Symbol.EndObj);
                lexer.Position = position;
                return s;
              }
              thr ow new PdfReaderException(PSSR.UnexpectedToken(lexer.Token));
            }

            protected string ReadString()
            {
              return ReadString(false);
            }

            /// <summary>
            /// Reads a string immediately or (optionally) indirectly from the PDF data stream.
            /// </summary>
            protected bool ReadBoolean(bool canBeIndirect)
            {
              Symbol symbol = lexer.ScanNextToken(canBeIndirect);
              if (symbol == Symbol.Boolean)
                return lexer.TokenToBoolean;
              else if (symbol == Symbol.R)
              {
                int position = lexer.Position;
                MoveToObject(lexer.Token);
                ReadObjectID(null);
                bool b = ReadBoolean();
                ReadSymbol(Symbol.EndObj);
                lexer.Position = position;
                return b;
              }
              thr ow new PdfReaderException(PSSR.UnexpectedToken(lexer.Token));
            }

            protected bool ReadBoolean()
            {
              return ReadBoolean(false);
            }
        */

        /// <summary>
        /// Reads an integer value directly from the PDF data stream.
        /// </summary>
        int ReadInteger(bool canBeIndirect)
        {
            Symbol symbol = _lexer.ScanNextToken();
            if (symbol == Symbol.Integer)
                return _lexer.TokenToInteger;

            if (symbol == Symbol.R)
            {
                int position = _lexer.Position;
                //        MoveToObject(lexer.Token);
                ReadObjectID(null);
                int n = ReadInteger();
                ReadSymbol(Symbol.EndObj);
                _lexer.Position = position;
                return n;
            }
            ParserDiagnostics.HandleUnexpectedToken(_lexer.Token);
            return 0;
        }

        int ReadInteger()
        {
            return ReadInteger(false);
        }

        //    /// <summary>
        //    /// Reads a real value directly or (optionally) indirectly from the PDF data stream.
        //    /// </summary>
        //    double ReadReal(bool canBeIndirect)
        //    {
        //      Symbol symbol = lexer.ScanNextToken(canBeIndirect);
        //      if (symbol == Symbol.Real || symbol == Symbol.Integer)
        //        return lexer.TokenToReal;
        //      else if (symbol == Symbol.R)
        //      {
        //        int position = lexer.Position;
        ////        MoveToObject(lexer.Token);
        //        ReadObjectID(null);
        //        double f = ReadReal();
        //        ReadSymbol(Symbol.EndObj);
        //        lexer.Position = position;
        //        return f;
        //      }
        //      thr ow new PdfReaderException(PSSR.UnexpectedToken(lexer.Token));
        //    }
        //
        //    double ReadReal()
        //    {
        //      return ReadReal(false);
        //    }

        //    /// <summary>
        //    /// Reads an object from the PDF input stream. If the object has a specialized parser, it it used.
        //    /// </summary>
        //    public static PdfObject ReadObject(PdfObject pdfObject, PdfObjectID objectID)
        //    {
        //      if (pdfObject == null)
        //        thr ow new ArgumentNullException("pdfObject");
        //      if (pdfObject.Document == null)
        //        th row new ArgumentException(PSSR.OwningDocumentRequired, "pdfObject");
        //
        //      Type type = pdfObject.GetType();
        //      PdfParser parser = CreateParser(pdfObject.Document, type);
        //      return parser.ReadObject(pdfObject, objectID, false);
        //    }

        /// <summary>
        /// Reads an object from the PDF input stream using the default parser.
        /// </summary>
        public static PdfObject ReadObject(PdfDocument owner, PdfObjectID objectID)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            Parser parser = new Parser(owner);
            return parser.ReadObject(null, objectID, false, false);
        }

        /// <summary>
        /// Reads the irefs from the compressed object with the specified index in the object stream
        /// of the object with the specified object id.
        /// </summary>
        internal void ReadIRefsFromCompressedObject(PdfObjectID objectID)
        {
            Debug.Assert(_document.IrefTable.ObjectTable.ContainsKey(objectID));
            if (!_document.IrefTable.ObjectTable.TryGetValue(objectID, out var iref))
            {
                // We should never come here because the object stream must be a type 1 entry in the xref stream
                // and iref was created before.
                throw new NotImplementedException("This case is not coded or something else went wrong");
            }

            Debug.Assert(iref.Value != null, "The object shall be read in by PdfReader.ReadIndirectObjectsFromIrefTable() before accessing it.");

            if (iref.Value is not PdfObjectStream objectStreamStream)
            {
                Debug.Assert(((PdfDictionary)iref.Value).Elements.GetName("/Type") == "/ObjStm");

                objectStreamStream = new PdfObjectStream((PdfDictionary)iref.Value);
                Debug.Assert(objectStreamStream.Reference == iref);
                // objectStream.Reference = iref; Superfluous, see Assert in line before.
                Debug.Assert(objectStreamStream.Reference.Value != null, "Something went wrong.");
            }
            Debug.Assert(objectStreamStream != null);

            //PdfObjectStream objectStreamStream = (PdfObjectStream)iref.Value;
            if (objectStreamStream == null)
                throw new Exception("Something went wrong here.");
            objectStreamStream.ReadReferences(_document.IrefTable);
        }

        /// <summary>
        /// Reads the compressed object with the specified index in the object stream
        /// of the object with the specified object ID.
        /// </summary>
        internal PdfReference ReadCompressedObject(PdfObjectID objectID, int index)
        {
#if true
            Debug.Assert(_document.IrefTable.ObjectTable.ContainsKey(objectID));
            if (!_document.IrefTable.ObjectTable.TryGetValue(objectID, out var iref))
            {
                throw new NotImplementedException("This case is not coded or something else went wrong");
            }
#else
            // We should never come here because the object stream must be a type 1 entry in the xref stream
            // and iref was created before.

            // Has the specified object already an iref in the object table?
            if (!_document._irefTable.ObjectTable.TryGetValue(objectID, out iref))
            {
                try
                {
#if true_
                    iref = new PdfReference(objectID,);
                    iref.ObjectID = objectID;
                    _document._irefTable.Add(os);
#else
                    PdfDictionary dict = (PdfDictionary)ReadObject(null, objectID, false, false);
                    PdfObjectStream os = new PdfObjectStream(dict);
                    iref = new PdfReference(os);
                    iref.ObjectID = objectID;
                    _document._irefTable.Add(os);
#endif
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
#endif

            Debug.Assert(iref.Value != null, "The object shall be read in by PdfReader.ReadIndirectObjectsFromIrefTable() before accessing it.");

            var objectStreamStream = iref.Value as PdfObjectStream;
            if (objectStreamStream == null)
            {
                Debug.Assert(((PdfDictionary)iref.Value).Elements.GetName("/Type") == "/ObjStm");

                objectStreamStream = new PdfObjectStream((PdfDictionary)iref.Value);
                Debug.Assert(objectStreamStream.Reference == iref);
                // objectStream.Reference = iref; Superfluous, see Assert in line before.
                Debug.Assert(objectStreamStream.Reference.Value != null, "Something went wrong.");
            }
            Debug.Assert(objectStreamStream != null);

            //PdfObjectStream objectStreamStream = (PdfObjectStream)iref.Value;
            if (objectStreamStream == null)
                throw new Exception("Something went wrong here.");
            return objectStreamStream.ReadCompressedObject(index);
        }

        /// <summary>
        /// Reads the compressed object with the specified number at the given offset.
        /// The parser must be initialized with the stream an object stream object.
        /// </summary>
        internal PdfReference ReadCompressedObject(int objectNumber, int offset)
        {
#if DEBUG__
            if (objectNumber == 1034)
                GetType();
#endif
            // Generation is always 0 for compressed objects.
            PdfObjectID objectID = new PdfObjectID(objectNumber);
            _lexer.Position = offset;
            PdfObject obj = ReadObject(null, objectID, false, true);
            return obj.ReferenceNotNull;
        }

        /// <summary>
        /// Reads the object stream header as pairs of integers from the beginning of the 
        /// stream of an object stream. Parameter first is the value of the First entry of
        /// the object stream object.
        /// </summary>
        internal int[][] ReadObjectStreamHeader(int n, int first)
        {
            // TODO: Concept for general error  handling.
            // If the stream is corrupted a lot of things can go wrong here.
            // Does it make sense to do a more detailed error checking?

            // Create n pairs of integers with object number and offset.
            int[][] header = new int[n][];
            for (int idx = 0; idx < n; idx++)
            {
                int number = ReadInteger();
#if DEBUG
                if (number == 1074)
                    GetType();
#endif
                int offset = ReadInteger() + first;  // Calculate absolute offset.
                header[idx] = new int[] { number, offset };
            }
            return header;
        }

        /// <summary>
        /// Reads the cross-reference table(s) and their trailer dictionary or
        /// cross-reference streams.
        /// </summary>
        internal PdfTrailer ReadTrailer()
        {
            int length = _lexer.PdfLength;

            // Implementation note 18 Appendix  H:
            // Acrobat viewers require only that the %%EOF marker appear somewhere within the last 1024 bytes of the file.
            int idx;
            if (length < 1030)
            {
                // Reading the final 30 bytes should work for all files. But often it does not.
                string trail = _lexer.ReadRawString(length - 31, 30); //lexer.Pdf.Substring(length - 30);
                idx = trail.LastIndexOf("startxref", StringComparison.Ordinal);
                _lexer.Position = length - 31 + idx;
            }
            else
            {
                // For larger files we read 1 kiB - in most cases we find "startxref" in that range.
                string trail = _lexer.ReadRawString(length - 1031, 1030);
                idx = trail.LastIndexOf("startxref", StringComparison.Ordinal);
                _lexer.Position = length - 1031 + idx;
            }

            // SAP sometimes creates files with a size of several MByte and places "startxref" somewhere in the middle...
            if (idx == -1)
            {
                // If "startxref" was still not found yet, read the file completely.
                string trail = _lexer.ReadRawString(0, length);
                idx = trail.LastIndexOf("startxref", StringComparison.Ordinal);

                if (idx == -1)
                    throw new Exception("The StartXRef table could not be found, the file cannot be opened.");

                _lexer.Position = idx;
            }

            ReadSymbol(Symbol.StartXRef);
            _lexer.Position = ReadInteger();

            // Read all trailers.
            PdfTrailer? newerTrailer = null;
            while (true)
            {
                var trailer = ReadXRefTableAndTrailer(_document.IrefTable);

                // Return the first found trailer, which is the one startxref points to.
                // This is the current trailer, even for incrementally updated files.
                if (_document.Trailer == null!)
                    _document.Trailer = trailer ?? NRT.ThrowOnNull<PdfTrailer>();

                // Add previous trailer to the newerTrailer, if existing.
                if (newerTrailer != null)
                    newerTrailer.PreviousTrailer = trailer;

                // Break, if there is no previous trailer.
                int prev = trailer != null ? trailer.Elements.GetInteger(PdfTrailer.Keys.Prev) : 0;
                if (prev == 0)
                    break;
                //if (prev > lexer.PdfLength)
                //  break;

                // Continue loading previous trailer and cache this one as the newerTrailer to add its previous trailer.
                _lexer.Position = prev;
                newerTrailer = trailer;
            }

            return _document.Trailer;
        }

        /// <summary>
        /// Reads cross reference table(s) and trailer(s).
        /// </summary>
        PdfTrailer? ReadXRefTableAndTrailer(PdfCrossReferenceTable xrefTable)
        {
            Debug.Assert(xrefTable != null);

            Symbol symbol = ScanNextToken();

            if (symbol == Symbol.XRef)  // Is it a cross-reference table?
            {
                // Reference: 3.4.3  Cross-Reference Table / Page 93
                while (true)
                {
                    symbol = ScanNextToken();
                    if (symbol == Symbol.Integer)
                    {
                        int start = _lexer.TokenToInteger;
                        int length = ReadInteger();
                        for (int id = start; id < start + length; id++)
                        {
                            int position = ReadInteger();
                            int generation = ReadInteger();
                            ReadSymbol(Symbol.Keyword);
                            string token = _lexer.Token;
                            // Skip start entry.
                            if (id == 0)
                                continue;
                            // Skip unused entries.
                            if (token != "n")
                                continue;
#if true
                            //!!!new 2018-03-14 begin
                            // Check if the object at the address has the correct ID and generation.
                            int idToUse = id;
                            if (!CheckXRefTableEntry(position, id, generation, out var idChecked, out var generationChecked))
                            {
                                // Found the keyword "obj", but ID or generation did not match.
                                // There is a tool where ID is off by one. In this case we use the ID from the object, not the ID from the XRef table.
                                if (generation == generationChecked && id == idChecked + 1)
                                    idToUse = idChecked;
                                else
                                {
                                    //!!!new 2022-12-20 File is corrupt, but try to recover it by using the ID we found at the location.
                                    idToUse = idChecked;
                                    //ParserDiagnostics.ThrowParserException("Invalid entry in XRef table, ID=" + id + ", Generation=" + generation + ", Position=" + position + ", ID of referenced object=" + idChecked + ", Generation of referenced object=" + generationChecked);  // TODO L10N using PSSR.
                                }
                            }
                            //!!!new 2018-03-14 end
#endif
                            // Even if it is restricted, an object can exist in more than one subsection.
                            // (PDF Reference Implementation Notes 15).
                            PdfObjectID objectID = new PdfObjectID(idToUse, generation);
                            // Ignore the latter one.
                            if (xrefTable.Contains(objectID))
                                continue;
                            xrefTable.Add(new PdfReference(objectID, position));
                        }
                    }
                    else if (symbol == Symbol.Trailer)
                    {
                        ReadSymbol(Symbol.BeginDictionary);
                        PdfTrailer trailer = new PdfTrailer(_document);
                        ReadDictionary(trailer, false);
                        return trailer;
                    }
                    else
                        ParserDiagnostics.HandleUnexpectedToken(_lexer.Token);
                }
            }
            // ReSharper disable once RedundantIfElseBlock because of code readability.
            else if (symbol == Symbol.Integer) // Is it an cross-reference stream?
            {
                // Reference: 3.4.7  Cross-Reference Streams / Page 93
                // TODO: Handle PDF files larger than 2 GiB, see implementation note 21 in Appendix H.

                // The parsed integer is the object id of the cross-reference stream.
                return ReadXRefStream(xrefTable);
            }
            return null;
        }

        /// <summary>
        /// Checks the x reference table entry. Returns true if everything is correct.
        /// Return false if the keyword "obj" was found, but ID or Generation are incorrect.
        /// Throws an exception otherwise.
        /// </summary>
        /// <param name="position">The position where the object is supposed to be.</param>
        /// <param name="id">The ID from the XRef table.</param>
        /// <param name="generation">The generation from the XRef table.</param>
        /// <param name="idChecked">The identifier found in the PDF file.</param>
        /// <param name="generationChecked">The generation found in the PDF file.</param>
        /// <returns></returns>
        bool CheckXRefTableEntry(int position, int id, int generation, out int idChecked, out int generationChecked)
        {
            int origin = _lexer.Position;
            idChecked = -1;
            generationChecked = -1;
            try
            {
                _lexer.Position = position;
                idChecked = ReadInteger();
                generationChecked = ReadInteger();
                //// TODO Should we use ScanKeyword here?
                //ReadKSymbol(Symbol.Keyword);
                //string token = _lexer.Token;
                Symbol symbol = _lexer.ScanNextToken();
                if (symbol != Symbol.Obj)
                    ParserDiagnostics.ThrowParserException("Invalid entry in XRef table, ID=" + id + ", Generation=" + generation + ", Position=" + position); // TODO L10N using PSSR.

                if (id != idChecked || generation != generationChecked)
                    return false;
            }
            catch (PdfReaderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ParserDiagnostics.ThrowParserException("Invalid entry in XRef table, ID=" + id + ", Generation=" + generation + ", Position=" + position, ex); // TODO L10N using PSSR.
            }
            finally
            {
                _lexer.Position = origin;
            }
            return true;
        }

        /// <summary>
        /// Reads cross reference stream(s).
        /// </summary>
        PdfTrailer ReadXRefStream(PdfCrossReferenceTable xrefTable)
        {
            // Read cross reference stream.
            //Debug.Assert(_lexer.Symbol == Symbol.Integer);

            var xrefStart = _lexer.Position - _lexer.Token.Length;

            int number = _lexer.TokenToInteger;
            int generation = ReadInteger();
            // According to specs, generation number "shall not" be "other than zero".
            // Debug.Assert(generation == 0);

            // 7.5.7 Object streams:
            // "The following objects shall not be stored in an object stream: [...]
            // Objects with a generation number other than zero"

            ReadSymbol(Symbol.Obj);
            ReadSymbol(Symbol.BeginDictionary);
            var objectID = new PdfObjectID(number, generation);

            var xrefStream = new PdfCrossReferenceStream(_document);

            ReadDictionary(xrefStream, false);
            ReadSymbol(Symbol.BeginStream);
            ReadStream(xrefStream);

            var iref = new PdfReference(xrefStream)
            {
                ObjectID = objectID,
                Value = xrefStream,
                Position = xrefStart
            };
            xrefTable.Add(iref);

            Debug.Assert(xrefStream.Stream != null);
            //string sValue = new RawEncoding().GetString(xrefStream.Stream.UnfilteredValue,);
            //sValue.GetType();
            byte[] bytes = xrefStream.Stream.UnfilteredValue;

#if DEBUG_
            for (int idx = 0; idx < bytes.Length; idx++)
            {
                if (idx % 4 == 0)
                    Console.WriteLine();
                Console.Write("{0:000} ", (int)bytes[idx]);
            }
            Console.WriteLine();
#endif
            //     bytes.GetType();
            // Add to table.
            //    xrefTable.Add(new PdfReference(objectID, -1));

            int size = xrefStream.Elements.GetInteger(PdfCrossReferenceStream.Keys.Size);
            var index = xrefStream.Elements.GetValue(PdfCrossReferenceStream.Keys.Index) as PdfArray;
            int prev = xrefStream.Elements.GetInteger(PdfCrossReferenceStream.Keys.Prev);
            var w = (PdfArray?)xrefStream.Elements.GetValue(PdfCrossReferenceStream.Keys.W);

            // E.g.: W[1 2 1] ¤ Index[7 12] ¤ Size 19

            // Setup subsections.
            int subsectionCount;
            int[][] subsections = null!;
            int subsectionEntryCount = 0;
            if (index == null)
            {
                // Setup with default values.
                subsectionCount = 1;
                subsections = new int[subsectionCount][];
                subsections[0] = new int[] { 0, size }; // HACK: What is size? Contradiction in PDF reference.
                subsectionEntryCount = size;
            }
            else
            {
                // Read subsections from array.
                Debug.Assert(index.Elements.Count % 2 == 0);
                subsectionCount = index.Elements.Count / 2;
                subsections = new int[subsectionCount][];
                for (int idx = 0; idx < subsectionCount; idx++)
                {
                    subsections[idx] = new int[] { index.Elements.GetInteger(2 * idx), index.Elements.GetInteger(2 * idx + 1) };
                    subsectionEntryCount += subsections[idx][1];
                }
            }

            // W key.
            Debug.Assert(w!.Elements.Count == 3);
            int[] wsize = { w.Elements.GetInteger(0), w.Elements.GetInteger(1), w.Elements.GetInteger(2) };
            int wsum = StreamHelper.WSize(wsize);
#if DEBUG
            if (wsum * subsectionEntryCount != bytes.Length)
                GetType();
#endif
            // BUG: This assertion fails with original PDF 2.0 documentation (ISO_32000-2_2020(en).pdf)
            //Debug.Assert(wsum * subsectionEntryCount == bytes.Length, "Check implementation here.");
#if DEBUG_ && CORE
            if (PdfDiagnostics.TraceXrefStreams)
            {
                int testcount = subsections[0][1];
                int[] currentSubsection = subsections[0];
                for (int idx = 0; idx < testcount; idx++)
                {
                    uint field1 = StreamHelper.ReadBytes(bytes, idx * wsum, wsize[0]);
                    uint field2 = StreamHelper.ReadBytes(bytes, idx * wsum + wsize[0], wsize[1]);
                    uint field3 = StreamHelper.ReadBytes(bytes, idx * wsum + wsize[0] + wsize[1], wsize[2]);
                    string res = String.Format("{0,2:00}: {1} {2,5} {3}  // ", idx, field1, field2, field3);
                    switch (field1)
                    {
                        case 0:
                            res += "Free list entry: object number, generation number";
                            break;

                        case 1:
                            res += "Not compressed: offset, generation number";
                            break;

                        case 2:
                            res += "Compressed: object stream object number, index in stream";
                            break;

                        default:
                            res += "??? Type undefined";
                            break;
                    }
                    Debug.WriteLine(res);
                }
            }
#endif
            int index2 = -1;
            for (int ssc = 0; ssc < subsectionCount; ssc++)
            {
                int abc = subsections[ssc][1];
                for (int idx = 0; idx < abc; idx++)
                {
                    index2++;

                    CrossReferenceStreamEntry item = new()
                    {
                        Type = StreamHelper.ReadBytes(bytes, index2 * wsum, wsize[0]),
                        Field2 = StreamHelper.ReadBytes(bytes, index2 * wsum + wsize[0], wsize[1]),
                        Field3 = StreamHelper.ReadBytes(bytes, index2 * wsum + wsize[0] + wsize[1], wsize[2])
                    };

                    xrefStream.Entries.Add(item);

                    switch (item.Type)
                    {
                        case 0:
                            // Nothing to do, not needed.
                            break;

                        case 1: // offset / generation number
                            //// Even it is restricted, an object can exist in more than one subsection.
                            //// (PDF Reference Implementation Notes 15).

                            int position = (int)item.Field2;
                            objectID = ReadObjectNumber(position);
#if DEBUG_
                            if (objectID.ObjectNumber == 1074)
                                GetType();
#endif
                            Debug.Assert(objectID.GenerationNumber == item.Field3);

                            //// Ignore the latter one.
                            if (!xrefTable.Contains(objectID))
                            {
#if DEBUG_
                                GetType();
#endif
                                // Add iref for all uncompressed objects.
                                xrefTable.Add(new PdfReference(objectID, position));

                            }
#if DEBUG
                            else
                            {
                                GetType();
                            }
#endif
                            break;

                        case 2:
                            // Nothing to do yet.
                            break;
                    }
                }
            }
            return xrefStream;
        }

        /// <summary>
        /// Parses a PDF date string.
        /// </summary>
        internal static DateTime ParseDateTime(string date, DateTime errorValue)  // TODO: TryParseDateTime
        {
            DateTime datetime = errorValue;
            try
            {
                if (date.StartsWith("D:", StringComparison.Ordinal))
                {
                    // Format is
                    // D:YYYYMMDDHHmmSSOHH'mm'
                    //   ^2      ^10   ^16 ^20
                    int length = date.Length;
                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, hh = 0, mm = 0;
                    char o = 'Z';
                    if (length >= 10)
                    {
                        year = Int32.Parse(date.Substring(2, 4));
                        month = Int32.Parse(date.Substring(6, 2));
                        day = Int32.Parse(date.Substring(8, 2));
                        if (length >= 16)
                        {
                            hour = Int32.Parse(date.Substring(10, 2));
                            minute = Int32.Parse(date.Substring(12, 2));
                            second = Int32.Parse(date.Substring(14, 2));
                            if (length >= 23)
                            {
                                if ((o = date[16]) != 'Z')
                                {
                                    hh = Int32.Parse(date.Substring(17, 2));
                                    mm = Int32.Parse(date.Substring(20, 2));
                                }
                            }
                        }
                    }
                    // There are miserable PDF tools around the world.
                    month = Math.Min(Math.Max(month, 1), 12);
                    datetime = new DateTime(year, month, day, hour, minute, second);
                    if (o != 'Z')
                    {
                        TimeSpan ts = new TimeSpan(hh, mm, 0);
                        if (o == '-')
                            datetime = datetime.Add(ts);
                        else
                            datetime = datetime.Subtract(ts);
                    }
                    // Now that we converted datetime to UTC, mark it as UTC.
                    datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
                }
                else
                {
                    // Some libraries use plain English format.
                    datetime = DateTime.Parse(date, CultureInfo.InvariantCulture);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception ex)
            {
                // If we cannot parse datetime, just eat it, but give a hint in DEBUG build.
                Debug.Assert(false, ex.Message);
            }
            return datetime;
        }

        //    /// <summary>
        //    /// Creates a parser for the specified PDF object type. A PDF object can define a specialized
        //    /// parser in the optional PdfObjectInfoAttribute. If no parser is specified, the default
        //    /// Parser object is returned.
        //    /// </summary>
        //    public static Parser CreateParser(PdfDocument document, Type pdfObjectType)
        //    {
        //      // TODO: ParserFactory
        //      object[] attribs = null; //pdfObjectType.GetCustomAttributes(typeof(PdfObjectInfoAttribute), false);
        //      if (attribs.Length == 1)
        //      {
        //        PdfObjectInfoAttribute attrib = null; //(PdfObjectInfoAttribute)attribs[0];
        //        Type parserType = attrib.Parser;
        //        if (parserType != null)
        //        {
        //          ConstructorInfo ctorInfo = parserType.GetConstructor(
        //            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
        //            new Type[]{typeof(PdfDocument)}, null);
        //          Parser parser = (Parser)ctorInfo.Invoke(new object[]{document});
        //          Debug.Assert(parser != null, "Creation of parser failed.");
        //          return parser;
        //        }
        //      }
        //      return new Parser(document);
        //    }

        /*
            /// <summary>
            /// Reads a date value directly or (optionally) indirectly from the PDF data stream.
            /// </summary>
            protected DateTime ReadDate(bool canBeIndirect)
            {
              Symbol symbol = lexer.ScanNextToken(canBeIndirect);
              if (symbol == Symbol.String || symbol == Symbol.UnicodeString || symbol == Symbol.HexString || symbol == Symbol.UnicodeHexString)
              {
                // D:YYYYMMDDHHmmSSOHH'mm'
                //   ^2      ^10   ^16 ^20
                string date = lexer.Token;
                int length = date.Length;
                int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, hh = 0, mm = 0;
                char o = 'Z';
                if (length >= 10)
                {
                  year = Int32.Parse(date.Substring(2, 4));
                  month = Int32.Parse(date.Substring(6, 2));
                  day = Int32.Parse(date.Substring(8, 2));
                  if (length >= 16)
                  {
                    hour = Int32.Parse(date.Substring(10, 2));
                    minute = Int32.Parse(date.Substring(12, 2));
                    second = Int32.Parse(date.Substring(14, 2));
                    if (length >= 23)
                    {
                      if ((o = date[16]) != 'Z')
                      {
                        hh = Int32.Parse(date.Substring(17, 2));
                        mm = Int32.Parse(date.Substring(20, 2));
                      }
                    }
                  }
                }
                DateTime datetime = new DateTime(year, month, day, hour, minute, second);
                if (o != 'Z')
                {
                  TimeSpan ts = new TimeSpan(hh, mm, 0);
                  if (o == '+')
                    datetime.Add(ts);
                  else
                    datetime.Subtract(ts);
                }
                return datetime;
              }
              else if (symbol == Symbol.R)
              {
                int position = lexer.Position;
                MoveToObject(lexer.Token);
                ReadObjectID(null);
                DateTime d = ReadDate();
                ReadSymbol(Symbol.EndObj);
                lexer.Position = position;
                return d;
              }
              thr ow new PdfReaderException(PSSR.UnexpectedToken(lexer.Token));
            }

            protected DateTime ReadDate()
            {
              return ReadDate(false);
            }

            /// <summary>
            /// Reads a PdfRectangle value directly or (optionally) indirectly from the PDF data stream.
            /// </summary>
            protected PdfRectangle ReadRectangle(bool canBeIndirect)
            {
              Symbol symbol = lexer.ScanNextToken(canBeIndirect);
              if (symbol == Symbol.BeginArray)
              {
                PdfRectangle rect = new PdfRectangle();
                rect.X1 = ReadReal();
                rect.Y1 = ReadReal();
                rect.X2 = ReadReal();
                rect.Y2 = ReadReal();
                ReadSymbol(Symbol.EndArray);
                return rect;
              }
              else if (symbol == Symbol.R)
              {
                int position = lexer.Position;
                MoveToObject(lexer.Token);
                ReadObjectID(null);
                PdfRectangle rect = ReadRectangle();
                ReadSymbol(Symbol.EndObj);
                lexer.Position = position;
                return rect;
              }
              thr ow new PdfReaderException(PSSR.UnexpectedToken(lexer.Token));
            }

            /// <summary>
            /// Short cut for ReadRectangle(false).
            /// </summary>
            protected PdfRectangle ReadRectangle()
            {
              return ReadRectangle(false);
            }

            /// <summary>
            /// Reads a generic dictionary.
            /// </summary>
            protected PdfDictionary ReadDictionary(bool canBeIndirect)
            {
              // Just read over dictionary
              PdfDictionary dictionary = new PdfDictionary();
              Symbol symbol = lexer.ScanNextToken(canBeIndirect);
              if (symbol == Symbol.BeginDictionary)
              {
                int nestingLevel = 0;
                symbol = ScanNextToken();
                while (symbol != Symbol.Eof)
                {
                  switch (symbol)
                  {
                    case Symbol.BeginDictionary:
                      nestingLevel++;
                      break;

                    case Symbol.EndDictionary:
                      if (nestingLevel == 0)
                        return dictionary;
                      else
                        nestingLevel--;
                      break;
                  }
                  symbol = ScanNextToken();
                }
                Debug.Assert(false, "Must not come here");
                return dictionary;
              }
              else if (symbol == Symbol.R)
              {
                return dictionary;
              }
              thr ow new PdfReaderException(PSSR.UnexpectedToken(lexer.Token));
            }

            /// <summary>
            /// Short cut for ReadDictionary(false).
            /// </summary>
            protected PdfDictionary ReadDictionary()
            {
              return ReadDictionary(false);
            }

            /// <summary>
            /// Reads a generic array.
            /// </summary>
            protected PdfArray ReadArray(bool canBeIndirect)
            {
              // Just read over array
              PdfArray array = new PdfArray();
              Symbol symbol = lexer.ScanNextToken(canBeIndirect);
              if (symbol == Symbol.BeginArray)
              {
                int nestingLevel = 0;
                symbol = ScanNextToken();
                while (symbol != Symbol.Eof)
                {
                  switch (symbol)
                  {
                    case Symbol.BeginArray:
                      nestingLevel++;
                      break;

                    case Symbol.EndArray:
                      if (nestingLevel == 0)
                        return array;
                      else
                        nestingLevel--;
                      break;
                  }
                  symbol = ScanNextToken();
                }
                Debug.Assert(false, "Must not come here");
                return array;
              }
              else if (symbol == Symbol.R)
              {
                return array;
              }
              th row new PdfReaderException(PSSR.UnexpectedToken(lexer.Token));
            }

            protected PdfArray ReadArray()
            {
              return ReadArray(false);
            }

            protected object ReadGeneric(KeysMeta meta, string token)
            {
              KeyDescriptor descriptor =  meta[token];
              Debug.Assert(descriptor != null);
              object result = null;
              switch (descriptor.KeyType & KeyType.TypeMask)
              {
                case KeyType.Name:
                  result = ReadName();
                  break;

                case KeyType.String:
                  result = ReadString(descriptor.CanBeIndirect);
                  break;

                case KeyType.Boolean:
                  result = ReadBoolean(descriptor.CanBeIndirect);
                  break;

                case KeyType.Integer:
                  result = ReadInteger(descriptor.CanBeIndirect);
                  break;

                case KeyType.Real:
                  result = ReadReal(descriptor.CanBeIndirect);
                  break;

                case KeyType.Date:
                  result = ReadDate(descriptor.CanBeIndirect);
                  break;

                case KeyType.Rectangle:
                  result = ReadRectangle(descriptor.CanBeIndirect);
                  break;

                case KeyType.Array:
                  result = ReadArray(descriptor.CanBeIndirect);
                  break;

                case KeyType.Dictionary:
                  result = ReadDictionary(descriptor.CanBeIndirect);
                  break;

                case KeyType.Stream:
                  break;

                case KeyType.NumberTree:
                  thr ow new NotImplementedException("KeyType.NumberTree");

                case KeyType.NameOrArray:
                  char ch = lexer.MoveToNonWhiteSpace();
                  if (ch == '/')
                    result = ReadName();
                  else if (ch == '[')
                    result = ReadArray();
                  else
                    th row new NotImplementedException("KeyType.NameOrArray");
                  break;

                case KeyType.ArrayOrDictionary:
                  thr ow new NotImplementedException("KeyType.ArrayOrDictionary");
              }
              //Debug.Assert(false, "ReadGeneric");
              return result;
            }

            //    /// <summary>
            //    /// Gets the current symbol from the lexer.
            //    /// </summary>
            //    protected Symbol Symbol
            //    {
            //      get {return lexer.Symbol;}
            //    }
            //
            //    /// <summary>
            //    /// Gets the current token from the lexer.
            //    /// </summary>
            //    protected string Token
            //    {
            //      get {return lexer.Token.ToString();}
            //    }

            public static object Read(PdfObject o, string key)
            {
              return null;
            }
        */

        ParserState SaveState()
        {
            var state = new ParserState
            {
                Position = _lexer.Position,
                Symbol = _lexer.Symbol
            };
            return state;
        }

        void RestoreState(ParserState state)
        {
            _lexer.Position = state.Position;
            _lexer.Symbol = state.Symbol;
        }

        class ParserState
        {
            public int Position;
            public Symbol Symbol;
        }

        readonly PdfDocument _document;
        readonly Lexer _lexer;
        readonly ShiftStack _stack;
    }

    static class StreamHelper
    {
        public static int WSize(int[] w)
        {
            Debug.Assert(w.Length == 3);
            return w[0] + w[1] + w[2];
        }

        public static uint ReadBytes(byte[] bytes, int index, int byteCount)
        {
            uint value = 0;
            for (int idx = 0; idx < byteCount; idx++)
            {
                value *= 256;
                value += bytes[index + idx];
            }
            return value;
        }
    }
}
