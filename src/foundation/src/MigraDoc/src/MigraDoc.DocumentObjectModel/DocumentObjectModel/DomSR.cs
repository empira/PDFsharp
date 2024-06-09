// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Reflection;
using System.Resources;
#if DEBUG
using System.Text.RegularExpressions;
#endif

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// String resources of MigraDoc.DocumentObjectModel. Provides all localized text strings
    /// for this assembly.
    /// </summary>
    static class DomSR
    {
        /// <summary>
        /// Loads the message from the resource associated with the enum type and formats it
        /// using 'string.Format'. Because this function is intended to be used during error
        /// handling it never raises an exception.
        /// </summary>
        /// <param name="id">The type of the parameter identifies the resource
        /// and the name of the enum identifies the message in the resource.</param>
        /// <param name="args">Parameters passed through 'string.Format'.</param>
        /// <returns>The formatted message.</returns>
        public static string FormatMessage(DomMsgID id, params object[] args)
        {
            string? message;
            try
            {
                message = GetString(id);
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

        public static string CompareJustCells => "Only cells can be compared by this Comparer.";

        /// <summary>
        /// Gets the localized message identified by the specified DomMsgID.
        /// </summary>
        public static string? GetString(DomMsgID id)
        {
            return ResMngr.GetString(id.ToString()) ?? $"(could not get string for {id})"; // BUG ResMngr is to be deleted and replaced by string interpolation
        }

        #region How to use
#if true_
        // Message with no parameter is property.
        public static string SampleMessage1
        {
            // In the first place English only
            get { return "This is sample message 1."; }
        }

        // Message with no parameter is property.
        public static string SampleMessage2
        {
            // Then localized:
            get { return DomSR.GetString(DomMsgID.SampleMessage1); }
        }

        // Message with parameters is function.
        public static string SampleMessage3(string parm)
        {
            // In the first place English only
            //return String.Format("This is sample message 2: {0}.", parm);
        }
        public static string SampleMessage4(string parm)
        {
            // Then localized:
            return String.Format(GetString(DomMsgID.SampleMessage2), parm);
        }
#endif
        #endregion

        #region General Messages

        public static string? StyleExpected => GetString(DomMsgID.StyleExpected);

        public static string? BaseStyleRequired => GetString(DomMsgID.BaseStyleRequired);

        public static string? EmptyBaseStyle => GetString(DomMsgID.EmptyBaseStyle);

        public static string InvalidFieldFormat(string format)
        {
            return FormatMessage(DomMsgID.InvalidFieldFormat, format);
        }

        public static string InvalidInfoFieldName(string name)
        {
            return FormatMessage(DomMsgID.InvalidInfoFieldName, name);
        }

        public static string UndefinedBaseStyle(string baseStyle)
        {
            return FormatMessage(DomMsgID.UndefinedBaseStyle, baseStyle);
        }

        public static string InvalidValueName(string name)
        {
            return FormatMessage(DomMsgID.InvalidValueName, name);
        }

        public static string InvalidUnitValue(string unitValue)
        {
            return FormatMessage(DomMsgID.InvalidUnitValue, unitValue);
        }

        public static string InvalidUnitType(string unitType)
        {
            return FormatMessage(DomMsgID.InvalidUnitType, unitType);
        }

        public static string InvalidEnumValue<T>(T value) where T : Enum // struct, Enum
        {
            // ... where T : enum
            //   -or-
            // ... where T : Enum
            // is not implemented in C# because nobody has done this.
            // See Eric Lippert on this topic: http://stackoverflow.com/questions/1331739/enum-type-constraints-in-c-sharp
            // UPDATE: Enum constraint comes with C# 7.3, see: https://devblogs.microsoft.com/premier-developer/dissecting-new-generics-constraints-in-c-7-3/
            Debug.Assert(typeof(T).IsSubclassOf(typeof(Enum)));
            return FormatMessage(DomMsgID.InvalidEnumValue, value, typeof(T).Name);
        }

        public static string? InvalidEnumForLeftPosition => GetString(DomMsgID.InvalidEnumForLeftPosition);

        public static string? InvalidEnumForTopPosition => GetString(DomMsgID.InvalidEnumForTopPosition);

        public static string InvalidColorString(string colorString)
        {
            return FormatMessage(DomMsgID.InvalidColorString, colorString);
        }

        public static string InvalidFontSize(double value)
        {
            return FormatMessage(DomMsgID.InvalidFontSize, value);
        }

        public static string InsertNullNotAllowed()
        {
            return "Insert null not allowed.";
        }

        public static string ParentAlreadySet(DocumentObject value, DocumentObject docObject)
        {
            return $"Value of type '{value.GetType()}' must be cloned before setting into '{docObject.GetType()}'.";
        }

        public static string MissingObligatoryProperty(string propertyName, string className)
        {
            return FormatMessage(DomMsgID.MissingObligatoryProperty, propertyName, className);
        }

        public static string InvalidDocumentObjectType => "The given document object is not valid in this context.";

        #endregion

        #region DdlReader Messages

        #endregion

        #region Resource Manager

        public static ResourceManager ResMngr
        {
            // ReSharper disable ConvertIfStatementToNullCoalescingExpression
            get
            {
                _resmngr ??= new ResourceManager("MigraDoc.DocumentObjectModel.Resources.Messages", Assembly.GetExecutingAssembly());
                return _resmngr;
            }
            // ReSharper restore ConvertIfStatementToNullCoalescingExpression
        }

        /// <summary>
        /// Writes all messages defined by DomMsgID.
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestResourceMessages()
        {
            string[] names = Enum.GetNames(typeof(DomMsgID));
            foreach (string name in names)
            {
                string message = $"{name}: '{ResMngr.GetString(name)}'";
                Debug.WriteLine(message);
            }
        }
        static ResourceManager? _resmngr;

        #endregion
    }
}
