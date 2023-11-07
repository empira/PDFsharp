// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//
// Documentation of conditional compilation symbols used in PDFsharp.
// Checks correct settings and obsolete conditional compilation symbols.
//

//#if New/ViewMatrix // obsolete
//#error NewViewMatrix must not be defined anymore.
//#endif

//#if MIGRA/D7OC  // obsolete
//// empira internal only: Some hacks that make PDFsharp behave like PDFlib when used with Asc.RenderContext.
//// Applies to MigraDoc 1.2 only. The Open Source MigraDoc lite does not need this define.
//#error MIGRA/D7OC must not be defined anymore.
//#endif

// take advantage of .net 5+/core's System.IO.Compression new features.
// if not, use PdfSharp internal SharpZipLib implementation.
// cannot use with .netstandard/netframework!
#if NET_ZIP  // not obsolete
#endif

#if NET_2_0  // obsolete
#error Undefine 'NET_2_0' because earlier versions are not supported anymore.
#endif

#if Gdip  // obsolete
#error Conditional compilation symbol 'Gdip' was renamed to 'GDI'.
#endif

// Fragmentation of large object heap is a serious issue that must be tackled in the future.
// Update: .NET 4.51 can ultimately defragment LOH. So maybe we can wait and see.
#if UseMemoryStreams
// Use MemoryStream instead of byte[] to avoid large heap problems.
#error Undefine 'UseMemoryStreams' because it has no effect anymore.
#else
// Use byte[] (instead of MemoryStream) to analyze the symptoms of large heap issues.
#endif

#if GDI && WPF
// PDFsharp based on both System.Drawing and System.Windows classes
// This is for developing and cross testing only

#elif GDI
// PDFsharp based on System.Drawing classes

#elif WPF
// PDFsharp based on Windows Presentation Foundation.

#elif CORE
// PDFsharp independent of any particular .NET library.

#elif UWP
// PDFsharp based on 'Windows Universal Platform'.
#error UWP is not supported anymore
#else
#error Either 'CORE', 'GDI', 'WPF', or 'UWP' must be defined.
#endif
