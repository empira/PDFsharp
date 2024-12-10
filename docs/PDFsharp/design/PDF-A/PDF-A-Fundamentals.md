# PDF/A fundamentals

PDFsharp should optionally produce PDF/A-conforming documents.

## PDF/A, structure tree, and PDF/UA

Ob der Structure Tree in PDF/A benötigt wird, ist von der gewählten Konformitätsstufe abhängig:
* Stufe A (Accessible) benötigt den Structure Tree erwartungsgemäß.
* Stufe B (Basic) kommt ohne Structure Tree aus.
* Stufe U (Unicode) kommt wahrscheinlich auch ohne Structure Tree aus, habe ich bisher aber nicht überprüft.

Die Standards und Konformitätsstufen sind unter [PDFA-kompakt-20.pdf](https://pdfa.org/wp-content/uploads/2013/05/PDFA-kompakt-20.pdf) auf Seite 8 gut erklärt.

In Word kann man unabhängig von PDF/A auswählen, ob das Dokument barrierefrei sein soll:
Bei der obigen Auswahl erzeugt Word ein Dokument, in dem kein StructTreeRoot zu finden ist. In den Metadaten steht <pdfaid:part>3</pdfaid:part><pdfaid:conformance>B</pdfaid:conformance> und veraPDF validiert es entsprechend mit PDF/A-3B.
Mit zusätzlichem Häkchen bei der Barrierefreiheit erzeugt Word ein Dokument mit StructTreeRoot. In den Metadaten steht <pdfaid:part>3</pdfaid:part><pdfaid:conformance>A</pdfaid:conformance> und veraPDF validiert es entsprechend mit PDF/A-3A.

## Proposal

Mein Vorschlag wäre, dass wir uns hier an Word orientieren und ebenfalls die Varianten PDF/A-3B und PDF/A-3A anbieten. Bei Auswahl von PDF/A-3A würde dann der UAManager genutzt und das PDF auch gleich PDF/UA-konform, bei PDF/A-3B nicht.

### PDF/A-3B

Basic level.

Shall we support 

### PDF/A-3A

### PDF/A-4*

Published in 2020 based on PDF 2.0.

## MigraDoc

MigraDoc shall use PDFsharp for all PDF/A requirements.
See [MigraDoc PDF/A fundamentals](../../../MigraDoc/design/PDF-A/PDF-A-Fundamentals.md).

## Links
[PDF/A Wikipedia](https://en.wikipedia.org/wiki/PDF/A) · 
[veraPDF](https://verapdf.org/) · 
[pdfa.org](https://pdfa.org/archival-pdf/) · 
[PDFA-kompakt-20.pdf](https://pdfa.org/wp-content/uploads/2013/05/PDFA-kompakt-20.pdf) · 
[PDFA-forever_1b.pdf](https://pdfa.org/wp-content/uploads/2011/08/PDFA-forever_1b.pdf) · 
[PDF/A-3 Compliance Issues](https://www.soliddocuments.com/iso-19005-3-compliance.htm) · 
[PDF/A-2 Compliance Issues](https://www.soliddocuments.com/iso-19005-2-compliance.htm) · 
[PDF/A-1 Compliance Issues](https://www.soliddocuments.com/iso-19005-1-compliance.htm)
