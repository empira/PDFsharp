// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace MigraDoc.Rendering
{
    /// <summary>
    /// Abstract base class for all areas to render in.
    /// </summary>
    public abstract class Area
    {
        /// <summary>
        /// Gets the left boundary of the area.
        /// </summary>
        public abstract XUnitPt X { get; internal set; }

        /// <summary>
        /// Gets the top boundary of the area.
        /// </summary>
        public abstract XUnitPt Y { get; internal set; }

        /// <summary>
        /// Gets the largest fitting rect with the given y position and height.
        /// </summary>
        /// <param name="yPosition">Top bound of the searched rectangle.</param>
        /// <param name="height">Height of the searched rectangle.</param>
        /// <returns>
        /// The largest fitting rect with the given y position and height.
        /// Null if yPosition exceeds the area.
        /// </returns>
        internal abstract Rectangle? GetFittingRect(XUnitPt yPosition, XUnitPt height);

        /// <summary>
        /// Gets or sets the height of the smallest rectangle containing the area. 
        /// </summary>
        public abstract XUnitPt Height { get; internal set; }

        /// <summary>
        /// Gets or sets the width of the smallest rectangle containing the area. 
        /// </summary>
        public abstract XUnitPt Width { get; internal set; }

        /// <summary>
        /// Returns the union of this area and the given one.
        /// </summary>
        /// <param name="area">The area to unite with.</param>
        /// <returns>The union of the two areas.</returns>
        internal abstract Area Unite(Area area);

        /// <summary>
        /// Lowers the area and makes it smaller.
        /// </summary>
        /// <param name="verticalOffset">The measure of lowering.</param>
        /// <returns>The lowered Area.</returns>
        internal abstract Area Lower(XUnitPt verticalOffset);
    }

    class Rectangle : Area
    {
        /// <summary>
        /// Initializes a new rectangle object.
        /// </summary>
        /// <param name="x">Left bound of the rectangle.</param>
        /// <param name="y">Upper bound of the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        internal Rectangle(XUnitPt x, XUnitPt y, XUnitPt width, XUnitPt height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Initializes a new Rectangle by copying its values.
        /// </summary>
        /// <param name="rect">The rectangle to copy.</param>
        internal Rectangle(Rectangle rect)
        {
            _x = rect._x;
            _y = rect._y;
            _width = rect._width;
            _height = rect._height;
        }

        /// <summary>
        /// Gets the largest fitting rect with the given y position and height.
        /// </summary>
        /// <param name="yPosition">Top boundary of the requested rectangle.</param>
        /// <param name="height">Height of the requested rectangle.</param>
        /// <returns>The largest fitting rect with the given y position and height, or null if the requested height does not fit.</returns>
        internal override Rectangle? GetFittingRect(XUnitPt yPosition, XUnitPt height)
        {
            if (yPosition + height > _y + _height + Renderer.Tolerance)
                return null;

            return new Rectangle(_x, yPosition, _width, height);
        }

        /// <summary>
        /// Gets or sets the left boundary of the rectangle. 
        /// </summary>
        public override XUnitPt X
        {
            get => _x;
            internal set => _x = value;
        }
        XUnitPt _x;

        /// <summary>
        /// Gets or sets the top boundary of the rectangle. 
        /// </summary>
        public override XUnitPt Y
        {
            get => _y;
            internal set => _y = value;
        }
        XUnitPt _y;

        /// <summary>
        /// Gets or sets the width of the rectangle. 
        /// </summary>
        public override XUnitPt Width
        {
            get => _width;
            internal set => _width = value;
        }
        XUnitPt _width;

        /// <summary>
        /// Gets or sets the height of the rectangle. 
        /// </summary>
        public override XUnitPt Height
        {
            get => _height;
            internal set => _height = value;
        }
        XUnitPt _height;

        /// <summary>
        /// Returns the union of the rectangle and the given area.
        /// </summary>
        /// <param name="area">The area to unite with.</param>
        /// <returns>The union of the two areas.</returns>
        internal override Area Unite(Area? area)
        {
            if (area == null)
                return this;

            // This implementation is of course not correct, but it works for our purposes.
            XUnitPt minTop = Math.Min(_y, area.Y);
            XUnitPt minLeft = Math.Min(_x, area.X);
            XUnitPt maxRight = Math.Max(_x + _width, area.X + area.Width);
            XUnitPt maxBottom = Math.Max(_y + _height, area.Y + area.Height);
            return new Rectangle(minLeft, minTop, maxRight - minLeft, maxBottom - minTop);
        }

        internal override Area Lower(XUnitPt verticalOffset) 
            => new Rectangle(_x, _y + verticalOffset, _width, _height - verticalOffset);
    }
}
