# MigraDoc 6.2.0 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **MigraDoc** `History.md`.

## What’s new in version 6.2

### Breaking changes

**Corrected bug with page orientation**  
See *Issues* below.

### Features

**MigraDoc Preview user control restored for GDI build**  
The MigraDoc Preview user control is again available in the GDI+ build.
MigraDoc Preview samples for GDI and WPF have been added to the samples repository.

**Color.Parse no longer causes exceptions when invoked with a number value**  
If Color.Parse is invoked with a number value, as in `var color = Color.Parse("#f00");`,
it no longer throws a handled exception internally.

**MigraDoc: No more hard-coded fonts names**  
It is now possible to control font names used by MigraDoc for error messages and characters for bullet lists.
PDFsharp 6.2.0 Preview 1 and earlier always used `Courier New` for error messages like Image not found and used hard-coded fonts for bullets.  
[Read more](http://docs.pdfsharp.net/link/migradoc-font-resolving-6.2.html)

### Issues

**MigraDoc: Infinite loop in renderer**  
We fixed a bug that led to an infinite loop in PDF Renderer.

**Corrected bug with page orientation**  
When opening and modifying existing PDF pages, there sometimes were issues due to page rotation and page orientation.
Behavior has changed for page format Ledger and apps supporting Ledger format must be tested and may have to be updated.
Behavior has also changed when you set a new page format or orientation after setting page height and/or page width, including modifying a *Clone()* of the *DefaultPageSetup*. The new method *ResetPageSize* should be called before setting *PageFormat* and *Orientation* for a "cloned" PageSetup.
These are breaking changes.
