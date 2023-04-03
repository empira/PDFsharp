// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// The symbols used by DdlScanner/DdlParser.
    /// </summary>
    enum Symbol
    {
        // TokenType.None
        None,
        Eof,
        Eol,                    // End of line
        // TokenType.Keyword
        True,
        False,
        Null,

        // TokenType.Identifier
        Identifier,
        Comment,

        // TokenType.IntegerLiteral
        IntegerLiteral,
        HexIntegerLiteral,
        OctIntegerLiteral,

        // TokenType.StringLiteral
        StringLiteral,

        // TokenType.RealLiteral
        RealLiteral,

        // TokenType.OperatorOrPunctuator
        Slash,             // /
        BackSlash,         // \
        ParenLeft,         // (
        ParenRight,        // )
        BraceLeft,         // {
        BraceRight,        // }
        BracketLeft,       // [
        BracketRight,      // ]
        EmptyLine,         // CR LF CR LF
        Colon,             // :
        Semicolon,         // ;
        Assign,            // =
        Plus,              // +
        Minus,             // -
        Dot,               // .
        Comma,             // ,
        Percent,           // %
        Dollar,            // $
        Hash,              // #
        Currency,          // ¤
        //Questionmark,    // ?
        Quotationmark,     // "
        At,                // @
        //Bar,             // |
        PlusAssign,        // +=
        MinusAssign,       // -=
        CR,                // 0x0D
        LF,                // 0x0A

        // TokenType.Keyword
        Styles,
        Document,
        EmbeddedFile,
        Section,
        TableTemplates,
        TableTemplate,
        Paragraph,
        HeaderOrFooter,   // Only used as context in ParseDocumentElements
        Header,
        PrimaryHeader,
        FirstPageHeader,
        EvenPageHeader,
        Footer,
        PrimaryFooter,
        FirstPageFooter,
        EvenPageFooter,
        Table,
        Columns,
        Column,
        Rows,
        Row,
        Cell,
        Image,
        TextFrame,
        Chart,
        Footnote,
        PageBreak,
        Barcode,

        // Diagramms
        HeaderArea,
        FooterArea,
        TopArea,
        BottomArea,
        LeftArea,
        RightArea,
        PlotArea,
        Legend,
        XAxis,
        YAxis,
        ZAxis,
        Series,
        XValues,
        Point,

        // paragraph formats
        Bold,
        Italic,
        Underline,
        FontSize,
        FontColor,
        Font,

        // Hyperlink used by ParagraphParser
        Hyperlink,

        // Token used by ParagraphParser
        Text,  // Plain text in a paragraph.
        Blank,
        Tab,
        NonBreakeableBlank,
        SoftHyphen,
        LineBreak,
        Space,
        NoSpace,

        // Field used by ParagraphParser
        Field,

        // Field types used by ParagraphParser
        DateField,
        PageField,
        NumPagesField,
        InfoField,
        SectionField,
        SectionPagesField,
        BookmarkField,
        PageRefField,

        Character, //???
        Symbol,
        Chr
    }
}
