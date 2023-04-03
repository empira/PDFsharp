//// PDFsharp - A .NET library for processing PDF
//// See the LICENSE file in the solution root for more information.

//using System.Diagnostics;

//namespace PdfSharp.Diagnostics
//{
//    public static class DebugAssert
//    {
//        // Comes from empira Application Framework and
//        // TODO: Needs to be remastered

//        /// <summary>
//        /// Throws a DebugAssertException if the specified condition is not True.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void True(bool condition, string? message = null)
//        {
//            if (condition is false)
//            {
//                message ??= Invariant($"Condition must be true.");
//#if true
//                if (Debugger.IsAttached || DebugAssertBehavior.Rethrow)
//                    throw new DebugAssertException(message);
//#else
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Condition must be true.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//#endif
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified condition is not False.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void False(bool condition, string? message = null)
//        {
//            if (condition is true)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Condition must be false.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified value is not null.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void Null(object? value, string? message = null)
//        {
//            if (value is not null)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Value is not null.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified value is null.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void NotNull(object? value, string? message = null)
//        {
//            if (value is null)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Value is null.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified value is not zero.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void Zero(int value, string? message = null)
//        {
//            if (value != 0)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? $"Value is not 0 but {value}.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified value is zero.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void NotZero(int value, string? message = null)
//        {
//            if (value == 0)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Value is 0.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified objects are not reference equal.
//        /// </summary>
//        //[Conditional("DEBUG")]
//        //public static void Equals(object? a, object? b, string? message = null)
//        //{
//        //    if (Object.Equals(a, b))
//        //    {

//        //        if (Debugger.IsAttached || DebugAssertBehavior.Rethrow)
//        //        {
//        //            message ??= Invariant($"'{a}' must not be equal to '{b}'.");
//        //            throw new DebugAssertException(message);
//        //        }
//        //    }

//        //    if ((a is null && b is null) || 
//        //    {
//        //        message ??= Invariant($"Condition must be true.");

//        //        if (Debugger.IsAttached || DebugAssertBehavior.Rethrow)
//        //            throw new DebugAssertException(message);
//        //    }
//        //}

//        ///// <summary>
//        ///// Throws a DebugAssertException if the specified objects are reference equal.
//        ///// </summary>
//        //[Conditional("DEBUG")]
//        //public static void NotEquals(object? a, object? b, string? message = null)
//        //{
//        //    if (Object.Equals(a,b))
//        //    {
//        //        message ??= Invariant($"'{a}' must not be equal to '{b}'.");

//        //        if (Debugger.IsAttached || DebugAssertBehavior.Rethrow)
//        //            throw new DebugAssertException(message);
//        //    }
//        //}

//        /// <summary>
//        /// Throws a DebugAssertException if the specified string is not null and not empty.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void NullOrEmpty(string? value, string? message = null)
//        {
//            if (String.IsNullOrEmpty(value) is false)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Value is neither null nor empty.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified string is null or empty.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void NotNullAndNotEmpty(string? value, string? message = null)
//        {
//            if (String.IsNullOrEmpty(value))
//            {
//                try
//                {
//                    //Show 'assert' only if an debugger is attached.
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Value is null or empty.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified values are not equal.
//        /// </summary>
//        public static void Equals(object? expected, object? actual, string? message = null)
//        {
//            if (Object.Equals(expected, actual) is false)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Values are not equal.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified values are equal.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void NotEquals(object? expected, object? actual, string? message = null)
//        {
//            if (Object.Equals(expected, actual))
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Values are equal.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified values are not equal.
//        /// </summary>
//        public static void Equal<T>(T expected, T actual, string? message = null)
//        {
//            if (EqualityComparer<T>.Default.Equals(expected, actual) is false)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Values are not equal.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified values are equal.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void NotEqual<T>(T expected, T actual, string? message = null)
//        {
//            if (EqualityComparer<T>.Default.Equals(expected, actual))
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Values are equal.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        ///// <summary>
//        ///// Throws a DebugAssertException if the specified strings are not binary equal.
//        ///// </summary>
//        //[Conditional("DEBUG")]
//        //public static void Equals(string? expected, string? actual, string? message = null)
//        //{
//        //    if (String.Equals(expected, actual, StringComparison.Ordinal) is false)
//        //    {
//        //        try
//        //        {
//        //            if (Debugger.IsAttached)
//        //                throw new DebugAssertException(message ?? "Strings are not equal.");
//        //        }
//        //        catch
//        //        {
//        //            // Catch exception to simulate "Debug.Assert (Ignore)".

//        //            if (DebugAssertBehavior.Rethrow)
//        //                throw;
//        //        }
//        //    }
//        //}

//        ///// <summary>
//        ///// Throws a DebugAssertException if the specified strings are binary equal.
//        ///// </summary>
//        //[Conditional("DEBUG")]
//        //public static void NotEquals(string? expected, string? actual, string? message = null)
//        //{
//        //    if (String.Equals(expected, actual, StringComparison.Ordinal))
//        //    {
//        //        try
//        //        {
//        //            if (Debugger.IsAttached)
//        //                throw new DebugAssertException(message ?? "Strings are equal.");
//        //        }
//        //        catch
//        //        {
//        //            // Catch exception to simulate "Debug.Assert (Ignore)".

//        //            if (DebugAssertBehavior.Rethrow)
//        //                throw;
//        //        }
//        //    }
//        //}

//        /// <summary>
//        /// Throws a DebugAssertException if the specified strings are not binary equal
//        /// and the case of the strings is ignored.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void EqualsIgnoreCase(string? expected, string? actual, string? message = null)
//        {
//            if (String.Equals(expected, actual, StringComparison.OrdinalIgnoreCase) is false)
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Strings are not equal.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        /// <summary>
//        /// Throws a DebugAssertException if the specified strings are binary equal
//        /// and the case of the strings is ignored.
//        /// </summary>
//        [Conditional("DEBUG")]
//        public static void NotEqualsIgnoreCase(string? expected, string? actual, string? message = null)
//        {
//            if (String.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
//            {
//                try
//                {
//                    if (Debugger.IsAttached)
//                        throw new DebugAssertException(message ?? "Strings are equal.");
//                }
//                catch
//                {
//                    // Catch exception to simulate "Debug.Assert (Ignore)".

//                    if (DebugAssertBehavior.Rethrow)
//                        throw;
//                }
//            }
//        }

//        ///// <summary>
//        ///// Throws a DebugAssertException.
//        ///// </summary>
//        //[Conditional("DEBUG")]
//        //[Pure]
//        //public static DebugAssertException ShouldNotComeHere(string? message = null)
//        //{
//        //    try
//        //    {
//        //        if (Debugger.IsAttached)
//        //            throw new DebugAssertException(message ?? "Should not come here.");
//        //    }
//        //    catch
//        //    {
//        //        // Catch exception to simulate "Debug.Assert (Ignore)".

//        //        if (DebugAssertBehavior.Rethrow)
//        //            throw;
//        //    }
//        //}
//    }

//    public static class DebugAssertBehavior // Experimental.
//    {
//        public static bool Rethrow { get; set; } = false;
//    }
//}
