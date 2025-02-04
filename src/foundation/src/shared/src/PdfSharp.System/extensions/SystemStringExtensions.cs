using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfSharp
{
#if !NET6_0_OR_GREATER
    /// <summary>
    /// Extension methods for functionality missing in .NET Framework.
    /// </summary>
    public static class SystemStringExtensions
    {
        /// <summary>
        /// Brings "bool StartsWith(char value)" to String class.
        /// </summary>
        public static bool StartsWith(this string @string, char value) => @string.Length != 0 && @string[0] == value;
    }
#endif
}
