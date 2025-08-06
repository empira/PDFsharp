﻿# 6.2.0 change log

This text is copied to `docs.pdfsharp.net`.

## Version 6.2

### PDFsharp 6.2.0 (final release)

*not yet released*

### PDFsharp 6.2.0 Preview 3

#### General issues

#### PDFsharp features

**Improved access to CropBox, ArtBox, BleedBox, TrimBox**  
PdfPage now has new properties that make access to those boxes easier.
These are `HasBleedBox`, `BleedBoxReadOnly`, and `EffectiveBleedBoxReadOnly`.  
Same applies to ArtBox, CropBox and TrimBox.

**Loading images: Improved access to buffer of MemoryStream**  
LoadImage from MemoryStream now works with a buffer that is not publicly visible.  
For better performance, set 'publiclyVisible' to true when creating the MemoryStream.

#### PDFsharp issues

**Lexer.ScanNumber and CLexer.ScanNumber**  
Based on a bug that crashes PDFsharp if a number in a PDFfile has to much leading zeros, we revised the code of **Lexer.ScanNumber** and **CLexer.ScanNumber**.

**No more commas allowed in XUnit**  
An old hack allows **XUnit** to be assigned from a string that uses a comma as decimal separator.
This was a introduced for languages like German, that use a comma instead of a point as decimal separator.
Now you get an exception. Applies also to **XUnitPt**.

#### MigraDoc features

**MigraDoc Preview user control restored for GDI build**  
The MigraDoc Preview user control is again available in the GDI+ build.
MigraDoc Preview samples for GDI and WPF have been added to the samples repository.

**Color.Parse no longer causes exceptions when invoked with a number value**  
If Color.Parse is invoked with a number value, as in `var color = Color.Parse("#f00");`,
it no longer throws a handled exception internally.

#### MigraDoc issues


---

### PDFsharp 6.2.0 Preview 2

#### General features

**CoreBuildFontResolver removed**  
The code was removed from PDFsharp, but still exists in the samples repository.  
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

We have recently removed the code that read UTF-16LE strings because that format was not mentioned in the PDF specs.
We found a PDF file created with Adobe PDF Library 18.0.3 that contains UTF-16 strings with a LE BOM.
PDFsharp now can again read UTF-16 strings with LE BOM.

#### General issues

**Updated NuGet package references to avoid vulnerability warnings**  
The warnings came from transitive packages, i.e. packages used by packages that were referenced by PDFsharp.
The vulnerable code was not invoked by PDFsharp.

**MD5 replaced**  
We use the class **MD5Managed** instead of **MD5** from .NET.
One reason is that .NET MD5 cannot be used in a FIPS-compliant environment.
Another reason is that some platforms like Blazor do not support the retired class **MD5** anymore.

#### PDFsharp features

**MD5 replaced where possible**  
The ImageSelector class needs hash codes for internal use.
We now use SHA1 instead of MD5 because MD5 is not FIPS-compliant and not supported everywhere.
Early PDF encryption uses MD5. So PDFsharp must use MD5 when opening files created with such early encryptions.
And PDFsharp must use MD5 when creating files with such early encryptions.

**Inefficient reading of object streams**  
Reading files with many objects in many object streams now works much faster.

**Ignore incorrect "/Length" entry on streams close to EOF**  
PDFsharp 6.2.0 Preview 1 failed if a PDF contained a stream close to the end of file with an incorrect /Length attribute.
PDFsharp 6.2.0 Preview 2 can correct the /Length attribute even for streams close to the end of the file.

**ImageSelector improved**  
We fixed incorrect handling of identical images with only different masks.

**Issue with Google Slides**  
PDFsharp can now open PDF files created from Google Slides.

**Size of digital signature may vary without timestamp**  
PDFsharp 6.2.0 Preview 1 took into account that the size of digital signatures with timestamp could vary from call to call.
PDFsharp 6.2.0 Preview 2 now takes into account that the size of digital signatures without timestamp can also vary from call to call.

**Option to read images from non-seekable streams**  
PDFsharp is now able to read images from streams that are not seekable.

#### PDFsharp issues

**Bug fixes in PNG reader**  
Fixed issues with reading PNG files in Core build.

**Correct datetime format in XMP meta data**  
Time zone was missing.

**Handle Reverse Solidus without effect correctly**  
The reverse solidus was handled incorrectly for invalid combinations of reverse solidus and other characters.

**Fixes with encrypted PDFs**  
Fixes for issues with encrypted PDFs.

**Improved behavior or better error messages for streams with limited capabilities**  
PDFsharp now shows better error messages when trying to read a PDF from streams that cannot seek, cannot report their length, or are otherwise limited.

**Corrected bug with page orientation**  
When opening and modifying existing PDF pages, there sometimes were issues due to page rotation and page orientation.
Behavior has changed for page format Ledger and apps supporting Ledger format must be tested and may have to be updated.
This is a breaking change.

**BeginContainer issue fixed**  
BeginContainer and EndContainer can now be used even in GDI build when drawing on PDF pages.

#### MigraDoc features

**MigraDoc: No more hard-coded fonts names**  
It is now possible to control font names used by MigraDoc for error messages and characters for bullet lists.
PDFsharp 6.2.0 Preview 1 and earlier always used `Courier New` for error messages like Image not found and used hard-coded fonts for bullets.

#### MigraDoc issues

**MigraDoc: Infinite loop in renderer**  
We fixed a bug that led to an infinite loop in PDF Renderer.

**Corrected bug with page orientation**  
When opening and modifying existing PDF pages, there sometimes were issues due to page rotation and page orientation.
Behavior has changed for page format Ledger and apps supporting Ledger format must be tested and may have to be updated.
This is a breaking change.

---

### PDFsharp 6.2.0 Preview 1

#### Features

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

#### Bug fixes

**Cleanup NuGet packages**  
MigraDoc packages now depend on PDFsharp NuGet packages instead of including assemblies directly.
All packages now depend on e.g. `Microsoft.Extensions.Logging` instead of including the Microsoft assembly.

**.restext files removed**  
Some assemblies had a subfolder `de` containing German messages.
We removed all `.restext` resources. All messages are now available in US English only.

**Bug in parsing object referenced**  
Lexer now supports white-space within object references.

**Page orientation now works as expected**  
The connection between page Width, Height, PageOrientation, and PageRotation was weird.
It was replaced by a consistent concept.
This is a breaking change.
