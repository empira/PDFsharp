// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents a legend of a chart.
    /// </summary>
    public class Legend : ChartObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Legend class.
        /// </summary>
        public Legend()
        {
            BaseValues = new LegendValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Legend class with the specified parent.
        /// </summary>
        internal Legend(DocumentObject parent) : base(parent)
        {
            BaseValues = new LegendValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Legend Clone()
            => (Legend)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var legend = (Legend)base.DeepCopy();
            if (legend.Values.Format is not null)
            {
                legend.Values.Format = legend.Values.Format.Clone();
                legend.Values.Format.Parent = legend;
            }
            if (legend.Values.LineFormat is not null)
            {
                legend.Values.LineFormat = legend.Values.LineFormat.Clone();
                legend.Values.LineFormat.Parent = legend;
            }
            return legend;
        }

        /// <summary>
        /// Gets or sets the style name of the legend’s text.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets the paragraph format of the legend’s text.
        /// </summary>
        public ParagraphFormat Format
        {
            get => Values.Format ??= new ParagraphFormat(this);
            set
            {
                SetParentOf(value);
                Values.Format = value;
            }
        }

        /// <summary>
        /// Gets the line format of the legend’s border.
        /// </summary>
        public LineFormat LineFormat
        {
            get => Values.LineFormat ??= new LineFormat(this);
            set
            {
                SetParentOf(value);
                Values.LineFormat = value;
            }
        }

        /// <summary>
        /// Converts Legend into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\legend");
            var pos = serializer.BeginAttributes();

            if (Values.Style is not null)
                serializer.WriteSimpleAttribute("Style", Style);

            Values.Format?.Serialize(serializer, "Format", null);

            Values.LineFormat?.Serialize(serializer);

            serializer.EndAttributes(pos);
        }

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public override bool IsNull()
        {
            // legend objects are never null, i.e. the presence of this object is meaningful.
            return false;
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Legend));

        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitLegend(this);
        }

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new LegendValues Values => (LegendValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class LegendValues : ChartObjectValues
        {
            internal LegendValues(DocumentObject owner) : base(owner)
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
            public ParagraphFormat? Format { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineFormat? LineFormat { get; set; }
        }
    }
}
