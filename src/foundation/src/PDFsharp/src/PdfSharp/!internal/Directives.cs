// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//
// Documentation of conditional compilation symbols used in PDFsharp.
// Checks correct settings and obsolete conditional compilation symbols.
//

#if !DEBUG && (TEST_CODE || TEST_CODE_)
// Ensure not to accidentally rename TEST_CODE to TEST_CODE_
// This would compile code previously disabled with #if TEST_CODE_
#warning *********************************************************
#warning ***** TEST_CODE MUST BE UNDEFINED FOR FINAL RELEASE *****
#warning *********************************************************
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
#error Either 'CORE', 'GDI', or 'WPF' must be defined.
#endif
