# Global Stuff

## List of global classes

Classes should be cleaned up...
* Create a new static class `partial class Globals` that contains ALL global suff.
* Make a singleton `Global` as the one and only instance of this class.
* Recreating `Globals` should reset PDFsharp in the same state as it would be immediately after it's assemblies were loaded.
* `Global` has a version number. All classes that are collected in a global cache, like the **XGraphicsObect** cache,
  have this generation number. This allows to dedect e.g. **XFont** objects that survived this 'global reset'.
* Ensure that there is only ONE global mutex semaphore. More than one mutex can cause deadlocks that can happen extreamly rare
  and it is practically impossible to find them.
* The goal is to remove all static data declarations that are not constant.

### LogHost
* in PdfSharp.System assembly.

### static class Diagnostics `xxx`

### DiagnosticsHelper - 'PdfSharp.Internal.DiagnosticsHelper' in 'PdfSharp'

### PdfDiagnostics

### Capabilities

### GlobalFontSettings

* ResetFontResolvers

### LogHost - `PdfSharp.Logging.LogHost` in `PdfSharp.System`