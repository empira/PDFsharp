# PDFsharp 6.2.2 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.2.2

### Breaking changes

*(none)*

### Features

*(none)*

### Issues

**Fixed problem with JPEG images from MemoryStreams**  
In special cases, JPEG images from a MemoryStream were not handled correctly. GitHub #303

**Avoid crash on ID duplicates in an object stream**  
PDF files with duplicate IDs in an object stream were not handled correctly.
We consider such files corrupted, but PDFsharp should now handle them correctly. GitHub #296

**Fixed problems with signatures in DEBUG build**  
The Debug build of PDFsharp adds comments to the generated PDF files.
This caused problems with the Signature field in PDFs, but was fixed now.Github #293
