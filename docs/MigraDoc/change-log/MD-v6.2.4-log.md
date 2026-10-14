# MigraDoc 6.2.4 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **MigraDoc** `History.md`.

## What’s new in version 6.2.4

### Breaking changes

*(none)*

### Features

*(none)*

### Issues

**MigraDoc shows filename in PDF if image cannot be found**  
If an image cannot be found, MigraDoc renders a placeholder for that image in PDF.
The placeholder now shows the path of the image that cannot be found.

**Fixed MigraDoc memory leak after rendering PDF**  
With MigraDoc 6.2.3, a reference to the MigraDoc Document was kept after rendering PDF.
Only a reference to the last document was kept.
So, when rendering 10 documents, 9 documents were released.
With MigraDoc 6.2.4, the MigraDoc Document is released after rendering.

The bug fixes of PDFsharp are also useful when generating PDF files from MigraDoc.
