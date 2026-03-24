// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 Ready

namespace PdfSharp.Pdf.Actions
{
    /// <summary>
    /// Specifies the action types.
    /// </summary>
    public enum PdfNamedActionTypes
    {
        // Reference 2.0: 12.6.4 Action types / Page 510
        // Reference 2.0: Table 201 — Action types / Page 510

        /// <summary>
        /// Go to a destination in the current document.
        /// </summary>
        GoTo = 1,

        /// <summary>
        /// ("Go-to remote") Go to a destination in another document.
        /// </summary>
        GoToR,

        /// <summary>
        /// ("Go-to embedded"; PDF 1.6) Go to a destination in an embedded file.
        /// </summary>
        GoToE,

        /// <summary>
        /// ("Go-to document part"; PDF 2.0) Go to a specified DPart in the current document.
        /// </summary>
        GoToDp,

        /// <summary>
        /// Launch an application, usually to open a file.
        /// </summary>
        Launch,

        /// <summary>
        /// Begin reading an article thread.
        /// </summary>
        Thread,

        /// <summary>
        /// Resolve a uniform resource identifier.
        /// </summary>
        URI,

        /// <summary>
        /// (PDF 1.2; deprecated in PDF 2.0) Play a sound.
        /// </summary>
        Sound,

        /// <summary>
        /// (PDF 1.2; deprecated in PDF 2.0) Play a movie.
        /// </summary>
        Movie,

        /// <summary>
        /// (PDF 1.2) Set an annotation’s Hidden flag.
        /// </summary>
        Hide,

        /// <summary>
        /// (PDF 1.2) Execute a predefined action.
        /// </summary>
        Named,

        /// <summary>
        /// (PDF 1.2) Send data to a uniform resource locator.
        /// </summary>
        SubmitForm,

        /// <summary>
        /// (PDF 1.2) Set fields to their default values.
        /// </summary>
        ResetForm,

        /// <summary>
        /// (PDF 1.2) Import field values from a file.
        /// </summary>
        ImportData,

        /// <summary>
        /// (PDF 1.5) Set the states of optional content groups.
        /// </summary>
        SetOCGState,

        /// <summary>
        /// (PDF 1.5) Controls the playing of multimedia content.
        /// </summary>
        Rendition,

        /// <summary>
        /// (PDF 1.5) Updates the display of a document, using a transition dictionary.
        /// </summary>
        Trans,

        /// <summary>
        /// (PDF 1.6) Set the current view of a 3D annotation.
        /// </summary>
        GoTo3DView,

        /// <summary>
        /// (PDF 1.3) Execute an ECMAScript script.
        /// </summary>
        JavaScript,

        /// <summary>
        /// (PDF 2.0; RichMedia annotation only) Specifies a command to be sent to the annotation’s handler.
        /// </summary>
        RichMediaExecute,
    }
}