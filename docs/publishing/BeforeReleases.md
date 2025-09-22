# Before releases

Before you follow the instructions from the Azure DevOps wiki how to publish PDFsharp,
complete the following tasks.

## General todos

### Update the assembly build version

Set the assembly build version in file `gitversion.yml` to the number of days from
January 1st 2005 to the release day. **This is essential for users that use intaller tools.**

```yml
assembly-file-versioning-format: '{Major}.{Minor}.{Patch}.{env:PDFSHARP_BUILD_VERSION ?? 9999}'
```

### Update and sync used NuGet packages

Update version numbers of NuGet packages used in PDFsharp code (like `MicroSoft.Extensions.Logging`) and
build tools (xunit, gitversion, …).
Do this with `"Manage NuGet Packages for Solution…"` to see what’s new and update the version numbers
manually in `Directory.Packages.props`.

Sync `Directory.Packages.props` with all other solutions, like `PDFsharp.Samples` etc.

### Check specific files

* Search for **`CHECK_BEFORE_RELEASE`** and verify the code at these places.
* Check `Directory.Build.targets`
  * `USE_LONG_SIZE` must be defined in a release or preview version.
  * `TEST_CODE_xxx` must be 'undefined' by the suffix `_xxx`.

### Run all tests

* Run all tests in both RELEASE and DEBUG configurations.  
* Run all tests, including all skipped tests, under Windows and WSL2 with `run-tests` (see below).
* TODO: Run all tests in a Linux Docker Container

#### Run run-test script

Execute `run-test.ps1` with the following parameters:

1. -Config \<String\>: Run the script once with "Debug" and once with "Release".
2. -Net6 \<Boolean\>: Set it to $false to run tests for Net8.
3. -SkipBuild \<Boolean\>: Set it to $false to build the solution first.
4. -RunAllTests \<Boolean\>: Set it to $true to run even slow tests.

Run `help .\dev\run-tests.ps1` for more information.

So this is the call for the DEBUG build using .NET 8:

```PWSH
.\dev\run-tests.ps1 Debug $false $false $true
```

And this is the call for the RELEASE build using .NET 8:

```PWSH
.\dev\run-tests.ps1 Release $false $false $true
```

## Update files and configurations

### Update .md files

* Check and update [README.md](../README.md)

### Update NuGet configuration

* Check/update BoilerplateText.md file - transfer changes to all NuGet projects
* Check/update referenced projects in NuGet .csproj files
* Check/update referenced NuGet packages in .nuspec files
* Check/update text files
* Check packages with NuGet Package Explorer

## Test other repositories

Rebuild both DEBUG and RELEASE and update local NuGet packages.

### GBE test

Use ComparePdf (part of PDFsharp.Lab) with GBE to compare the "!!TestResult.pdf" of the last release with the current one:

```PWSH
.\comparepdfs -f "{PathToFolderOfOldFile}\!!TestResult.pdf" "{PathToFolderOfNewFile}\!!TestResult.pdf" -t "{TargetFolder}" --unique-target -n "{ReportName}" -o
```

The folder with the report will be created in `{TargetFolder}`.
Due to `--unique-target`, the current date and time will be appended to the target folder and the PDF filename `{ReportName}`.
The report will be opened after creation, because `-o` is set.

Run `.\comparepdfs --help` for more information.

### PDFsharp IssueSubmissionTemplate

* Build solution
* Run the applications

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

With Qbx.LayouterComparer in AkutQBX.sln you can automatically generate and compare a desired set of the example files with a desired set of layouters with this and a referenced QBX version. 
Usually you will run Qbx.LayouterComparer in the updated QBX repository and reference a repository clone with the last QBX commit before the update to the new PDFsharp version as head. 
Simply adjust the folders in the Program.cs and comment out not desired files and layouters.
