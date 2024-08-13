// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A Hyperlink is used to reference targets in the document (Local), on a drive (File) or a network (Web).
    /// </summary>
    public class Hyperlink : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Hyperlink class.
        /// </summary>
        public Hyperlink()
        {
            BaseValues = new HyperlinkValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Hyperlink class with the specified parent.
        /// </summary>
        internal Hyperlink(DocumentObject parent) : base(parent)
        {
            BaseValues = new HyperlinkValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Hyperlink class with the text the hyperlink shall content.
        /// The type will be treated as Local by default.
        /// </summary>
        internal Hyperlink(string bookmarkName, string text) : this()
        {
            BookmarkName = bookmarkName;
            Elements.AddText(text);
        }

        /// <summary>
        /// Initializes a new instance of the Hyperlink class with the type and text the hyperlink shall
        /// represent.
        /// </summary>
        internal Hyperlink(string bookmarkName, string filename, HyperlinkType type, string text)
            : this()
        {
            BookmarkName = bookmarkName;
            Filename = filename;
            Type = type;
            Elements.AddText(text);
        }

        /// <summary>
        /// Initializes a new instance of the Hyperlink class with the type "Url" and text the hyperlink shall
        /// represent. The mailto protocol is automatically added if missing.
        /// </summary>
        public static Hyperlink CreateMailLink(string url)
        {
            const string protocol = "mailto:";

            var hyperlink = new Hyperlink();
            if (!url.StartsWith(protocol, StringComparison.OrdinalIgnoreCase))
                url = protocol + url;
            hyperlink.Filename = url;
            hyperlink.Type = HyperlinkType.Url;

            return hyperlink;
        }

        /// <summary>
        /// Initializes a new instance of the Hyperlink class with the type "Url" and text the hyperlink shall
        /// represent. The HTTP protocol is automatically added if neither HTTP nor HTTPS is specified.
        /// </summary>
        public static Hyperlink CreateWebLink(string url)
        {
            // Header values like subject, cc, bcc and body may be included in the url.
            // A further release could provide parameters for them and include them in the url.
            const string protocolHttp = "http://";
            const string protocolHttps = "https://";

            var hyperlink = new Hyperlink();
            if (!url.StartsWith(protocolHttp, StringComparison.OrdinalIgnoreCase) && !url.StartsWith(protocolHttps, StringComparison.OrdinalIgnoreCase))
                url = protocolHttp + url;
            hyperlink.Filename = url;
            hyperlink.Type = HyperlinkType.Url;

            return hyperlink;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Hyperlink Clone()
            => (Hyperlink)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            Hyperlink hyperlink = (Hyperlink)base.DeepCopy();
            if (hyperlink.Values.Elements is not null)
            {
                hyperlink.Values.Elements = hyperlink.Values.Elements.Clone();
                hyperlink.Values.Elements.Parent = hyperlink;
            }
            return hyperlink;
        }

        /// <summary>
        /// Adds a text phrase to the hyperlink.
        /// </summary>
        public Text AddText(string text)
        {
            return Elements.AddText(text);
        }

        /// <summary>
        /// Adds a single character repeated the specified number of times to the hyperlink.
        /// </summary>
        public Text AddChar(char ch, int count)
        {
            return Elements.AddChar(ch, count);
        }

        /// <summary>
        /// Adds a single character to the hyperlink.
        /// </summary>
        public Text AddChar(char ch)
        {
            return Elements.AddChar(ch);
        }

        /// <summary>
        /// Adds one or more Symbol objects.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType, int count)
        {
            return Elements.AddCharacter(symbolType, count);
        }

        /// <summary>
        /// Adds a Symbol object.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType)
        {
            return Elements.AddCharacter(symbolType);
        }

        /// <summary>
        /// Adds one or more Symbol objects defined by a character.
        /// </summary>
        public Character AddCharacter(char ch, int count)
        {
            return Elements.AddCharacter(ch, count);
        }

        /// <summary>
        /// Adds a Symbol object defined by a character.
        /// </summary>
        public Character AddCharacter(char ch)
        {
            return Elements.AddCharacter(ch);
        }

        /// <summary>
        /// Adds a space character as many as count.
        /// </summary>
        public Character AddSpace(int count)
        {
            return Elements.AddSpace(count);
        }

        /// <summary>
        /// Adds a horizontal tab.
        /// </summary>
        public void AddTab()
        {
            Elements.AddTab();
        }

        /// <summary>
        /// Adds a new FormattedText.
        /// </summary>
        public FormattedText AddFormattedText()
        {
            return Elements.AddFormattedText();
        }

        /// <summary>
        /// Adds a new FormattedText object with the given format.
        /// </summary>
        public FormattedText AddFormattedText(TextFormat textFormat)
        {
            return Elements.AddFormattedText(textFormat);
        }

        /// <summary>
        /// Adds a new FormattedText with the given Font.
        /// </summary>
        public FormattedText AddFormattedText(Font font)
        {
            return Elements.AddFormattedText(font);
        }

        /// <summary>
        /// Adds a new FormattedText with the given text.
        /// </summary>
        public FormattedText AddFormattedText(string text)
        {
            return Elements.AddFormattedText(text);
        }

        /// <summary>
        /// Adds a new FormattedText object with the given text and format.
        /// </summary>
        public FormattedText AddFormattedText(string text, TextFormat textFormat)
        {
            return Elements.AddFormattedText(text, textFormat);
        }

        /// <summary>
        /// Adds a new FormattedText object with the given text and font.
        /// </summary>
        public FormattedText AddFormattedText(string text, Font font)
        {
            return Elements.AddFormattedText(text, font);
        }

        /// <summary>
        /// Adds a new FormattedText object with the given text and style.
        /// </summary>
        public FormattedText AddFormattedText(string text, string style)
        {
            return Elements.AddFormattedText(text, style);
        }

        /// <summary>
        /// Adds a new Bookmark.
        /// </summary>
        /// <param name="name">The name of the bookmark.</param>
        /// <param name="prepend">True, if the bookmark shall be inserted at the beginning of the paragraph.</param>
        public BookmarkField AddBookmark(string name, bool prepend = true)
        {
            return Elements.AddBookmark(name, prepend);
        }

        /// <summary>
        /// Adds a new PageField.
        /// </summary>
        public PageField AddPageField()
        {
            return Elements.AddPageField();
        }

        /// <summary>
        /// Adds a new PageRefField.
        /// </summary>
        public PageRefField AddPageRefField(string name)
        {
            return Elements.AddPageRefField(name);
        }

        /// <summary>
        /// Adds a new NumPagesField.
        /// </summary>
        public NumPagesField AddNumPagesField()
        {
            return Elements.AddNumPagesField();
        }

        /// <summary>
        /// Adds a new SectionField.
        /// </summary>
        public SectionField AddSectionField()
        {
            return Elements.AddSectionField();
        }

        /// <summary>
        /// Adds a new SectionPagesField.
        /// </summary>
        public SectionPagesField AddSectionPagesField()
        {
            return Elements.AddSectionPagesField();
        }

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public DateField AddDateField()
        {
            return Elements.AddDateField();
        }

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public DateField AddDateField(string format)
        {
            return Elements.AddDateField(format);
        }

        /// <summary>
        /// Adds a new InfoField.
        /// </summary>
        public InfoField AddInfoField(InfoFieldType iType)
        {
            return Elements.AddInfoField(iType);
        }

        /// <summary>
        /// Adds a new Footnote with the specified text.
        /// </summary>
        public Footnote AddFootnote(string text)
        {
            return Elements.AddFootnote(text);
        }

        /// <summary>
        /// Adds a new Footnote.
        /// </summary>
        public Footnote AddFootnote()
        {
            return Elements.AddFootnote();
        }

        /// <summary>
        /// Adds a new Image object.
        /// </summary>
        public Image AddImage(string fileName)
        {
            return Elements.AddImage(fileName);
        }
        /// <summary>
        /// Adds a new Bookmark.
        /// </summary>
        public void Add(BookmarkField bookmark)
        {
            Elements.Add(bookmark);
        }

        /// <summary>
        /// Adds a new PageField.
        /// </summary>
        public void Add(PageField pageField)
        {
            Elements.Add(pageField);
        }

        /// <summary>
        /// Adds a new PageRefField.
        /// </summary>
        public void Add(PageRefField pageRefField)
        {
            Elements.Add(pageRefField);
        }

        /// <summary>
        /// Adds a new NumPagesField.
        /// </summary>
        public void Add(NumPagesField numPagesField)
        {
            Elements.Add(numPagesField);
        }

        /// <summary>
        /// Adds a new SectionField.
        /// </summary>
        public void Add(SectionField sectionField)
        {
            Elements.Add(sectionField);
        }

        /// <summary>
        /// Adds a new SectionPagesField.
        /// </summary>
        public void Add(SectionPagesField sectionPagesField)
        {
            Elements.Add(sectionPagesField);
        }

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        public void Add(DateField dateField)
        {
            Elements.Add(dateField);
        }

        /// <summary>
        /// Adds a new InfoField.
        /// </summary>
        public void Add(InfoField infoField)
        {
            Elements.Add(infoField);
        }

        /// <summary>
        /// Adds a new Footnote.
        /// </summary>
        public void Add(Footnote footnote)
        {
            Elements.Add(footnote);
        }

        /// <summary>
        /// Adds a new Text.
        /// </summary>
        public void Add(Text text)
        {
            Elements.Add(text);
        }

        /// <summary>
        /// Adds a new FormattedText.
        /// </summary>
        public void Add(FormattedText formattedText)
        {
            Elements.Add(formattedText);
        }

        /// <summary>
        /// Adds a new Image.
        /// </summary>
        public void Add(Image image)
        {
            Elements.Add(image);
        }

        /// <summary>
        /// Adds a new Character.
        /// </summary>
        public void Add(Character character)
        {
            Elements.Add(character);
        }

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
        /// If set to true, the Hyperlink shall look like the surrounding text without using the Hyperlink Style.
        /// May be used for text references, e.g. in tables, which shall not be rendered as links. 
        /// </summary>
        public bool NoHyperlinkStyle
        {
            // This Property is not part of Word’s Automation Model. With this property Hyperlinks can be used without implicitly setting the Hyperlink style.
            // This way they occur like linked cross references in Word, which are not implemented as an own DocumentObject type in MigraDoc.
            get => Values.NoHyperlinkStyle ?? false;
            set => Values.NoHyperlinkStyle = value;
        }

        /// <summary>
        /// For HyperlinkType Local/Bookmark: Gets or sets the target bookmark name of the Hyperlink.
        /// For HyperlinkTypes File and Url/Web: Gets or sets the target filename of the Hyperlink, e.g. a path to a file or an URL.
        /// For HyperlinkType ExternalBookmark: Not valid - throws Exception.
        /// This property is retained due to compatibility reasons.
        /// </summary>
        public string Name
        {
            get
            {
                switch (Type)
                {
                    case HyperlinkType.ExternalBookmark:
                        throw new InvalidOperationException("For HyperlinkType ExternalBookmark Filename and BookmarkName must be set. Use these properties instead.");
                    case HyperlinkType.File:
                    case HyperlinkType.Url:
                        return Values.Filename ?? "";
                    default:
                        // case HyperlinkType.Bookmark:
                        return Values.BookmarkName ?? "";
                }
            }
            set
            {
                switch (Type)
                {
                    case HyperlinkType.ExternalBookmark:
                        throw new InvalidOperationException("For HyperlinkType ExternalBookmark Filename and BookmarkName must be set. Use these properties instead.");
                    case HyperlinkType.File:
                    case HyperlinkType.Url:
                        Values.Filename = value;
                        break;
                    default:
                        // case HyperlinkType.Bookmark:
                        Values.BookmarkName = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the target filename of the Hyperlink, e.g. a path to a file or an URL.
        /// Used for HyperlinkTypes ExternalBookmark, File and Url/Web.
        /// </summary>
        public string Filename
        {
            get => Values.Filename ?? "";
            set => Values.Filename = value;
        }

        /// <summary>
        /// Gets or sets the target bookmark name of the Hyperlink.
        ///Used for HyperlinkTypes ExternalBookmark and Bookmark.
        /// </summary>
        public string BookmarkName
        {
            get => Values.BookmarkName ?? "";
            set => Values.BookmarkName = value;
        }

        /// <summary>
        /// Defines if the HyperlinkType ExternalBookmark shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.
        /// </summary>
        public HyperlinkTargetWindow NewWindow
        {
            get => Values.NewWindow ?? HyperlinkTargetWindow.UserPreference;
            set => Values.NewWindow = value;
        }

        /// <summary>
        /// Gets or sets the target type of the Hyperlink.
        /// </summary>
        public HyperlinkType Type
        {
            get => Values.Type ?? 0;
            set
            {
                switch (value)
                {
                    case HyperlinkType.File:
                    case HyperlinkType.Url:
                        if (!String.IsNullOrEmpty(Values.BookmarkName) && String.IsNullOrEmpty(Values.Filename))
                            throw new InvalidOperationException("For HyperlinkTypes File and Web/Url Filename must be set instead of BookmarkName.");
                        break;
                    case HyperlinkType.Bookmark:
                        if (!String.IsNullOrEmpty(Values.Filename) && String.IsNullOrEmpty(Values.BookmarkName))
                            throw new InvalidOperationException("For HyperlinkType Local/Bookmark BookmarkName must be set instead of Filename.");
                        break;
                }

                Values.Type = value;
            }
        }

        /// <summary>
        /// Gets the ParagraphElements of the Hyperlink specifying its 'clickable area'.
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
        /// Converts Hyperlink into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.Write("\\hyperlink");
            var str = "[";

            if (Type is HyperlinkType.ExternalBookmark or HyperlinkType.File or HyperlinkType.Url)
            {
                if (String.IsNullOrEmpty(Values.Filename))
                    throw new InvalidOperationException(MdDomMsgs.MissingObligatoryProperty(nameof(Filename), $"Hyperlink {Type.ToString()}").Message);

                str += " Filename = \"" + Filename.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            }
            if (Type is HyperlinkType.ExternalBookmark or HyperlinkType.Bookmark or HyperlinkType.EmbeddedDocument)
            {
                if (String.IsNullOrEmpty(Values.BookmarkName))
                    throw new InvalidOperationException(MdDomMsgs.MissingObligatoryProperty(nameof(BookmarkName), $"Hyperlink {Type.ToString()}").Message);

                str += " BookmarkName = \"" + BookmarkName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            }
            if (Type is HyperlinkType.ExternalBookmark or HyperlinkType.EmbeddedDocument)
            {
                str += " NewWindow = " + NewWindow;
            }

            if (Values.Type is not null)
                str += " Type = " + Type;
            str += " ]";
            serializer.Write(str);
            serializer.Write("{");
            if (Values.Elements is not null)
                Values.Elements.Serialize(serializer);
            serializer.Write("}");
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        public void AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitHyperlink(this);
            if (visitChildren && Values.Elements is not null)
            {
                ((IVisitable)Values.Elements).AcceptVisitor(visitor, true);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Hyperlink));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public HyperlinkValues Values => (HyperlinkValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class HyperlinkValues : Values
        {
            internal HyperlinkValues(DocumentObject owner) : base(owner)
            { }

            new Hyperlink Owner => (Hyperlink)base.Owner!;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Font? Font { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? NoHyperlinkStyle { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name
            {
                get => Owner.Name;
                set => Owner.Name = value ?? ""; // BUG??? "null" becomes "".
            }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Filename { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? BookmarkName { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HyperlinkTargetWindow? NewWindow { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public HyperlinkType? Type { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphElements? Elements { get; set; }
        }
    }
}
