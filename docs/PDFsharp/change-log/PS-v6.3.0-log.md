# PDFsharp 6.3.0 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.3

**This file is not yet up-to-date.**

### Breaking changes

**PdfDictionary enumerator**  
=> PdfItem? -> PdfItem

**xxxx**
xxxx.

### Features

* PDF object model revised
* 

**PDF/A enhancements**
* PdfAManager

**File embedding**
* **FileManager**

**XMP metadata**
* xxx

**PdfDate**  
**PdfDate** now based on **DateTimeOffset** internally and corrects minor issues caused by
differences between .NET and .NET Framework.

**PdfName**  
**PdfName** now based on new class **Name** and fixed some minor issues related to UTF-8 encoding, 
escaping delimiter characters, and handling of the empty name.

**PdfDictionary**
Now implements IEnumerable<KeyValuePair<string, PdfItem>> and not 
IEnumerable<KeyValuePair<string, PdfItem?>> anymore.

### Issues

**PdfGraphicsState fixes**  
Usually not relevant as the error affected only drawing when starting with transparent strokes and fills. (GitHub #281)
