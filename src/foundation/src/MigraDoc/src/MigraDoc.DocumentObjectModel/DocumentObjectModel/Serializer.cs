// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Text;

#pragma warning disable IDE0057  // because we still use .NET Framework

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Object to be passed to the Serialize function of a DocumentObject to convert
    /// it into DDL.
    /// </summary>
    sealed class Serializer
    {
        /// <summary>
        /// A Serializer object for converting MDDOM into DDL.
        /// </summary>
        /// <param name="textWriter">A TextWriter to write DDL in.</param>
        /// <param name="indent">Indent of a new block. Default is 2.</param>
        /// <param name="initialIndent">Initial indent to start with.</param>
        internal Serializer(TextWriter textWriter, int indent, int initialIndent)
        {
            _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
            _indent = indent;
            _writeIndent = initialIndent;
            if (textWriter is StreamWriter)
                WriteStamp();
        }

        /// <summary>
        /// Initializes a new instance of the Serializer class with the specified TextWriter.
        /// </summary>
        internal Serializer(TextWriter textWriter) : this(textWriter, 2, 0)
        { }

        /// <summary>
        /// Initializes a new instance of the Serializer class with the specified TextWriter and indentation.
        /// </summary>
        internal Serializer(TextWriter textWriter, int indent) : this(textWriter, indent, 0)
        { }

        readonly TextWriter _textWriter;

        /// <summary>
        /// Gets or sets the indentation for a new indentation level.
        /// </summary>
        internal int Indent
        {
            get => _indent;
            set => _indent = value;
        }
        int _indent = 2;

        /// <summary>
        /// Gets or sets the initial indentation which precede every line.
        /// </summary>
        internal int InitialIndent
        {
            get => _writeIndent;
            set => _writeIndent = value;
        }
        int _writeIndent;

        /// <summary>
        /// Increases indent of DDL code.
        /// </summary>
        void IncreaseIndent()
        {
            _writeIndent += _indent;
        }

        /// <summary>
        /// Decreases indent of DDL code.
        /// </summary>
        void DecreaseIndent()
        {
            _writeIndent -= _indent;
        }

        /// <summary>
        /// Writes the header for a DDL file containing copyright and creation time information.
        /// </summary>
        void WriteStamp()
        {
            if (_fWriteStamp)
            {
                WriteComment("Created by empira MigraDoc Document Object Model");
                WriteComment(String.Format("generated file created {0:d} at {0:t}", DateTime.Now));
            }
        }

        /// <summary>
        /// Appends a string indented without line feed.
        /// </summary>
        internal void Write(string str)
        {
            string wrappedStr = DoWordWrap(str);
            if (wrappedStr.Length < str.Length && wrappedStr != "")
            {
                WriteLineToStream(wrappedStr);
                Write(str.Substring(wrappedStr.Length));
            }
            else
                WriteToStream(str);
            CommitText();
        }

        /// <summary>
        /// Writes a string indented with line feed.
        /// </summary>
        internal void WriteLine(string str)
        {
            string wrappedStr = DoWordWrap(str);
            if (wrappedStr.Length < str.Length)
            {
                WriteLineToStream(wrappedStr);
                WriteLine(str.Substring(wrappedStr.Length));
            }
            else
                WriteLineToStream(wrappedStr);
            CommitText();
        }

        /// <summary>
        /// Returns the part of the string str that fits into the line (up to 80 chars).
        /// If word-wrap is impossible it returns the input-string str itself.
        /// </summary>
        string DoWordWrap(string str)
        {
            if (str.Length + _writeIndent <= LineBreakBeyond)
                return str;

            int idxCRLF = str.IndexOf("\x0D\x0A", StringComparison.Ordinal);
            if (idxCRLF > 0 && idxCRLF + _writeIndent <= LineBreakBeyond)
                return str.Substring(0, idxCRLF + 1);

            int splitIndexBlank = str.Substring(0, LineBreakBeyond - _writeIndent).LastIndexOf(" ", StringComparison.Ordinal);
            int splitIndexCRLF = str.Substring(0, LineBreakBeyond - _writeIndent).LastIndexOf("\x0D\x0A", StringComparison.Ordinal);
            int splitIndex = Math.Max(splitIndexBlank, splitIndexCRLF);
            if (splitIndex == -1)
                splitIndex = Math.Min(str.IndexOf(" ", LineBreakBeyond - _writeIndent + 1, StringComparison.Ordinal),
                                      str.IndexOf("\x0D\x0A", LineBreakBeyond - _writeIndent + 1, StringComparison.Ordinal));
            return splitIndex > 0 ? str.Substring(0, splitIndex) : str;

        }

        /// <summary>
        /// Writes an empty line.
        /// </summary>
        internal void WriteLine()
        {
            WriteLine("");
        }

        /// <summary>
        /// Write a line without committing (without marking the text as serialized).
        /// </summary>
        internal void WriteLineNoCommit(string str)
        {
            WriteLineToStream(str);
        }

        /// <summary>
        /// Write a line without committing (without marking the text as serialized).
        /// </summary>
        internal void WriteLineNoCommit()
        {
            WriteLineNoCommit("");
        }

        /// <summary>
        /// Writes a text as comment and automatically word-wraps it.
        /// </summary>
        internal void WriteComment(string? comment)
        {
            if (String.IsNullOrEmpty(comment))
                return;

            // If string contains CR/LF, split up recursively.
            int crlf = comment.IndexOf("\x0D\x0A", StringComparison.Ordinal);
            if (crlf != -1)
            {
                WriteComment(comment.Substring(0, crlf));
                WriteComment(comment.Substring(crlf + 2));
                return;
            }
            CloseUpLine();
            int len;
            int chopBeyond = LineBreakBeyond - _indent - "// ".Length;
            while ((len = comment.Length) > 0)
            {
                string wrt;
                if (len <= chopBeyond)
                {
                    wrt = $"// {comment}";
                    comment = "";
                }
                else
                {
                    int idxChop;
                    if ((idxChop = comment.LastIndexOf(' ', chopBeyond)) == -1 &&
                        (idxChop = comment.IndexOf(' ', chopBeyond)) == -1)
                    {
                        wrt = $"// {comment}";
                        comment = "";
                    }
                    else
                    {
                        wrt = $"// {comment.Substring(0, idxChop)}";
                        comment = comment.Substring(idxChop + 1);
                    }
                }
                WriteLineToStream(wrt);
                CommitText();
            }
        }

        /// <summary>
        /// Writes a line break if the current position is not at the beginning
        /// of a new line.
        /// </summary>
        internal void CloseUpLine()
        {
            if (_linePos > 0)
                WriteLine();
        }

        /// <summary>
        /// Effectively writes text to the stream. The text is automatically indented and
        /// word-wrapped. A given text never gets word-wrapped to keep comments or string
        /// literals unbroken.
        /// </summary>
        void WriteToStream(string text, bool fLineBreak, bool fAutoIndent)
        {
            // If string contains CR/LF, split up recursively.
            int crlf = text.IndexOf("\x0D\x0A", StringComparison.Ordinal);
            if (crlf != -1)
            {
                WriteToStream(text.Substring(0, crlf), true, fAutoIndent);
                WriteToStream(text.Substring(crlf + 2), fLineBreak, fAutoIndent);
                return;
            }

            int len = text.Length;
            if (len > 0)
            {
                if (_linePos > 0)
                {
                    // Does not work.
                    // if (IsBlankRequired(this .lastChar, _text[0]))
                    //   _text = "·" + _text;
                }
                else
                {
                    if (fAutoIndent)
                    {
                        text = Indentation + text;
                        len += _writeIndent;
                    }
                }
                _textWriter.Write(text);
                _linePos += len;
                // Wordwrap required?
                if (_linePos > LineBreakBeyond)
                {
                    fLineBreak = true;
                    //this .textWriter.Write("//¶");  // for debugging only
                }
                else
                    _lastChar = text[len - 1];
            }

            if (fLineBreak)
            {
                _textWriter.WriteLine("");  // what a line break is may depend on encoding
                _linePos = 0;
                _lastChar = '\x0A';
            }
        }

        /// <summary>
        /// Write the text into the stream without breaking it and adds an indentation to it.
        /// </summary>
        void WriteToStream(string text)
        {
            WriteToStream(text, false, true);
        }

        /// <summary>
        /// Write a line to the stream.
        /// </summary>
        void WriteLineToStream(string text)
        {
            WriteToStream(text, true, true);
        }

        ///// <summary>
        ///// Mighty function to figure out if a blank is required as separator.
        ///// // Does not work without context...
        ///// </summary>
        //bool IsBlankRequired(char left, char right)
        //{
        //    if (left == ' ' || right == ' ')
        //        return false;

        //    // 1st try
        //    bool leftLetterOrDigit = Char.IsLetterOrDigit(left);
        //    bool rightLetterOrDigit = Char.IsLetterOrDigit(right);

        //    if (leftLetterOrDigit && rightLetterOrDigit)
        //        return true;

        //    return false;
        //}

        /// <summary>
        /// Start attribute part.
        /// </summary>
        internal int BeginAttributes()
        {
            int pos = Position;
            WriteLineNoCommit("[");
            IncreaseIndent();
            BeginBlock();
            return pos;
        }

        /// <summary>
        /// Start attribute part.
        /// </summary>
        internal int BeginAttributes(string str)
        {
            int pos = Position;
            WriteLineNoCommit(str);
            WriteLineNoCommit("[");
            IncreaseIndent();
            BeginBlock();
            return pos;
        }

        /// <summary>
        /// End attribute part.
        /// </summary>
        internal bool EndAttributes()
        {
            DecreaseIndent();
            WriteLineNoCommit("]");
            return EndBlock();
        }

        /// <summary>
        /// End attribute part.
        /// </summary>
        internal bool EndAttributes(int pos)
        {
            bool commit = EndAttributes();
            if (!commit)
                Position = pos;
            return commit;
        }

        /// <summary>
        /// Write attribute of type Unit, Color, int, float, double, bool, string, or enum.
        /// </summary>
        internal void WriteSimpleAttribute(string valueName, object value)
        {
            if (value is INullableValue ival)
                value = ival.GetValue();

            Type type;

            if (value is Unit unit)
            {
                string strUnit = unit.ToString(); // BUG Can anything actually return null here? If so, how to write null?
                if (unit.Type == UnitType.Point)
                    WriteLine(valueName + " = " + strUnit);
                else
                    WriteLine(valueName + " = \"" + strUnit + "\"");
            }
            else if (value is float f)
            {
                WriteLine(valueName + " = " + f.ToString(CultureInfo.InvariantCulture));
            }
            else if (value is double d)
            {
                WriteLine(valueName + " = " + d.ToString(CultureInfo.InvariantCulture));
            }
            else if (value is bool b)
            {
                WriteLine(valueName + " = " + b.ToString().ToLower());
            }
            else if (value is string)
            {
                var sb = new StringBuilder(value.ToString());
                sb.Replace("\\", "\\\\");
                sb.Replace("\"", "\\\"");
                WriteLine(valueName + " = \"" + sb + "\"");
            }
            else if (value is int || (type = value.GetType()).BaseType == typeof(Enum) || value is Color)
            {
                WriteLine(valueName + " = " + value);
            }
            else
            {
                Debug.Assert(false, $"Type '{type}' of value '{valueName}' not supported");
            }
        }

        /// <summary>
        /// Start content part.
        /// </summary>
        internal int BeginContent()
        {
            int pos = Position;
            WriteLineNoCommit("{");
            IncreaseIndent();
            BeginBlock();
            return pos;
        }

        /// <summary>
        /// Start content part.
        /// </summary>
        internal int BeginContent(string str)
        {
            int pos = Position;
            WriteLineNoCommit(str);
            WriteLineNoCommit("{");
            IncreaseIndent();
            BeginBlock();
            return pos;
        }

        /// <summary>
        /// End content part.
        /// </summary>
        internal bool EndContent()
        {
            DecreaseIndent();
            WriteLineNoCommit("}");
            return EndBlock();
        }

        /// <summary>
        /// End content part.
        /// </summary>
        internal bool EndContent(int pos)
        {
            bool commit = EndContent();
            if (!commit)
                Position = pos;
            return commit;
        }

        /// <summary>
        /// Starts a new nesting block.
        /// </summary>
        internal int BeginBlock()
        {
            int pos = Position;
            if (_stackIdx + 1 >= _commitTextStack.Length)
                throw new ArgumentException("Block nesting level exhausted.");
            _stackIdx += 1;
            _commitTextStack[_stackIdx] = false;
            return pos;
        }

        /// <summary>
        /// Ends a nesting block.
        /// </summary>
        internal bool EndBlock()
        {
            if (_stackIdx <= 0)
                throw new ArgumentException("Block nesting level underflow.");
            _stackIdx -= 1;
            if (_commitTextStack[_stackIdx + 1])
                _commitTextStack[_stackIdx] = _commitTextStack[_stackIdx + 1];
            return _commitTextStack[_stackIdx + 1];
        }

        /// <summary>
        /// Ends a nesting block.
        /// </summary>
        internal bool EndBlock(int pos)
        {
            bool commit = EndBlock();
            if (!commit)
                Position = pos;
            return commit;
        }

        /// <summary>
        /// Gets or sets the position within the underlying stream.
        /// </summary>
        int Position
        {
            get
            {
                _textWriter.Flush();
                if (_textWriter is StreamWriter streamWriter)
                    return (int)streamWriter.BaseStream.Position;

                if (_textWriter is StringWriter stringWriter)
                    return stringWriter.GetStringBuilder().Length;

                return 0;
            }
            set
            {
                _textWriter.Flush();
                if (_textWriter is StreamWriter streamWriter)
                    streamWriter.BaseStream.SetLength(value);
                else
                {
                    if (_textWriter is StringWriter stringWriter)
                        stringWriter.GetStringBuilder().Length = value;
                }
            }
        }

        /// <summary>
        /// Flushes the buffers of the underlying text writer.
        /// </summary>
        internal void Flush()
        {
            _textWriter.Flush();
        }

        /// <summary>
        /// Returns an indent string of blanks.
        /// </summary>
        static string Ind(int indent)
        {
            return new String(' ', indent);
        }

        /// <summary>
        /// Gets an indent string of current indent.
        /// </summary>
        string Indentation => Ind(_writeIndent);

        /// <summary>
        /// Marks the current block as 'committed'. That means the block contains
        /// serialized data.
        /// </summary>
        void CommitText()
        {
            _commitTextStack[_stackIdx] = true;
        }

        int _stackIdx;
        readonly bool[] _commitTextStack = new bool[32];

        int _linePos;
        const int LineBreakBeyond = 200;
#pragma warning disable IDE0052
        char _lastChar;
#pragma warning restore IDE0052
        readonly bool _fWriteStamp = false;
    }
}
