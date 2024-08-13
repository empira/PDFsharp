# Before released

Before you follow the instructions from the Azure DevOps wiki how to publish PDFsharp
complete the following tasks.

## General todos

### Update the assembly build version

Set the assembly build version in file in `gitversion.yml` to the number of days from 
January 1st 2005 to the release day. **This is essential for users that use intaller tools.**
```
assembly-file-versioning-format: '{Major}.{Minor}.{Patch}.{env:PDFSHARP_BUILD_VERSION ?? 9999}'
```

### Check specific files

* Search for **`CHECK_BEFORE_RELEASE`** and verify the code at this places.
* Check `Directory.Build.targets`
    * `USE_LONG_SIZE` must be defined.
    * `TEST_CODE_xxx` must be 'undefined' by the suffix `_xxx`.

### Run all test

* Run all tests in both RELEASE and DEBUG configuration.  
  UnitTest with `RunTests` including all skipped tests.
* Run all text under WSL2

## Update files and configurations

### Update .md files

* Check and update [README.md](../README.md)

### Update NuGet configuration

* ??? 
* Check packages with NuGet Package Explorer

## Test other repositories

Rebuild both DEBUG and RELEASE and update local NuGet packages.

### GBE test

* Use ComparePdf with GBE

### PDFsharp.Samples

* Build solution
* Run the script that executes all samples

### PDFsharp.Tests

* Build solution
* Run the tests

### PDFsharp.Lab

* Build solution
* Run the script that executes all samples

### PDFsharp.MAUI

* Build solution
* Run the script that executes all samples

### PDFsharp.Blazor

* Build solution
* Run the script that executes all samples

### Test with QBX

* Test with QBX
