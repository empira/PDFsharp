# NuGet package generation

This folder contains dummy C# projects for the generation of the PDFsharp and MigraDoc NuGet packages.

## Developer notes

* The description and release notes are single text files read by MSBUILD and put to the nuspec files.
  This is done because putting the text directly in XML is very cumbersome.
* The description and release notes should be worded such that it is not necessary to revise them on every new release.
* For inspecting NuGet packages the **NuGet Package Explorer** is a nice app.

## Descriptions

### PDFsharp

PDFsharp is the Open Source .NET library that easily creates and processes PDF documents on the fly from any .NET language.

Text from PDFsharp 1.51:
PDFsharp is the Open Source .NET library that easily creates and processes PDF documents on the fly from any .NET language. The same drawing routines can be used to create PDF documents, draw on the screen, or send output to any printer.

#### PDFsharp Core

This is the PDFsharp Core package.
This package does not rely on Windows components and can be used on any platform including Linux.
Image formats supported by this package include Windows BMP, PNG, and JPEG.
See Project Information for details.

#### PDFsharp GDI

This is the PDFsharp GDI package.
This package relies on Windows GDI+ components and can be used under Windows only.
This package supports image formats handled by GDI+ like Windows BMP, GIF, TIFF, PNG, and JPEG.
See Project Information for details.

#### PDFsharp WPF

This is the PDFsharp WPF package.
This package relies on Windows WPF components and can be used under Windows only.
This package supports image formats handled by WPF like Windows BMP, GIF, TIFF, PNG, and JPEG.
See Project Information for details.

### MigraDoc

MigraDoc - the Open Source .NET library that easily creates documents based on an object model with paragraphs, tables, styles, etc. and renders them into PDF or RTF.

Text from MigraDoc 1.51:
MigraDoc - the Open Source .NET library that easily creates documents based on an object model with paragraphs, tables, styles, etc. and renders them into PDF or RTF.

#### MigraDoc Core

This is the MigraDoc Core package.
This package does not rely on Windows components and can be used on any platform including Linux.
Image formats supported by this package include Windows BMP, PNG, and JPEG.
See Project Information for details.

#### MigraDoc GDI

This is the MigraDoc GDI package.
This package relies on Windows GDI+ components and can be used under Windows only.
This package supports image formats handled by GDI+ like Windows BMP, GIF, TIFF, PNG, and JPEG.
See Project Information for details.

#### MigraDoc WPF

This is the MigraDoc WPF package.
This package relies on Windows WPF components and can be used under Windows only.
This package supports image formats handled by WPF like Windows BMP, GIF, TIFF, PNG, and JPEG.
See Project Information for details.

