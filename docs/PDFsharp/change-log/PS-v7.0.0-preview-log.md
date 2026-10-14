# PDFsharp 7.0.0 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 7.0

### Breaking changes

**PdfDictionary enumerator**  
Item is no longer nullable, so **PdfItem?** became **PdfItem**.

**PdfRectangle** changes  
**PdfRectangle.Empty** is now obsolete and throws an exception.  
Use `new PdfRectangle()` to initialize empty values and to compare if a result of a call is the empty rectangle.

**MediaBox**, **CropBox**, **BleedBox**, **ArtBox**, **TrimBox** changes  
Now obsolete: MediaBoxReadOnly, CropBoxReadOnly, BleedBoxReadOnly, ArtBoxReadOnly, TrimBoxReadOnly.  
Use **HasMediaBox**, **HasCropBox**, etc. to check if the boxes have been set.  
Use **EffectiveCropBoxReadOnly**, **EffectiveTrimBoxReadOnly**, etc. to get the effective value.

**PdfDate**
* PdfDate is now based on DateTimeOffset
* Value is now nullable in getters
* ToString returns "" if null
* Still trailing ‘'’ if not ‘Z’
* Changed in MigraDoc accordingly

**PdfRectangle**
* GetRectangle is nullable

**PdfAnnotation**
* Keys were moved from the base class **PdfAnnotation** to the specific class where they belong.  
  For example, the `/A` key can now be found under `PdfLinkAnnotation.Keys.A`.  
  Other keys were moved to the **PdfMarkupAnnotation** class.

**Minor changes**
* Some PDF annotation classes lost their default constructor because the constructor now requires a PDF document parameter.  
* Class **PdfRubberStampAnnotation** renamed to **PdfStampAnnotation**.  
* Enum **PdfRubberStampAnnotationIcon** renamed to **PdfStampAnnotationIcons**.  
* Enum **PdfTextAnnotationIcon** renamed to **PdfTextAnnotationIcons**.

### Features

* PDF object model revised.
* PDFsharp is now more forgiving when opening certain non-conforming PDF files.

**PDF/A enhancements**
* PdfAManager

**PDF Forms enhancements**
* New classes to support Forms and form elements
* PsX Forms makes using Forms much easier, see https://www.pdfsharp.com/Offers

**File embedding**
* New **FileManager**

**PDF Annotations enhancements**
* New classes to support Annotations
* Outlook: PsX Annotations (coming soon) will make using annotations much easier, see https://www.pdfsharp.com/Offers

**XMP metadata**
* New class MetadataManager
* DocumentMetadataStrategy: KeepExisting, AutoGenerate, UserGenerated, NoMetadata

**PdfDate**  
**PdfDate** now based on **DateTimeOffset** internally and corrects minor issues caused by
differences between .NET and .NET Framework. Fixed in 6.3.0

**PdfName**  
**PdfName** now based on new class **Name** and fixed some minor issues related to UTF-8 encoding, 
escaping delimiter characters, and handling of the empty name.

**PdfDictionary**
Now implements IEnumerable<KeyValuePair<string, PdfItem>> and not 
IEnumerable<KeyValuePair<string, PdfItem?>> anymore.

### Issues

**PdfGraphicsState fixes**  
Usually not relevant as the error affected only drawing when starting with transparent strokes and fills. (GitHub #281)
