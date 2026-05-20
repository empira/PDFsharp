// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A Section is a collection of document objects sharing the same header, footer, 
    /// and page setup.
    /// </summary>
    public class Section : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Section class.
        /// </summary>
        public Section()
        {
            BaseValues = new SectionValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Section class with the specified parent.
        /// </summary>
        internal Section(DocumentObject parent) : base(parent)
        {
            BaseValues = new SectionValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Section Clone()
            => (Section)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var section = (Section)base.DeepCopy();
            if (section.Values.PageSetup != null)
            {
                section.Values.PageSetup = section.Values.PageSetup.Clone();
                section.Values.PageSetup.Parent = section;
            }
            if (section.Values.Headers != null)
            {
                section.Values.Headers = section.Values.Headers.Clone();
                section.Values.Headers.Parent = section;
            }
            if (section.Values.Footers != null)
            {
                section.Values.Footers = section.Values.Footers.Clone();
                section.Values.Footers.Parent = section;
            }
            if (section.Values.Elements != null)
            {
                section.Values.Elements = section.Values.Elements.Clone();
                section.Values.Elements.Parent = section;
            }
            return section;
        }

        /// <summary>
        /// Gets the previous section, or null if no such section exists.
        /// </summary>
        public Section? PreviousSection()
        {
            if (Parent is not Sections sections)
                return null;

            var index = sections.IndexOf(this);
            return index > 0 ? sections[index - 1] : null;
        }

        /// <summary>
        /// Adds a new paragraph to the section.
        /// </summary>
        public Paragraph AddParagraph()
        {
            return Elements.AddParagraph();
        }

        /// <summary>
        /// Adds a new paragraph with the specified text to the section.
        /// </summary>
        public Paragraph AddParagraph(string paragraphText, TextRenderOption textRenderOption = TextRenderOption.Default)
        {
            return Elements.AddParagraph(paragraphText, textRenderOption);
        }

        /// <summary>
        /// Adds a new paragraph with the specified text and style to the section.
        /// </summary>
        public Paragraph AddParagraph(string paragraphText, string style, TextRenderOption textRenderOption = TextRenderOption.Default)
        {
            return Elements.AddParagraph(paragraphText, style, textRenderOption);
        }

        /// <summary>
        /// Adds a new chart with the specified type to the section.
        /// </summary>
        public Chart AddChart(ChartType type)
        {
            return Elements.AddChart(type);
        }

        /// <summary>
        /// Adds a new chart to the section.
        /// </summary>
        public Chart AddChart()
        {
            return Elements.AddChart();
        }

        /// <summary>
        /// Adds a new table to the section.
        /// </summary>
        public Table AddTable()
        {
            return Elements.AddTable();
        }

        /// <summary>
        /// Adds a manual page break.
        /// </summary>
        public void AddPageBreak()
        {
            Elements.AddPageBreak();
        }

        /// <summary>
        /// Adds a new Image to the section.
        /// </summary>
        public Image AddImage(string fileName)
        {
            return Elements.AddImage(fileName);
        }

        /// <summary>
        /// Adds a new text frame to the section.
        /// </summary>
        public TextFrame AddTextFrame()
        {
            return Elements.AddTextFrame();
        }

        /// <summary>
        /// Adds a new paragraph to the section.
        /// </summary>
        public void Add(Paragraph paragraph)
        {
            Elements.Add(paragraph);
        }

        /// <summary>
        /// Adds a new chart to the section.
        /// </summary>
        public void Add(Chart chart)
        {
            Elements.Add(chart);
        }

        /// <summary>
        /// Adds a new table to the section.
        /// </summary>
        public void Add(Table table)
        {
            Elements.Add(table);
        }

        /// <summary>
        /// Adds a new image to the section.
        /// </summary>
        public void Add(Image image)
        {
            Elements.Add(image);
        }

        /// <summary>
        /// Adds a new text frame to the section.
        /// </summary>
        public void Add(TextFrame textFrame)
        {
            Elements.Add(textFrame);
        }

        /// <summary>
        /// Gets the PageSetup object.
        /// </summary>
        public PageSetup PageSetup
        {
            get => Values.PageSetup ??= new PageSetup(this);
            set
            {
                SetParentOf(value);
                Values.PageSetup = value;
            }
        }

        /// <summary>
        /// Gets the HeadersFooters collection containing the headers.
        /// </summary>
        public HeadersFooters Headers
        {
            get => Values.Headers ??= new HeadersFooters(this);
            set
            {
                SetParentOf(value);
                Values.Headers = value;
            }
        }

        /// <summary>
        /// Gets the HeadersFooters collection containing the footers.
        /// </summary>
        public HeadersFooters Footers
        {
            get => Values.Footers ??= new HeadersFooters(this);
            set
            {
                SetParentOf(value);
                Values.Footers = value;
            }
        }

        /// <summary>
        /// Gets the document elements that build the section’s content.
        /// </summary>
        public DocumentElements Elements
        {
            get => Values.Elements ??= new DocumentElements(this);
            set
            {
                SetParentOf(value);
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
        /// Gets the last paragraph of this section, or null if no paragraph exists in this section.
        /// </summary>
        public Paragraph LastParagraph
        {
            [return: MaybeNull]
            get
            {
                // _elements is created when needed and can be null.
                if (Values.Elements == null)
                    return null!;

                var count = Values.Elements.Count;
                for (var idx = count - 1; idx >= 0; idx--)
                {
                    if (Values.Elements[idx] is Paragraph paragraph)
                        return paragraph;
                }
                return null!;
            }
        }

        /// <summary>
        /// Gets the last table of this section, or null if no table exists in this section.
        /// </summary>
        public Table LastTable
        {
            [return: MaybeNull]
            get
            {
                // _elements is created when needed and can be null.
                if (Values.Elements == null)
                    return null!;

                var count = Values.Elements.Count;
                for (var idx = count - 1; idx >= 0; idx--)
                {
                    if (Values.Elements[idx] is Table table)
                        return table;
                }
                return null!;
            }
        }

        /// <summary>
        /// Converts Section into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment ?? "");
            serializer.WriteLine("\\section");

            var pos = serializer.BeginAttributes();
            if (Values.PageSetup is not null) // BUG_OLD: Is there a reason to check the Values value / backing field and not the property like done below with Headers etc.?
                PageSetup.Serialize(serializer);
            serializer.EndAttributes(pos);

            serializer.BeginContent();
            Values.Headers?.Serialize(serializer);
            Values.Footers?.Serialize(serializer);
            if (!Values.Elements.IsValueNullOrEmpty()) // BUG_OLD: IsNull("elements") uses DocumentObject.IsNull(). DocumentElements inherits DocumentObjectCollection, which overrides IsNull() with its own implementation checking IsNull() of its children.
                Values.Elements?.Serialize(serializer);

            serializer.EndContent();
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitSection(this);

            if (visitChildren && Values.Headers != null)
                ((IVisitable)Values.Headers).AcceptVisitor(visitor, true);
            if (visitChildren && Values.Footers != null)
                ((IVisitable)Values.Footers).AcceptVisitor(visitor, true);
            if (visitChildren && Values.Elements != null)
                ((IVisitable)Values.Elements).AcceptVisitor(visitor, true);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Section));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public SectionValues Values => (SectionValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class SectionValues : Values
        {
            internal SectionValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public PageSetup? PageSetup { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HeadersFooters? Headers { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HeadersFooters? Footers { get; set; }

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
