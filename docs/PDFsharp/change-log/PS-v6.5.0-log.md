# PDFsharp 6.5.0 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.5.0

### Breaking changes

*(none)*

### Features

**Support PDF dictionary keys that are no PDF Names**  
Dictionaries in PDF should consist of key/value pairs where the key must be a PDF Name.
If the key is a string or an integer, PDFsharp will now convert it to a PDF Name. Previous versions threw an exception and stopped parsing.
This enhances support for corrupted PDF files.

**ANSI-encoded names in PDF**  
Legacy PDF files may contain PDF Names that use ANSI encoding.
PDFsharp now supports decoding such names using local ANSI encoding.

### Issues

**Improved handling of exceptions in PNG decoding**  
Previously, exceptions during PNG decoding would lead to an "unsupported image format" exception, even if the problem was an out-of-memory situation.
Now, out-of-memory exceptions will be returned in that case.

**Unique name for signature object**  
Previously, PDFsharp would add a digital signature to a PDF file using the name "Signature1".
Now, PDFsharp ensures that a unique name is used for the signature.
