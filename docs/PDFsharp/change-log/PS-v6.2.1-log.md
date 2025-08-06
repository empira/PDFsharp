# PDFsharp 6.2.1 change log

A copy of the text below this line is added to `docs.pdfsharp.net` **PDFsharp** `History.md`.

## What’s new in version 6.2

### Breaking changes

*(none)*

### Features

**Formatting options for PDF files**  
Using the *Options.Layout* member of the **PdfDocument** class, you can set how PDF files will be generated.  
Use **PdfWriterLayout.Verbose** to get readable output. This is the default for Debug builds of PDFsharp.  
Use **PdfWriterLayout.Compact** to get small output files. This is the default for Release builds of PDFsharp and should be used for production code.

### Issues

**Encrypted hyperlinks are now working**  
We fixed a bug that caused PDF file encryption to throw an exception if the PDF file contained certain types of hyperlinks.

**Character escaping for PDF names**  
**PdfName** now correctly includes the curly braces ('{' and '}') in the list of characters to be escaped.

**Fixed incorrect number formatting**  
This bug could lead to incorrectly formatted Outline entries.

**Fixed exception that occurred for files of size 1030 bytes**  
PDFsharp can now open files with a size of 1030 bytes.

**Arrays in PDF files are now written without line feeds**  
When using **PdfWriterLayout.Compact**, arrays in PDF files will be written without line feeds.  
The files with line feeds are technically correct, but we were informed that some printer software cannot handle those files correctly.

**Keep both /A and /Dest entries for outlines in PDF**  
According to the PDF Reference 1.7, an /A entry is not permitted if a /Dest entry is present.  
PDFsharp 6.2.1 no longer deletes the /A entry if both are present.
