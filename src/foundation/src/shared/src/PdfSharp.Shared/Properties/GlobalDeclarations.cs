// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//global using System.IO;

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
//[assembly: SuppressMessage("LoggingGenerator", "SYSLIB1006:Multiple logging methods cannot use the same event ID within a class",
//    Justification = "We use logging event IDs as documented, i.e. multiple times", Scope = "member"/*, Target = "~M:PdfSharp.Internal.Logging.LogMessages.XGraphicsCreated(Microsoft.Extensions.Logging.ILogger,System.String)"*/)]
