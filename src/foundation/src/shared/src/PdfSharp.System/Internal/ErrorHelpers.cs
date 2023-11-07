// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Internal
{
    /// <summary>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    static class TH
    {
        // Microsoft throws in the helper function. Maybe this creates less code.
        // But we decided to throw at the position where the problem happens and 
        // therefore we only compose the exception here and return it.

        //[DoesNotReturn]
        public static void ThrowInvalidOperationException_ToDo(string? info = null)
        {
            var message = info == null
                ? "Here is something to do."
                : $"Here is something to do: {info}";

            throw new InvalidOperationException(message);
        }

        //[DoesNotReturn]
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

        //private static ArgumentException GetArgumentException(ExceptionResource resource)
        //{
        //    return new ArgumentException(GetResourceString(resource));
        //}
    }
}
