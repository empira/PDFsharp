// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Represents a barcode in the document or paragraph. !!!Still under Construction!!!
    /// </summary>
    public class Barcode : Shape
    {
        /// <summary>
        /// Initializes a new instance of the Barcode class.
        /// </summary>
        public Barcode()
        {
            BaseValues = new BarcodeValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Barcode class with the specified parent.
        /// </summary>
        internal Barcode(DocumentObject parent) : base(parent)
        {
            BaseValues = new BarcodeValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Barcode Clone()
            => (Barcode)DeepCopy();

        /// <summary>
        /// Gets or sets the text orientation for the barcode content.
        /// </summary>
        public TextOrientation Orientation
        {
            get => Values.Orientation ?? TextOrientation.Horizontal;
            set => Values.Orientation = value;
        }

        /// <summary>
        /// Gets or sets the type of the barcode.
        /// </summary>
        public BarcodeType Type
        {
            get => Values.Type ?? BarcodeType.Barcode25i;
            set => Values.Type = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether bars shall appear beside the barcode.
        /// </summary>
        public bool BearerBars
        {
            get => Values.BearerBars ?? false;
            set => Values.BearerBars = value;
        }

        /// <summary>
        /// Gets or sets the text of the barcode.
        /// </summary>
        public bool Text
        {
            get => Values.Text ?? false;
            set => Values.Text = value;
        }

        /// <summary>
        /// Gets or sets the code represented by the barcode.
        /// </summary>
        public string Code
        {
            get => Values.Code ?? "";
            set => Values.Code = value;
        }

        /// <summary>
        /// Ratio between narrow and wide lines.
        /// </summary>
        public double LineRatio
        {
            get => Values.LineRatio ?? 0.0;
            set => Values.LineRatio = value;
        }

        /// <summary>
        /// Height of lines.
        /// </summary>
        public double LineHeight
        {
            get => Values.LineHeight ?? 0.0;
            set => Values.LineHeight = value;
        }

        /// <summary>
        /// Width of a narrow line.
        /// </summary>
        public double NarrowLineWidth
        {
            get => Values.NarrowLineWidth ?? 0.0;
            set => Values.NarrowLineWidth = value;
        }

        /// <summary>
        /// Converts Barcode into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (String.IsNullOrEmpty(Values.Code))
                throw new InvalidOperationException(DomSR.MissingObligatoryProperty("Name", "BookmarkField"));

            serializer.WriteLine("\\barcode(\"" + Code + "\")");

            int pos = serializer.BeginAttributes();

            base.Serialize(serializer);

            if (Values.Orientation is not null)
                serializer.WriteSimpleAttribute("Orientation", Orientation);
            if (Values.BearerBars is not null)
                serializer.WriteSimpleAttribute("BearerBars", BearerBars);
            if (Values.Text is not null)
                serializer.WriteSimpleAttribute("Text", Text);
            if (Values.Type is not null)
                serializer.WriteSimpleAttribute("Type", Type);
            if (Values.LineRatio is not null)
                serializer.WriteSimpleAttribute("LineRatio", LineRatio);
            if (Values.LineHeight is not null)
                serializer.WriteSimpleAttribute("LineHeight", LineHeight);
            if (Values.NarrowLineWidth is not null)
                serializer.WriteSimpleAttribute("NarrowLineWidth", NarrowLineWidth);

            serializer.EndAttributes(pos);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Barcode));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new BarcodeValues Values => (BarcodeValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class BarcodeValues : ShapeValues
        {
            internal BarcodeValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextOrientation? Orientation { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public BarcodeType? Type { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? BearerBars { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Text { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Code { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? LineRatio { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? LineHeight { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? NarrowLineWidth { get; set; }
        }
    }
}
