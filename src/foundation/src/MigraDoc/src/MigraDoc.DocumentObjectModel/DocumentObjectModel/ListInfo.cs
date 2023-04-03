// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A ListInfo is the representation of a series of paragraphs as a list.
    /// </summary>
    public class ListInfo : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the ListInfo class.
        /// </summary>
        public ListInfo()
        {
            BaseValues = new ListInfoValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the ListInfo class with the specified parent.
        /// </summary>
        internal ListInfo(DocumentObject parent) : base(parent)
        {
            BaseValues = new ListInfoValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new ListInfo Clone() 
            => (ListInfo)DeepCopy();

        /// <summary>
        /// Gets or sets the type of the list.
        /// </summary>
        public ListType ListType
        {
            get => Values.ListType ?? ListType.BulletList1;
            set => Values.ListType = value;
        }

        /// <summary>
        /// Gets or sets the left indent of the list symbol.
        /// </summary>
        public Unit NumberPosition
        {
            get => Values.NumberPosition ?? Unit.Empty;
            set => Values.NumberPosition = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// the previous list numbering should be continued.
        /// </summary>
        public bool ContinuePreviousList
        {
            get => Values.ContinuePreviousList ?? false;
            set => Values.ContinuePreviousList = value;
        }

        /// <summary>
        /// Converts ListInfo into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (Values.ListType is not null)
                serializer.WriteSimpleAttribute("ListInfo.ListType", ListType);
            //if (Values.NumberPosition is not null)
            if (!Values.NumberPosition.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("ListInfo.NumberPosition", NumberPosition);
            if (Values.ContinuePreviousList is not null)
                serializer.WriteSimpleAttribute("ListInfo.ContinuePreviousList", ContinuePreviousList);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(ListInfo));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ListInfoValues Values => (ListInfoValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ListInfoValues : Values
        {
            internal ListInfoValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ListType? ListType { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? NumberPosition { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? ContinuePreviousList { get; set; }
        }
    }
}
