// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Logging;
using PdfSharp.Pdf.Advanced;
using Microsoft.Extensions.Logging;
using PdfSharp.Internal.Logging;

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
        /// <summary>
        /// Constructs a parser for a document.
        /// </summary>
        public Parser(PdfDocument document, PdfReaderOptions options, ILogger? logger)
        {
            _document = document;
            _options = options;
            _lexer = document._lexer ?? throw new ArgumentNullException(nameof(document), "Lexer not defined.");
            _documentParser = this;
            _logger = logger ?? LogHost.CreateLogger(LogCategory.PdfReading);
        }

        /// <summary>
        /// Constructs a parser for an ObjectStream.
        /// </summary>
        public Parser(PdfDocument? document, Stream objectStream, Parser documentParser)
        {
            _document = document!; // NRT HACK
            _options = documentParser._options;
            _lexer = new Lexer(objectStream, documentParser._logger);
            _documentParser = documentParser;
            _logger = documentParser._logger ?? LogHost.CreateLogger(LogCategory.PdfReading);
        }

        /// <summary>
        /// Sets PDF input stream position to the specified object.
        /// </summary>
        public SizeType MoveToObject(PdfObjectID objectID)
        {
            SizeType position = _document.IrefTable[objectID]?.Position ?? throw new AggregateException($"Invalid object ID {objectID}.");
            if (position < 0)
                throw new AggregateException($"Invalid position {position} for object ID {objectID}.");

            return _lexer.Position = position;
        }

        /// <summary>
        /// Gets the current symbol from the underlying lexer.
        /// </summary>
        public Symbol Symbol => _lexer.Symbol;

        public PdfObjectID ReadObjectNumber(SizeType position)
        {
            _lexer.Position = position;
            int objectNumber = ReadInteger();
            int generationNumber = ReadInteger();
            return new(objectNumber, generationNumber);
        }

        /// <summary>
        /// Internal function to read PDF object from input stream.
        /// </summary>
        /// <param name="pdfObject">Either the instance of a derived type or null. If it is null
        /// an appropriate object is created.</param>
        /// <param name="objectID">The address of the object.</param>
        /// <param name="includeReferences">If true, specifies that all indirect objects
        /// are included recursively.</param>
        /// <param name="fromObjectStream">If true, the object is parsed from an object stream.</param>
        PdfObject ReadObjectInternal(PdfObject? pdfObject, PdfObjectID objectID, bool includeReferences, bool fromObjectStream)
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
                // Reading from 'classical' object.
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
            if (fromObjectStream is false && objectID != new PdfObjectID(objectNumber, generationNumber))
            {
                // Investigate if this happens. Please send us the PDF file for analysing it.
                LogHost.Logger.LogError("Something happened in an object stream that is not expected but can be ignored.");
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
            if (fromObjectStream is false)
                ReadSymbol(Symbol.Obj);

            bool checkForStream = false;
            var symbol = ScanNextToken();
            switch (symbol)
            {
                case Symbol.BeginArray:
                    {
                        PdfArray array;
                        if (pdfObject == null)
                            array = new PdfArray(_document);
                        else
                            array = (PdfArray)pdfObject;

                        pdfObject = ReadArray(array, includeReferences);
                        pdfObject.SetObjectID(objectNumber, generationNumber);
                    }
                    break;

                case Symbol.BeginDictionary:
                    {
                        PdfDictionary dict;
                        if (pdfObject == null)
                            dict = new PdfDictionary(_document);
                        else
                            dict = (PdfDictionary)pdfObject;
                        checkForStream = true;
                        pdfObject = ReadDictionary(dict, includeReferences);
                        pdfObject.SetObjectID(objectNumber, generationNumber);
                    }
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

                case Symbol.LongInteger:
                    pdfObject = new PdfLongIntegerObject(_document, _lexer.TokenToLongInteger);
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
                case Symbol.HexString:
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
                    ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
                    break;

                default:
                    // Should not come here anymore.
                    ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
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

                var stream = new PdfDictionary.PdfStream(bytes, dict ?? NRT.ThrowOnNull<PdfDictionary>());
                dict.Stream = stream;
                ReadSymbol(Symbol.EndStream);
                symbol = ScanNextToken();
#endif
            }
            if (fromObjectStream is false && symbol != Symbol.EndObj)
                ParserDiagnostics.ThrowParserException(PSSR.UnexpectedToken(_lexer.Token));
            return pdfObject ?? NRT.ThrowOnNull<PdfObject>();
        }

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
                        throw TH.ObjectNotAvailableException_CannotRetrieveStreamLengthByNow();

                    length = integer;
                }
                else
                {
                    var state = SaveState();
                    try
                    {
                        length = ReadIndirectObject(reference);
                    }
                    catch (Exception e)
                    {
                        throw TH.ObjectNotAvailableException_CannotRetrieveStreamLengthByNow(e);
                    }
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
            throw TH.ObjectNotAvailableException_CannotRetrieveStreamLength();
        }

        public PdfArray ReadArray(PdfArray array, bool includeReferences)
        {
            Debug.Assert(Symbol == Symbol.BeginArray);

            if (array == null!)
                array = new PdfArray(_document);

            var items = new List<PdfItem>();
            int count;

            items = ParseObject(Symbol.EndArray);
            count = items.Count;
            for (int idx = 0; idx < count; idx++)
            {
                PdfItem val = items[idx];
                if (includeReferences && val is PdfReference reference)
                    val = _documentParser.ReadReference(reference, true); // ReadReference() has to be always called for the document, even if called from ObjectStream parser.
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
            var xxx = _lexer.GetNeighborhoodOfCurrentPosition();
            var yyy = _lexer.GetNeighborhoodOfCurrentPosition(true);
#endif

#if DEBUG_
            ReadDictionaryCounter++;
            Debug.WriteLine(ReadDictionaryCounter.ToString());
            if (ReadDictionaryCounter == 101)
                GetType();
#endif
            dict ??= new PdfDictionary(_document);
            DictionaryMeta meta = dict.Meta;

            var items = ParseObject(Symbol.EndDictionary);
            int count = items.Count;
            for (int idx = 0; idx < count; idx += 2)
            {
                var val = items[idx];
                if (val is not PdfName)
                    ParserDiagnostics.ThrowParserException("Name expected."); // TODO L10N using PSSR.

                string key = val.ToString() ?? NRT.ThrowOnNull<string>();
                val = items[idx + 1];
                if (includeReferences && val is PdfReference reference)
                    val = _documentParser.ReadReference(reference, true); // ReadReference() has to be always called for the document, even if called from ObjectStream parser.
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
        List<PdfItem> ParseObject(Symbol stop)
        {
#if DEBUG_
            ParseObjectCounter++;
            Debug.WriteLine(ParseObjectCounter.ToString());
            if (ParseObjectCounter == 178)
                GetType();
#endif
            //bool testForObjectReference = _options.UseOldCode == false;

            var items = new List<PdfItem>();
            Symbol symbol;
            while ((symbol = ScanNextToken(true)) != Symbol.Eof)
            {
                if (symbol == stop)
                    return items;

                switch (symbol)
                {
                    case Symbol.Comment:
                        // Ignore comments.
                        break;

                    case Symbol.Null:
                        items.Add(PdfNull.Value);
                        break;

                    case Symbol.Boolean:
                        items.Add(new PdfBoolean(_lexer.TokenToBoolean));
                        break;

                    case Symbol.Integer:
                        items.Add(new PdfInteger(_lexer.TokenToInteger));
                        break;

                    case Symbol.LongInteger:
                        items.Add(new PdfLongInteger(_lexer.TokenToLongInteger));
                        break;

                    case Symbol.Real:
                        items.Add(new PdfReal(_lexer.TokenToReal));
                        break;

                    case Symbol.String:
                        items.Add(new PdfString(_lexer.Token, PdfStringFlags.RawEncoding));
                        break;

                    case Symbol.HexString:
                        items.Add(new PdfString(_lexer.Token, PdfStringFlags.HexLiteral));
                        break;

                    case Symbol.Name:
                        items.Add(new PdfName(_lexer.Token));
                        break;

                    case Symbol.R:
                        //Debug.Assert(_options.UseOldCode == true, "Must not come here anymore");
                        Debug.Assert(false, "Must not come here anymore");
                        break;

                    case Symbol.ObjRef:
                        {
                            (int objectNumber, int generationNumber) = _lexer.TokenToObjectID;
                            var objectID = new PdfObjectID(objectNumber, generationNumber);

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
                                    items.Add(iref);
                                }
                                else
                                {
                                    // PDF Reference 2.0 section 7.3.10:
                                    // An indirect reference to an undefined object shall not be considered an error by a PDF processor;
                                    // it shall be treated as a reference to the null object.
                                    items.Add(PdfNull.Value);
                                }
                            }
                            else
                                items.Add(iref);
                            break;
                        }

                    case Symbol.BeginArray:
                        var array = new PdfArray(_document);
                        ReadArray(array, false);
                        items.Add(array);
                        break;

                    case Symbol.BeginDictionary:
                        var dict = new PdfDictionary(_document);
                        ReadDictionary(dict, false);
                        items.Add(dict);
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
                        ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
                        SkipCharsUntil(stop);
                        return items;
                }
            }
            ParserDiagnostics.ThrowParserException("Unexpected end of file."); // TODO L10N using PSSR.
            return items;  // Dummy code.
        }

        Symbol ScanNextToken(bool testForObjectReference = false)
            => _lexer.ScanNextToken(testForObjectReference);

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
            // Should not come here anymore.
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
                    ParserDiagnostics.HandleUnexpectedCharacter(ch, _lexer.DumpNeighborhoodOfPosition());

                if (ch != 'e')
                {
                    _lexer.ScanNextChar(false);
                    goto Skip;
                }
            }
            Symbol current = ScanNextToken(symbol == Symbol.ObjRef);
            if (symbol != current)
                ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
            return current;
        }

        /// <summary>
        /// Reads a name from the PDF data stream. The preceding slash is part of the result string.
        /// </summary>
        string ReadName()
        {
            var symbol = ScanNextToken(false);
            if (symbol != Symbol.Name)
                ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
            return _lexer.Token;
        }

        /// <summary>
        /// Reads an integer value directly from the PDF data stream.
        /// </summary>
        int ReadInteger()
        {
            Symbol symbol = _lexer.ScanNextToken(false);
            if (symbol == Symbol.Integer)
                return _lexer.TokenToInteger;

            if (symbol == Symbol.LongInteger)
            {
                // Should not happen or is a bug in the parser.
                Debug.Assert(false, "ReadInteger founds a long integer.");
            }

            if (symbol == Symbol.R)
            {
                Debug.Assert(false, "Should not come here.");

                SizeType position = _lexer.Position;
                //        MoveToObject(lexer.Token);
                ReadObjectID(null);
                int n = ReadInteger();
                ReadSymbol(Symbol.EndObj);
                _lexer.Position = position;
                return n;
            }
            ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
            return 0;
        }

        /// <summary>
        /// Reads an offset value (int or long) directly from the PDF data stream.
        /// </summary>
        SizeType ReadSize()
        {
            Symbol symbol = _lexer.ScanNextToken(false);

#if USE_LONG_SIZE
            if (symbol is Symbol.Integer or Symbol.LongInteger)
                return _lexer.TokenToLongInteger;
#else
            if (symbol is Symbol.Integer)
                return _lexer.TokenToInteger;

            if (symbol is Symbol.LongInteger)
                throw new PdfReaderException("File too large"); // Should not come here.
#endif
            ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
            return 0;
        }

        /// <summary>
        /// Reads the PdfObject of the reference, no matter if it's saved at document level or inside an ObjectStream.
        /// </summary>
        internal PdfObject ReadIndirectObject(PdfReference pdfReference, bool withoutDecrypting = false)
        {
            try
            {
                var objectID = pdfReference.ObjectID;
                var pdfObject = pdfReference.Value;

                // Return already loaded PdfObject.
                if (pdfObject != null!)
                {
                    Debug.Assert(_document.IrefTable.Contains(objectID));
                    return pdfObject;
                }
                
                var isInObjectStream = pdfReference.Position == -1;

                // Option 1: Load object from ObjectStream.
                if (isInObjectStream)
                {
                    pdfObject = ReadIndirectObjectFromObjectStreamInternal(objectID);
                }
                // Option 2: Load object on file level.
                else
                {
                    pdfObject = ReadObjectInternal(null, objectID, false, false);

                    // Decrypt file level object.
                    if (!withoutDecrypting)
                        _document.EffectiveSecurityHandler?.DecryptObject(pdfObject);
                }

                // Set maximum object number.
                _document.IrefTable.MaxObjectNumber = Math.Max(_document.IrefTable.MaxObjectNumber, objectID.ObjectNumber);
                
                Debug.Assert(_document.IrefTable.Contains(pdfReference.ObjectID));

                return pdfObject;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // Rethrow exception to notify caller.
                throw;
            }
        }

        PdfObject ReadIndirectObjectFromObjectStreamInternal(PdfObjectID objectID)
        {
            var doNextRound = true;
            var checkedObjectStreams = new List<PdfObjectStream>();

            // If the object can't be loaded immediately, retry loading after updating ObjectStreams.
            while (doNextRound)
            {
                foreach (var objectStreamWithParser in _objectStreamsWithParsers.Values)
                {
                    var objectStream = objectStreamWithParser.ObjectStream;

                    if (checkedObjectStreams.Contains(objectStream))
                        continue;

                    checkedObjectStreams.Add(objectStream);

                    if (objectStream.TryGetObjectOffset(objectID, out var offset))
                    {
                        var objectStreamParser = objectStreamWithParser.Parser;
                        objectStreamParser._lexer.Position = offset;
                        var pdfObject = objectStreamParser.ReadObjectInternal(null, objectID, false, true);
                        return pdfObject;
                    }
                }

                Debug.WriteLine($"Reading indirect object with ID {objectID} from ObjectStream will be retried.");

                // Load skippedObjectStreamNumbers in the next round and reset skippedObjectStreamNumbers variable.
                var oldObjectStreamCount = _objectStreamsWithParsers.Count;
                // Maybe there are more references known now.
                ReadAllObjectStreamsAndTheirReferences();

                // Retry, if the number of ObjectStreams has changed.
                doNextRound = _objectStreamsWithParsers.Count > oldObjectStreamCount;
            }

            Debug.WriteLine($"Reading indirect object with ID {objectID} from ObjectStream finally failed.");

            throw TH.PdfReaderException_ObjectCouldNotBeFoundInObjectStreams();
        }

        /// <summary>
        /// Reads the PdfObjects of all known references, no matter if they are saved at document level or inside an ObjectStream.
        /// </summary>
        internal void ReadAllIndirectObjects()
        {
            var pdfReferences = _document.IrefTable.AllReferences;
            foreach (var pdfReference in pdfReferences)
            {
                if (pdfReference.Value == null!)
                {
                    var pdfObject = ReadIndirectObject(pdfReference);

                    Debug.Assert(pdfObject.Reference == pdfReference);
                    pdfObject.Reference = pdfReference;
                    Debug.Assert(pdfObject.Reference.Value != null, "Something went wrong.");
                }
                else
                {
                    Debug.Assert(_document.IrefTable.Contains(pdfReference.ObjectID));
                }
            }
        }

        List<PdfObjectID> LoadObjectStreamIDs(PdfReference[] pdfReferences)
        {
            // The PDF Reference 1.7 states in chapter 7.5.6 (Incremental Updates):
            // "When a conforming reader reads the file,
            //  it shall build its cross-reference information in such a way that the
            //  most recent copy of each object shall be the one accessed from the file."

            // IrefTable.AllReferences is sorted by ObjectId which gives older objects preference
            // (as they typically have lower ObjectIds).
            // For xref-streams, we revert the order, so that the most current object is read first.
            // This is because Parser.ReadObject does not overwrite an object that was already collected.

            // Collect xref streams.
            var xrefStreams = pdfReferences.Select(x => x.Value).OfType<PdfCrossReferenceStream>().ToList();
            // Sort them so the last xref stream is read first.
            // TODO: Is this always sufficient? (haven't found any issues so far testing with ~1300 PDFs...)
            xrefStreams.Sort((a, b) => (int)((b.Reference?.Position ?? 0) - (a.Reference?.Position ?? 0)));

            // By checking the CrossReferenceStream entries, we can get all ObjectStream IDs without loading all file level objects now.
            var objectStreamNumbers = new HashSet<int>();
            foreach (var xrefStream in xrefStreams)
            {
                Debug.Assert(_document.IrefTable.Contains(xrefStream.ObjectID));

                // Loop CrossReferenceStream entries to compressed objects.
                foreach (var item in xrefStream.Entries.Where(x => x.Type == 2))
                {
                    var objectStreamNumber = (int)item.Field2;
                    objectStreamNumbers.Add(objectStreamNumber);
                }
            }

            var objectStreamIDs = objectStreamNumbers.Select(x => new PdfObjectID(x)).ToList();
            return objectStreamIDs;
        }

        /// <summary>
        /// Reads all ObjectStreams and the references to the PdfObjects they hold.
        /// </summary>
        internal void ReadAllObjectStreamsAndTheirReferences()
        {
            var pdfReferences = _document.IrefTable.AllReferences;
            
            var objectStreamIDsToLoad = LoadObjectStreamIDs(pdfReferences);

            var skippedObjectStreamIDs = new List<PdfObjectID>();
            var doNextRound = true;

            // If any ObjectStream can't be loaded immediately because its stream length is saved in an ObjectStream, retry it later.
            // Repeat while not all object streams are load and object streams were still added in the last round, to retry loading for objects streams that could not be loaded by now.
            while (objectStreamIDsToLoad.Count > 0 && doNextRound)
            {
                doNextRound = false;
                foreach (var objectStreamID in objectStreamIDsToLoad)
                {
                    if (!_objectStreamsWithParsers.ContainsKey(objectStreamID))
                    {
                        PdfReference? objectStreamReference = null;
                        try
                        {
                            objectStreamReference = _document.IrefTable[objectStreamID];

                            var objectStream = ReadObjectStream(objectStreamReference!);

                            var objectStreamParser = new Parser(_document, new MemoryStream(objectStream.Stream.Value), _documentParser);
                            _objectStreamsWithParsers.Add(objectStreamID, (objectStream, objectStreamParser));

                            // Read and add all References to objects residing in the object stream.
                            objectStream.ReadReferences(_document.IrefTable);

                            // Try a next round as long as at least one ObjectStream could be loaded.
                            doNextRound = true;
                        }
                        catch (ObjectNotAvailableException)
                        {
                            // Exceptional case because of exception: Reset objectStreamReference.Value back to null, because it could not be read properly and that may cause further errors.
                            objectStreamReference?.SetObject(null!);

                            // Try it again in the next round.
                            skippedObjectStreamIDs.Add(objectStreamID);

                            Debug.WriteLine($"Loading ObjectStream with ID {objectStreamID} will be retried.");
                        }
                    }
                }

                // Load skippedObjectStreamNumbers in the next round and reset skippedObjectStreamNumbers variable.
                objectStreamIDsToLoad = skippedObjectStreamIDs;
                skippedObjectStreamIDs = [];
            }

            if (objectStreamIDsToLoad.Any())
            {
                Debug.WriteLine($"Loading ObjectStreams with IDs {String.Join(", ", objectStreamIDsToLoad)} finally failed.");

                throw new ObjectNotAvailableException($"Could not load the ObjectStreams with the following ObjectNumbers: {String.Join(", ", objectStreamIDsToLoad)}. " +
                                                      $"Perhaps there is a cyclic reference between ObjectStreams, whose stream lengths are saved in an object inside the other ObjectStream.");
            }
        }

        PdfObjectStream ReadObjectStream(PdfReference reference)
        {
            if (reference.Value == default!)
            {
                ReadIndirectObject(reference);
            }

            Debug.Assert(reference.Value != null, "The object shall be read in by PdfReader.ReadIndirectObjectsFromIrefTable() before accessing it.");

            if (reference.Value is not PdfObjectStream pdfObjectStream)
            {
                Debug.Assert(((PdfDictionary)reference.Value).Elements.GetName("/Type") == "/ObjStm");

                pdfObjectStream = new PdfObjectStream((PdfDictionary)reference.Value, _documentParser);
                Debug.Assert(pdfObjectStream.Reference == reference);
                // objectStream.Reference = iref; Superfluous, see Assert in line before.
                Debug.Assert(pdfObjectStream.Reference.Value != null, "Something went wrong.");
            }
            Debug.Assert(pdfObjectStream != null);

            if (pdfObjectStream == null)
                throw new Exception("Something went wrong here.");

            return pdfObjectStream;
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
#if DEBUG_
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
            SizeType length = _lexer.PdfLength;

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
                if (length > int.MaxValue)
                {
                    //TODO: Implement chunking to read long files.
                    throw new NotImplementedException(
                        "Reading >2GB files with a 'startxref' in the middle not implemented.");
                }
                string trail = _lexer.ReadRawString(0, (int)length);

                idx = trail.LastIndexOf("startxref", StringComparison.Ordinal);

                if (idx == -1)
                    throw new Exception("The StartXRef table could not be found, the file cannot be opened.");

                _lexer.Position = idx;
            }

            ReadSymbol(Symbol.StartXRef);
            _lexer.Position = ReadSize();

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
        /// Reads cross-reference table(s) and trailer(s).
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
                            SizeType position = ReadSize();
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
                            var objectID = new PdfObjectID(idToUse, generation);
                            // Ignore the latter one.
                            if (xrefTable.Contains(objectID))
                                continue;
                            xrefTable.Add(new PdfReference(objectID, position));
                        }
                    }
                    else if (symbol == Symbol.Trailer)
                    {
                        ReadSymbol(Symbol.BeginDictionary);
                        var trailer = new PdfTrailer(_document);
                        ReadDictionary(trailer, false);
                        return trailer;
                    }
                    else
                        ParserDiagnostics.HandleUnexpectedToken(_lexer.Token, _lexer.DumpNeighborhoodOfPosition());
                }
            }
            // ReSharper disable once RedundantIfElseBlock because of code readability.
            else if (symbol == Symbol.Integer) // Is it a cross-reference stream?
            {
                // Reference: 3.4.7  Cross-Reference Streams / Page 93
                // TODO: Handle PDF files larger than 2 GiB, see implementation note 21 in Appendix H.

                // The parsed integer is the object ID of the cross-reference stream.
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
        bool CheckXRefTableEntry(SizeType position, int id, int generation, out int idChecked, out int generationChecked)
        {
            SizeType origin = _lexer.Position;
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
                Symbol symbol = _lexer.ScanNextToken(false);
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
        /// Reads cross-reference stream(s).
        /// </summary>
        PdfTrailer ReadXRefStream(PdfCrossReferenceTable xrefTable)
        {
            // Read cross-reference stream.
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

                    PdfCrossReferenceStream.CrossReferenceStreamEntry item = new()
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

                            SizeType position = (SizeType)item.Field2;
                            objectID = ReadObjectNumber(position);
#if DEBUG_
                            if (objectID.ObjectNumber == 1074)
                                GetType();
#endif
                            Debug.Assert(objectID.GenerationNumber == item.Field3);

                            // Ignore the latter one.
                            if (!xrefTable.Contains(objectID))
                            {
                                // Add iref for all uncompressed objects.
                                xrefTable.Add(new PdfReference(objectID, position));

                            }
#if DEBUG_
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

        struct ParserState
        {
            public SizeType Position;
            public Symbol Symbol;
        }

        readonly PdfDocument _document;
        readonly PdfReaderOptions _options;
        readonly Lexer _lexer;
        readonly Dictionary<PdfObjectID, (PdfObjectStream ObjectStream, Parser Parser)> _objectStreamsWithParsers = new();
        readonly Parser _documentParser;
        readonly ILogger _logger;
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
