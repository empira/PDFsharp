// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Resources;
using System.Reflection;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Pdf;

#pragma warning disable 1591

namespace PdfSharp
{
    /// <summary>
    /// The Pdf-Sharp-String-Resources.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    static class PSSR
    {
        // How to use:
        // Create a function or property for each message text, depending on how many parameters are
        // part of the message. For the beginning, type plain English text in the function or property. 
        // The use of functions is safe when a parameter must be changed. The compiler tells you all
        // places in your code that must be modified.
        // For localization, create an enum value for each function or property with the same name. Then
        // create localized message files with the enum values as messages identifiers. In the properties
        // and functions all text is replaced by Format or GetString functions with the corresponding enum value
        // as first parameter. The use of enums ensures that typing errors in message resource names are 
        // simply impossible. Use the TestResourceMessages function to ensure that each enum value has an 
        // appropriate message text.

        #region Helper functions
        ///// <summary>
        ///// Loads the message from the resource associated with the enum type and formats it
        ///// using 'String.Format'. Because this function is intended to be used during error
        ///// handling it never raises an exception.
        ///// </summary>
        ///// <param name="id">The type of the parameter identifies the resource
        ///// and the name of the enum identifies the message in the resource.</param>
        ///// <param name="args">Parameters passed through 'String.Format'.</param>
        ///// <returns>The formatted message.</returns>
        //public static string Format(PSMsgID id, params object[] args)
        //{
        //    string message;
        //    try
        //    {
        //        message = GetString(id);
        //        message = message != null ? Format(message, args) : "INTERNAL ERROR: Message not found in resources.";
        //        return message;
        //    }
        //    catch (Exception ex)
        //    {
        //        message = String.Format("UNEXPECTED ERROR while formatting message with ID {0}: {1}", id.ToString(), ex.ToString());
        //    }
        //    return message;
        //}

        public static string Format(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            string message;
            try
            {
                message = String.Format(format, args);
            }
            catch (Exception ex)
            {
                message = $"UNEXPECTED ERROR while formatting message '{format}': {ex}";
            }
            return message;
        }

        ///// <summary>
        ///// Gets the localized message identified by the specified DomMsgID.
        ///// </summary>
        //public static string GetString(PSMsgID id)
        //{
        //    return ResMngr.GetString(id.ToString());
        //}

        #endregion

        #region General messages

        public static string IndexOutOfRange => "The index is out of range.";

        public static string ListEnumCurrentOutOfRange => "Enumeration out of range.";

        public static string PageIndexOutOfRange => "The index of a page is out of range.";

        public static string OutlineIndexOutOfRange => "The index of an outline is out of range.";

        public static string SetValueMustNotBeNull => "The set value property must not be null.";

        public static string InvalidValue(int val, string name, int min, int max)
        {
            return Format("{0} is not a valid value for {1}. {1} should be greater than or equal to {2} and less than or equal to {3}.",
              val, name, min, max);
        }

        public static string ObsoleteFunctionCalled => "The function is obsolete and must not be called.";

        public static string OwningDocumentRequired => "The PDF object must belong to a PdfDocument, but property Document is null.";

        public static string FileNotFound(string path) => Format("The file '{0}' does not exist.", path);

        public static string FontDataReadOnly => "Font data is read-only.";

        public static string ErrorReadingFontData => "Error while parsing an OpenType font.";

        #endregion

        #region XGraphics specific messages

        // ----- XGraphics ----------------------------------------------------------------------------

        public static string PointArrayEmpty => "The PointF array must not be empty.";

        public static string PointArrayAtLeast(int count)
        {
            return Format("The point array must contain {0} or more points.", count);
        }

        public static string NeedPenOrBrush => "XPen or XBrush or both must not be null.";

        public static string CannotChangeImmutableObject(string typename)
        {
            return $"You cannot change this immutable {typename} object.";
        }

        public static string FontAlreadyAdded(string fontname)
        {
            return $"Fontface with the name '{fontname}' already added to font collection.";
        }

        public static string NotImplementedForFontsRetrievedWithFontResolver(string name)
        {
            return $"Not implemented for font '{name}', because it was retrieved with font resolver.";
        }

        #endregion

        #region PDF specific messages

        // ----- PDF ----------------------------------------------------------------------------------

        public static string InvalidPdf => "The file is not a valid PDF document.";

        public static string InvalidVersionNumber => "Invalid version number. Valid values are 12, 13, and 14.";

        public static string CannotHandleXRefStreams => "Cannot handle cross-reference streams. The current implementation of PDFsharp cannot handle this PDF feature introduced with Acrobat 6.";

        public static string PasswordRequired => "A password is required to open the PDF document.";

        public static string InvalidPassword => "The specified password is invalid.";

        public static string OwnerPasswordRequired => "To modify the document the owner password is required";

        public static string UserOrOwnerPasswordRequired => "At least a user or an owner password is required to encrypt the document.";

        public static string CannotModify => "The document cannot be modified.";

        public static string NameMustStartWithSlash => "A PDF name must start with a slash (/).";

        public static string ImportPageNumberOutOfRange(int pageNumber, int maxPage, string path)
        {
            return String.Format("The page cannot be imported from document '{2}', because the page number is out of range. " +
              "The specified page number is {0}, but it must be in the range from 1 to {1}.", pageNumber, maxPage, path);
        }

        public static string MultiplePageInsert => "The page cannot be added to this document because the document already owned this page.";

        public static string UnexpectedTokenInPdfFile => "Unexpected token in PDF file. The PDF file may be corrupt. If it is not, please send us the file for service.";

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
            return String.Format("The document requires color mode {0}, but a color is defined using {1}. " +
              "Use only colors that match the color mode of the PDF document", mode, space);
        }

        public static string CannotGetGlyphTypeface(string fontName)
        {
            return Format("Cannot get a matching glyph typeface for font '{0}'.", fontName);
        }


        // ----- PdfParser ----------------------------------------------------------------------------

        public static string UnexpectedToken(string token) => $"Token '{token}' was not expected.";
        //{
        //    return Format(PSMsgID.UnexpectedToken, token);
        //    //return Format("Token '{0}' was not expected.", token);
        //}
        
        #endregion

        #region Resource manager

        //        /// <summary>
        //        /// Gets the resource manager for this module.
        //        /// </summary>
        //        public static ResourceManager ResMngr
        //        {
        //            get
        //            {
        //                if (_resmngr == null)
        //                {
        //                    try
        //                    {
        //                        Lock.EnterFontFactory();
        //                        if (_resmngr == null)
        //                        {
        //#if true_
        //                            // Force the English language.
        //                            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        //#endif
        //                            _resmngr = new ResourceManager("PdfSharp.Resources.Messages", Assembly.GetExecutingAssembly());
        //                        }
        //                    }
        //                    finally { Lock.ExitFontFactory(); }
        //                }
        //                return _resmngr;
        //            }
        //        }

        //        static ResourceManager _resmngr;

        ///// <summary>
        ///// Writes all messages defined by PSMsgID.
        ///// </summary>
        //[Conditional("DEBUG")]
        //public static void TestResourceMessages()
        //{
        //    string[] names = Enum.GetNames(typeof(PSMsgID));
        //    foreach (string name in names)
        //    {
        //        string message = String.Format("{0}: '{1}'", name, ResMngr.GetString(name));
        //        Debug.Assert(message != null);
        //        Debug.WriteLine(message);
        //    }
        //}

        //static PSSR()
        //{
        //    TestResourceMessages();
        //}

        #endregion
    }
}