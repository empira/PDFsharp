// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Represents a text frame that can be freely placed.
    /// </summary>
    public class TextFrame : Shape, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the TextFrame class.
        /// </summary>
        public TextFrame()
        {
            BaseValues = new TextFrameValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the TextFrame class with the specified parent.
        /// </summary>
        internal TextFrame(DocumentObject parent) : base(parent)
        {
            BaseValues = new TextFrameValues(this); 
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new TextFrame Clone() 
            => (TextFrame)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var textFrame = (TextFrame)base.DeepCopy();
            if (textFrame.Values.Elements != null)
            {
                textFrame.Values.Elements = textFrame.Values.Elements.Clone();
                textFrame.Values.Elements.Parent = textFrame;
            }
            return textFrame;
        }

        /// <summary>
        /// Adds a new paragraph to the text frame.
        /// </summary>
        public Paragraph AddParagraph() 
            => Elements.AddParagraph();

        /// <summary>
        /// Adds a new paragraph with the specified text to the text frame.
        /// </summary>
        public Paragraph AddParagraph(string paragraphText) 
            => Elements.AddParagraph(paragraphText);

        /// <summary>
        /// Adds a new chart with the specified type to the text frame.
        /// </summary>
        public Chart AddChart(ChartType type) 
            => Elements.AddChart(type);

        /// <summary>
        /// Adds a new chart to the text frame.
        /// </summary>
        public Chart AddChart() 
            => Elements.AddChart();

        /// <summary>
        /// Adds a new table to the text frame.
        /// </summary>
        public Table AddTable() 
            => Elements.AddTable();

        /// <summary>
        /// Adds a new Image to the text frame.
        /// </summary>
        public Image AddImage(string fileName) 
            => Elements.AddImage(fileName);

        /// <summary>
        /// Adds a new paragraph to the text frame.
        /// </summary>
        public void Add(Paragraph paragraph) 
            => Elements.Add(paragraph);

        /// <summary>
        /// Adds a new chart to the text frame.
        /// </summary>
        public void Add(Chart chart) 
            => Elements.Add(chart);

        /// <summary>
        /// Adds a new table to the text frame.
        /// </summary>
        public void Add(Table table) 
            => Elements.Add(table);

        /// <summary>
        /// Adds a new image to the text frame.
        /// </summary>
        public void Add(Image image) 
            => Elements.Add(image);

        /// <summary>
        /// Gets or sets the Margin between the text frame’s content and its left edge.
        /// </summary>
        public Unit MarginLeft
        {
            get => Values.MarginLeft ?? Unit.Empty;
            set => Values.MarginLeft = value;
        }

        /// <summary>
        /// Gets or sets the Margin between the text frame’s content and its right edge.
        /// </summary>
        public Unit MarginRight
        {
            get => Values.MarginRight ?? Unit.Empty;
            set => Values.MarginRight = value;
        }

        /// <summary>
        /// Gets or sets the Margin between the text frame’s content and its top edge.
        /// </summary>
        public Unit MarginTop
        {
            get => Values.MarginTop ?? Unit.Empty;
            set => Values.MarginTop = value;
        }

        /// <summary>
        /// Gets or sets the Margin between the text frame’s content and its bottom edge.
        /// </summary>
        public Unit MarginBottom
        {
            get => Values.MarginBottom ?? Unit.Empty;
            set => Values.MarginBottom = value;
        }

        /// <summary>
        /// Sets all margins in one step with the same value.
        /// </summary>
        public Unit Margin
        {
            set
            {
                Values.MarginLeft = value;
                Values.MarginRight = value;
                Values.MarginTop = value;
                Values.MarginBottom = value;
            }
        }

        /// <summary>
        /// Gets or sets the text orientation for the text frame’s content.
        /// </summary>
        public TextOrientation Orientation
        {
            get => Values.Orientation ?? TextOrientation.Horizontal;
            set => Values.Orientation = value;
        }

        /// <summary>
        /// The document elements that build the text frame’s content.
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
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitTextFrame(this);

            if (visitChildren && Values.Elements != null)
                ((IVisitable)Values.Elements).AcceptVisitor(visitor, true);
        }

        /// <summary>
        /// Converts TextFrame into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\textframe");
            int pos = serializer.BeginAttributes();
            base.Serialize(serializer);
            //if (Values.MarginLeft is not null)
            if (!Values.MarginLeft.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("MarginLeft", MarginLeft);
            //if (Values.MarginRight is not null)
            if (!Values.MarginRight.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("MarginRight", MarginRight);
            //if (Values.MarginTop is not null)
            if (!Values.MarginTop.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("MarginTop", MarginTop);
            //if (Values.MarginBottom is not null)
            if (!Values.MarginBottom.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("MarginBottom", MarginBottom);
            if (Values.Orientation is not null)
                serializer.WriteSimpleAttribute("Orientation", Orientation);
            serializer.EndAttributes(pos);

            serializer.BeginContent();
            Values.Elements?.Serialize(serializer);
            serializer.EndContent();
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(TextFrame));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new TextFrameValues Values => (TextFrameValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class TextFrameValues : ShapeValues
        {
            internal TextFrameValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? MarginLeft { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? MarginRight { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? MarginTop { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? MarginBottom { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public TextOrientation? Orientation { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DocumentElements? Elements { get; set; }
        }
    }
}
