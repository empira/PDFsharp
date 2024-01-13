// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Defines the DataMatrix 2D barcode. THIS IS AN EMPIRA INTERNAL IMPLEMENTATION. THE CODE IN
    /// THE OPEN SOURCE VERSION IS A FAKE.
    /// </summary>
    public class CodeDataMatrix : MatrixCode
    {
        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix()
            : this("", "", 26, 26, 0, XSize.Empty)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, int length)
            : this(code, "", length, length, 0, XSize.Empty)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, int length, XSize size)
            : this(code, "", length, length, 0, size)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, DataMatrixEncoding dmEncoding, int length, XSize size)
            : this(code, CreateEncoding(dmEncoding, code.Length), length, length, 0, size)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, int rows, int columns)
            : this(code, "", rows, columns, 0, XSize.Empty)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, int rows, int columns, XSize size)
            : this(code, "", rows, columns, 0, size)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, DataMatrixEncoding dmEncoding, int rows, int columns, XSize size)
            : this(code, CreateEncoding(dmEncoding, code.Length), rows, columns, 0, size)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, int rows, int columns, int quietZone)
            : this(code, "", rows, columns, quietZone, XSize.Empty)
        {}

        /// <summary>
        /// Initializes a new instance of CodeDataMatrix.
        /// </summary>
        public CodeDataMatrix(string code, string encoding, int rows, int columns, int quietZone, XSize size)
            : base(code, encoding, rows, columns, size)
        {
            QuietZone = quietZone;
        }

        /// <summary>
        /// Sets the encoding of the DataMatrix.
        /// </summary>
        public void SetEncoding(DataMatrixEncoding dmEncoding)
        {
            Encoding = CreateEncoding(dmEncoding, Text.Length);
        }

        static string CreateEncoding(DataMatrixEncoding dmEncoding, int length)
        {
            string tempencoding = "";
            switch (dmEncoding)
            {
                case DataMatrixEncoding.Ascii:
                    tempencoding = new string('a', length);
                    break;
                case DataMatrixEncoding.C40:
                    tempencoding = new string('c', length);
                    break;
                case DataMatrixEncoding.Text:
                    tempencoding = new string('t', length);
                    break;
                case DataMatrixEncoding.X12:
                    tempencoding = new string('x', length);
                    break;
                case DataMatrixEncoding.EDIFACT:
                    tempencoding = new string('e', length);
                    break;
                case DataMatrixEncoding.Base256:
                    tempencoding = new string('b', length);
                    break;
            }
            return tempencoding;
        }

        /// <summary>
        /// Gets or sets the size of the Matrix' Quiet Zone.
        /// </summary>
        public int QuietZone
        {
            get => _quietZone;
            set => _quietZone = value;
        }

        int _quietZone;

        /// <summary>
        /// Renders the matrix code.
        /// </summary>
        protected internal override void Render(XGraphics gfx, XBrush brush, XPoint position)
        {
            XGraphicsState state = gfx.Save();

            switch (Direction)
            {
                case CodeDirection.RightToLeft:
                    gfx.RotateAtTransform(180, position);
                    break;

                case CodeDirection.TopToBottom:
                    gfx.RotateAtTransform(90, position);
                    break;

                case CodeDirection.BottomToTop:
                    gfx.RotateAtTransform(-90, position);
                    break;
            }

            XPoint pos = position + CalcDistance(Anchor, AnchorType.TopLeft, Size);

            if (!HasMatrixImage) // Cannot use (MatrixImage == null) here.
                MatrixImage = DataMatrixImage.GenerateMatrixImage(Text, Encoding, Rows, Columns, brush);

            if (QuietZone > 0)
            {
                XSize sizeWithZone = new XSize(Size.Width, Size.Height);
                sizeWithZone.Width = sizeWithZone.Width / (Columns + 2 * QuietZone) * Columns;
                sizeWithZone.Height = sizeWithZone.Height / (Rows + 2 * QuietZone) * Rows;

                XPoint posWithZone = new XPoint(pos.X, pos.Y);
                posWithZone.X += Size.Width / (Columns + 2 * QuietZone) * QuietZone;
                posWithZone.Y += Size.Height / (Rows + 2 * QuietZone) * QuietZone;

                gfx.DrawRectangle(XBrushes.White, pos.X, pos.Y, Size.Width, Size.Height);
                gfx.DrawImage(MatrixImage, posWithZone.X, posWithZone.Y, sizeWithZone.Width, sizeWithZone.Height);
            }
            else
                gfx.DrawImage(MatrixImage, pos.X, pos.Y, Size.Width, Size.Height);

            gfx.Restore(state);
        }

        /// <summary>
        /// Determines whether the specified string can be used as data in the DataMatrix.
        /// </summary>
        /// <param name="text">The code to be checked.</param>
        protected override void CheckCode(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            DataMatrixImage mImage = new DataMatrixImage(Text, Encoding, Rows, Columns, XBrushes.Black);
            mImage.Iec16022Ecc200(Columns, Rows, Encoding, Text.Length, Text, 0, 0, 0);
        }
    }
}
