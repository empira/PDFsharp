// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using System.Resources;
using System.Reflection;
#if DEBUG
using System.Text.RegularExpressions;
#endif

namespace MigraDoc.RtfRendering.Resources
{
    /// <summary>
    /// Provides diagnostic messages taken from the resources.
    /// </summary>
    class Messages2
    {
        internal static string TextframeContentsNotTurned
        {
            get { return FormatMessage(IDs.TextframeContentsNotTurned); }
        }

        internal static string ImageFreelyPlacedInWrongContext(string imageName)
        {
            return FormatMessage(IDs.ImageFreelyPlacedInWrongContext, imageName);
        }

        internal static string ChartFreelyPlacedInWrongContext
        {
            get { return FormatMessage(IDs.ChartFreelyPlacedInWrongContext); }
        }

        internal static string ImageNotFound(string imageName)
        {
            return FormatMessage(IDs.ImageNotFound, imageName);
        }

        internal static string ImageNotReadable(string imageName, string innerException)
        {
            return FormatMessage(IDs.ImageNotReadable, imageName, innerException);
        }

        internal static string ImageTypeNotSupported(string imageName)
        {
            return FormatMessage(IDs.ImageTypeNotSupported, imageName);
        }

        internal static string InvalidNumericFieldFormat(string format)
        {
            return FormatMessage(IDs.InvalidNumericFieldFormat, format);
        }

        internal static string CharacterNotAllowedInDateFormat(char character)
        {
            string charString = character.ToString();
            return FormatMessage(IDs.CharacterNotAllowedInDateFormat, charString);
        }

        internal static string UpdateField
        {
            get { return FormatMessage(IDs.UpdateField); }
        }

        // ReSharper disable InconsistentNaming
        enum IDs
        {
            UpdateField,
            TextframeContentsNotTurned,
            InvalidNumericFieldFormat,
            ImageFreelyPlacedInWrongContext,
            ChartFreelyPlacedInWrongContext,
            ImageNotFound,
            ImageNotReadable,
            ImageTypeNotSupported,
            CharacterNotAllowedInDateFormat
        }
        // ReSharper restore InconsistentNaming

        static ResourceManager ResourceManager
        {
            get
            {
                return _resourceManager ??= new("MigraDoc.RtfRendering.Resources.Messages", Assembly.GetExecutingAssembly());
            }
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
            catch (Exception ex)
            {
                message = "INTERNAL ERROR while formatting error message: " + ex;
            }
            return message;
        }
    }
}
