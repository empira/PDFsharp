// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents a footnote in a paragraph.
    /// </summary>
    public class Footnote : TextBasedDocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Footnote class.
        /// </summary>
        public Footnote(TextRenderOption textRenderOption = TextRenderOption.Default) : base(textRenderOption)
        {
            BaseValues = new FootnoteValues(this);
            //NYI: Nested footnote check!
        }

        /// <summary>
        /// Initializes a new instance of the Footnote class with the specified parent.
        /// </summary>
        internal Footnote(DocumentObject parent, TextRenderOption textRenderOption = TextRenderOption.Default) : base(parent, textRenderOption)
        {
            BaseValues = new FootnoteValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Footnote class with a text the Footnote shall contain.
        /// </summary>
        internal Footnote(string content, TextRenderOption textRenderOption = TextRenderOption.Default) : this(textRenderOption)
        {
            Elements.AddParagraph(content, textRenderOption);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Footnote Clone()
            => (Footnote)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var footnote = (Footnote)base.DeepCopy();
            if (footnote.Values.Elements is not null)
            {
                footnote.Values.Elements = footnote.Values.Elements.Clone();
                footnote.Values.Elements.Parent = footnote;
            }
            if (footnote.Values.Format is not null)
            {
                footnote.Values.Format = footnote.Values.Format.Clone();
                footnote.Values.Format.Parent = footnote;
            }
            return footnote;
        }

        /// <summary>
        /// Adds a new paragraph to the footnote.
        /// </summary>
        public Paragraph AddParagraph()
            => Elements.AddParagraph();

        /// <summary>
        /// Adds a new paragraph with the specified text to the footnote.
        /// </summary>
        public Paragraph AddParagraph(string text, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddParagraph(text, textRenderOption);

        /// <summary>
        /// Adds a new table to the footnote.
        /// </summary>
        public Table AddTable()
            => Elements.AddTable();

        /// <summary>
        /// Adds a new image to the footnote.
        /// </summary>
        public Image AddImage(string name)
            => Elements.AddImage(name);

        /// <summary>
        /// Adds a new paragraph to the footnote.
        /// </summary>
        public void Add(Paragraph paragraph)
            => Elements.Add(paragraph);

        /// <summary>
        /// Adds a new table to the footnote.
        /// </summary>
        public void Add(Table table)
            => Elements.Add(table);

        /// <summary>
        /// Adds a new image to the footnote.
        /// </summary>
        public void Add(Image image)
            => Elements.Add(image);

        /// <summary>
        /// Gets the collection of paragraph elements that defines the footnote.
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
        /// Gets or sets the character to be used to mark the footnote.
        /// </summary>
        public string Reference
        {
            get => Values.Reference ?? "";
            set => Values.Reference = value;
        }

        /// <summary>
        /// Gets or sets the style name of the footnote.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets the format of the footnote.
        /// </summary>
        public ParagraphFormat Format
        {
            get => Values.Format ??= new ParagraphFormat(this);
            set
            {
                SetParentOf(value);
                Values.Format = value;
            }
        }

        /// <summary>
        /// Converts Footnote into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\footnote");

            int pos = serializer.BeginAttributes();
            if (!Values.Reference.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Reference", Reference);
            if (!Values.Style.IsValueNullOrEmpty())
                serializer.WriteSimpleAttribute("Style", Style);

            if (Values.Format is not null)
                Values.Format.Serialize(serializer, "Format", null);

            serializer.EndAttributes(pos);

            pos = serializer.BeginContent();
            if (Values.Elements is not null)
                Values.Elements.Serialize(serializer);
            serializer.EndContent(pos);
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitFootnote(this);

            if (visitChildren && Values.Elements is not null)
                ((IVisitable)Values.Elements)?.AcceptVisitor(visitor, true);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Footnote));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public FootnoteValues Values => (FootnoteValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class FootnoteValues : Values
        {
            internal FootnoteValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public DocumentElements? Elements { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Reference { get; set; }

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
        }
    }
}
