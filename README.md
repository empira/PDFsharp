# PDFsharp & MigraDoc 6

Version **6.2.0 Preview 1**  
Published **2024-08-12**

This is a preview version of the **PDFsharp** project, the main project of PDFsharp & MigraDoc 6 with updates for C# 12 and .NET 6.

PDFsharp: Copyright (c) 2005-2024 empira Software GmbH, Troisdorf (Cologne Area), Germany
MigraDoc: Copyright (c) 2001-2024 empira Software GmbH, Troisdorf (Cologne Area), Germany
Published Open Source under the [MIT License](https://docs.pdfsharp.net/LICENSE.html)

For more information see [docs.pdfsharp.net](https://docs.pdfsharp.net/)

## Read this FIRST

Project documentation can be found on our DOCS site: <https://docs.pdfsharp.net>.

Note: PowerShell 7 is required to execute the PowerShell scripts that come with PDFsharp.

### Download assets first

Assets like bitmaps, fonts, or PDF files are not part of the repository anymore.
You must download them before compiling the solution for the first time.
Use `download-assets.ps1` in the `dev` folder to create `assets` folder required for some unit tests and needed by some projects.

Execute 
```ps
.\dev\download-assets.ps1
```

### Build the solution

`dotnet build` should build the solution without any warnings or errors.

* You need the latest .NET SDK version installed
* Please note that you need a git repository with at least one commit in order to build the PDFsharp solution.  
  Without a git repo with at least one commit, you will get an error message from `GitVersion.MsBuild` while building the solution.
  You can set a tag to define a valid version, e.g.: `git tag v6.2.0` to make it build with a specific version number. Without tag, version 0.1.0 will be used.

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

## Libraries used by PDFsharp

The Core build of PDFsharp uses BigGustave to read PNG images. BigGustave was released into the public domain and does not restrict the MIT license used by PDFsharp.  
Link to project repository: https://github.com/EliotJones/BigGustave
