// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the collection of document sections.
    /// </summary>
    public class Sections : DocumentObjectCollection, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Sections class.
        /// </summary>
        public Sections()
        {
            BaseValues = new SectionsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Sections class with the specified parent.
        /// </summary>
        internal Sections(DocumentObject parent) : base(parent)
        {
            BaseValues = new SectionsValues(this);
        }

        /// <summary>
        /// Gets a section by its index. First section has index 0.
        /// </summary>
        public new Section this[int index] => (base[index] as Section)!; // HACK // BUG: May return null TODO: Section? Exception?

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Sections Clone()
            => (Sections)DeepCopy();

        /// <summary>
        /// Adds a new section.
        /// </summary>
        public Section AddSection()
        {
            var section = new Section();
            Add(section);
            return section;
        }

        /// <summary>
        /// Converts Sections into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int count = Count;
            for (int index = 0; index < count; ++index)
            {
                var section = this[index];
                section.Serialize(serializer);
            }
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitSections(this);
            foreach (var section in this.Cast<Section>()) // BUG: Uses DocumentObjectCollection.GetEnumerator() returning DocumentObject?. Override with new GetEnumerator() returning Section or Section? like this[int index]?
                ((IVisitable)section).AcceptVisitor(visitor, visitChildren);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Sections));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public SectionsValues Values => (SectionsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class SectionsValues : Values
        {
            internal SectionsValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
