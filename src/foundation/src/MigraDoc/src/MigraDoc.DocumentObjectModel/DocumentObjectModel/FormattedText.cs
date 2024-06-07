// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the format of a text.
    /// </summary>
    public class FormattedText : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the FormattedText class.
        /// </summary>
        public FormattedText()
        {
            BaseValues = new FormattedTextValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the FormattedText class with the specified parent.
        /// </summary>
        internal FormattedText(DocumentObject parent) : base(parent)
        {
            BaseValues = new FormattedTextValues(this);
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new FormattedText Clone()
            => (FormattedText)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            FormattedText formattedText = (FormattedText)base.DeepCopy();
            if (formattedText.Values.Font != null)
            {
                formattedText.Values.Font = formattedText.Values.Font.Clone();
                formattedText.Values.Font.Parent = formattedText;
            }

            if (formattedText.Values.Elements != null)
            {
                formattedText.Values.Elements = formattedText.Values.Elements.Clone();
                formattedText.Values.Elements.Parent = formattedText;
            }

            return formattedText;
        }

        /// <summary>
        /// Adds a new Bookmark.
        /// <param name="name">The name of the bookmark.</param>
        /// <param name="prepend">True, if the bookmark shall be inserted at the beginning of the paragraph.</param>
        /// </summary>
        public BookmarkField AddBookmark(string name, bool prepend = true)
            => Elements.AddBookmark(name, prepend);

        /// <summary>
        /// Adds a single character repeated the specified number of times to the formatted text.
        /// </summary>
        public Text AddChar(char ch, int count)
            => Elements.AddChar(ch, count);

        /// <summary>
        /// Adds a single character to the formatted text.
        /// </summary>
        public Text AddChar(char ch)
            => Elements.AddChar(ch);

        /// <summary>
        /// Adds a new PageField.
        /// </summary>
        public PageField AddPageField()
            => Elements.AddPageField();

        /// <summary>
        /// Adds a new PageRefField.
        /// </summary>
        public PageRefField AddPageRefField(string name)
            => Elements.AddPageRefField(name);

        /// <summary>
        /// Adds a new NumPagesField.
        /// </summary>
        public NumPagesField AddNumPagesField()
            => Elements.AddNumPagesField();

        /// <summary>
        /// Adds a new SectionField.
        /// </summary>
        public SectionField AddSectionField()
            => Elements.AddSectionField();

        /// <summary>
        /// Adds a new SectionPagesField.
        /// </summary>
        public SectionPagesField AddSectionPagesField()
            => Elements.AddSectionPagesField();

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public DateField AddDateField()
            => Elements.AddDateField();

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public DateField AddDateField(string format)
            => Elements.AddDateField(format);

        /// <summary>
        /// Adds a new InfoField.
        /// </summary>
        public InfoField AddInfoField(InfoFieldType iType)
            => Elements.AddInfoField(iType);

        /// <summary>
        /// Adds a new Footnote with the specified text.
        /// </summary>
        public Footnote AddFootnote(string text)
            => Elements.AddFootnote(text);

        /// <summary>
        /// Adds a new Footnote.
        /// </summary>
        public Footnote AddFootnote()
            => Elements.AddFootnote();

        /// <summary>
        /// Adds a text phrase to the formatted text.
        /// </summary>
        /// <param name="text">Content of the new text object.</param>
        /// <returns>Returns a new Text object with the last element of text that was added.</returns>
        public Text AddText(string text)
            => Elements.AddText(text);

        /// <summary>
        /// Adds a new FormattedText.
        /// </summary>
        public FormattedText AddFormattedText()
            => Elements.AddFormattedText();

        /// <summary>
        /// Adds a new FormattedText object with the given format.
        /// </summary>
        public FormattedText AddFormattedText(TextFormat textFormat)
            => Elements.AddFormattedText(textFormat);

        /// <summary>
        /// Adds a new FormattedText with the given Font.
        /// </summary>
        public FormattedText AddFormattedText(Font font)
            => Elements.AddFormattedText(font);

        /// <summary>
        /// Adds a new FormattedText with the given text.
        /// </summary>
        public FormattedText AddFormattedText(string text)
            => Elements.AddFormattedText(text);

        /// <summary>
        /// Adds a new FormattedText object with the given text and format.
        /// </summary>
        public FormattedText AddFormattedText(string text, TextFormat textFormat)
            => Elements.AddFormattedText(text, textFormat);

        /// <summary>
        /// Adds a new FormattedText object with the given text and font.
        /// </summary>
        public FormattedText AddFormattedText(string text, Font font)
            => Elements.AddFormattedText(text, font);

        /// <summary>
        /// Adds a new FormattedText object with the given text and style.
        /// </summary>
        public FormattedText AddFormattedText(string text, string style)
            => Elements.AddFormattedText(text, style);

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
        /// <param name="newWindow">Defines if the document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public Hyperlink AddHyperlink(string filename, string bookmarkName,
            HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
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
        public Hyperlink AddHyperlinkToEmbeddedDocument(string destinationPath,
            HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
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
        public Hyperlink AddHyperlinkToEmbeddedDocument(string filename, string destinationPath,
            HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
            => Elements.AddHyperlinkToEmbeddedDocument(filename, destinationPath, newWindow);

        /// <summary>
        /// Adds a new Image object.
        /// </summary>
        public Image AddImage(string fileName)
            => Elements.AddImage(fileName);

        /// <summary>
        /// Adds a Symbol object.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType)
            => Elements.AddCharacter(symbolType);

        /// <summary>
        /// Adds one or more Symbol objects.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType, int count)
            => Elements.AddCharacter(symbolType, count);

        /// <summary>
        /// Adds a Symbol object defined by a character.
        /// </summary>
        public Character AddCharacter(char ch)
            => Elements.AddCharacter(ch);

        /// <summary>
        /// Adds one or more Symbol objects defined by a character.
        /// </summary>
        public Character AddCharacter(char ch, int count)
            => Elements.AddCharacter(ch, count);

        /// <summary>
        /// Adds one or more Space characters.
        /// </summary>
        public Character AddSpace(int count)
            => Elements.AddSpace(count);

        /// <summary>
        /// Adds a horizontal tab.
        /// </summary>
        public void AddTab()
            => Elements.AddTab();

        /// <summary>
        /// Adds a line break.
        /// </summary>
        public void AddLineBreak()
            => Elements.AddLineBreak();

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
        /// Gets or sets the font object.
        /// </summary>
        public Font Font
        {
            get => Values.Font ??= new Font(this);
            set
            {
                SetParent(value);
                Values.Font = value;
            }
        }

        /// <summary>
        /// Gets or sets the style name.
        /// </summary>
        public string Style
        {
            get => Values.Style ?? "";
            set => Values.Style = value;
        }

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        public string FontName
        {
            get => Font.Name;
            set => Font.Name = value;
        }

        /// <summary>
        /// Gets or sets the name of the font.
        /// For internal use only.
        /// </summary>
        internal string Name
        {
            get => Font.Name;
            set => Font.Name = value;
        }

        /// <summary>
        /// Gets or sets the size in point.
        /// </summary>
        public Unit Size
        {
            get => Font.Size;
            set => Font.Size = value;
        }

        /// <summary>
        /// Gets or sets the bold property.
        /// </summary>
        public bool Bold
        {
            get => Font.Bold;
            set => Font.Bold = value;
        }

        /// <summary>
        /// Gets or sets the italic property.
        /// </summary>
        public bool Italic
        {
            get => Font.Italic;
            set => Font.Italic = value;
        }

        /// <summary>
        /// Gets or sets the underline property.
        /// </summary>
        public Underline Underline
        {
            get => Font.Underline;
            set => Font.Underline = value;
        }

        /// <summary>
        /// Gets or sets the color property.
        /// </summary>
        public Color Color
        {
            get => Font.Color;
            set => Font.Color = value;
        }

        /// <summary>
        /// Gets or sets the superscript property.
        /// </summary>
        public bool Superscript
        {
            get => Font.Superscript;
            set => Font.Superscript = value;
        }

        /// <summary>
        /// Gets or sets the subscript property.
        /// </summary>
        public bool Subscript
        {
            get => Font.Subscript;
            set => Font.Subscript = value;
        }

        /// <summary>
        /// Gets the collection of paragraph elements that defines the FormattedText.
        /// </summary>
        public ParagraphElements Elements
        {
            get => Values.Elements ??= new ParagraphElements(this);
            set
            {
                SetParent(value);
                Values.Elements = value;
            }
        }

        /// <summary>
        /// Converts FormattedText into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            bool isFormatted = false;
            if (!Values.Font.IsValueNullOrEmpty())
            {
                Font.Serialize(serializer);
                isFormatted = true;
            }
            else
            {
                if (Values.Style is not null)
                {
                    serializer.Write("\\font(\"" + Style + "\")");
                    isFormatted = true;
                }
            }

            if (isFormatted)
                serializer.Write("{");

            if (!Values.Elements.IsValueNullOrEmpty())
                Elements.Serialize(serializer);

            if (isFormatted)
                serializer.Write("}");
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitFormattedText(this);

            if (visitChildren)
                ((IVisitable?)Values.Elements)?.AcceptVisitor(visitor, true);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(FormattedText));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public FormattedTextValues Values => (FormattedTextValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class FormattedTextValues : Values
        {
            internal FormattedTextValues(DocumentObject owner) : base(owner)
            { }

            new FormattedText Owner => (FormattedText)base.Owner!;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Font? Font { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Style { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? FontName
            {
                get => Font?.Name;
                set => Owner.Font.Values.Name = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name
            {
                get => Font?.Name;
                set => Owner.Font.Values.Name = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Size
            {
                get => Font?.Size;
                set => Owner.Font.Values.Size = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Bold
            {
                get => Font?.Bold;
                set => Owner.Font.Values.Bold = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Italic
            {
                get => Font?.Italic;
                set => Owner.Font.Values.Italic = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Underline? Underline
            {
                get => Font?.Underline;
                set => Owner.Font.Values.Underline = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? Color
            {
                get => Font?.Color;
                set => Owner.Font.Values.Color = DocumentObjectModel.Color.MakeNullIfEmpty(value);
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Superscript
            {
                get => Font?.Superscript;
                set => Owner.Font.Values.Superscript = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Subscript
            {
                get => Font?.Subscript;
                set => Owner.Font.Values.Subscript = value;
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphElements? Elements { get; set; }
        }
    }
}
