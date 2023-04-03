// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Defines the background filling of the shape.
    /// </summary>
    public class FillFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the FillFormat class.
        /// </summary>
        public FillFormat()
        {
            BaseValues = new FillFormatValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the FillFormat class with the specified parent.
        /// </summary>
        internal FillFormat(DocumentObject parent) : base(parent)
        {
            BaseValues = new FillFormatValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new FillFormat Clone() 
            => (FillFormat)DeepCopy();

        /// <summary>
        /// Gets or sets the color of the filling.
        /// </summary>
        public Color Color
        {
            get => Values.Color ?? Color.Empty;
            set => Values.Color = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the background color should be visible.
        /// </summary>
        public bool Visible
        {
            get => Values.Visible ?? false;
            set => Values.Visible = value;
        }

        /// <summary>
        /// Converts FillFormat into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.BeginContent("FillFormat");
            if (Values.Visible is not null)
                serializer.WriteSimpleAttribute("Visible", Visible);
            //if (Values.Color is not null)
            if (!Values.Color.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Color", Color);
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(FillFormat));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public FillFormatValues Values => (FillFormatValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class FillFormatValues : Values
        {
            internal FillFormatValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? Color
            {
                get => _color;
                set => _color = DocumentObjectModel.Color.MakeNullIfEmpty(value);  // Contradicts Unit! BUG investigate what is better
            }
            Color? _color;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Visible { get; set; }
        }
    }
}
