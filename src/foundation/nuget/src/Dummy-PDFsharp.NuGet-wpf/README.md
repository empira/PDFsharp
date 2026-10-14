# Dummy PDFsharp WPF

Let MSBUILD create the NuGet package and see how it looks in detail.
Use `nuspec` file inside the package to find the correct entries for target framework, folder names etc.

Set `GeneratePackageOnBuild` to **true** in project file.
```XML
<GeneratePackageOnBuild>false</GeneratePackageOnBuild>
```

# Developer notes

According to the docs it should be possible to create NuGet packages without nuspec files, because
all stuff can be set in the csproj file. We still use nuspec because I cannot fix the following issues:

* Set the package icon in cssproj so that it is displayed.
* We want a single package for all (e.g.) PDFsharp assemblies.
  Currently I don’t know how to manage that without a nuspec file.
