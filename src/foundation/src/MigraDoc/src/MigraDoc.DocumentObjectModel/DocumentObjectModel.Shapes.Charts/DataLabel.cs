// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents a DataLabel of a Series
    /// </summary>
    public class DataLabel : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the DataLabel class.
        /// </summary>
        public DataLabel()
        {
            BaseValues = new DataLabelValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the DataLabel class with the specified parent.
        /// </summary>
        internal DataLabel(DocumentObject parent) : base(parent)
        {
            BaseValues = new DataLabelValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new DataLabel Clone()
            => (DataLabel)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            DataLabel dataLabel = (DataLabel)base.DeepCopy();
            if (dataLabel.Values.Font != null)
            {
                dataLabel.Values.Font = dataLabel.Values.Font.Clone();
                dataLabel.Values.Font.Parent = dataLabel;
            }
            return dataLabel;
        }

        /// <summary>
        /// Gets or sets a numeric format string for the DataLabel.
        /// </summary>
        public string Format
        {
            get => Values.Format ?? "";
            set => Values.Format = value;
        }

        /// <summary>
        /// Gets the Font for the DataLabel.
        /// </summary>
        public Font Font
        {
            get => Values.Font ??= new Font(this);
            set
            {
                SetParent(value);
                Values.Font = value;
            }
        }

        /// <summary>
        /// Gets or sets the Style for the DataLabel.
        /// Only the Font-associated part of the Style's ParagraphFormat is used.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the position of the DataLabel.
        /// </summary>
        public DataLabelPosition Position
        {
            get => Values.Position ?? DataLabelPosition.Center;
            set => Values.Position = value;
        }

        /// <summary>
        /// Gets or sets the type of the DataLabel.
        /// </summary>
        public DataLabelType Type
        {
            get => Values.Type ?? DataLabelType.None;
            set => Values.Type = value;
        }

        /// <summary>
        /// Converts DataLabel into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int pos = serializer.BeginContent("DataLabel");

            if (!Style.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Style", Style);
            if (!Format.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Format", Format);
            if (Values.Position is not null)
                serializer.WriteSimpleAttribute("Position", Position);
            if (Values.Type is not null)
                serializer.WriteSimpleAttribute("Type", Type);
            if (Values.Font is not null)
                Values.Font.Serialize(serializer);

            serializer.EndContent(pos);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(DataLabel));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public DataLabelValues Values => (DataLabelValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class DataLabelValues : Values
        {
            internal DataLabelValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Format { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Font? Font { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DataLabelPosition? Position { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DataLabelType? Type { get; set; }
        }
    }
}
