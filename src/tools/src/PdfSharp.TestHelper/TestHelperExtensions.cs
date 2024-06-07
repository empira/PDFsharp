// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.Reflection;

namespace PdfSharp.TestHelper
{
    public static class TestHelperExtensions
    {
        public static string[] Splitter(this string text, string sep)
        {
            var result = new List<string>();
            while (true)
            {
                int idx = text.IndexOf(sep, StringComparison.Ordinal);
                if (idx < 0)
                {
                    result.Add(text);
                    return result.ToArray();
                }

                result.Add(text.Substring(0, idx));
                text = text.Substring(idx + sep.Length);
            }
        }

        public static string GetOriginalLocation(this Assembly assembly)
        {
#if NET6_0_OR_GREATER
            var dllPath = assembly.Location;
#else
            // In net 4.7.2 assembly.Location returns a temporary folder, when executed via Test Explorer.
            // Get the original dll’s path via CodeBase instead.
            var dllPath = assembly.CodeBase;
            if (dllPath.StartsWith("file:///"))
                dllPath = dllPath.Substring(8);
#endif
            return dllPath;
        }

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Split the elements of a sequence into chunks of size at most <paramref name="size"/>.
        /// </summary>
        /// <remarks>
        /// Every chunk except the last will be of size <paramref name="size"/>.
        /// The last chunk will contain the remaining elements and may be of a smaller size.
        /// </remarks>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> whose elements to chunk.
        /// </param>
        /// <param name="size">
        /// Maximum size of each chunk.
        /// </param>
        /// <typeparam name="TSource">
        /// The type of the elements of source.
        /// </typeparam>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that contains the elements the input sequence split into chunks of size <paramref name="size"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="size"/> is below 1.
        /// </exception>
        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            return ChunkIterator(source, size);
        }

        static IEnumerable<TSource[]> ChunkIterator<TSource>(IEnumerable<TSource> source, int size)
        {
            using IEnumerator<TSource> e = source.GetEnumerator();
            while (e.MoveNext())
            {
                TSource[] chunk = new TSource[size];
                chunk[0] = e.Current;

                int i = 1;
                for (; i < chunk.Length && e.MoveNext(); i++)
                {
                    chunk[i] = e.Current;
                }

                if (i == chunk.Length)
                {
                    yield return chunk;
                }
                else
                {
                    Array.Resize(ref chunk, i);
                    yield return chunk;
                    yield break;
                }
            }
        }
#endif
    }
}
