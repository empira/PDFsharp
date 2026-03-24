# 6.3.0 change log

This text is copied to `docs.pdfsharp.net`.

## What’s new in version 6.3

**This file is not yet up-to-date.**

<!--### PDFsharp 6.3.0 (final release)


-->
### Breaking changes

* From 6.2 to 6.3 needs a recompile.

### General features

* GitVersion build task replaced for better build time.
* Packages for .NET 9.
* 6.4: Implementation for .NET 9 that does not use obsolete APIs.
* DownloadAssets is invoked automatically during building. Requires Internet access.
* 6.4: `PDFsharpBuildFewerFrameworks` build property to speed up development by compiling against .NET 8 only.
* 6.4: `PDFsharpRunSlowTests` build property to include slow unit tests.

**.NET 9** replaces **.NET 6**  
We added .NET 9 (`net9.0`) to `<TargetFrameworks>`.  
We removed .NET 6 because it is no longer supported by Microsoft. Projects using .NET 6 can still use the current version.


#### General issues

* TODO
