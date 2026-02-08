# Per-tier project lists for .NET 10 upgrade

This file lists projects assigned to each tier. The authoritative full list remains in `.github/upgrades/assessment.md`.

## Tier 1 (Leaf nodes)
- docs\docs-dummy.csproj
- src\tools\src\CopyAsLink\CopyAsLink.csproj
- src\tools\src\NRT-Tests\NRT-Tests.csproj
- src\foundation\src\shared\src\PdfSharp.WPFonts\PdfSharp.WPFonts.csproj
- src\foundation\src\shared\src\PdfSharp.Shared\PdfSharp.Shared.csproj
- src\foundation\nuget\src\Dummy-PDFsharp.NuGet-wpf\Dummy-PDFsharp.NuGet-wpf.csproj

## Tier 2 (Core libraries)
- src\foundation\src\shared\src\PdfSharp.System\PdfSharp.System.csproj
- src\foundation\src\shared\src\PdfSharp.Testing\PdfSharp.Testing.csproj
- src\foundation\src\shared\src\PdfSharp.Fonts\PdfSharp.Fonts.csproj
- src\foundation\src\PDFsharp\src\PdfSharp.Cryptography\PdfSharp.Cryptography.csproj
- src\foundation\src\shared\src\PdfSharp.Snippets\PdfSharp.Snippets.csproj

## Tier 3 (Feature libraries)
- src\foundation\src\PDFsharp\src\PdfSharp.Charting\PdfSharp.Charting.csproj
- src\foundation\src\MigraDoc\src\MigraDoc.Rendering\MigraDoc.Rendering.csproj
- src\foundation\src\MigraDoc\src\MigraDoc.DocumentObjectModel\MigraDoc.DocumentObjectModel.csproj
- src\foundation\src\PDFsharp\src\PdfSharp.BarCodes\PdfSharp.BarCodes.csproj

## Tier 4 (Applications / Samples / Tools / Tests)
- src\foundation\src\PDFsharp\src\PdfSharp\PdfSharp.csproj
- src\samples\src\PDFsharp\src\HelloWorld\HelloWorld,PDFsharp.csproj
- src\samples\src\MigraDoc\src\HelloWorld\HelloWorld,MigraDoc.csproj
- All test projects under src\foundation\src\**\tests\ (migradoc & pdfsharp tests)
- Tools: src\tools\src\PdfFileViewer\PdfFileViewer.csproj, src\tools\src\PdfSharp.TestHelper\PdfSharp.TestHelper.csproj

## Notes
- These lists are derived from `assessment.md` and the plan's tier definitions. During execution, if any project has cross-tier dependencies that prevent strict ordering, the task execution will treat those projects as part of the higher-risk tier or merge them into a single PR.
- The exact per-project change list for each tier will be generated during the execution actions for that tier.
