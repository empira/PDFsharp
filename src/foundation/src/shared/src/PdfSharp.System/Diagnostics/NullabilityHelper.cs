// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Diagnostics
{
    /// <summary>
    /// Helper class for code migration to nullable reference types.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class NRT
    {
        /// <summary>
        /// Throws an InvalidOperationException because an expression which must not be null is null.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        //[DoesNotReturn]
        public static void ThrowOnNull(string? message = null)
            => throw new InvalidOperationException(message ?? "Expression must not be null here.");

        /// <summary>
        /// Throws InvalidOperationException. Use this function during the transition from older C# code
        /// to new code that uses nullable reference types.
        /// </summary>
        /// <typeparam name="TResult">The type this function must return to be compiled correctly.</typeparam>
        /// <param name="message">An optional message used for the exception.</param>
        /// <returns>Nothing, always throws.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        //[DoesNotReturn]
        public static TResult ThrowOnNull<TResult>(string? message = null)
            => throw new InvalidOperationException(message ?? $"'{typeof(TResult).Name}' must not be null here.");

        /// <summary>
        /// Throws InvalidOperationException. Use this function during the transition from older C# code
        /// to new code that uses nullable reference types.
        /// </summary>
        /// <typeparam name="TResult">The type this function must return to be compiled correctly.</typeparam>
        /// <typeparam name="TType">The type of the object that is null.</typeparam>
        /// <param name="message">An optional message used for the exception.</param>
        /// <returns>Nothing, always throws.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        //[DoesNotReturn]
        public static TResult ThrowOnNull<TResult, TType>(string? message = null)
           => throw new InvalidOperationException(message ?? $"'{typeof(TType).Name}' must not be null here.");
    }
}
