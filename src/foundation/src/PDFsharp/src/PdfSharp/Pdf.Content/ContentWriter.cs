// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Represents a writer for generation of PDF streams. 
    /// </summary>
    class ContentWriter
    {
        public ContentWriter(Stream contentStream)
        {
            _stream = contentStream;
#if DEBUG
            //layout = PdfWriterLayout.Verbose;
#endif
        }

        public void Close(bool closeUnderlyingStream)
        {
            if (_stream != null && closeUnderlyingStream)
            {
                _stream.Close();
                _stream = null;
            }
        }

        public void Close() => Close(true);

        public int Position => (int)(_stream?.Position ?? 0);

        //public PdfWriterLayout Layout
        //{
        //  get { return layout; }
        //  set { layout = value; }
        //}
        //PdfWriterLayout layout;

        //public PdfWriterOptions Options
        //{
        //  get { return options; }
        //  set { options = value; }
        //}
        //PdfWriterOptions options;

        // -----------------------------------------------------------

        /// <summary>
        /// Writes the specified value to the PDF stream.
        /// </summary>
        public void Write(bool value)
        {
            //WriteSeparator(CharCat.Character);
            //WriteRaw(value ? bool.TrueString : bool.FalseString);
            //lastCat = CharCat.Character;
        }

        public void WriteRaw(string rawString)
        {
            if (String.IsNullOrEmpty(rawString))
                return;
            //AppendBlank(rawString[0]);
            byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            _stream?.Write(bytes, 0, bytes.Length);
            _lastCat = GetCategory((char)bytes[bytes.Length-1]);
        }

        public void WriteLineRaw(string rawString)
        {
            if (String.IsNullOrEmpty(rawString))
                return;
            //AppendBlank(rawString[0]);
            byte[] bytes = PdfEncoders.RawEncoding.GetBytes(rawString);
            _stream?.Write(bytes, 0, bytes.Length);
            _stream?.Write(new byte[] { (byte)'\n' }, 0, 1);
            _lastCat = GetCategory((char)bytes[bytes.Length-1]);
        }

        public void WriteRaw(char ch)
        {
            Debug.Assert(ch < 256, "Raw character greater than 255 detected.");
            _stream?.WriteByte((byte)ch);
            _lastCat = GetCategory(ch);
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

        void WriteIndent()
            => WriteRaw(IndentBlanks);

        void WriteSeparator(CharCat cat, char ch)
        {
            switch (_lastCat)
            {
                //case CharCat.NewLine:
                //  if (this.layout == PdfWriterLayout.Verbose)
                //    WriteIndent();
                //  break;

                case CharCat.Delimiter:
                    break;

                    //case CharCat.Character:
                    //  if (this.layout == PdfWriterLayout.Verbose)
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
        }
        CharCat _lastCat;

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        internal Stream Stream => _stream ?? NRT.ThrowOnNull<Stream>();

        Stream? _stream = null!;
    }
}
