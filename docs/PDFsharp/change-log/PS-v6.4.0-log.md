# PDFsharp 6.4.0 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.4

**This file is not yet up-to-date.**

### Breaking changes

**PdfDictionary enumerator**  
=> PdfItem? -> PdfItem

**PdfDate**
* PdfDate is now based on DateTimeOffset
* Value is now nullable in getters
* tostring returns "" if null
* Still trailing ‘'’ if not ‘Z’.
* Change in MigraDoc accordingly

**PdfRectangle**
* GetRectangle is nullable*

**minor changes**
* Some PDF annotations lost their default constructor because the constructor now requires a PDF document parameter.


### Features

* PDF object model revised
* 

**PDF/A enhancements**
* PdfAManager

**File embedding**
* **FileManager**

**XMP metadata**
* preserve, update

**PdfDate**  
**PdfDate** now based on **DateTimeOffset** internally and corrects minor issues caused by
differences between .NET and .NET Framework. Fix in 6.3.0

**PdfName**  
**PdfName** now based on new class **Name** and fixed some minor issues related to UTF-8 encoding, 
escaping delimiter characters, and handling of the empty name.

**PdfDictionary**
Now implements IEnumerable<KeyValuePair<string, PdfItem>> and not 
IEnumerable<KeyValuePair<string, PdfItem?>> anymore.

### Issues

**PdfGraphicsState fixes**  
Usually not relevant as the error affected only drawing when starting with transparent strokes and fills. (GitHub #281)
