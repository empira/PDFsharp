// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Diagnostics
{
#if true_  // #DELETE
    public static class RuntimeAssert  // Replacement for DebugAssert. DebugAssert disappears in NuGet packages.
    {
        /// <summary>
        /// Throws a RuntimeAssertException if the specified condition is not True.
        /// </summary>
        public static void True(bool condition, string? message = null)
        {
            if (condition is false)
            {
                throw new RuntimeAssertException(message ?? "Condition must be true.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified condition is not False.
        /// </summary>
        public static void False(bool condition, string? message = null)
        {
            if (condition is true)
            {
                throw new RuntimeAssertException(message ?? "Condition must be false.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified value is not null.
        /// </summary>
        public static void Null(object? value, string? message = null)
        {
            if (value is not null)
            {
                throw new RuntimeAssertException(message ?? "Value is not null.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified value is null.
        /// </summary>
        public static void NotNull(object? value, string? message = null)
        {
            if (value is null)
            {
                throw new RuntimeAssertException(message ?? "Value is null.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified value is not zero.
        /// </summary>
        public static void Zero(int value, string? message = null)
        {
            if (value != 0)
            {
                throw new RuntimeAssertException(message ?? $"Value is not 0 but {value}.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified value is zero.
        /// </summary>
        public static void NotZero(int value, string? message = null)
        {
            if (value == 0)
            {
                throw new RuntimeAssertException(message ?? "Value is 0.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified string is not null and not empty.
        /// </summary>
        public static void NullOrEmpty(string? value, string? message = null)
        {
            if (String.IsNullOrEmpty(value) is false)
            {
                throw new RuntimeAssertException(message ?? "Value is neither null nor empty.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified string is null or empty.
        /// </summary>
        public static void NotNullAndNotEmpty(string? value, string? message = null)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new RuntimeAssertException(message ?? "Value is null or empty.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified values are not equal.
        /// </summary>
        public static void Equals(object? expected, object? actual, string? message = null)
        {
            if (Object.Equals(expected, actual) is false)
            {
                if (message == null)
                    throw new RuntimeAssertException("Values are not equal.");
                throw new RuntimeAssertException(message);
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified values are equal.
        /// </summary>
        public static void NotEquals(object? expected, object? actual, string? message = null)
        {
            if (Object.Equals(expected, actual))
            {
                throw new RuntimeAssertException(message ?? "Values are equal.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified values are not equal.
        /// </summary>
        public static void Equal<T>(T expected, T actual, string? message = null)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual) is false)
            {
                if (message == null)
                    throw new RuntimeAssertException(message ?? "Values are not equal.");
                throw new RuntimeAssertException(message);
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified values are equal.
        /// </summary>
        public static void NotEqual<T>(T expected, T actual, string? message = null)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new RuntimeAssertException(message ?? "Values are equal.");
            }
        }

        /// <summary>
        /// Throws a RuntimeAssertException if the specified strings are not binary equal.
        /// </summary>
        public static void Equals(string? expected, string? actual, string? message = null)
        {
            if (String.Equals(expected, actual, StringComparison.Ordinal) is false)
            {
                throw new RuntimeAssertException(message ?? "Strings are not equal.");
            }
        }

        ///// <summary>
        ///// Throws a RuntimeAssertException if the specified strings are binary equal.
        ///// </summary>
        //public static void NotEquals(string? expected, string? actual, string? message = null)
        //{
        //    if (String.Equals(expected, actual, StringComparison.Ordinal))
        //    {
        //        throw new RuntimeAssertException(message ?? "Strings are equal.");
        //    }
        //}

        ///// <summary>
        ///// Throws a RuntimeAssertException if the specified strings are not binary equal
        ///// and the case of the strings is ignored.
        ///// </summary>
        //public static void EqualsIgnoreCase(string? expected, string? actual, string? message = null)
        //{
        //    if (String.Equals(expected, actual, StringComparison.OrdinalIgnoreCase) is false)
        //    {
        //        if (message == null)
        //            throw new RuntimeAssertException(message ?? "Strings are not equal.");
        //        throw new RuntimeAssertException(message);
        //    }
        //}

        /// <summary>
        /// Throws a RuntimeAssertException if the specified strings are binary equal
        /// and the case of the strings is ignored.
        /// </summary>
        public static void NotEqualsIgnoreCase(string? expected, string? actual, string? message = null)
        {
            if (String.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            {
                throw new RuntimeAssertException(message ?? "Strings are equal.");
            }
        }
    }
#endif
}
