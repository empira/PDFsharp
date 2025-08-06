# General 6.2.0 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **general** `History.md`.

## What’s new in version 6.2

### Breaking changes

**CoreBuildFontResolver removed**  
See *General features* below.

### General features

**CoreBuildFontResolver removed**  
The code was removed from PDFsharp but still exists in the samples repository.  
To reactivate the old behavior under Windows, set *UseWindowsFontsUnderWindows* to true.  
To reactivate the old behavior under WSL2, set *UseWindowsFontsUnderWsl2* to true.  
Note that these settings only have an effect for the Core build.
Also note that these settings only have an effect under Windows or WSL2, respectively.  
No fonts will be found under Linux or MacOS, so do not use these properties if you want to develop portable Core applications.

```cs
GlobalFontSettings.UseWindowsFontsUnderWindows = true;
GlobalFontSettings.UseWindowsFontsUnderWsl2 = true;
```

The name is now SubstitutingFontResolver and it can be found in the PDFsharp.Samples repository.

[Information about font resolving](https://docs.pdfsharp.net/PDFsharp/Topics/Fonts/Font-Resolving.html)

**UTF-16LE**  

We had recently removed the code that reads UTF-16LE strings because that format was not mentioned in the PDF specs.
We found a PDF file created with Adobe PDF Library 18.0.3 that contains UTF-16 strings with a LE BOM.
PDFsharp now can again read UTF-16 strings with LE BOM.

**Digital signatures**  
PDF documents can now be signed with a digital signature.

**Multi-colored glyphs**  
Multi-colored glyphs like emojis are now supported.

**.NET Framework 4.6.2**  
All assemblies have downgraded .NET Framework references in `<TargetFrameworks>` from 4.7.2 to 4.6.2.  
`net472` becomes `net462`.

**.NET 8**  
We added .NET 8 (`net8.0`) to `<TargetFrameworks>`.

**PDF/A**  
PDF documents can now be PDF/A conforming. We are still working on this feature, so currently there are some limitations.

**PDFsharp.Shared**  
Code that grants friendly access to all PDFsharp Project assemblies lives in this new assembly.

### General issues

**Support PDF files with indirect objects in outlines**  
PDFsharp 6.2.0 no longer throws exceptions in this case.

**Support PDF files with whitespace in ASCII85 filtered objects**  
PDFsharp 6.2.0 no longer throws exceptions in this case.

**Support encrypting PDF files with .NET Framework 4.x again**  
This did not work with PDFsharp 6.2.0 Preview 2 and Preview 3.

**Updated NuGet package references to avoid vulnerability warnings**  
The warnings came from transitive packages, i.e. packages used by packages that were referenced by PDFsharp.
The vulnerable code was not invoked by PDFsharp.

**MD5 replaced**  
We use the class **MD5Managed** instead of **MD5** from .NET.
One reason is that .NET MD5 cannot be used in a FIPS-compliant environment.
Another reason is that some platforms like Blazor do not support the retired class **MD5** anymore.

**Cleanup NuGet packages**  
MigraDoc packages now depend on PDFsharp NuGet packages instead of including assemblies directly.
All packages now depend on e.g. `Microsoft.Extensions.Logging` instead of including the Microsoft assembly.

**.restext files removed**  
Some assemblies had a subfolder `de` containing German messages.
We removed all `.restext` resources. All messages are now available in US English only.
