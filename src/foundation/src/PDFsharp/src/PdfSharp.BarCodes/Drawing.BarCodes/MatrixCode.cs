// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using PdfSharp.Diagnostics;

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Represents the base class of all 2D codes.
    /// </summary>
    public abstract class MatrixCode : CodeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixCode"/> class.
        /// </summary>
        public MatrixCode(string text, string encoding, int rows, int columns, XSize size)
            : base(text, size, CodeDirection.LeftToRight)
        {
            _encoding = encoding;
            if (String.IsNullOrEmpty(_encoding))
                _encoding = new String('a', Text.Length);

            if (columns < rows)
            {
                _rows = columns;
                _columns = rows;
            }
            else
            {
                _columns = columns;
                _rows = rows;
            }

            Text = text;
        }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        public string Encoding
        {
            get => _encoding;
            set
            {
                _encoding = value;
                _matrixImage = null;
            }
        }
        string _encoding;

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public int Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                _matrixImage = null;
            }
        }
        int _columns;

        /// <summary>
        /// Gets or sets the number of rows.
        /// </summary>
        public int Rows
        {
            get => _rows;
            set
            {
                _rows = value;
                _matrixImage = null;
            }
        }
        int _rows;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public new string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                _matrixImage = null;
            }
        }

        /// <summary>
        /// Gets or sets the MatrixImage.
        /// Getter throws if MatrixImage is null.
        /// Use HasMatrixImage to test if image was created.
        /// </summary>
        internal XImage MatrixImage
        {
            get => _matrixImage ?? NRT.ThrowOnNull<XImage>();
            set => _matrixImage = value;
        }
        XImage? _matrixImage = null!;

        /// <summary>
        /// MatrixImage throws if it is null. Here is a way to check if the image was created.
        /// </summary>
        internal bool HasMatrixImage
        {
            get => _matrixImage is not null;
        }

        /// <summary>
        /// When implemented in a derived class renders the 2D code.
        /// </summary>
        protected internal abstract void Render(XGraphics gfx, XBrush brush, XPoint center);

        /// <summary>
        /// Determines whether the specified string can be used as Text for this matrix code type.
        /// </summary>
        protected override void CheckCode(string text)
        { }
    }
}
