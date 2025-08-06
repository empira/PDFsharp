// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Represents the base class of all codes.
    /// </summary>
    public abstract class CodeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBase"/> class.
        /// </summary>
        protected CodeBase(string text, XSize size, CodeDirection direction)
        {
            _text = text;
            Size = size;
            _direction = direction;
        }

        //public static CodeBase FromType(CodeType type, string text, XSize size, CodeDirection direction)
        //{
        //  switch (type)
        //  {
        //    case CodeType.Code2of5Interleaved:
        //      return new Code2of5Interleaved(text, size, direction);

        //    case CodeType.Code3of9Standard:
        //      return new Code3of9Standard(text, size, direction);

        //    default:
        //      throw new InvalidEnumArgumentException("type", (int)type, typeof(CodeType));
        //  }
        //}

        //public static CodeBase FromType(CodeType type, string text, XSize size)
        //{
        //  return FromType(type, text, size, CodeDirection.LeftToRight);
        //}

        //public static CodeBase FromType(CodeType type, string text)
        //{
        //  return FromType(type, text, XSize.Empty, CodeDirection.LeftToRight);
        //}

        //public static CodeBase FromType(CodeType type)
        //{
        //  return FromType(type, "", XSize.Empty, CodeDirection.LeftToRight);
        //}

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public XSize Size { get; set; }

        /// <summary>
        /// Gets or sets the text the bar code shall represent.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                CheckCode(value);
                _text = value;
            }
        }
        string _text;

        /// <summary>
        /// Always MiddleCenter.
        /// </summary>
        public AnchorType Anchor
        {
            get => _anchor;
            set => _anchor = value;
        }

        AnchorType _anchor;

        /// <summary>
        /// Gets or sets the drawing direction.
        /// </summary>
        public CodeDirection Direction
        {
            get => _direction;
            set => _direction = value;
        }

        CodeDirection _direction;

        /// <summary>
        /// When implemented in a derived class, determines whether the specified string can be used as Text
        /// for this bar code type.
        /// </summary>
        /// <param name="text">The code string to check.</param>
        /// <returns>True if the text can be used for the actual barcode.</returns>
        protected abstract void CheckCode(string text);

        /// <summary>
        /// Calculates the distance between an old anchor point and a new anchor point.
        /// </summary>
        /// <param name="oldType"></param>
        /// <param name="newType"></param>
        /// <param name="size"></param>
        public static XVector CalcDistance(AnchorType oldType, AnchorType newType, XSize size)
        {
            if (oldType == newType)
                return new XVector();

            var delta = Deltas[(int)oldType, (int)newType];
            var result = new XVector(size.Width / 2 * delta.X, size.Height / 2 * delta.Y);
            return result;
        }

        struct Delta
        {
#pragma warning disable IDE0290 // Use primary constructor
            public Delta(int x, int y)
#pragma warning restore IDE0290 // Use primary constructor
            {
                X = x;
                Y = y;
            }
            public readonly int X;
            public readonly int Y;
        }

        static readonly Delta[,] Deltas = new Delta[9, 9]
        {
              { new(0, 0),   new(1, 0),   new(2, 0),  new(0, 1),   new(1, 1),   new(2, 1),  new(0, 2),  new(1, 2),  new(2, 2) },
              { new(-1, 0),  new(0, 0),   new(1, 0),  new(-1, 1),  new(0, 1),   new(1, 1),  new(-1, 2), new(0, 2),  new(1, 2) },
              { new(-2, 0),  new(-1, 0),  new(0, 0),  new(-2, 1),  new(-1, 1),  new(0, 1),  new(-2, 2), new(-1, 2), new(0, 2) },
              { new(0, -1),  new(1, -1),  new(2, -1), new(0, 0),   new(1, 0),   new(2, 0),  new(0, 1),  new(1, 1),  new(2, 1) },
              { new(-1, -1), new(0, -1),  new(1, -1), new(-1, 0),  new(0, 0),   new(1, 0),  new(-1, 1), new(0, 1),  new(1, 1) },
              { new(-2, -1), new(-1, -1), new(0, -1), new(-2, 0),  new(-1, 0),  new(0, 0),  new(-2, 1), new(-1, 1), new(0, 1) },
              { new(0, -2),  new(1, -2),  new(2, -2), new(0, -1),  new(1, -1),  new(2, -1), new(0, 0),  new(1, 0),  new(2, 0) },
              { new(-1, -2), new(0, -2),  new(1, -2), new(-1, -1), new(0, -1),  new(1, -1), new(-1, 0), new(0, 0),  new(1, 0) },
              { new(-2, -2), new(-1, -2), new(0, -2), new(-2, -1), new(-1, -1), new(0, -1), new(-2, 0), new(-1, 0), new(0, 0) },
        };
    }
}