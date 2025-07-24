// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.Signatures;

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Represents a writer for generation of PDF streams.
    /// </summary>
    class PdfWriter
    {
        public PdfWriter(Stream pdfStream, PdfDocument document, PdfStandardSecurityHandler? effectiveSecurityHandler)
        {
            _stream = pdfStream ?? throw new ArgumentNullException(nameof(pdfStream));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            EffectiveSecurityHandler = effectiveSecurityHandler;

            Layout = document.Options.Layout;
        }

        public void Close(bool closeUnderlyingStream)
        {
            if (closeUnderlyingStream)
                _stream.Close();
            _stream = null!;
        }

        public void Close() => Close(true);

        public SizeType Position => (SizeType)_stream.Position;

        /// <summary>
        /// Gets or sets the kind of layout.
        /// </summary>
        public PdfWriterLayout Layout { get; set; }

        internal bool IsCompactLayout => Layout == PdfWriterLayout.Compact;

        internal bool IsStandardLayout => Layout >= PdfWriterLayout.Standard;

        internal bool IsIndentedLayout => Layout >= PdfWriterLayout.Indented;

        internal bool IsVerboseLayout => Layout >= PdfWriterLayout.Verbose;

        public PdfWriterOptions Options { get; set; }

        // -----------------------------------------------------------

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(bool value)
        {
            WriteSeparator(CharCat.Character);
            // Wrong: Writes "True" or "False" where it should be "true" or "false": WriteRaw(value ? bool.TrueString : bool.FalseString);
            WriteRaw(value ? "true" : "false");
            _lastCat = CharCat.Character;
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(PdfBoolean value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.Value ? "true" : "false");
            _lastCat = CharCat.Character;
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(int value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.ToString(CultureInfo.InvariantCulture));
            _lastCat = CharCat.Character;
        }

        public void Write(long value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.ToString(CultureInfo.InvariantCulture));
            _lastCat = CharCat.Character;
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(uint value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.ToString(CultureInfo.InvariantCulture));
            _lastCat = CharCat.Character;
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(PdfInteger value)
        {
            WriteSeparator(CharCat.Character);
            _lastCat = CharCat.Character;
            WriteRaw(value.Value.ToString(CultureInfo.InvariantCulture));
        }

        // DELETE
        ////        /// <summary>
        ////        /// Writes the specified value to the PDF stream.
        ////        /// </summary>
        ////#pragma warning disable CS0618 // Type or member is obsolete
        ////        public void Write(PdfUInteger value)
        ////#pragma warning restore CS0618 // Type or member is obsolete
        ////        {
        ////            WriteSeparator(CharCat.Character);
        ////            _lastCat = CharCat.Character;
        ////            WriteRaw(value.Value.ToString(CultureInfo.InvariantCulture));
        ////        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(PdfLongInteger value)
        {
            WriteSeparator(CharCat.Character);
            _lastCat = CharCat.Character;
            WriteRaw(value.Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(double value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.ToString(Config.SignificantDecimalPlaces7, CultureInfo.InvariantCulture));
            _lastCat = CharCat.Character;
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(PdfReal value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.Value.ToString(Config.SignificantDecimalPlaces7, CultureInfo.InvariantCulture));
            _lastCat = CharCat.Character;
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(PdfString value)
        {
            WriteSeparator(CharCat.Delimiter);
#if true
            PdfStringEncoding encoding = (PdfStringEncoding)(value.Flags & PdfStringFlags.EncodingMask);
            string pdf = (value.Flags & PdfStringFlags.HexLiteral) == 0 ?
                PdfEncoders.ToStringLiteral(value.Value, encoding, EffectiveSecurityHandler) :
                PdfEncoders.ToHexStringLiteral(value.Value, encoding, EffectiveSecurityHandler);
            WriteRaw(pdf);
#else
            switch (value.Flags & PdfStringFlags.EncodingMask)
            {
                case PdfStringFlags.Undefined:
                case PdfStringFlags.PDFDocEncoding:
                    if ((value.Flags & PdfStringFlags.HexLiteral) == 0)
                        WriteRaw(PdfEncoders.DocEncode(value.Value, false));
                    else
                        WriteRaw(PdfEncoders.DocEncodeHex(value.Value, false));
                    break;

                case PdfStringFlags.WinAnsiEncoding:
                    throw new NotImplementedException("Unexpected encoding: WinAnsiEncoding");

                case PdfStringFlags.Unicode:
                    if ((value.Flags & PdfStringFlags.HexLiteral) == 0)
                        WriteRaw(PdfEncoders.DocEncode(value.Value, true));
                    else
                        WriteRaw(PdfEncoders.DocEncodeHex(value.Value, true));
                    break;

                case PdfStringFlags.StandardEncoding:
                case PdfStringFlags.MacRomanEncoding:
                case PdfStringFlags.MacExpertEncoding:
                default:
                    throw new NotImplementedException("Unexpected encoding");
            }
#endif
            _lastCat = CharCat.Delimiter;
        }

        /// <summary>
        /// Writes a signature placeholder hexadecimal string to the PDF stream.
        /// </summary>
        public void Write(PdfSignaturePlaceholderItem item, out SizeType startPosition, out SizeType endPosition)
        {
            WriteSeparator(CharCat.Delimiter);

            // ReSharper disable once RedundantCast
            startPosition = (SizeType)Position;
            // A PDF hex string with question marks '<????????...??>'
            WriteRaw(item.ToString());
            // ReSharper disable once RedundantCast
            endPosition = (SizeType)Position;

            _lastCat = CharCat.Delimiter;
        }

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(PdfName value)
        {
            // From Adobe specs: 3.2.4 Name objects
            // Beginning with PDF 1.2, any character except null (character code 0) may be included
            // in a name by writing its 2-digit hexadecimal code, preceded by the number
            // sign character (#); see implementation notes 3 and 4 in Appendix H. This
            // syntax is required in order to represent any of the delimiter or white-space characters
            // or the number sign character itself; it is recommended but not required for
            // characters whose codes are outside the range 33(!) to 126(~).

            // And also:
            // In such situations, it is recommended that the sequence of bytes (after expansion
            // of # sequences, if any) be interpreted according to UTF-8, a variable-length byte-encoded
            // representation of Unicode in which the printable ASCII characters have
            // the same representations as in ASCII.This enables a name object to represent text
            // in any natural language, subject to the implementation limit on the length of a
            // name.

            WriteSeparator(CharCat.Delimiter);
            string name = value.Value;
            Debug.Assert(name[0] == '/');

            // Encode to raw UTF-8 if any char is larger than 126.
            // 127 [DEL] is not a valid value and is also encoded.
            for (int idx = 1; idx < name.Length; idx++)
            {
                char ch = name[idx];
                if (ch > 126)
                {
                    // Non-ASCII character found, convert whole string to raw UTF-8.
                    var bytes = Encoding.UTF8.GetBytes(name);
                    var nameBuilder = new StringBuilder();
                    foreach (var ch2 in bytes)
                        nameBuilder.Append((char)ch2);
                    name = nameBuilder.ToString();
                    break;
                }
            }

            // Here all high bytes are zero.
            var pdf = new StringBuilder("/");
            for (int idx = 1; idx < name.Length; idx++)
            {
                char ch = name[idx];
                Debug.Assert(ch < 256);
                if (ch > ' ')
                {
                    switch (ch)
                    {
                        case '%':
                        case '/':
                        case '<':
                        case '>':
                        case '(':
                        case ')':
                        case '[':
                        case ']':
                        case '{':
                        case '}':
                        case '#':
                            break;

                        default:
                            if (ch <= 126)
                            {
                                // See recommendation above.
                                pdf.Append(ch);
                                continue;
                            }
                            break;
                    }
                }
                pdf.AppendFormat("#{0:X2}", (int)ch);
            }
            WriteRaw(pdf.ToString());
            _lastCat = CharCat.Character;
        }

        public void Write(PdfLiteral value)
        {
            var rawString = value.Value;
            var first = rawString[0];
            var last = rawString[^1];
            WriteSeparator(GetCategory(first));
            WriteRaw(rawString);
            _lastCat = GetCategory(last);
        }

        public void Write(PdfRectangle rect)
        {
            const string format = Config.SignificantDecimalPlaces3;
            WriteSeparator(CharCat.Delimiter/*, '/'*/);
            WriteRaw(PdfEncoders.Format("[{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "}]", rect.X1, rect.Y1, rect.X2, rect.Y2));
            _lastCat = CharCat.Delimiter;
        }

        public void Write(PdfReference iref)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(iref.ToString());
            _lastCat = CharCat.Character;
        }

        public void WriteDocString(string text, bool unicode)
        {
            WriteSeparator(CharCat.Delimiter);
            //WriteRaw(PdfEncoders.DocEncode(text, unicode));
            byte[] bytes;
            if (!unicode)
                bytes = PdfEncoders.DocEncoding.GetBytes(text);
            else
                bytes = PdfEncoders.UnicodeEncoding.GetBytes(text);
            bytes = PdfEncoders.FormatStringLiteral(bytes, unicode, true, false, EffectiveSecurityHandler);
            Write(bytes);
            _lastCat = CharCat.Delimiter;
        }

        public void WriteDocString(string text)
        {
            WriteSeparator(CharCat.Delimiter);
            //WriteRaw(PdfEncoders.DocEncode(text, false));
            byte[] bytes = PdfEncoders.DocEncoding.GetBytes(text);
            bytes = PdfEncoders.FormatStringLiteral(bytes, false, false, false, EffectiveSecurityHandler);
            Write(bytes);
            _lastCat = CharCat.Delimiter;
        }

        public void WriteDocStringHex(string text)
        {
            WriteSeparator(CharCat.Delimiter);
            //WriteRaw(PdfEncoders.DocEncodeHex(text));
            byte[] bytes = PdfEncoders.DocEncoding.GetBytes(text);
            bytes = PdfEncoders.FormatStringLiteral(bytes, false, false, true, EffectiveSecurityHandler);
            _stream.Write(bytes, 0, bytes.Length);
            _lastCat = CharCat.Delimiter;
        }

        /// <summary>
        /// Begins a direct or indirect dictionary or array.
        /// </summary>
        public void WriteBeginObject(PdfObject obj)
        {
            bool isIndirect = obj.IsIndirect;
            if (isIndirect)
            {
                WriteObjectAddress(obj);
            }
            _stack.Add(new StackItem(obj));

            string? suffix = null;
            if (IsVerboseLayout && _stack.Count > 1)
                suffix = GetTypeAndComment(obj);

            if (isIndirect)
            {
                if (obj is PdfArray)
                {
                    if (IsCompactLayout)
                    {
                        WriteRaw('[');
                    }
                    else
                    {
                        if (suffix != null)
                            WriteRaw("[" + suffix);
                        else
                            WriteRaw("[\n");

                    }
                }
                else if (obj is PdfDictionary)
                {
                    if (IsCompactLayout)
                    {
                        WriteRaw("<<");
                    }
                    else
                    {
                        if (suffix != null)
                            WriteRaw("<<" + suffix);
                        else
                            WriteRaw("<<\n");
                    }
                }
                else
                {
                    // Case: PdfIntegerObject or PdfNullObject

                    //Debug.Assert(false, "Should not come here.");
                    Debug.Assert(obj is not null, "Should not come here.");
                }
                _lastCat = CharCat.NewLine;
            }
            else
            {
                if (obj is PdfArray)
                {
#if true_
                    // Same as PdfDictionary
                    NewLine();
                    WriteSeparator(CharCat.Delimiter);
                    WriteRaw("[\n");
                    _lastCat = CharCat.NewLine;
#else
                    if (IsCompactLayout)
                    {
                        WriteRaw('[');
                    }
                    else
                    {
                        //NewLine();
                        //WriteSeparator(CharCat.Delimiter);
                        if (suffix != null)
                        {
                            WriteRaw("[   " + GetTypeAndComment(obj));
                            _lastCat = CharCat.NewLine;
                        }
                        else
                        {
                            WriteRaw('[');
                            _lastCat = CharCat.Delimiter;
                        }
                    }
#endif
                }
                else if (obj is PdfDictionary)
                {
                    if (IsCompactLayout)
                    {
                        WriteRaw("<<");
                    }
                    else
                    {
                        NewLine();
                        WriteSeparator(CharCat.Delimiter);
                        if (suffix != null)
                            WriteRaw("<<   " + GetTypeAndComment(obj));
                        else
                            WriteRaw("<<\n");
                        _lastCat = CharCat.NewLine;
                    }
                }
                else
                {
                    // Case: PdfIntegerObject or PdfNullObject

                    //Debug.Assert(false, "Should not come here.");
                    Debug.Assert(obj is not null, "Should not come here.");
                }
            }
            if (IsVerboseLayout)
                IncreaseIndent();
        }

        /// <summary>
        /// Ends a direct or indirect dictionary or array.
        /// </summary>
        public void WriteEndObject()
        {
            bool noLayout = Layout == PdfWriterLayout.Compact;

            int count = _stack.Count;
            Debug.Assert(count > 0, "PdfWriter stack underflow.");

            StackItem stackItem = _stack[count - 1];
            _stack.RemoveAt(count - 1);

            PdfObject value = stackItem.Object;
            var indirect = value.IsIndirect;

            if (IsVerboseLayout)
                DecreaseIndent();

            if (value is PdfArray)
            {
                if (indirect)
                {
                    if (IsCompactLayout)
                    {
                        WriteRaw("]\n");
                        _lastCat = CharCat.NewLine;
                    }
                    else
                    {

                        WriteRaw("\n]\n");
                        _lastCat = CharCat.Delimiter;
                    }
                }
                else
                {
                    if (IsCompactLayout)
                    {
                        WriteRaw("]");
                        _lastCat = CharCat.Delimiter;
                    }
                    else
                    {
                        //WriteSeparator(CharCat.NewLine);
                        WriteRaw("]");
                        _lastCat = CharCat.Delimiter;
                    }
                }
            }
            else if (value is PdfDictionary)
            {
                if (indirect)
                {
                    if (IsCompactLayout)
                    {
                        if (!stackItem.HasStream)
                            WriteRaw(">>\n");
                        _lastCat = CharCat.NewLine;
                    }
                    else
                    {
                        if (!stackItem.HasStream)
                            WriteRaw(">>\n");
                        _lastCat = CharCat.NewLine;
                    }
                }
                else
                {
                    Debug.Assert(!stackItem.HasStream, "Direct object with stream??");
                    if (IsCompactLayout)
                    {
                        WriteSeparator(CharCat.NewLine);
                        WriteRaw(">>");
                        _lastCat = CharCat.Delimiter;
                    }
                    else
                    {
                        WriteSeparator(CharCat.NewLine);
                        if (IsVerboseLayout)
                        {
                            WriteRaw(">>\n");
                            _lastCat = CharCat.NewLine;
                        }
                        else
                        {
                            WriteRaw(">>");
                            _lastCat = CharCat.Delimiter;
                        }
                    }
                }
            }
            if (indirect)
            {
                if (IsCompactLayout)
                {
                    NewLine();
                    WriteRaw("endobj\n");
                }
                else
                {
                    NewLine();
                    WriteRaw("endobj\n");
                    if (IsVerboseLayout)
                        WriteRaw("%--------------------------------------------------------------------------------------------------\n");
                }
            }
        }

        /// <summary>
        /// Writes the stream of the specified dictionary.
        /// </summary>
        public void WriteStream(PdfDictionary dict, bool omitStream)
        {
            var stackItem = _stack[^1];
            Debug.Assert(stackItem.Object is PdfDictionary);
            Debug.Assert(stackItem.Object.IsIndirect);
            stackItem.HasStream = true;

            var bytes = dict.Stream!.Value;
            if (IsCompactLayout)
            {
                WriteRaw(">>\nstream\n");

                // Earlier versions of PDFsharp skipped the '\n' before 'endstream' if the last byte of
                // the stream is a linefeed. This was wrong and is now fixed.
                Write(bytes);

                WriteRaw("\nendstream\n");
            }
            else
            {
                //WriteRaw(_lastCat == CharCat.NewLine ? ">>\nstream\n" : " >>\nstream\n");

                if (IsVerboseLayout)
                    WriteRaw(Invariant($">>\n% Length: {bytes.Length}\nstream\n"));
                else
                    WriteRaw(">>\nstream\n");

                if (omitStream)
                {
                    WriteRaw("  «…stream content omitted…»\n");  // Useful for debugging only. PDF file is always invalid.
                }
                else
                {
                    Write(bytes);
                }
                WriteRaw("\nendstream\n");
            }
        }

        public void WriteRaw(string rawString)
        {
            if (String.IsNullOrEmpty(rawString))
                return;

            byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            _stream.Write(bytes, 0, bytes.Length);
            _lastCat = GetCategory((char)bytes[^1]);
        }

        public void WriteRaw(char ch)
        {
            Debug.Assert(ch < 256, "Raw character greater than 255 detected.");

            _stream.WriteByte((byte)ch);
            _lastCat = GetCategory(ch);
        }

        public void Write(byte[] bytes)
        {
            if (bytes == null! || bytes.Length == 0)
                return;

            _stream.Write(bytes, 0, bytes.Length);
            _lastCat = GetCategory((char)bytes[^1]);
        }

        void WriteObjectAddress(PdfObject value)
        {
            if (Layout == PdfWriterLayout.Verbose)
            {
                string comment = value.Comment;
                if (!String.IsNullOrEmpty(comment))
                    comment = $" -- {value.Comment}";

#if DEBUG_
                if (_document is null)
                    _ = typeof(int);
#endif
                // #PDF-A
                if (_document.IsPdfA)
                {
                    // Write full type name and comment in a separate line to be PDF-A conform.
                    WriteRaw(Invariant($"% {value.GetType().FullName}{comment}\n"));
                    WriteRaw(Invariant($"{value.ObjectID.ObjectNumber} {value.ObjectID.GenerationNumber} obj\n"));
                }
                else
                {
                    // Write object number and full type name and comment in one line.
                    WriteRaw(Invariant($"{value.ObjectID.ObjectNumber} {value.ObjectID.GenerationNumber} obj   % {value.GetType().FullName}{comment}\n"));
                }
            }
            else
            {
                // Write object number only.
                WriteRaw(Invariant($"{value.ObjectID.ObjectNumber} {value.ObjectID.GenerationNumber} obj\n"));
            }
        }

        public void WriteFileHeader(PdfDocument document)
        {
            var header = new StringBuilder("%PDF-");
            int version = document._version;
            //header.Append((version / 10).ToString(CultureInfo.InvariantCulture) + "." +
            //  (version % 10).ToString(CultureInfo.InvariantCulture) + "\n%\xD3\xF4\xCC\xE1\n");
            header.Append(Invariant($"{version / 10}.{version % 10}\n%\xD3\xF4\xCC\xE1\n"));
            WriteRaw(header.ToString());

            if (Layout == PdfWriterLayout.Verbose)
            {
                WriteRaw($"% PDFsharp Version {PdfSharpProductVersionInformation.Version} (verbose mode)\n");
                // Keep some space for later fix-up.
                _commentPosition = (int)_stream.Position + 2;
                WriteRaw("%                                                \n");  // Creation date placeholder
                WriteRaw("%                                                \n");  // Creation time placeholder
                WriteRaw("%                                                \n");  // File size placeholder
                WriteRaw("%                                                \n");  // Pages placeholder
                WriteRaw("%                                                \n");  // Objects placeholder
#if DEBUG
                WriteRaw("% This document is created from a DEBUG build. Do not use a DEBUG build of PDFsharp for production.\n");
#endif
                WriteRaw("%--------------------------------------------------------------------------------------------------\n");
            }
        }

        public void WriteEof(PdfDocument document, SizeType startxref)
        {
            WriteRaw($"% Created with PDFsharp {PdfSharpProductVersionInformation.SemanticVersion} ({Capabilities.Build.BuildName}) under .NET {Capabilities.Build.Framework}\n");
            WriteRaw("startxref\n");
            WriteRaw(startxref.ToString(CultureInfo.InvariantCulture));
            WriteRaw("\n%%EOF\n");
            SizeType fileSize = (SizeType)_stream.Position;
            if (Layout == PdfWriterLayout.Verbose)
            {
                TimeSpan duration = DateTime.Now - document._creation;

                _stream.Position = _commentPosition;
                // Without InvariantCulture parameter the following line fails if the current culture is e.g.
                // a Far East culture, because the date string contains non-ASCII characters.
                // So never never never never use ToString without a culture info.
                WriteRaw(Invariant($"Creation date: {document._creation:G}"));
                _stream.Position = _commentPosition + 50;
                WriteRaw(Invariant($"Creation time: {duration.TotalSeconds:0.000} seconds"));
                _stream.Position = _commentPosition + 100;
                WriteRaw(Invariant($"File size: {fileSize:#,###} bytes"));
                _stream.Position = _commentPosition + 150;
                WriteRaw(Invariant($"Pages: {document.Pages.Count:#}"));  // No thousands separator here.
                _stream.Position = _commentPosition + 200;
                WriteRaw(Invariant($"Objects: {document.IrefTable.Count:#,###}"));
            }
        }

        //static string GetFullTypeName(PdfObject obj) => obj.GetType().FullName ?? "?";
        static string? GetTypeAndComment(PdfObject value, bool typenameAlways = false)
        {
            var type = value.GetType();
            string comment = value.Comment;

            bool showType = typenameAlways || (type != typeof(PdfDictionary) && type != typeof(PdfArray));
            string? result;

            if (showType)
            {
                if (!String.IsNullOrEmpty(comment))
                    result = Invariant($"% {value.GetType().Name} ({value.GetType().FullName}) -- {comment}\n");
                else
                    result = $"% {value.GetType().Name} ({value.GetType().FullName})\n";
            }
            else
            {
                if (!String.IsNullOrEmpty(comment))
                    result = Invariant($"% {comment}\n");
                else
                    result = null;
            }
            return result;
        }

        /// <summary>
        /// Gets or sets the indentation for a new indentation level.
        /// </summary>
        internal int Indent
        {
            get => _indent;
            set => _indent = value;
        }

        int _indent = 2;
        int _writeIndent;

        /// <summary>
        /// Increases indent level.
        /// </summary>
        void IncreaseIndent()
        {
            _writeIndent += _indent;
        }

        /// <summary>
        /// Decreases indent level.
        /// </summary>
        void DecreaseIndent()
        {
            _writeIndent -= _indent;
        }

        /// <summary>
        /// Gets an indent string of current indent.
        /// </summary>
        string IndentBlanks => new(' ', _writeIndent);

        void WriteIndent()
        {
            WriteRaw(IndentBlanks);
        }

        void WriteSeparator(CharCat cat /*, char ch = '\0'*/)
        {
            switch (_lastCat)
            {
                case CharCat.NewLine:
                    if (Layout == PdfWriterLayout.Verbose)
                        WriteIndent();
                    break;

                case CharCat.Delimiter:
                    break;

                case CharCat.Character:
                    if (Layout == PdfWriterLayout.Verbose)
                    {
                        _stream.WriteByte((byte)' ');
                    }
                    else
                    {
                        if (cat == CharCat.Character)
                            _stream.WriteByte((byte)' ');
                    }
                    break;
            }
        }

        public void NewLine()
        {
            if (_lastCat != CharCat.NewLine)
                WriteRaw('\n');
        }

        static CharCat GetCategory(char ch)
        {
            if (Lexer.IsDelimiter(ch))
                return CharCat.Delimiter;
            if (ch == Chars.LF)
                return CharCat.NewLine;
            return CharCat.Character;
        }

        enum CharCat
        {
            NewLine,
            Character,
            Delimiter,
        }
        CharCat _lastCat;

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        internal Stream Stream => _stream;

        Stream _stream;
        readonly PdfDocument _document;

        internal PdfStandardSecurityHandler? EffectiveSecurityHandler { get; set; }

        class StackItem(PdfObject value)
        {
            public readonly PdfObject Object = value;
            public bool HasStream;
        }

        readonly List<StackItem> _stack = [];
        int _commentPosition;
    }
}
