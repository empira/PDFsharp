// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// A TextWithShapeFormat object
    /// Defines the background filling of the shape.
    /// </summary>
    public class TextWithShapeFormat : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the FillFormat class.
        /// </summary>
        public TextWithShapeFormat() { }

        /// <summary>
        /// Initializes a new instance of the FillFormat class with the specified parent.
        /// </summary>
        internal TextWithShapeFormat(DocumentObject parent) : base(parent) { }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new TextWithShapeFormat Clone()
            => (TextWithShapeFormat)DeepCopy();

        /// <summary>
        /// Gets or sets a value indicating whether the text should be in front of the shape.
        /// </summary>
        public bool? TextInFront { get; set; }

        /// <summary>
        /// Converts FillFormat into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int pos = serializer.BeginContent("TextWithShapeFormat");
            if (this.TextInFront.HasValue)
                serializer.WriteSimpleAttribute("TextInFront", this.TextInFront.Value);

            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(TextWithShapeFormat));
    }
}
