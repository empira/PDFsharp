# PDFsharp.Cryptography

This is the PDFsharp.Cryptography project.
It contains classes for signing a PDF document with a digital signature using the
PDFsharp default signer.
This additional assembly prevents PDFsharp.dll from depending on `System.Security.Cryptography.Pkcs`
in case a customer project does not need digital signatures.
