// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// ReSharper disable InconsistentNaming
#pragma warning disable 1591 // Because this is preview code.

namespace PdfSharp.Internal
{
    /// <summary>
    /// PDFsharp Graphics messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    static class GfxMsgs
    {
        // ----- Generic messages ------------------------------------------------------------------

        /// <summary>
        /// Use as a generic hack during development.
        /// </summary>
        public static SyMsg Generic(string message)
            => new(SyMsgId.None, message);

        // Example:
        static SyMsg Some_Example(string? hint = null)
           => new(SyMsgId.UnexpectedNullValueRetrieved,
               "A function returns null, but a value was expected."
               + (hint != null ? " " + hint : "")
               + " " + "[[Link to pdfsharp.com]]");

        // ----- General messages ------------------------------------------------------------------

        public static SyMsg General_NotImplemented(string member)
            => new(SyMsgId.ToDo, $"The member {member} is not (yet) implemented.");

        public static SyMsg General_BadType(string target)
            => new(SyMsgId.ToDo, $"The object passed to '{target}' is not a valid type.");

        public static SyMsg General_Expected_Type(Type expectedType)
            => new(SyMsgId.ToDo, $"Expected object of type '{expectedType.FullName}'.");

        public static SyMsg General_ObjectIsReadOnly
            => new(SyMsgId.ToDo, "The object is marked 'read-only'.");

        public static SyMsg General_PlatformMismatch(string name)
            => new(SyMsgId.ToDo, $"The object '{name}' was created for another platform. " +
                                 "You cannot mix objects from different platforms.");

        // ----- Geometry messages -----------------------------------------------------------------


        // ----- Imaging messages ------------------------------------------------------------------

        public static SyMsg Imaging_NoFileUri(Uri uri)
            => new(SyMsgId.None, "Only file URIs are allowed for bitmap images. " +
                                 $"Invalid URI: '{uri.ToString()}'.");

        // ----- Font messages ---------------------------------------------------------------------

        public static SyMsg Font_NoFileUri(Uri uri)
            => new(SyMsgId.None, "Only file URIs are allowed for font files. " +
                                 $"Invalid URI: '{uri.ToString()}'.");
    }
}