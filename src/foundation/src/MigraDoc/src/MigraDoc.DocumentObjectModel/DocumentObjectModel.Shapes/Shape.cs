// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Base Class for all positionable Classes.
    /// </summary>
    public abstract class Shape : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the Shape class.
        /// </summary>
        protected Shape()
        { }

        /// <summary>
        /// Initializes a new instance of the Shape class with the specified parent.
        /// </summary>
        internal Shape(DocumentObject parent) : base(parent)
        { }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Shape Clone()
            => (Shape)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var shape = (Shape)base.DeepCopy();
            if (shape.Values.WrapFormat is not null)
            {
                shape.Values.WrapFormat = shape.Values.WrapFormat.Clone();
                shape.Values.WrapFormat.Parent = shape;
            }
            if (shape.Values.LineFormat is not null)
            {
                shape.Values.LineFormat = shape.Values.LineFormat.Clone();
                shape.Values.LineFormat.Parent = shape;
            }
            if (shape.Values.FillFormat is not null)
            {
                shape.Values.FillFormat = shape.Values.FillFormat.Clone();
                shape.Values.FillFormat.Parent = shape;
            }
            return shape;
        }

        /// <summary>
        /// Gets or sets the wrapping format of the shape.
        /// </summary>
        public WrapFormat WrapFormat
        {
            get => Values.WrapFormat ??= new WrapFormat(this);
            set
            {
                SetParent(value);
                Values.WrapFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the reference point of the Top property.
        /// </summary>
        public RelativeVertical RelativeVertical
        {
            get => Values.RelativeVertical ?? RelativeVertical.Line;
            set => Values.RelativeVertical = value;
        }

        /// <summary>
        /// Gets or sets the reference point of the Left property.
        /// </summary>
        public RelativeHorizontal RelativeHorizontal
        {
            get => Values.RelativeHorizontal ?? RelativeHorizontal.Character;
            set => Values.RelativeHorizontal = value;
        }

        /// <summary>
        /// Gets or sets the position of the top side of the shape.
        /// </summary>
        public TopPosition Top
        {
            get => Values.Top ??= TopPosition.NullValue;
            set => Values.Top = value;
        }

        /// <summary>
        /// Gets or sets the position of the left side of the shape.
        /// </summary>
        public LeftPosition Left
        {
            get => Values.Left ??= LeftPosition.NullValue;
            set => Values.Left = value;
        }

        /// <summary>
        /// Gets the line format of the shape’s border.
        /// </summary>
        public LineFormat LineFormat
        {
            get => Values.LineFormat ??= new LineFormat(this);
            set
            {
                SetParent(value);
                Values.LineFormat = value;
            }
        }

        /// <summary>
        /// Gets the background filling format of the shape.
        /// </summary>
        public FillFormat FillFormat
        {
            get => Values.FillFormat ??= new FillFormat(this);
            set
            {
                SetParent(value);
                Values.FillFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the shape.
        /// </summary>
        public Unit Height
        {
            get => Values.Height ?? Unit.Empty;
            set => Values.Height = value;
        }

        /// <summary>
        /// Gets or sets the width of the shape.
        /// </summary>
        public Unit Width
        {
            get => Values.Width ?? Unit.Empty;
            set => Values.Width = value;
        }

        /// <summary>
        /// Converts Shape into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            //if (Values.Height is not null)
            if (!Values.Height.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Height", Height);
            //if (Values.Width is not null)
            if (!Values.Width.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Width", Width);
            if (Values.RelativeHorizontal is not null)
                serializer.WriteSimpleAttribute("RelativeHorizontal", RelativeHorizontal);
            if (Values.RelativeVertical is not null)
                serializer.WriteSimpleAttribute("RelativeVertical", RelativeVertical);
            //if (!IsNull("Left")) // B/UG: IsNull("Left") uses DocumentObject.IsNull(). LeftPosition has its own implementation of IsNull().  
            if (!Values.Left.IsValueNullOrEmpty())
                Values.Left?.Serialize(serializer);
            //if (!IsNull("Top")) // B/UG: IsNull("Top") uses DocumentObject.IsNull(). TopPosition has its own implementation of IsNull().  
            if (!Values.Top.IsValueNullOrEmpty())
                Values.Top?.Serialize(serializer);
            Values.WrapFormat?.Serialize(serializer);
            Values.LineFormat?.Serialize(serializer);
            Values.FillFormat?.Serialize(serializer);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Shape));

        /// <summary>
        /// Let the derived class create the Values.
        /// </summary>
        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ShapeValues Values => (ShapeValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ShapeValues : Values
        {
            internal ShapeValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public WrapFormat? WrapFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public RelativeVertical? RelativeVertical { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public RelativeHorizontal? RelativeHorizontal { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TopPosition? Top { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LeftPosition? Left { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineFormat? LineFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public FillFormat? FillFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Height { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Width { get; set; }
        }
    }
}
