# Coding issues

This article is about coding issues that may/should be fixed over time.

## Console output

We habe `Console.WriteLine` in the source code.

* Remove every occurence of `Console.WriteLine` used for diagnostic purposes
* Use `(PdfSharp/MigraDoc)LogHost.Logger`instead

## Logging

* Use semantic logging instead of string interpolation
* Use the correct logging category

## Asserts vs. exceptions

Asserts are for checking correctness of code, NOT for checking correctness of external ressources like images or PDF files.

Exceptions are for external ressources that does not fulfill the expectations.

## Nested types

We use sometimes public types nested in other public types.
Public types must be always top-level types.

You can use nested types (classes, structres or enums), but only when they are private or internal

Here is a list of such type found by Martin:

**PDFsharp**
* PngBuilder.SaveOptions
* XGraphics.SpaceTransformer
* XGraphics.XGraphicsInternals
* PdfArray.ArrayElements
* PdfDictionary.DictionaryElements
* PdfDictionary.PdfStream
* PdfDictionary.PdfStream.Keys
* ... .Keys
* PdfName.PdfXNameComparer
* PdfPage.InheritablePageKeys
* PdfAcroField.PdfAcroFieldCollection
* PdfCrossReferenceStream.CrossReferenceStreamEntry
* PdfFormXObjectTable.Selector
* PdfImageTable.ImageSelector
* PdfCryptFilter.CryptFilterMethod

**MigraDoc.ObjectModel**

* Capabilities.FeatureNotAvailableAction
* Border.BorderValues
* ... .XxxValues

**MigraDoc.Rendering**
* DocumentRenderer.PrepareDocumentProgressEventArgs
