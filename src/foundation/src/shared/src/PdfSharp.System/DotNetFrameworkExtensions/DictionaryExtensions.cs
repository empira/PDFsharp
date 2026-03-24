// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if !NET8_0_OR_GREATER 

using System.Runtime.CompilerServices;

#pragma warning disable IDE0130

// Extensions for .NET functionality missing in .NET Standard/Framework.

namespace PdfSharp.DotNetFrameworkExtensions
{
    /// <summary>
    /// Extensions for Dictionary.
    /// </summary>
    public static class DictionaryExtensions
    {
        extension<TKey, TValue>(Dictionary<TKey, TValue> dict) where TKey : notnull
        {
            /// <summary>
            /// Implementation of TryAdd.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAdd(TKey key, TValue value)
            {
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                    return true;
                }
                return false;
            }
        }
    }
}
#endif
