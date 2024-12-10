// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Internal
{
    /// <summary>
    /// Allows optional error handling without exceptions by assigning to a Nullable&lt;SuppressExceptions&gt; parameter.
    /// </summary>
    class SuppressExceptions  // #RENAME? It is more an ExceptionSuppressor.
    {
        /// <summary>
        /// Returns true, if an error occurred.
        /// </summary>
        public bool ErrorOccurred { get; private set; }

        /// <summary>
        /// If suppressExceptions is set, its ErrorOccurred is set to true, otherwise throwException is invoked.
        /// </summary>
        public static void HandleError(SuppressExceptions? suppressExceptions, Action throwException)
        {
            if (suppressExceptions == null)
                throwException.Invoke();

            suppressExceptions!.ErrorOccurred = true;
        }

        /// <summary>
        /// Returns true, if suppressExceptions is set and its ErrorOccurred is true.
        /// </summary>
        public static bool HasError(SuppressExceptions? suppressExceptions)
        {
            return suppressExceptions?.ErrorOccurred ?? false;
        }
    }

    // Experimental - not yet in use.

#if true_

    interface IErrorResult// TODO_OLD: Find final name
    {
        uint HResult { get; }

        string Message { get; }
    }

    interface IErrorExt : IErrorResult
    {
        Exception Ex { get; }
    }

    struct ParserError(uint HResult, string Message) : IErrorResult
    {
        public uint HResult { get; }

        public string Message { get; }
    }

    struct SomeOtherError(uint HResult, string Message) : IErrorExt
    {
        public uint HResult { get; }

        public string Message { get; }

        public Exception Ex { get; }
    }

    interface IResult<out T, out TE>
    {
        T Ok { get; }

        TE Error { get; }
    }

    enum ErrorValues
    {
        Ok = 0,
        Error = 1
    }

    readonly struct SomeResult<TPdfDocument, TException>(TPdfDocument doc, TException ex) : IResult<TPdfDocument, TException>
    {
        public TPdfDocument Ok { get; } = doc;

        public TException Error { get; } = ex;
    }

    enum ParserErrorCode
    {
        Ok = 0, Error = 1
    }

    class ParserException(ParserErrorCode code, string? message = null, Exception? innerException = null)
        : Exception(message, innerException)
    {
        public ParserErrorCode ErrorCode { get; } = code;
    }

    readonly struct ParserResult<T>(T? result) : IResult<T?, ParserException?>
    {
        public ParserResult(ParserException ex) : this(default(T))
        {
            Error = ex;
        }

        public T? Ok { get; } = result;

        public ParserException? Error { get; } = default;
    }

    class FoobarParser()
    {
        public ParserResult<SizeType> Parse1() => new ParserResult<SizeType>(42);

        public (SizeType Size, ParserException? Exception) Parse2() => (42, null);
    }
#endif
}
