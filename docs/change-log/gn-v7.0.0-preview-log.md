# 7.0.0 change log

This text is copied to `docs.pdfsharp.net`.

## What’s new in version 7.0

**This file is not yet up-to-date.**

<!--### PDFsharp 7.0.0 (final release)


-->
### Breaking changes

* From 6.x to 7.0 needs a recompile.

### General features

* GitVersion build task replaced for better build time.
* Packages for .NET 9 and .NET 10.
* 6.4: Implementation for .NET 9 that does not use obsolete APIs.
* DownloadAssets is invoked automatically during building. Requires Internet access.
* 6.4: `PDFsharpBuildFewerFrameworks` build property to speed up development by compiling against .NET 10 only.
* 6.4: `PDFsharpRunSlowTests` build property to include slow unit tests.
* Code review: many classes were refactored to achieve cleaner code with improved maintainability.

**.NET 9** replaces **.NET 6**  
We added .NET 9 (`net9.0`) and .NET 10 to `<TargetFrameworks>`.  
We removed .NET 6 because it is no longer supported by Microsoft. Projects using .NET 6 can still use the current version.


#### General issues

* TODO
