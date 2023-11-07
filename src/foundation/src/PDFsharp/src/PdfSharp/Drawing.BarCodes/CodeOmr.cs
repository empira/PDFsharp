// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>
    /// Represents an OMR code.
    /// </summary>
    public class CodeOmr : BarCode
    {
        /// <summary>
        /// Initializes a new OmrCode with the given data.
        /// </summary>
        public CodeOmr(string text, XSize size, CodeDirection direction)
            : base(text, size, direction)
        { }

        /// <summary>
        /// Renders the OMR code.
        /// </summary>
        protected internal override void Render(XGraphics gfx, XBrush brush, XFont? font, XPoint position)
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

            //XPoint pt = center - size / 2;
            XPoint pt = position - CodeBase.CalcDistance(AnchorType.TopLeft, Anchor, Size);
            uint value;
            UInt32.TryParse(Text, out value);

            if (_synchronizeCode)
            {
                var rect = new XRect(pt.X, pt.Y, _makerThickness, Size.Height);
                gfx.DrawRectangle(brush, rect);
                pt.X += 2 * _makerDistance;
            }
            for (int idx = 0; idx < 32; idx++)
            {
                if ((value & 1) == 1)
                {
                    var rect = new XRect(pt.X + idx * _makerDistance, pt.Y, _makerThickness, Size.Height);
                    gfx.DrawRectangle(brush, rect);
                }
                value = value >> 1;
            }
            gfx.Restore(state);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a synchronize mark is rendered.
        /// </summary>
        public bool SynchronizeCode
        {
            get => _synchronizeCode;
            set => _synchronizeCode = value;
        }

        bool _synchronizeCode;

        /// <summary>
        /// Gets or sets the distance of the markers.
        /// </summary>
        public double MakerDistance
        {
            get => _makerDistance;
            set => _makerDistance = value;
        }

        double _makerDistance = 12;  // 1/6"

        /// <summary>
        /// Gets or sets the thickness of the markers.
        /// </summary>
        public double MakerThickness
        {
            get => _makerThickness;
            set => _makerThickness = value;
        }

        double _makerThickness = 1;

        ///// <summary>
        ///// Renders the mark at the given position.
        ///// </summary>
        ///// <param name="position">The mark position to render.</param>
        //private void RenderMark(int position)
        //{
        //  double yPos =  TopLeft.Y + UpperDistance + position * ToUnit(markDistance).Centimeter;
        //  //Center mark
        //  double xPos = TopLeft.X + Width / 2 - this.MarkWidth / 2;

        //  Gfx.DrawLine(pen, xPos, yPos, xPos + MarkWidth, yPos);
        //}

        ///// <summary>
        ///// Distance of the marks. Default is 2/6 inch.
        ///// </summary>
        //public MarkDistance MarkDistance
        //{
        //  get { return markDistance; }
        //  set { markDistance = value; }
        //}
        //private MarkDistance markDistance = MarkDistance.Inch2_6;

        ///// <summary>
        ///// Converts a mark distance to an XUnit object.
        ///// </summary>
        ///// <param name="markDistance">The mark distance to convert.</param>
        ///// <returns>The converted mark distance.</returns>
        //public static XUnit ToUnit(MarkDistance markDistance)
        //{
        //  switch (markDistance)
        //  {
        //    case MarkDistance.Inch1_6:
        //      return XUnit.FromInch(1.0 / 6.0);
        //    case MarkDistance.Inch2_6:
        //      return XUnit.FromInch(2.0 / 6.0);
        //    case MarkDistance.Inch2_8:
        //      return XUnit.FromInch(2.0 / 8.0);
        //    default:
        //      throw new ArgumentOutOfRangeException("markDistance");
        //  }
        //}

        ///// <summary>
        ///// The upper left point of the reading zone.
        ///// </summary>
        //public XPoint TopLeft
        //{
        //  get
        //  {
        //    XPoint topLeft = center;
        //    topLeft.X -= Width;
        //    double height = upperDistance + lowerDistance;
        //    height += (data.Marks.Length - 1) * ToUnit(MarkDistance).Centimeter;
        //    topLeft.Y -= height / 2;
        //    return topLeft;
        //  }
        //}

        ///// <summary>
        ///// the upper distance from position to the first mark.
        ///// The default value is 8 / 6 inch.
        ///// </summary>
        //double UpperDistance
        //{
        //  get { return upperDistance; }
        //  set { upperDistance = value; }
        //}
        //private double upperDistance = XUnit.FromInch(8.0 / 6.0).Centimeter;

        ///// <summary>
        ///// The lower distance from the last possible mark to the end of the reading zone.
        ///// The default value is 
        ///// </summary>
        //double LowerDistance
        //{
        //  get { return lowerDistance; }
        //  set { lowerDistance = value; }
        //}
        //private double lowerDistance = XUnit.FromInch(2.0 / 6.0).Centimeter;

        ///// <summary>
        ///// Gets or sets the width of the reading zone.
        ///// Default and minimum is 3/12 inch. 
        ///// </summary>
        //public double Width
        //{
        //  get { return width; }
        //  set { width = value; }
        //}
        //double width = XUnit.FromInch(3.0 / 12.0).Centimeter;

        ///// <summary>
        ///// Gets or sets the mark width. Default is 1/2 * width.
        ///// </summary>
        //public XUnit MarkWidth
        //{
        //  get
        //  {
        //    if (markWidth > 0)
        //      return markWidth;
        //    else
        //      return width / 2;
        //  }
        //  set { markWidth = value; }
        //}
        //XUnit markWidth;

        ///// <summary>
        ///// Gets or sets the width of the mark line. Default is 1pt.
        ///// </summary>
        //public XUnit MarkLineWidth
        //{
        //  get { return markLineWidth; }
        //  set { markLineWidth = value; }
        //}
        //XUnit markLineWidth = 1;

        /// <summary>
        /// Determines whether the specified string can be used as Text for the OMR code.
        /// </summary>
        protected override void CheckCode(string text)
        { }
    }
}
