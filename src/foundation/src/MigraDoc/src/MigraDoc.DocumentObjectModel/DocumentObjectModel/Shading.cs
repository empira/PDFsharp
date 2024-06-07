// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Shading represents the background color of a document object.
    /// </summary>
    public sealed class Shading : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the Shading class.
        /// </summary>
        public Shading()
        {
            BaseValues = new ShadingValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Shading class with the specified parent.
        /// </summary>
        internal Shading(DocumentObject parent) : base(parent)
        {
            BaseValues = new ShadingValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Shading Clone()
            => (Shading)DeepCopy();

        /// <summary>
        /// Clears the Shading object. Additionally, 'shading = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public void Clear()
            => IsCleared = true;

        /// <summary>
        /// Gets or sets a value indicating whether the shading is visible.
        /// </summary>
        public bool Visible
        {
            get => Values.Visible ?? false;
            set => Values.Visible = value;
        }

        /// <summary>
        /// Gets or sets the shading color.
        /// </summary>
        public Color Color
        {
            get => Values.Color ?? Color.Empty;
            set => Values.Color = value;
        }

        /// <summary>
        /// Gets the information if the shading is marked as cleared. Additionally, 'shading = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public bool IsCleared { get; private set; }

        /// <summary>
        /// Converts Shading into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (IsCleared)
                serializer.WriteLine("Shading = null");

            int pos = serializer.BeginContent("Shading");

            //if (!_visible.IsNull)
            if (Values.Visible is not null)
                serializer.WriteSimpleAttribute("Visible", Visible);

            //if (Values.Color is not null && !Values.Color.Value.IsNull)
            if (!Values.Color.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Color", Color);

            serializer.EndContent(pos);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Shading));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ShadingValues Values => (ShadingValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ShadingValues : Values
        {
            internal ShadingValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Visible { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? Color
            {
                get => _color;
                set => _color = DocumentObjectModel.Color.MakeNullIfEmpty(value);
            }
            Color? _color;
        }
    }
}
