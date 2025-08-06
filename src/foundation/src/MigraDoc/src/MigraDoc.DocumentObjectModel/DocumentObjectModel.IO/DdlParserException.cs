// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// Represents an exception issued by the DDL parser.
    /// This exception will always be caught inside the DDL parser.
    /// </summary>
    sealed class DdlParserException : Exception
    {
        ///// <summary>
        ///// Initializes a new instance of the DdlParserException class with the specified message.
        ///// </summary>
        //public DdlParserException(MdDomMsg domMsg)
        //    : base(domMsg.Message)
        //{
        //    Error = new DdlReaderError(DdlErrorLevel.Error, domMsg);
        //}

        /// <summary>
        /// Initializes a new instance of the DdlParserException class with the specified message and the
        /// inner exception.
        /// </summary>
        public DdlParserException(MdDomMsg domMsg, Exception? innerException = null)
            : base(domMsg.Message, innerException)
        {
            Error = new DdlReaderError(DdlErrorLevel.Error, domMsg);
        }

        ///// <summary>
        ///// Initializes a new instance of the DdlParserException class with the specified error level, name,
        ///// error code and message.
        ///// </summary>
        //public DdlParserException(DdlErrorLevel level, string message, DomMsgId errorCode)
        //    : base(message)
        //{
        //    Error = new DdlReaderError(level, message, (int)errorCode);
        //}

        /// <summary>
        /// Initializes a new instance of the DdlParserException class with the DdlReaderError.
        /// </summary>
        public DdlParserException(DdlReaderError error, Exception? innerException = null)
            : base(error.ErrorMessage, innerException)
        {
            Error = error;
        }

        /// <summary>
        /// Gets the DdlReaderError.
        /// </summary>
        public DdlReaderError Error { get; }
    }
}
