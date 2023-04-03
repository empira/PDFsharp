// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the title of an axis.
    /// </summary>
    public class AxisTitle : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the AxisTitle class.
        /// </summary>
        public AxisTitle()
        {
            BaseValues = new AxisTitleValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the AxisTitle class with the specified parent.
        /// </summary>
        internal AxisTitle(DocumentObject parent) : base(parent)
        {
            BaseValues = new AxisTitleValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new AxisTitle Clone() 
            => (AxisTitle)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var axisTitle = (AxisTitle)base.DeepCopy();
            if (axisTitle.Values.Font is not null)
            {
                axisTitle.Values.Font = axisTitle.Values.Font.Clone();
                axisTitle.Values.Font.Parent = axisTitle;
            }
            return axisTitle;
        }

        /// <summary>
        /// Gets or sets the style name of the axis.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the caption of the title.
        /// </summary>
        public string Caption
        {
            get => Values.Caption ?? "";
            set => Values.Caption = value;
        }

        /// <summary>
        /// Gets the font object of the title.
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
        /// Gets or sets the orientation of the caption.
        /// </summary>
        public Unit Orientation
        {
            get => Values.Orientation ?? Unit.Empty;
            set => Values.Orientation = value;
        }

        /// <summary>
        /// Gets or sets the alignment of the caption.
        /// </summary>
        public HorizontalAlignment Alignment
        {
            get => Values.Alignment ?? HorizontalAlignment.Left;
            set => Values.Alignment = value;
        }

        /// <summary>
        /// Gets or sets the alignment of the caption.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get => Values.VerticalAlignment ?? VerticalAlignment.Top;
            set => Values.VerticalAlignment = value;
        }

        /// <summary>
        /// Converts AxisTitle into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            /*var pos =*/ serializer.BeginContent("Title");

            if (Values.Style is not null)
                serializer.WriteSimpleAttribute("Style", Style);

            Values.Font?.Serialize(serializer);

            //if (Values.Orientation is not null)
            if (!Values.Orientation.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Orientation", Orientation);

            if (Values.Alignment is not null)
                serializer.WriteSimpleAttribute("Alignment", Alignment);

            if (Values.VerticalAlignment is not null /*&& !Values.VerticalAlignment.IsNull*/) // BUG??? IsNull?
                serializer.WriteSimpleAttribute("VerticalAlignment", VerticalAlignment);

            if (Values.Caption is not null)
                serializer.WriteSimpleAttribute("Caption", Caption);

            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(AxisTitle));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new AxisTitleValues Values => (AxisTitleValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class AxisTitleValues : ChartObjectValues
        {
            internal AxisTitleValues(DocumentObject owner) : base(owner)
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
            public string? Caption { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Font? Font { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Orientation { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HorizontalAlignment? Alignment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public VerticalAlignment? VerticalAlignment { get; set; }
        }
    }
}
