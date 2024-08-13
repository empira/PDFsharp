// PDFsharp - A .NET library for processing PDF
// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfSharp.Internal
{
    /// <summary>
    /// PDFsharp messages.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    static class PsMsgs
    {
        // Old stuff
        //// How to use:
        //// Create a function or property for each message text, depending on how many parameters are
        //// part of the message. For the beginning, type plain English text in the function or property. 
        //// The use of functions is safe when a parameter must be changed. The compiler tells you all
        //// places in your code that must be modified.
        //// For localization, create an enum value for each function or property with the same name. Then
        //// create localized message files with the enum values as messages identifiers. In the properties
        //// and functions all text is replaced by Format or GetString functions with the corresponding enum value
        //// as first parameter. The use of enums ensures that typing errors in message resource names are 
        //// simply impossible. Use the TestResourceMessages function to ensure that each enum value has an 
        //// appropriate message text.

        #region General messages

        //public static string IndexOutOfRange 
        //    => "The index is out of range.";

        public static string ListEnumCurrentOutOfRange
            => "Enumeration out of range.";

        public static string PageIndexOutOfRange
            => "The index of a page is out of range.";

        public static string OutlineIndexOutOfRange
            => "The index of an outline is out of range.";

        public static string SetValueMustNotBeNull
            => "The set value property must not be null.";

        public static string InvalidValue(int val, string name, int min, int max)
            => Invariant($"{val} is not a valid value for {name}. {name} should be greater than or equal to {min} and less than or equal to {max}.");

        public static string ObsoleteFunctionCalled
            => "The function is obsolete and must not be called.";

        public static string OwningDocumentRequired
            => "The PDF object must belong to a PdfDocument, but property Document is null.";

        public static string FileNotFound(string path)
            => $"The file '{path}' does not exist.";

        public static string FontDataReadOnly
            => "Font data is read-only.";

        public static string ErrorReadingFontData
            => "Error while parsing an OpenType font.";

        #endregion

        #region XGraphics specific messages

        // ----- XGraphics ----------------------------------------------------------------------------

        public static string PointArrayEmpty
            => "The PointF array must not be empty.";

        public static string PointArrayAtLeast(int count)
            => Invariant($"The point array must contain {count} or more points.");

        public static string NeedPenOrBrush
            => "XPen or XBrush or both must not be null.";

        public static string CannotChangeImmutableObject(string typename)
            => $"You cannot change this immutable {typename} object.";

        public static string FontAlreadyAdded(string fontName)
            => $"FontFace with the name '{fontName}' already added to font collection.";

        public static string NotImplementedForFontsRetrievedWithFontResolver(string name)
            => $"Not implemented for font '{name}', because it was retrieved with font resolver.";

        #endregion

        #region PDF specific messages

        // ----- PDF ----------------------------------------------------------------------------------

        public static string InvalidPdf
            => "The file is not a valid PDF document.";

        public static string InvalidVersionNumber(int value)
            => Invariant($"The value {value} is not a valid version number.");  // Valid values are 12, 13, and 14. 

        public static string PasswordRequired
            => "A password is required to open the PDF document.";

        public static string InvalidPassword
            => "The specified password is invalid.";

        public static string OwnerPasswordRequired
            => "To modify the document the owner password is required";

        public static string UserOrOwnerPasswordRequired
            => "At least a user or an owner password is required to encrypt the document.";

        public static string CannotModify
            => "The document cannot be modified.";

        public static string NameMustStartWithSlash
            => "A PDF name must start with a slash (/).";

        public static string ImportPageNumberOutOfRange(int pageNumber, int maxPage, string path)
        {
            return String.Format("The page cannot be imported from document '{2}', because the page number is out of range. " +
              "The specified page number is {0}, but it must be in the range from 1 to {1}.", pageNumber, maxPage, path);
        }

        public static string MultiplePageInsert => "The page cannot be added to this document because the document already owned this page.";

        public static string UnexpectedTokenInPdfFile => "Unexpected token in PDF file. The PDF file may be corrupt. If it is not, please send us the file for service (issues (at) pdfsharp.net).";

        public static string InappropriateColorSpace(PdfColorMode colorMode, XColorSpace colorSpace)
        {
            string mode;
            switch (colorMode)
            {
                case PdfColorMode.Rgb:
                    mode = "RGB";
                    break;

                case PdfColorMode.Cmyk:
                    mode = "CMYK";
                    break;

                default:
                    mode = "(undefined)";
                    break;
            }

            string space;
            switch (colorSpace)
            {
                case XColorSpace.Rgb:
                    space = "RGB";
                    break;

                case XColorSpace.Cmyk:
                    space = "CMYK";
                    break;

                case XColorSpace.GrayScale:
                    space = "grayscale";
                    break;

                default:
                    space = "(undefined)";
                    break;
            }
            return $"The document requires color mode {mode}, but a color is defined using {space}. " +
                   "Use only colors that match the color mode of the PDF document";
        }

        public static string CannotGetGlyphTypeface(string fontName)
            => Invariant($"Cannot get a matching glyph typeface for font '{fontName}'.");

        // ----- PdfParser ----------------------------------------------------------------------------

        public static string UnexpectedToken(string token) => $"Token '{token}' was not expected.";
        //{
        //    return Format(PSMsgID.UnexpectedToken, token);
        //    //return Format("Token '{0}' was not expected.", token);
        //}

        #endregion
    }
}