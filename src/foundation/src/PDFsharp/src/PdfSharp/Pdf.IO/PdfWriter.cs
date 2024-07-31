﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Represents a writer for generation of PDF streams.
    /// </summary>
    class PdfWriter
    {
        public PdfWriter(Stream pdfStream, PdfStandardSecurityHandler? effectiveSecurityHandler)
        {
            _stream = pdfStream;
            EffectiveSecurityHandler = effectiveSecurityHandler;
            //System.Xml.XmlTextWriter
#if DEBUG
            _layout = PdfWriterLayout.Verbose;
#endif
        }

        public void Close(bool closeUnderlyingStream)
        {
            if (_stream != null! && closeUnderlyingStream)
            {
#if UWP
                _stream.Dispose();
#else
                _stream.Close();
#endif
            }
            _stream = null!;
        }

        public void Close() => Close(true);

        public SizeType Position => (SizeType)_stream.Position;

        /// <summary>
        /// Gets or sets the kind of layout.
        /// </summary>
        public PdfWriterLayout Layout
        {
            get => _layout;
            set => _layout = value;
        }
        PdfWriterLayout _layout;

        public PdfWriterOptions Options
        {
            get => _options;
            set => _options = value;
        }
        PdfWriterOptions _options;

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

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public void Write(PdfUInteger value)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            WriteSeparator(CharCat.Character);
            _lastCat = CharCat.Character;
            WriteRaw(value.Value.ToString(CultureInfo.InvariantCulture));
        }

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

            WriteSeparator(CharCat.Delimiter, '/');
            string name = value.Value;
            Debug.Assert(name[0] == '/');

            // Encode to raw UTF-8 is any char is larger than 126.
            // 127 [DEL] is not a valid value and is also get encoded.
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
                        // TODO: is this all?
                        case '%':
                        case '/':
                        case '<':
                        case '>':
                        case '(':
                        case ')':
                        case '[':
                        case ']':
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
            WriteSeparator(CharCat.Character);
            WriteRaw(value.Value);
            _lastCat = CharCat.Character;
        }

        public void Write(PdfRectangle rect)
        {
            const string format = Config.SignificantDecimalPlaces3;
            WriteSeparator(CharCat.Delimiter, '/');
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
            bool indirect = obj.IsIndirect;
            if (indirect)
            {
                WriteObjectAddress(obj);
                EffectiveSecurityHandler?.EnterObject(obj.ObjectID);
            }
            _stack.Add(new StackItem(obj));
            if (indirect)
            {
                if (obj is PdfArray)
                    WriteRaw("[\n");
                else if (obj is PdfDictionary)
                    WriteRaw("<<\n");
                _lastCat = CharCat.NewLine;
            }
            else
            {
                if (obj is PdfArray)
                {
                    WriteSeparator(CharCat.Delimiter);
                    WriteRaw('[');
                    _lastCat = CharCat.Delimiter;
                }
                else if (obj is PdfDictionary)
                {
                    NewLine();
                    WriteSeparator(CharCat.Delimiter);
                    WriteRaw("<<\n");
                    _lastCat = CharCat.NewLine;
                }
            }
            if (_layout == PdfWriterLayout.Verbose)
                IncreaseIndent();
        }

        /// <summary>
        /// Ends a direct or indirect dictionary or array.
        /// </summary>
        public void WriteEndObject()
        {
            int count = _stack.Count;
            Debug.Assert(count > 0, "PdfWriter stack underflow.");

            StackItem stackItem = _stack[count - 1];
            _stack.RemoveAt(count - 1);

            PdfObject value = stackItem.Object;
            var indirect = value.IsIndirect;
            if (indirect)
                EffectiveSecurityHandler?.LeaveObject();
            if (_layout == PdfWriterLayout.Verbose)
                DecreaseIndent();
            if (value is PdfArray)
            {
                if (indirect)
                {
                    WriteRaw("\n]\n");
                    _lastCat = CharCat.NewLine;
                }
                else
                {
                    WriteRaw("]");
                    _lastCat = CharCat.Delimiter;
                }
            }
            else if (value is PdfDictionary)
            {
                if (indirect)
                {
                    if (!stackItem.HasStream)
                        WriteRaw(_lastCat == CharCat.NewLine ? ">>\n" : " >>\n");
                }
                else
                {
                    Debug.Assert(!stackItem.HasStream, "Direct object with stream??");
                    WriteSeparator(CharCat.NewLine);
                    WriteRaw(">>\n");
                    _lastCat = CharCat.NewLine;
                }
            }
            if (indirect)
            {
                NewLine();
                WriteRaw("endobj\n");
                if (_layout == PdfWriterLayout.Verbose)
                    WriteRaw("%--------------------------------------------------------------------------------------------------\n");
            }
        }

        /// <summary>
        /// Writes the stream of the specified dictionary.
        /// </summary>
        public void WriteStream(PdfDictionary value, bool omitStream)
        {
            var stackItem = _stack[_stack.Count - 1];
            Debug.Assert(stackItem.Object is PdfDictionary);
            Debug.Assert(stackItem.Object.IsIndirect);
            stackItem.HasStream = true;

            WriteRaw(_lastCat == CharCat.NewLine ? ">>\nstream\n" : " >>\nstream\n");

            if (omitStream)
            {
                WriteRaw("  «…stream content omitted…»\n");  // useful for debugging only
            }
            else
            {
                // Earlier versions of PDFsharp skipped the '\n' before 'endstream' if the last byte of
                // the stream is a linefeed. This was wrong and now fixed.
                var bytes = value.Stream.Value;
                if (bytes.Length != 0)
                    Write(bytes);
            }
            WriteRaw("\nendstream\n");
        }

        public void WriteRaw(string rawString)
        {
            if (String.IsNullOrEmpty(rawString))
                return;

            byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            _stream.Write(bytes, 0, bytes.Length);
            _lastCat = GetCategory((char)bytes[bytes.Length - 1]);
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
            _lastCat = GetCategory((char)bytes[bytes.Length - 1]);
        }

        void WriteObjectAddress(PdfObject value)
        {
            if (_layout == PdfWriterLayout.Verbose)
            {
                string comment = value.Comment;
                if (!String.IsNullOrEmpty(comment))
                    comment = $" -- {value.Comment}";
                WriteRaw(Invariant($"{value.ObjectID.ObjectNumber} {value.ObjectID.GenerationNumber} obj   % {value.GetType().FullName}{comment}\n"));
            }
            else
                WriteRaw(Invariant($"{value.ObjectID.ObjectNumber} {value.ObjectID.GenerationNumber} obj\n"));
        }

        public void WriteFileHeader(PdfDocument document)
        {
            var header = new StringBuilder("%PDF-");
            int version = document._version;
            //header.Append((version / 10).ToString(CultureInfo.InvariantCulture) + "." +
            //  (version % 10).ToString(CultureInfo.InvariantCulture) + "\n%\xD3\xF4\xCC\xE1\n");
            header.Append(Invariant($"{version / 10}.{version % 10}\n%\xD3\xF4\xCC\xE1\n"));
            WriteRaw(header.ToString());

            if (_layout == PdfWriterLayout.Verbose)
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
            // position check required for incremental updates to avoid overwriting the start of the file
            if (_layout == PdfWriterLayout.Verbose && _commentPosition > 0)
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
                WriteRaw(Invariant($"Objects: {document.IrefTable.ObjectTable.Count:#,###}"));
            }
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

        void WriteSeparator(CharCat cat, char ch = '\0')
        {
            switch (_lastCat)
            {
                case CharCat.NewLine:
                    if (_layout == PdfWriterLayout.Verbose)
                        WriteIndent();
                    break;

                case CharCat.Delimiter:
                    break;

                case CharCat.Character:
                    if (_layout == PdfWriterLayout.Verbose)
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

        internal PdfStandardSecurityHandler? EffectiveSecurityHandler { get; set; }

        class StackItem
        {
            public StackItem(PdfObject value)
            {
                Object = value;
            }

            public readonly PdfObject Object;
            public bool HasStream;
        }

        readonly List<StackItem> _stack = new();
        int _commentPosition;
    }
}
