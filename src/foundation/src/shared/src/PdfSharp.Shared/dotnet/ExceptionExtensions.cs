// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// This file contains code from the .NET source to use some newer C# features in .NET Standard / Framework.
// All classes are internal, i.e. using PDFsharp packages does not make this functionality visible in
// your projects.

#if NET462 || NETSTANDARD2_0
#pragma warning disable IDE0130
namespace System
#pragma warning restore IDE0130
{
    /// <summary>
    /// Make new code compile with .NET Standard/Framework.
    /// </summary>
    /*public*/ static class ArgumentExceptionExtensions
    {
        extension(ArgumentException _)
        {
            public static void ThrowIfNull(object? value)
            {
                if (value is null)
                    throw new AggregateException("test");
            }
        }
    }

    /*public*/ static class ArgumentNullExceptionExtensions
    {
        extension(ArgumentNullException _)
        {
            public static void ThrowIfNull(object? value)
            {
                if (value is null)
                    throw new AggregateException("test");
            }
        }
    }

    /*public*/ static class ArgumentOutOfRangeExceptionExtensions
    {
        extension(ArgumentOutOfRangeException _)
        {
            public static void ThrowIfNegative(int index)
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index.ToString());
            }
        }

        extension(ArgumentOutOfRangeException _)
        {
            public static void ThrowIfGreaterThan(int value, int other)
            {
                if (value > other)
                    throw new ArgumentOutOfRangeException(nameof(value), value.ToString());
            }
        }

        extension(ArgumentOutOfRangeException _)
        {
            public static void ThrowIfLessThan(int value, int other)
            {
                if (value < other)
                    throw new ArgumentOutOfRangeException(nameof(value), value.ToString());
            }
        }
    }

    /*public*/ static class ObjectDisposedExceptionExtensions
    {

            extension(ObjectDisposedException _)
        {
            public static void ThrowIf(bool condition, Type? type)
            {
                if (condition)
                    throw new ObjectDisposedException(type?.FullName);
            }
        }
    }
}
#endif
