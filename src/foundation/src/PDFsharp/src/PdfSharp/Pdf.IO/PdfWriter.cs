// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Internal;

// v7.0.0 Review

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Represents a writer for generation of PDF streams.
    /// </summary>
    class PdfWriter(Stream pdfStream, PdfDocument document, PdfStandardSecurityHandler? effectiveSecurityHandler)
    {
        public void Close(bool closeUnderlyingStream = true)
        {
            if (closeUnderlyingStream)
                _stream.Close();
            _stream = null!;
        }

        // ReSharper disable once RedundantCast because SizeType can be a 32- or 64-bit integer.
        public SizeType Position => (SizeType)_stream.Position;

        /// <summary>
        /// Gets or sets the kind of layout.
        /// </summary>
        public PdfWriterLayout Layout { get; set; } = document.Options.Layout;

        internal bool IsCompactLayout => Layout == PdfWriterLayout.Compact;

        internal bool IsStandardLayout => Layout >= PdfWriterLayout.Standard;

        internal bool IsIndentedLayout => Layout >= PdfWriterLayout.Indented;

        internal bool IsVerboseLayout => Layout >= PdfWriterLayout.Verbose;

        public PdfWriterOptions Options { get; set; }

        // -----------------------------------------------------------

        /// <summary>
        /// Writes a PDF comment to the PDF stream.
        /// </summary>
        public void WriteComment(string text)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw("% " + text + '\n');
        }
        /// <summary>
        /// Writes ‘mull’ to the PDF stream.
        /// </summary>
        public void Write(PdfNull _)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw("null");
        }

        /// <summary>
        /// Writes ‘true’ or ‘false’ to the PDF stream.
        /// </summary>
        public void Write(bool value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value ? "true" : "false");
        }

        /// <summary>
        /// Writes ‘true’ or ‘false’ to the PDF stream.
        /// </summary>
        public void Write(PdfBoolean value) => Write(value.Value);

        /// <summary>
        /// Writes the specified integer value to the PDF stream.
        /// </summary>
        public void Write(int value, bool isFlag)
        {
            WriteSeparator(CharCat.Character);
            if (isFlag)
            {
                // Maybe an unsigned value instead of a negative one causes
                // problems with some PDF readers.
                //WriteRaw(((uint)value).ToString(CultureInfo.InvariantCulture));
                WriteRaw(value.ToString(CultureInfo.InvariantCulture));
                if (IsVerboseLayout)
                {
                    WriteRaw($"  % 0x{(uint)value >> 4:X4}_{(uint)value & 0xFFFF:X4}\n");
                }
            }
            else
                WriteRaw(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the specified long integer value to the PDF stream.
        /// </summary>
        public void Write(long value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the specified unsigned integer value to the PDF stream.
        /// </summary>
        public void Write(uint value)
        {
            WriteSeparator(CharCat.Character);
            WriteRaw(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the specified integer value to the PDF stream.
        /// </summary>
        public void Write(PdfInteger value) => Write(value.Value, value.IsFlag);

        /// <summary>
        /// Writes the specified long integer value to the PDF stream.
        /// </summary>
        public void Write(PdfLongInteger value) => Write(value.Value);

        /// <summary>
        /// Writes the specified real value to the PDF stream.
        /// </summary>
        public void Write(double value)
        {
            // See unit test Test_Single_Write_and_Read to understand why double values now
            // are written as singles values.
            float f = (float)value;
            WriteSeparator(CharCat.Character);
            WriteRaw(f.ToString(Config.SignificantDecimalPlaces7, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the specified real value to the PDF stream.
        /// </summary>
        public void Write(PdfReal value) => Write(value.Value);

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(PdfString value)
        {
            WriteSeparator(CharCat.Delimiter);
            var encoding = (PdfStringEncoding)(value.Flags & PdfStringFlags.EncodingMask);
            string pdf = (value.Flags & PdfStringFlags.HexLiteral) == 0 ?
                PdfEncoders.ToStringLiteral(value.Value, encoding, EffectiveSecurityHandler) :
                PdfEncoders.ToHexStringLiteral(value.Value, encoding, EffectiveSecurityHandler);
            WriteRaw(pdf);
            //_lastCat = CharCat.Delimiter;
        }

        /// <summary>
        /// Writes a signature placeholder hexadecimal string to the PDF stream.
        /// </summary>
        public void Write(PdfPlaceholder item, out SizeType startPosition, out SizeType endPosition)
        {
            WriteSeparator(CharCat.Delimiter);

            // ReSharper disable once RedundantCast
            startPosition = (SizeType)Position;

            // We have to write effectively filler bytes to easily update the stream postion.
            WriteRaw(item.Value);

            // ReSharper disable once RedundantCast
            endPosition = (SizeType)Position;
        }

#if false
        /// <summary>  // DELETE
        /// Writes a signature placeholder hexadecimal string to the PDF stream.
        /// </summary>
        public void Write(PdfSignaturePlaceholderItem_ item, out SizeType startPosition, out SizeType endPosition)  // TODO: Use PdfPlaceholder and DELETE
        {
            WriteSeparator(CharCat.Delimiter);
            // ReSharper disable once RedundantCast
            startPosition = (SizeType)Position;
            // A PDF hex string with question marks '<????????...??>'
            WriteRaw(item.ToString());
            // ReSharper disable once RedundantCast
            endPosition = (SizeType)Position;
            //_lastCat = CharCat.Delimiter;
        }
#endif

        /// <summary>
        /// Writes the specified PDF name to the PDF stream.
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
            // the same representations as in ASCII. This enables a name object to represent text
            // in any natural language, subject to the implementation limit on the length of a
            // name.

            WriteSeparator(CharCat.Delimiter);
            var literalName = value.Name.LiteralValue;
            WriteRaw(literalName);
            // In the rare case someone used an empty name the last category must 
            // nevertheless set to Character. Otherwise, a subsequent number would be parsed as
            // part of the name if the writer mode is Compact.
            _lastChar = '0';  // 0 as a symbolic character.
            _lastCat = CharCat.Character;
        }

        /// <summary>
        /// Writes the specified PDF literal value to the PDF stream.
        /// </summary>
        public void Write(PdfLiteral value)
        {
            if (String.IsNullOrEmpty(value.Value))
                return;

            WriteSeparator(GetCategory(value.Value[0]));
            WriteRaw(value.Value);
        }

        /// <summary>
        /// Writes the specified PDF rectangle value to the PDF stream.
        /// </summary>
        public void Write(PdfRectangle rect)
        {
            const string format = Config.SignificantDecimalPlaces3;
            WriteSeparator(CharCat.Delimiter);
            WriteRaw(PdfEncoders.Format("[{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "}]", rect.X1, rect.Y1, rect.X2, rect.Y2));
        }

        /// <summary>
        /// Writes the specified PDF reference value to the PDF stream.
        /// </summary>
        public void Write(PdfReference iref)
        {
#if DEBUG_
            if (iref.ObjectID.ObjectNumber == 6)
                _ = typeof(int);
#endif
            WriteSeparator(CharCat.Character);
            WriteRaw(iref.ToString());
        }

        public void WriteDocString(string text, bool unicode)
        {
            WriteSeparator(CharCat.Delimiter);
            var bytes = unicode
                ? PdfEncoders.UnicodeEncoding.GetBytes(text)
                : PdfEncoders.DocEncoding.GetBytes(text);
            bytes = PdfEncoders.FormatStringLiteral(bytes, unicode, true, false, EffectiveSecurityHandler);
            Write(bytes);
        }

        /// <summary>
        /// Writes the specified PDF DOC encoded string value to the PDF stream.
        /// </summary>
        public void WriteDocString(string text)
        {
            WriteSeparator(CharCat.Delimiter);
            byte[] bytes = PdfEncoders.DocEncoding.GetBytes(text);
            bytes = PdfEncoders.FormatStringLiteral(bytes, false, false, false, EffectiveSecurityHandler);
            Write(bytes);
        }

        /// <summary>
        /// Writes the specified PDF DOC encoded string value to the PDF stream.
        /// </summary>
        public void WriteDocStringHex(string text)
        {
            WriteSeparator(CharCat.Delimiter);
            byte[] bytes = PdfEncoders.DocEncoding.GetBytes(text);
            bytes = PdfEncoders.FormatStringLiteral(bytes, false, false, true, EffectiveSecurityHandler);
            Write(bytes);
        }

        /// <summary>
        /// Begins a direct or indirect dictionary or array.
        /// </summary>
        public void WriteBeginObject(PdfObject obj, int childItemCount = 0)
        {
#if DEBUG
            if (obj.ObjectID.ObjectNumber == 10)
                _ = typeof(int);
#endif
            bool isIndirect = obj.IsIndirect;
            if (isIndirect)
                WriteObjectAddress(obj);

            _stack.Add(new StackItem(obj, childItemCount));

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
                            WriteRaw("[   " + suffix);
                        else
                            WriteRaw("[");
                    }
                }
                else if (obj is PdfDictionary)
                {
                    if (IsCompactLayout)
                        WriteRaw("<<");
                    else
                    {
                        if (suffix != null)
                            WriteRaw("<<   " + suffix);
                        else
                            WriteRaw("<<\n");
                    }
                }
                else
                {
                    // Case: PdfIntegerObject or PdfNullObject

                    Debug.Assert(obj is not null, "Should not come here.");
                    Debug.Assert(obj is PdfPrimitiveObject, "Should be primitive object.");
                }
            }
            else  // !isIndirect
            {
                if (obj is PdfArray)
                {
                    if (IsCompactLayout)
                        WriteRaw('[');
                    else
                    {
                        if (suffix != null)
                            WriteRaw(" [   " + suffix);
                        else
                            WriteRaw(" [");
                    }
                }
                else if (obj is PdfDictionary)
                {
                    if (IsCompactLayout)
                        WriteRaw("<<");
                    else
                    {
                        NewLine();
                        WriteSeparator(CharCat.Delimiter);
                        if (suffix != null)
                            WriteRaw("<<   " + suffix);
                        else
                            WriteRaw("<<\n");
                    }
                }
                else
                {
                    // Case: PdfIntegerObject or PdfNullObject

                    Debug.Assert(obj is not null, "Should not come here.");
                }
            }
            if (IsIndentedLayout)
                IncreaseIndent();
        }

        /// <summary>
        /// Ends a direct or indirect dictionary or array.
        /// </summary>
        public void WriteEndObject()
        {
            int count = _stack.Count;
            Debug.Assert(count > 0, "PdfWriter stack underflow.");

            var stackItem = _stack[count - 1];
            _stack.RemoveAt(count - 1);

            PdfObject value = stackItem.Object;
            var indirect = value.IsIndirect;

            if (IsIndentedLayout)
                DecreaseIndent();

            if (value is PdfArray)
            {
                if (indirect)
                {
                    if (IsCompactLayout)
                        WriteRaw("]\n");
                    else
                        WriteRaw("]");
                }
                else
                {
                    // DELETE TODO
                    if (IsCompactLayout)
                    {
                        WriteRaw("]");
                    }
                    else
                    {
                        // Indent after new line.
                        if (_lastCat == CharCat.NewLine && IsIndentedLayout)
                            WriteIndent();
                        WriteRaw("]");
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
                    }
                    else
                    {
                        if (!stackItem.HasStream)
                            WriteRaw(">>\n");
                    }
                }
                else
                {
                    Debug.Assert(!stackItem.HasStream, "Direct object with stream??");
                    if (IsCompactLayout)
                    {
                        WriteSeparator(CharCat.NewLine);
                        WriteRaw(">>");
                    }
                    else
                    {
                        WriteSeparator(CharCat.NewLine);
                        //WriteRaw(">>\n");
                        if (IsVerboseLayout)
                            WriteRaw(">>\n");
                        else
                            WriteRaw(">>");
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
                WriteRaw(">>stream\n");

                // Earlier versions of PDFsharp skipped the '\n' before 'endstream' if the last byte of
                // the stream is a linefeed. This was wrong and is now fixed.
                Write(bytes);

                WriteRaw("\nendstream\n");
            }
            else
            {
                if (IsVerboseLayout)
                    WriteRaw(Invariant($">>\n% Real length: {bytes.Length}\nstream\n"));
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
            _lastChar = (char)bytes[^1];
            _lastCat = GetCategory(_lastChar);
        }

        public void WriteRaw(char ch)
        {
            Debug.Assert(ch < 256, "Raw character greater than 255 detected.");

            _stream.WriteByte((byte)ch);
            _lastChar = ch;
            _lastCat = GetCategory(ch);
        }

        public void Write(byte[] bytes)
        {
            if (bytes == null! || bytes.Length == 0)
                return;

            _stream.Write(bytes, 0, bytes.Length);
            _lastChar = (char)bytes[^1];
            _lastCat = GetCategory(_lastChar);
        }

        void WriteObjectAddress(PdfObject value)
        {
            var objID = value.ObjectID;
            if (IsVerboseLayout)
            {
                // #PDF-A
                if (_document.IsPdfA)
                {
                    // Write full type name and comment in a separate line to be PDF-A conform.
                    WriteRaw(GetTypeAndComment(value)!);
                    WriteRaw(Invariant($"{objID.ObjectNumber} {objID.GenerationNumber} obj\n"));
                }
                else
                {
                    // Write object number and full type name and comment in one line.
                    WriteRaw(Invariant($"{objID.ObjectNumber} {objID.GenerationNumber} obj   {GetTypeAndComment(value, true)}"));
                }
            }
            else
            {
                // Write object number only.
                WriteRaw(Invariant($"{objID.ObjectNumber} {objID.GenerationNumber} obj\n"));
            }
        }

        public void WriteFileHeader(PdfDocument document)
        {
            var header = new StringBuilder("%PDF-");
            int version = document.Version;
            //header.Append((version / 10).ToString(CultureInfo.InvariantCulture) + "." +
            //  (version % 10).ToString(CultureInfo.InvariantCulture) + "\n%\xD3\xF4\xCC\xE1\n");
            header.Append(Invariant($"{version / 10}.{version % 10}\n%\xD3\xF4\xCC\xE1\n"));
            WriteRaw(header.ToString());

            if (IsVerboseLayout)
            {
                WriteRaw($"% PDFsharp {PdfSharpProductVersionInformation.Version} (verbose layout)\n");
                WriteRaw($"% Version  {PdfSharpProductVersionInformation.SemanticVersion} ({Capabilities.Build.BuildName}) under .NET {Capabilities.Build.Framework}\n");
                // Keep some space for later fix-up.
                _commentPosition = (int)_stream.Position + 2;
                WriteRaw("%                                                          \n");  // Title placeholder
                WriteRaw("%                                                          \n");  // Creation date placeholder
                WriteRaw("%                                                          \n");  // Creation time placeholder
                WriteRaw("%                                                          \n");  // File size placeholder
                WriteRaw("%                                                          \n");  // Pages placeholder
                WriteRaw("%                                                          \n");  // Objects placeholder
                WriteRaw("%                                                          \n");  // PDF/A placeholder
                WriteRaw("%                                                          \n");  // Embedded files placeholder
                WriteRaw("%                                                          \n");  // Signature type placeholder
                WriteRaw("%                                                          \n");  // Encryption type placeholder
                WriteRaw("%                                                          \n");  // User password placeholder
                WriteRaw("%                                                          \n");  // Owner password placeholder
#if DEBUG
                WriteRaw("% This document is created from a DEBUG build. Do not use a DEBUG build of PDFsharp for production.\n");
#endif
                WriteRaw("%--------------------------------------------------------------------------------------------------\n");
            }
        }

        // ReSharper disable once IdentifierTypo
        public void WriteEof(PdfDocument document, SizeType startxref)
        {
            WriteRaw($"% Created with PDFsharp {PdfSharpProductVersionInformation.SemanticVersion} ({Capabilities.Build.BuildName}) under .NET {Capabilities.Build.Framework}\n");
#if DEBUG
            WriteRaw($"% Branch {SemVersionInformation.BranchName} ({SemVersionInformation.Sha}) {SemVersionInformation.CommitDate}\n");
#endif
            WriteRaw("startxref\n");
            WriteRaw(startxref.ToString(CultureInfo.InvariantCulture));
            WriteRaw("\n%%EOF\n");
            SizeType fileSize = (SizeType)_stream.Position;
            if (IsVerboseLayout)
            {
                // Note
                // Without InvariantCulture parameter the following code fails if the current culture is e.g.
                // a Far East culture, because the date string may contain non-ASCII characters.
                // So never never never never use ToString without a culture info.

                var now = DateTimeOffset.Now;
                TimeSpan duration = now - document.CreationDate;

                const int offset = 60;

                // Set stream to document comment section.
                _stream.Position = _commentPosition;

                var title = _document.Info.Title;
                title = (title.Length switch
                {
                    > 46 => "'" + title[..43] + "...'",
                    > 0 => "'" + title + "'",
                    _ => "«not set»" + new String(' ', 39)
                } + "                                                  ")[..48];
                WriteRaw(Invariant($"Title    {title}"));

                // Always write the real creation date here, i.e. the time on the producing operating system.
                _stream.Position = _commentPosition + offset;
                WriteRaw(Invariant($"Creation date: ·· {now:yyyy-MM-dd HH:mm:sszzz}"));

                _stream.Position = _commentPosition + 2 * offset;
                WriteRaw(Invariant($"Creation time: ·· {duration.TotalSeconds:0.0##} seconds"));

                _stream.Position = _commentPosition + 3 * offset;
                WriteRaw(Invariant($"File size ······· {fileSize:#,###} bytes"));

                _stream.Position = _commentPosition + 4 * offset;
                WriteRaw(Invariant($"Pages ··········· {document.Pages.Count:#}"));  // No thousands separator here.

                _stream.Position = _commentPosition + 5 * offset;
                WriteRaw(Invariant($"Ind. objects ···· {document.IrefTable.Count:#,###}"));

                _stream.Position = _commentPosition + 6 * offset;
                WriteRaw(Invariant($"PDF/A ··········· {document.GetPdfAManager().Format.Name}"));

                _stream.Position = _commentPosition + 7 * offset;
                WriteRaw(Invariant($"Embedded files ·· {document.GetEmbeddedFilesManager().FileCount}"));

                _stream.Position = _commentPosition + 8 * offset;
                WriteRaw(Invariant($"Signature type ·· {document.GetSigningManager().SignatureType}"));

                var sm = document.GetSecurityManager();

                _stream.Position = _commentPosition + 9 * offset;
                WriteRaw(Invariant($"Encryption type · {sm.EncryptionType}"));

                _stream.Position = _commentPosition + 10 * offset;
                WriteRaw(Invariant($"User password ··· {sm.UserPassword}"));

                _stream.Position = _commentPosition + 11 * offset;
                WriteRaw(Invariant($"Owner password ·· {sm.OwnerPassword}"));

                //“” „“ ’ ‘’ ‚‘ »« ›‹ –
                //· × ² ³ ½ € † …
                //✔ ✘ ↯ ± − × ÷ ⋅ √ ≠ ≤ ≥ ≡
                //® © ← ↑ → ↓ ↔ ↕ ∅
                //✔⇒
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
                if (type == typeof(PdfArray) || type == typeof(PdfDictionary))
                {
                    if (!String.IsNullOrEmpty(comment))
                        result = Invariant($"% {value.GetType().Name} -- {comment}\n");
                    else
                        result = $"% {value.GetType().Name}\n";
                }
                else
                {
                    var name = type.Name;
                    var fullName = type.FullName;
                    if (!String.IsNullOrEmpty(comment))
                        result = Invariant($"% {value.GetType().Name} ({value.GetType().FullName}) -- {comment}\n");
                    else
                        result = $"% {value.GetType().Name} ({value.GetType().FullName})\n";
                }
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
        void IncreaseIndent() => _writeIndent += _indent;

        /// <summary>
        /// Decreases indent level.
        /// </summary>
        void DecreaseIndent() => _writeIndent -= _indent;

        /// <summary>
        /// Gets an indent string of current indent.
        /// </summary>
        string IndentBlanks => new(' ', _writeIndent);

        void WriteIndent() => WriteRaw(IndentBlanks);

        /// <summary>
        /// Writes a space depending on the category of the last character.
        /// </summary>
        /// <param name="cat"></param>
        void WriteSeparator(CharCat cat)
        {
            if (IsCompactLayout)
            {
                // Write a blank only if two characters are neighbors.
                switch (_lastCat)
                {
                    case CharCat.NewLine:
                    case CharCat.Delimiter:
                        break;

                    case CharCat.Character:
                        if (cat == CharCat.Character)
                        {
                            //_stream.WriteByte((byte)' ');
                            //_lastCat = CharCat.Delimiter;
                            WriteRaw(' ');
                        }
                        break;
                }
            }
            else
            {
                switch (_lastCat)
                {
                    case CharCat.NewLine:
                        if (IsIndentedLayout)
                            WriteIndent();
                        break;

                    case CharCat.Delimiter:
                        // Case e.g. “[0 […]0]” should be “[0 […] 0]”. And yes, that’s very nerdy to fix such details.
                        if (_lastChar is ']' or '>' or ')' or '}')  // '}' may occur in PostScript functions.
                            WriteRaw(' ');
                        break;

                    case CharCat.Character:
                        if (IsStandardLayout)
                            WriteRaw(' ');
                        else
                        {
                            if (cat == CharCat.Character)
                                WriteRaw(' ');
                        }
                        break;
                }
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
            /// <summary>
            /// A LF character.
            /// </summary>
            NewLine,

            /// <summary>
            /// A regular character as used in keywords ('true', 'false', 'null', or 'R') or a number used
            /// in an indirect reference or literals.
            /// </summary>
            Character,

            /// <summary>
            /// A PDF delimiter character ('(', ',')', '&lt;', '>', '[', ']', '{', '}', '/')
            /// </summary>
            Delimiter,
        }

        /// <summary>
        /// Character Category of the last character written.
        /// </summary>
        CharCat _lastCat;

        /// <summary>
        /// Last character of the last character written.
        /// Needed for optimal verbose layout.
        /// </summary>
        char _lastChar;

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        internal Stream Stream => _stream;

        Stream _stream = pdfStream ?? throw new ArgumentNullException(nameof(pdfStream));
        readonly PdfDocument _document = document ?? throw new ArgumentNullException(nameof(document));

        internal PdfStandardSecurityHandler? EffectiveSecurityHandler { get; set; } = effectiveSecurityHandler;

        class StackItem(PdfObject value, int childItemCount)
        {
            public readonly PdfObject Object = value;
            public readonly int ChildItemCount = childItemCount;
            public bool HasStream;
        }

        readonly List<StackItem> _stack = [];
        int _commentPosition;
    }
}
