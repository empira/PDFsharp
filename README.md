# PDFsharp & MigraDoc 6.0

Version **6.0.0**  
Published **2023-11-07**

This is a stable version of the **PDFsharp** project, the main project of PDFsharp & MigraDoc 6.0 with updates for C# 10 and .NET 6.0.

PDFsharp: Copyright (c) 2005-2023 empira Software GmbH, Troisdorf (Cologne Area, Germany)  
MigraDoc: Copyright (c) 2001-2023 empira Software GmbH, Troisdorf (Cologne Area, Germany)  
Published Open Source under the [MIT License](https://docs.pdfsharp.net/LICENSE.html)

For more information see [docs.pdfsharp.net](https://docs.pdfsharp.net/)

## Read this FIRST

Project documentation can be found on our DOCS site: <https://docs.pdfsharp.net>.

Note: PowerShell 7 or higher is required to execute the PowerShell scripts that come with PDFsharp.

### Download assets first

Assets like bitmaps, fonts, or PDF files are not part of the repository anymore.
You must download them before compiling the solution for the first time.
Use `download-assets.ps1` in the `dev` folder to create `assets` folder required for some unit tests and needed by some projects.

Execute `.\dev\download-assets.ps1`

### Build the solution

`dotnet build` should build the solution without any warnings or errors.

* You need the latest .NET SDK version installed
* If you got an exception from `GitVersion.MsBuild` let us know.  
  You can set a tag to define a valid version, e.g.: `git tag v6.0.0` to make it work.

### Central package management

The solution uses central package management.
Version numbers for all referenced packages are stored in file `Directory.Packages.props` in the `src` folder.
When adding new packages, add the required version here.

## Authors

PDFsharp and MigraDoc was mainly written by the following software developers.
With support of a lot of community developers who found issues and fixed bugs.

### Original PDFsharp developers

Stefan Lange  
Niklas Schneider  
David Stephensen

### Original MigraDoc developers

Klaus Potzesny  
Niklas Schneider  
Stefan Lange

### Current PDFsharp and MigraDoc developers

Stefan Lange  
Thomas Hövel  
Martin Ossendorf  
Andreas Seifert
