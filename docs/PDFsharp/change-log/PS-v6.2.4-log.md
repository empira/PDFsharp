# PDFsharp 6.2.4 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.2.4

### Breaking changes

*(none)*

### Features

**Optional /Length entry for encryption added always**  
The PDF viewer of Edge fails to open some encrypted PDF files if the /Length entry is not present. GitHub #261  
We now always add the /Length entry even in cases where it is optional according to PDF specifications.

### Issues

**CArray written without spaces**  
When editing a content stream with an array, that CArray will now be saved with spaces. GitHub #300

**JPEG issue resolved**  
JPEG JFIF files use "byte stuffing" and 0xff bytes may be followed by 0x00 bytes that must be ignored.
PDFsharp 6.2.4 now handles this 0xff 0x00 combination where PDFsharp up to 6.2.3 caused an exception.
GitHub #304, #309, #310

**Indirect DecodeParms now handled correctly**  
PDFsharp 6.2.3 caused an exception if DecodeParms where specified as an indirect object. GitHub #323  
This indirection is valid, but very unusual. Fixed with 6.2.4.
