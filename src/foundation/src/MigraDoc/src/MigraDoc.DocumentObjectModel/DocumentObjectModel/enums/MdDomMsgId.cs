// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// MigraDoc DOM message ID.
    /// Represents IDs for error and diagnostic messages generated by the MigraDoc DOM.
    /// </summary>
    enum MdDomMsgId
    {
        None = 0,

        Success = 100,

        // ----- General Messages ---------------------------------------------------------------------

        StyleExpected = MessageIdOffset.MdDom,
        BaseStyleRequired,
        EmptyBaseStyle,
        InvalidFieldFormat,
        InvalidInfoFieldName,
        UndefinedBaseStyle,
        InvalidUnitValue,
        InvalidUnitType,
        InvalidEnumValue,
        InvalidEnumForLeftPosition,
        InvalidEnumForTopPosition,
        InvalidColorString,
        InvalidFontSize,
        InsertNullNotAllowed,
        ParentAlreadySet,
        MissingObligatoryProperty,
        InvalidDocumentObjectType,

        // ----- DdlReader Messages -------------------------------------------------------------------

        SymbolExpected,
        SymbolsExpected,
        OperatorExpected,
        KeyWordExpected,
        EndOfFileExpected,
        UnexpectedEndOfFile,

        StyleNameExpected,

        // --- old ---
        //    Internal,

        UnexpectedSymbol,

        IdentifierExpected,
        BoolValueExpected,
        RealValueExpected,
        IntegerValueExpected,
        //    IntegerOrIdentifierExpected,
        StringValueExpected,
        NullValueExpected,
        //    SymbolExpected,
        NumberValueExpected,

        InvalidEnum,
        //    InvalidFlag,
        //    InvalidStyle,
        //    InvalidStyleDefinition,
        InvalidType,
        InvalidAssignment,
        InvalidValueName,
        //    InvalidOperation,
        //    InvalidFormat,
        InvalidRange,
        InvalidColor,
        InvalidFieldType,
        InvalidValueForOperation,
        InvalidSymbolType,

        //    ValueOutOfRange,

        MissingBraceLeft,
        MissingBraceRight,
        MissingBracketLeft,
        MissingBracketRight,
        MissingParenLeft,
        MissingParenRight,
        //    MissingSemicolon,
        MissingComma,

        //    MissingDocumentPart,
        //    MissingEof,
        //    MissingIdentifier,
        //    MissingEndBuildingBlock,
        //    MissingSymbol,
        //    MissingParameter,

        SymbolNotAllowed,
        SymbolIsNotAnObject,
        //    BlockcommentOutsideCodeBlock,
        //    EOFReachedMissingSymbol,
        //    UnexpectedEOFReached,
        //    StyleAlreadyDefined,
        //    MultipleDefaultInSwitch,
        //    UnexpectedNewlineInDirective,
        //    UnexpectedSymbolInDirective,

        //    UnknownUnitOfMeasure,
        //    UnknownValueOperator,
        //    UnknownCodeSymbol,
        //    UnknownScriptSymbol,
        //    UnknownFieldSpecifier,
        //    UnknownFieldOption,
        //    UnknownValueType,
        //    UnknownEvaluationType,
        //    UnknownColorFunction,
        //    UnknownAxis,
        UnknownChartType,

        //    MisplacedCompilerSettings,
        //    MisplacedScopeType,
        //    TooMuchCells,
        NoAccess,

        //    FileNotFound,
        //    NotSupported,

        NewlineInString,
        EscapeSequenceNotAllowed,
        //    SymbolsNotAllowedInsideText,
        NullAssignmentNotSupported,
        OutOfRange,

        //    Warning_StyleOverwrittenMoreThanOnce,
        //    Warning_StyleAndBaseStyleAreEqual,
        //    Warning_NestedParagraphInParagraphToken,
        UseOfUndefinedBaseStyle,
        UseOfUndefinedStyle,

        //    NestedFootnote,
        //    ImageInFootnote,
        //    ShapeInFootnote
    }
}
