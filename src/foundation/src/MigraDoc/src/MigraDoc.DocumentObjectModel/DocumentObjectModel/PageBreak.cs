// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A PageBreak is used to put following elements on a new page.
    /// </summary>
    public class PageBreak : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the PageBreak class.
        /// </summary>
        public PageBreak()
        {
            BaseValues = new PageBreakValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the PageBreak class with the specified parent.
        /// </summary>
        internal PageBreak(DocumentObject parent) : base(parent)
        {
            BaseValues = new PageBreakValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PageBreak Clone()
            => (PageBreak)DeepCopy();

        /// <summary>
        /// Converts PageBreak into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer) 
            => serializer.WriteLine("\\pagebreak");

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(PageBreak));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public PageBreakValues Values => (PageBreakValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class PageBreakValues : Values
        {
            internal PageBreakValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
