# README for logging changes

## Rules for log files and docs.pdfsharp.net history content

* There are **3 markdown files** to log the changes for the currently developed version:
  * `gn-vX.X.X-log.md` for general changes
  * `PS-vX.X.X-log.md` for PDFsharp specific changes
  * `MD-vX.X.X-log.md` for MigraDoc specific changes
* All versions shall be sorted descending by date.
* The information about a **version** shall begin with the **heading** `What’s new in version X.x`.
* The information about a **preview version** shall begin with the **subheading** `PDFsharp X.X.X Preview X`.
* A horizontal line shall be included after each version and preview version.

### Preview versions

* Only The **log for the general changes** shall list the **preview versions separately**
* The changes of a preview version shall refer to the previous preview version, if existing - otherwise to previous version.
* The **PDFsharp and MigraDoc change logs** shall **not** list the preview versions separately.  
All changes of a version (including its preview versions) shall be summarized thematically. 
References to the preview version, a change was published with, shall be added to this text (e. g. `(since 6.2.0 PV3`).
* **If a "final" version is going to be released**, its **preview versions shall no longer be listed separately** in the general change log.  
The changes shall be summarized like they are in the PDFsharp and MigraDoc change logs.

### Version content

#### General change log headings

* **Breaking changes**  
  This shall **always** be included. 
  Explicitely write `There are no breaking changes.` if there are none.
* **General features** (optional)
* **General issues** (optional)
* Furthermore, there are headings for PDFsharp and MigraDoc specific changes.  
  The text here shall not go in detail and refer to the PDFsharp/MigraDoc specific history, where the change is explained.
  * **PDFsharp features** (optional)
  * **PDFsharp issues** (optional)
  * **MigraDoc features** (optional)
  * **MigraDoc issues** (optional)

#### PDfsharp/MigraDoc change log headings

* **Features** (optional)
* **Issues** (optional)

### docs.pdfsharp.net

* **If a version (or preview version) is released**, the content of the files shall be added to the **3 History.md files** in docs.pdfsharp.net.  
  Due to the summarization of the preview versions in the change log files, **only the general History.md file shall list preview versions separately, and only for a version, which is currently developed**.
* A horizontal line shall be included after each version.

## Rules for version readme file

A version readme file with the name `README-vX.X` shall be published on **docs.pdfsharp.net**.

### Headings

* **What’s new**  
  Shall **refer to the general History.md** published on docs.pdfsharp.net.
  There’s **no need to to refer to the PDFsharp/MigraDoc specific Histroy.md**, as the general refers to them at its top.
* **Prerequisites**
* **Breaking changes**  
  This shall **always** be included. 
  Explicitely write `There are no breaking changes.` if there are none.  
  Should be a copy, summary or excerpt of the general History.md breaking changes.  
  At last, the **general History.md shall be referenced**.
* Perhaps further headings for additional information.