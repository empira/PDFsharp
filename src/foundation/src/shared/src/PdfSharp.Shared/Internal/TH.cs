// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591 // Because this is preview code.

using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Internal
{
    /// <summary>
    /// Experimental throw helper.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    class TH2
    {
        // Microsoft throws in the helper function. Maybe this creates less code.
        // But we decided to throw at the position where the problem happens and 
        // therefore we only compose the exception here and return it.

        public static InvalidOperationException InvalidOperationException_ToDo(
                string? info = null)
        {
            var message = info == null
                ? "Here is something to do."
                : $"Here is something to do: {info}";

            return new(message);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException_ToDo(
                string? info = null)
            => throw InvalidOperationException_ToDo(info);

        public static NotImplementedException NotImplementedException(
                string? info = null,
                Exception? innerException = null)
        {
            var message = info == null
                ? "Here is something to do."
                : $"Here is something to do: {info}";

            return innerException == null
                ? new(message)
                : new(message, innerException);
        }

        [DoesNotReturn]
        public static void ThrowNotImplementedException(
                string? info = null,
                Exception? innerException = null)
            => throw NotImplementedException(info);

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_HandleIsNotPinned()
        {
            // throw new InvalidOperationException(SR.InvalidOperation_HandleIsNotPinned);
            throw new Exception();
        }

        //[DoesNotReturn]
        //internal static void ThrowValueArgumentOutOfRange_NeedNonNegNumException()
        //{
        //    throw GetArgumentOutOfRangeException(ExceptionArgument.value,
        //        ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        //}

        //static ArgumentException GetArgumentException(ExceptionResource resource)
        //{
        //    return new ArgumentException(GetResourceString(resource));
        //}
    }
}
