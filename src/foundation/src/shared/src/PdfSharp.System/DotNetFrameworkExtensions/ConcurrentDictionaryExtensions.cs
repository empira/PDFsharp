// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if !NET8_0_OR_GREATER

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

using System.Collections.Concurrent;

namespace PdfSharp.DotNetFrameworkExtensions
{

    /// <summary>
    /// PDFsharp extensions for .NET types.
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Extensions for ConcurrentDictionary.
        /// </summary>
        extension<TKey, TValue>(ConcurrentDictionary<TKey,TValue> dict)
        {
            // Extensions for .NET functionality missing in .NET Standard/Framework.

            public  TValue? GetValueOrDefault(TKey key) 
                => dict.TryGetValue(key, out var value) ? value : default(TValue);
        }
    }
}
#endif
