// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// MigraDoc DOM messages.
    /// String resources of MigraDoc.DocumentObjectModel. Provides all localized text strings
    /// for this assembly.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    static class MdDomMsgs
    {
        #region General Messages

        public static MdDomMsg StyleExpected(Type wrongType)
            => new(MdDomMsgId.StyleExpected, $"The value must be of type 'Style', but it is of type '{wrongType.Name}'.");

        public static MdDomMsg BaseStyleRequired
            => new(MdDomMsgId.BaseStyleRequired, "Base style name must be defined.");

        public static MdDomMsg EmptyBaseStyle
            => new(MdDomMsgId.EmptyBaseStyle, "Attempt to set empty base style is invalid.");

        public static MdDomMsg InvalidFieldFormat(string format)
            => new(MdDomMsgId.InvalidFieldFormat, $"'{format}' is not a valid numeric field format.");

        public static MdDomMsg InvalidInfoFieldName(string name)
            => new(MdDomMsgId.InvalidInfoFieldName, $"Property 'Name' of 'InfoField' has invalid value '{name}'.");

        public static MdDomMsg UndefinedBaseStyle(string style)
            => new(MdDomMsgId.UndefinedBaseStyle, $"The base style of style '{style}' is undefined.");

        public static MdDomMsg InvalidValueName(string name)
            => new(MdDomMsgId.InvalidValueName, $"Invalid value name: '{name}'.");

        public static MdDomMsg InvalidUnitValue(string unitValue)
            => new(MdDomMsgId.InvalidUnitValue, $"String '{unitValue}' is not a valid value for structure 'Unit'.");

        public static MdDomMsg InvalidUnitType(string unitType)
            => new(MdDomMsgId.InvalidUnitType, $"'{unitType}' is an unknown unit type.");

        public static MdDomMsg InvalidEnumValue<T>(T value) where T : Enum
            => new(MdDomMsgId.InvalidEnumValue, $"The value '{value:X}' is not valid for enum type '{typeof(T)}'.");

        public static MdDomMsg InvalidEnumForLeftPosition
            => new(MdDomMsgId.InvalidEnumForLeftPosition, "ShapePosition must be 'Left', 'Center', or 'Right'.");

        public static MdDomMsg InvalidEnumForTopPosition
            => new(MdDomMsgId.InvalidEnumForTopPosition, "ShapePosition must be 'Top', 'Center', or 'Bottom'.");

        public static MdDomMsg InvalidColorString(string colorString)
            => new(MdDomMsgId.InvalidColorString, $"Color could not be parsed from string '{colorString}'.");

        public static MdDomMsg InvalidFontSize(double value)
            => new(MdDomMsgId.InvalidFontSize, $"The font size '{value}' is out of range.");

        public static MdDomMsg InsertNullNotAllowed()
            => new(MdDomMsgId.InsertNullNotAllowed, "Insert null not allowed.");

        public static MdDomMsg ParentAlreadySet(DocumentObject value, DocumentObject docObject)
            => new(MdDomMsgId.ParentAlreadySet, $"Value of type '{value.GetType()}' must be cloned before setting into '{docObject.GetType()}'.");

        public static MdDomMsg MissingObligatoryProperty(string propertyName, string className)
            => new(MdDomMsgId.MissingObligatoryProperty, $"Obligatory property '{propertyName}' not set in '{className}'.");

        public static MdDomMsg InvalidDocumentObjectType
            => new(MdDomMsgId.InvalidDocumentObjectType, "The given document object is not valid in this context.");

        #endregion

        #region DdlReader Messages

        public static MdDomMsg SymbolExpected(string expected, string token)
            => new(MdDomMsgId.SymbolExpected, $"'{expected}' expected, found '{token}'.");

        //public static MdDomMsg SymbolsExpected
        //    => new(DomMsgId.SymbolsExpected, "One of the following symbols {x} is expected.");

        //public static MdDomMsg OperatorExpected
        //    => new(DomMsgId.OperatorExpected, "Syntax error: Operator '{x}' is expected.");

        //public static MdDomMsg KeyWordExpected
        //    => new(DomMsgID.KeyWordExpected, "'{y}' - '{x}' expected.");

        public static MdDomMsg EndOfFileExpected
            => new(MdDomMsgId.EndOfFileExpected, "End of file expected.");
        public static MdDomMsg UnexpectedEndOfFile
            => new(MdDomMsgId.UnexpectedEndOfFile, "Unexpected end of file.");

        public static MdDomMsg StyleNameExpected(string name)
            => new(MdDomMsgId.StyleNameExpected, $"Invalid style name '{name}'.");

        public static MdDomMsg UnexpectedSymbol(string token)
            => new(MdDomMsgId.UnexpectedSymbol, $"Unexpected symbol '{token}'.");

        public static MdDomMsg IdentifierExpected(string token)
            => new(MdDomMsgId.IdentifierExpected, $"Identifier expected: '{token}'.");

        public static MdDomMsg BoolValueExpected(string token)
            => new(MdDomMsgId.BoolValueExpected, $"Bool value expected: '{token}'.");

        public static MdDomMsg RealValueExpected(string token)
            => new(MdDomMsgId.RealValueExpected, $"Real value expected: '{token}'.");

        public static MdDomMsg IntegerValueExpected(string token)
            => new(MdDomMsgId.IntegerValueExpected, $"Integer value expected: '{token}'.");

        public static MdDomMsg StringValueExpected(string token)
            => new(MdDomMsgId.StringValueExpected, $"String value expected: '{token}'.");

        public static MdDomMsg NullValueExpected(string token)
            => new(MdDomMsgId.NullValueExpected, $"Null value expected: '{token}'.");

        public static MdDomMsg NumberValueExpected(string token)
            => new(MdDomMsgId.NumberValueExpected, $"Number value expected: '{token}'.");

        public static MdDomMsg InvalidEnum(string token, string typeName)
            => new(MdDomMsgId.InvalidEnum, $"'{token}' is not a valid value for type '{typeName}'.");

        public static MdDomMsg InvalidType(string typeName, string valueName)
            => new(MdDomMsgId.InvalidType, $"Variable type '{typeName}' not supported by '{valueName}'.");

        public static MdDomMsg InvalidAssignment(string name)
            => new(MdDomMsgId.InvalidAssignment, $"Invalid assignment to '{name}'.");

        //public static MdDomMsg InvalidValueName => "Invalid value name: '{x}'.");

        public static MdDomMsg InvalidRange(string range)
            => new(MdDomMsgId.InvalidRange, $"Invalid range: '{range}'.");

        public static MdDomMsg InvalidColor(string token)
            => new(MdDomMsgId.InvalidColor, $"Invalid color: '{token}'.");

        public static MdDomMsg InvalidFieldType(string token)
            => new(MdDomMsgId.InvalidFieldType, $"Invalid field type: '{token}'.");

        public static MdDomMsg InvalidValueForOperation(string value, string token)
            => new(MdDomMsgId.InvalidValueForOperation, $"Operation '{token}' is not valid for value '{value}'.");

        public static MdDomMsg InvalidSymbolType(string token)
            => new(MdDomMsgId.InvalidSymbolType, $"Symbol not valid '{token}'.");

        public static MdDomMsg MissingBraceLeft(string token)
            => new(MdDomMsgId.MissingBraceLeft, $"Missing left brace after '{token}'.");

        public static MdDomMsg MissingBraceRight(string token)
            => new(MdDomMsgId.MissingBraceRight, $"Missing right brace after '{token}'.");

        public static MdDomMsg MissingBracketLeft(string token)
            => new(MdDomMsgId.MissingBracketLeft, $"Missing left bracket after '{token}'.");

        public static MdDomMsg MissingBracketRight(string token)
            => new(MdDomMsgId.MissingBracketRight, $"Missing right bracket after '{token}'.");

        public static MdDomMsg MissingParenLeft(string token)
            => new(MdDomMsgId.MissingParenLeft, $"Missing left parenthesis after '{token}'.");

        public static MdDomMsg MissingParenRight(string token)
            => new(MdDomMsgId.MissingParenRight, $"Missing right parenthesis after '{token}'.");

        public static MdDomMsg MissingComma
            => new(MdDomMsgId.MissingComma, "Missing comma.");

        public static MdDomMsg SymbolNotAllowed(string token)
            => new(MdDomMsgId.SymbolNotAllowed, $"Symbol '{token}' is not allowed in this context.");

        public static MdDomMsg SymbolIsNotAnObject(string token)
            => new(MdDomMsgId.SymbolIsNotAnObject, $"Symbol '{token}' is not an object.");

        public static MdDomMsg UnknownChartType(string name)
            => new(MdDomMsgId.UnknownChartType, $"Unknown chart type: '{name}'.");

        public static MdDomMsg NoAccess(string name)
            => new(MdDomMsgId.NoAccess, $"Access denied: '{name}' for internal use only.");

        public static MdDomMsg NewlineInString
            => new(MdDomMsgId.NewlineInString, "Newline in string not allowed.");

        public static MdDomMsg EscapeSequenceNotAllowed(string token)
            => new(MdDomMsgId.EscapeSequenceNotAllowed, $"Invalid escape sequence '{token}'.");

        public static MdDomMsg NullAssignmentNotSupported(string name)
            => new(MdDomMsgId.NullAssignmentNotSupported, $"Assigning 'null' to '{name}' not allowed.");

        public static MdDomMsg OutOfRange(string range)
            => new(MdDomMsgId.OutOfRange, $"Valid range only within '{range}'.");

        public static MdDomMsg UseOfUndefinedBaseStyle(string styleName)
            => new(MdDomMsgId.UseOfUndefinedBaseStyle, $"Use of undefined base style '{styleName}'.");

        public static MdDomMsg UseOfUndefinedStyle(string styleName)
            => new(MdDomMsgId.UseOfUndefinedStyle, $"Use of undefined style '{styleName}'.");

        #endregion
    }
}
