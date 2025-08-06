# PDFsharp.Shared

This is the PDFsharp.Shared project.
It contains code that is used in all PDFsharp foundation assemblies, but should not be publicly visible.
The visibility of all code in this assembly is internal and shared by the InternalsVisibleTo attribute.

This approach has the following advantages.

* Functionality can be shared between PDFsharp assemblies without making it publicly visible.
* It is easier to use code required by the C# compiler to use language features not available in
  .NET Framework or .NET Stndard
* We have no public internal stuff anymore. E.g. the class **NRT** was introduced to transform the
  code to nullable reference types. This class is now internal and not visible to PDFsharp users anymore.

The approach has some technical consequences.

* All PDFsharp assemblies has a strong name because some users may need it for the global assembly cache
  still used with .NET Framework. Therefore, `PDFsharp.Shared` also has a strong name. And because
  a strong named assembly can only grant friendly access to assemblies that also are strong named,
  also test and other assemblies must have a strong name.
* We use GitVersion as our tool for creating version information. This tool generates the internal class
  **GitVersionInformation** in the root namespace for every assembly. Because all internals of 
  `PDFsharp.Shared` are visible to all PDFsharp assemblies, **GitVersionInformation** from `PDFsharp.Shared`
  clashes with the own version of every assembly with friendly access. This is not a problem, because firstly
  all field of all copies of **GitVersionInformation** have the same value anyway and secondly the compiler
  uses the class of the current assembly, not the imported one. We only have to disable the warning when
  we access a field of **GitVersionInformation**.

