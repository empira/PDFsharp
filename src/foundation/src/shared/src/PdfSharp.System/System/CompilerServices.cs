// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// This file contains code from the .NET source to use some newer C# features in .NET Standard / Framework.
// All classes are internal, i.e. using PDFsharp packages does not make this functionality visible in
// your projects.

namespace System.Runtime.CompilerServices
{
#if !NET6_0_OR_GREATER
    /// <summary>
    /// Extension method GetSubArray required for the built-in range operator (e.g.'[1..9]').
    /// Fun fact: This class must be compiled into each assembly. If it is only visible through
    /// InternalsVisibleTo code will not compile with .NET Framework 4.6.2 and .NET Standard 2.0.
    /// </summary>
    /*public*/
    static class RuntimeHelpers
    {
        /// <summary>
        /// Slices the specified array using the specified range.
        /// </summary>
        public static T[] GetSubArray<T>(T[] array, Range range)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            (int offset, int length) = range.GetOffsetAndLength(array.Length);

            if (default(T) != null || typeof(T[]) == array.GetType())
            {
                // We know the type of the array to be exactly T[].
                if (length == 0)
                    //return Array.Empty<T>();
                    return [];

                var dest = new T[length];
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
            else
            {
                // The array is actually a U[] where U:T.
                var dest = (T[])Array.CreateInstance(array.GetType().GetElementType()!, length);
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
        }
    }
#endif
}
