// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// A PictureFormat object.
    /// Used to set more detailed image attributes.
    /// </summary>
    public class PictureFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the PictureFormat class.
        /// </summary>
        public PictureFormat()
        {
            BaseValues = new PictureFormatValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the PictureFormat class with the specified parent.
        /// </summary>
        internal PictureFormat(DocumentObject parent) : base(parent)
        {
            BaseValues = new PictureFormatValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PictureFormat Clone() 
            => (PictureFormat)DeepCopy();

        /// <summary>
        /// Gets or sets the part cropped from the left of the image.
        /// </summary>
        public Unit CropLeft
        {
            get => Values.CropLeft ?? Unit.Empty;
            set => Values.CropLeft = value;
        }

        /// <summary>
        /// Gets or sets the part cropped from the right of the image.
        /// </summary>
        public Unit CropRight
        {
            get => Values.CropRight ?? Unit.Empty;
            set => Values.CropRight = value;
        }

        /// <summary>
        /// Gets or sets the part cropped from the top of the image.
        /// </summary>
        public Unit CropTop
        {
            get => Values.CropTop ?? Unit.Empty;
            set => Values.CropTop = value;
        }

        /// <summary>
        /// Gets or sets the part cropped from the bottom of the image.
        /// </summary>
        public Unit CropBottom
        {
            get => Values.CropBottom ?? Unit.Empty;
            set => Values.CropBottom = value;
        }

        /// <summary>
        /// Converts PictureFormat into DDL
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.BeginContent("PictureFormat");
            //if (Values.CropLeft is not null)
            if (!Values.CropLeft.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("CropLeft", CropLeft);
            //if (Values.CropRight is not null)
            if (!Values.CropRight.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("CropRight", CropRight);
            //if (Values.CropTop is not null)
            if (!Values.CropTop.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("CropTop", CropTop);
            //if (Values.CropBottom is not null)
            if (!Values.CropBottom.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("CropBottom", CropBottom);
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(PictureFormat));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public PictureFormatValues Values => (PictureFormatValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class PictureFormatValues : Values
        {
            internal PictureFormatValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? CropLeft { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? CropRight { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? CropTop { get; set; } //= Unit.NullValue;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? CropBottom { get; set; } //= Unit.NullValue;
        }
    }
}
