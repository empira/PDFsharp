// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Defines the format of a line in a shape object.
    /// </summary>
    public class LineFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the LineFormat class.
        /// </summary>
        public LineFormat()
        {
            BaseValues = new LineFormatValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Lineformat class with the specified parent.
        /// </summary>
        internal LineFormat(DocumentObject parent) : base(parent)
        {
            BaseValues = new LineFormatValues(this); 
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new LineFormat Clone() 
            => (LineFormat)DeepCopy();

        /// <summary>
        /// Gets or sets a value indicating whether the line should be visible.
        /// </summary>
        public bool Visible
        {
            get => Values.Visible ?? false;
            set => Values.Visible = value;
        }

        /// <summary>
        /// Gets or sets the width of the line in Unit.
        /// </summary>
        public Unit Width
        {
            get => Values.Width ?? Unit.Empty;
            set => Values.Width = value;
        }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        public Color Color
        {
            get => Values.Color ?? Color.Empty;
            set => Values.Color = value;
        }

        /// <summary>
        /// Gets or sets the dash style of the line.
        /// </summary>
        public DashStyle DashStyle
        {
            get => Values.DashStyle ?? DashStyle.Solid;
            set => Values.DashStyle = value;
        }

        /// <summary>
        /// Gets or sets the style of the line.
        /// </summary>
        public LineStyle Style
        {
            get => Values.Style ?? LineStyle.Single;
            set => Values.Style = value;
        }

        /// <summary>
        /// Converts LineFormat into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var pos = serializer.BeginContent("LineFormat");
            if (Values.Visible is not null)
                serializer.WriteSimpleAttribute("Visible", Visible);
            if (Values.Style is not null)
                serializer.WriteSimpleAttribute("Style", Style);
            if (Values.DashStyle is not null)
                serializer.WriteSimpleAttribute("DashStyle", DashStyle);
            //if (Values.Width is not null)
            if (!Values.Width.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Width", Width);
            //if (Values.Color is not null)
            if (!Values.Color.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Color", Color);
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(LineFormat));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public LineFormatValues Values => (LineFormatValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class LineFormatValues : Values
        {
            internal LineFormatValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Visible { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Width { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? Color
            {
                get => _color;
                set => _color = DocumentObjectModel.Color.MakeNullIfEmpty(value);
            }
            Color? _color;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DashStyle? DashStyle { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineStyle? Style { get; set; }
        }
    }
}
