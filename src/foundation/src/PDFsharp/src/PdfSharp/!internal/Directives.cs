// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//
// Documentation of conditional compilation symbols used in PDFsharp.
// Checks correct settings and obsolete conditional compilation symbols.
//

#if !DEBUG && TEST_CODE
#warning ***********************************************************
#warning ***** ‘TEST_CODE’ MUST BE UNDEFINED FOR FINAL RELEASE *****
#warning ***********************************************************
#endif

#if TEST_CODE_  // ‘’
// Ensure not to accidentally rename ‘TEST_CODE’ to ‘TEST_CODE_’.
// This would compile code previously disabled with ‘#if TEST_CODE_’.
// Rename ‘TEST_CODE’ always to ‘TEST_CODE_xxx’ in ‘Directory.Build.targets’.
#warning *****************************************************
#warning ***** ‘TEST_CODE_’ MUST NEVER BE DEFINED        *****
#warning ***** THIS ACCIDENTALLY ACTIVATES EXCLUDED CODE *****
#warning *****************************************************
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

#elif WUI
// PDFsharp based on 'Windows Universal Platform'.
#error WUI is not supported anymore
#else
#error Either 'CORE', 'GDI', or 'WPF' must be defined.
#endif
