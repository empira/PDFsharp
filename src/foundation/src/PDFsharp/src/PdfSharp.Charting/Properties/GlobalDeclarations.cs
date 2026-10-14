// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

global using static System.FormattableString;
#if PSGFX
global using PdfSharp.Graphics.XGfx;
#else
global using float_ = double;
global using FLOAT_ = double;
#endif
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
