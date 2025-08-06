// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents a tab inside a paragraph.
    /// </summary>
    public class TabStop : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the TabStop class.
        /// </summary>
        public TabStop()
        {
            BaseValues = new TabStopValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the TabStop class with the specified parent.
        /// </summary>
        internal TabStop(DocumentObject parent) : base(parent)
        {
            BaseValues = new TabStopValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the TabStop class with the specified position.
        /// </summary>
        public TabStop(Unit position)
            : this()
        {
            Values.Position = position;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new TabStop Clone()
            => (TabStop)DeepCopy();

        /// <summary>
        /// Gets the tab stop position.
        /// </summary>
        public Unit Position
        {
            get => Values.Position ?? Unit.Empty;
            set => Values.Position = value;
        }
        // useful enhancement: 'Position = Center' and 'Position = Right'

        /// <summary>
        /// Gets or sets the alignment of the tabstop.
        /// </summary>
        public TabAlignment Alignment
        {
            get => Values.Alignment ?? TabAlignment.Left;
            set => Values.Alignment = value;
        }

        /// <summary>
        /// Gets or sets the character which is used as a leader for the tabstop.
        /// </summary>
        public TabLeader Leader
        {
            get => Values.Leader ?? TabLeader.Spaces;
            set => Values.Leader = value;
        }

        /// <summary>
        /// Generates a '+=' in DDL if it is true, otherwise '-='.
        /// The value cannot be set. A cleared tab can only be created by removing it
        /// from the tab stops collection.
        /// </summary>
        public bool AddTab { get; internal set; } = true;

        /// <summary>
        /// Converts TabStop into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (AddTab)
            {
                serializer.WriteLine("TabStops +=");
                serializer.BeginContent();
                serializer.WriteSimpleAttribute("Position", Position);
                if (Values.Alignment is not null)
                    serializer.WriteSimpleAttribute("Alignment", Alignment);
                if (Values.Leader is not null)
                    serializer.WriteSimpleAttribute("Leader", Leader);
                serializer.EndContent();
            }
            else
                serializer.WriteLine($"TabStops -= \"{Position}\"");
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(TabStop));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public TabStopValues Values => (TabStopValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class TabStopValues : Values
        {
            internal TabStopValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Position { get; set; } //= Unit.Empty;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TabAlignment? Alignment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TabLeader? Leader { get; set; }
        }
    }
}
