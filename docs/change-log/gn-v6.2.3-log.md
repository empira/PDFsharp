# General 6.2.3 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **general** `History.md`.

## What’s new in version 6.2.3

### Breaking changes

With version 6.2.3, we added support for .NET 9 and .NET 10.  
Version 6.2.3 no longer compiles against .NET 6 which is out of support.  
The NuGet packages can still be used for applications that use .NET 6.  
Version 6.2.3 was built using Visual Studio 2026.
It still compiles with Visual Studio 2022, provided the .NET 10 SDK is installed, but warnings will be shown.
You can remove .NET 10 from the list of target frameworks if you do not need it.

### General features

*(none)*

### General issues

The bug fixes of PDFsharp are useful when generating PDF files from PDFsharp or MigraDoc.
