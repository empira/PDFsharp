﻿# PDFsharp 3.1

## PDFsharp PURE

* 3.1 and 5.0
* 

## PDFsharp GDI

* Features from GDI+
* New project format
* Windows only
* GDI+ "Global Lock"

## PDFsharp WPF

* Features from WPF
* Windows only ✓

* WinAnsiEncoding 1252 does exist.
  ⇒ Must call "var ansiEncoding = CodePagesEncodingProvider.Instance.GetEncoding(1252)!;" with .NET Core.
* ~~Process.Sta/rt needs ProcessStartInfo to start PDF file.~~
  * Use PdfFileUtility.StartPdfViewer(...);

* .NET Core 3.1 / .NET Standard 2.1
* Run without dependencies to GDI+ or WPF unter Windows
* Run on Linux
* Run on Mac
* Windows version for WinForms
* Windows version for WPF

Drop
* Drop code from Silverlight, Windows Phone, WinRT, and other UWP versions.

## From 4.7.2 to 4.6.2

Replace **net472** with **net462** in all `*.csproj` and in all `*.nuspec` files.

Add (or change) `NET462` in the following files:
`CompilerServices.cs` line 56
`FlateDecode.cs` line 34

