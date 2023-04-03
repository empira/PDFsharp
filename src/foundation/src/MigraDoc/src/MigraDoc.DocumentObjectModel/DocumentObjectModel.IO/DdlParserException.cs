// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Represents an exception used by the DDL parser. This exception will always be caught inside
    /// the DDL parser.
    /// </summary>
    class DdlParserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DdlParserException class with the specified message.
        /// </summary>
        public DdlParserException(string message)
            : base(message)
        {
            Error = new DdlReaderError(DdlErrorLevel.Error, message, 0);
        }

        /// <summary>
        /// Initializes a new instance of the DdlParserException class with the specified message and the
        /// inner exception.
        /// </summary>
        public DdlParserException(string message, Exception innerException)
            : base(message, innerException)
        {
            Error = new DdlReaderError(DdlErrorLevel.Error, message, 0);
        }

        /// <summary>
        /// Initializes a new instance of the DdlParserException class with the specified error level, name,
        /// error code and message.
        /// </summary>
        public DdlParserException(DdlErrorLevel level, string message, DomMsgID errorCode)
            : base(message)
        {
            Error = new DdlReaderError(level, message, (int)errorCode);
        }

        /// <summary>
        /// Initializes a new instance of the DdlParserException class with the DdlReaderError.
        /// </summary>
        public DdlParserException(DdlReaderError error)
            : base(error.ErrorMessage)
        {
            Error = error;
        }

        /// <summary>
        /// Gets the DdlReaderError.
        /// </summary>
        public DdlReaderError Error { get; }
    }
}
