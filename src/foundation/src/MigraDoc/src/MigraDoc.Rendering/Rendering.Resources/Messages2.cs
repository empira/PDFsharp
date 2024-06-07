// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Resources;
using System.Reflection;
#if DEBUG
using System.Text.RegularExpressions;
#endif

namespace MigraDoc.Rendering.Resources
{
    /// <summary>
    /// Provides diagnostic messages taken from the resources.
    /// </summary>
    static class Messages2
    {
        internal static string NumberTooLargeForRoman(int number)
        {
            return FormatMessage(IDs.NumberTooLargeForRoman, number);
        }

        internal static string NumberTooLargeForLetters(int number)
        {
            return FormatMessage(IDs.NumberTooLargeForLetters, number);
        }

        internal static string DisplayEmptyImageSize
            => FormatMessage(IDs.DisplayEmptyImageSize);

        internal static string DisplayImageFileNotFound 
            => FormatMessage(IDs.DisplayImageFileNotFound);

        internal static string DisplayInvalidImageType => FormatMessage(IDs.DisplayInvalidImageType);

        internal static string DisplayImageNotRead => FormatMessage(IDs.DisplayImageNotRead);

        internal static string PropertyNotSetBefore(string propertyName, string functionName)
        {
            return FormatMessage(IDs.PropertyNotSetBefore, propertyName, functionName);
        }

        internal static string BookmarkNotDefined(string bookmarkName)
        {
            return FormatMessage(IDs.BookmarkNotDefined, bookmarkName);
        }

        internal static string ImageNotFound(string imageName)
        {
            return FormatMessage(IDs.ImageNotFound, imageName);
        }

        internal static string InvalidImageType(string type)
        {
            return FormatMessage(IDs.InvalidImageType, type);
        }

        internal static string ImageNotReadable(string imageName, string innerException)
        {
            return FormatMessage(IDs.ImageNotReadable, imageName, innerException);
        }

        internal static string EmptyImageSize => FormatMessage(IDs.EmptyImageSize);

        internal static string ObjectNotRenderable => FormatMessage(IDs.ObjectNotRenderable);

        // ReSharper disable InconsistentNaming
        enum IDs
        {
            PropertyNotSetBefore,
            BookmarkNotDefined,
            ImageNotFound,
            InvalidImageType,
            ImageNotReadable,
            EmptyImageSize,
            ObjectNotRenderable,
            NumberTooLargeForRoman,
            NumberTooLargeForLetters,
            DisplayEmptyImageSize,
            DisplayImageFileNotFound,
            DisplayInvalidImageType,
            DisplayImageNotRead
        }
        // ReSharper restore InconsistentNaming

        static ResourceManager ResourceManager
        {
            // ReSharper disable ConvertIfStatementToNullCoalescingExpression
            get
            {
                if (_resourceManager == null)
                    _resourceManager = new ResourceManager("MigraDoc.Rendering.Resources.Messages", Assembly.GetExecutingAssembly());

                return _resourceManager;
            }
            // ReSharper restore ConvertIfStatementToNullCoalescingExpression
        }

        static ResourceManager? _resourceManager;

        static string FormatMessage(IDs id, params object[] args)
        {
            string? message;
            try
            {
                message = ResourceManager.GetString(id.ToString());
                if (message != null)
                {
#if DEBUG
                    if (Regex.Matches(message, @"\{[0-9]\}").Count > args.Length)
                    {
                        //TODO too many placeholders or too few args...
                    }
#endif
                    message = String.Format(message, args);
                }
                else
                    message = "<<<error: message not found>>>";
                return message;
            }
            catch (Exception /*ex*/)
            {
                //message = "INTERNAL ERROR while formatting error message: " + ex;
                message = "INTERNAL ERROR while formatting error message: " + id;
            }
            return message;
        }
    }
}
