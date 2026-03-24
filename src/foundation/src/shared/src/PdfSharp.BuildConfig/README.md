# PDFsharp.BuildConfig

TODO: not yet completed

This is the PDFsharp.BuildConfig project.
It is a dummy project only used to create GitVersion stuff with the `CreateBuildConfiguration.ps1` script.

The purpose of this project is to create version information directly from git.
Starting with PDFsharp 6.0 we used the tool GitVersion to create the information.
After version 6.2.x we will not use GitVersion anymore.

GitVersion used information from the current branch and the last tag to calculate all values for [semantic versioning](https://semver.org/).
These information is used in the build process to add version information to all assemblies and NuGet packages.
This is great for automatic build processes.
Unfortunately, retrieving the same information for each project in the solution and each framework of every project more than doubles the build time.
Therefore, we decided to get information only once directly with git commands and store the result in two files.
A props file for MS Build and a C# file to make the information available at runtime.

This sounds easy but there are several unexpected complication we had to solve.
The generated props file must not be controlled by git because otherwise we had changes in the repository after each commit.
We use a counter in the prerelease tag of the semver version.
After each commit the number is incremented and the generated file changes.

## More details

What this project does is to create two files when it is compiled: `SemVersion.props` and `SemVersionInformation-generated.cs`.

`SemVersion.props` is located in the `./src directory` and contains the following MS Build variables

* Version
* FileVersion
* InformationalVersion
* Major
* Minor
* Patch
* PreReleaseLabel
* SemVer
* BranchName
* CommitDate

`SemVersionInformation-generated.cs` is located in the `internals` folder of the `PdfSharp.Shared` project.
It contains constants used in the practical class **SemVersionInformation**.

Because the files are not in the repository they do not exist after a git clone, but are required to build the solution.
To resolve this egg and chicken problem, `SemVersion.props` is only included from `Directory.Build.props` if it exists.
The version information is therefore only available after the second build.

`SemVersionInformation-generated.cs` is more complicated.
`PdfSharp.Shared` cannot compile without this file.
Even if `PdfSharp.BuildConfig` is complied before `PdfSharp.Shared` it does not work.
It seems that MS Build or the C# compiler takes a snapshot of all file of the solution when parallel compilation starts.
Our solution is to include the dummy C# file `SemVersionInformation-substitute.cs` in the `PdfSharp.Shared` project if `SemVersionInformation-generated.cs` does not exist.

## Override GitVersion values

In `SemVersion.json` you can set `UseThisInformation` to 1 to override the Gitversion information with own values. See `CreateBuildConfiguration.ps1` for more information.