# PDFsharp 6.2.0 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.2

### Breaking changes

**Page orientation now works as expected**  
See *Issues* below.

### Features

**Improved access to CropBox, ArtBox, BleedBox, TrimBox**  
PdfPage now has new properties that make access to those boxes easier.
These are `HasBleedBox`, `BleedBoxReadOnly`, and `EffectiveBleedBoxReadOnly`.  
Same applies to ArtBox, CropBox and TrimBox.

**Loading images: Improved access to buffer of MemoryStream**  
LoadImage from MemoryStream now works with a buffer that is not publicly visible.  
For better performance, set 'publiclyVisible' to true when creating the MemoryStream.

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

### Issues

**Lexer.ScanNumber and CLexer.ScanNumber**  
Based on a bug that crashed PDFsharp if a number in a PDFfile has too many leading zeros, we revised the code of **Lexer.ScanNumber** and **CLexer.ScanNumber**.

**No more commas allowed in XUnit**  
An old hack allowed **XUnit** to be assigned from a string that uses a comma as decimal separator.
This was introduced for languages like German that use a comma instead of a point as decimal separator.
Now you get an exception. Applies also to **XUnitPt**.

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

**Bug in parsing object referenced**  
Lexer now supports white-space within object references.

**Page orientation now works as expected**  
The connection between page Width, Height, PageOrientation, and PageRotation was weird.
It was replaced by a consistent concept.
This is a breaking change.
