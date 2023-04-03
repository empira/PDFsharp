// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Base class for all classes that hold the values of a DocumentObject.
    /// </summary>
    public class Values : ICloneable
    {
        internal Values(DocumentObject owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public object Clone()
        {
            var clone = (Values)MemberwiseClone();
            clone.Owner = null!;
            return clone;
        }

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        [DV(ReadOnly = true, RefOnly = true)]
        public DocumentObject? Owner { get; internal set; }

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        [DV(ReadOnly = true, RefOnly = true)]
        public object? Tag => Owner?.Tag;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        [DV(ReadOnly = true, RefOnly = true)]
        public Document? Document => Owner?.Document;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        [DV(ReadOnly = true, RefOnly = true)]
        public Section? Section => Owner?.Section;
    }
}
