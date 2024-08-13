// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Represents the gridlines on the axes.
    /// </summary>
    public class Gridlines : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Gridlines class.
        /// </summary>
        public Gridlines()
        {
            BaseValues = new GridlinesValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Gridlines class with the specified parent.
        /// </summary>
        internal Gridlines(DocumentObject parent) : base(parent)
        {
            BaseValues = new GridlinesValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Gridlines Clone()
            => (Gridlines)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var gridlines = (Gridlines)base.DeepCopy();
            if (gridlines.Values.LineFormat is not null)
            {
                gridlines.Values.LineFormat = gridlines.Values.LineFormat.Clone();
                gridlines.Values.LineFormat.Parent = gridlines;
            }
            return gridlines;
        }

        /// <summary>
        /// Gets the line format of the grid.
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
        /// Converts Gridlines into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var axisObject = Parent as Axis ?? throw new InvalidOperationException("Parent is not of type Axis.");

            var pos = serializer.BeginContent(axisObject.CheckGridlines(this)); // H/ACK // BUG: What if Parent is not Axis?

            Values.LineFormat?.Serialize(serializer);

            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Gridlines));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new GridlinesValues Values => (GridlinesValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class GridlinesValues : ChartObjectValues
        {
            internal GridlinesValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineFormat? LineFormat { get; set; }
        }
    }
}
