// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents one border in a borders collection. The type determines its position in a cell,
    /// paragraph etc.
    /// </summary>
    public class Border : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the Border class.
        /// </summary>
        public Border()
        {
            BaseValues = new BorderValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Border class with the specified parent.
        /// </summary>
        internal Border(DocumentObject parent) : base(parent)
        {
            BaseValues = new BorderValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Border Clone()
            => (Border)DeepCopy();

        /// <summary>
        /// Clears the Border object. Additionally, 'Border = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public void Clear()
            => Values.BorderCleared = true;

        /// <summary>
        /// Gets or sets a value indicating whether the border visible is.
        /// </summary>
        public bool Visible
        {
            get => Values.Visible ?? false;
            set => Values.Visible = value;
        }

        /// <summary>
        /// Gets or sets the line style of the border.
        /// </summary>
        public BorderStyle Style
        {
            get => Values.Style ?? BorderStyle.None;
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the line width of the border.
        /// </summary>
        public Unit Width
        {
            get => Values.Width ?? Unit.Empty;
            set => Values.Width = value;
        }

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        public Color Color
        {
            get => Values.Color ?? Color.Empty;
            set => Values.Color = value;
        }

        /// <summary>
        /// Gets the name of this border ("top", "bottom"....).
        /// </summary>
        public string Name => (Parent as Borders)?.GetMyName(this) ?? "";

        /// <summary>
        /// Gets the information if the border is marked as cleared. Additionally, 'xxx = null'
        /// is written to the DDL stream when serialized.
        /// </summary>
        public bool BorderCleared
        {
            get => Values.BorderCleared ?? false;
            set => Values.BorderCleared = value;
        }

        /// <summary>
        /// Converts Border into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
            => throw new Exception("A Border cannot be serialized stand alone.");

        /// <summary>
        /// Converts Border into DDL.
        /// </summary>
        internal void Serialize(Serializer serializer, string name, Border? refBorder)
        {
            //if (_fClear ?? false)
            if (BorderCleared)
                serializer.WriteLine(name + " = null");

            int pos = serializer.BeginContent(name);

            if (Values.Visible is not null && (refBorder == null || (Visible != refBorder.Visible)))
                serializer.WriteSimpleAttribute("Visible", Visible);

            if (Values.Style is not null && (refBorder == null || (Style != refBorder.Style)))
                serializer.WriteSimpleAttribute("Style", Style);

            //if (Values.Width is not null && (refBorder == null || (Width != refBorder.Width)))
            if (!Values.Width.IsValueNullOrEmpty() && (refBorder == null || (Width != refBorder.Width)))
                serializer.WriteSimpleAttribute("Width", Width);

            //if (Values.Color is not null && (refBorder == null || (Color != refBorder.Color)))
            if (!Values.Color.IsValueNullOrEmpty() && (refBorder == null || (Color != refBorder.Color)))
                serializer.WriteSimpleAttribute("Color", Color);
            serializer.EndContent(pos);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Border));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public BorderValues Values => (BorderValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class BorderValues : Values
        {
            internal BorderValues(DocumentObject owner) : base(owner)
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
            public BorderStyle? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Width { get; set; }

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

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? BorderCleared { get; set; }
        }
    }
}
