// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel.Shapes.Charts
{
    /// <summary>
    /// An area object in the chart which contain text or legend.
    /// </summary>
    public class TextArea : ChartObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the TextArea class.
        /// </summary>
        public TextArea()
        {
            BaseValues = new TextAreaValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the TextArea class with the specified parent.
        /// </summary>
        internal TextArea(DocumentObject parent) : base(parent)
        {
            BaseValues = new TextAreaValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new TextArea Clone()
            => (TextArea)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            TextArea textArea = (TextArea)base.DeepCopy();
            if (textArea.Values.Format != null)
            {
                textArea.Values.Format = textArea.Values.Format.Clone();
                textArea.Values.Format.Parent = textArea;
            }
            if (textArea.Values.LineFormat != null)
            {
                textArea.Values.LineFormat = textArea.Values.LineFormat.Clone();
                textArea.Values.LineFormat.Parent = textArea;
            }
            if (textArea.Values.FillFormat != null)
            {
                textArea.Values.FillFormat = textArea.Values.FillFormat.Clone();
                textArea.Values.FillFormat.Parent = textArea;
            }
            if (textArea.Values.Elements != null)
            {
                textArea.Values.Elements = textArea.Values.Elements.Clone();
                textArea.Values.Elements.Parent = textArea;
            }
            return textArea;
        }

        /// <summary>
        /// Adds a new paragraph to the text area.
        /// </summary>
        public Paragraph AddParagraph()
        {
            return Elements.AddParagraph();
        }

        /// <summary>
        /// Adds a new paragraph with the specified text to the text area.
        /// </summary>
        public Paragraph AddParagraph(string paragraphText)
        {
            return Elements.AddParagraph(paragraphText);
        }

        /// <summary>
        /// Adds a new table to the text area.
        /// </summary>
        public Table AddTable()
        {
            return Elements.AddTable();
        }

        /// <summary>
        /// Adds a new Image to the text area.
        /// </summary>
        public Image AddImage(string fileName)
        {
            return Elements.AddImage(fileName);
        }

        /// <summary>
        /// Adds a new legend to the text area.
        /// </summary>
        public Legend AddLegend()
        {
            return Elements.AddLegend();
        }

        /// <summary>
        /// Adds a new paragraph to the text area.
        /// </summary>
        public void Add(Paragraph paragraph)
        {
            Elements.Add(paragraph);
        }

        /// <summary>
        /// Adds a new table to the text area.
        /// </summary>
        public void Add(Table table)
        {
            Elements.Add(table);
        }

        /// <summary>
        /// Adds a new image to the text area.
        /// </summary>
        public void Add(Image image)
        {
            Elements.Add(image);
        }

        /// <summary>
        /// Adds a new legend to the text area.
        /// </summary>
        public void Add(Legend legend)
        {
            Elements.Add(legend);
        }

        /// <summary>
        /// Gets or sets the height of the area.
        /// </summary>
        public Unit Height
        {
            get => Values.Height ?? Unit.Empty;
            set => Values.Height = value;
        }

        /// <summary>
        /// Gets or sets the width of the area.
        /// </summary>
        public Unit Width
        {
            get => Values.Width ?? Unit.Empty;
            set => Values.Width = value;
        }

        /// <summary>
        /// Gets or sets the default style name of the area.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the default paragraph format of the area.
        /// </summary>
        public ParagraphFormat Format
        {
            get => Values.Format ??= new(this);
            set
            {
                SetParent(value);
                Values.Format = value;
            }
        }

        /// <summary>
        /// Gets the line format of the area's border.
        /// </summary>
        public LineFormat LineFormat
        {
            get => Values.LineFormat ??= new(this);
            set
            {
                SetParent(value);
                Values.LineFormat = value;
            }
        }

        /// <summary>
        /// Gets the background filling of the area.
        /// </summary>
        public FillFormat FillFormat
        {
            get => Values.FillFormat ??= new(this);
            set
            {
                SetParent(value);
                Values.FillFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the left padding of the area.
        /// </summary>
        public Unit LeftPadding
        {
            get => Values.LeftPadding ?? Unit.Empty;
            set => Values.LeftPadding = value;
        }

        /// <summary>
        /// Gets or sets the right padding of the area.
        /// </summary>
        public Unit RightPadding
        {
            get => Values.RightPadding ?? Unit.Empty;
            set => Values.RightPadding = value;
        }

        /// <summary>
        /// Gets or sets the top padding of the area.
        /// </summary>
        public Unit TopPadding
        {
            get => Values.TopPadding ?? Unit.Empty;
            set => Values.TopPadding = value;
        }

        /// <summary>
        /// Gets or sets the bottom padding of the area.
        /// </summary>
        public Unit BottomPadding
        {
            get => Values.BottomPadding ?? Unit.Empty;
            set => Values.BottomPadding = value;
        }

        /// <summary>
        /// Gets or sets the Vertical alignment of the area.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get => Values.VerticalAlignment ?? VerticalAlignment.Top;
            set => Values.VerticalAlignment = value;
        }

        /// <summary>
        /// Gets the document objects that creates the text area.
        /// </summary>
        public DocumentElements Elements
        {
            get => Values.Elements ??= new(this);
            set
            {
                SetParent(value);
                Values.Elements = value;
            }
        }

        /// <summary>
        /// Converts TextArea into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            Chart? chartObject = Parent as Chart;

            serializer.WriteLine("\\" + chartObject?.CheckTextArea(this));
            int pos = serializer.BeginAttributes();

            if (Values.Style is not null)
                serializer.WriteSimpleAttribute("Style", Style);
            Values.Format?.Serialize(serializer, "Format", null);

            //if (Values.TopPadding is not null)
            if (!Values.TopPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("TopPadding", TopPadding);
            //if (Values.LeftPadding is not null)
            if (!Values.LeftPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("LeftPadding", LeftPadding);
            //if (Values.RightPadding is not null)
            if (!Values.RightPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("RightPadding", RightPadding);
            //if (Values.BottomPadding is not null)
            if (!Values.BottomPadding.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("BottomPadding", BottomPadding);

            //if (Values.Width is not null)
            if (!Values.Height.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Width", Width);
            //if (Values.Height is not null)
            if (!Values.Height.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Height", Height);

            if (Values.VerticalAlignment is not null)
                serializer.WriteSimpleAttribute("VerticalAlignment", VerticalAlignment);

            Values.LineFormat?.Serialize(serializer);
            Values.FillFormat?.Serialize(serializer);

            serializer.EndAttributes(pos);

            serializer.BeginContent();
            Values.Elements?.Serialize(serializer);
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(TextArea));

        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitTextArea(this);
            if (Values.Elements != null && visitChildren)
                ((IVisitable)Values.Elements).AcceptVisitor(visitor, visitChildren);
        }

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new TextAreaValues Values => (TextAreaValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class TextAreaValues : ChartObjectValues
        {
            internal TextAreaValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Height { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Width { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphFormat? Format { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public LineFormat? LineFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public FillFormat? FillFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? LeftPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? RightPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? TopPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? BottomPadding { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public VerticalAlignment? VerticalAlignment { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DocumentElements? Elements { get; set; }
        }
    }
}
