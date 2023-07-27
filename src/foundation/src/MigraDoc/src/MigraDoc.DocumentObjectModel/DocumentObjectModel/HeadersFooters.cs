// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the collection of HeaderFooter objects.
    /// </summary>
    public class HeadersFooters : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the HeadersFooters class.
        /// </summary>
        public HeadersFooters()
        {
            BaseValues = new HeadersFootersValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the HeadersFooters class with the specified parent.
        /// </summary>
        public HeadersFooters(DocumentObject parent) : base(parent)
        {
            BaseValues = new HeadersFootersValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new HeadersFooters Clone()
            => (HeadersFooters)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            HeadersFooters headersFooters = (HeadersFooters)base.DeepCopy();
            if (headersFooters.Values.EvenPage is not null)
            {
                headersFooters.Values.EvenPage = headersFooters.Values.EvenPage.Clone();
                headersFooters.Values.EvenPage.Parent = headersFooters;
            }
            if (headersFooters.Values.FirstPage != null)
            {
                headersFooters.Values.FirstPage = headersFooters.Values.FirstPage.Clone();
                headersFooters.Values.FirstPage.Parent = headersFooters;
            }
            if (headersFooters.Values.Primary != null)
            {
                headersFooters.Values.Primary = headersFooters.Values.Primary.Clone();
                headersFooters.Values.Primary.Parent = headersFooters;
            }
            return headersFooters;
        }

        /// <summary>
        /// Returns true if this collection contains headers, false otherwise.
        /// </summary>
        public bool IsHeader
        {
            get
            {
                // Return false if it has no parent.
                var sec = Parent as Section;
                return sec?.Values.Headers == this;
            }
        }

        /// <summary>
        /// Returns true if this collection contains footers, false otherwise.
        /// </summary>
        public bool IsFooter // => !IsHeader; -> No, would be true if no parent.
        {
            get
            {
                // Return false if it has no parent.
                var sec = Parent as Section;
                return sec?.Values.Footers == this;
            }
        }

        /// <summary>
        /// Determines whether a particular header or footer exists.
        /// </summary>
        [Obsolete("Uses IsNull and should be avoided.")] // BUG???
        public bool HasHeaderFooter(HeaderFooterIndex index)
        {
            return !IsNull(index.ToString());
        }

        /// <summary>
        /// Determines whether a particular header or footer exists.
        /// </summary>
        public bool HasHeaderFooter(HeaderFooter? item)
            => item is not null && !item.IsNull();

        /// <summary>
        /// Gets or sets the even page HeaderFooter of the HeadersFooters object.
        /// </summary>
        public HeaderFooter EvenPage
        {
            get => Values.EvenPage ??= new(this);
            set
            {
                SetParent(value);
                Values.EvenPage = value;
            }
        }

        /// <summary>
        /// Gets or sets the first page HeaderFooter of the HeadersFooters object.
        /// </summary>
        public HeaderFooter FirstPage
        {
            get => Values.FirstPage ??= new(this);
            set
            {
                SetParent(value);
                Values.FirstPage = value;
            }
        }

        /// <summary>
        /// Gets or sets the primary HeaderFooter of the HeadersFooters object.
        /// </summary>
        public HeaderFooter Primary
        {
            get => Values.Primary ??= new HeaderFooter(this);
            set
            {
                SetParent(value);
                Values.Primary = value;
            }
        }

        /// <summary>
        /// Converts HeadersFooters into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            bool hasPrimary = HasHeaderFooter(Values.Primary);
            bool hasEvenPage = HasHeaderFooter(Values.EvenPage);
            bool hasFirstPage = HasHeaderFooter(Values.FirstPage);

            // \primary...
            if (hasPrimary)
                Primary.Serialize(serializer, "primary");

            // \even... 
            if (hasEvenPage)
                EvenPage.Serialize(serializer, "evenpage");

            // \firstpage...
            if (hasFirstPage)
                FirstPage.Serialize(serializer, "firstpage");
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitHeadersFooters(this);

            if (visitChildren)
            {
                if (HasHeaderFooter(Values.Primary))
                    ((IVisitable)Values.Primary!).AcceptVisitor(visitor, true); // "!" is OK if HasHeaderFooter is true.
                if (HasHeaderFooter(Values.EvenPage))
                    ((IVisitable)Values.EvenPage!).AcceptVisitor(visitor, true);
                if (HasHeaderFooter(Values.FirstPage))
                    ((IVisitable)Values.FirstPage!).AcceptVisitor(visitor, true);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(HeadersFooters));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public HeadersFootersValues Values => (HeadersFootersValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class HeadersFootersValues : Values
        {
            internal HeadersFootersValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HeaderFooter? EvenPage { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HeaderFooter? FirstPage { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HeaderFooter? Primary { get; set; }
        }
    }
}
