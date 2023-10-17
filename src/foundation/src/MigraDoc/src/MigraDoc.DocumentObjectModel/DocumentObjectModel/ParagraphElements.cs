// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// A ParagraphElements collection contains the individual objects of a paragraph.
    /// </summary>
    public class ParagraphElements : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the ParagraphElements class.
        /// </summary>
        public ParagraphElements()
        {
            BaseValues = new ParagraphElementsValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the ParagraphElements class with the specified parent.
        /// </summary>
        internal ParagraphElements(DocumentObject parent) : base(parent)
        {
            BaseValues = new ParagraphElementsValues(this);
        }

        /// <summary>
        /// Gets a ParagraphElement by its index.
        /// </summary>
        public new DocumentObject? this[int index] => base[index];

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new ParagraphElements Clone()
            => (ParagraphElements)DeepCopy();

        /// <summary>
        /// Adds a Text object. The text may contain \n and \t, which results in more than one text objects to be added.
        /// The function returns the last text object that was created.
        /// </summary>
        /// <param name="text">Contents of the new Text objects.</param>
        /// <returns>Returns a new Text object with the last element of text that was added.</returns>
        public Text AddText(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            Text result = default!;
            string[] lines = text.Split('\n');
            int lineCount = lines.Length;
            for (int line = 0; line < lineCount; line++)
            {
                string[] tabParts = lines[line].Split('\t');
                int count = tabParts.Length;
                for (int idx = 0; idx < count; idx++)
                {
                    if (tabParts[idx].Length != 0)
                    {
                        Add(result = new(tabParts[idx]));
                    }
                    if (idx < count - 1)
                        AddTab();
                }
                if (line < lineCount - 1)
                    AddLineBreak();
            }
            return result;
        }

        /// <summary>
        /// Adds a single character repeated the specified number of times to the paragraph.
        /// </summary>
        public Text AddChar(char ch, int count)
            => AddText(new string(ch, count));

        /// <summary>
        /// Adds a single character to the paragraph.
        /// </summary>
        public Text AddChar(char ch) 
            => AddText(new string(ch, 1));

        /// <summary>
        /// Adds a Character object.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType) 
            => AddCharacter(symbolType, 1);

        /// <summary>
        /// Adds one or more Character objects.
        /// </summary>
        public Character AddCharacter(SymbolName symbolType, int count)
        {
            var character = new Character();
            Add(character);
            character.SymbolName = symbolType;
            character.Count = count;
            return character;
        }

        /// <summary>
        /// Adds a Character object defined by a character.
        /// </summary>
        public Character AddCharacter(char ch) 
            => AddCharacter((SymbolName)ch, 1);

        /// <summary>
        /// Adds one or more Character objects defined by a character.
        /// </summary>
        public Character AddCharacter(char ch, int count) 
            => AddCharacter((SymbolName)ch, count);

        /// <summary>
        /// Adds a space character as many as count.
        /// </summary>
        public Character AddSpace(int count) 
            => AddCharacter(SymbolName.Blank, count);

        /// <summary>
        /// Adds a horizontal tab.
        /// </summary>
        public Character AddTab() 
            => AddCharacter(SymbolName.Tab, 1);

        /// <summary>
        /// Adds a line break.
        /// </summary>
        public Character AddLineBreak() 
            => AddCharacter(SymbolName.LineBreak, 1);

        /// <summary>
        /// Adds a new FormattedText.
        /// </summary>
        public FormattedText AddFormattedText()
        {
            var formattedText = new FormattedText();
            Add(formattedText);
            return formattedText;
        }

        /// <summary>
        /// Adds a new FormattedText object with the given format.
        /// </summary>
        public FormattedText AddFormattedText(TextFormat textFormat)
        {
            var formattedText = AddFormattedText();

            if ((textFormat & TextFormat.Bold) == TextFormat.Bold)
                formattedText.Bold = true;
            if ((textFormat & TextFormat.NotBold) == TextFormat.NotBold)
                formattedText.Bold = false;
            if ((textFormat & TextFormat.Italic) == TextFormat.Italic)
                formattedText.Italic = true;
            if ((textFormat & TextFormat.NotItalic) == TextFormat.NotItalic)
                formattedText.Italic = false;
            if ((textFormat & TextFormat.Underline) == TextFormat.Underline)
                formattedText.Underline = Underline.Single;
            if ((textFormat & TextFormat.NoUnderline) == TextFormat.NoUnderline)
                formattedText.Underline = Underline.None;

            return formattedText;
        }

        /// <summary>
        /// Adds a new FormattedText with the given Font.
        /// </summary>
        public FormattedText AddFormattedText(Font font)
        {
            var formattedText = new FormattedText();
            formattedText.Font.ApplyFont(font);
            Add(formattedText);
            return formattedText;
        }

        /// <summary>
        /// Adds a new FormattedText with the given text.
        /// </summary>
        public FormattedText AddFormattedText(string text)
        {
            var formattedText = new FormattedText();
            formattedText.AddText(text);
            Add(formattedText);
            return formattedText;
        }

        /// <summary>
        /// Adds a new FormattedText object with the given text and format.
        /// </summary>
        public FormattedText AddFormattedText(string text, TextFormat textFormat)
        {
            var formattedText = AddFormattedText(textFormat);
            formattedText.AddText(text);
            return formattedText;
        }

        /// <summary>
        /// Adds a new FormattedText object with the given text and font.
        /// </summary>
        public FormattedText AddFormattedText(string text, Font font)
        {
            var formattedText = AddFormattedText(font);
            formattedText.AddText(text);
            return formattedText;
        }

        /// <summary>
        /// Adds a new FormattedText object with the given text and style.
        /// </summary>
        public FormattedText AddFormattedText(string text, string style)
        {
            var formattedText = AddFormattedText(text);
            formattedText.Style = style;
            return formattedText;
        }

        /// <summary>
        /// Adds a new Hyperlink of Type "Local", i.e. the target is a Bookmark within the Document.
        /// </summary>
        public Hyperlink AddHyperlink(string bookmarkName)
        {
            var hyperlink = new Hyperlink
            {
                BookmarkName = bookmarkName
            };
            Add(hyperlink);
            return hyperlink;
        }

        /// <summary>
        /// Adds a new Hyperlink.
        /// </summary>
        public Hyperlink AddHyperlink(string name, HyperlinkType type)
        {
            if (type == HyperlinkType.Bookmark)
                return AddHyperlink(name);

            if (type is HyperlinkType.ExternalBookmark or HyperlinkType.EmbeddedDocument)
                throw new NotSupportedException("No bookmarkName defined. " +
                                                "Please use AddHyperlink(string filename, string bookmarkName, bool? newWindow) " +
                                                "or one of the AddHyperlinkToEmbeddedDocument() functions.");

            // HyperlinkTypes File and Web/Url:
            var hyperlink = new Hyperlink
            {
                Filename = name,
                Type = type
            };
            Add(hyperlink);
            return hyperlink;
        }

        /// <summary>
        /// Adds a new Hyperlink using the mailto protocol. The protocol is automatically added if missing.
        /// </summary>
        public Hyperlink AddMailLink(string url)
        {
            var hyperlink = Hyperlink.CreateMailLink(url);
            Add(hyperlink);
            return hyperlink;
        }

        /// <summary>
        /// Adds a new Hyperlink using the HTTP or HTTPS protocol. The HTTP protocol is automatically added if neither HTTP nor HTTPS is specified.
        /// </summary>
        public Hyperlink AddWebLink(string url)
        {
            var hyperlink = Hyperlink.CreateWebLink(url);
            Add(hyperlink);
            return hyperlink;
        }

        /// <summary>
        /// Adds a new Hyperlink of Type "ExternalBookmark", i.e. the target is a Bookmark in an external PDF Document.
        /// </summary>
        /// <param name="filename">The path to the target document.</param>
        /// <param name="bookmarkName">The Named Destination's name in the target document.</param>
        /// <param name="newWindow">Defines if the HyperlinkType ExternalBookmark shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public Hyperlink AddHyperlink(string filename, string bookmarkName, HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
        {
            var hyperlink = new Hyperlink
            {
                Filename = filename,
                BookmarkName = bookmarkName,
                NewWindow = newWindow,
                Type = HyperlinkType.ExternalBookmark
            };
            Add(hyperlink);
            return hyperlink;
        }

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
            => AddHyperlinkToEmbeddedDocument("", destinationPath, newWindow);

        /// <summary>
        /// Adds a new Hyperlink of Type "EmbeddedDocument".
        /// The target is a Bookmark in an embedded Document in an external PDF Document.
        /// </summary>
        /// <param name="filename">The path to the target document. Can be empty if target is an embedded document in the current document.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents in the target document.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the root document to the embedded document.
        /// Each segment name refers to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">Defines if the HyperlinkType ExternalBookmark shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public Hyperlink AddHyperlinkToEmbeddedDocument(string filename, string destinationPath, HyperlinkTargetWindow newWindow = HyperlinkTargetWindow.UserPreference)
        {
            var hyperlink = new Hyperlink
            {
                Name = filename,
                BookmarkName = destinationPath,
                NewWindow = newWindow,
                Type = HyperlinkType.EmbeddedDocument
            };
            Add(hyperlink);
            return hyperlink;
        }

        /// <summary>
        /// Adds a new Bookmark.
        /// </summary>
        /// <param name="name">The name of the bookmark.</param>
        /// <param name="prepend">True, if the bookmark shall be inserted at the beginning of the paragraph.</param>
        public BookmarkField AddBookmark(string name, bool prepend = true)
        {
            var fieldBookmark = new BookmarkField
            {
                Name = name
            };
            if (prepend)
                InsertObject(0, fieldBookmark);
            else
                Add(fieldBookmark);
            return fieldBookmark;
        }

        /// <summary>
        /// Adds a new PageField.
        /// </summary>
        public PageField AddPageField()
        {
            var fieldPage = new PageField();
            Add(fieldPage);
            return fieldPage;
        }

        /// <summary>
        /// Adds a new RefFieldPage.
        /// </summary>
        public PageRefField AddPageRefField(string name)
        {
            var fieldPageRef = new PageRefField
            {
                Name = name
            };
            Add(fieldPageRef);
            return fieldPageRef;
        }

        /// <summary>
        /// Adds a new NumPagesField.
        /// </summary>
        public NumPagesField AddNumPagesField()
        {
            var fieldNumPages = new NumPagesField();
            Add(fieldNumPages);
            return fieldNumPages;
        }

        /// <summary>
        /// Adds a new SectionField.
        /// </summary>
        public SectionField AddSectionField()
        {
            var fieldSection = new SectionField();
            Add(fieldSection);
            return fieldSection;
        }

        /// <summary>
        /// Adds a new SectionPagesField.
        /// </summary>
        public SectionPagesField AddSectionPagesField()
        {
            var fieldSectionPages = new SectionPagesField();
            Add(fieldSectionPages);
            return fieldSectionPages;
        }

        /// <summary>
        /// Adds a new DateField.
        /// </summary>
        /// 
        public DateField AddDateField()
        {
            var fieldDate = new DateField();
            Add(fieldDate);
            return fieldDate;
        }

        /// <summary>
        /// Adds a new DateField with the given format.
        /// </summary>
        public DateField AddDateField(string format)
        {
            var fieldDate = new DateField
            {
                Format = format
            };
            Add(fieldDate);
            return fieldDate;
        }

        /// <summary>
        /// Adds a new InfoField with the given type.
        /// </summary>
        public InfoField AddInfoField(InfoFieldType iType)
        {
            var fieldInfo = new InfoField
            {
                Name = iType.ToString()
            };
            Add(fieldInfo);
            return fieldInfo;
        }

        /// <summary>
        /// Adds a new Footnote with the specified Text.
        /// </summary>
        public Footnote AddFootnote(string text)
        {
            var footnote = new Footnote();
            var par = footnote.Elements.AddParagraph();
            par.AddText(text);
            Add(footnote);
            return footnote;
        }

        /// <summary>
        /// Adds a new Footnote.
        /// </summary>
        public Footnote AddFootnote()
        {
            var footnote = new Footnote();
            Add(footnote);
            return footnote;
        }

        /// <summary>
        /// Adds a new Image.
        /// </summary>
        public Image AddImage(string name)
        {
            var image = new Image
            {
                Name = name
            };
            Add(image);
            return image;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Add(DocumentObject? docObj) 
            => base.Add(docObj);

        /// <summary>
        /// Converts ParagraphElements into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            int count = Count;
            for (int index = 0; index < count; ++index)
            {
                DocumentObject element = this[index]!;
                element.Serialize(serializer);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(ParagraphElements));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public ParagraphElementsValues Values => (ParagraphElementsValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ParagraphElementsValues : Values
        {
            internal ParagraphElementsValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}
