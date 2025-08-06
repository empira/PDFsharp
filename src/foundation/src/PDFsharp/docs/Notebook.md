# Notebook


## Changes in 6.2


## Changes in 6.1

* PDFsharp now not depends on Debug and Console logger anymore
* Support for .NET Standard and .NET Framework 4.72
* Logging factory can be set.

## New C# features

PDFsharp uses should be compatible with .NET Framework 4.7x and .NET Standard 2.0.
Therefore only C# features are used that are handled by the compiler, like primary constructors, newer pattern matching,
switch expressions, etc.
However, there are some features that depend on the runtime.
PDFsharp assemblies need some additional code to work with frameworks other aht .NET 6 or higher.
PDFsharp currently uses the following:

* **init-only setter**  
  For a reasion I do not understand the compiler requires the following declaration
  ```C#
    #if !NET6_0_OR_GREATER
    namespace System.Runtime.CompilerServices
    {
        internal static class IsExternalInit { }
    }
    #endif
  ```
  See [manuelroemer/IsExternalInit](https://github.com/manuelroemer/IsExternalInit).
  Why a NuGet package with an empty class?

* **Indices and ranges**  
  The new range and index from end operator make C# code more readable.
  To use it we add the **System.Range** and **System.Index** to the PDFsharp assembly.
  The classes are declared as internal to prevent users of the PDFsharp NuGet packages to 
  accidentally use them.
  
