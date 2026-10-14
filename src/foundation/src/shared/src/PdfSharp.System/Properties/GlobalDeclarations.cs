// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if USE_LONG_SIZE
global using SizeType = System.Int64;
#else
global using SizeType = System.Int32;
#endif

global using static System.FormattableString;

using System.Diagnostics.CodeAnalysis;
//using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
