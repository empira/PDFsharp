// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.IO;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    static class MSG
    {
        public static string SomeMessage(string someString, int someInt)
            => Invariant($"Lorem ipsum {someString} labere {someInt}.");
    }

    /// <summary>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    static class TH  // RENAME TODO 
    {
        #region General Messages

        //public static string? BaseStyleRequired => GetString(DomMsgID.BaseStyleRequired);

        //public static string? EmptyBaseStyle => GetString(DomMsgID.EmptyBaseStyle);

        //public static string InvalidFieldFormat(string format)
        //{
        //    return FormatMessage(DomMsgID.InvalidFieldFormat, format);
        //}

        //public static string InvalidInfoFieldName(string name)
        //{
        //    return FormatMessage(DomMsgID.InvalidInfoFieldName, name);
        //}

        // StyleExpected = The value must be of type MigraDoc.DocumentObjectModel.Style.
        public static ArgumentException ArgumentException_StyleExpected(Type type) =>
            new($"The value is of type '{type.FullName}' but must be of type 'MigraDoc.DocumentObjectModel.Style'.");

        // BaseStyleRequired = Base style name must be defined.
        public static ArgumentException ArgumentException_UndefinedBaseStyle(string baseStyle) =>
            new($"Base style name '{baseStyle}' is undefined.");

        //{
        //    return FormatMessage(DomMsgID.UndefinedBaseStyle, baseStyle);
        //}

        //public static string InvalidValueName(string name)
        //{
        //    return FormatMessage(DomMsgID.InvalidValueName, name);
        //}

        //public static string InvalidUnitValue(string unitValue)
        //{
        //    return FormatMessage(DomMsgID.InvalidUnitValue, unitValue);
        //}

        //public static string InvalidUnitType(string unitType)
        //{
        //    return FormatMessage(DomMsgID.InvalidUnitType, unitType);
        //}

        //public static string InvalidEnumValue<T>(T value) where T : unmanaged // struct, Enum
        //{
        //    // ... where T : enum
        //    //   -or-
        //    // ... where T : Enum
        //    // is not implemented in C# because nobody has done this.
        //    // See Eric Lippert on this topic: http://stackoverflow.com/questions/1331739/enum-type-constraints-in-c-sharp
        //    // UPDATE: Enum constraint comes with C# 7.3, see: https://devblogs.microsoft.com/premier-developer/dissecting-new-generics-constraints-in-c-7-3/
        //    Debug.Assert(typeof(T).IsSubclassOf(typeof(Enum)));
        //    return FormatMessage(DomMsgID.InvalidEnumValue, value, typeof(T).Name);
        //}

        //public static string? InvalidEnumForLeftPosition => GetString(DomMsgID.InvalidEnumForLeftPosition);

        //public static string? InvalidEnumForTopPosition => GetString(DomMsgID.InvalidEnumForTopPosition);

        //public static string InvalidColorString(string colorString)
        //{
        //    return FormatMessage(DomMsgID.InvalidColorString, colorString);
        //}

        //public static string InvalidFontSize(double value)
        //{
        //    return FormatMessage(DomMsgID.InvalidFontSize, value);
        //}

        //public static string InsertNullNotAllowed()
        //{
        //    return "Insert null not allowed.";
        //}

        //public static string ParentAlreadySet(DocumentObject value, DocumentObject docObject)
        //{
        //    return $"Value of type '{value.GetType()}' must be cloned before set into '{docObject.GetType()}'.";
        //}

        //public static string MissingObligatoryProperty(string propertyName, string className)
        //{
        //    return FormatMessage(DomMsgID.MissingObligatoryProperty, propertyName, className);
        //}

        public static string InvalidDocumentObjectType => "The given document object is not valid in this context.";

        //// ----- General Messages ---------------------------------------------------------------------
        //StyleExpected,
        //BaseStyleRequired,
        //EmptyBaseStyle,
        //InvalidFieldFormat,
        //InvalidInfoFieldName,
        //UndefinedBaseStyle,
        //InvalidUnitValue,
        //InvalidUnitType,
        //InvalidEnumValue,
        //InvalidEnumForLeftPosition,
        //InvalidEnumForTopPosition,
        //InvalidColorString,
        //InvalidFontSize,
        //InsertNullNotAllowed,
        //ParentAlreadySet,
        //MissingObligatoryProperty,
        //InvalidDocumentObjectType,
        #endregion

        #region DdlReader Messages

        //Success = 1000,

        //SymbolExpected,
        //SymbolsExpected,
        //OperatorExpected,
        //KeyWordExpected,
        //EndOfFileExpected,
        //UnexpectedEndOfFile,

        //StyleNameExpected,

        //// --- old ---
        ////    Internal,

        //UnexpectedSymbol,

        //IdentifierExpected,
        //BoolExpected,
        //RealExpected,
        //IntegerExpected,
        ////    IntegerOrIdentifierExpected,
        //StringExpected,
        //NullExpected,
        ////    SymbolExpected,
        //NumberExpected,

        //InvalidEnum,
        ////    InvalidFlag,
        ////    InvalidStyle,
        ////    InvalidStyleDefinition,
        //InvalidType,
        //InvalidAssignment,
        //InvalidValueName,
        ////    InvalidOperation,
        ////    InvalidFormat,
        //InvalidRange,
        //InvalidColor,
        //InvalidFieldType,
        //InvalidValueForOperation,
        //InvalidSymbolType,

        ////    ValueOutOfRange,

        //MissingBraceLeft,
        //MissingBraceRight,
        //MissingBracketLeft,
        //MissingBracketRight,
        //MissingParenLeft,
        //MissingParenRight,
        ////    MissingSemicolon,
        //MissingComma,

        ////    MissingDocumentPart,
        ////    MissingEof,
        ////    MissingIdentifier,
        ////    MissingEndBuildingBlock,
        ////    MissingSymbol,
        ////    MissingParameter,

        //SymbolNotAllowed,
        //SymbolIsNotAnObject,
        ////    BlockcommentOutsideCodeBlock,
        ////    EOFReachedMissingSymbol,
        ////    UnexpectedEOFReached,
        ////    StyleAlreadyDefined,
        ////    MultipleDefaultInSwitch,
        ////    UnexpectedNewlineInDirective,
        ////    UnexpectedSymbolInDirective,

        ////    UnknownUnitOfMeasure,
        ////    UnknownValueOperator,
        ////    UnknownCodeSymbol,
        ////    UnknownScriptSymbol,
        ////    UnknownFieldSpecifier,
        ////    UnknownFieldOption,
        ////    UnknownValueType,
        ////    UnknownEvaluationType,
        ////    UnknownColorFunction,
        ////    UnknownAxis,
        //UnknownChartType,

        ////    MisplacedCompilerSettings,
        ////    MisplacedScopeType,
        ////    TooMuchCells,
        //NoAccess,

        ////    FileNotFound,
        ////    NotSupported,

        //NewlineInString,
        public static DdlParserException DdlParserException_NewlineInString()
            => new(DdlErrorLevel.Error,
                "Newline in string not allowed.",
                DomMsgID.NewlineInString);

        //EscapeSequenceNotAllowed
        public static DdlParserException DdlParserException_EscapeSequenceNotAllowed(string s)
            => new(DdlErrorLevel.Error,
                $"Invalid escape sequence '{s}'.",
                DomMsgID.EscapeSequenceNotAllowed);

        ////    SymbolsNotAllowedInsideText,
        //NullAssignmentNotSupported,
        //OutOfRange,

        ////    Warning_StyleOverwrittenMoreThanOnce,
        ////    Warning_StyleAndBaseStyleAreEqual,
        ////    Warning_NestedParagraphInParagraphToken,
        //UseOfUndefinedBaseStyle,
        //UseOfUndefinedStyle,

        ////    NestedFootnote,
        ////    ImageInFootnote,
        ////    ShapeInFootnote

        #endregion

        #region Renderer Messages
        public static InvalidOperationException InvalidOperationException_MergeDownTooLarge(int row, int column, int mergeDown) =>
            new($"Table cell in row {row} and column {column} has MergeDown {mergeDown} exceeding the rows in the table.");

        public static InvalidOperationException InvalidOperationException_MergeRightTooLarge(int row, int column, int mergeRight) =>
            new($"Table cell in row {row} and column {column} has MergeRight {mergeRight} exceeding the columns in the table.");
        #endregion
    }
}
