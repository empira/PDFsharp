// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Define how the shape should be wrapped between the texts.
    /// </summary>
    public class WrapFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the WrapFormat class.
        /// </summary>
        public WrapFormat()
        {
            BaseValues = new WrapFormatValues(this); 
        }

        /// <summary>
        /// Initializes a new instance of the WrapFormat class with the specified parent.
        /// </summary>
        internal WrapFormat(DocumentObject parent) : base(parent)
        {
            BaseValues = new WrapFormatValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new WrapFormat Clone() 
            => (WrapFormat)DeepCopy();

        /// <summary>
        /// Gets or sets the wrapping style.
        /// </summary>
        public WrapStyle Style
        {
            get => Values.Style ?? WrapStyle.TopBottom;
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the distance between the top side of the shape with the adjacent text.
        /// </summary>
        public Unit DistanceTop
        {
            get => Values.DistanceTop ?? Unit.Empty;
            set => Values.DistanceTop = value;
        }

        /// <summary>
        /// Gets or sets the distance between the bottom side of the shape with the adjacent text.
        /// </summary>
        public Unit DistanceBottom
        {
            get => Values.DistanceBottom ?? Unit.Empty;
            set => Values.DistanceBottom = value;
        }

        /// <summary>
        /// Gets or sets the distance between the left side of the shape with the adjacent text.
        /// </summary>
        public Unit DistanceLeft
        {
            get => Values.DistanceLeft ?? Unit.Empty;
            set => Values.DistanceLeft = value;
        }

        /// <summary>
        /// Gets or sets the distance between the right side of the shape with the adjacent text.
        /// </summary>
        public Unit DistanceRight
        {
            get => Values.DistanceRight ?? Unit.Empty;
            set => Values.DistanceRight = value;
        }

        /// <summary>
        /// Converts WrapFormat into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (IsNull())
                return; // IsNull called here so callers must not make this check.

            var pos = serializer.BeginContent("WrapFormat");
            if (Values.Style is not null)
                serializer.WriteSimpleAttribute("Style", Style);
            //if (Values.DistanceTop is not null)
            if (!Values.DistanceTop.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("DistanceTop", DistanceTop);
            //if (Values.DistanceLeft is not null)
            if (!Values.DistanceLeft.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("DistanceLeft", DistanceLeft);
            //if (Values.DistanceRight is not null)
            if (!Values.DistanceRight.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("DistanceRight", DistanceRight);
            //if (Values.DistanceBottom is not null)
            if (!Values.DistanceBottom.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("DistanceBottom", DistanceBottom);
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(WrapFormat));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public WrapFormatValues Values => (WrapFormatValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class WrapFormatValues : Values
        {
            internal WrapFormatValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public WrapStyle? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceTop { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceBottom { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceLeft { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? DistanceRight { get; set; } //= Unit.NullValue;
        }
    }
}