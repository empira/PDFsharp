// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.CompilerServices;

#pragma warning disable IDE0130

namespace PdfSharp.DotNetFrameworkExtensions
{
    /// <summary>
    /// PDFsharp extensions for .NET types.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Extensions for Enum.
        /// </summary>
        extension(Enum)
        {
#if !NET8_0_OR_GREATER
            // Extensions for .NET functionality missing in .NET Standard/Framework.

            /// <summary>
            /// Brings "Parse&lt;T&gt;(string)" to Enum.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T Parse<T>(string name, bool ignoreCase) where T : struct, Enum
            {
                var enumValue = (int)Enum.Parse(typeof(T), name, ignoreCase);
                T result = (T)(enumValue as object);
                return result;
            }
#endif
        }
    }
}
