// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Represents an error or diagnostic message reported by the DDL reader.
    /// </summary>
    public class DdlReaderError
    {
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
        /// Initializes a new instance of the <see cref="DdlReaderError"/> class.
        /// </summary>
        public DdlReaderError(DdlErrorLevel errorLevel, string errorMessage, int errorNumber)
        {
            ErrorLevel = errorLevel;
            ErrorMessage = errorMessage;
            ErrorNumber = errorNumber;
            SourceFile = "";
        }

        //    public DdlReaderError(string errorName, DdlReaderError _level, DomMsgID _error, string message, string msg2,
        //      string DocumentFileName, int CurrentLine, int CurrentLinePos)
        //    {
        //    }
        //
        //    public DdlReaderError(string errorName, int _level, string _error, string message, string adf,
        //      string  DocumentFileName,  int CurrentLine, int CurrentLinePos)
        //    {
        //    }
        //
        //    public DdlReaderError(string errorName, DdlErrorLevel errorInfo , string _error, string message, string adf,
        //      string  DocumentFileName,  int CurrentLine, int CurrentLinePos)
        //    {
        //    }
        //
        //    public DdlReaderError(string errorName, DdlErrorLevel errorInfo , DomMsgID _error, string message, string adf,
        //      string  DocumentFileName,  int CurrentLine, int CurrentLinePos)
        //    {
        //    }

        //public const int NoErrorNumber = -1;

        /// <summary>
        /// Returns a string that represents the current DdlReaderError.
        /// </summary>
        public override string ToString()
        {
            return $"[{SourceFile}({SourceLine},{SourceColumn}):] {"xxx"} DDL{ErrorNumber}: {ErrorMessage}";
        }

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
