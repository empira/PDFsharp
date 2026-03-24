// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Pdf.Content.Objects;

// v7.0.0 REVIEW

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Represents a writer for generation of PDF streams. 
    /// </summary>
    public class ContentWriter
    {
        public ContentWriter(ContentWriterOptions options)
        {
            //_stream = contentStream;
            _options = options;
        }
        StringBuilder _sb = new StringBuilder();

        public void Close(bool closeUnderlyingStream)
        {
            //if (_stream != null && closeUnderlyingStream)
            //{
            //    _stream.Close();
            //    _stream = null;
            //}
        }

        public void Close() => Close(true);

        public void Clear()
        {
            _sb.Clear();
        }

        public override String ToString()
        {
            return _sb.ToString();
        }

        public void Write(CNumber number)
        {
            var s = number.ToString();
            _sb.Append(s);
            _sb.Append(' ');  // HACK
        }

        public void Write(string s)
        {
            _sb.Append(s);
        }

        public void Write(CName name)
        {
            _sb.Append(name);
            _sb.Append(' ');  // HACK
        }

        public void Write(CArray array)
        {
            _sb.Append(array.ToString());  // HACK
        }

        public void WriteRaw(string rawString)
        {
#if true
            if (String.IsNullOrEmpty(rawString))
                return;
            _sb.Append(rawString);

            //AppendBlank(rawString[0]);
            //byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            //_stream?.Write(bytes, 0, bytes.Length);
            // ReSharper disable once UseIndexFromEndExpression
            _lastCat = GetCategory(rawString[rawString.Length - 1]);
#else
            if (String.IsNullOrEmpty(rawString))
                return;
            //AppendBlank(rawString[0]);
            byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            _stream?.Write(bytes, 0, bytes.Length);
            // ReSharper disable once UseIndexFromEndExpression
            _lastCat = GetCategory((char)bytes[bytes.Length - 1]);
#endif
        }

        public void WriteLineRaw(string rawString)
        {
#if true
            if (String.IsNullOrEmpty(rawString))
                return;
            _sb.Append(rawString);
            _sb.Append('\n');
            //AppendBlank(rawString[0]);
            //byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            //_stream!.Write(bytes, 0, bytes.Length);
            //_stream!.Write([(byte)'\n'], 0, 1);
            //_lastCat = GetCategory((char)bytes[bytes.Length - 1]);
            _lastCat = GetCategory(rawString[^1]);
#else
            if (String.IsNullOrEmpty(rawString))
                return;
            //AppendBlank(rawString[0]);
            byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            _stream!.Write(bytes, 0, bytes.Length);
            _stream!.Write([(byte)'\n'], 0, 1);
            _lastCat = GetCategory((char)bytes[bytes.Length - 1]);
#endif
        }

        public void WriteRaw(char ch)
        {
#if true
            Debug.Assert(ch < 256, "Raw character greater than 255 detected.");
            _sb.Append(ch);
            //_stream?.WriteByte((byte)ch);
            _lastCat = GetCategory(ch);
#else
            Debug.Assert(ch < 256, "Raw character greater than 255 detected.");
            _stream?.WriteByte((byte)ch);
            _lastCat = GetCategory(ch);
#endif
        }

        /// <summary>
        /// Gets or sets the indentation for a new indentation level.
        /// </summary>
        internal int Indent
        {
            get => _indent;
            set => _indent = value;
        }
        protected int _indent = 2;
        int _writeIndent = 0;

        /// <summary>
        /// Increases indent level.
        /// </summary>
        void IncreaseIndent()
            => _writeIndent += _indent;

        /// <summary>
        /// Decreases indent level.
        /// </summary>
        void DecreaseIndent()
            => _writeIndent -= _indent;

        /// <summary>
        /// Gets an indent string of current indent.
        /// </summary>
        string IndentBlanks => new string(' ', _writeIndent);

        void WriteIndent() => WriteRaw(IndentBlanks);

        void WriteSeparator(CharCat cat, char ch)
        {
            switch (_lastCat)
            {
                //case CharCat.NewLine:
                //  if (this.layout == PdfWriterLayout.Ver/bose)
                //    WriteIndent();
                //  break;

                case CharCat.Delimiter:
                    break;

                    //case CharCat.Character:
                    //  if (this.layout == PdfWriterLayout.Ver/bose)
                    //  {
                    //    //if (cat == CharCat.Character || ch == '/')
                    //    this.stream.WriteByte((byte)' ');
                    //  }
                    //  else
                    //  {
                    //    if (cat == CharCat.Character)
                    //      this.stream.WriteByte((byte)' ');
                    //  }
                    //  break;
            }
        }

        void WriteSeparator(CharCat cat)
        {
            WriteSeparator(cat, '\0');
        }

        public void NewLine()
        {
            if (_lastCat != CharCat.NewLine)
                WriteRaw('\n');
        }

        CharCat GetCategory(char ch)
        {
            //if (Lexer.IsDelimiter(ch))
            //  return CharCat.Delimiter;
            //if (ch == Chars.LF)
            //  return CharCat.NewLine;
            return CharCat.Character;
        }

        enum CharCat
        {
            NewLine,
            Character,
            Delimiter,
            IRef
        }
        CharCat _lastCat;

        ///// <summary>
        ///// Gets the underlying stream.
        ///// </summary>
        //internal Stream Stream => _stream ?? NRT.ThrowOnNull<Stream>();

        //Stream? _stream = null!;
        ContentWriterOptions _options;
    }
}

namespace PdfSharp.Pdf.Content
{
    public class ContentWriterOptions
    {
        public ContentWriterLayout Layout { get; set; } =
#if DEBUG
        ContentWriterLayout.LineFeed;
#else
        ContentWriterLayout.SingleLine;
#endif
    }
}

namespace PdfSharp.Pdf.Content  // #FOLDER Pdf.Content.enums
{
    /// <summary>
    /// Determines how the PDF output stream is formatted. Even all formats create valid PDF files,
    /// only Compact or Standard should be used for production purposes.
    /// </summary>
    public enum ContentWriterLayout
    {
        // TODO

        /// <summary>
        /// The content stream is written in one single line.
        /// This is default in release build.
        /// </summary>
        SingleLine,

        /// <summary>
        /// The content stream is written with a line feed after each operator.
        /// This is default in debug build.
        /// </summary>
        LineFeed,

        ///// <summary>
        ///// </summary>
        //Verbose,
    }
}
