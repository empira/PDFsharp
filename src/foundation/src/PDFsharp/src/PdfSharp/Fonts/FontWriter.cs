// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable IDE0290

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Represents a writer for generation of font file streams. 
    /// </summary>
    class FontWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FontWriter"/> class.
        /// Data is written in Motorola format (big-endian).
        /// </summary>
        public FontWriter(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Closes the writer and, if specified, the underlying stream.
        /// </summary>
        public void Close(bool closeUnderlyingStream)
        {
            if (_stream != null! && closeUnderlyingStream)
            {
                _stream.Close();
                _stream.Dispose();
            }
            _stream = null!;
        }

        /// <summary>
        /// Closes the writer and the underlying stream.
        /// </summary>
        public void Close() 
            => Close(true);

        /// <summary>
        /// Gets or sets the position within the stream.
        /// </summary>
        public int Position
        {
            get => (int)_stream.Position;
            set => _stream.Position = value;
        }

        /// <summary>
        /// Writes the specified value to the font stream.
        /// </summary>
        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        /// <summary>
        /// Writes the specified value to the font stream.
        /// </summary>
        public void WriteByte(int value)
        {
            _stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Writes the specified value to the font stream using big-endian.
        /// </summary>
        public void WriteShort(short value)
        {
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Writes the specified value to the font stream using big-endian.
        /// </summary>
        public void WriteShort(int value)
        {
            WriteShort((short)value);
        }

        /// <summary>
        /// Writes the specified value to the font stream using big-endian.
        /// </summary>
        public void WriteUShort(ushort value)
        {
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Writes the specified value to the font stream using big-endian.
        /// </summary>
        public void WriteUShort(int value)
        {
            WriteUShort((ushort)value);
        }

        /// <summary>
        /// Writes the specified value to the font stream using big-endian.
        /// </summary>
        public void WriteInt(int value)
        {
            _stream.WriteByte((byte)(value >> 24));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Writes the specified value to the font stream using big-endian.
        /// </summary>
        public void WriteUInt(uint value)
        {
            _stream.WriteByte((byte)(value >> 24));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)value);
        }

        //public short ReadFWord()
        //public ushort ReadUFWord()
        //public long ReadLongDate()
        //public string ReadString(int size)

        public void Write(byte[] buffer)
        {
            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        internal Stream Stream => _stream;

        Stream _stream;
    }
}
