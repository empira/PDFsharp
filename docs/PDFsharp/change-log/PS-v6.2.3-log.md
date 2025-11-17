# PDFsharp 6.2.3 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.2.3

### Breaking changes

*(none)*

### Features

*(none)*

### Issues

**Fixed problem with JPEG images from XImage.FromGdiPlusImage**  
JPEG images created from GDI+ images were not handled correctly. GitHub #318  
This API is only available in the GDI+ build of PDFsharp.  
It is recommended to avoid the *XImage.FromGdiPlusImage* API and use *XImage.FromFile* or *XImage.FromStream* instead if possible.

**Avoid exception when reading certain corrupted files**  
An incorrectly formatted logging string caused an exception when logging a warning about some corrupted PDF files. GitHub #290
