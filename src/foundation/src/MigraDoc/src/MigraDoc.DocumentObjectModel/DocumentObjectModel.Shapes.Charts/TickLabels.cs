// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the format of the label of each value on the axis.
    /// </summary>
    public class TickLabels : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the TickLabels class.
        /// </summary>
        public TickLabels()
        {
            BaseValues = new TickLabelsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the TickLabels class with the specified parent.
        /// </summary>
        internal TickLabels(DocumentObject parent) : base(parent)
        {
            BaseValues = new TickLabelsValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new TickLabels Clone() 
            => (TickLabels)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            TickLabels tickLabels = (TickLabels)base.DeepCopy();
            if (tickLabels.Values.Font != null)
            {
                tickLabels.Values.Font = tickLabels.Values.Font.Clone();
                tickLabels.Values.Font.Parent = tickLabels;
            }
            return tickLabels;
        }

        /// <summary>
        /// Gets or sets the style name of the label.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the label’s number format.
        /// </summary>
        public string Format
        {
            get => Values.Format ?? "";
            set => Values.Format = value;
        }

        /// <summary>
        /// Gets the font of the label.
        /// </summary>
        public Font Font
        {
            get => Values.Font ??= new(this);
            set
            {
                SetParent(value);
                Values.Font = value;
            }
        }

        /// <summary>
        /// Converts TickLabels into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int pos = serializer.BeginContent("TickLabels");

            if (Values.Style is not null)
                serializer.WriteSimpleAttribute("Style", Style);

            Values.Font?.Serialize(serializer);

            if (Values.Format is not null)
                serializer.WriteSimpleAttribute("Format", Format);

            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(TickLabels));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new TickLabelsValues Values => (TickLabelsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class TickLabelsValues : ChartObjectValues
        {
            internal TickLabelsValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Style { get; set; }

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
        }
    }
}
