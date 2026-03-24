// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

//using PdfSharp.Drawing;
//using PdfSharp.Fonts;
//using PdfSharp.Internal;
//using PdfSharp.Pdf.Internal;
//using System.Drawing;

#pragma warning disable IDE0290

namespace PdfSharp.Internal.OpenType
{
    public struct OpenTypeGlyphMetrics //: IEquatable<CanvasGlyphMetrics>
    {
        public OpenTypeGlyphMetrics(
            int leftSideBearing, int advanceWidth, int rightSideBearing,
            int topSideBearing, int advanceHeight, int bottomSideBearing,
            int verticalOrigin, int distancesFromHorizontalBaselineToBlackBoxBottom,
            OpenTypeRect drawBounds)
        {
            LeftSideBearing = leftSideBearing;
            AdvanceWidth = advanceWidth;
            RightSideBearing = rightSideBearing;
            TopSideBearing = topSideBearing;
            AdvanceHeight = advanceHeight;
            BottomSideBearing = bottomSideBearing;
            VerticalOrigin = verticalOrigin;
            DistancesFromHorizontalBaselineToBlackBoxBottom = distancesFromHorizontalBaselineToBlackBoxBottom;
            DrawBounds = drawBounds;
        }

        public int LeftSideBearing;
        public int AdvanceWidth;
        public int RightSideBearing;
        public int TopSideBearing;
        public int AdvanceHeight;
        public int BottomSideBearing;
        public int VerticalOrigin;
        public int DistancesFromHorizontalBaselineToBlackBoxBottom;
        public OpenTypeRect DrawBounds;

        //public static bool operator ==(CanvasGlyphMetrics x, CanvasGlyphMetrics y);
        //public static bool operator !=(CanvasGlyphMetrics x, CanvasGlyphMetrics y);
        //public bool Equals(CanvasGlyphMetrics other);
        //public override bool Equals(object obj);
        //public override int GetHashCode();
    }

    public struct OpenTypeRect //: IFormattable
    {
        public OpenTypeRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        //public Rect(double x, double y, double width, double height);

        //public Rect(Point point1, Point point2);

        //public Rect(Point location, Size size);


        public int X;

        public int Y;

        public int Width;

        public int Height;

        public int Left => X;

        public int Top => Y;

        public int Right => X + Width;

        public int Bottom => Y + Height;

        //public static Rect Empty { get; }

        //public bool IsEmpty { get; }

        //public bool Contains(Point point);
        //public void Intersect(Rect rect);
        //public void Union(Rect rect);
        //public void Union(Point point);
        //public override string ToString();
        //public string ToString(IFormatProvider provider);
        //string IFormattable.ToString(string format, IFormatProvider provider);
        //public bool Equals(Rect value);
        //public static bool operator ==(Rect rect1, Rect rect2);
        //public static bool operator !=(Rect rect1, Rect rect2);
        //public override bool Equals(object o);
        //public override int GetHashCode();
    }

    public struct RenderingGlyphMetrics //: IEquatable<CanvasGlyphMetrics>
    {
        public RenderingGlyphMetrics(
            float leftSideBearing, float advanceWidth, float rightSideBearing,
            float topSideBearing, float advanceHeight, float bottomSideBearing,
            float verticalOrigin, float distancesFromHorizontalBaselineToBlackBoxBottom,
            RenderingRect drawBounds)
        {
            LeftSideBearing = leftSideBearing;
            AdvanceWidth = advanceWidth;
            RightSideBearing = rightSideBearing;
            TopSideBearing = topSideBearing;
            AdvanceHeight = advanceHeight;
            BottomSideBearing = bottomSideBearing;
            VerticalOrigin = verticalOrigin;
            DistancesFromHorizontalBaselineToBlackBoxBottom = distancesFromHorizontalBaselineToBlackBoxBottom;
            DrawBounds = drawBounds;
        }

        public float LeftSideBearing;
        public float AdvanceWidth;
        public float RightSideBearing;
        public float TopSideBearing;
        public float AdvanceHeight;
        public float BottomSideBearing;
        public float VerticalOrigin;
        public float DistancesFromHorizontalBaselineToBlackBoxBottom;
        public RenderingRect DrawBounds;

        //public static bool operator ==(CanvasGlyphMetrics x, CanvasGlyphMetrics y);
        //public static bool operator !=(CanvasGlyphMetrics x, CanvasGlyphMetrics y);
        //public bool Equals(CanvasGlyphMetrics other);
        //public override bool Equals(object obj);
        //public override int GetHashCode();
    }

    public struct RenderingRect //: IFormattable
    {
        public RenderingRect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        //public Rect(double x, double y, double width, double height);

        //public Rect(Point point1, Point point2);

        //public Rect(Point location, Size size);


        public float X;

        public float Y;

        public float Width;

        public float Height;

        public float Left => X;

        public float Top => Y;

        public float Right => X + Width;

        public float Bottom => Y + Height;

        //public static Rect Empty { get; }

        //public bool IsEmpty { get; }

        //public bool Contains(Point point);
        //public void Intersect(Rect rect);
        //public void Union(Rect rect);
        //public void Union(Point point);
        //public override string ToString();
        //public string ToString(IFormatProvider provider);
        //string IFormattable.ToString(string format, IFormatProvider provider);
        //public bool Equals(Rect value);
        //public static bool operator ==(Rect rect1, Rect rect2);
        //public static bool operator !=(Rect rect1, Rect rect2);
        //public override bool Equals(object o);
        //public override int GetHashCode();
    }
}
