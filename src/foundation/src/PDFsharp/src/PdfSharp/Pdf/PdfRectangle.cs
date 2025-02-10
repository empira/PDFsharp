// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
#endif
using System.CodeDom;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an immutable PDF rectangle value.
    /// In a PDF file it is represented by an array of four real values.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class PdfRectangle : PdfItem
    {
        // This reference type must behave like a value type. Therefore, it cannot be changed (like System.String).

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with all values set to zero.
        /// </summary>
        public PdfRectangle()
        { }

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with two points specifying
        /// two diagonally opposite corners. Notice that in contrast to GDI+ convention the 
        /// 3rd and the 4th parameter specify a point and not a width. This is so much confusing
        /// that this function is for internal use only.
        /// </summary>
        internal PdfRectangle(double x1, double y1, double x2, double y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

#if GDI
        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with two points specifying
        /// two diagonally opposite corners.
        /// </summary>
        public PdfRectangle(PointF pt1, PointF pt2)
        {
            X1 = pt1.X;
            Y1 = pt1.Y;
            X2 = pt2.X;
            Y2 = pt2.Y;
        }
#endif

#if WPF
        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with two points specifying
        /// two diagonally opposite corners.
        /// </summary>
        public PdfRectangle(Point pt1, Point pt2)
        {
            X1 = pt1.X;
            Y1 = pt1.Y;
            X2 = pt2.X;
            Y2 = pt2.Y;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with two points specifying
        /// two diagonally opposite corners.
        /// </summary>
        public PdfRectangle(XPoint pt1, XPoint pt2)
        {
            X1 = pt1.X;
            Y1 = pt1.Y;
            X2 = pt2.X;
            Y2 = pt2.Y;
        }

#if GDI
        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with the specified location and size.
        /// </summary>
        public PdfRectangle(PointF pt, SizeF size)
        {
            X1 = pt.X;
            Y1 = pt.Y;
            X2 = pt.X + size.Width;
            Y2 = pt.Y + size.Height;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with the specified location and size.
        /// </summary>
        public PdfRectangle(XPoint pt, XSize size)
        {
            X1 = pt.X;
            Y1 = pt.Y;
            X2 = pt.X + size.Width;
            Y2 = pt.Y + size.Height;
        }

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with the specified XRect.
        /// </summary>
        public PdfRectangle(XRect rect)
        {
            if (rect.IsEmpty)
                throw new InvalidOperationException("Cannot create PdfRectangle from an empty XRect.");

            X1 = rect.X;
            Y1 = rect.Y;
            X2 = rect.X + rect.Width;
            Y2 = rect.Y + rect.Height;
        }

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with the specified PdfArray.
        /// </summary>
        internal PdfRectangle(PdfItem item)
        {
            if (item is null or PdfNull)
                return;

            if (item is PdfReference reference)
                item = reference.Value;

            var array = item as PdfArray;
            if (array == null)
                throw new InvalidOperationException(PsMsgs.UnexpectedTokenInPdfFile);

            X1 = array.Elements.GetReal(0);
            Y1 = array.Elements.GetReal(1);
            X2 = array.Elements.GetReal(2);
            Y2 = array.Elements.GetReal(3);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public new PdfRectangle Clone() => (PdfRectangle)Copy();

        /// <summary>
        /// Implements cloning this instance.
        /// </summary>
        protected override object Copy() => (PdfRectangle)base.Copy();

        /// <summary>
        /// Tests whether all coordinates are zero.
        /// </summary>
        [Obsolete("Use 'IsZero' instead.")]
        public bool IsEmpty => IsZero;

        /// <summary>
        /// Tests whether all coordinates are zero.
        /// </summary>
        public bool IsZero => X1 == 0 && Y1 == 0 && X2 == 0 && Y2 == 0;

        /// <summary>
        /// Tests whether the specified object is a PdfRectangle and has equal coordinates.
        /// </summary>
        // ReSharper disable CompareOfFloatsByEqualityOperator
        public override bool Equals(object? obj) =>
            obj is PdfRectangle rect && rect.X1 == X1 && rect.Y1 == Y1 && rect.X2 == X2 && rect.Y2 == Y2;
        // ReSharper restore CompareOfFloatsByEqualityOperator

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode()
        {
            // This code is from System.Drawing...
            return (int)(((((uint)X1) ^ ((((uint)Y1) << 13) |
              (((uint)Y1) >> 0x13))) ^ ((((uint)X2) << 0x1a) |
              (((uint)X2) >> 6))) ^ ((((uint)Y2) << 7) |
              (((uint)Y2) >> 0x19)));
        }

        /// <summary>
        /// Tests whether two structures have equal coordinates.
        /// </summary>
        public static bool operator ==(PdfRectangle? left, PdfRectangle? right)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            // use: if (Object.ReferenceEquals(left, null))
            if ((object?)left != null)
            {
                if ((object?)right != null)
                    return left.X1 == right.X1 && left.Y1 == right.Y1 && left.X2 == right.X2 && left.Y2 == right.Y2;
                return false;
            }
            return (object?)right == null;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Tests whether two structures differ in one or more coordinates.
        /// </summary>
        public static bool operator !=(PdfRectangle? left, PdfRectangle? right) => !(left == right);

        /// <summary>
        /// Gets or sets the x-coordinate of the first corner of this PdfRectangle.
        /// </summary>
        public double X1 { get; }

        /// <summary>
        /// Gets or sets the y-coordinate of the first corner of this PdfRectangle.
        /// </summary>
        public double Y1 { get; }

        /// <summary>
        /// Gets or sets the x-coordinate of the second corner of this PdfRectangle.
        /// </summary>
        public double X2 { get; }

        /// <summary>
        /// Gets or sets the y-coordinate of the second corner of this PdfRectangle.
        /// </summary>
        public double Y2 { get; }

        /// <summary>
        /// Gets X2 - X1.
        /// </summary>
        public double Width => X2 - X1;

        /// <summary>
        /// Gets Y2 - Y1.
        /// </summary>
        public double Height => Y2 - Y1;

        /// <summary>
        /// Gets or sets the coordinates of the first point of this PdfRectangle.
        /// </summary>
        public XPoint Location => new(X1, Y1);

        /// <summary>
        /// Gets or sets the size of this PdfRectangle.
        /// </summary>
        public XSize Size => new(X2 - X1, Y2 - Y1);

#if GDI
        /// <summary>
        /// Determines if the specified point is contained within this PdfRectangle.
        /// </summary>
        public bool Contains(PointF pt) => Contains(pt.X, pt.Y);
#endif

        /// <summary>
        /// Determines if the specified point is contained within this PdfRectangle.
        /// </summary>
        public bool Contains(XPoint pt) => Contains(pt.X, pt.Y);

        /// <summary>
        /// Determines if the specified point is contained within this PdfRectangle.
        /// </summary>
        public bool Contains(double x, double y) =>
            // Treat rectangle inclusive/inclusive.
            X1 <= x && x <= X2 && Y1 <= y && y <= Y2;

#if GDI
        /// <summary>
        /// Determines if the rectangular region represented by rect is entirely contained within this PdfRectangle.
        /// </summary>
        public bool Contains(RectangleF rect) =>
            X1 <= rect.X && (rect.X + rect.Width) <= X2 &&
            Y1 <= rect.Y && (rect.Y + rect.Height) <= Y2;
#endif

        /// <summary>
        /// Determines if the rectangular region represented by rect is entirely contained within this PdfRectangle.
        /// </summary>
        public bool Contains(XRect rect) =>
            X1 <= rect.X && (rect.X + rect.Width) <= X2 &&
            Y1 <= rect.Y && (rect.Y + rect.Height) <= Y2;

        /// <summary>
        /// Determines if the rectangular region represented by rect is entirely contained within this PdfRectangle.
        /// </summary>
        public bool Contains(PdfRectangle rect) =>
            X1 <= rect.X1 && rect.X2 <= X2 &&
            Y1 <= rect.Y1 && rect.Y2 <= Y2;

        /// <summary>
        /// Returns the rectangle as an XRect object.
        /// </summary>
        public XRect ToXRect() => new(X1, Y1, Width, Height);

        /// <summary>
        /// Returns the rectangle as a string in the form «[x1 y1 x2 y2]».
        /// </summary>
        public override string ToString()
        {
            const string format = Config.SignificantDecimalPlaces3;
            return PdfEncoders.Format("[{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "}]", X1, Y1, X2, Y2);
        }

        /// <summary>
        /// Writes the rectangle.
        /// </summary>
        internal override void WriteObject(PdfWriter writer) => writer.Write(this);

        /// <summary>
        /// Represents an empty PdfRectangle.
        /// </summary>
        [Obsolete("A rectangle defined by two points cannot be meaningfully defined as empty. Do not use this property.")]
        public static PdfRectangle Empty => throw new InvalidOperationException("Use 'new PdfRectangle()' instead of 'PdfRectangle.Empty'");

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
        {
            get
            {
                const string format = Config.SignificantDecimalPlaces10;
                return String.Format(CultureInfo.InvariantCulture,
                    "X1={0:" + format + "}, Y1={1:" + format + "}, X2={2:" + format + "}, Y2={3:" + format + "}", X1, Y1, X2, Y2);
            }
        }
    }
}
