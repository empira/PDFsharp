# Issues

Design issues in PDFsharp to ponder on.

## TLS for imported documents

Importing a PDF document depends on TLS. Thread affinity may be problematic since we
have an execution context.
It may be better to save the weak document handles in the Globals and protect access with
a semaphore. But another global lock may lead to undiscoverable dead-lock situations.
