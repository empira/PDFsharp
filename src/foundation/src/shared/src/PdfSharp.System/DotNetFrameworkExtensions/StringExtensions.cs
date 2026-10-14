// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.CompilerServices;

#pragma warning disable IDE0130

namespace PdfSharp.DotNetFrameworkExtensions
{
    //#if !NET8_0_OR_GREATER
    //    /// <summary>
    //    /// Extension methods for functionality missing in .NET Framework.
    //    /// </summary>
    //    public static class SystemStringExtensions
    //    {
    //        /// <summary>
    //        /// Brings "bool StartsWith(char value)" to String class.
    //        /// </summary>
    //        public static bool StartsWith(this string @string, char value) => @string.Length != 0 && @string[0] == value;

    //        /// <summary>
    //        /// Brings "bool EndsWith(char value)" to String class.
    //        /// </summary>
    //        public static bool EndsWith(this string @string, char value) => @string.Length != 0 && @string[^1] == value;
    //    }
    //#endif

    /// <summary>
    /// PDFsharp extensions for .NET types.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Extensions for String.
        /// </summary>
        extension(String? s)
        {
            /// <summary>
            /// The functionality I missed since C# 1.0.
            /// </summary>
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool NullOrEmpty => String.IsNullOrEmpty(s);

#if !NET8_0_OR_GREATER

            // Extensions for .NET functionality missing in .NET Standard/Framework.

            /// <summary>
            /// Brings "bool StartsWith(char value)" to String class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool StartsWith(char ch) => s?.Length != 0 && s?[0] == ch;

            /// <summary>
            /// Brings "bool EndsWith(char value)" to String class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool EndsWith(char ch) => s?.Length != 0 && s?[^1] == ch;
#endif
        }
    }
}
