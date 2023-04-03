// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

//using static MigraDoc.DocumentObjectModel.Shapes.Charts.Chart;

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// Base class for all chart classes.
    /// </summary>
    public abstract class ChartObject : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the ChartObject class.
        /// </summary>
        protected ChartObject()
        { }

        /// <summary>
        /// Initializes a new instance of the ChartObject class with the specified parent.
        /// </summary>
        internal ChartObject(DocumentObject parent) : base(parent)
        { }

        /// <summary>
        /// Converts ChartObject into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            // Nothing to do.
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(ChartObject));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ChartObjectValues Values => (ChartObjectValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ChartObjectValues : Values
        {
            internal ChartObjectValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
