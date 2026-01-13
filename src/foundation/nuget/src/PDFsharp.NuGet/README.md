# PDFsharp Core build

PDFsharp is the Open Source library for creating and modifying PDF documents using .NET. It has an easy-to-use API that allows developers to generate or modify PDF files programmatically. PDFsharp can be used for various applications, including creating reports, invoices, and other types of documents.

This package does not depend on Windows and can be used on any .NET compatible platform including Linux and macOS.

See [docs.pdfsharp.net](https://docs.pdfsharp.net) for details.

See [https://www.pdfsharp.com](https://www.pdfsharp.com) for professional support offers, premium technical advice, and contract work options.
Choose a support plan that suits your needs. We offer a variety of options, from small projects to large teams, with flexible response times.
Our team provides PDFsharp expert assistance, including implementation, optimization, and tailored solutions. 
# PdfSharpWithHebrewSupport

PdfSharpWithHebrewSupport is a small, backwards-compatible customization of PDFsharp that adds a simple Hebrew‑script (RTL) preprocessing step before text is drawn or measured. It reorders RTL runs for Hebrew-script characters so Hebrew/Yiddish text renders in natural visual order without full shaping engines.

Version: 6.2.3-hebrew1  
Targets: `net8.0`, `net9.0`, `net10.0`, `netstandard2.0`  
License: MIT (based on PDFsharp)

## Features
- Preprocesses text passed to PDFsharp to reverse RTL runs for characters in:
  - Hebrew block (U+0590..U+05FF)
  - Hebrew presentation forms (U+FB1D..U+FB4F)
- Minimal change: uses the existing PDFsharp render pipeline and keeps API compatibility.
- Includes unit tests that verify mixed LTR/RTL run reordering.

## Why use this
PDFsharp does not include a full Unicode BiDi + shaping engine. For Hebrew and other Hebrew‑script languages, a simple run reordering is often enough. This package provides that preprocessing without adding heavy dependencies.

## Install
dotnet CLI:
```
dotnet add package PdfSharpWithHebrewSupport --version 6.2.3-hebrew1
```

PackageReference:

```xml
<PackageReference Include="PdfSharpWithHebrewSupport" Version="6.2.3-hebrew1" />
```

## Usage
Install the package and use PDFsharp as usual. The package hooks into the render pipeline and reorders RTL runs before text is measured or drawn.

Example — inspect preprocessed text:
```csharp
var renderEvents = new PdfSharp.Events.RenderEvents();
renderEvents.PrepareTextEvent += (s, e) =>
{
    // e.Text has had Hebrew-script RTL runs reordered already
    Console.WriteLine(e.Text);
};
```

If you need custom behavior, you can still subscribe to `PrepareTextEvent` and modify `e.Text`.

## Limitations
- This is a heuristic limited to Hebrew-script characters. It does NOT:
  - Implement the full Unicode BiDi algorithm for every edge case.
  - Provide contextual glyph shaping (Arabic, Persian, Urdu, Syriac, N’Ko, Adlam, etc.)
- For Arabic-family or other joining scripts you must use a shaping engine (e.g., HarfBuzz) and feed shaped glyphs into the renderer.

## Changelog (high level)
- 6.2.3-hebrew1: Added Hebrew-script RTL preprocessing and unit tests to reorder RTL runs prior to rendering.

## Source & Attribution
This package is built from PDFsharp sources (original authors: empira Software GmbH). This fork/custom build is published by the package author listed on nuget.org.

Original project: https://github.com/empira/PDFsharp  
This fork: https://github.com/ronenfe/PDFsharp

## Support
Open issues on the package repository for bug reports or requests (RTL ranges, tests, or shaping integration guidance).
