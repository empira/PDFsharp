// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents a paragraph which is used to build up a document with text.
    /// </summary>
    public class Paragraph : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Paragraph class.
        /// </summary>
        public Paragraph()
        {
            BaseValues = new ParagraphValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Paragraph class with the specified parent.
        /// </summary>
        internal Paragraph(DocumentObject parent) : base(parent)
        {
            BaseValues = new ParagraphValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Paragraph Clone()
            => (Paragraph)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var paragraph = (Paragraph)base.DeepCopy();
            if (paragraph.Values.Format != null)
            {
                paragraph.Values.Format = paragraph.Values.Format.Clone();
                paragraph.Values.Format.Parent = paragraph;
            }
            if (paragraph.Values.Elements != null)
            {
                paragraph.Values.Elements = paragraph.Values.Elements.Clone();
                paragraph.Values.Elements.Parent = paragraph;
            }
            return paragraph;
        }

        /// <summary>
        /// Adds a text phrase to the paragraph.
        /// </summary>
        /// <returns>Returns a new Text object with the last element of text that was added.</returns>
        public Text AddText(string text, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddText(text, textRenderOption);

        /// <summary>
        /// Adds a single character repeated the specified number of times to the paragraph.
        /// </summary>
        public Text AddChar(char ch, int count, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddChar(ch, count, textRenderOption);

        /// <summary>
        /// Adds a single character to the paragraph.
        /// </summary>
        public Text AddChar(char ch, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddChar(ch, textRenderOption);

        /// <summary>
        /// Adds one or more Symbol objects.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType, int count, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddCharacter(symbolType, count, textRenderOption);

        /// <summary>
        /// Adds a Symbol object.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddCharacter(symbolType, textRenderOption);

        /// <summary>
        /// Adds one or more Symbol objects defined by a character.
        /// </summary>
        public Character AddCharacter(char ch, int count, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddCharacter(ch, count, textRenderOption);

        /// <summary>
        /// Adds a Symbol object defined by a character.
        /// </summary>
        public Character AddCharacter(char ch, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddCharacter(ch, textRenderOption);

        /// <summary>
        /// Adds a space character as many as count.
        /// </summary>
        public Character AddSpace(int count, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddSpace(count, textRenderOption);

        /// <summary>
        /// Adds a horizontal tab.
        /// </summary>
        public void AddTab(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddTab(textRenderOption);

        /// <summary>
        /// Adds a line break.
        /// </summary>
        public void AddLineBreak(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddLineBreak(textRenderOption);

        /// <summary>
        /// Adds a new FormattedText.
        /// </summary>
        public FormattedText AddFormattedText(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFormattedText(textRenderOption);

        /// <summary>
        /// Adds a new FormattedText object with the given format.
        /// </summary>
        public FormattedText AddFormattedText(TextFormat textFormat, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFormattedText(textFormat, textRenderOption);

        /// <summary>
        /// Adds a new FormattedText with the given Font.
        /// </summary>
        public FormattedText AddFormattedText(Font font, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFormattedText(font, textRenderOption);

        /// <summary>
        /// Adds a new FormattedText with the given text.
        /// </summary>
        public FormattedText AddFormattedText(string text, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFormattedText(text, textRenderOption);

        /// <summary>
        /// Adds a new FormattedText object with the given text and format.
        /// </summary>
        public FormattedText AddFormattedText(string text, TextFormat textFormat, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFormattedText(text, textFormat, textRenderOption);

        /// <summary>
        /// Adds a new FormattedText object with the given text and font.
        /// </summary>
        public FormattedText AddFormattedText(string text, Font font, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFormattedText(text, font, textRenderOption);

        /// <summary>
        /// Adds a new FormattedText object with the given text and style.
        /// </summary>
        public FormattedText AddFormattedText(string text, string style, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFormattedText(text, style, textRenderOption);

        /// <summary>
        /// Adds a new Hyperlink of Type "Local", i.e. the target is a Bookmark within the Document.
        /// </summary>
        public Hyperlink AddHyperlink(string bookmarkName)
            => Elements.AddHyperlink(bookmarkName);

        /// <summary>
        /// Adds a new Hyperlink.
        /// </summary>
        public Hyperlink AddHyperlink(string name, HyperlinkType type)
            => Elements.AddHyperlink(name, type);

        /// <summary>
        /// Adds a new Hyperlink using the mailto protocol. The protocol is automatically added if missing.
        /// </summary>
        public Hyperlink AddMailLink(string url)
            => Elements.AddMailLink(url);

        /// <summary>
        /// Adds a new Hyperlink using the HTTP or HTTPS protocol. The HTTP protocol is automatically added if neither HTTP nor HTTPS is specified.
        /// </summary>
        public Hyperlink AddWebLink(string url)
            => Elements.AddWebLink(url);

        /// <summary>
        /// Adds a new Hyperlink of Type "ExternalBookmark", i.e. the target is a Bookmark in an external PDF Document.
        /// </summary>
        /// <param name="filename">The path to the target document.</param>
        /// <param name="bookmarkName">The Named Destination’s name in the target document.</param>
        /// <param name="newWindow">Defines if the HyperlinkType ExternalBookmark shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public Hyperlink AddHyperlink(string filename, string bookmarkName, HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
            => Elements.AddHyperlink(filename, bookmarkName, newWindow);

        /// <summary>
        /// Adds a new Hyperlink of Type "EmbeddedDocument".
        /// The target is a Bookmark in an embedded Document in this Document.
        /// </summary>
        /// <param name="destinationPath">The path to the named destination through the embedded documents.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the current (root or embedded) document to the embedded document holding the destination.
        /// ".." references to the parent, other strings refer to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">Defines if the HyperlinkType ExternalBookmark shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public Hyperlink AddHyperlinkToEmbeddedDocument(string destinationPath, HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
            => Elements.AddHyperlinkToEmbeddedDocument(destinationPath, newWindow);

        /// <summary>
        /// Adds a new Hyperlink of Type "EmbeddedDocument".
        /// The target is a Bookmark in an embedded Document in an external PDF Document.
        /// </summary>
        /// <param name="filename">The path to the target document.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents in the target document.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the root document to the embedded document.
        /// Each segment name refers to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">Defines if the HyperlinkType ExternalBookmark shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public Hyperlink AddHyperlinkToEmbeddedDocument(string filename, string destinationPath, HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
            => Elements.AddHyperlinkToEmbeddedDocument(filename, destinationPath, newWindow);

        /// <summary>
        /// Adds a new Bookmark.
        /// </summary>
        /// <param name="name">The name of the bookmark.</param>
        /// <param name="prepend">True, if the bookmark shall be inserted at the beginning of the paragraph.</param>
        public BookmarkField AddBookmark(string name, bool prepend = true)
            => Elements.AddBookmark(name, prepend);

        /// <summary>
        /// Adds a new PageField.
        /// </summary>
        public PageField AddPageField(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddPageField(textRenderOption);

        /// <summary>
        /// Adds a new PageRefField.
        /// </summary>
        public PageRefField AddPageRefField(string name, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddPageRefField(name, textRenderOption);

        /// <summary>
        /// Adds a new NumPagesField.
        /// </summary>
        public NumPagesField AddNumPagesField(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddNumPagesField(textRenderOption);

        /// <summary>
        /// Adds a new SectionField.
        /// </summary>
        public SectionField AddSectionField(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddSectionField(textRenderOption);

        /// <summary>
        /// Adds a new SectionPagesField.
        /// </summary>
        public SectionPagesField AddSectionPagesField(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddSectionPagesField(textRenderOption);

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public DateField AddDateField(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddDateField(textRenderOption);

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public DateField AddDateField(string format, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddDateField(format, textRenderOption);

        /// <summary>
        /// Adds a new InfoField.
        /// </summary>
        public InfoField AddInfoField(InfoFieldType iType, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddInfoField(iType, textRenderOption);

        /// <summary>
        /// Adds a new Footnote with the specified text.
        /// </summary>
        public Footnote AddFootnote(string text, TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFootnote(text, textRenderOption);

        /// <summary>
        /// Adds a new Footnote.
        /// </summary>
        public Footnote AddFootnote(TextRenderOption textRenderOption = TextRenderOption.Default)
            => Elements.AddFootnote(textRenderOption);

        /// <summary>
        /// Adds a new Image object.
        /// </summary>
        public Image AddImage(string fileName)
            => Elements.AddImage(fileName);

        /// <summary>
        /// Adds a new Bookmark.
        /// </summary>
        public void Add(BookmarkField bookmark)
            => Elements.Add(bookmark);

        /// <summary>
        /// Adds a new PageField.
        /// </summary>
        public void Add(PageField pageField)
            => Elements.Add(pageField);

        /// <summary>
        /// Adds a new PageRefField.
        /// </summary>
        public void Add(PageRefField pageRefField)
            => Elements.Add(pageRefField);

        /// <summary>
        /// Adds a new NumPagesField.
        /// </summary>
        public void Add(NumPagesField numPagesField)
            => Elements.Add(numPagesField);

        /// <summary>
        /// Adds a new SectionField.
        /// </summary>
        public void Add(SectionField sectionField)
            => Elements.Add(sectionField);

        /// <summary>
        /// Adds a new SectionPagesField.
        /// </summary>
        public void Add(SectionPagesField sectionPagesField)
            => Elements.Add(sectionPagesField);

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public void Add(DateField dateField)
            => Elements.Add(dateField);

        /// <summary>
        /// Adds a new InfoField.
        /// </summary>
        public void Add(InfoField infoField)
            => Elements.Add(infoField);

        /// <summary>
        /// Adds a new Footnote.
        /// </summary>
        public void Add(Footnote footnote)
            => Elements.Add(footnote);

        /// <summary>
        /// Adds a new Text.
        /// </summary>
        public void Add(Text text)
            => Elements.Add(text);

        /// <summary>
        /// Adds a new FormattedText.
        /// </summary>
        public void Add(FormattedText formattedText)
            => Elements.Add(formattedText);

        /// <summary>
        /// Adds a new Hyperlink.
        /// </summary>
        public void Add(Hyperlink hyperlink)
            => Elements.Add(hyperlink);

        /// <summary>
        /// Adds a new Image.
        /// </summary>
        public void Add(Image image)
            => Elements.Add(image);

        /// <summary>
        /// Adds a new Character.
        /// </summary>
        public void Add(Character character)
            => Elements.Add(character);

        /// <summary>
        /// Gets or sets the style name.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the ParagraphFormat object of the paragraph.
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
        /// Gets the collection of document objects that defines the paragraph.
        /// </summary>
        public ParagraphElements Elements
        {
            get => Values.Elements ??= new ParagraphElements(this);
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
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitParagraph(this);

            if (visitChildren && Values.Elements != null)
                ((IVisitable)Values.Elements).AcceptVisitor(visitor, true);
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        internal bool SerializeContentOnly
        {
            get => _serializeContentOnly;
            set => _serializeContentOnly = value;
        }

        bool _serializeContentOnly;

        /// <summary>
        /// Converts Paragraph into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (!_serializeContentOnly)
            {
                serializer.WriteComment(Values.Comment);
                serializer.WriteLine("\\paragraph");

                int pos = serializer.BeginAttributes();
                {
                    if (!String.IsNullOrEmpty(Values.Style))
                        serializer.WriteLine("Style = \"" + Values.Style + "\"");

                    if (Values.Format is not null)
                        Values.Format.Serialize(serializer, "Format", null);
                }
                serializer.EndAttributes(pos);

                serializer.BeginContent();
                {
                    if (Values.Elements is not null)
                        Elements.Serialize(serializer);
                    serializer.CloseUpLine();
                }
                serializer.EndContent();
            }
            else
            {
                Elements.Serialize(serializer);
                serializer.CloseUpLine();
            }
        }

        /// <summary>
        /// Returns an array of Paragraphs that are separated by parabreaks. Null if no parabreak is found.
        /// </summary>
        internal Paragraph[]? SplitOnParaBreak()
        {
            if (Values.Elements == null)
                return null;

            int startIdx = 0;
            List<Paragraph> paragraphs = new List<Paragraph>();
            for (int idx = 0; idx < Elements.Count; idx++)
            {
                var element = Elements[idx];
                if (element is Character)
                {
                    Character character = (Character)element;
                    if (character.SymbolName == SymbolName.ParaBreak)
                    {
                        Paragraph paragraph = new Paragraph();
                        paragraph.Format = Format.Clone();
                        paragraph.Style = Style;
                        paragraph.Elements = SubsetElements(startIdx, idx - 1);
                        startIdx = idx + 1;
                        paragraphs.Add(paragraph);
                    }
                }
            }
            if (startIdx == 0) //No paragraph breaks given.
                return null;
            else
            {
                var paragraph = new Paragraph
                {
                    Format = Format.Clone(),
                    Style = Style,
                    Elements = SubsetElements(startIdx, Values.Elements.Count - 1)
                };
                paragraphs.Add(paragraph);

                return paragraphs.ToArray();
            }
        }

        /// <summary>
        /// Gets a subset of the paragraph elements.
        /// </summary>
        /// <param name="startIdx">Start index of the required subset.</param>
        /// <param name="endIdx">End index of the required subset.</param>
        /// <returns>A ParagraphElements object with cloned elements.</returns>
        ParagraphElements SubsetElements(int startIdx, int endIdx)
        {
            var paragraphElements = new ParagraphElements();
            if (Values.Elements is not null)
            {
                for (int idx = startIdx; idx <= endIdx; idx++)
                    paragraphElements.Add((DocumentObject?)(Values.Elements[idx]?.Clone()));
            }
            return paragraphElements;
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Paragraph));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ParagraphValues Values => (ParagraphValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ParagraphValues : Values
        {
            internal ParagraphValues(DocumentObject owner) : base(owner)
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
            public ParagraphElements? Elements { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
