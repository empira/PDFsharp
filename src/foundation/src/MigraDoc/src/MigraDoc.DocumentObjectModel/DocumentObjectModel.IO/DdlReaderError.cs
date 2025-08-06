// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Represents an error or diagnostic message reported by the DDL reader.
    /// </summary>
    // Better name would be DdlReaderProblemDetails. But this class name
    // is public. Therefore, we stay with it.
    public class DdlReaderError
    {
        /// <summary>
        /// Initializes a new instance of the DdlReaderError class.
        /// </summary>
        internal DdlReaderError(DdlErrorLevel level, MdDomMsg domMsg,
            string sourceFile = "", int sourceLine = 0, int sourceColumn = 0)
        {
            ErrorLevel = level;
            ErrorMessage = domMsg.Message;
            ErrorNumber = (int)domMsg.Id;
            SourceFile = sourceFile;
            SourceLine = sourceLine;
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Initializes a new instance of the DdlReaderError class.
        /// </summary>
        public DdlReaderError(DdlErrorLevel errorLevel, string errorMessage, int errorNumber,
            string sourceFile, int sourceLine, int sourceColumn)
        {
            ErrorLevel = errorLevel;
            ErrorMessage = errorMessage;
            ErrorNumber = errorNumber;
            SourceFile = sourceFile;
            SourceLine = sourceLine;
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Returns a string that represents the current DdlReaderError.
        /// </summary>
        public override string ToString() 
            => $"[{SourceFile}({SourceLine},{SourceColumn}):] {"xxx"} DDL{ErrorNumber}: {ErrorMessage}";

        /// <summary>
        /// Specifies the severity of this diagnostic.
        /// </summary>
        public readonly DdlErrorLevel ErrorLevel;

        /// <summary>
        /// Specifies the diagnostic message text.
        /// </summary>
        public readonly string ErrorMessage;

        /// <summary>
        /// Specifies the diagnostic number.
        /// </summary>
        public readonly int ErrorNumber;

        /// <summary>
        /// Specifies the filename of the DDL text that caused the diagnostic,
        /// or an empty string ("").
        /// </summary>
        public readonly string SourceFile;

        /// <summary>
        /// Specifies the line of the DDL text that caused the diagnostic (1-based),
        /// or 0 if there is no line information. 
        /// </summary>
        public readonly int SourceLine;

        /// <summary>
        /// Specifies the column of the source text that caused the diagnostic (1-based),
        /// or 0 if there is no column information.
        /// </summary>
        public readonly int SourceColumn;
    }
}
