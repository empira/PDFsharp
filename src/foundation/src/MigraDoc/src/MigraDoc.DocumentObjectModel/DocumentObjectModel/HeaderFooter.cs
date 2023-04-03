// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents a header or footer object in a section.
    /// </summary>
    public class HeaderFooter : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the HeaderFooter class.
        /// </summary>
        public HeaderFooter()
        {
            BaseValues = new HeaderFooterValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the HeaderFooter class with the specified parent.
        /// </summary>
        internal HeaderFooter(DocumentObject parent) : base(parent)
        {
            BaseValues = new HeaderFooterValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new HeaderFooter Clone() 
            => (HeaderFooter)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var headerFooter = (HeaderFooter)base.DeepCopy();
            if (headerFooter.Values.Format != null)
            {
                headerFooter.Values.Format = headerFooter.Values.Format.Clone();
                headerFooter.Values.Format.Parent = headerFooter;
            }
            if (headerFooter.Values.Elements != null)
            {
                headerFooter.Values.Elements = headerFooter.Values.Elements.Clone();
                headerFooter.Values.Elements.Parent = headerFooter;
            }
            return headerFooter;
        }

        /// <summary>
        /// Adds a new paragraph to the header or footer.
        /// </summary>
        public Paragraph AddParagraph()
        {
            return Elements.AddParagraph();
        }

        /// <summary>
        /// Adds a new paragraph with the specified text to the header or footer.
        /// </summary>
        public Paragraph AddParagraph(string paragraphText)
        {
            return Elements.AddParagraph(paragraphText);
        }

        /// <summary>
        /// Adds a new chart with the specified type to the header or footer.
        /// </summary>
        public Chart AddChart(ChartType type)
        {
            return Elements.AddChart(type);
        }

        /// <summary>
        /// Adds a new chart to the header or footer.
        /// </summary>
        public Chart AddChart()
        {
            return Elements.AddChart();
        }

        /// <summary>
        /// Adds a new table to the header or footer.
        /// </summary>
        public Table AddTable()
        {
            return Elements.AddTable();
        }

        /// <summary>
        /// Adds a new Image to the header or footer.
        /// </summary>
        public Image AddImage(string fileName)
        {
            return Elements.AddImage(fileName);
        }

        /// <summary>
        /// Adds a new text frame to the header or footer.
        /// </summary>
        public TextFrame AddTextFrame()
        {
            return Elements.AddTextFrame();
        }

        /// <summary>
        /// Adds a new paragraph to the header or footer.
        /// </summary>
        public void Add(Paragraph paragraph)
        {
            Elements.Add(paragraph);
        }

        /// <summary>
        /// Adds a new chart to the header or footer.
        /// </summary>
        public void Add(Chart chart)
        {
            Elements.Add(chart);
        }

        /// <summary>
        /// Adds a new table to the header or footer.
        /// </summary>
        public void Add(Table table)
        {
            Elements.Add(table);
        }

        /// <summary>
        /// Adds a new image to the header or footer.
        /// </summary>
        public void Add(Image image)
        {
            Elements.Add(image);
        }

        /// <summary>
        /// Adds a new text frame to the header or footer.
        /// </summary>
        public void Add(TextFrame textFrame)
        {
            Elements.Add(textFrame);
        }

        /// <summary>
        /// Returns true if this is a header, false otherwise.
        /// </summary>
        public bool IsHeader => ((HeadersFooters?)Parent)!.IsHeader; // BUG Exception if no parent? (following properties too)

        /// <summary>
        /// Returns true if this is a footer, false otherwise.
        /// </summary>
        public bool IsFooter => ((HeadersFooters?)Parent)!.IsFooter;

        /// <summary>
        /// Returns true if this is a first page header or footer, false otherwise.
        /// </summary>
        public bool IsFirstPage => ((HeadersFooters?)Parent)?.Values.FirstPage == this;

        /// <summary>
        /// Returns true if this is an even page header or footer, false otherwise.
        /// </summary>
        public bool IsEvenPage => ((HeadersFooters?)Parent)?.Values.EvenPage == this;

        /// <summary>
        /// Returns true if this is a primary header or footer, false otherwise.
        /// </summary>
        public bool IsPrimary => ((HeadersFooters?)Parent)?.Values.Primary == this;

        /// <summary>
        /// Gets or sets the style name.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set
            {
                var style = Document.Styles[value];
                if (style != null)
                    Values.Style = value;
                else
                    throw new ArgumentException($"Invalid style name '{value}'.");
            }
        }

        /// <summary>
        /// Gets or sets the paragraph format.
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
        /// Gets the collection of document objects that defines the header or footer.
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
        /// Gets or sets a comment associated with this object.
        /// </summary>
        public string Comment
        {
            get => Values.Comment ?? "";
            set => Values.Comment = value;
        }

        /// <summary>
        /// Converts HeaderFooter into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var headersfooters = (HeadersFooters?)Parent;
            if (headersfooters?.Primary == this)
                Serialize(serializer, "primary");
            else if (headersfooters?.EvenPage == this)
                Serialize(serializer, "evenpage");
            else if (headersfooters?.FirstPage == this)
                Serialize(serializer, "firstpage");
        }

        /// <summary>
        /// Converts HeaderFooter into DDL.
        /// </summary>
        internal void Serialize(Serializer serializer, string prefix)
        {
            if (IsNull()) // BUG???
                return;

            // Do not write attributes if there are no elements. // BUG???
            if (Values.Elements is null) // BUG???
                return;

            serializer.WriteComment(Values.Comment);
            serializer.WriteLine("\\" + prefix + (IsHeader ? "header" : "footer"));

            int pos = serializer.BeginAttributes();
            Values.Format?.Serialize(serializer, "Format", null);
            serializer.EndAttributes(pos);

            serializer.BeginContent();
            Values.Elements?.Serialize(serializer);
            serializer.EndContent();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitHeaderFooter(this);

            if (visitChildren && Values.Elements != null)
                ((IVisitable)Values.Elements).AcceptVisitor(visitor, true);
        }

        /// <summary>
        /// Determines whether this instance is null (not set).
        /// </summary>
        public override bool IsNull()
        {
            return false; // BUG???
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(HeaderFooter));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public HeaderFooterValues Values => (HeaderFooterValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class HeaderFooterValues : Values
        {
            internal HeaderFooterValues(DocumentObject owner) : base(owner)
            { }

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
            public DocumentElements? Elements { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
