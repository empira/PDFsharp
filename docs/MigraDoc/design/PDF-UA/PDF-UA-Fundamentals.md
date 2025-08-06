# PDF/UA fundamentals

MigraDoc should optionally produce PDF/UA-conforming PDF documents.

## Based on PDFsharp

Configure Renderer according to PDFsharp.

## DOM

DOM needs some extensions

* Language tag for  
  **Document**, **Paragraph**, **FormattedText**  
  but not for  
  **Section**, **Table**, etc. They inherit from **Document**.

* Alternate text for images

## Render process

Because DOM well-defines the structure tree, it should be an easy top-down implementation.
