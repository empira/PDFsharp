// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable CommentTypo because of too much token strings in comments
// ReSharper disable GrammarMistakeInComment

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// A simple hand-coded parser for MigraDoc DDL.
    /// </summary>
    class DdlParser
    {
        /// <summary>
        /// Initializes a new instance of the DdlParser class.
        /// </summary>
        internal DdlParser(string ddl, DdlReaderErrors? errors)
            : this("", ddl, errors)
        { }

        /// <summary>
        /// Initializes a new instance of the DdlParser class.
        /// </summary>
        internal DdlParser(string fileName, string ddl, DdlReaderErrors? errors)
        {
            _errors = errors ?? new DdlReaderErrors();
            _scanner = new DdlScanner(fileName, ddl, _errors);
        }

        /// <summary>
        /// Parses the keyword «\document».
        /// </summary>
        internal Document ParseDocument(Document? document)
        {
            document ??= new Document();

            MoveToCode();
            EnsureSymbol(Symbol.Document);
            ReadCode();
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(document);

            EnsureSymbol(Symbol.BraceLeft);
            ReadCode();

            while (Symbol == Symbol.EmbeddedFile)
                ParseEmbeddedFiles(document.EmbeddedFiles);

            if (Symbol == Symbol.Styles)
                ParseStyles(document.Styles);

            // A document with no sections is valid and has zero pages.
            while (Symbol == Symbol.Section)
                ParseSection(document.Sections);

            EnsureSymbol(Symbol.BraceRight);
            ReadCode();
            EnsureCondition(Symbol == Symbol.Eof, () => MdDomMsgs.EndOfFileExpected);

            return document;
        }

        /// <summary>
        /// Parses one of the keywords «\document», «\styles», «\section», «\table», «\textframe», «\chart»
        /// and «\paragraph» and returns the corresponding DocumentObject or DocumentObjectCollection.
        /// </summary>
        internal DocumentObject? ParseDocumentObject()
        {
            DocumentObject? obj = null;

            MoveToCode();
            switch (Symbol)
            {
                case Symbol.Document:
                    obj = ParseDocument(null);
                    break;

                case Symbol.EmbeddedFile:
                    obj = ParseEmbeddedFiles([] /*new EmbeddedFiles()*/);
                    break;

                case Symbol.Styles:
                    obj = ParseStyles([] /*new Styles()*/);
                    break;

                case Symbol.Section:
                    obj = ParseSection([] /*new Sections()*/);
                    break;

                case Symbol.Table:
                    obj = new Table();
                    ParseTable(null, (Table)obj);
                    break;

                case Symbol.TextFrame:
                    var elems = new DocumentElements();
                    ParseTextFrame(elems);
                    obj = elems[0];
                    break;

                case Symbol.Chart:
                    throw new NotImplementedException();

                case Symbol.Paragraph:
                    obj = new DocumentElements();
                    ParseParagraph((DocumentElements)obj);
                    break;

                default:
                    ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
                    break;
            }
            ReadCode();
            EnsureCondition(Symbol == Symbol.Eof, () => MdDomMsgs.EndOfFileExpected);

            return obj;
        }

        /// <summary>
        /// Parses the keyword «\styles».
        /// </summary>
        Styles ParseStyles(Styles styles)
        {
            MoveToCode();
            EnsureSymbol(Symbol.Styles);

            ReadCode();  // read '{'
            EnsureSymbol(Symbol.BraceLeft);

            ReadCode();  // read first style name
            // An empty \styles block is valid.
            while (Symbol is Symbol.Identifier or Symbol.StringLiteral)
                ParseStyleDefinition(styles);

            EnsureSymbol(Symbol.BraceRight);
            ReadCode();  // read beyond '}'

            return styles;
        }

        /// <summary>
        /// Parses a style definition block within the keyword «\styles».
        /// </summary>
        Style? ParseStyleDefinition(Styles styles)
        {
            //   StyleName [: BaseStyleName]
            //   {
            //     ...
            //   }
            Style? style = null;
            try
            {
                var styleName = Token;
                string? baseStyleName = null;

                if (Symbol != Symbol.Identifier && Symbol != Symbol.StringLiteral)
                    ThrowParserException(MdDomMsgs.StyleNameExpected(styleName));

                ReadCode();

                if (Symbol == Symbol.Colon)
                {
                    ReadCode();
                    if (Symbol != Symbol.Identifier && Symbol != Symbol.StringLiteral)
                        ThrowParserException(MdDomMsgs.StyleNameExpected(styleName));

                    // If baseStyle is not valid, choose InvalidStyleName by default.
                    baseStyleName = Token;
                    if (styles.GetIndex(baseStyleName) == -1)
                    {
                        ReportParserInfo(DdlErrorLevel.Warning, MdDomMsgs.UseOfUndefinedBaseStyle(baseStyleName));
                        baseStyleName = StyleNames.InvalidStyleName;
                    }

                    ReadCode();
                }

                // Get or create style.
                style = styles[styleName];
                if (style != null)
                {
                    // Reset base style.
                    if (baseStyleName != null)
                        style.BaseStyle = baseStyleName;
                }
                else
                {
                    // Style does not exist and no base style is given, choose InvalidStyleName by default.
                    if (String.IsNullOrEmpty(baseStyleName))
                    {
                        baseStyleName = StyleNames.InvalidStyleName;
                        ReportParserInfo(DdlErrorLevel.Warning, MdDomMsgs.UseOfUndefinedStyle(styleName));
                    }
                    style = styles.AddStyle(styleName, baseStyleName);
                }

                // Parse definition (if any).
                if (Symbol == Symbol.BraceLeft)
                {
                    ParseAttributeBlock(style);
                }
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
            return style;
        }

        /// <summary>
        /// Determines if the current symbol is a header or footer.
        /// </summary>
        bool IsHeaderFooter()
        {
            var sym = Symbol;
            return sym is Symbol.Header or Symbol.Footer
                or Symbol.PrimaryHeader or Symbol.PrimaryFooter
                or Symbol.EvenPageHeader or Symbol.EvenPageFooter
                or Symbol.FirstPageHeader or Symbol.FirstPageFooter;
        }

        /// <summary>
        /// Parses the keyword «\EmbeddedFiles».
        /// </summary>
        EmbeddedFiles ParseEmbeddedFiles(EmbeddedFiles embeddedFiles)
        {
            Debug.Assert(embeddedFiles != null);

            MoveToCode();
            EnsureSymbol(Symbol.EmbeddedFile);

            try
            {
                var embeddedFile = new EmbeddedFile();

                ReadCode(); // read '['
                ParseAttributes(embeddedFile);

                embeddedFiles.Add(embeddedFile);
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
            return embeddedFiles;
        }

        /// <summary>
        /// Parses the keyword «\section».
        /// </summary>
        Section? ParseSection(Sections sections)
        {
            Debug.Assert(sections != null);

            MoveToCode();
            EnsureSymbol(Symbol.Section);

            Section? section = null;
            try
            {
                section = sections.AddSection();

                ReadCode(); // read '[' or '{'
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(section);

                EnsureSymbol(Symbol.BraceLeft);

            ReadMoreContent:
                // If a section contains only one paragraph, the paragraph keyword is omitted in MDDDL and the paragraph content is directly inserted into the section.
                // The IsParagraphContent() check has to be made, before moving to the next token by ReadCode().
                if (IsParagraphContent()) 
                {
                    // Paragraph content was inserted directly into the section.
                    var paragraph = section.Elements.AddParagraph();
                    ParseParagraphContent(section.Elements, paragraph);
                    // After paragraph content, there must not be any HeaderFooter or other document elements in the section.
                }
                else
                {
                    // Read next token to check for HeaderFooter or the section’s document elements.
                    ReadCode();

                    if (IsHeaderFooter())
                    {
                        ParseHeaderFooter(section);

                        // Headers and footers may be followed by other headers and footers, paragraph content or document elements.
                        goto ReadMoreContent;
                    }
                    ParseDocumentElements(section.Elements, Symbol.Section);
                }

                EnsureSymbol(Symbol.BraceRight);
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
            return section;
        }

        /// <summary>
        /// Parses the keywords «\header».
        /// Doesn’t move to next token as paragraph content may follow, which has to be checked before moving to the next token.
        /// </summary>
        void ParseHeaderFooter(Section section)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            try
            {
                Symbol hdrFtrSym = Symbol;
                bool isHeader = hdrFtrSym == Symbol.Header ||
                  hdrFtrSym == Symbol.PrimaryHeader ||
                  hdrFtrSym == Symbol.FirstPageHeader ||
                  hdrFtrSym == Symbol.EvenPageHeader;

                // Recall that the styles "Header" resp. "Footer" are used as default if
                // no other style was given. But this belongs to the rendering process,
                // not to the DDL parser. Therefore, no code here belongs to that.
                var headerFooter = new HeaderFooter();
                ReadCode(); // read '[' or '{'
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(headerFooter);

                EnsureSymbol(Symbol.BraceLeft);
                if (IsParagraphContent())
                {
                    Paragraph paragraph = headerFooter.Elements.AddParagraph();
                    ParseParagraphContent(headerFooter.Elements, paragraph);
                }
                else
                {
                    ReadCode(); // parse '{'
                    ParseDocumentElements(headerFooter.Elements, Symbol.HeaderOrFooter);
                }
                EnsureSymbol(Symbol.BraceRight);

                HeadersFooters headersFooters = isHeader ? section.Headers : section.Footers;
                if (hdrFtrSym is Symbol.Header or Symbol.Footer)
                {
                    headersFooters.Primary = headerFooter.Clone();
                    headersFooters.EvenPage = headerFooter.Clone();
                    headersFooters.FirstPage = headerFooter.Clone();
                }
                else
                {
                    switch (hdrFtrSym)
                    {
                        case Symbol.PrimaryHeader:
                        case Symbol.PrimaryFooter:
                            headersFooters.Primary = headerFooter;
                            break;

                        case Symbol.EvenPageHeader:
                        case Symbol.EvenPageFooter:
                            headersFooters.EvenPage = headerFooter;
                            break;

                        case Symbol.FirstPageHeader:
                        case Symbol.FirstPageFooter:
                            headersFooters.FirstPage = headerFooter;
                            break;
                    }
                }
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Determines whether the next text is paragraph content or document element.
        /// </summary>
        bool IsParagraphContent()
        {
            if (MoveToParagraphContent())
            {
                if (_scanner.Char == Chars.BackSlash)
                {
                    Symbol symbol = _scanner.PeekKeyword();
                    return symbol switch
                    {
                        Symbol.Bold => true,
                        Symbol.Italic => true,
                        Symbol.Underline => true,
                        Symbol.Field => true,
                        Symbol.Font => true,
                        Symbol.FontColor => true,
                        Symbol.FontSize => true,
                        Symbol.Footnote => true,
                        Symbol.Hyperlink => true,
                        Symbol.Symbol => true,
                        Symbol.Chr => true,
                        Symbol.Tab => true,
                        Symbol.LineBreak => true,
                        Symbol.Space => true,
                        Symbol.SoftHyphen => true,
                        _ => false
                    };
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parses the document elements of a «\paragraph», «\cell» or comparable.
        /// </summary>
#pragma warning disable IDE0060
        DocumentElements ParseDocumentElements(DocumentElements elements, Symbol context)
#pragma warning restore IDE0060
        {
            //
            // This is clear:
            //   \section { Hallo World! }
            // All section content will be treated as paragraph content.
            //
            // but this is ambiguous:
            //   \section { \image(...) }
            // It could be an image inside a paragraph or at the section level.
            // In this case it will be treated as an image on section level.
            //
            // If this is not your intention it must be like this:
            //   \section { \paragraph { \image(...) } }
            //

            while (TokenType == TokenType.KeyWord)
            {
                switch (Symbol)
                {
                    case Symbol.Paragraph:
                        ParseParagraph(elements);
                        break;

                    case Symbol.PageBreak:
                        ParsePageBreak(elements);
                        break;

                    case Symbol.Table:
                        ParseTable(elements, null);
                        break;

                    case Symbol.TextFrame:
                        ParseTextFrame(elements);
                        break;

                    case Symbol.Image:
                        ParseImage(elements.AddImage(""), false);
                        break;

                    case Symbol.Chart:
                        ParseChart(elements);
                        break;

                    case Symbol.Barcode:
                        ParseBarcode(elements);
                        break;

                    default:
                        ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
                        break;
                }
            }
            return elements;
        }

        /// <summary>
        /// Parses the keyword «\paragraph».
        /// </summary>
        void ParseParagraph(DocumentElements elements)
        {
            MoveToCode();
            EnsureSymbol(Symbol.Paragraph);

            Paragraph paragraph = elements.AddParagraph();
            try
            {
                ReadCode(); // read '[' or '{'
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(paragraph);

                // Empty paragraphs without braces are valid.
                if (Symbol == Symbol.BraceLeft)
                {
                    ParseParagraphContent(elements, paragraph);
                    EnsureSymbol(Symbol.BraceRight);
                    ReadCode(); // read beyond '}'
                }
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the inner text of a paragraph, i.e. stops on BraceRight and treats empty
        /// line as paragraph separator.
        /// </summary>
        void ParseParagraphContent(DocumentElements elements, Paragraph? paragraph)
        {
            var para = paragraph ?? elements.AddParagraph();

            while (para != null)
            {
                ParseFormattedText(para.Elements, 0);
                if (Symbol != Symbol.BraceRight && Symbol != Symbol.Eof)
                {
                    para = elements.AddParagraph();
                }
                else
                    para = null;
            }
        }

        /// <summary>
        /// Removes the last blank from the text. Used before a tab, a line break or a space will be
        /// added to the text.
        /// </summary>
        static void RemoveTrailingBlank(ParagraphElements elements)
        {
            var dom = elements.LastObject;
            if (dom is Text text)
            {
                if (text.Content.EndsWith(" ", StringComparison.Ordinal))
                    text.Content = text.Content.Remove(text.Content.Length - 1, 1);
            }
        }

        /// <summary>
        /// Parses the inner text of a paragraph. Parsing ends if '}' is reached or an empty
        /// line occurs on nesting level 0.
        /// </summary>
        void ParseFormattedText(ParagraphElements elements, int nestingLevel)
        {
            MoveToParagraphContent();

            bool loop = true;
            bool rootLevel = nestingLevel == 0;
            ReadText(rootLevel);
            while (loop)
            {
                switch (Symbol)
                {
                    case Symbol.Eof:
                        ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                        break;

                    case Symbol.EmptyLine:
                        elements.AddCharacter(SymbolName.ParaBreak);
                        ReadText(rootLevel);
                        break;

                    case Symbol.BraceRight:
                        loop = false;
                        break;

                    case Symbol.Comment:
                        // Ignore comments.
                        ReadText(rootLevel);
                        break;

                    case Symbol.Text:
                        elements.AddText(Token);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Tab:
                        RemoveTrailingBlank(elements);
                        elements.AddTab();
                        _scanner.MoveToNonWhiteSpaceOrEol();
                        ReadText(rootLevel);
                        break;

                    case Symbol.LineBreak:
                        RemoveTrailingBlank(elements);
                        elements.AddLineBreak();
                        _scanner.MoveToNonWhiteSpaceOrEol();
                        ReadText(rootLevel);
                        break;

                    case Symbol.Bold:
                        ParseBoldItalicEtc(elements.AddFormattedText(TextFormat.Bold), nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Italic:
                        ParseBoldItalicEtc(elements.AddFormattedText(TextFormat.Italic), nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Underline:
                        ParseBoldItalicEtc(elements.AddFormattedText(TextFormat.Underline), nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Font:
                        ParseFont(elements.AddFormattedText(), nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.FontSize:
                        ParseFontSize(elements.AddFormattedText(), nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.FontColor:
                        ParseFontColor(elements.AddFormattedText(), nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Image:
                        ParseImage(elements.AddImage(""), true);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Field:
                        ParseField(elements, nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Footnote:
                        ParseFootnote(elements, nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Hyperlink:
                        ParseHyperlink(elements, nestingLevel + 1);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Space:
                        RemoveTrailingBlank(elements);
                        ParseSpace(elements, nestingLevel + 1);
                        _scanner.MoveToNonWhiteSpaceOrEol();
                        ReadText(rootLevel);
                        break;

                    case Symbol.Symbol:
                        ParseSymbol(elements);
                        ReadText(rootLevel);
                        break;

                    case Symbol.Chr:
                        ParseChr(elements);
                        ReadText(rootLevel);
                        break;

                    default:
                        ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the keywords «\bold», «\italic», and «\underline».
        /// </summary>
        void ParseBoldItalicEtc(FormattedText formattedText, int nestingLevel)
        {
            ReadCode();
            EnsureSymbol(Symbol.BraceLeft);
            ParseFormattedText(formattedText.Elements, nestingLevel);
            EnsureSymbol(Symbol.BraceRight);
        }

        /// <summary>
        /// Parses the keyword «\font».
        /// </summary>
        void ParseFont(FormattedText formattedText, int nestingLevel)
        {
            EnsureSymbol(Symbol.Font);
            ReadCode();

            if (Symbol == Symbol.ParenLeft)
            {
                formattedText.Style = ParseElementName();
                ReadCode();
            }

            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(formattedText);

            EnsureSymbol(Symbol.BraceLeft);
            ParseFormattedText(formattedText.Elements, nestingLevel);
            EnsureSymbol(Symbol.BraceRight);
        }

        /// <summary>
        /// Parses code like «("name")».
        /// </summary>
        string ParseElementName()
        {
            EnsureSymbol(Symbol.ParenLeft);
            ReadCode();
            if (Symbol != Symbol.StringLiteral)
                ThrowParserException(MdDomMsgs.StringValueExpected(Token));

            string name = Token;
            ReadCode();
            EnsureSymbol(Symbol.ParenRight);

            return name;
        }

        /// <summary>
        /// Parses the keyword «\fontsize».
        /// </summary>
        void ParseFontSize(FormattedText formattedText, int nestingLevel)
        {
            EnsureSymbol(Symbol.FontSize);
            ReadCode();

            EnsureSymbol(Symbol.ParenLeft);
            ReadCode();
            //NYI: Check token for correct Unit format
            formattedText.Font.Size = Token;
            ReadCode();
            EnsureSymbol(Symbol.ParenRight);
            ReadCode();

            EnsureSymbol(Symbol.BraceLeft);
            ParseFormattedText(formattedText.Elements, nestingLevel);
            EnsureSymbol(Symbol.BraceRight);
        }

        /// <summary>
        /// Parses the keyword «\fontcolor».
        /// </summary>
        void ParseFontColor(FormattedText formattedText, int nestingLevel)
        {
            EnsureSymbol(Symbol.FontColor);
            ReadCode();  // read '('

            EnsureSymbol(Symbol.ParenLeft);
            ReadCode();  // read color token
            Color color = ParseColor();
            formattedText.Font.Color = color;
            EnsureSymbol(Symbol.ParenRight);
            ReadCode();
            EnsureSymbol(Symbol.BraceLeft);
            ParseFormattedText(formattedText.Elements, nestingLevel);
            EnsureSymbol(Symbol.BraceRight);
        }

        /// <summary>
        /// Parses the keyword «\symbol» resp. «\(».
        /// </summary>
        void ParseSymbol(ParagraphElements elements)
        {
            EnsureSymbol(Symbol.Symbol);

            ReadCode();  // read '('
            EnsureSymbol(Symbol.ParenLeft);

            const char ch = (char)0;
            SymbolName symType = 0;
            int count = 1;

            ReadCode();  // read name
            if (TokenType == TokenType.Identifier)
            {
                try
                {
                    if (Enum.IsDefined(typeof(SymbolName), Token))
                    {
                        EnsureCondition(IsSymbolType(Token), () => MdDomMsgs.InvalidSymbolType(Token));
                        symType = (SymbolName)Enum.Parse(typeof(SymbolName), Token, true);
                    }
                }
                catch (Exception ex)
                {
                    ThrowParserException(MdDomMsgs.InvalidEnum(Token, nameof(SymbolName)), ex);
                }
            }
            else
            {
                ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
            }

            ReadCode();  // read integer or identifier
            if (Symbol == Symbol.Comma)
            {
                ReadCode();  // read integer
                if (TokenType == TokenType.IntegerLiteral)
                    count = _scanner.GetTokenValueAsInt();
                ReadCode();
            }

            EnsureSymbol(Symbol.ParenRight);

            if (symType != 0)
                elements.AddCharacter(symType, count);
            else
                elements.AddCharacter(ch, count);
        }

        /// <summary>
        /// Parses the keyword «\chr».
        /// </summary>
        void ParseChr(ParagraphElements elements)
        {
            EnsureSymbol(Symbol.Chr);

            ReadCode();  // read '('
            EnsureSymbol(Symbol.ParenLeft);

            char ch = (char)0;
            SymbolName symType = 0;
            int count = 1;

            ReadCode();  // read integer
            if (TokenType == TokenType.IntegerLiteral)
            {
                int val = _scanner.GetTokenValueAsInt();
                if (val is >= 1 and <= 255)
                    ch = (char)val;
                else
                    ThrowParserException(MdDomMsgs.OutOfRange("1 - 255"));
            }
            else
            {
                ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
            }

            ReadCode();  // read integer or identifier
            if (Symbol == Symbol.Comma)
            {
                ReadCode();  // read integer
                if (TokenType == TokenType.IntegerLiteral)
                    count = _scanner.GetTokenValueAsInt();
                ReadCode();
            }

            EnsureSymbol(Symbol.ParenRight);

            if (symType != 0)
                elements.AddCharacter(symType, count);
            else
                elements.AddCharacter(ch, count);
        }

        /// <summary>
        /// Parses the keyword «\field».
        /// </summary>
#pragma warning disable IDE0060
        void ParseField(ParagraphElements elements, int nestingLevel)
#pragma warning restore IDE0060
        {
            EnsureSymbol(Symbol.Field);

            ReadCode();  // read '('
            EnsureSymbol(Symbol.ParenLeft);

            ReadCode();  // read identifier
            EnsureSymbol(Symbol.Identifier);
            string fieldType = Token.ToLower();

            ReadCode();  // read ')'
            EnsureSymbol(Symbol.ParenRight);

            DocumentObject? field = null;
            switch (fieldType)
            {
                case "date":
                    field = elements.AddDateField();
                    break;

                case "page":
                    field = elements.AddPageField();
                    break;

                // ReSharper disable once StringLiteralTypo
                case "numpages":
                    field = elements.AddNumPagesField();
                    break;

                case "info":
                    field = elements.AddInfoField(0);
                    break;

                // ReSharper disable once StringLiteralTypo
                case "sectionpages":
                    field = elements.AddSectionPagesField();
                    break;

                case "section":
                    field = elements.AddSectionField();
                    break;

                case "bookmark":
                    field = elements.AddBookmark("");
                    break;

                // ReSharper disable once StringLiteralTypo
                case "pageref":
                    field = elements.AddPageRefField("");
                    break;
            }
            EnsureCondition(field != null, () => MdDomMsgs.InvalidFieldType(Token));

            if (_scanner.PeekSymbol() == Symbol.BracketLeft)
            {
                ReadCode();  // read '['
                ParseAttributes(field, false);
            }
        }

        /// <summary>
        /// Parses the keyword «\footnote».
        /// </summary>
#pragma warning disable IDE0060
        void ParseFootnote(ParagraphElements elements, int nestingLevel)
#pragma warning restore IDE0060
        {
            EnsureSymbol(Symbol.Footnote);
            ReadCode();

            var footnote = elements.AddFootnote();
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(footnote);

            EnsureSymbol(Symbol.BraceLeft);

            // The keyword «\paragraph» is typically omitted.
            if (IsParagraphContent())
            {
                Paragraph paragraph = footnote.Elements.AddParagraph();
                ParseParagraphContent(footnote.Elements, paragraph);
            }
            else
            {
                ReadCode(); // read beyond '{'
                ParseDocumentElements(footnote.Elements, Symbol.Footnote);
            }
            EnsureSymbol(Symbol.BraceRight);
        }

        /// <summary>
        /// Parses the keyword «\hyperlink».
        /// </summary>
        void ParseHyperlink(ParagraphElements elements, int nestingLevel)
        {
            EnsureSymbol(Symbol.Hyperlink);
            ReadCode();

            Hyperlink hyperlink = elements.AddHyperlink("");
            //NYI: Without name and type the hyperlink is senseless, so attributes need to be checked
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(hyperlink);

            EnsureSymbol(Symbol.BraceLeft);
            ParseFormattedText(hyperlink.Elements, nestingLevel);
            EnsureSymbol(Symbol.BraceRight);
        }

        /// <summary>
        /// Parses the keyword «\space».
        /// </summary>
#pragma warning disable IDE0060
        void ParseSpace(ParagraphElements elements, int nestingLevel)
#pragma warning restore IDE0060
        {
            // Samples
            // \space
            // \space(5)
            // \space(em)
            // \space(em,5)
            EnsureSymbol(Symbol.Space);

            Character space = elements.AddSpace(1);

            // «\space» can stand alone
            if (_scanner.PeekSymbol() == Symbol.ParenLeft)
            {
                ReadCode(); // read '('
                EnsureSymbol(Symbol.ParenLeft);

                ReadCode(); // read beyond '('
                if (Symbol == Symbol.Identifier)
                {
                    string type = Token;
                    if (!IsSpaceType(type))
                        ThrowParserException(MdDomMsgs.InvalidEnum(type, nameof(SymbolName)));

                    space.SymbolName = (SymbolName)Enum.Parse(typeof(SymbolName), type, true);

                    ReadCode(); // read ',' or ')'
                    if (Symbol == Symbol.Comma)
                    {
                        ReadCode();  // read integer
                        EnsureSymbol(Symbol.IntegerLiteral);
                        space.Count = _scanner.GetTokenValueAsInt();
                        ReadCode(); // read ')'
                    }
                }
                else if (Symbol == Symbol.IntegerLiteral)
                {
                    space.Count = _scanner.GetTokenValueAsInt();
                    ReadCode();
                }
                EnsureSymbol(Symbol.ParenRight);
            }
        }

        /// <summary>
        /// Parses a page break in a document elements container.
        /// </summary>
        void ParsePageBreak(DocumentElements elements)
        {
            EnsureSymbol(Symbol.PageBreak);
            elements.AddPageBreak();
            ReadCode();
        }

        /// <summary>
        /// Parses the keyword «\table».
        /// </summary>
        void ParseTable(DocumentElements? elements, Table? table)
        {
            var tbl = table;
            try
            {
                if (tbl == null)
                {
                    if (elements == null)
                        throw new ArgumentException("Either elements or table must be set.");
                    tbl = elements.AddTable();
                }

                MoveToCode();
                EnsureSymbol(Symbol.Table);

                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(tbl);

                EnsureSymbol(Symbol.BraceLeft);
                ReadCode();

                // Table must start with «\columns»...
                EnsureSymbol(Symbol.Columns);
                ParseColumns(tbl);

                // ...followed by «\rows».
                EnsureSymbol(Symbol.Rows);
                ParseRows(tbl);

                EnsureSymbol(Symbol.BraceRight);
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\columns».
        /// </summary>
        void ParseColumns(Table table)
        {
            Debug.Assert(table != null);
            Debug.Assert(Symbol == Symbol.Columns);

            ReadCode();
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(table.Columns);

            EnsureSymbol(Symbol.BraceLeft);
            ReadCode();

            var loop = true;
            while (loop)
            {
                switch (Symbol)
                {
                    case Symbol.Eof:
                        ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                        break;

                    case Symbol.BraceRight:
                        loop = false;
                        ReadCode();
                        break;

                    case Symbol.Column:
                        ParseColumn(table.AddColumn());
                        break;

                    default:
                        EnsureSymbol(Symbol.Column);
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the keyword «\column».
        /// </summary>
        void ParseColumn(Column column)
        {
            Debug.Assert(column != null);
            Debug.Assert(Symbol == Symbol.Column);

            ReadCode();
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(column);

            // Read empty content
            if (Symbol == Symbol.BraceLeft)
            {
                ReadCode();
                EnsureSymbol(Symbol.BraceRight);
                ReadCode();
            }
        }

        /// <summary>
        /// Parses the keyword «\rows».
        /// </summary>
        void ParseRows(Table table)
        {
            Debug.Assert(table != null);
            Debug.Assert(Symbol == Symbol.Rows);

            ReadCode();
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(table.Rows);

            EnsureSymbol(Symbol.BraceLeft);
            ReadCode();

            var loop = true;
            while (loop)
            {
                switch (Symbol)
                {
                    case Symbol.Eof:
                        ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                        break;

                    case Symbol.BraceRight:
                        ReadCode(); // read '}'
                        loop = false;
                        break;

                    case Symbol.Row:
                        ParseRow(table.AddRow());
                        break;

                    default:
                        EnsureSymbol(Symbol.Row);
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the keyword «\row».
        /// </summary>
        void ParseRow(Row row)
        {
            Debug.Assert(row != null);
            Debug.Assert(Symbol == Symbol.Row);

            ReadCode();
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(row);

            if (Symbol == Symbol.BraceLeft)
            {
                ReadCode();

                var loop = true;
                var idx = 0;
                //int cells = row.Cells.Count;
                while (loop)
                {
                    switch (Symbol)
                    {
                        case Symbol.Eof:
                            ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                            break;

                        case Symbol.BraceRight:
                            loop = false;
                            ReadCode();
                            break;

                        case Symbol.Cell:
                            ParseCell(row[idx]);
                            idx++;
                            break;

                        default:
                            ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Parses the keyword «\cell».
        /// </summary>
        void ParseCell(Cell cell)
        {
            Debug.Assert(cell != null);
            Debug.Assert(Symbol == Symbol.Cell);

            ReadCode();
            if (Symbol == Symbol.BracketLeft)
                ParseAttributes(cell);

            // Empty cells without braces are valid.
            if (Symbol == Symbol.BraceLeft)
            {
                if (IsParagraphContent())
                {
                    ParseParagraphContent(cell.Elements, null);
                }
                else
                {
                    ReadCode();
                    if (Symbol != Symbol.BraceRight)
                        ParseDocumentElements(cell.Elements, Symbol.Cell);
                }
                EnsureSymbol(Symbol.BraceRight);
                ReadCode(); // read '}'
            }
        }

        /// <summary>
        /// Parses the keyword «\image».
        /// </summary>
        void ParseImage(Image image, bool paragraphContent)
        {
            // Future syntax by example
            //   \image("Name")
            //   \image("Name")[...]
            //   \image{base64...}       //NYI
            //   \image[...]{base64...}  //NYI
            Debug.Assert(image != null);

            try
            {
                MoveToCode();
                EnsureSymbol(Symbol.Image);
                ReadCode();

                if (Symbol == Symbol.ParenLeft)
                    image.Name = ParseElementName();

                if (_scanner.PeekSymbol() == Symbol.BracketLeft)
                {
                    ReadCode();
                    ParseAttributes(image, !paragraphContent);
                }
                else if (!paragraphContent)
                    ReadCode(); // We are a part of a section, cell etc.; read beyond ')'.
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\textframe».
        /// </summary>
        void ParseTextFrame(DocumentElements elements)
        {
            Debug.Assert(elements != null);

            var textFrame = elements.AddTextFrame();
            try
            {
                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(textFrame);

                EnsureSymbol(Symbol.BraceLeft);
                if (IsParagraphContent())
                {
                    ParseParagraphContent(textFrame.Elements, null);
                }
                else
                {
                    ReadCode(); // read '{'
                    ParseDocumentElements(textFrame.Elements, Symbol.TextFrame);
                }
                EnsureSymbol(Symbol.BraceRight);
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
        }

        void ParseBarcode(DocumentElements elements)
        {
            // Syntax:
            // 1.  \barcode(Code)
            // 2.  \barcode(Code)[...]
            // 3.  \barcode(Code, Type)
            // 4.  \barcode(Code, Type)[...]

            try
            {
                ReadCode();
                EnsureSymbol(Symbol.ParenLeft, () => MdDomMsgs.MissingParenLeft(GetSymbolText(Symbol.Barcode)));
                ReadCode();
                EnsureSymbol(Symbol.StringLiteral, () => MdDomMsgs.UnexpectedSymbol(Token));

                var barcode = elements.AddBarcode();
                barcode.SetValue("Code", Token);
                ReadCode();
                if (Symbol == Symbol.Comma)
                {
                    ReadCode();
                    EnsureSymbol(Symbol.Identifier, () => MdDomMsgs.IdentifierExpected(Token));
                    BarcodeType barcodeType = (BarcodeType)Enum.Parse(typeof(BarcodeType), Token, true);
                    barcode.SetValue("type", barcodeType);
                    ReadCode();
                }
                EnsureSymbol(Symbol.ParenRight, () => MdDomMsgs.MissingParenRight(GetSymbolText(Symbol.Barcode)));

                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(barcode);
                //barcode->ConsistencyCheck(mInfoHandler->Infos());
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\chart».
        /// </summary>
        void ParseChart(DocumentElements elements)
        {
            // Syntax:
            // 1.  \chartarea(Type){...}
            // 2.  \chartarea(Type)[...]{...}
            //
            // Usage of header-, bottom-, footer-, left- and rightarea are similar.

            ChartType chartType = 0;
            try
            {
                ReadCode(); // read '('
                EnsureSymbol(Symbol.ParenLeft, () => MdDomMsgs.MissingParenLeft(GetSymbolText(Symbol.Chart)));

                ReadCode(); // ChartType name
                EnsureSymbol(Symbol.Identifier, () => MdDomMsgs.IdentifierExpected(Token));
                string chartTypeName = Token;

                ReadCode(); // read ')'
                EnsureSymbol(Symbol.ParenRight, () => MdDomMsgs.MissingParenRight(GetSymbolText(Symbol.Chart)));

                try
                {
                    chartType = (ChartType)Enum.Parse(typeof(ChartType), chartTypeName, true);
                }
                catch (Exception ex)
                {
                    ThrowParserException(MdDomMsgs.UnknownChartType(chartTypeName), ex);
                }

                var chart = elements.AddChart(chartType);

                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(chart);

                EnsureSymbol(Symbol.BraceLeft, () => MdDomMsgs.MissingBraceLeft(GetSymbolText(Symbol.Chart)));

                ReadCode(); // read beyond '{'

                bool fContinue = true;
                while (fContinue)
                {
                    switch (Symbol)
                    {
                        case Symbol.Eof:
                            ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                            break;

                        case Symbol.BraceRight:
                            fContinue = false;
                            break;

                        case Symbol.PlotArea:
                            ParseArea(chart.PlotArea);
                            break;

                        case Symbol.HeaderArea:
                            ParseArea(chart.HeaderArea);
                            break;

                        case Symbol.FooterArea:
                            ParseArea(chart.FooterArea);
                            break;

                        case Symbol.TopArea:
                            ParseArea(chart.TopArea);
                            break;

                        case Symbol.BottomArea:
                            ParseArea(chart.BottomArea);
                            break;

                        case Symbol.LeftArea:
                            ParseArea(chart.LeftArea);
                            break;

                        case Symbol.RightArea:
                            ParseArea(chart.RightArea);
                            break;

                        case Symbol.XAxis:
                            ParseAxes(chart.XAxis, Symbol);
                            break;

                        case Symbol.YAxis:
                            ParseAxes(chart.YAxis, Symbol);
                            break;

                        case Symbol.ZAxis:
                            ParseAxes(chart.ZAxis, Symbol);
                            break;

                        case Symbol.Series:
                            ParseSeries(chart.SeriesCollection.AddSeries());
                            break;

                        case Symbol.XValues:
                            ParseSeries(chart.XValues.AddXSeries());
                            break;

                        default:
                            ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
                            break;
                    }
                }
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\plotarea» inside a chart.
        /// </summary>
        void ParseArea(PlotArea area)
        {
            // Syntax:
            // 1.  \plotarea{...}
            // 2.  \plotarea[...]{...} //???

            try
            {
                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                {
                    ParseAttributes(area, false);
                    ReadCode();
                }

                if (Symbol != Symbol.BraceLeft)
                    return;

                bool fContinue = true;
                while (fContinue)
                {
                    ReadCode();
                    switch (Symbol)
                    {
                        case Symbol.BraceRight:
                            fContinue = false;
                            break;

                        default:
                            // Ignore all? Issue warning?
                            _ = typeof(int);
                            break;
                    }
                }
                EnsureSymbol(Symbol.BraceRight);
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keywords «\headerarea», «\toparea», «\bottomarea», «\footerarea»,
        /// «\leftarea» or «\rightarea» inside a chart.
        /// </summary>
        void ParseArea(TextArea area)
        {
            // Syntax:
            // 1.  \toparea{...}
            // 2.  \toparea[...]{...}
            //
            // Usage of header-, bottom-, footer-, left- and rightarea are similar.

            try
            {
                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                {
                    ParseAttributes(area, false);
                    ReadCode();
                }

                if (Symbol != Symbol.BraceLeft)
                    return;

                if (IsParagraphContent())
                    ParseParagraphContent(area.Elements, null);
                else
                {
                    ReadCode(); // read beyond '{'
                    bool fContinue = true;
                    while (fContinue)
                    {
                        switch (Symbol)
                        {
                            case Symbol.BraceRight:
                                fContinue = false;
                                break;

                            case Symbol.Legend:
                                ParseLegend(area.AddLegend());
                                break;

                            case Symbol.Paragraph:
                                ParseParagraph(area.Elements);
                                break;

                            case Symbol.Table:
                                ParseTable(null, area.AddTable());
                                break;

                            case Symbol.TextFrame:
                                ParseTextFrame(area.Elements);
                                break;

                            case Symbol.Image:
                                var image = new Image();
                                ParseImage(image, false);
                                area.Elements.Add(image);
                                break;

                            default:
                                ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
                                break;
                        }
                    }
                }
                EnsureSymbol(Symbol.BraceRight);
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keywords «\xaxis», «\yaxis» or «\zaxis» inside a chart.
        /// </summary>
        void ParseAxes(Axis axis, Symbol symbolAxis)
        {
            // Syntax:
            // 1.  \xaxis[...]
            // 2.  \xaxis[...]{...} //???
            //
            // Usage of yaxis and zaxis are similar.

            try
            {
                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                {
                    ParseAttributes(axis, false);
                    ReadCode();
                }

                if (Symbol != Symbol.BraceLeft)
                    return;

                while (Symbol != Symbol.BraceRight)
                    ReadCode();

                EnsureSymbol(Symbol.BraceRight, () => MdDomMsgs.MissingBraceRight(GetSymbolText(symbolAxis)));
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\series» inside a chart.
        /// </summary>
        void ParseSeries(Series series)
        {
            // Syntax:
            // 1.  \series{...}
            // 2.  \series[...]{...}

            try
            {
                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(series);

                EnsureSymbol(Symbol.BraceLeft, () => MdDomMsgs.MissingBraceLeft(GetSymbolText(Symbol.Series)));
                ReadCode(); // read beyond '{'

                bool fContinue = true;
                bool fFoundComma = true;
                while (fContinue)
                {
                    switch (Symbol)
                    {
                        case Symbol.Eof:
                            ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                            break;

                        case Symbol.BraceRight:
                            fContinue = false;
                            break;

                        case Symbol.Comma:
                            fFoundComma = true;
                            ReadCode();
                            break;

                        case Symbol.Point:
                            EnsureCondition(fFoundComma, () => MdDomMsgs.MissingComma);
                            ParsePoint(series.Add(0));
                            fFoundComma = false;
                            break;

                        case Symbol.Null:
                            EnsureCondition(fFoundComma, () => MdDomMsgs.MissingComma);
                            series.AddBlank();
                            fFoundComma = false;
                            ReadCode();
                            break;

                        default:
                            EnsureCondition(fFoundComma, () => MdDomMsgs.MissingComma);
                            series.Add(_scanner.GetTokenValueAsReal());
                            fFoundComma = false;
                            ReadCode();
                            break;
                    }
                }
                EnsureSymbol(Symbol.BraceRight, () => MdDomMsgs.MissingBraceRight(GetSymbolText(Symbol.Series)));
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\xvalues» inside a chart.
        /// </summary>
        void ParseSeries(XSeries series)
        {
            // Syntax:
            // 1.  \xvalues{...}

            try
            {
                ReadCode();
                EnsureSymbol(Symbol.BraceLeft, () => MdDomMsgs.MissingBraceLeft(GetSymbolText(Symbol.Series)));

                bool fFoundComma = true;
                bool fContinue = true;
                while (fContinue)
                {
                    ReadCode();
                    switch (Symbol)
                    {
                        case Symbol.Eof:
                            ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                            break;

                        case Symbol.BraceRight:
                            fContinue = false;
                            break;

                        case Symbol.Comma:
                            fFoundComma = true;
                            break;

                        case Symbol.Null:
                            EnsureCondition(fFoundComma, () => MdDomMsgs.MissingComma);
                            series.AddBlank();
                            fFoundComma = false;
                            break;

                        case Symbol.StringLiteral:
                        case Symbol.IntegerLiteral:
                        case Symbol.RealLiteral:
                        case Symbol.HexIntegerLiteral:
                            EnsureCondition(fFoundComma, () => MdDomMsgs.MissingComma);
                            series.Add(Token);
                            fFoundComma = false;
                            break;

                        default:
                            ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));
                            break;
                    }
                }
                EnsureSymbol(Symbol.BraceRight, () => MdDomMsgs.MissingBraceRight(GetSymbolText(Symbol.Series)));
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\point» inside a series.
        /// </summary>
        void ParsePoint(Point point)
        {
            // Syntax:
            // 1.  \point{...}
            // 2.  \point[...]{...}

            try
            {
                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                    ParseAttributes(point);

                EnsureSymbol(Symbol.BraceLeft, () => MdDomMsgs.MissingBraceLeft(GetSymbolText(Symbol.Point)));
                ReadCode(); // read beyond '{'
                point.Value = _scanner.GetTokenValueAsReal();

                ReadCode(); // read '}'
                EnsureSymbol(Symbol.BraceRight, () => MdDomMsgs.MissingBraceRight(GetSymbolText(Symbol.Point)));
                ReadCode(); // read beyond '}'
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses the keyword «\legend» inside a textarea.
        /// </summary>
        void ParseLegend(Legend legend)
        {
            // Syntax:
            // 1.  \legend
            // 2.  \legend[...]
            // 3.  \legend[...]{...}

            try
            {
                ReadCode();
                if (Symbol == Symbol.BracketLeft)
                {
                    ParseAttributes(legend, false);
                    ReadCode();
                }

                // Empty legends are allowed.
                if (Symbol != Symbol.BraceLeft)
                    return;

                AdjustToNextBlock(); // consume/ignore all content
            }
            catch (DdlParserException pe)
            {
                ReportParserException(pe);
                AdjustToNextBlock();
            }
        }

        /// <summary>
        /// Parses an attribute declaration block enclosed in brackets «[…]». If readNextSymbol is
        /// set to true, the closing bracket will be read.
        /// </summary>
        void ParseAttributes(DocumentObject element, bool readNextSymbol)
        {
            EnsureSymbol(Symbol.BracketLeft);
            ReadCode();  // read beyond '['

            while (Symbol == Symbol.Identifier)
                ParseAttributeStatement(element);

            EnsureSymbol(Symbol.BracketRight);

            // Do not read ']' when parsing in paragraph content.
            if (readNextSymbol)
                ReadCode();  // read beyond ']'
        }

        /// <summary>
        /// Parses an attribute declaration block enclosed in brackets «[…]».
        /// </summary>
        void ParseAttributes(DocumentObject element)
        {
            ParseAttributes(element, true);
        }

        /// <summary>
        /// Parses a single statement in an attribute declaration block.
        /// </summary>
        void ParseAttributeStatement(DocumentObject? doc)
        {
            // Syntax is easy
            //   identifier: xxxxx
            // or 
            //   sequence of identifiers: xxx.yyy.zzz
            //
            // followed by: «=», «+=», «-=», or «{»
            //
            // Parser of rhs depends on the type of the l-value.

            if (doc == null)
                throw new ArgumentNullException(nameof(doc));
            var valueName = "";
            try
            {
                valueName = Token;
                ReadCode();

                // Resolve path, if it exists.
                object? val;
                while (Symbol == Symbol.Dot)
                {
                    Debug.Assert(doc != null, "Make ReSharper happy.");
                    val = doc.GetValue(valueName);
                    if (val == null)
                    {
                        var documentObject = doc;
                        val = documentObject.CreateValue(valueName);
                        doc.SetValue(valueName, val);
                    }
                    EnsureCondition(val != null, () => MdDomMsgs.InvalidValueName(valueName));
                    doc = val as DocumentObject;
                    EnsureCondition(doc != null, () => MdDomMsgs.SymbolIsNotAnObject(valueName));

                    ReadCode();
                    EnsureCondition(Symbol == Symbol.Identifier, () => MdDomMsgs.InvalidValueName(Token));
                    valueName = Token;
                    EnsureCondition(valueName[0] != '_', () => MdDomMsgs.NoAccess(Token));

                    ReadCode();
                }

                Debug.Assert(doc != null, "Make ReSharper happy.");
                switch (Symbol)
                {
                    case Symbol.Assign:
                        //DomValueDescriptor is needed from assignment routine.
                        var pvd = doc.Meta[valueName];
                        EnsureCondition(pvd != null, () => MdDomMsgs.InvalidValueName(valueName));
                        ParseAssign(doc, pvd);
                        break;

                    case Symbol.PlusAssign:
                    case Symbol.MinusAssign:
                        // Hard-coded for TabStops only...
                        if (doc is not ParagraphFormat)
                            ThrowParserException(MdDomMsgs.SymbolNotAllowed(Token));
                        if (String.Compare(valueName, "TabStops", StringComparison.OrdinalIgnoreCase) != 0)
                            ThrowParserException(MdDomMsgs.InvalidValueForOperation(valueName, Token));

                        ParagraphFormat paragraphFormat = (ParagraphFormat)doc;
                        TabStops tabStops = paragraphFormat.TabStops;

                        if (true) // HACK in ParseAttributeStatement       // BUG THHO4STLA Already existed in 2019.
                        {
                            bool fAddItem = Symbol == Symbol.PlusAssign;
                            var tabStop = new TabStop();

                            ReadCode();

                            if (Symbol == Symbol.BraceLeft)
                            {
                                ParseAttributeBlock(tabStop);
                            }
                            else if (Symbol is Symbol.StringLiteral or Symbol.RealLiteral or Symbol.IntegerLiteral)
                            {
                                // Special hack for tab stops...
                                Unit unit = Token;
                                tabStop.SetValue("Position", unit);

                                ReadCode();
                            }
                            else
                                ThrowParserException(MdDomMsgs.UnexpectedSymbol(Token));

                            if (fAddItem)
                                tabStops.AddTabStop(tabStop);
                            else
                                tabStops.RemoveTabStop(tabStop.Position);
                        }
                        break;

                    case Symbol.BraceLeft:
                        val = doc.GetValue(valueName);
                        EnsureCondition(val != null, () => MdDomMsgs.InvalidValueName(valueName));

                        if (val is DocumentObject doc2)
                            ParseAttributeBlock(doc2);
                        else
                            ThrowParserException(MdDomMsgs.SymbolIsNotAnObject(valueName));
                        break;

                    default:
                        ThrowParserException(MdDomMsgs.SymbolNotAllowed(Token));
                        return;
                }
            }
            catch (DdlParserException ex)
            {
                ReportParserException(ex);
                AdjustToNextBlock();
            }
            catch (ArgumentException ex)
            {
                ReportParserException(MdDomMsgs.InvalidAssignment(valueName), ex);
            }
        }

        /// <summary>
        /// Parses an attribute declaration block enclosed in braces «{…}».
        /// </summary>
        void ParseAttributeBlock(DocumentObject element)
        {
            // Technically the same as ParseAttributes

            EnsureSymbol(Symbol.BraceLeft);
            ReadCode();  // move beyond '{'

            while (Symbol == Symbol.Identifier)
                ParseAttributeStatement(element);

            EnsureSymbol(Symbol.BraceRight);
            ReadCode();  // move beyond '}'
        }

        /// <summary>
        /// Parses an assign statement in an attribute declaration block.
        /// </summary>
        void ParseAssign(DocumentObject dom, ValueDescriptor vd)
        {
            if (dom == null)
                throw new ArgumentNullException(nameof(dom));
            if (vd == null)
                throw new ArgumentNullException(nameof(vd));

            if (Symbol == Symbol.Assign)
                ReadCode();

            var valType = vd.ValueType;

            try
            {
                // BUG ReviewSTLA
                if (valType == typeof(string))
                    ParseStringAssignment(dom, vd);
                else if (valType == typeof(int))
                    ParseIntegerAssignment(dom, vd);
                else if (valType == typeof(Unit))
                    ParseUnitAssignment(dom, vd);
                else if (valType == typeof(double) || valType == typeof(float))
                    ParseRealAssignment(dom, vd);
                else if (valType == typeof(bool))
                    ParseBoolAssignment(dom, vd);
                else if (typeof(Enum).IsAssignableFrom(valType))
                    ParseEnumAssignment(dom, vd);
                else if (valType == typeof(Color))
                    ParseColorAssignment(dom, vd);
                else if (typeof(ValueType).IsAssignableFrom(valType))
                    ParseValueTypeAssignment(dom, vd);
                else if (typeof(DocumentObject).IsAssignableFrom(valType))
                    ParseDocumentObjectAssignment(dom, vd);
                else
                {
                    AdjustToNextStatement();
                    ThrowParserException(MdDomMsgs.InvalidType(vd.ValueType.Name, vd.ValueName));
                }
            }
            catch (Exception ex)
            {
                ReportParserException(MdDomMsgs.InvalidAssignment(vd.ValueName), ex);
            }
        }

        /// <summary>
        /// Parses the assignment to a boolean l-value.
        /// </summary>
        void ParseBoolAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            EnsureCondition(Symbol is Symbol.True or Symbol.False,
                () => MdDomMsgs.BoolValueExpected(Token));

            dom.SetValue(vd.ValueName, Symbol == Symbol.True);
            ReadCode();
        }

        /// <summary>
        /// Parses the assignment to an integer l-value.
        /// </summary>
        void ParseIntegerAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.HexIntegerLiteral or Symbol.StringLiteral,
                () => MdDomMsgs.IntegerValueExpected(Token));

            int n = Int32.Parse(Token, CultureInfo.InvariantCulture);
            dom.SetValue(vd.ValueName, n);

            ReadCode();
        }

        /// <summary>
        /// Parses the assignment to a floating-point l-value.
        /// </summary>
        void ParseRealAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            EnsureCondition(Symbol is Symbol.RealLiteral or Symbol.IntegerLiteral or Symbol.StringLiteral,
                () => MdDomMsgs.RealValueExpected(Token));

            double r = double.Parse(Token, CultureInfo.InvariantCulture);
            dom.SetValue(vd.ValueName, r);

            ReadCode();
        }

        /// <summary>
        /// Parses the assignment to a Unit l-value.
        /// </summary>
        void ParseUnitAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            EnsureCondition(Symbol is Symbol.RealLiteral or Symbol.IntegerLiteral or Symbol.StringLiteral,
                () => MdDomMsgs.RealValueExpected(Token));

            Unit unit = Token;
            dom.SetValue(vd.ValueName, unit);
            ReadCode();
        }

        /// <summary>
        /// Parses the assignment to a string l-value.
        /// </summary>
        void ParseStringAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            EnsureCondition(Symbol == Symbol.StringLiteral, () => MdDomMsgs.StringValueExpected(_scanner.Token));

            vd.SetValue(dom, Token);  //dom.SetValue(vd.ValueName, scanner.Token);

            ReadCode();  // read next token
        }

        /// <summary>
        /// Parses the assignment to an enum l-value.
        /// </summary>
        void ParseEnumAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            EnsureSymbol(Symbol.Identifier, () => MdDomMsgs.IdentifierExpected(Token));

            try
            {
                object val = Enum.Parse(vd.ValueType, Token, true);
                dom.SetValue(vd.ValueName, val);
            }
            catch (Exception ex)
            {
                ThrowParserException(MdDomMsgs.InvalidEnum(Token, vd.ValueName), ex);
            }

            ReadCode();  // read next token
        }

        /// <summary>
        /// Parses the assignment to a struct (i.e. LeftPosition) l-value.
        /// </summary>
        void ParseValueTypeAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            var val = vd.GetValue(dom, GV.ReadWrite);
            try
            {
                var iValue = val as INullableValue;
                iValue!.SetValue(Token); // iValue can be null. Possible Exception handled by try..catch.
                dom.SetValue(vd.ValueName, val);
                ReadCode();
            }
            catch (Exception ex)
            {
                ReportParserException(MdDomMsgs.InvalidAssignment(vd.ValueName), ex);
            }
        }

        /// <summary>
        /// Parses the assignment to a DocumentObject l-value.
        /// </summary>
        void ParseDocumentObjectAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            // Create value if it does not exist.
            var val = vd.GetValue(dom, GV.ReadWrite);
            //DocumentObject docObj = (DocumentObject)val;

            try
            {
                if (Symbol == Symbol.Null)
                {
                    //string name = vd.ValueName;
                    var type = vd.ValueType;
                    if (typeof(Border) == type)
                        (val as Border)?.Clear();
                    else if (typeof(Borders) == type)
                        (val as Borders)?.ClearAll();
                    else if (typeof(Shading) == type)
                        (val as Shading)?.Clear();
                    else if (typeof(TabStops) == type)
                        (val as TabStops)?.ClearAll();
                    else
                        ThrowParserException(MdDomMsgs.NullAssignmentNotSupported(vd.ValueName));

                    ReadCode();
                }
                else
                {
                    throw new Exception("Case: TopPosition");
                    //dom.SetValue(vd.ValueName, docObj);
                }
            }
            catch (Exception ex)
            {
                ReportParserException(MdDomMsgs.InvalidAssignment(vd.ValueName), ex);
            }
        }

        /// <summary>
        /// Parses the assignment to a Value l-value.
        /// </summary>
        void ParseValueAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            try
            {
                // What ever it is, send it to SetValue.
                dom.SetValue(vd.ValueName, Token);
            }
            catch (Exception ex)
            {
                ThrowParserException(MdDomMsgs.InvalidEnum(Token, vd.ValueName), ex);
            }

            ReadCode();  // read next token
        }

        /// <summary>
        /// Parses the assignment to a Color l-value.
        /// </summary>
        void ParseColorAssignment(DocumentObject dom, ValueDescriptor vd)
        {
            var _ = vd.GetValue(dom, GV.ReadWrite);
            var color = ParseColor();
            dom.SetValue(vd.ValueName, color);
        }

        /// <summary>
        /// Parses a color. It can be «green», «123456», «0xFFABCDEF», 
        /// «RGB(r, g, b)», «CMYK(c, m, y, k)», «CMYK(a, c, m, y, k)», «GRAY(g)», or «"MyColor"».
        /// </summary>
        Color ParseColor()
        {
            MoveToCode();
            Color color = Color.Empty;
            if (Symbol == Symbol.Identifier)
            {
                switch (Token)
                {
                    case "RGB":
                        color = ParseRGB();
                        break;

                    case "CMYK":
                        color = ParseCMYK();
                        break;

                    case "HSB":
                        throw new NotImplementedException("ParseColor(HSB)");

                    case "Lab":
                        throw new NotImplementedException("ParseColor(Lab)");

                    case "GRAY":
                        color = ParseGray();
                        break;

                    default: // Must be color enum
                        try
                        {
                            color = Color.Parse(Token);
                            ReadCode();  // read token
                        }
                        catch (Exception ex)
                        {
                            ThrowParserException(MdDomMsgs.InvalidColor(Token), ex);
                        }
                        break;
                }
            }
            else if (Symbol is Symbol.IntegerLiteral or Symbol.HexIntegerLiteral)
            {
                color = new Color(_scanner.GetTokenValueAsUInt());
                ReadCode(); // read beyond literal
            }
            else if (Symbol == Symbol.StringLiteral)
            {
                throw new NotImplementedException("ParseColor(color-name)");
            }
            else
                ThrowParserException(MdDomMsgs.StringValueExpected(Token));
            return color;
        }

        /// <summary>
        /// Parses «RGB(r, g, b)».
        /// </summary>
        // ReSharper disable once InconsistentNaming
        Color ParseRGB()
        {
            ReadCode();  // read '('
            EnsureSymbol(Symbol.ParenLeft);

            ReadCode();  // read red value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.HexIntegerLiteral,
                () => MdDomMsgs.IntegerValueExpected(Token));
            var r = _scanner.GetTokenValueAsUInt();
            EnsureCondition(r is >= 0 and <= 255, () => MdDomMsgs.InvalidRange("0 - 255"));

            ReadCode();  // read ','
            EnsureSymbol(Symbol.Comma);

            ReadCode();  // read green value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.HexIntegerLiteral,
                () => MdDomMsgs.IntegerValueExpected(Token));
            var g = _scanner.GetTokenValueAsUInt();
            EnsureCondition(g is >= 0 and <= 255, () => MdDomMsgs.InvalidRange("0 - 255"));

            ReadCode();  // read ','
            EnsureSymbol(Symbol.Comma);

            ReadCode();  // read blue value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.HexIntegerLiteral,
                () => MdDomMsgs.IntegerValueExpected(Token));
            var b = _scanner.GetTokenValueAsUInt();
            EnsureCondition(b is >= 0 and <= 255, () => MdDomMsgs.InvalidRange("0 - 255"));

            ReadCode();  // read ')'
            EnsureSymbol(Symbol.ParenRight);

            ReadCode();  // read next token

            return new Color(0xFF000000 | (r << 16) | (g << 8) | b);
        }

        /// <summary>
        /// Parses «CMYK(c, m, y, k)» or «CMYK(a, c, m, y, k)».
        /// </summary>
        // ReSharper disable once InconsistentNaming
        Color ParseCMYK()
        {
            ReadCode();  // read '('
            EnsureSymbol(Symbol.ParenLeft);

            ReadCode();  // read v1 value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.RealLiteral,
                () => MdDomMsgs.NumberValueExpected(Token));
            double v1 = _scanner.GetTokenValueAsReal();
            EnsureCondition(v1 is >= 0 and <= 100, () => MdDomMsgs.InvalidRange("0..100"));

            ReadCode();  // read ','
            EnsureSymbol(Symbol.Comma);

            ReadCode();  // read v2 value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.RealLiteral,
                () => MdDomMsgs.NumberValueExpected(Token));
            double v2 = _scanner.GetTokenValueAsReal();
            EnsureCondition(v2 is >= 0 and <= 100, () => MdDomMsgs.InvalidRange("0..100"));

            ReadCode();  // read ','
            EnsureSymbol(Symbol.Comma);

            ReadCode();  // read v3 value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.RealLiteral,
                () => MdDomMsgs.NumberValueExpected(Token));
            double v3 = _scanner.GetTokenValueAsReal();
            EnsureCondition(v3 is >= 0 and <= 100, () => MdDomMsgs.InvalidRange("0..100"));

            ReadCode();  // read ','
            EnsureSymbol(Symbol.Comma);

            ReadCode();  // read v4 value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.RealLiteral,
                () => MdDomMsgs.NumberValueExpected(Token));
            double v4 = _scanner.GetTokenValueAsReal();
            EnsureCondition(v4 is >= 0 and <= 100, () => MdDomMsgs.InvalidRange("0..100"));

            ReadCode();  // read ')' or ','
            bool hasAlpha = false;
            double v5 = 0;
            if (Symbol == Symbol.Comma)
            {
                hasAlpha = true;
                ReadCode();  // read v5 value
                EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.RealLiteral,
                    () => MdDomMsgs.NumberValueExpected(Token));
                v5 = _scanner.GetTokenValueAsReal();
                EnsureCondition(v5 is >= 0 and <= 100, () => MdDomMsgs.InvalidRange("0..100"));

                ReadCode();  // read ')'
            }
            EnsureSymbol(Symbol.ParenRight);

            ReadCode();  // read next token

            double a, c, m, y, k;
            if (hasAlpha)
            {
                a = v1; c = v2; m = v3; y = v4; k = v5;
            }
            else
            {
                a = 100; c = v1; m = v2; y = v3; k = v4;
            }
            return Color.FromCmyk(a, c, m, y, k);
        }

        /// <summary>
        /// Parses «GRAY(g)».
        /// </summary>
        Color ParseGray()
        {
            ReadCode();  // read '('
            EnsureSymbol(Symbol.ParenLeft);

            ReadCode();  // read gray value
            EnsureCondition(Symbol is Symbol.IntegerLiteral or Symbol.HexIntegerLiteral,
                () => MdDomMsgs.IntegerValueExpected(Token));
            double gray = _scanner.GetTokenValueAsReal();
            EnsureCondition(gray is >= 0 and <= 100, () => MdDomMsgs.InvalidRange("0..100"));

            ReadCode();  // read ')'
            EnsureSymbol(Symbol.ParenRight);

            ReadCode();  // read next token

            uint g = (uint)((1 - gray / 100) * 255 + 0.5);
            return new Color(0xff000000 + (g << 16) + (g << 8) + g);
        }

        /// <summary>
        /// Determines the name/text of the given symbol.
        /// </summary>
        static string GetSymbolText(Symbol docSym)
            => KeyWords.NameFromSymbol(docSym);

        /// <summary>
        /// Returns whether the specified type is a valid SpaceType.
        /// </summary>
        static bool IsSpaceType(string type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (type == "")
                throw new ArgumentException("Type is empty.", nameof(type));

            if (Enum.IsDefined(typeof(SymbolName), type))
            {
                var symbolName = (SymbolName)Enum.Parse(typeof(SymbolName), type, false); // Symbols are case sensitive.
                switch (symbolName)
                {
                    case SymbolName.Blank:
                    case SymbolName.Em:
                    //case SymbolName.Em4: // same as SymbolName.EmQuarter
                    case SymbolName.EmQuarter:
                    case SymbolName.En:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns whether the specified type is a valid enum for \symbol.
        /// </summary>
        static bool IsSymbolType(string type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (type == "")
                throw new ArgumentException("Type is empty.", nameof(type));

            if (Enum.IsDefined(typeof(SymbolName), type))
            {
                SymbolName symbolName = (SymbolName)Enum.Parse(typeof(SymbolName), type, false); // symbols are case sensitive
                switch (symbolName)
                {
                    case SymbolName.Euro:
                    case SymbolName.Copyright:
                    case SymbolName.Trademark:
                    case SymbolName.RegisteredTrademark:
                    case SymbolName.Bullet:
                    case SymbolName.Not:
                    case SymbolName.EmDash:
                    case SymbolName.EnDash:
                    case SymbolName.NonBreakableBlank:
                        //case SymbolName.HardBlank: //same as SymbolName.NonBreakableBlank:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If cond is evaluated to false, a DdlParserException with the specified error will be thrown.
        /// </summary>
        void EnsureCondition([DoesNotReturnIf(false)] bool cond, Func<MdDomMsg> func)
        {
            if (!cond)
                ThrowParserException(func());
        }

        /// <summary>
        /// If current symbol is not equal symbol a DdlParserException will be thrown.
        /// </summary>
        void EnsureSymbol(Symbol symbol)
        {
            if (Symbol != symbol)
                ThrowParserException(MdDomMsgs.SymbolExpected(KeyWords.NameFromSymbol(symbol), Token));
        }

        /// <summary>
        /// If current symbol is not equal symbol a DdlParserException with the specified message ID
        /// will be thrown.
        /// </summary>
        void EnsureSymbol(Symbol symbol, Func<MdDomMsg> func)
        {
            if (Symbol != symbol)
            {
                var domMsg = func();
                //ThrowParserException(err, KeyWords.NameFromSymbol(symbol), Token);
                ThrowParserException(domMsg);
            }
        }

        ///// <summary>
        ///// If current symbol is not equal symbol a DdlParserException with the specified message ID
        ///// will be thrown.
        ///// </summary>
        //void EnsureSymbol(Symbol symbol, DomMsgID err, params object[] parms)
        //{
        //    if (Symbol != symbol)
        //        ThrowParserException(err, KeyWords.NameFromSymbol(symbol), parms);
        //}

        /// <summary>
        /// Creates an ErrorInfo based on the given errorlevel, error and parms and adds it to the ErrorManager2.
        /// </summary>
        void ReportParserInfo(DdlErrorLevel level, MdDomMsg domMsg)
        {
            var error = new DdlReaderError(level, domMsg,
              _scanner.DocumentFileName, _scanner.CurrentLine, _scanner.CurrentLinePos);

            _errors.AddError(error);
        }

        /// <summary>
        /// Adds the ErrorInfo from the ErrorInfoException2 to the ErrorManager2.
        /// </summary>
        void ReportParserException(DdlParserException ex)
            => _errors.AddError(ex.Error);

        /// <summary>
        /// Creates an ErrorInfo based on the given inner exception, error, and parms and adds it to the ErrorManager2.
        /// </summary>
        void ReportParserException(MdDomMsg domMsg, Exception? innerException = null)
        {
            var message = domMsg.Message;
            if (innerException != null)
                message = " Inner exception: " + innerException;

            //message += DomSR.FormatMessage(errorCode, parms);
            var error = new DdlReaderError(DdlErrorLevel.Error, message, (int)domMsg.Id,
              _scanner.DocumentFileName, _scanner.CurrentLine, _scanner.CurrentLinePos);

            _errors.AddError(error);
        }

        /// <summary>
        /// Creates an ErrorInfo based on the DomMsgID and the specified parameters.
        /// Throws a DdlParserException with that ErrorInfo.
        /// </summary>
        [DoesNotReturn]
        void ThrowParserException(MdDomMsg domMsg, Exception? innerException = null)
        {
            var message = domMsg.Message;
            if (innerException != null)
                message = " Inner exception: " + innerException;

            var error = new DdlReaderError(DdlErrorLevel.Error, message, (int)domMsg.Id,
                _scanner.DocumentFileName, _scanner.CurrentLine, _scanner.CurrentLinePos);

            throw new DdlParserException(error, innerException);
        }

        /// <summary>
        /// Used for exception handling. Sets the DDL stream to the next valid position behind
        /// the current block.
        /// </summary>
        void AdjustToNextBlock()
        {
            bool skipClosingBraceOrBracket = Symbol is Symbol.BraceLeft or Symbol.BracketLeft;
            ReadCode();

            bool finish = false;
            while (!finish)
            {
                switch (Symbol)
                {
                    case Symbol.BraceLeft:
                    case Symbol.BracketLeft:
                        AdjustToNextBlock();
                        break;

                    case Symbol.BraceRight:
                    case Symbol.BracketRight:
                        if (skipClosingBraceOrBracket)
                            ReadCode();
                        finish = true;
                        break;

                    case Symbol.Eof:
                        ThrowParserException(MdDomMsgs.UnexpectedEndOfFile);
                        break;

                    default:
                        AdjustToNextStatement();
                        break;
                }
            }
        }

        /// <summary>
        /// Used for exception handling. Sets the DDL stream to the next valid position behind
        /// the current statement.
        /// </summary>
        void AdjustToNextStatement()
        {
            bool finish = false;
            while (!finish)
            {
                switch (Symbol)
                {
                    case Symbol.Assign:
                        //read one more symbol
                        ReadCode();
                        break;

                    default:
                        ReadCode();
                        finish = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Shortcut for scanner.ReadCode().
        /// Reads the next DDL token. Comments are ignored.
        /// </summary>
        Symbol ReadCode() => _scanner.ReadCode();

        /// <summary>
        /// Shortcut for scanner.ReadText().
        /// Reads either text or \keyword from current position.
        /// </summary>
        Symbol ReadText(bool rootLevel) => _scanner.ReadText(rootLevel);

        /// <summary>
        /// Shortcut for scanner.MoveToCode().
        /// Moves to the next DDL token if Symbol is not set to a valid position.
        /// </summary>
        void MoveToCode() => _scanner.MoveToCode();

        /// <summary>
        /// Shortcut for scanner.MoveToParagraphContent().
        /// Moves to the first character the content of a paragraph starts with. Empty lines
        /// and comments are skipped. Returns true if such a character exists, and false if the
        /// paragraph ends without content.
        /// </summary>
        internal bool MoveToParagraphContent() => _scanner.MoveToParagraphContent();

        /// <summary>
        /// Shortcut for scanner.MoveToNextParagraphContentLine().
        /// Moves to the first character of the content of a paragraph beyond an EOL. 
        /// Returns true if such a character exists and belongs to the current paragraph.
        /// Returns false if a new line (at root level) or '}' occurs. If a new line caused
        /// the end of the paragraph, the DDL cursor is moved to the next valid content
        /// character or '}' respectively.
        /// </summary>
        internal bool MoveToNextParagraphContentLine(bool rootLevel)
            => _scanner.MoveToNextParagraphContentLine(rootLevel);

        /// <summary>
        /// Gets the current symbol from the scanner.
        /// </summary>
        Symbol Symbol => _scanner.Symbol;

        /// <summary>
        /// Gets the current token from the scanner.
        /// </summary>
        string Token => _scanner.Token;

        /// <summary>
        /// Gets the current token type from the scanner.
        /// </summary>
        TokenType TokenType => _scanner.TokenType;

        readonly DdlScanner _scanner;
        readonly DdlReaderErrors _errors;
    }
}
