// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591 // Because this is preview code.

namespace PdfSharp.Internal
{
    /// <summary>
    /// (PDFsharp) System messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    static class SyMsgs
    {
        // ----- Generic messages ------------------------------------------------------------------

        /// <summary>
        /// Use as a generic hack during development.
        /// </summary>
        public static SyMsg Generic(string message)
            => new(SyMsgId.None, message);





        public static string IndexOutOfRange3
            => "Index out of range.";

        public static SyMsg IndexOutOfRange2<T>(string parameter, T lowerBound, T upperBound)
            => new(SyMsgId.IndexOutOfRange,
            $"The value of '{parameter}' is out of range. " +
                   Invariant($"The value must be between '{lowerBound}' and '{upperBound}'."));

        // ----- General messages ------------------------------------------------------------------


        public static SyMsg UnexpectedNullValueRetrieved(string? hint = null)
            => new(SyMsgId.UnexpectedNullValueRetrieved,
                "A function returns null, but a value was expected."
                + (hint != null ? " " + hint : "")
                + " " + "[[Link to pdfsharp.com]]");

        // ----- PDF object model ------------------------------------------------------------------

        public static SyMsg IndirectReferenceMustNotBeNull
            => new(SyMsgId.IndirectReferenceMustNotBeNull,
                "The indirect reference (PdfReference) is null, but is expected to be defined.");

        public static SyMsg ObjectWithoutOwner
            => new(SyMsgId.ObjectWithoutOwner,
                "The direct or indirect object must be owned by a PdfDocument to execute the expected operation.");

        public static SyMsg ArrayEntryIsOfWrongType(int index, Type expected, Type found)
            => new(SyMsgId.ArrayEntryIsOfWrongType,
                $"The array entry '{index}' was expected to be of type '{expected.FullName}', but is of type '{found.FullName}'.");

        public static SyMsg DictionaryEntryDoesNotExist(string key)
            => new(SyMsgId.DictionaryEntryDoesNotExist,
                $"The dictionary entry with key '{key}' does not exist.");

        public static SyMsg DictionaryEntryDoesNotExistAndNoDefaultSpecified(string key)
            => new(SyMsgId.DictionaryEntryDoesNotExistAndNoDefaultSpecified,
                $"The dictionary entry with key '{key}' does not exist and no default value was specified.");

        public static SyMsg DictionaryEntryIsOfWrongType(string key, Type expected, Type found)
            => new(SyMsgId.DictionaryEntryIsOfWrongType,
                $"The dictionary entry with key '{key}' was expected to be of type '{expected.FullName}', but is of type '{found.FullName}'.");

        public static SyMsg IndirectObjectExpected
            => new(SyMsgId.IndirectObjectExpected,
                $"An indirect object was expected.");

        public static SyMsg IndirectObjectExpectedButDirectObjectFound
            => new(SyMsgId.IndirectObjectExpectedButDirectObjectFound,
                $"An indirect object was expected, but a direct object was found.");

        public static SyMsg DirectObjectExpected
            => new(SyMsgId.DirectObjectExpected,
                $"A direct object was expected.");

        public static SyMsg DirectObjectExpectedButIndirectObjectFound
            => new(SyMsgId.DirectObjectExpectedButIndirectObjectFound,
                $"A direct object was expected, but an indirect object was found.");
    }
}